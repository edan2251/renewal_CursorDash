# 육각 4박·피버 종료·배경 반응 — D-039 (2026-10-26)

상태: **IMPLEMENTED_STATIC_AND_PURE_CHECKED / 수정 후 사용자 Unity 검수 대기**. 최신 사용자 요청의 여섯 항목만 로컬 구현했다. 이전 D-038 배포 권한을 재사용하지 않았다. 현재 공개 버전은 여전히 D-038이며 이번 소스는 빌드·게시되지 않았다.

## 기준·보존·실행 범위

- Git D:/renewal_CursorDash / main / 시작 HEAD 8c357b876f03e805e3a0f0544fbbf1bb00f4bb2d. 기존 추적 변경 없음, 기존 미추적 파일은 사용자 작업으로 유지했다. branch/HEAD/스테이징을 변경하지 않는다.
- Unity D:/renewal_CursorDash/Cursor-Dash-master. AGENTS, STATUS→MASTER_SPEC, RELEASE_20261026, DECISIONS/ENVIRONMENT/VALIDATION/SOURCES와 실제 연결 코드를 읽었다. 이 작업에서 Unity 실행·UI 조작·미저장 상태 저장/폐기·Play·WebGL 빌드·Git 커밋/push·배포를 하지 않았다. 메모리의 미저장 상태를 검사/저장했다는 뜻이 아니다.
- Before/시작 해시·Git 목록/이번 차이·검사 근거는 `Cursor-Dash-master/Logs/hex4-fever-exit-20261026`에 보관한다. 새 파일은 이 문서와 무시되는 로컬 진단 산출물뿐이다.
- **USER_APPROVED(사용자 승인): 현재 피버 파편의 크기·강도·분포와 커서 표현.** 관련 파괴/공급/머리 판정·궤적 코드는 그대로다. 일반 원형·육각 아트/등장/파괴·공격 경로/반사·HP/피해/점수·최고점수·피버8초·×4/피격초기화·넥서스 최종 파괴·바깥 테두리 정지도 유지한다.
- **보류:** 노드끼리 겹치는 배치 문제, 새 Flash Background 그림, 사이버 공간 재구축 연출. 공개 웹 상단 도구바/HUD 겹침과 이전 빌드 경고/BGM 상태도 이번에 수정하지 않았다.

## 육각 4박 기준과 시간

실제 `ArrowNode.prefab`의 첫 Rotator는 **60도 이동 0.4초 + 정지 0.4초**다. 반대 방향 두 테두리를 합산하거나 360도를 한 회로 세지 않는다. `hex.spin=1` 기준이다.

| 사건 | 이전 D-038 | 이번 D-039, 생성 후 게임시간 |
|---|---:|---:|
| 생성 | 0/3 | 1/3, 짧은 기존 탄성 |
| 첫 회전 끝 | 1/3, 0.40초 | 2/3, 0.40초 |
| 둘째 회전 끝 | 1/3 유지 | 완충, 1.20초 |
| 변형 시작 | 4.20초 | 1.75초, 셋째 이동 안에서 시작 |
| 셋째 회전 끝 | 2/3 | 변형 완료, 2.00초 |
| 완료된 공격형 유지 | 기존 0.025초 | 셋째 뒤 원래 정지 구간 2.00~2.40초 |
| 넷째 회전 시작 | 발사하지 않음 | **발사 2.40초** |
| 넥서스 외곽 도달 | 4.46초 | **2.46초**(비행0.06초 유지) |

모두 디스크 저장값과 실제 사용 수식으로 계산한 **명목값**이다. 이번에 Unity에서 실측하지 않았다. 기본 발사는 4.40→2.40초로2초 앞당겨진다. LateUpdate 샘플링/프레임·Pause·히트스톱으로 실제 벽시계 시점은 달라진다. 사용자 검수창의 `MorphStartedAt/WarningCompletedAt/FiredAt/BlockedAt/NexusReachedAt`으로 관찰한다.

충전은 생성1/3에서 시작하고 첫·둘째 이동 안에서 각각1/3씩 빠르게 차오른다. 각 완료 뒤 기존0.14초/탄성0.06 반응을 주고 정지 구간에서는 유지한다. 셋째 이동 끝에 변형이 완료되도록 `hex.morph` 상한0.25초를 먼저 배치한다. 기본 실제 변형도0.25초다(이전 좁은 슬롯의0.175초와 다름). 회전 이동이 이보다 짧으면 해당 이동 길이까지 압축한다. 추가 초 단위 발사 타이머는 없다.

