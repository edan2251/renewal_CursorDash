# D-032 — 피버 정합·지속 공급·파괴·응축 보완 (2026-10-26)

## 후속 수정 — 첫 피버 타격의 Vector4 예외 (2026-10-26)

사용자는 다른 항목을 검수하지 못했고 첫 검정 원 접촉의 실제 예외를 보고했다. 아래 최초 구현은 최종 승인된 상태가 아니다.
원인은 제가 추가한12각형 shell이 기존 Vector4 거리4개에12변을 기록하던 코드다. 컴파일 검사/순수상태·분포 검사는 이 메시 생성 경로를 실행하지 않아 이전 보고에서 놓쳤다. 큰 볼록 도형을 중심 fan으로 나누고 각 삼각형에는 실제 외곽 변1개만 기록한다. 내부 방사선에는 AA를 적용하지 않아 조각 간 빈 줄이 생기지 않도록 했다. 12개 변을4개로 잘라 없애거나 공유shader를 변경하지 않았다. 기존3/4변 파편의 좌표·coverage·indices는 이전과 동일하다.
`FeverBonusNode.TryCollect`는 예외에도 finally ResetObject, `FeverBonusField.Collect`는 등록 전 초기화 실패 오브젝트를 정리하고 rethrow한다. 보상은 재시도하지 않고 예외도 숨기지 않는다. 점수·강도2.5·18조각·.48수명/공급 .12·14·간격·모든 다른 시각값은 그대로다.
변경 게임 파일은 FeverBonusBurst.cs/FeverBonusNode.cs/FeverBonusField.cs, 회귀는 CursorDashFeverPolishTests.cs다. Before는 `Logs/fever-burst-vector-fix-20261026/Before`, 이전/현재 함수 본문·실제 Unity Vector의 관리형 재현 결과는 `Geometry/result.json`에 있다. 이전은 같은 IndexOutOfRangeException 재현/현재는640경우96,001단언 PASS. 게임107/테스트12/Editor2 참조컴파일 오류0. 공식82는 작성·컴파일만, Unity·실제 Mesh/Material·GPU/Physics/이번Play·빌드·커밋/push/배포는 NOT_RUN이다.

**재검수는 우선 이 오류부터**: 기존 Play를 직접 종료→컴파일 완료 확인→새 훈련→검수창 만충→자연 피버 진입→검정 원 하나에 느리게 접촉→다른 원 빠르게 통과→여러 원 연속 접촉. Console에 같은 예외가 없고 매 적중의 점수/제거/파편이1회씩 나오는지 확인한다. 이미 실패한 이전 런타임 객체를 그대로 정상으로 간주하지 않는다. 통과한 뒤 아래의 다른 D-032 검수를 이어가며, 자동 다음 단계는 시작하지 않는다.

상태는 **수정 후 사용자 Unity/GPU·영상 검수 대기**다. 정적·순수 계산·참조컴파일을 통과했지만 실제 Play/최종 화면/Physics 성공을 주장하지 않는다. 자동 Play·Unity UI·씬 저장·빌드·커밋·push·배포·다음 단계는 실행하지 않았다.

## 자료와 실제 관찰

