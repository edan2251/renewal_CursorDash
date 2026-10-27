# D-062 — 실제 규칙으로 플레이하는 회색 고스트 튜토리얼

> **후속 D-063 적용:** 현재 실습은 한개씩 원형부터 교대·제거후1.2초, 고스트는 연속곡선, 종료전환2.2초·점수숨김/재등장, 첫피버만충정지3초/패널2초·배속3초복귀다. 일반처치회로도 이전강한상태로복원했다. [최신시간표·검수](TUTORIAL_PACE_FEVER_NOTICE_20261028.md)를 우선 읽는다. 아래 다중공급/짧은안내값은 이력이며 실제객체·소유권·실제반사집계 구조는 유지한다.

2026-10-28 / 로컬 구현·Unity 집중 검수. 다음 한 작업은 사용자 시각·손 조작 검수다. 빌드·커밋·push·배포하지 않는다.

## 현재 요구와 보존

이번 사용자 CP0~CP6 및 “이번 변경의 Unity 검수 조작 허용” 회신이 근거다. D-061의 **순백색·시각 모사·원형 다음 육각·반대편 동시 생성**은 폐기된 구현 이력이다. 공개 도구/설정 숨김, 일반 처치 회로 완화, 사망 뒤 점수 클릭대기와 첫 피버 안내는 그대로다.

HEAD main `8d26194dc2c7940145c4c394a5d3ad56eb15c9bc`, 빈 index와 이전 미커밋/미추적 변경을 보존했다. 이번 기준은 이전 작업 시작1808파일이 아닌 **1819파일**이다. 비교/복구 사본은 `D:/renewal_CursorDash/Backups/autonomous-guide-20261028`, 해시·검사·화면은 `Cursor-Dash-master/Logs/autonomous-guide-20261028`에 있다. 씬·프리팹·Packages·ProjectSettings·원본 리소스·사용자 Recovery/Archive/영상은 수정·정리하지 않았다.

첨부 `C:/Users/molph/Downloads/녹음 2026-10-28 010703.mp4`는 존재(17,557,160바이트)만 확인했다. 기존 WinRT 프레임 추출 시 Windows 스크립트 실행 정책에서 차단되어 디코딩/관람하지 못했다. 정책을 우회·변경하지 않았다. 아래 원인은 영상에 대한 단정이 아니라 실제 코드와 Unity 판정 경로 확인이다.

## CP0 — 육각 0/2 원인과 수정 경계

실제 경로는 `ArrowNode.Fire → 소유한 PlayerCharacterAfterImage.TryBlockBeam → Block(Resolving) → 기존 .06초 반환 → Impact → StageManager.RegisterHexKill → Practice.Confirm`이다.

D-061 실습 정리가 `Resolving` 노드를 즉시 실패로 회수해, 정상 반사탄이 육각으로 돌아오기 전에 실행을 취소했다. 이제 `ReflectedReturnPending`이면 그대로 유지하고 실제 `Impact`만 집계한다. 미반사 넥서스 도달/비활성·이전 실행만 실패 회수한다. 강제 완료·카운터 보상·레이저 속도나 판정 완화는 없다.

이전 고스트는 실제 Node/Collider 없이 별도 도형을 그린 시각 모사였다. 실제 육각의 중첩 `Node` 변환과 별도 .88배 표현이 일치하지 않을 수 있었다. 이번에는 원래 풀의 같은 객체·동일한 중첩 변환을 사용한다. 기본 육각 타이밍은 확인에 필요한 현재 실행값을 읽을 뿐 변경하지 않았다.

## CP1~CP4 — 실제 객체·소유권·연속 실습

