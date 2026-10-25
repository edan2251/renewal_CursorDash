# CursorDash CP1~CP4 구현 결과 — 2026-10-24

CP1~CP4 구현·공식 회귀·로컬 WebGL 검증을 완료했다. 실기기 모바일 조작과 최종 미적 승인은 대기다. 다음 기능을 자동 구현하지 않는다.

2026-10-24 D-017 후속 배포: **[공개 플레이](https://edan2251.github.io/renewal_CursorDash/)**, 동일 buildId의 검증 빌드를 재사용해 gh-pages c42faf5로 배포했다. Pages success와 실제 공개 게임/훈련/Dev/수치·JSON·복사를 확인했다. [배포 이력/실제 화면/남은 모바일 검수](D:/renewal_CursorDash/docs/CursorDash/DEPLOYMENT_CP14_20261024.md)를 따른다. 아래 로컬 전용/공개 배포 없음 문장은 구현 완료 당시 이력이며 이번 배포에서 게임 소스/기본값을 바꾸지 않았다.

## A. 실제 구현

| CP | 결과 |
|---|---|
| CP1 | 충전 원/회전 테두리 유지. .15초 흡수·커서 변형 → .05초 반동 → .20초 본체 전진. 최종 커서만 .60배. 준비/변형/반동 접촉 제거, 전진 중 잔상 무시, 보이는 앞점의 연속 외곽 검사로 피해1회. 취소·Pause·풀 재사용 복원 |
| CP2 | 육각→정삼각형, 기존 빔이 접촉점에서 .06초에 본체로 돌아온 뒤 파괴·삼각 파편. 두 공격의 안정된 공통 넥서스 경계. HP 즉시/에너지 수위 보간·물결. 네 사건별 히트스톱·셰이크·실제 국소 화면 왜곡·파편·음량/음높이 |
| CP3 | 같은 Cyberpunk/NodeCreator/StageManager의 실제 성공조건 가이드·훈련. 생성/반복/정리/무적/HP/재시작, 일반 설정, 패널 입력 차단·독립 정지 상태, 버전 저장. 훈련/Dev 기록 제외·라운드 내 자격 복구 금지 |
| CP4 | 한국어 141개 allowlist의 Editor 및 모바일 대응 HTML 도구. 검색/그룹/즐겨찾기/변경만, 숫자·슬라이더·미세조절·색·곡선, 요청/실제/대기, 안전 재시험/다음 라운드. 이름 프리셋 저장/불러오기/복제/선택 삭제. 전체/변경 JSON 파일/텍스트·한국어 변경 보고서·정직한 복사 실패 대안 |

새 패키지·렌더 파이프라인·전역 개발 설정은 추가하지 않았다. Inspector 저장값과 런타임 allowlist는 같은 실행 경로를 사용하며 Play의 수치 실험이 원본 에셋을 자동 저장하지 않는다. 임의 리플렉션/실행 명령/파일 경로를 JSON으로 받지 않는다.

### 주요 변경 파일 (Unity Assets 기준)

| 파일/그룹 | 변경 이유 |
|---|---|
| Scripts/Gameplay/Object/Node/NormalNode.cs, Prefabs/Nodes/Cyberpunk/NormalNode.prefab | 확정 시간/배율·반동·생성별 스냅샷·앞점 판정 |
| Scripts/Gameplay/Object/Node/ArrowNode.cs, Prefabs/Nodes/Cyberpunk/ArrowNode.prefab | 삼각 메시/흡수·같은 빔 반환·도착 파괴·정확히 한 번 처리 |
| Scripts/Gameplay/StageManager/StageManager.cs, Scenes/Cyberpunk.unity | 공통 안정 경계·첫 레이아웃 대기·HP/정지/기록 경로 |
| CursorDashTuning.cs, CursorDashStorage.cs, CursorDashRuntimeUI.cs, CursorDashBoundaryView.cs | 파라미터/검증/저장/명령 allowlist·Editor/브라우저 연결·실제 경계 표시 |
| CursorDashSession.cs, CursorDashPractice.cs, Menu/StageSelectSceneManager/MainSceneController.cs | 정규/훈련/가이드 전환·실제 성공 조건·기록 분리 |
| HitFeedback.cs, ImpactFragments.cs, LocalImpactDistortion.cs, NexusEnergy.cs, CameraShake.cs | 사건별 효과·실제 화면 샘플링·별도 UI 카메라·누적 상한·수위 |
| Object/Character/PlayerCharacterController.cs, VirtualJoyStick.cs, PlayerCharacterAfterImage.cs | 실제 입력 편의/시각/판정 조정·패널 소유권·안전한 잔상 풀 재계산 |
| NodeCreator.cs, PatternReader.cs, ScoreManager.cs, Object/Object/HoldAchivement.cs | 기존 생성 구조 활용·다음 라운드 적용·정규 기록 분리 |
| Object/Animator/Rotator.cs, Services/GameManager/SoundManager.cs | 자기 회전 핸들·실제 사건별 오디오 조정 |
| Resources/CursorDash/LocalImpact.shader, NexusEnergy.shader, BuildIdentity.txt | Built-in 국소 샘플링·수위 마스크·미커밋 빌드 식별 |
| Plugins/WebGL/CursorDashBridge.jslib, WebGLTemplates/CursorDash/index.html, TemplateData/cursordash-tools.js/.css | 모바일 HTML 조작·v2 localStorage·파일/텍스트/복사·안전영역 |
| Editor/CursorDashLocalPreviewBuild.cs | 기존 빌드를 덮어쓰지 않는 전용 로컬 BuildPipeline 실행 |
| Tests/Editor/CursorDashSceneRegression.cs | 기존16개 유지·변경 계약 갱신·관련11개 추가/통합 |

신규 Unity 파일의 .meta를 함께 유지했다. 관련 문서 AGENTS/MASTER_SPEC/DECISIONS/STATUS/ENVIRONMENT/VALIDATION 및 TUNING_GUIDE/본 보고서/실제 내보내기 예시를 갱신했다. 전체 경로·사전 상태 대비 변경과 신규 파일은 실행 폴더의 `file-audit-final.json`에 기록한다.

## B. 실제 화면 근거

실행 폴더: `D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805`.

| 장면 | 실제 캡처 |
|---|---|
| 원형 충전/흡수·변형 | [충전](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/01-circle-charge.png), [중간 변형](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/02-circle-morph.png) |
| 반동/작아진 커서 전진/외곽 충돌 | [반동](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/03-circle-recoil.png), [전진](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/04-circle-dash.png), [충돌](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/05-circle-impact.png) |
| 육각 변형/삼각 빔/같은 빔 반환/도착 파편 | [변형](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/06-hex-morph.png), [발사](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/07-triangle-beam.png), [반환](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/08-same-beam-return.png), [파편](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Integrated-Editor/09-triangle-shards.png) |
| 실제 WebGL 경계/피격 수위 | [경계](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Web-Final/01-hex-charge-boundary.jpg), [HP2/3 수위](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Web-Final/02-circle-impact-energy.jpg) |
| 실제 브라우저 마우스 이동/잔상 | [최종 빌드 드래그](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Web-Final/05-final-mouse-drag.jpg) |
| 한국어 복사 보고서/세로 패널 | [보고서](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Web-Final/03-report-clipboard.jpg), [390×844 안정 레이아웃](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/Web-Final/04-mobile-390x844-settled.jpg) |

Editor 전투 화면은 내부 생성/실제 물리·잔상 경로로 시점을 잡았다. 실제 사람의 터치 입력으로 표시하지 않는다. WebGL 원형 시퀀스36프레임은 관찰용 .6초 충전/.45초 전진 실험이며 기본값 .15/.20/.60을 변경한 출하 값이 아니다. `Web-Final/browser-checks.json`에 입력 방식과 관측값을 남겼다. 크기 조절 직후의 이전 `04-mobile-390x844.jpg`, `06-mobile-numeric-settled.jpg`는 브라우저 합성 프레임이 늦어 이전 화면이므로 최종 레이아웃 증거로 쓰지 않는다.

## C. 초기값과 도구 사용

원형 준비2.5초 / 변형.15초 / 반동.05초·최종 월드 길이8% / 전진.20초 / 길이1.6·폭1.05 / 최종 커서배율.60 / 충돌 유지.4초.
육각 준비3.5초 / 변형.25초 / 외곽 도달.25초 / 반환.06초 / 파괴 후 풀복귀1.333초.
실제 원형 최종 월드 길이 .4416, 반동 이동량 .035328(기존 root.5·morphRoot.92 반영). 보고서/관측에 산출값을 표시한다.
넥서스3HP, 아트 본체 비율 .59241706, Editor 반경 약 .325520843. 웹 첫 안정 레이아웃별 약 .32422~.325. 라운드 도중 화면 크기 변경은 안정 판정을 흔들지 않으며 픽셀 정렬로 표시 반경에 약 .0004 차이를 관측했다. 효과 흔들림/수위는 판정과 분리한다.

전체141개 key·범위·단위·적용시점·제한과 사용법은 [TUNING_GUIDE](D:/renewal_CursorDash/docs/CursorDash/TUNING_GUIDE.md)를 따른다.
메인 **훈련 → 개발자 도구 → 검색/그룹 → 수치 변경 → 안전 재생성·시험** 순서다. 다음 라운드 값은 새 라운드에서 읽는다. 패널 열린 동안 기본 정지한다.
프리셋을 저장하고 **변경 보고서 → 클립보드 복사**로 검토값을 전달할 수 있다. JSON 파일 또는 HTML 텍스트 가져오기를 같은 allowlist로 검증한다. 기본값 승격/원본 저장은 별도 후속 작업이다.

실제 최종 빌드에서 출력한 예시: [전체141키](D:/renewal_CursorDash/docs/CursorDash/examples/cp14-full.json), [변경JSON](D:/renewal_CursorDash/docs/CursorDash/examples/cp14-changes.json), [한국어 보고서](D:/renewal_CursorDash/docs/CursorDash/examples/cp14-changes-ko.txt).
예시는 **원형 전진 .20→.45초 실험**이다. 사용자 확정 출하값을 .45초로 바꾸자는 제안/승인이 아니다. 검증 종료 시 세션 기본값으로 복원했고 저장 예시만 남겼다.

## D. 공식 테스트와 웹 검증

최종 공식 **27 PASS / 0 FAIL / 0 SKIP**. `run_tests(EditMode, CursorDash.EditorTests)` 정상 승인 경로이며 우회 runner/캐시 조작은 없다.
새 JSON/XML27개 이름/Passed, RunFinished 로그와 Editor Play/compile 종료를 교차 확인했다. 완료 UTC `2026-10-23T17:59:31.4580605Z`.
[집계](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/tests-final-summary.json), [JSON](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/CP3-CP4/Run-04/CursorDash-regression.json), [XML](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/CP3-CP4/Run-04/CursorDash-regression.xml).

[개별27개 테스트 이름과 결과](D:/renewal_CursorDash/Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805/official-cases.md).

기존16개를 유지하고 관련11개를 추가했다. 옛 육각 Animator 소멸 기대는 삼각 메시 숨김/도착 파괴로 갱신했다. 옛 고정 .3초 반사 대기는 실제 외곽 경로에서 BlockedBeams 증가를 제한시간 안에 기다리도록 바꾸고 반환/무피해 핵심 단언을 유지했다. 테스트 삭제/무시/핵심 단언 제거·무관한 기대 변경은 없다.
CP1 18/0 → CP2 첫20/1(타이밍 단언 보완)→21/0 → 통합 첫7/18(컴파일/Play 도메인 혼합 실행 이력 보존)→정상 재실행26/0→신규 보조 경로 포함27/0→첫 레이아웃/정지 미리보기 보완 후 최종27/0. 이전 실패 파일도 보존했다.
공식 진행 캐시가 Play 도메인 전환 뒤 stale running을 보였으므로 그 응답만으로 성공 판단하지 않았다. 과거 독립 검증 함수2개/USER_VERIFIED는 이번27개에 합산하지 않는다.

웹 PASS: 실제 마우스 이동·입력 해제·잔상, 원형 실제 피해1회/수위, 실제 빔 경로 보조 방어·반환·파괴, 요청.45/실제.20 분리 후 새 생성 적용, 프리셋 저장/복제/선택 삭제·새로고침 유지·비자동 적용, 일반 설정 저장/새로고침/복원, JSON 초기화→가져오기 왕복, 범위 오류 원자적 거부, 파일 선택→읽기/검증, 실제 다운로드 파일141키/buildId 확인, 보고서 복사 후 클립보드 일치·전체 선택 대안.
Chrome의 download 이벤트 1회 timeout과 앱 브라우저 native confirm 정체는 도구 한계로 기록했다. 사용자 취소 이후 최종 HTML 확인 UI와 실제 파일 다운로드를 앱 브라우저에서 검증했다. 실패했던 호출을 성공으로 바꾸어 기록하지 않는다.
Browser Console warn/error는 최종 수집에서0건. 활성 앱 브라우저 관측 약125~179FPS, 비활성 Chrome 약1FPS throttling을 함께 기록한다. 실기기 성능/프레임 유지 보증은 아니다.

## E. 빌드·보존·남은 문제

최종 **Succeeded, errors0/warnings0**, buildId `cp14-20261024-031826-fe9e3b0bcb8d`, 79,591,684bytes.
출력 `D:/renewal_CursorDash/Builds/LocalDevPreview/cp14-20261024-031826-fe9e3b0bcb8d`.
앞선 두 빌드도 새 폴더에 보존했다. 첫 빌드에서 발견한 CanvasScaler 이전 경계 포착을 수정해 두 번째에서 실제 표시/판정을 맞췄다. 마지막은 HTML 삭제 확인만 보완한 캐시 빌드(14.59초)다. 게임 C#은 최종27개 통과 이후 동일하다. buildId와 별도 SHA256 목록으로 소스를 식별한다.
[현재 미리보기](http://127.0.0.1:8765/)는 Python HTTP를 127.0.0.1에만 바인딩했다. 외부/LAN/방화벽/터널/공개 배포는 없다. 프로세스가 종료되면 ENVIRONMENT의 재개 명령을 사용한다.

- 시작 HEAD `8ceedbb7ead4c2b97417739f62fd7904e1a51497`/main 유지. 원래 tracked변경6개/미추적241개 보존, 기존 출력13개는 검증 결과를 별도 복사한 뒤 시작 바이트로 복원했다. 사용자 파일 삭제/이동이나 Git 복구 없음.
- 기존 최고36.93275, Flame 최고키 없음, BGM0/SFX0, 새 Editor v2키3개 부재와 SO10개 메모리를 사전 상태로 보존했다. Cyberpunk의 사전 메모리 highScore11.5285225와 디스크0은 서로 다른 상태였으므로 메모리 스냅샷을 기준으로 복원하고 SaveAssets하지 않았다.
- 종료 Editor: Cyberpunk clean, Play/compile/pause off, timeScale1. dirty는 D-014 Shader3+LiberationSans FontTexture4개뿐이다. 해당 파일/.meta/TMP 폰트 에셋을 초기화/재생성/전체 저장하지 않았다.
- Unity 빌드가 **TimeManager.asset 직렬화 형식**을 Fixed Timestep.02에서 v2 tickCount2822399/rate141120000(약.0199999929초)으로 자동 이관했다. 최대 timestep.1은 같다. 의도한 밸런스 변경과 분리해 보존/보고한다. Packages/Graphics/Quality/Player 설정의 해시는 최종 빌드와 일치한다.
- 최종 공식/Play 종료 로그에서 DOTween SafeMode 집계를 관찰하지 않았다. 자기 핸들 수명/해당 노드 경로의 결과이며 과거26건 전체 해결이라고 확정하지 않는다. SafeMode 끄기/로그 숨김/전역 KillAll 없음.
- Editor 종료 시 `Deleting invalid font reference.`를 관측했다. 스택/원인 확정은 하지 않았고 기존 TMP dirty만으로 동일 문제라고 단정하지 않는다. 웹 한국어 HTML/게임 텍스트·셰이더 오류는 관찰되지 않았다. 기능 검수를 막는 오류로 재현되지 않아 별도 후속 조사로 남긴다.

## F. 사용자에게 남긴 최소 검수

1. 훈련에서 원형 생성: 작은 보라 커서의 .15초 변형·짧은 반동·.20초 전진과 보이는 앞점 피격이 읽히는지 본다. 충전/반동 접촉 제거와 돌진 중 잔상 무시를 구분한다.
2. 육각 생성 후 잔상으로 방어: 방어 작은 반응→즉시 반환→도착 삼각 파편 순서와 소리/왜곡 강도를 본다.
3. HP를 연속 낮춰 실제 피해/종료는 즉시, 내부 수위만 이어서 내려가는지와 타격 뒤 Pause/재개·Home/Retry를 본다.
4. 실제 휴대폰에서 안전영역/주소창·키보드 변화, 소수·음수 입력, 긴 스크롤·멀티터치 UI 입력 차단/닫은 뒤 새 제스처, 설정·프리셋 새로고침, 파일/클립보드 대안, 가독성/발열/프레임 유지를 본다.
5. 원하는 값만 바꿔 **변경 보고서** 또는 **변경 JSON**을 전달한다. 모바일 LAN 공개/배포는 이번에 하지 않았으므로 실기기 접근 경로는 별도 승인 범위다.

자동 테스트·데스크톱 실제 입력·모바일 수동 검수는 서로 대체하지 않는다. 현재 기능 구현/자동·루프백 검증에 재현되는 차단은 없으며, 위 검수와 폰트 참조 메시지를 후속 항목으로 관리한다.