실제 `Rotator` 소유 트윈의 `Elapsed(true)`를 Preparing/Warning 모두 읽는다. 회전 횟수는 셋째 끝부터 쉬는 동안3으로 같으므로 횟수만으로 발사를 판정하면 조기 발사가 된다. 이제 **3×(이동+정지)**인 넷째 시작까지 같은 시계로 기다린다. 발사 때 이 육각의 두 Rotator만 중지한다. 취소·비활성화·풀 재사용 시 상태 초기화는 유지한다. 첫 링 누락은 둘째 대체, 둘 다 없거나 정지하면 기존 `hex.prepare=3.5`가 전체 발사까지의 대체시간이다. `preFireDuration=.025`는 직렬화 이력으로 보존하며 현재 대기는 실제 링 정지 간격으로 결정한다. 별도 값을 더하지 않는다.

레이저 비행0.06초/원점부터 현재 끝까지 전체 경로 방어/반사0.06초는 그대로다. ‘4’는 발사 시점이지 잔상 접촉 시점이 아니다. 경로 중간 잔상이라면 명목상 약0.03초 뒤, 외곽 근처라면 최대0.06초 뒤 접촉한다. 프레임·빔 원점과 표시 발사구 차이도 있으므로 실제 방어 체감은 사용자 확인이 필요하다. 속도·판정시간·잔상 수명은 바꾸지 않았다. 정지 간격을0으로 지정하면 셋째 끝과 넷째 시작이 같은 시각이 되며 별도 대기를 만들지 않는다.

## 플레이어 회전

`player.turn`은 도/초이며 `FixedUpdate→AdvanceMotion→MoveTowardsAngle(회전속도×fixedDeltaTime)`에 적용된다. **540→1800**으로 변경했다. 이동속도5.75/가속60/입력 해제18/입력 기준/충돌은 그대로다. 고정 물리간격0.02초라면 이동 중 방향 변경의 상한은 한 틱36도, 180도 전환은 명목0.10초다. 정지 직후 즉시 방향을 잡는 기존 분기는 유지한다.

`PlayerCharacterController.DefaultTurnDegreesPerSecond` 상수를 실제 fallback과 도구 정의가 함께 사용한다. 미저장 씬 충돌을 피하도록 씬 파일을 수정하지 않았다. `Cyberpunk.unity`에 남은 옛 직렬화 필드 `turnDegreesPerSecond: 540`은 더 이상 바인딩되지 않으며 런타임 기본값으로 읽히지 않는다. 해당 Inspector 필드 대신 기존 개발자 도구 **player.turn**에서 조절한다. 사용자 프리셋/설정은 지우거나 덮어쓰지 않았다. 이전 프리셋에서540 등을 명시적으로 적용하면 그 오버라이드는 유지되므로 새 기본 검수 때1800인지 확인한다. JSON 키·스키마·저장 경로는 그대로다.

## 게이지0 → 중앙 회수 → 복귀

규칙 모델은 원래도8초를 모두 소모한 뒤 Gauge=0/Exiting으로 전환했다. 이를 변경하지 않았다. 반면 피버 광학은 마지막0.18초부터 먼저 약해지고 있었으므로, 남은 게이지가 양수인 동안에는 광학이 먼저 끝나지 않게 했다. 원호의 소모도 보간값이 아닌 실제 `Model.Gauge`를 직접 사용한다.

- 실제8초 종료: 보너스 판정·생성은 종료, 노드는 기존 경로로 정리된다. 이 시점에 게이지0이며 광학 타격 잔여도 차단한다.
- 기존 **Exiting 0.95초**의 처음0.057초(진행0~0.06): 게이지는 완전히 사라지고 흰 세계는 유지한다. 이 구간은 피버 플레이 시간의 연장이 아니다.
- 그 뒤 종료 진행0.90(0.855초)까지: `u=clamp((진행-.06)/.84)`에 대해 이동량 `0.18u+0.82u^2.2`로 회수한다. 초반부터 움직이되 후반일수록 중심 수렴이 빨라진다. 전체 종료시간을 줄이거나 알파만 없애지 않는다.
- 기존 넓은 곡면 에너지장을 실제 NexusPosition 중심으로 수축·회전시키고, 바깥→중심의 기존 곡선 에너지 조각도 같은 가속으로 이동시킨다. CPU WhiteAtViewport와 공통 셰이더/커서 마스크가 같은 회수장을 사용한다.
- 점선 `HpCentral`이 종료 진행0.56에서 펼쳐지기 시작할 때 기존 `FeverRingSpin.Begin()`을 한 번 호출한다. +800도/초의 유한한 반응을 지수 감쇠해 기존24도/초로 복귀한다(`nexus.spin` 배율 적용). 보이지 않을 때만 가속을 소비하는 문제를 피한다. 다른 링은 바꾸지 않는다.
- 0.95초 종료 완료: Normal/배율+1 한 번, ×4 상한 유지. 정상 완료는 회전 여운을 이어가고 Cancel/피격/중지/비활성화는 가속을 지운다. 기존 마스크·광학·효과 정리를 재사용한다.

