# D-068 잔상 표시 복구와 고스트 감속·고정 꼬리 (2026-10-29)

> **D-069 후속 주의 (2026-10-29):** 아래 D-068은 당시 구현·검사 이력이다. 최신 사용자 요청으로 **정지 후 남아 보이는 꼬리도 실제 방어·반사가 가능**해야 한다. 아래의 ‘표시 전용/시간 만료 뒤 방어 불가/시드 무방어’는 [D-069](RETAINED_TAIL_DEFENSE_20261029.md)로 대체되었다. 표시 방식·20%/50% 길이·고스트.375/이동값 보존은 유지한다.

**LOCAL_FIX_IMPLEMENTED / STATIC_PASS / REFERENCE_COMPILE_PASS / NATIVE_DISPLAY_NOT_RUN / 사용자 Unity 검수 대기.**

사용자는 D-067 이후 실제 잔상이 보이지 않는다고 보고했다(**FAIL: 사용자 관찰**). 실제 플레이어의 이동 이상도 확인하도록 요청했고, 고스트는 현 속도보다25% 감속하되 표시 꼬리는 항상 최대 길이의50%로 유지하도록 재지정했다. 사용자는 이번 실제 Unity 검수를 직접 담당한다고 명시했다. Unity 창·Play·Test Runner를 조작하지 않았고, 빌드·커밋·push·배포도 하지 않았다. 이전 D-067 미커밋 변경은 보존한다.

## 원인 조사와 수정 경계

확인한 소스 차이는 D-067의 `TrailRenderer.emitting=false`, `time=86400`, `Clear/AddPosition/SetPosition` 수동 재구성 경로다. 기존 자동 표시를 교체한 이 경로의 **계산만 검사했고 실제 Unity 리본 생성·GPU 표시를 확인하지 못했다**. Unity 문서에도 TrailRenderer 정점의 타임스탬프·수명이 명시돼 있다([공식 AddPositions](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/TrailRenderer.AddPositions.html)). 다만 이번에 실제 Unity 내부의 어느 단계에서 표시가 누락됐는지까지 재현·확정한 것은 아니다.

재현되지 않은 시간 보정을 반복하는 대신, 표시 경로는 거리/정점에 맞는 기존 Unity [LineRenderer](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LineRenderer.html)로 바꿨다. 새 관리 시스템이나 다른 꼬리 디자인을 만든 것이 아니다.

- `PlayerCharacterAfterImage` 안의 기존 이동 샘플과 색·폭·감쇠·재질·레이어·정렬값을 사용한다.
- 같은 플레이어 아래 `Cursor movement ribbon` 자식 하나를 만들거나 고스트 복제에 이미 있으면 재사용한다. 원래 TrailRenderer는 참조 호환용으로 남기고 표시·자동 방출을 끈다. 시간은 원래 수명으로 돌린다.
- 표시 정점은 **머리→꼬리** 순서로 넣어 머리가 진하고 뒤로 감쇠되게 한다. 정지/변경 없는 정점은 재작성하지 않는다.
- 실제 레이저 방어는 기존 콜라이더 풀·시계·교차 계산을 그대로 쓴다. 표시용 기록 버퍼를 따로 두므로 느린 고스트의 긴 꼬리를 위해 방어 콜라이더나 수명을 늘리지 않는다.
- `FeverElementMask`에서 이 커서 자식 리본만 기존 alpha 보존 꼬리 마스크로 처리한다. 육각 레이저나 다른 선의 색·판정은 바꾸지 않는다. 고스트의 기존 GuideStyle도 같은 자식 렌더러를 수집하므로 회색/스케치·fade 경로를 유지한다.

실제 플레이어 `PlayerCharacterController.cs`와 `CursorDashTuning.cs`는 D-066/현재 HEAD와 동일하다. 기본 speed5.75/가속60/해제감속18/turn1800 및 입력·위치·회전 계산에 이번이나 D-067 변경은 없다. 실제 기기에서 느낀 움직임까지 정상으로 판정한 것은 아니다. 표시 복구 후 사용자 검수로 확인한다. 저장된 설정을 초기화하거나 강제로 덮어쓰지 않았다.

## 고스트 속도와 꼬리의 분리

기존 `Assets/Resources/CursorDash/VisualPrototype.asset`:

| 항목 | D-067 → D-068 | 의미 |
|---|---|---|
| `tutorialGhostInput` | .50 → **.375** | 현재보다25% 감속한 공통 순항 입력 |
| `tutorialGhostTailFraction` | 속도에 따른 길이 → **.50** | 속도와 별개인 고정 표시 길이 |
| `cursorRestTailFraction` | .20 유지 | 실제 플레이어 정지 표시 꼬리 |
| `tutorialDefenseNexusGap` | .60 유지 | 넥서스 본체 앞 수비 지점 |

