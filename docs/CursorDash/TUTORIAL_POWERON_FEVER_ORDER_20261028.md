# D-064 — 튜토리얼 고스트·전원 복귀와 흰 확산부터 첫 피버 안내

2026-10-28 / 로컬 구현·승인된 Unity 집중 검수. 빌드·커밋·push·배포 없음. 사용자 시각 검수 대기.

## 현재 결정과 보존

이번 사용자 본문과 팝업 답변이 D-063의 첫 피버 시간표·Nexus 수축 재등장을 대체한다. **가벼운 손그림 윤곽 진행**, **현재 고스트 입력 .68의75%인 .51**을 먼저 승인했으나, 종료 전 최종 지시로 **.70**으로 대체했다. 최종 기본값은 .70이다.

기준 main `8d26194dc2c7940145c4c394a5d3ad56eb15c9bc`/빈index/기존1827파일. 사본 `Backups/tutorial-poweron-fever-order-20261028`, 로그 `Cursor-Dash-master/Logs/tutorial-poweron-fever-order-20261028`. 기존 D-061~063 미커밋/미추적과 씬·프리팹·meta/설정/사용자 기록을 보존한다. 육각 타임라인·일반 전투/공급·입력/반사·승인 VFX/파편·피버8초·점수/배율/사망 점수 대기와 보류된 노드 겹침/새Flash그림/별도 재구축은 변경하지 않는다.

## 1. 고스트의 경로와 표시

- 기존 실제 Controller/AfterImage/풀 Node·실제 접촉·반사 경로를 유지한다. 고스트만 최대 아날로그 입력 .68→.51→최종.70이다. 실제 플레이어 speed·turn/잔상수명·공격 시점은 불변.
- 제자리 작은 원과 매 프레임 이동하는 타원 목표를 제거했다. 목표까지 한 번 이어지는 넓은 cubic 경로를 계획하고 실제 Controller에 연속 아날로그 입력을 준다. 육각은 기존 실제 발사 시각을 읽어 접근 후 한 번 가로지르는 잔상을 만든다. 공격 시계를 고스트에 맞추지 않는다.
- 원형은 실제 준비진행90% 부근에 도착하도록 안전한 접근점까지 이동한 뒤 실제 접촉한다. 초기화/강제삭제로 시범을 통과시키지 않는다. `PreparationProgress`는 Warning에서1, `RestColliderRadius/RestContactRadius`는 읽기 전용 진단·접근 기하이며 판정을 바꾸지 않는다.
- 양쪽 생성 검사에 Nexus 본체 반경+노드 footprint+추가여유 .22→.85world를 사용한다. 다른 노드/HUD/체크리스트/양쪽 머리/화면여백 검사와 안전자리 없으면 대기는 유지한다. 일반 게임 배치는 변경하지 않는다.
- 회색 (.76,.79,.82)→(.55,.58,.62), Sketch .18→.30. 기존 실제 도형 셰이더에 고정된 작은 윤곽 굴곡과 잉크 농도차만 넣었다. 시간/프레임 난수·새 이미지/아틀라스/공유 재질 색 변경은 없다. 일반 객체의 GuideSketch0 경로는 유지하며 풀 반환 시 기존 재질/블록을 복원한다. 본격 연필 텍스처 제작이 아니다.

## 2. 마지막 성공 → 파괴 여운 → 고스트 fade → 전원 복귀

마지막 실제2+2 집계는 `Practicing → Settling → Fading → Reset`으로 한 번만 진행한다. 즉시 새 공급/공격을 멈추되 실제 마지막 HitFeedback 파편을 지우지 않는다. 최소 .35초와 실제 활성 파편 종료를 모두 기다린 뒤 고스트를 .40초간 fade한다. 그 후 기존2.2초 회로/점수 인계를 실행한다. 뒤늦은 처치·풀 반환은 성공으로 재집계하지 않는다.

튜토리얼 동안 Nexus의 합성된 본체/장식/별도 최종 외곽선과 실제 게이지 표시만 고스트와 같은 회색이다. 기존 원본/HP·수위·충돌 경계는 바꾸지 않는다. ResetBody/ResetRing은 항상1이며 **기존 본체 1→.02→1 및 점선 동시 축소를 제거**했다. 기존 회로52%부터 재구축,57%에 기존800도/초 ring impulse,56~88%에 시범회색→정상색 power 복귀,68~98%에 점수0성장은 유지한다. 빈 게이지를 장식으로 되살리지 않는다.