Pause는 진행/회전/배경을 현 위치에서 멈추고 Resume 시 이어가는 것이 정상이다. Retry/Home/게임오버는 취소 경로이며 잔여 마스크·광학·고속 회전·추가 배율 보상이 없어야 한다. 소스/순수 검사와 달리 실제 전환 화면·GPU 검수는 아직 하지 않았다.

## 배경 조절값

기존 `Assets/Resources/CursorDash/VisualPrototype.asset`를 선택하면 조절한다. `CursorDashVisualProfile.cs`의 생성 기본값/리소스 누락 fallback도 동일하게 맞췄다. 전체 관리 패널이나 새 그림은 만들지 않았다.

| 항목 | 전 | 후 |
|---|---:|---:|
| backgroundSpeed | .16 | .22 |
| backgroundMix | .66 | .84 |
| backgroundBend | .22 | .30 |
| backgroundHitSpeed | 1.8 | 2.6 |
| backgroundHitRecovery | .26초 | .38초 |
| backgroundHitColor | 0(없음) | .65 |
| backgroundHitShift | 코드.045 | 프로필.075 |
| 이동 변위 상한 | .09 | .14 |
| circuitRestContrast | .72 | .62 |
| 셰이더 navy | (.013,.027,.085) | (.010,.021,.070) |
| teal | (.014,.170,.205) | (.012,.210,.260) |
| violet | (.130,.032,.205) | (.200,.035,.280) |

`LivingBackground.shader`의 영역 경계도 첫영역(-.85,1.20)→(-.55,.95), 둘째(-.65,1.15)→(-.45,.95)로 또렷하게 했다. 타격은 기존 연속된 색 위상을 유지하면서 색 영역의 굽힘·분포 자체를 밀어낸다. 새 무지개색이나 화면 전체 점멸은 없다. 보통 처치에너지.75/넥서스피격1 중 최댓값만 유지하고, 변위상한/지수복귀로 연속 타격이 무한 누적되지 않는다. 피버/전환 동안 일반 타격 반응은 차단하고 흰 세계를 유지한다. 배경 시계는 그 아래 계속 흘러 복귀할 때 위치가 재설정되지 않는다. 회로는 기존 원본 이미지를 쓰며 대비만 낮췄다. **모바일/GPU 가독성과 강화 정도는 사용자 검수 대기**다.

## 변경 파일

Unity 루트 기준 실제 수정20개:

- `Assets/Scripts/Gameplay/Object/Animator/Rotator.cs`: 같은 트윈 누적시간 읽기만 추가.
- `Assets/Scripts/Gameplay/Object/Node/NodeRhythmMath.cs`, `ArrowNode.cs`: 4박 충전/변형/발사 문턱과 실제 시계 연결.
- `Assets/Scripts/Gameplay/Object/Character/PlayerCharacterController.cs`, `Assets/Scripts/Gameplay/StageManager/CursorDashTuning.cs`: 회전속도 기본값 통합/도구 설명.
- `Assets/Scripts/Gameplay/StageManager/FeverFlowMath.cs`, `FeverWorldTransition.cs`, `FeverGaugeView.cs`, `FeverNexusRig.cs`: 완전소모/가속회수·마스크 일치/링 복귀.
- `Assets/Resources/CursorDash/FeverFlow.cginc`, `FeverWorld.shader`: 같은 종료곡선·곡면과 이동 표현.
- `Assets/Scripts/Gameplay/StageManager/CursorDashLivingBackground.cs`, `CursorDashVisualProfile.cs`, `Assets/Resources/CursorDash/LivingBackground.shader`, `VisualPrototype.asset`: 배경 대비/팔레트·국소 반응.
- `Assets/Editor/CursorDashNexusReview.cs`: 기존 수동 창의 박자 설명만 수정.
- `Assets/Tests/Editor/CursorDashFeverPolishTests.cs`, `CursorDashFeverRulesTests.cs`, `CursorDashSceneRegression.cs`, `CursorDashTestPolishTests.cs`: 새 타이밍 기대값/기본 회전·종료 회귀 단언.

문서: AGENTS, STATUS, MASTER_SPEC, DECISIONS, ENVIRONMENT, VALIDATION, SOURCES, TUNING_GUIDE 및 이 문서. 기존 씬·프리팹·meta·패키지/프로젝트 설정·기존 미추적 파일·실험 프리셋은 보존한다.

