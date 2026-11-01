# D-070 — 메인 로고의 편집 기준과 runtime 연출 분리

2026-11-01. **LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / FOCUSED_NATIVE_5_PASS / 제한된 시각 비교 PASS / 사용자 검수 대기.** 이번 범위는 0단계 보존과 1단계 로고 하나뿐이다. 다른 UI·게임 규칙·설정 체계·에셋 정리·빌드·Git게시 없음.

## 작업 전 보존

- Git `D:/renewal_CursorDash`, Unity `D:/renewal_CursorDash/Cursor-Dash-master`, main / HEAD `9121b786cb3e4fad98c44f8f304341a6ceaa8ba6`. 기존 추적 수정17개·미추적 작업 보존, staging/브랜치/이력 변경 없음.
- 실제 MCP `Cursor-Dash-master@6cde4df4002de875`·프로젝트 경로·Cyberpunk를 재확인했다. 씬 dirty=false에서 착수했다. Project_Brawl 요청/MCP 설정·서버 변경 없음.
- 저장소 밖 사본: **`D:/CursorDash_Backups/main-logo-view-20261101-1710`**.
- Workspace: Assets 전체(미추적/.meta 포함), Packages, ProjectSettings, docs/CursorDash, tools, AGENTS.md, .gitignore. SHA256 검증 1,625파일 / 152,033,705바이트. `manifest-before.json`, `git-status-before.txt`에 기준 기록.
- CursorDash 전용 저장 데이터는 `playerprefs-registry-before.json`(존재/타입/값)과 PersistentData 사본으로 보존했다. Unity 연결 주소 참조만 별도 기록했고 전역 설정/인증정보는 복제하지 않았다. Library/Temp/기존 대형 빌드·배포·참고영상은 제외, 원본 삭제 없음.
- 초기 imported dirty 4개(TMP shader3/LiberationSans Font Texture)는 저장·폐기하지 않았다. 종료 시 Medium/Bold TTF의 runtime Font Texture도 dirty 조회에 나타난다. 해당 디스크 파일은 모두 작업 전 SHA256과 동일하다. SaveAssets/포괄 저장 없음.

## 변경 전후 기준

실제 Unity Main/시작/Home을 1080×1920에서 확인했다. 기존 SpaceFlow의 메서드 호출로 재현했으며 손·터치 검수와 구분한다.

| 항목 | 보존한 기본값 |
|---|---|
| Sprite | Assets/Art/UI/Branding/Logo.png, GUID f8abf33bd82f6b742a3716e6bce57786 / 994×280 |
| 배치 | 앵커(.5,.79), Pos(0,0), Pivot(.5,.5), Scale1 |
| 크기/색 | 840×236.61972 Canvas 단위 / White RGBA(1,1,1,1) |
| 렌더 | Screen Space Overlay / Sorting25 / Layer0 / 카메라 미지정 |
| 화면 대응 | Scaler1080×1920, Match.5, 최대 Canvas 폭86% |
| 시작 퇴장 | 오른쪽150 Canvas 단위 / 전체1.35초의 첫22%=.297초 |
| 곡선 | t=Clamp(진행/.22), 이동=t², alpha=1−SmoothStep(t) |
| 메인 복귀 | 기존 reveal.45초 Smooth / offset150→0 / alpha0→1 |

전후 native 프레임의 actual age로 같은 수식을 비교했다. 전/후 이동 오차 각각 .000006단위 미만, alpha .0000001 미만. 프레임 시각이 같다는 뜻은 아니다. 배경은 원래 움직이므로 전체 픽셀 동일 비교가 아니다.

## 파일 변경 / 기존 변경 구분

작업 직전 사본과 비교한 기존 게임 파일 변경은 **3개뿐**이다.

- `Assets/Scenes/Cyberpunk.unity`: MainLogo prefab 인스턴스1개와 StageManager 참조 추가. 기존 오브젝트/컴포넌트 삭제·재배치 없음.
- `Assets/Scripts/Gameplay/StageManager/StageManager.cs`: serialized MainLogoView 참조/getter만 추가.
- `Assets/Scripts/Gameplay/StageManager/CursorDashSpaceFlow.cs`: 로고 생성·표시·포즈를 연결된 View에 위임. 이 파일의 기존 D-067 점수 전환 수정은 그대로 보존.