요청한 `D:/renewal_CursorDash_incoming/references`는 없고 동일 파일들은 작업 루트의 `_incoming/references`에 있다. `40_current_fever_review.mp4`(34.299967초,668×1184)는 현재 문제 기록이고 목표가 아니다. 0~14초/2초,19~26초/1초 표본을 실제 디코드했다. 19~21초 머리와 검정 궤적·파편의 상하 분리/곡선 띠,23~26초 점수32 옆의 깨진 배율을 확인했다. `41_issue_cursor_hit_alignment.png`의 손그림은 주석이며 디자인으로 사용하지 않았다.
이미지에서 머리 y약845,넥서스 중심y약564,분리된 꼬리 끝y약281 관계는 `2×564−845≈283`에 가깝다. 렌더 마스크의 상하 반전과 맞는 근거이며, 개별 파편의 물리 위치가 틀렸다고 확정한 것은 아니다.
기존06/07 참고는 표본을 다시 확인했다. 모임/급격한 반응/여운의 리듬만 참고하며 색·모양을 복제하지 않는다. 07의8.75초 표시는 끝 프레임으로 제한된 중복이므로 근거에서 제외했다. 오디오는 확인하지 않았다. 별도 배경 참고 영상은 없다는 정정을 유지한다.21타격감 영상은 기존 분석 이력만 유지하며 이번에 재생했다고 보고하지 않는다.
수정 전 프레임 근거는 `Logs/fever-stream-alignment-20261026/current40-0-14.png`, `current40-19-26.png`다. **이번 수정 후 화면 자료가 아니다.**

## 원인과 변경

| 구간 | 확인한 구조·위험 | 이번 최소 수정 / 아직 남은 확인 |
|---|---|---|
| 머리/꼬리/노드·파편 | 실제 머리는 CursorForeground의 별도 Camera.Render, 검정 원/리본/파편은 FeverElementMask CommandBuffer. 후자만 미리 GPU 변환한 투영을 다시 전달. 녹화의 상하 대칭 분리와 부합 | 공식 API 예시처럼 논리 camera.projectionMatrix 전달. 머리·물리 좌표 이동 없음. GPU에서 수정 후 일치하는지는 사용자 검수 |
| 머리 경로 시점 | FixedUpdate에서 MovePosition 예약 직후 미래 위치로 보상, 화면 머리는 보간된 현재 위치. 큰 상하 반전과는 별개인 소규모 시차 위험 | 피버만 LateUpdate에서 이전 실제 머리→현재 실제 머리 Box 경로 검사. 긴 꼬리 전체는 사용하지 않음. Trigger와 removed/실행ID를 공유 |
| 효과 첫 위치 | 기존에도 노드 원점/접촉점을 보상 전에 저장. 파편은 직선 이동이나 렌더 반전과 상관된 분포가 띠 인상을 만들 수 있음 | 저장된 실제 노드 위치/접촉 유지. 진단에서 원점·접촉점 표시. 커서가 이동한 뒤 남은 파편은 정상이며 머리에 붙이지 않음 |
| 글자 | 배율에 점수의 cloned material을 강제로 공유. 초기화·장면 글꼴 스캔·재활성화 시 글꼴/아틀라스와 재질이 엇갈릴 수 있는 연결 | 점수/배율을 Bold로 명시하고 각 font.material에서 별도 재질 생성/복원·재연결. 글자 간격0/자동크기 해제. safe area·자릿수폭/60%크기/1회 펄스 유지. 이전 런타임의 실제 재질을 수집하지 못해 이것이 유일 원인이라고 확정하지 않음 |
| 공급 | 진입18개/.85초마다6개, 그룹 배치·크기 차이·공간 부족 때 best 후보 강제 생성 | 개별 시간 공급, 균일 크기·정지 원, 상한/최소간격/머리·넥서스·실제HUD/가장자리 배제. 후보96회 모두 부적합이면 기다림. 살아있는 노드를 지우지 않음 |
| 파편 | 같은 황금비 수열의 상보값으로 방향·크기·속도·감속이 연동됨 | 각 채널이 독립된 결정적 hash. 서로 다른 형상·크기·회전·속도·감속의 18조각, 전진+좌우 폭발. 궤도/공전 없음. .014초 압축 뒤 강하게 이동하고 감속 |
| 흐릿한 큰 고리 | 별도 확장 회색 원/원형 왜곡이 실제 접촉 위치와 검정 실루엣 위치를 분리 | 큰 회색 확장 원 제거, 작은 정확한 접촉 반응으로 교체. 남긴 작은 국소 왜곡은 게임판/실루엣/실제 머리에 같은 계산·좌표를 적용(상한 .002/짧은 축 기준UV) |
| 응축 | 기존7%의 사인 압축이 이미 되풀린 뒤 광원이 기다리는 느낌. 시각 본체 수축 시 가림 반경도 함께 줄어듦 | 링이 도착하는 동안 최대11.5% 수축+밝기 집중, .61→.67에 빠른 해제. 원래 본체 반경으로 흡수선을 가려 낮은HP·수축 중에도 재노출 방지. 실제HP/경계 불변 |
| 배경 | 느린 색 흐름/강한 회로, 일반 타격 신호가 색 흐름에는 연결되지 않음 | 같은 남색/청록/보라 큰 경계 대비 증가, 회로 기본 대비 .72. 기존 타격 신호에 제한 가속·짧은 밀림/감쇠를 연결. 색상 점프/주기 점멸 없음. 기존 플래시의 순간 증가항·공통흔들림 수치 불변 |

