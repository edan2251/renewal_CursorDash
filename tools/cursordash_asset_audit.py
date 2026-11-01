"""Conservative, read-only Unity reference classification. Never moves or deletes assets.

Input is the AssetDatabase dependency export from the target Editor. A static lack
of references is deliberately insufficient to declare an asset unused.
"""
import argparse
import json
import re
from pathlib import Path


def audit(project: Path, inventory: Path, report: Path):
    data = json.loads(inventory.read_text(encoding="utf-8-sig"))
    assets = {entry["path"]: entry for entry in data["assets"]}
    roots = {}
    # Shader includes are compiler dependencies even when the serialized asset
    # dependency query does not expose them as GUID links.
    for path, entry in assets.items():
        if path.endswith((".shader", ".cginc", ".hlsl")):
            text = (project / path).read_text(encoding="utf-8-sig", errors="replace")
            for include in re.findall(r'#include\s+"([^"]+)"', text):
                candidate = (Path(path).parent / include).as_posix()
                if candidate in assets and candidate not in entry["dependencies"]:
                    entry["dependencies"].append(candidate)

    def root(path, reason):
        if path in assets:
            roots.setdefault(path, set()).add(reason)

    for scene in data["buildScenes"]:
        if scene["enabled"]:
            root(scene["path"], "활성 빌드 씬")
    # Retained scenes are intentionally included; not being the active scene is
    # not a reason to delete their prefab/image/animation dependencies.
    for path in assets:
        if path.endswith(".unity"):
            root(path, "보존 중인 씬 (현재 실행 사용 여부와 구분)")
        if path.endswith((".cs", ".asmdef", ".jslib")):
            root(path, "컴파일/Editor/검사/플랫폼 연결 (모든 클래스 실행을 뜻하지 않음)")
        if path.startswith("Assets/WebGLTemplates/"):
            root(path, "기존 WebGL 템플릿 보존")

    resource_keys = {}
    for path in assets:
        if "/Resources/" in path:
            key = path.split("/Resources/", 1)[1].rsplit(".", 1)[0]
            resource_keys.setdefault(key, []).append(path)
    dynamic_calls = []
    for source in (project / "Assets").rglob("*.cs"):
        text = source.read_text(encoding="utf-8-sig", errors="replace")
        relative = source.relative_to(project).as_posix()
        for match in re.finditer(r'Resources\.Load(All)?(?:<[^>]+>)?\(\s*"([^"]*)"\s*\)', text):
            all_items, key = match.groups()
            for candidate, paths in resource_keys.items():
                if candidate == key or (all_items and candidate.startswith(key.rstrip("/") + "/")):
                    for path in paths:
                        root(path, f"Resources 문자열 참조: {relative}")
        for match in re.finditer(r'"(Assets/[^"\r\n]+)"', text):
            root(match.group(1), f"코드/검사/Editor 경로 참조: {relative}")
        for number, line in enumerate(text.splitlines(), 1):
            if any(word in line for word in ("Resources.Load", "Addressables.", "LoadScene", "Shader.Find", "AnimationEvent")):
                dynamic_calls.append({"path": relative, "line": number, "code": line.strip()[:500]})

    reasons = {path: set(value) for path, value in roots.items()}
    queue = list(roots)
    while queue:
        parent = queue.pop()
        for path in assets[parent]["dependencies"]:
            if path not in assets:
                continue
            first = path not in reasons
            reasons.setdefault(path, set()).add("Unity 직렬화 의존: " + parent)
            if first:
                queue.append(path)

    entries = []
    for path, value in sorted(assets.items()):
        referenced = path in reasons
        entries.append({"path": path, "type": value["type"],
                        "category": "REFERENCED" if referenced else "UNCERTAIN",
                        "reasons": sorted(reasons.get(path, {"정적 참조 미발견. 동적 문자열/외부 도구/보존 목적을 배제하지 못해 유지"}))})
    output = report.with_suffix(".json")
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps({"project": str(project), "inventory": str(inventory),
                                 "provenUnused": [], "quarantined": [],
                                 "dynamicCalls": dynamic_calls, "assets": entries}, ensure_ascii=False, indent=2), encoding="utf-8")
    counts = {kind: sum(e["category"] == kind for e in entries) for kind in ("REFERENCED", "UNCERTAIN")}
    lines = ["# D-072 에셋 참조 분류", "", "Unity AssetDatabase의 실제 GUID 의존성과 활성 빌드 씬, 보존 씬, Resources/문자열/Editor/검사/WebGL 템플릿 경로를 교차 확인했다. 실행 시 모든 자산이 실제로 로드되었다는 뜻은 아니다.", "",
             f"- 참조/컴파일 연결 확인: {counts['REFERENCED']}개", f"- 불확실하여 유지: {counts['UNCERTAIN']}개", "- 충분히 검증된 미사용: 0개 / 격리·삭제: 0개", "",
             "참조가 없는 것만으로 미사용을 확정하지 않았다. 특히 _Recovery/옛 씬·Sprite fallback·PNG 비교·Resources·이전 테스트는 보존한다. 전체 경로와 근거는 같은 이름의 JSON에 있다.", "",
             "## 추가 확인 후보", "", "| 경로 | 확인 수준 / 유지 이유 |", "|---|---|"]
    for entry in entries:
        if entry["category"] == "UNCERTAIN":
            lines.append(f"| `{entry['path']}` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |")
    lines += ["", "## 검증 한계와 후속 격리 방법", "",
              "직렬화 GUID 의존성은 UnityEvent·Animation/Animator의 직렬화된 연결도 포함한다. 문자열로 계산되는 Resources/씬 이름, 외부 MCP 실행 경로, 조건부 컴파일의 실제 실행은 정적 검색만으로 모두 확정할 수 없다. JSON의 dynamicCalls를 함께 확인한다.", "",
              "후속 격리는 후보별 실제 접근/씬·테스트·빌드 비교를 통과한 뒤 원본과 .meta를 저장소 밖에 함께 보존하고 GUID/복구 경로를 기록해야 한다. 이번에는 확정 근거가 없어 실행하지 않았다. 사용자 백업/복구 씬/참고 자료는 삭제하지 않았다."]
    report.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return counts


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", type=Path, required=True)
    parser.add_argument("--inventory", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    arguments = parser.parse_args()
    print(json.dumps(audit(arguments.project, arguments.inventory, arguments.report)))