새 파일: `Assets/Prefabs/UI/MainLogo.prefab`, `Assets/Scripts/Gameplay/StageManager/CursorDashMainLogoView.cs`, `Assets/Tests/Editor/CursorDashMainLogoViewTests.cs`와 각각의 `.meta`.

문서는 이 문서/STATUS/MASTER_SPEC/DECISIONS/VALIDATION/AGENTS에 이번 범위와 결과만 추가했다. VisualPrototype 및 기존 꼬리·고스트·점수·피버·입력·테스트 소스는 작업 직전 사본과 동일하다. HEAD 전체 diff를 이번 변경으로 취급하지 않는다. 관련 로그는 `Cursor-Dash-master/Logs/main-logo-view-20261101/`이며 기존 Reporter의 `Logs/CursorDash-regression.json/xml`도 이번 실행 결과로 갱신되었다.

## 책임 분리 / Unity 사용법

```text
MainLogo                         Canvas / Scaler / CursorDashMainLogoView
└─ PlacementRoot                 사람이 편집하는 위치·크기
   └─ AnimationRoot              runtime 상대 이동·폭 제한 scale·CanvasGroup alpha
      └─ Logo                    Image / Source Image / Color
```

- **정식 기준은 Scene/Prefab의 PlacementRoot와 Image**다. SpaceFlow가 위치/anchor/size/sprite/color를 덮어쓰지 않는다. Scene Override도 사용한다.
- Main/Home은 AnimationRoot의 상대 이동0/alpha1로 복원한다. 좁은 화면의 폭86% 제한도 자식 scale에만 적용하므로 기준 RectTransform은 변하지 않는다.
- VisualPrototype의 mainLogo/spaceLogoWidth/spaceLogoY는 삭제하지 않았다. View가 없는 경우의 **legacy fallback**이며 연결된 Cyberpunk에는 적용되지 않는다. JSON/프리셋/Tuning API 변경 없음.
- 프롬프트/BEST 캡션은 기존 runtime Canvas에 남는다. 연결된 Cyberpunk에는 runtime `Existing title logo`를 추가 생성하지 않는다.
- ExecuteAlways나 에디터 게임 로직을 사용하지 않는다. 편집 표시 자체는 저장된 Canvas/Image다.

**편집할 정확한 경로:**

1. Cyberpunk Hierarchy `MainLogo/PlacementRoot` → RectTransform Anchors/Pos X,Y/Width/Height. 현재(.5,.79), (0,0), 840×236.61972, Scale1.
2. `MainLogo/PlacementRoot/AnimationRoot/Logo` → Image Source Image/Color. Preserve Aspect는 기존처럼 켜져 있다.
3. 이 씬 배치는 Scene 인스턴스를 먼저 편집한다. 공통 기본을 바꾸려면 Project의 `Assets/Prefabs/UI/MainLogo.prefab`를 열어 같은 자식을 편집하거나 의도한 Override만 Apply한다.
4. AnimationRoot의 position/scale/CanvasGroup alpha는 사람이 배치 편집할 항목이 아니다. MainLogo Canvas/Scaler/Layer/정렬과 View 참조도 유지한다. exitDistance150/maximumCanvasWidth.86은 기존 동작 보존용이다.
5. Play 중 편집은 일반 Unity 규칙대로 기본 Scene 값으로 저장되지 않는다. 기본 배치는 Play OFF에서 편집/저장한다.

## 검사 결과

| 구분 | 결과 / 범위 |
|---|---|
| 작업 전 실행 | PASS: Main/시작 퇴장/Home, 실제1080×1920 |
| native 컴파일 | PASS: 새 View·연결·검사 import/compile 후 사용 가능 / 최종 Console 오류·경고0 |
| 편집 모드 | PASS: Play OFF의 실제 Scene/Game View에서 표시·PlacementRoot 선택 |
| 기준값 유지 | PASS: 임시 Pos(31,-26)/크기720×203/다른 기존 Sprite/tint를 Scene 저장 → Play → Home 두 번 → Play 종료 후 유지 |
| 기본 복원 | PASS: 원본 Sprite/White/840×236.61972/Pos0/Scale1 복원, 검사 중 생긴 불필요한 로고 Override 정리 |
| 기존 흐름 | PASS: timeline 이동/alpha·중복 시작 거부·quick/Home·결과 확인 reveal/Home·중복 로고 없음 |
| 실제 시각 비교 | PASS: 같은1080×1920 Main 기본 외형/선택 가능한 편집 로고/퇴장·복귀 프레임 확인 |
| 저장 데이터 | PASS: 종료 후 Editor12개/standalone0개 레지스트리의 존재·타입·값과 PersistentData 3파일 해시가 사본과 동일, BEST131 유지. 전체 초기화 없음 |
| 보존 범위 | PASS: 사본의 기존1,625파일 삭제0 / 예상 밖 기존 소스·에셋·설정 변경0 |
| 손 입력/UI 전체 | NOT_RUN: 실제 마우스·터치/설정 패널 전체 회귀는 수행하지 않음 |
| 모바일/WebGL/성능 | NOT_RUN: 다른 실제 화면비·기기·브라우저·성능 검사 없음 |
| 전체 전투 회귀 | NOT_RUN: 노드/튜토리얼/피버 전체 완주를 이번에 다시 확인하지 않음 |