커서의 씬 저장 기준은 root scale.5/Box .75×.75(offset0), CursorVisual local0/scale.5다. 따라서 몸체 Box의 월드 폭은 .375, 기본126px/100PPU 이미지의 사각 바운드는 .315다. 기존 판정의 여유·투명 모서리를 새 규칙으로 바꾸지 않았다. 실제 런타임 개발자 값이 다를 수 있다. 초록 진단은 이 **기존 실제 Box**이며 픽셀 단위 삼각형 판정이라는 뜻이 아니다. 머리·꼬리는 같은 이동 root를 사용하고 native TrailRenderer의 수명/방어 segment는 그대로다. 물리 보간/렌더 타이밍의 최종 일치는 Play로 확인해야 한다.

## 공급량과 수치 — 초기 시안, 고정 밸런스 아님

노드당 기본점수1×현재배율, 피버8초/종료배율1→2→3→4→4/피격1/보너스 비충전·일반 난이도 정지는 그대로다. 새로운 공급은 처치·이동을 조건으로 삼지 않는다. 최초 진입 다음 실제 갱신부터 한 개씩 등장한다. 상한에서 사라지는 것은 생성용 교체가 아니라 기존2.8초 수명 끝의 .14초 시각 페이드와 판정의 동시 종료다.

새 기본 .12초는 초당약8.33개의 공급 기회다. 공간 거절/상한이 없고 바로 수집한다는 순수 시계 모델은8초67개, 이전18+9×6=72보다약6.9% 적다. 무수집/배치성공 모델은8초42개/동시최대14(2.8초 만료 후 공급 재개)다. 프레임 정체·좁은 화면·배치 거절은 실제 공급을 더 낮춘다. 실제 처치수/점수 보장이 아니며 점수제나 시간을 같이 보정하지 않았다. 오래 멈췄다가 보상 노드가 한꺼번에 밀려 나오지 않도록 프레임당 최대1개/상한의 시간 부채를 쌓지 않는다.

| 조절 항목 | 위치 | 초기값·단위 |
|---|---|---|
| 개별 생성 간격 | Resources/CursorDash/VisualPrototype.asset → Fever Spawn Interval | .12 게임초 |
| 동시 존재 상한 | 같은 asset → Fever Concurrent Limit |14(기존 pool36 이내) |
| 노드 외곽 간격 | 같은 asset → Fever Node Gap |.22 월드 |
| 현재 머리 추가 여백 | 같은 asset → Fever Cursor Clearance |.28 월드+실제 Box 외접범위 |
| 가장자리 | 같은 asset → Fever Side/Top/Bottom Margin |.12/.18/.16 viewport, 원 반경을 추가 제외 |
| 기본 원 반경/대상 수명 | Stage의 CursorDashFever.Settings → bonusRadius/bonusLife | 기존.205/2.8초 |
| 파괴 강도/파편 수명 | VisualPrototype → Fever Break Strength / Fragment Life | 기존2.5/.48초(크기범위·18개 유지) |
| 연속 잔류 | FeverBonusBurst.SettleResidue | 이전 파편 목표알파×.82, 하한.42; .08초 보간/첫.09초 강도 보존. 파편 전체 즉시 삭제 없음 |
| 응축 | VisualPrototype → Fever Gather Compression |.115(시각만11.5%) |
| 응축 시간 연결 | FeverFlowMath.GatherEnvelope | 진입진행.10~.52 집중/.52~.61 유지/.61~.67 해제. 총 진입1.05초 그대로, 해제약.063초 |
| 배경 기본 속도/혼합/굽음 | VisualPrototype → Background Speed/Mix/Bend | 기존.16/.66/.22 |
| 타격 흐름 가속·회복 | 같은 asset → Background Hit Speed/Recovery |1.8계수/.26초 지수 감쇠. 일반처치 에너지.75·피격1, 가산누적아님 |
| 회로 기본 대비 | 같은 asset → Circuit Rest Contrast |.72, 원본 Image 색/알파·원본 파일 불변 |

