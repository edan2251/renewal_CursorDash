# D-033 — 노드 축소 퇴장과 육각 충전3박자 (2026-10-26)

구현/정적·순수 계산/참조컴파일을 마쳤으며 **실제 Unity·GPU·Physics/Play 시각 검수 대기**다. 자동 Play·빌드·커밋·push·배포·다음 단계는 실행하지 않았다. 이전 D-032 항목의 사용자 검수 완료나 Vector4 오류 수정의 실제 재확인을 추정하지 않는다.

## 사라지는 이유와 변경

의도된 기존 경로는 세 가지다. 피버 진입 시 일반 원형/육각과 레이저를 처치 보상 없이 정리하고 일반 공격을 중지한다. 피버의 검정 원은 현재 기본2.8게임초 수명이 있고, 피버가 끝나거나 취소돼도 정리한다. 동시 상한에 도달하면 신규 생성이 기다리며 기존 노드를 지워 공간을 만들지는 않는다.

- 검정 원: 기존2.8초 안에서 .12초 한 번 탄성 등장(70%→약103%→100%), 마지막.24초 동안 크기가 줄어0에 도달한다. 기존 마지막.14초 페이드도 유지한다. 보이는 원과 실제 CircleCollider2D가 같은 root를 사용하므로 함께 줄어들고, 남은 표시에 앞서 판정만 꺼지지 않는다. 퇴장 중에도 실제 작은 원에 맞으면 기존1회 처치/점수/파괴를 처리한다. 피버 종료도 남은 시간과 수명 중 짧은 쪽을 따라 미리 축소하며, 종료/취소·Retry/Home 같은 명시적 정리는 끝에서 수행한다. 내부 단계 건너뛰기 버튼은 시간을 강제로 넘기는 검수조작이므로 자연 퇴장 검수에는 사용하지 않는다.
- 일반 노드: 기존대로 피버 진입 순간 실제 노드·공격·레이저는 즉시 무해하게 풀로 복귀한다. 그때 보이던 몸체의 메시/재질·표시값만 별도 복사해 .18초 줄인다. 이 퇴장 표시는 공격/판정/점수 대상이 아니다. Collider·Node·Animator·입자·레이저 동작은 복제하지 않고, 승인된 모양을 다른 그림으로 바꾸지 않는다. 기존 NodeCreator가 짧은 표시와 자원 해제를 소유하며 새 전역 관리 시스템은 없다. 진입 시간을 임의로 아주 짧게 조절한 경우 흰 전면이 시작되기 전에 끝나도록 최대 진입시간의55%로 제한한다.
- 일시정지는 게임초 기준이라 축소도 멈춘다. 취소·정지·비활성화·게임오버·Retry/Home는 남은 표시를 정리한다. 원본/공유 메시나 재질을 파괴하지 않고 소유한 복사본만 해제한다. 퇴장 생성 실패 때도 finally에서 실제 위협 Reset을 보장하며 보상이나 피해를 발생시키지 않는다.

현재2.8초는 기존 시안 설정을 유지한 값이며 최종 수명 확정이라는 뜻은 아니다. 수명·발생 간격.12·상한14·노드당 점수·피버8초·최대×4·피격초기화는 변경하지 않았다.

## 육각 충전 — 시간표

기존 중앙 확대 게이지의 **표시값**을0→1/3→2/3→1로 세 번 나누며, 각 구간 안에서는 연속적으로 빠르게 차오른다. 갑자기 칸을 켜지 않는다. 각회 .18초 상승/.14초 한 번 탄성 후 정착하며 그 사이에는 채움량이 증가하지 않는다. 처음 두 번은 약간 넘었다가 돌아오고, 세 번째는 완충 직후 짧게 눌렸다가 더 강한 내부 밝기 반응과 함께 안정된다. 전체 몸체·외곽선/테두리가 커지거나 흔들리는 효과가 아니다. 그 색·형태·두 겹 역회전은 보존했다.

저장 프리팹 준비3.5초 기준:

| 박자 | 상승 | 탄성·안착 | 이후 |
|---|---|---|---|
| 1 | .11~.29초 /0→1/3 | .29~.43초 | 1/3을1.59초까지 유지 |
| 2 |1.59~1.77초 /1/3→2/3 |1.77~1.91초 |2/3을3.18초까지 유지 |
| 3 |3.18~3.36초 /2/3→1 |3.36~3.50초 /반응1.35배 |원래3.5초에 준비완료 |

기존 실제상태 전환은 여전히 `elapsed>=prepare`이며 표시가 먼저1이 되어도 발사하지 않는다. 기존3.5준비+.25변형+.025예비=설정합계3.775초 발사, 여기에.06비행=3.835초 도달은 **변경 전후 동일**하다. 이 값은 저장값으로 계산한 명목 시간이며 실제 Editor 프레임 측정이 아니다. 기존 프레임 경계와 elapsed reset 방식도 유지한다. 개발자 `hex.prepare`가 다른 값이면 그 길이 안에 박자를 재배분한다. 짧은 준비값은 상승/안착 각각 준비의14% 이내로 제한해 겹치거나 공격 시간을 늘리지 않는다.
변형·발사/반동/뒤→앞 에너지 배출/잔상 방어·반사/피해 코드와 수치는 그대로다. 시각적인 ‘또잉’이며 효과음은 추가하지 않았다. 중간 파괴·비활성화·재사용에서 기존 Reset/elapsed/Visible 경로를 사용하고 예약된 타이머/트윈을 추가하지 않았다.

