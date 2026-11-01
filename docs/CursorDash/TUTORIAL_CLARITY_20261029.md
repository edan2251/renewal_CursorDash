# D-067 튜토리얼 행동·정지 꼬리·점수 동시 전환 (2026-10-29)

## 후속 사용자 검수 실패·대체 구현

사용자가 이 수정의 실제 잔상 미표시를 보고했다(FAIL). 아래 관리형 계산/참조 컴파일 PASS는 실제 Unity 표시 보장이 아니었다. 수동 TrailRenderer 표시는 D-068의 거리 기반 리본으로, 고스트.50/속도 의존 꼬리는.375/고정50%로 대체했다. 나머지 문구·점수 전환과 기존 미커밋은 유지한다. 최신 내용은 [D-068 인계](TRAIL_DISPLAY_GHOST_SPEED_20261029.md)를 따른다.

**IMPLEMENTED / STATIC_PASS / REFERENCE_COMPILE_PASS / NATIVE_PLAY_NOT_RUN / 사용자 Unity 검수 대기.**

최신 사용자 요청의 네 항목만 로컬 수정했다. 기준 main `9121b786cb3e4fad98c44f8f304341a6ceaa8ba6`/시작 tracked 변경·index 없음. 적용 지침은 `C:/Users/molph/.codex/AGENTS.md`(빈 파일), `D:/renewal_CursorDash/AGENTS.md`와 최신 사용자 본문이다. 관련 하위 지침은 발견되지 않았다. STATUS→MASTER_SPEC→DECISIONS/ENVIRONMENT/VALIDATION/SOURCES→D-066 릴리스와 D-064 튜토리얼 인계를 읽었다.

작업 전 1,647파일 해시와 기존 미추적 7,933경로를 기록하고, 수정 후보의 사본을 `Backups/tutorial-clarity-20261029`에 보존했다. 로그는 `Cursor-Dash-master/Logs/tutorial-clarity-20261029`이다. 브랜치 전환·Git 쓰기·Unity 실행/창 조작/Play·미저장 저장/폐기는 하지 않았다. 이전 배포 승인은 재사용하지 않는다. 공개본은 D-066 그대로이며 이 로컬 수정은 배포되지 않았다.

## 변경 내용과 원인

1. 체크리스트를 `원형노드 파괴 0/2`, `육각노드로부터 수비 0/2`로 바꿨다. 카운터·완료 표시는 유지한다. 문구만 바꾸며, 육각 성공은 기존 실제 잔상 반사→반환탄 처치 완료 경로로 집계한다. 강제 통과·성공 조건 완화는 없다.
2. 기존 TrailRenderer와 이동 샘플을 재사용해 정지 뒤에도 최대 이동 꼬리의 약 20%를 표시한다. 기본 최대 길이 `5.75×0.6782609=3.9world`에 대응하는 최소 표시 길이는 **0.78world**다. 실제로 지나온 경로를 뒤에서 잘라 남기며, 머리의 최신 위치에 연결한다. 이동 기록이 짧으면 존재하지 않는 경로를 새로 만들지 않는다. 표시용 최소 꼬리는 레이저 방어를 연장하지 않는다. 방어 콜라이더/수명/교차 계산은 기존 값을 사용하고, 등장 경로는 계속 표시 전용이다. 고정 배열과 기존 렌더러를 사용하며, 같은 기하가 유지되면 다시 쓰지 않고 개수가 같으면 기존 정점을 갱신한다. 모바일 성능을 측정한 것은 아니다.
3. 기존 점수는 전환 35%(기본 약 0.4725초)에 카운트 진행이 끝났고, 자릿수 축소 공식 때문에 작은 최고점은 그보다 먼저 0이 됐다. 반면 색·투명도는 전체 1.35초를 사용했다. 이제 **숫자·색·크기/위치는 같은 시작과 전체 종료 시점**을 쓰며, **투명도만 전체의 55%(기본 0.7425초)에 먼저 완료**한다. 양수 최고점은 마지막 시점에 0이 되도록 정수 표시 계산도 바꿨다. 표시 전용이며 실제 최고점/점수/저장·보상 이벤트는 변경하지 않는다. 같은 TMP 하나와 기존 글자 획 회로 가림·Auto Size/Rect4×4/Scale1을 유지한다. 결과→메인 전환과 득점 반응도 변경하지 않는다.
4. 고스트는 시간에 맞춰 느리게 이동하던 `FollowRoute`와 접촉/수비 직전 최대 입력 `MoveToward`가 분리돼 있었다. 이제 방향만 부드럽게 돌리고 접근·접촉·수비에 동일한 순항 입력을 쓴다. **.70→.50**은 기본 최대 꼬리 길이의 약 50%에 대응한다(기본 순항 2.875world/s, 정상 이동 꼬리 약 1.95world). 대기 시간은 속도를 낮추는 대신 한 번의 넓은 곡선 길이로 배분하고 경로 진행은 길이 기준으로 추적한다. 고스트만의 자동 입력이며 실제 플레이어 이동·회전 설정은 바꾸지 않는다.