## 검사와 미검수

- **PASS** 정적21항목: 실제시계/별도 예약공격 없음·배경 상한·공유 종료장·기본값 적용 경로·승인 파편/커서·공급·규칙·씬/프리팹/meta·설정 보존, whitespace.
- **PASS** 실제 세 순수 소스 `NodeRhythmMath/FeverState/FeverFlowMath`를 .NET으로 컴파일/실행: **5,425단언**(박자468/규칙19/표현수학4,938). 여러 이동/정지값·30/60/120Hz 샘플링·완전소모/종료 중 Pause/Abort/Reset·상한×4·비선형 회수/뷰포트/잔여0·회전감쇠. 실제 DOTween/물리나 GPU 검증은 아니다.
- **PASS** 기존 설치 참조DLL 대상 C# 컴파일: 게임108/테스트12/수동Editor2 소스, **오류0**. 런타임 경고130은 이전 참조검사와 같은 개수다. 기존 Unity빌드 경고4와 다른 집계다.
- **PASS** 시작1,842파일 중1,814동일/허용28수정(코드·리소스·검증20/기존문서8)·신규문서1. 누락/범위밖변경/Before백업불일치0, main/HEAD8c357b8·스테이징없음. 기존 미추적 파일을 지우거나 옮기지 않았다.
- 공식 **93개**: 기존90의 박자 기대값을 수정하고 회전 기본값/게이지0 뒤 종료·취소/가속회수3개 추가. Play 보존·방어/피해·취소 단언을 삭제/무시하지 않았다. 방향 회전 Play 검사는1800에 맞춰 초기 굴절 관찰을0.08→0.04초로 앞당겼다. **실제 공식 실행 NOT_RUN**.
- **NOT_RUN** Unity 실행/실제 컴파일·Shader/GPU/Physics·Play/시각·실제 프레임 시간·모바일·음향·WebGL/웹/배포. 이번 수정이 실제 화면에서 해결됐다고 보고하지 않는다.

## 사용자 Unity 검수 순서

1. 사용자가 `Assets/Scenes/Cyberpunk.unity`를 열고 직접 Play→훈련. Console 컴파일/셰이더 오류가 없는지 먼저 확인한다. 기존 Dev 프리셋이 켜져 있다면 `hex.spin=1`, `hex.morph=.25`, `player.turn=1800`을 확인한다. 설정 초기화/프리셋 삭제는 필요 없다.
2. 새 육각 하나 생성: 생성1/3→첫60도 끝2/3→둘째 끝완충→셋째 끝삼각형 공격형완료→다음 움직임 시작박자4에발사. `Tools→CursorDash→넥서스 수동 검수`의 시작/완료/발사 기록도 비교한다. 빔 비행.06초가 남으므로 실제 잔상 접촉은4 직후다. 변형/멈춤 동안 취소·파괴, Pause/Resume, 재사용에 늦은 발사가 없는지 본다.
3. 피버 만충→유지: 승인 파편 크기·분포·커서/궤적·지속 공급이 그대로인지 본다. 마지막 원호가0까지 소진된 뒤에만 흰 세계 회수가 시작돼야 한다. 규칙상8초 후에는 노드 공급/득점 판정이 끝나야 한다.
4. 종료: 바깥 흑백 공간이 실제 넥서스 중심으로 초반부터 움직이고 후반 빨려들며 빠르게 수렴하는지, 점선이 나타날 때 휘리릭 돌고24도/초의 평상시로 감속하는지 본다. 빈 원호/중앙 배율/시작선/잔여 마스크·색수차가 남으면 안 된다. 배율은 종료 후 한 번만 상승한다.
5. 종료 초반·회수 중·링복귀 중 각각 Pause→Resume, Retry, Home, 체력0을 확인한다. Pause는 정지/재개, 취소·씬전환은 잔여 효과 제거/배율 미보상/시간·입력 복구가 기준이다.
6. 일반 화면20초 관찰→일반 원형/육각 처치·넥서스피격 단발/연속: 큰 남색/청록/보라 영역이 보이고 타격 때 영역이 밀리며 색조가 반응한 뒤 돌아오는지, 노드·예고·커서를 가리지 않는지 확인한다. 피버 중 컬러 누출/복귀 순간 배경 점프가 없어야 한다. 실휴대폰은 별도 검수다.

다음 한 작업은 위 사용자 Unity/영상 검수다. 검수 전에 다른 기능·배치 보류·빌드/배포로 진행하지 않는다.