VisualPrototype은 기존 프로젝트 내 설정 한 개를 확장한 것으로 새 전역 관리 시스템/패널을 추가하지 않았다. 이전 Settings의 bonusInitial/bonusPerWave/bonusWaveSeconds는 과거 묶음 공급 필드로, 지금 공급에는 사용하지 않는다. 일반 노드 등장 .20초 및 모든 승인 전투값은 그대로다.

## 검사·보존

- PASS 정적63항목: 기존 규칙·노드/공통VFX/기록·씬·프리팹·meta/패키지/설정 보존, 범위와 연결 확인.
- PASS 실제 순수 소스.NET34,512단언: 기존 점수/배율·접촉 계산, 개별 공급/상한·정지·긴프레임·무처치 재개, 독립 분포와 응축 곡선. 순수한 공급량 모델은 기하/Physics/실제 화면 검증이 아니다.
- PASS 참조컴파일: 게임107·공식테스트소스12·Editor2 오류0. Unity를 실행하지 않는 격리 C# 검사다.
- 공식81개는 기존76+추가5(공급 비의존/상한·무수집/분포/응축/실제 HUD 아틀라스·Pause)를 작성·컴파일했으며 실행은 NOT_RUN. 기존 테스트 하나는 새 개별 공급을 .32초 기다리도록 수정하고 보상/제거/중복 방지 단언은 유지했다. 자동으로 테스트를 실행하지 않았다.
- NOT_RUN: 실제 Unity 컴파일·셰이더·GPU/Physics·이번 Play·최종화면·음향/모바일/실제 기록 저장. 정적 통과를 시각 승인으로 취급하지 않는다.
- 사용자의 기존 미저장 상태를 저장하거나 폐기하지 않았다. 원본/기존 미추적 파일·TMP 예외 D-014 유지. main/HEAD8ceedbb 유지, 스테이징 없음. 이전 사용자 변경을 포함한 시작1,831파일을 기준으로 이번 수정만 Before/only-D032.patch와 비교한다. Logs는 Git 제외된 로컬 근거다.
- 새 Flash Background 그림과 불규칙 회로 재구축 전환은 **다음 후보만 기록, 구현/이미지생성/리소스요청 없음**이다.

## 사용자 Unity 검수 순서