## 조절 위치와 파일

- 기존 `Assets/Resources/CursorDash/VisualPrototype.asset`: Fever Node Appear Seconds=.12 / Retire Seconds=.24 / Threat Retire Seconds=.18. 기존 다른 값은 유지했다.
- 육각 `ArrowNode` Inspector의 **Three-beat fill**: Charge Beat Rise Seconds=.18 / Settle Seconds=.14 / Elasticity=.06. .06은 한 번에 차는1/3에 대한 탄성비율이고 세번째는1.35배다. 기존 Preparation/Warning/Attack/Pre Fire 값은 이번 조절 대상이 아니다.
- `NodeRhythmMath.cs`는 위 곡선만 계산하는 작은 순수 helper다. 원형·육각의 승인된 기본 등장 NodeEntranceMath는 수정하지 않았다.
- 구현: ArrowNode.cs/NodeFlatGraphic.cs/NodeFlat.shader의 충전표시, FeverBonusNode.cs/FeverBonusField.cs/NodeCreator.cs/CursorDashFever.cs의 축소·정리, 기존 VisualProfile/asset의3값 추가. 회귀는 CursorDashFeverPolishTests.cs에3개 추가했다. 씬·프리팹·기존.meta/설정·패키지는 저장하지 않았다.

## 실제 확인 범위

PASS: 정적41, 실제 NodeRhythmMath 소스의 격리.NET 곡선 검사(161,907단언), 참조컴파일 게임108/테스트12/Editor2 오류0. 준비의 여러 길이에서 세 번의 상승/멈춤·완료 시점, 단발 탄성/약3% 초과·단조 축소·만료 시0을 수치 확인했다. 신규 공식3개를 포함해85개를 작성·컴파일했지만 실행하지 않았다. 수치/컴파일 결과는 실제 렌더/물리 성공이 아니다.
NOT_RUN: Unity 실제 컴파일/셰이더·GPU/복사 메시·재질의 최종표시/Physics·이번Play·모바일/효과음·최고기록 저장. 자동 Play/빌드/git/배포도 하지 않았다. 지난 Vector4 수정·일반 파괴/원형돌진/육각공격/최종파괴·점수/시간/입력 소스는 보존하고 실제 회귀는 사용자에게 남긴다.
보존 기준/Before/순수검사/컴파일/이번 차이는 `Logs/node-rhythm-retire-20261026`이다. 기존 변경·미추적 파일을 포함한 현재 작업본에서 비교하며 Git HEAD로 되돌리지 않는다.

## 사용자 Unity 검수 순서

1. 직접 `Assets/Scenes/01.StageSelectScene.unity` → Play → 훈련(Cyberpunk). 검수창의 ‘만충’만 눌러 자연 진입하고, 피버 안에서 잠시 움직이지 않는다. 여러 검정 원이 개별적으로 가볍게 튕겨 나타나며 약2.8초 뒤 작은 크기로 줄어 사라지는지 본다. 공간이 생긴 뒤 공급이 계속되고 기존 노드가 생성용으로 교체되지 않아야 한다.
2. 마지막에 줄어드는 원을 실제로 접촉하거나 빠르게 통과한다. 보이는 작은 원과 판정이 맞고 점수/제거/파괴가 각1회인지, 이전 Vector4 예외가 없는지 본다. 피버 마지막 .24초도 자연스럽게 축소해야 한다. 단계 끝으로 강제 건너뛰지 않는다.
3. 일반 모드에서 검수창/기존 생성 버튼으로 원형·육각을 남긴 뒤 만충한다. 진입 때 공격·레이저가 즉시 멈추고 모양만 .18초 축소되는지, 정리 점수가 지급되지 않는지 본다. 이때의 축소 모양은 이미 정리된 위협의 퇴장 표시다.
4. 정상 모드 육각 하나를 방치한다. 약 .11/1.59/3.18초의 세 번 짧은 채움과 중간 정지가 읽히는지, 세번째 뒤3.5초에 기존 흡수/변형→예비→발사로 연결되는지 본다. 충전 도중과 세번째 직후 각각 Reset/파괴·재생성해 잔류 반응/지연 발사가 없는지도 확인한다. 기존 반환 레이저 파괴는 새로 튜닝하지 않았다.
5. 탄성/퇴장 중 일시정지·재개, 피버 취소, Retry/Home·체력0을 확인한다. 표시·판정·공급·시간/입력과 복사 표시가 남거나 보상/공격이 중복되지 않아야 한다. 결과 확인 후 다음 요청을 기다린다.

## 정확한 변경 파일

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NodeFlat.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/ArrowNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/FeverBonusNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeFlatGraphic.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverBonusField.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NodeCreator.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeverPolishTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/ENVIRONMENT.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/SOURCES.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeRhythmMath.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeRhythmMath.cs.meta`
- `docs/CursorDash/NODE_RHYTHM_EXIT_20261026.md`

로컬 Logs에는 Before/이번만의 패치/순수검사·참조컴파일 결과를 보존한다. 초기1,832파일 중1,815동일·허용17수정/3신규, 누락·예상외변경·백업불일치0. HEAD/main/스테이징없음은 그대로다.