- 기존 실제 커서 GameObject를 시범용으로 복제하고 **같은 PlayerCharacterController/TrailRenderer/잔상 구현**에 자동 이동 입력만 공급한다. 순간이동이나 Contact/Kill 직접 호출로 시범 처치를 만들지 않는다.
- 원형·육각은 **원래 NodeCreator 풀과 Execute/Update/실제 Physics2D/반사 경로**를 사용한다. 소속은 Node·커서·잔상에 명시하고, 실제 플레이어 전역 인스턴스를 고스트로 교체하지 않는다. 커서끼리의 물리 쌍만 무시하며 전역 레이어 설정은 변경하지 않는다.
- 실제 커서는 실제 원형만 접촉하고, 각 레이저는 소속 잔상만 검사한다. 고스트 성공/실패는 로컬 카운터·로컬 파편/회수로 끝나며 실제 점수·배율·HP·게이지·방어 연속·회로/화면 피드백을 호출하지 않는다. 실제 실습의 기존 피드백은 유지한다.
- 고스트는 가까운 원형으로 실제 이동하며, 육각 발사 예고를 보고 실제 경로 중간을 가로질러 잔상을 만든다. 반환·처치까지 기다린다. 실패나 풀 재사용 때 새 실행 토큰을 확인해 목표를 다시 고른다. 100% 무실수 봇이나 플레이 가독성 최종 승인을 의미하지 않는다.
- 실습과 고스트의 시계·난수는 독립이다. 처음부터 두 체크리스트를 표시하며 원형/육각 성공 순서는 자유, 각 2회가 상한이다. 실제 노드 reference+ExecutionId의 성공만 한 번 수락한다. 실패·풀 반환·시범 처치는 집계하지 않는다.
- 생성 후보는 원래 artwork의 실제 바깥 반경과 중첩 scale, 양쪽 활성 풀, Nexus·HUD·체크리스트·두 커서 머리·화면 여백을 함께 검사한다. 최대48후보에서 안전한 곳이 없으면 다음 공급 기회로 미룬다. 이동 영역을 반으로 나누거나 노드를 지워 공간을 만들지 않는다.
- 회색은 객체별 소유 재질/PropertyBlock과 고정된 국소 잉크 변화로 표현한다. 원형·육각의 실제 mesh/충전·회전·변형을 유지하고, 공유 재질/원본 asset은 수정하지 않는다. 매 프레임 무작위 윤곽이나 크기 변경은 없다. 반환 시 원래 재질·색·블록을 복원한다.
- 로컬 원형/육각 파괴는 기존 효과를 각 최대3개씩 재사용한다. mesh/material을 재사용하지만 기존 파편 토폴로지 배열 할당까지 제거한 것은 아니며 실제 성능은 측정하지 않았다.

## CP5 — 완료와 최초 피버 안내

두 항목 모두2/2 → 두 공급 중지·고스트/공격/잔상/예약 작업 정리 → 기존 **1.1초** 초기화(링·게이지 회수 → 회로 걷기 → 링 회전/회로 복원) → 점수0/배율1/정상HP/빈게이지/정식시계0 본게임을 유지한다. 실습 시간은 공식 기록에 포함하지 않는다. 정상 초기화 종료만 기존 tutorial 완료를 저장하고, 개발 다시보기는 기존 완료/BEST를 지우지 않는다.

최초 피버 안내는 기존 불투명 흰 화면/중앙 검정 두 줄을 유지한다.

```text
- FEVER TIME -
검은 노드를 마구 파괴하세요!
```

안내 .10/.80/.32/.24초, 최소 시간배율.06, 실제 피버8초/공급은 안내 중 보존한다. 정상 완료 키와 다시보기 flag는 구분한다. 테스트에서는 실제2+2 완료 뒤 실제 원형8개 접촉으로 피버에 들어가고 **일시적인 ReplayFeverNotice flag**로 안내를 확인했다. 이는 실제 신규 사용자 저장 후 앱 재실행 검증을 대신하지 않는다. 다음 피버 안내 없음은 기존 모델의 검수용 충전 호출로 확인했다.

## 조절 위치와 초기값

Unity Project에서 `Assets/Resources/CursorDash/VisualPrototype.asset` 선택 → **Ghost tutorial**. 새 설정 패널은 만들지 않았다.

| 필드 | 현재값 | 의미 |
|---|---:|---|
| tutorialGhostSupplySeconds | 2.1초 | 시범 공급 간격; 실제 노드 타이밍과 별개 |
| tutorialGhostLimit | 2 | 시범 동시 상한 |
| tutorialPracticeSupplySeconds | 1.25초 | 실습 독립 공급 간격 |
| tutorialPracticeLimit | 3 | 실습 동시 상한 |
| tutorialNodeGap | .28 world | 시범/실습 노드 사이 최소 추가 간격 |
| tutorialInk | (.76,.79,.82,1) | 밝은 회색 잉크 |
| tutorialSketch | .18 | 고정된 약한 잉크 변화 |
| tutorialResetSeconds | 1.1초, 유지 | 완료 뒤 정리·초기화 |

옛 tutorialRespawnSeconds .65/tutorialDemoGap .75/tutorialGhostScale .88은 직렬화 호환용으로 숨겨 보존하며 동작을 제어하지 않는다. 실제 노드의 준비/회전/변형·발사·반사 수치는 변경하지 않는다.