정상 Test Runner 최종 **5/5 PASS**(편집/Prefab3, 실제 EnterPlayMode/ExitPlayMode2), `native-logo-tests.json/xml`에 개별 결과가 있다. 첫 실행3FAIL은 Test Runner 빈 씬에서 시작한 검사 준비 문제였다. 미저장 씬이 없는지 확인한 뒤 저장된 대상 씬을 여는 준비만 추가하고 핵심 단언을 유지해 통과했다. MCP job 추적은 domain reload 뒤 갱신이 멈췄지만 기존 Unity Reporter의 실제 XML/JSON으로 완료를 확인했고 종료된 job 정보만 정리했다. 서버 재시작/실패 우회 없음.

진단 캡처 코드의 타입/직렬화 오류는 게임 컴파일 오류와 구분했다. Unity가 생성한 새 scene `m_Name: `의 공백은 diff whitespace 경고이며 C# 소스 검사와 구분한다. 무관한 기존 YAML을 정리하지 않았다.

## 실제 증거와 수동 확인

모두 Unity 루트 기준 `Logs/main-logo-view-20261101/` 아래:

- 전: Baseline/main-before.png, home-before.png, motion-native-before.json, exit-native-before-*.
- 후: After/main-after.png, home-after.png, motion-native-after.json, exit-native-after-*.
- 편집: After/scene-edit-logo-selected.png, After/edit-mode-refreshed.png.
- 결과/보존: native-logo-tests.json/xml, native-final-state.json, logo-motion-comparison.json, preferences-before-restore.json(차이0), persistent-data-check.json(3파일 동일), delta-check.json, changes-from-task-start.patch.
- 초기 Baseline/motion-before.json과 edit-mode-after.png는 각각 중간 프레임 누락/Canvas 재표시 전 캡처이므로 성공 증거로 사용하지 않는다. 연속 동영상 증거는 없다.

사용자 순서: Cyberpunk → PlacementRoot 선택/F → 위치·크기 임시 편집/Scene 저장 → Play Main → 시작 퇴장 → Home/반복 → Play 종료 후 편집 기준 확인 → 원하는 기본 배치로 복원/저장. 이미지 변경은 Logo Source Image에서 같은 경로로 확인한다.

종료 Play OFF/timeScale1/Cyberpunk dirty=false/PlacementRoot 선택. imported dirty와 runtime Font Texture는 저장·폐기하지 않았다. 디스크 파일 해시 동일. OS 직접 입력·다른 프로젝트 종료 없음.

## 이번 변경만 되돌리기 / 보류

이후 추가 편집이 없는지 먼저 확인한다. 사본 Workspace에서 Cyberpunk.unity, StageManager.cs, CursorDashSpaceFlow.cs의 **작업 직전 버전**을 복원한다. HEAD로 되돌리면 기존 사용자 변경을 잃으므로 금지한다. 새 MainLogo prefab/View/검사와 각각 .meta만 제거하고 이번 문서 추가분을 되돌린다. 이후 수정이 있다면 이번 로고 변경 부분만 수동 역적용한다. 폴더 전체 삭제/reset/clean/stash를 사용하지 않는다. 이 복구는 실제 실행하지 않았다.

다른 UI 이전/로고 디자인 개선/노드·배경·입력·피버 변경/폴더 재편·미사용 삭제는 미진행. 일반 노드 겹침·새 Flash 그림·별도 사이버 재구축은 계속 보류. **다음 한 작업은 사용자 로고 편집 검수**이며 자동 다음 단계/빌드/Git게시 권한은 없다.