기본값 계산상 고스트 순항 속도는2.875→**2.15625world/s**, 표시 꼬리는 **1.95world**(최대3.9의50%)다. 고스트 생성 첫 프레임부터 진행 방향 뒤쪽에 표시 전용 꼬리를 시드하고, 이후 실제 이동 경로로 교체하며 길이를 유지한다. 시드는 방어 샘플이 아니므로 점수/반사/가짜 접촉을 만들지 않는다. 고스트 초기 방향은 오른쪽으로 일관되게 두어 하단 안내문 쪽으로 초기 꼬리가 내려가지 않게 했다.

고스트의 실제 방어에 유효한 이동 기록은 여전히 기존 **.6782609초**다. 느린 이동에서 표시 꼬리 전부가 유효 방어 영역이라는 뜻은 아니다. 원형90% 게이트·실제 잔상 반사·소유권·일반 공격·레이저 속도는 유지한다. 느려진 고스트의 곡선 준비만 기존.65초를 입력 비율에 따라 **약.8667초**로 앞당겼다. 이는 고스트 조작 준비이며 육각 타임라인/실제 발사/판정을 조절하지 않는다.

표시 기록 버퍼는 최소 샘플 거리와 최대 표시 길이에 맞춰 별도로 확보하며, 기존 방어 풀의 개수와 독립이다. 재사용 배열/렌더러만 쓰고 매 프레임 재질·게임 오브젝트를 만들지 않는다. 모바일 성능을 실제 측정하지는 않았다.

## 검사

- **PASS 정적:** diffcheck, 범위·보존/HEAD/index 유지, 일반 플레이어 Controller/Tuning·씬/프리팹·육각·점수·피버·공급·파편·기존 D-067 점수/문구 변경 보존.
- **PASS 참조 컴파일:** runtime118/test17/Editor2 오류0, 기존 runtime경고133. Unity 실행 없이 C# 참조 컴파일한 결과다.
- **PASS 관리형 계산/메타데이터:** 실제 꼬리 소스와 고스트/Controller 수학을 어댑터에서 실행. 플레이어 정지20%/머리 연결/만료 방어 불가/초기화, 고스트 최초50% 시드·느린 이동을 길게 반복해도1.95 유지·정지 뒤에도50%/방어 풀 불변, 일정.375 입력·제어된 방향8×계산간격3의 방어 교차24를 확인했다. 실제 Unity 리본·재질·물리·GPU 확인이 아니다.
- **추가한 공식 회귀 단언:** 실제 자식 LineRenderer 활성/정점·머리 연결, 기존 TrailRenderer 비활성, Native BakeMesh의 실제 기하 생성, 고스트 최초·이동 중1.95/입력.375/표시기록과 방어 풀의 분리. 작성·컴파일만 했고 실행PASS로 표시하지 않는다.
- **NOT_RUN:** 사용자가 직접 검수 선택. Unity 컴파일/정상TestRunner/Play·실제 표시·재질·피버 검은 궤적·손조작·모바일/WebGL·성능은 미검증이다. 새 실제 캡처·동영상도 없다.

로그 `Cursor-Dash-master/Logs/trail-display-repair-20261029`, 사본 `Backups/trail-display-repair-20261029`. 시작 main9121b78/빈index·기존D-067 미커밋 상태, 1,648파일/기존미추적7,952경로를 보존한다. 공식 API 조사 외에 새 설치·연결·전역 변경·컴퓨터 조작은 없다.

## 사용자 검수 순서

1. **Cyberpunk 메인→시작→이동**: 흰 꼬리가 머리에 붙어 보이는지, 급회전에서도 경로가 이어지는지 확인한다.
2. **정지→다시 이동**: 짧은20% 표시 꼬리가 남고 정상 길이로 이어지는지 확인한다. 기존보다 실제 플레이어 속도가 달라 보이면 현재 player.speed/가속·해제감속 값과 함께 제보한다. 임의 초기화하지 않는다.
3. **튜토리얼부터 시작**: 고스트 생성부터50% 회색 꼬리가 있고, 현재보다25% 느린 일정 속도로 움직이는지 본다. 원형 접근/넥서스 앞 육각 수비·실반사가 자연스럽고 반복 성공하는지도 확인한다.
4. **피버/종료·Pause/재개·Retry·Home**: 검은 궤적·정상 색 복귀, 남은 꼬리/중복 리본/이전 기록이 없는지 본다.

기존 체크리스트 문구·동시 점수 전환/투명도 선행, 게임 규칙·보류 항목은 유지한다. **다음 한 작업은 사용자 Unity 실제 표시·조작 검수**이며 수정은 미커밋 상태로 남긴다.

## 이번 추가 변경 파일과 최종 보존

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Character/PlayerCharacterAfterImage.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGhostDemonstration.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverElementMask.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAutonomousGuideTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/TUTORIAL_CLARITY_20261029.md`
- `docs/CursorDash/VALIDATION.md`
- `docs/CursorDash/TRAIL_DISPLAY_GHOST_SPEED_20261029.md` (새 인계)

기존1,648파일 중 위12개 관련 추가수정/삭제0·예상외변경0, 기존미추적7,952경로 유지, main/HEAD/index 불변. D-067의 다른 코드·점수/문구 변경은 손대지 않았다.