## CP6 — 실제 검사와 한계

| 체크포인트 | 완료·남은 검수 |
|---|---|
| CP0 원인 경로 | PASS: 반환 중 조기회수 원인 및 실제 반사→처치 집계 교정. 제공 영상 관람은 BLOCKED |
| CP1 실제 객체/소유권 | PASS: 같은 실제 풀/커서/잔상 사용, 교차 접촉·다른 소속 잔상 반사 무효, 원본 재질 복원 |
| CP2 독립 자동 플레이 | PASS: 무조작 실제 물리로 원형≥2·육각≥2·반사≥2, 플레이어 진행0. 가르치기 좋은 속도/시선 유도는 사용자 시각 검수 대기 |
| CP3 공급·집계 | PASS: 실제2+2·중복/실패 구분·Retry0. 성공 순서 조합/간격·공간부족 수학 검사는 PASS; 모든 화면비/무작위 배치 손 조작은 NOT_RUN |
| CP4 회색/스케치 | PASS: DX11 실제 프레임의 회색·실제색·동일 크기 계열 확인, 고정 잉크·풀 복원. 질감 강도 사용자 승인·모바일/WebGL 비용은 NOT_RUN |
| CP5 완료/최초 피버 | PASS: 실제2+2→정상초기화→실접촉득점→안내/시간보존·다음피버 무안내. 신규 사용자 앱 재실행 저장 지속성 NOT_RUN |
| CP6 회귀 | PASS: 최종14개 정상 TestRunner·최종참조컴파일·순수/JS. 전체 게임 테스트·실손/모바일/성능은 NOT_RUN |

정상 Unity TestRunner API의 기존 EditMode 경로에서 EnterPlayMode/ExitPlayMode를 사용한다. 일괄 SaveAssets나 사용자 씬 저장·폐기를 하지 않는다. 기존 Test Runner 보호 fixture가 완료 키·설정·프리셋·BEST를 보관/복원한다. 검수 후 6개 문자열 저장키 해시와 imported dirty6개가 전후 동일, Cyberpunk clean/PlayOFF/timeScale1이다.

| 검사 | 결과·범위 |
|---|---|
| 최종 참조 컴파일 | PASS, runtime117/test16/Editor2 오류0·기존134경고. Native 실행과 별도 |
| 안내/집계·회수/간격/공급/시계 순수 생산 C# | PASS 3431단언, 실제 Physics/GPU 검사와 별도 |
| 기존 회로·전환/점수·Nexus 순수 회귀 | PASS 4,672,999 + 106,885단언 |
| 기존 JS 구문/모의 DOM·메모리 저장 | PASS 19단언, 실제 브라우저 저장은 아님 |
| NativeRun1 | FAIL 10PASS/3FAIL. MonoBehaviour 필드 초기화의 MaterialPropertyBlock 생성 및 테스트의 실제 커서 조회 문제를 수정 |
| NativeRun2 | FAIL 11PASS/2FAIL. 실제2+2/본게임/안내는 통과했으나 teardown에서 파괴된 안내 오브젝트 접근. Unity null 검사를 보강 |
| NativeRun3 | PASS 13/13, 2026-10-28 02:06:39 KST 완료. 실제 고스트 원형≥2/육각≥2/반사≥2, 실제 플레이어 실습2+2·첫 안내·8초 종료·다음 안내 없음, Pause/Home/풀 재질 복원 |
| NativeRun4 / 최종 | **PASS 14/14**, 2026-10-28 02:12:41 KST. 위13개 + 서로의 원형 접촉 무효/서로의 잔상으로 반사 불가/실제 공격 실패 회수/Retry 새 고스트·진행0. JSON/XML·최종Editor·저장키/dirty 전후를 교차 확인 |

실제 고스트는 자동 이동 → 실제 Rigidbody2D 접촉/잔상 → 반사/Impact를 사용했다. 플레이어 검사는 **내부 body 위치 및 RecordMovement를 공급해 실제 판정 경로**를 사용했으며 손·터치로 검수한 것이 아니다. 성공 조건을 강제로 올리거나 튜토리얼을 건너뛰어 완료 검증을 대체하지 않았다. 신규 자산 스타일은 실제 DX11 GameView 프레임에서 확인했지만 완전한 연속 시각승인이나 모바일/WebGL 비용 검증은 아니다.