## 3. 첫 피버 안내 — 흰색 최초 확산부터 문구 표시

| 단계 | 시간·상태 |
|---|---|
| 기존 만충·진입 | 기존 Closing .22초 + Entering1.05초/링흡수·응축·폭발·흰세계 확산 그대로 |
| 흰색 최초 확산 | **Entering 진행66%부터** Nexus/게이지/숫자숨김·정확안내/조작차단. 남은 기존 진입 표현은 계속 진행 |
| 회로 완성 대기 | Fever 도달 뒤 Gauge1/Elapsed0을 hold하고 실제 feverFronts가 각 경로 전체 길이에 도달한 렌더 상태를 확인. 비활성 회로는 완료 대기로 막지 않음 |
| 중앙 안내 | Nexus·게이지·배경숫자를 숨기고 정확한 두 줄,2초 유지. 조작·실제8초·노드공급 정지 |
| 문구 fade |2초 SmoothStep alpha1→0. 같은 시작부터 기존 .12초 간격/상한14로 검정 노드를 개별 공급. 실제 충돌·점수/수명/피버시간은 아직 hold |
| 문구 완전 소멸 | 기존 노드를 지우거나 다시 만들지 않고 보존. 조작과 실제8초 진행 복귀. Nexus/gauge는 .25초 짧은 회전·등장으로 연결 |

흰 화면을 새 패널로 다시 덮지 않고 기존 게임판의 흰 확산을 그대로 사용하므로 fade 중 노드가 보인다. 같은 Fever owner/Stage 시간 소유자를 재사용한다. 선행 생성 동안 appearanceAge만 진행하고 lifetime age는0을 유지한다. 공급 간격/상한/노드당 점수·8초는 불변이지만 **첫 안내 뒤에는 이미 배치된 노드가 있다는 초기 밀도 차이**가 생긴다. 사용자 요청에 따른 표시 선행 공급이며 새 보상 규칙이 아니다. 기존 두 번째 이후 피버에는 안내/선행공급을 추가하지 않는다.

Pause는 안내 시계를 멈추며 Retry/Home/Abort는 queued notice·preview·숨김·재등장 상태를 정리한다. 기존 Tools 다시보기/최초 완료 저장 의미를 유지한다. 최고점수·튜토리얼 완료·프리셋 초기화 없이 재현한다.

## 조절 위치

Unity Project → `Assets/Resources/CursorDash/VisualPrototype.asset` (기존 설정 에셋).

| 항목 | 이전 → 현재 | 의미 |
|---|---|---|
| tutorialGhostInput |.68→.51→최종.70| 종료 전 최신 사용자 지정 최대입력 |
| tutorialInk |(.76,.79,.82)→(.55,.58,.62)| 시범만 조금 어두운 회색 |
| tutorialSketch |.18→.30| 고정 미세 윤곽/잉크 차이 |
| tutorialNexusClearance |.22 하드코딩→.85| footprint 외 추가 생성여유 |
| tutorialFinishSettle |즉시Reset→최소.35초+실제파편종료| 마지막 성공 여운 |
| tutorialGhostFade |없음→.40초| 회로 인계 전 고스트 fade |
| tutorialResetSeconds |2.2→2.2| 회로·점수 전환 유지 |
| feverNoticeHold / FadeOut |3/2→2/2초| 공간 구축 후 안내 유지/소멸 |
| feverNoticeRecovery |1→0(호환용 숨김)| 이전 슬로모션 정책 폐기 |

고스트의 원형90% 접근/육각 예고 .35초·넓은 경로 구성은 `CursorDashGhostDemonstration`, 회로 완료 판단은 `CursorDashCircuits.FeverConstructionComplete`, 전원복귀56~88%/점수68~98%는 기존 순수 `CursorDashGuideFlow`, 안내 후 재등장 .25초는 `CursorDashFever`에 있다.

## 검사 결과