육각 수비 지점은 기존 노드→넥서스 구간의 46%에서 **실제 넥서스 본체 반경+0.60world 앞**으로 옮겼다. 그 지점 양옆 0.60world를 가로지르는 잔상을 만들고 기존 실제 레이저가 이를 반사한다. 고스트의 곡선/회전 준비 여유는 발사 0.65초 전이며 실제 공격 시각·레이저 속도·판정은 바꾸지 않는다. 원형의 약 90% 충전 후 접촉 게이트는 유지한다. Pause/완료/fade/화면 경계의 실제 이동 제한은 예외적으로 유지되며, 시작 가속까지 제거한 것은 아니다. 임의 생성과 여러 대상이 섞인 실제 동선의 자연스러움·성공 반복은 사용자 Play 검수가 필요하다.

## 조절 위치

기존 `Assets/Resources/CursorDash/VisualPrototype.asset`:

| 값 | 이전 → 현재 | 용도 |
|---|---|---|
| `tutorialGhostInput` | .70 → .50 | 접촉/수비/순항에 동일한 입력 크기 |
| `tutorialDefenseNexusGap` | 선분 46% → 본체 반경+.60world | 고스트 수비 위치 |
| `tutorialGhostSteering` | 7 유지 | 고스트 방향의 부드러운 추종 |
| `tutorialGhostCruiseSeconds` | 8 유지 | 순항 곡선의 여유 길이 기준(공격 타이머가 아님) |
| `cursorRestTailFraction` | 0 → .20 | 전체 커서 표시 꼬리의 최소 길이 비율 |
| `spaceScoreOpacityEnd` | 전체 전환까지 → .55 | 투명도 완료 시점의 전체 전환 내 비율 |
| `spaceStartSeconds` | 1.35 유지 | 점수/공유 공간 전환 전체 길이 |

물리 꼬리 수명 `trail.lifetime=.6782609`, 실제 입력·속도/회전·판정 폭은 기존 도구 값을 그대로 사용한다. 손으로 그린 고스트 표시·공급 간격/개수·실습 교대·첫 피버 안내·종료 전원 복귀는 유지한다. 육각 타임라인·일반 노드/반사/파편·점수/배율·피버 시간/공급·설정 UI·결과 대기·보류 항목은 수정하지 않는다.

## 검사와 한계