`NativeFrames`의 ghost-reflected-1/2, player-reflected-1/2와 first-fever-notice PNG가 이번 GameView 캡처다. 첫 안내의 전체 흰색·검정 정확 두 줄, 회색/실제 색 구별, 체크리스트를 직접 열어 확인했다. PNG는 연속 녹화가 아니다. 제공 영상 관람·손 조작 전체 전투·실제 모바일/WebGL·장시간 성능·앱 재실행 후 최초 저장 지속성은 **NOT_RUN**이다.

## 이번 변경 파일

- 기존 D-061 파일 수정: CursorDashGhostDemonstration.cs, CursorDashGuideFlow.cs, CursorDashGuideView.cs, CursorDashPractice.Guide.cs, CursorDashGuideTests.cs.
- 기존 실행/소유권 연결: PlayerCharacterController.cs, PlayerCharacterAfterImage.cs, Node.cs, NormalNode.cs, ArrowNode.cs, NodeCreator.cs, StageManager.cs, CursorDashPractice.cs.
- 표시/풀/프로필: CircularBreakVfx.cs, ImpactFragments.cs, CursorDashVisualProfile.cs, VisualPrototype.asset, CircularArtwork.shader, NodeFlat.shader.
- 검수: CursorDashNexusReview.cs, CursorDashSceneRegression.cs, CursorDashTestReport.cs.
- 새 파일: CursorDashGuideStyle.cs + meta, GuideInk.shader + meta, CursorDashAutonomousGuideTests.cs + meta, 이 인계 문서.
- 기존 AGENTS/STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES와 D-061 인계 상단을 갱신한다. 전체 정확 목록·전후 해시는 같은 Logs의 최종 보존 결과 참조. 이전 D-061의 그 외 미커밋 변경을 이번 신규 수정으로 집계하지 않는다.

최종 보존 대조: 시작1819파일 중 **1789파일 동일·관련30파일 수정·삭제0**, 신규 **7파일**(위6소스/meta와 이 문서). `.unity/.prefab/.meta` 기존파일·ProjectSettings/Packages, 육각 타임라인/리듬 수학·FeverState/종료·점수/저장·회로/배경/런타임UI/웹bridge는 이번 기준과 동일하다. 새 자산 meta GUID 중복0, Git HEAD와 빈 index를 유지했다. 상세 `Logs/autonomous-guide-20261028/preservation-final.json`.

기존 **Tools → CursorDash → 넥서스 수동 검수** 창에 고스트 실제 원형/육각/반사/실패 수와 최근64개 fire→reflected→returned impact/credited 기록을 읽기 전용으로 추가했다. 실제 처치가 아닌 단순 회수와 반환 중 상태를 구분할 때 사용한다. 별도 진단 시스템·공개 설정창은 추가하지 않았다.

## 사용자 수동 검수 순서

1. `Assets/Scenes/Cyberpunk.unity`를 열고 Play. 이미 완료한 경우 **Tools → CursorDash → 고스트 튜토리얼 다시 보기 (기록 보존)**. 실제 기록을 지우지 않는다.
2. 조작하지 않고 회색 고스트의 원형 접근·접촉과 육각 예고·잔상·반사·처치를 몇 차례 관찰한다. 실제 체크리스트는0/2로 유지되어야 한다.
3. 실제 커서로 색 있는 원형과 육각을 원하는 순서로 각각2회 처치한다. 육각은 실제 잔상 반사 후 돌아온 탄이 노드를 부숴야 횟수가 오른다. 서로의 시범·실습에는 간섭하지 않아야 한다.
4. Pause/Resume, 실패 반복, Retry/Home→다시보기에서 공급·공격·색·잔상과 카운터를 확인한다. 작은 화면에서 체크리스트/노드 가독성과 안전 간격을 함께 본다.
5. 실제2+2 후 링/게이지 회수→회로/링 복원→정상0점·HP·배율1·빈게이지·고스트 없음 확인. 정식 플레이 첫 피버 안내와 Pause/복귀, 두 번째 피버 안내 없음 확인. 개발 다시보기와 최초 완료 저장 시험은 구분한다.

현재 사망 점수 클릭대기·공개 UI 숨김·일반 모드/피버 전투와 승인 파편을 유지한다. 일반 노드 겹침(튜토리얼 외), 새 Flash 그림/사이버 공간 재구축/새 패널은 계속 보류다. 이번 Unity 검수 조작 승인은 이 수정의 집중 검사만이며 이후 빌드/Git/게시 승인으로 확장하지 않는다.