**PASS/최종NativeRun7: 18/18 (04:05:30 KST, 로직12·Play6), 참조117/test16/Editor2오류0·기존133경고, 순수4107+4672999+106885·JS모의19.** 정상TestRunner내부조작과실제Unity물리/Renderer를사용했다. 실제손·터치/모바일·WebGL·성능은미검증이다. 최종JSON/XML/Editor상태를교차확인했다. 고스트실제처치각2+/반사2+, 마지막원형충전 0.9473/입력0.70/거의정지입력프레임1/1210. 마지막성공파편대기/fade·정상크기전원복귀, 실습/고스트소유권·무피해피격/간격, 새첫피버회로완성·안내/선행공급·8초·다음안내없음/Pause·Retry·Home을확인했다.

중간5회는각17PASS/1FAIL이었다. 조기접촉의등장기하, 일부매우짧은접근과반환대기경로재계획은실제고스트코드를수정했다. 두무작위fixture간섭은해당검사배치를격리해수정했으며성공집계/피격/실반사/연속이동단언을삭제·완화하지않았다. 최종추가요청은 **Entering 흰색최초확산 프레임부터** 문구alpha1·본체alpha0·입력차단을검사하고 실제GameView `notice-first-white-expansion.png`를남겼다. 이전Run6의18PASS는.51/늦은Fever시점표시중간결과이며 이번Run7이최종이다.

PlayOFF/timeScale1/Cyberpunkclean/확인prefs6·imported dirty6동일, 저장폐기없음. 화면은 `NativeFrames`의실PNG이며연속영상은녹화하지않았다. 최신 `native-final-summary.json`, `native-final.json/xml`과중간 `native-run1~6`를구분한다.

## 사용자 Unity 검수 순서

1. `Assets/Scenes/Cyberpunk.unity` → 기존 가이드 다시보기. 점수숨김/고스트와동일회색Nexus/조금 어두운 회색·약한 손그림·먼 생성 위치를 확인한다.
2. 조작하지 않고 고스트 원형의 약90% 충전 후 접촉과 육각의 넓은 접근→한 번 가로지르기→실제반사를 반복 관찰한다. 느려진 동선의 미적 판단은 사용자 검수가 필요하다.
3. 실제 원형/육각 각2회 성공. 마지막 파괴 여운→고스트fade→회로재구축/점선가속·색전원복귀/점수0등장을 확인한다. Nexus 본체가 작아지지 않아야 한다.
4. 정상 첫 만충 또는 기존 Tools 첫피버안내다시보기: 먼저 폭발·흰세계→첫흰확산(Entering66%)부터Nexus/게이지대신중앙안내→회로완성뒤2초→문구2초fade/개별노드공급→조작·Nexus/게이지복귀→실제피버를 확인한다.
5. 안내 유지·fade 각각에서 Pause/재개·Retry·Home. 정상 두 번째 피버에 안내가 없고, 일반 공격/점수·기록/사망 대기가 유지되는지 확인한다.

실제 손/모바일·WebGL/장시간 성능·앱재시작 저장·최종 미적 승인은 별도 검수다. 자동 추가 구현·배포를 시작하지 않는다.

## 이번 변경 파일과 최종 보존

시작 1827파일 중 1794동일/33관련수정/삭제0, 새 인계문서1개. HEAD/index 동일. 씬·프리팹/meta·Packages/ProjectSettings/일반공격·육각타임라인·실제Controller/Trail·점수/저장·일반VFX/피버파편·시간/종료곡선·배경원본은 시작해시 그대로다. 전체 전후해시는 `Logs/tutorial-poweron-fever-order-20261028/preservation-final.json`.

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Resources/CursorDash/CircularArtwork.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/GuideInk.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NexusGameboard.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NodeFlat.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/FeverBonusNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/Node.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NormalNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashCircuits.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGhostDemonstration.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGuideFlow.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGuideView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashPractice.Guide.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverBonusField.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverGaugeView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverWorldTransition.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/HitFeedback.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NexusGameboardPresentation.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/StageManager.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAutonomousGuideTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashGuideTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSceneRegression.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashTestReport.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/ENVIRONMENT.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/SOURCES.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`
- `docs/CursorDash/TUTORIAL_POWERON_FEVER_ORDER_20261028.md` (신규)