1. `Assets/Scenes/01.StageSelectScene.unity`를 열고 직접 Play → 훈련(Cyberpunk). 기존 원형·육각의 등장/공격·반사/일반 파괴가 유지되는지 먼저 확인한다. 넥서스 가장 바깥 테두리의 정지는 유지한다.
2. `Tools → CursorDash → 넥서스 수동 검수`에서 ‘만충 → 원호 흡수 준비’를 한 번 누른 뒤 자연스럽게 진입하도록 기다린다(내부 게이지 조작이며 실제 처치 판정 시험이 아님). 낮은HP도15% 버튼으로 먼저 만든 뒤 시험한다. 바깥 링→점선→본체 압축·빛 집중→빠른 폭발/흰 전체 화면, 내부 잔선 없음/HUD 가독성을 본다.
3. 피버에서 가만히 있어 본다. 노드가 하나씩 계속 나타나고 여러 개가 함께 있으며, 머리·넥서스·상단HUD 위 생성이 없어야 한다. 14개에 도달하면 기다리고 수명 만료/실제 처치로 자리가 나면 한 개씩 재개해야 한다. 생성용으로 살아있는 원을 갑자기 지우면 안 된다.
4. 느린 접촉→원 사이 빠른 통과→급회전을 각각 한다. 실제 보이는 원에서 점수1회·제거1회·폭발1회여야 한다. 오래된 꼬리만 스친 원은 수집하지 않아야 한다. 머리 뒤로 검정 궤적이 붙고 뒤의 오래된 부분부터 사라지는지 본다.
5. 필요할 때만 같은 검수창의 ‘접촉 위치 진단(기본 꺼짐)’을 켠다. 초록=실제머리Box/자홍=살아있는원Circle/빨강=최근 접촉점/청록=파괴 노드·VFX원점. Pause 상태에서도 위치를 볼 수 있다. 새 파편의 첫 위치가 이 점 부근인지 확인하며, 과거 타격점에 남은 파편을 새 오류로 혼동하지 않는다. 진단은 기본 꺼짐/약.45게임초 최근점만/파일저장 없음이다.
6. 여러 번 연속으로 타격한다. 크고 또렷한 검정 파편이 전진·양옆으로 불규칙하게 터져 감속하고, 뒤늦게 곡선 고리로 줄서거나 화면을 장시간 덮지 않아야 한다. 강도2.5 유지/커서 조작·타겟 가독성을 본다. 일반 원형·육각 VFX는 새로 튜닝하지 않았다.
7. 피버를 자연 종료시킨다. 컬러 흐름이 튀지 않고 복귀/링·궤적·HUD 색 정상/점수 옆 작은 ×2가 읽혀야 한다. 이후 반복으로 ×3/×4/×4 유지, 피격 시×1숨김을 본다. 긴 점수·다른 화면비도 확인한다. 글자가 여전히 깨지면 검수창 ‘실제 글꼴·텍스처’가 점수/배율 모두 ‘일치’인지와 영상을 함께 남긴다. 둘 다 일치해도 정상 화면을 보장하지 않는다.
8. 피버/폭발 도중 Pause·Resume, Retry·Home, 훈련무적 해제 후HP0을 각각 확인한다. 생성·파편·링·배경·색·시간·입력의 잔류/예약 공격/중복 점수·배율 상승이 없어야 한다. 정상 최고점수 저장 시험은 훈련과 구분한다(훈련은 기록 제외).

최종 화면 확인/승인은 사용자의 새 영상으로 진행한다. 이번 보고서의 검사 개수로 완료를 판정하지 않으며 결과 전 D단계나 새 기능으로 넘어가지 않는다.

## 이번 변경 파일

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashNexusReview.cs`
- `Cursor-Dash-master/Assets/Resources/CursorDash/CursorForeground.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverFlow.cginc`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverWorld.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/LivingBackground.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NexusGameboard.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/OriginalBackgroundFlash.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Character/PlayerCharacterController.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/FeverBonusNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashLivingBackground.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverBonusBurst.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverBonusField.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverElementMask.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverFlowMath.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverGaugeView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverWorldTransition.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NexusGameboardPresentation.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/OriginalBackgroundFlash.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeverCombatTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeverPolishTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/ENVIRONMENT.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/SOURCES.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`
- 새 문서 `docs/CursorDash/FEVER_STREAM_ALIGNMENT_20261026.md`
- 로컬 근거/보존 `Cursor-Dash-master/Logs/fever-stream-alignment-20261026/` (공개 배포/커밋 없음)