- **PASS 정적:** 변경 파일 범위/비밀정보 추가 없음, `git diff --check`, HEAD/index 유지, 기존 파일·미추적 보존. 상세 `preservation-final.json`.
- **PASS 참조 컴파일:** 실제 저장된 코드 118개/테스트17개/관련 Editor2개, 오류0. 기존 참조 경고133, 테스트/Editor 경고0. Unity를 실행한 컴파일 결과와 구분한다.
- **PASS 순수 계산:** 실제 점수 타임라인·꼬리 소스와 고스트 경로/Controller 수학을 추출해 관리형 벡터 어댑터에서 실행했다. 0/1/8/99/큰 자릿수의 동시 전환·정확한 0/단조 감소/투명도 선행, 0.78 꼬리/굴곡/머리 연결/만료 방어 불가/초기화, 일정 아날로그 입력과 방향8×계산간격3의 **24개 제어된 수비 경로 교차**를 확인했다. 이것은 실제 Unity 물리·렌더링·고스트 전체 행동·성능 검증이 아니다.
- **수정 중 FAIL 이력:** 처음 참조 컴파일의 읽기 전용 TrailRenderer.positionCount 할당은 지원 API로 수정했다. 경로 계산 초기 교차 실패는 고스트 준비 여유로 보완했고, 120회/초 직접 RecordMovement를 호출해 원래 샘플링을 건너뛰던 검사 준비도 실제 LateUpdate 샘플링 사용으로 고쳤다. 최종 결과와 중간 로그를 별도로 보존했다. 성공 집계나 실제 발사/반사 규칙은 변경하지 않았다.
- **NOT_RUN:** 실제 Unity 컴파일/정상 TestRunner·물리·GPU/손 Play·모바일/WebGL·성능·재실행 저장. 컴퓨터 조작 없이 완료했으며 새 실제 화면·영상은 촬영하지 않았다.

관련 공식 테스트는 동일 입력 크기/넥서스 앞 실제 반사 접촉과 문구를 보강했다. 정지 꼬리는 남아도 만료 레이저를 막지 않고 Clear/Home에서 사라지는 Unity 회귀 사례를 추가했다. 기존 ‘35%에서 카운트 종료’ 기대값과 ‘TR시간=방어 수명’ 기대값은 이번 표시 사양에 맞게 교체하고 실제 수명·속도·폭 및 방어 단언은 유지했다. 작성/참조 컴파일만 했으며 공식 실행 PASS로 표시하지 않는다.

## 사용자 Unity 검수 순서

1. `Assets/Scenes/Cyberpunk.unity`를 열고 **메인→시작**: 최고점 숫자·색·크기가 함께 변하고 투명도가 먼저 안정되는지 본다. 실제 기록을 테스트용 값으로 바꾸지 않는다. 0점이면 숫자 변화 없이 같은 시각 전환만 보인다.
2. **이동→정지→급회전→정지**: 짧은 꼬리가 머리에 붙어 남는지, 이동 중에는 기존처럼 길어지는지 본다. 잠깐 정지해도 오래된 표시용 꼬리로 레이저가 반사되면 안 된다.
3. 메인 설정의 **튜토리얼부터 시작**으로 기록 초기화 없이 재진입한다. 새 두 문구와 실제 2/2 집계를 확인한다.
4. 조작하지 않고 여러 번 관찰: 고스트가 처치/수비 때만 빨라지지 않는지, 약 절반 길이 꼬리를 유지하며 넓은 곡선으로 이동하는지, 원형은 약90% 이후 접촉하는지 본다.
5. 육각이 여러 방향에 생길 때 **넥서스 앞에서 가로지른 잔상→실제 레이저 접촉→반환 경로**가 읽히는지 본다. 수비 타이밍·자연스러움과 성공 반복은 아직 실제 검증하지 않았다.
6. Pause/재개·Retry·Home·튜토리얼 완료→본게임·첫 피버/종료에서 꼬리·색·고스트/전환 상태가 남지 않는지 확인한다.

**다음 한 작업:** 사용자 Unity 시각·조작 검수. 로컬 변경은 미커밋 상태로 남기며 자동 Play·빌드·커밋·push·배포/다음 기능 구현을 시작하지 않는다.

## 최종 변경 파일

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Character/PlayerCharacterAfterImage.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGhostDemonstration.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashPractice.Guide.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashSpaceFlow.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashStartTimeline.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAutonomousGuideTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSceneRegression.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSpaceFlowTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`
- `docs/CursorDash/TUTORIAL_CLARITY_20261029.md` (새 인계 문서)

최종 보존 검사: 기존 1,647파일 중 위16개 관련 수정/삭제0·예상외변경0, 기존 미추적7,933경로 유지, HEAD/main·index 불변. 로그·백업·새 인계 문서는 별도로 남긴다.
