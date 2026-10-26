# D-046 승인 화면 보존·동적 배경 회로 (2026-10-27)

**현재 D-049:** [게이지 회로 후퇴·점수 획 가림](CIRCUIT_GAUGE_RETREAT_GLYPH_MASK_20261027.md)이 아래 회로 중앙 흡입/옛 폭·대비를 대체한다. 원본/구도·구축/복원·사망 퇴장과 게임 규칙은 유지한다.

**최신 후속 D-048:** 현재 접합/끝/개별 회수·평상시 흐름·시험값은 [CIRCUIT_JOINTS_INTAKE_FLOW_20261027.md](CIRCUIT_JOINTS_INTAKE_FLOW_20261027.md)를 따른다. 아래 D-046/047의 큰 접점/전체 축소는 구현 이력이다.

**후속 D-047:** 원본 기반 형태·굵기·밀도와 현재 값은 [CIRCUIT_ORIGINAL_LAYOUT_20261027.md](CIRCUIT_ORIGINAL_LAYOUT_20261027.md)를 따른다. 아래8묶음/40선·가는 폭은 초기 구현 이력이며 최신 값이 아니다. CS0246의 테스트 런타임 조회 수정은 유지한다.

상태: **IMPLEMENTED / STATIC_AND_PURE_CHECKED / 수정 후 사용자 Unity 검수 대기**.

## 최신 후속: 사용자 CS0246 차단 수정 (2026-10-27)

사용자 Unity에서 CircuitTests.cs:5의 CircuitPathGraph 타입/네임스페이스 오류로 Play가 차단됐다. 새 테스트가 Assembly-CSharp 게임 타입을 직접 참조했으나 기존 CursorDash.EditorTests.asmdef는 게임 직접 참조가 없는 별도 어셈블리다. 앞선 검사에서 테스트 참조에 임시 게임 DLL을 추가해 실제 구성의 오류를 놓친 점을 정정한다. 아래 과거 ‘테스트 참조 컴파일 통과’를 실제 Unity 테스트 구성의 통과로 해석하지 않는다.

기존 테스트와 동일한 런타임 조회 방식으로 새 테스트1개만 수정했다. 게임 타입 별칭/직접 new/게임 ScriptableObject 제네릭 참조를 제거하고 실제 로드된 타입의 생성자·필드/속성·메서드를 호출한다. 실제 생산 코드의 그래프/펄스/기본값을 검사하는8사례와32단언은 그대로다. 테스트 삭제/Ignore·기대값 변경이나 게임 코드/asmdef 재구성으로 우회하지 않았다.

Unity가 이미 생성한 CursorDash.EditorTests.csproj의271참조(게임 DLL 참조0)로 **이전 파일 동일 CS0246 재현 → 수정 후 테스트13소스 오류0/경고0**을 확인했다. 프로젝트 생성 파일/asmdef는 읽기만 했다. 전체 Unity 테스트 실행/Editor 재컴파일 완료/Play·시각 확인은 NOT_RUN이다. 사용자 Console에서 오류가 사라졌는지 확인한 뒤7절의 기존 회로 검수를 이어간다. 새 기능·회로 수치 수정/빌드·Git 게시/저장·폐기는 하지 않았다. 근거/Before: Logs/circuit-test-assembly-fix-20261027.

## 1. 기준과 권한·관찰한 범위

기준 커밋은 main **1d11743d9417597d009c1f92aecb430eb5180b37**이며 해당 커밋의47파일/체크포인트 기록을 확인했다. 이후 기존 사용자 Cyberpunk.unity의 옛 turnDegreesPerSecond 직렬화 한 줄 삭제와 기존 미추적 파일은 보존한다. 이번에는 커밋·amend·스테이징·push·빌드·배포를 하지 않았다. 모든 회로 변경은 미커밋 상태다.

사용자가 지정한 승인 영상은 적힌 `D:\renewal_CursorDash_incoming` 경로에는 없었으나, 실제 `D:\renewal_CursorDash\_incoming\references\60_approved_before_circuits.mp4`에 있었다. 앱 내 임시 탭/루프백 전용 서버로 디코딩한 **0.4/4/8/12/16/20/24초 표본**을 실제 읽었다. 630×1106/26.6333초의 일반 모드 화면이며 중앙 큰 점수, 넥서스·커서·상단 Pause와 차분한 바깥 회로를 보존 기준으로 삼았다. 오디오/프레임 사이의 모든 움직임/새 구현 게임 화면을 확인한 것이 아니다. 역할은 승인 기준이며 결함 목록/새 요구 추정에 사용하지 않았다. 원본 해시·메타데이터는 로컬 reference-evidence.json에 있다. 영상 전용 서버는 종료했고 정상 열람 탭은 닫았다.

일반 파일 명령의 제한된 프로세스 목록에서는 Unity가0개로 보였으나, 이후 앱 목록에는 대상 Unity 창(Cyberpunk/6000.3.23f1)이 있었다. 따라서 Unity 미실행/미저장 없음으로 단정하지 않는다. 내부 dirty 상태는 **확인 불가**였고 Unity 창 조작/실행/Play/저장·폐기/강제 임포트는 하지 않았다. 원래 씬과 자산 미저장 상태를 전체 저장/폐기하는 코드도 없다. D-014는 유지한다.

## 2. 기존 경로와 선택한 표현

실제 씬 `RemakeHUD/Background_Flash`는 **Assets/Art/Backgrounds/Cyberpunk/Original/Cyberpunk_Background_Flash.png**, GUID `f04ddfcc3b07b4bdfb99d6eef12f15e7`, 흰 tint/알파87/255다. Remake 폴더의 동명 이미지는 현재 이 Image의 원본이 아니며 두 파일을 구분했다. 실제 원본 PNG의 중앙은 RGBA(255,255,255,0)로 투명하고, 청록 트랙·보라 접점 등의 장식이 들어 있다. 팔레트/느린 색 흐름은 별도 `CursorDashLivingBackground`와 `LivingBackground.shader`가 소유한다. 그 코드/수치는 수정하지 않았다.

기존에는 `OriginalBackgroundFlash`가 원본 UI Image를 메모리에서 숨기고 Built-in 게임판 후처리에서 PNG를 합성했고, `FeverWorld`가 같은 PNG를 회색 회로로 재샘플링했다. 새 경로가 활성·지원될 때만 이 PNG 장식 합성을 억제한다. 원본 파일/이미지 색·Rect/임포트·GUID는 그대로다. 기능 OFF/셰이더·원본 미지원이면 기존 PNG 합성으로 돌아간다.

노드의 기존 절차적 메시/단색 가장자리 처리 방식을 참고하되 노드 코드는 변경하지 않았다. 새 구성은 **고정 그래프 + 재사용 메시2개/재질2개 + 월드 Renderer1개 + 기존 피버 합성용 전용 투명 레이어1개**다. 선마다 오브젝트/Collider를 만들지 않는다. 동적 회로는 배경 점수/HUD의 정점·알파·애니메이션을 건드리지 않는다.

- 정상 회로는 색면(-1000) 위 **sortingOrder -950**, 승인된 World TMP 점수(-900) 아래에 렌더링된다. 기존 게임판 왜곡/넥서스/커서/최종 광학/HUD 합성 순서는 유지한다.
- 피버 회로는 전용 CommandBuffer로 그려 흰 세계의 **전투 요소/점수 마스크 아래** 회색으로 합성한다. 회로 Renderer는 Stage 계층 밖이므로 게임 오브젝트 알파 마스크에 들어가 검정 적처럼 변하지 않는다.
- 실제 카메라의 논리 투영/월드 타격점·NexusPosition·기존 흑백 필드를 공유한다. 피버 회로 RT는 실제 대상 크기로 Flow를 갱신하며 GPU 투영을 수동으로 두 번 뒤집지 않는다. 실제 GPU 정합은 미검수다.

## 3. 경로·국소 타격·전환

일반 기본은 상·하 바깥 루트8개/연결 선40개다. 직선·45/90도 꺾임·분기/작은 빈 접점을 사용하고 중앙은 비운다. 밀도는 루트4~12/선20~60으로 제한한다. 고정 레이아웃이며 매 타격/라운드에 전체를 무작위 생성하지 않는다. 서로 다른 트리가 시각적으로 교차해 가짜 분기처럼 보이지 않는지도 순수 수학 검사로 확인했다. 일반 상태에서 흔들림/접점 반복 점멸은 없다.

타격은 `HitFeedback.Signaled`의 circleRemove/triangleBreak/hexBlock/nexusDamage 실제 point를 읽는다. 피버 처치는 기존 `BonusFeel(point, incoming)`에 회로 통지만 한 줄 추가했다. 점수/제거/물리/파편/기존 화면 효과를 다시 호출하지 않는다. 가까운 선이 .13 짧은화면축 단위 안에 없으면 시드를 만들지 않는다. 연결된 선의 그래프 거리와 분기 수에 따라 전달·감쇠하며 비연결 트리는 반응0이다. 반응은 가산하지 않고 최강값, 동시4/하드8 상한을 사용한다. 일반에서는 선의 대비가 잠깐 오르고 피버에서는 옅은 회색이 잠깐 짙어진다. 전체 회로 점등/추가 원형 파동/컬러 네온이 아니다.

피버 진입에서는 기존 흰 경계가 닿는 정상 경로를 바깥으로 밀고 경계 뒤의 정상 회로를 숨긴다. 흰 영역에 들어온 바깥 루트에서 순서·속도 차를 두어 선 끝이 진행하고, 구축 끝만 잠깐 더 읽힌 뒤 옅은 기본 대비가 된다. 정상/피버 레이어는 같은 실제 흑백 마스크의 반대 영역에 그려져 이중 잔선을 억제한다.

피버 종료는 승인된 **A .20초 등장 → B .51초 흡입 → C .24초 안정화**를 변경하지 않았다. A에서 Exit.Suction=0이므로 회로 이동/흡입0. B부터 **기존 FeverExitMotion.Travel·Power·Twist와 실제 NexusPosition**을 사용해 피버 경로 자체를 곡선 수렴시킨다. 원래 노드 찌그러짐/링 반동/공간·색수차 진행은 그대로다. 컬러가 드러난 바깥 루트부터 정상 경로의 선 끝이 다시 진행하고, 아직 흰 영역의 정상 회로는 셰이더가 가린다. 회로 완료를 기다리려고8초/판정·공급/일반 복귀를 지연하지 않는다.

마지막 `nexusFinal`은 흡입/복원보다 우선한다. 안쪽 끝부터 바깥 루트로 선 끝을 후퇴시킨다. 회수 중 사망이면 진행 중인 흡입값을 동결하고 같은 퇴장 선끝 진행에 따라 바깥 경로로 풀며 걷는다. 별도 흡입·복원 타이머를 함께 돌리지 않는다. 퇴장은 기존 Nexus finale와 같은 실제 시간 기준이며, 경로 시차 최대.16초+후퇴 최대.44초=**최대.60초**로 제한한다. 기존 최종 파괴/슬로모션/결과창.64초는 수정하지 않았다. 기본 세로 레이아웃의 퇴장 명목값은 약.41초이며 실제 프레임 실측이 아니다.

## 4. 조절 위치와 적용 시점

**Assets/Resources/CursorDash/VisualPrototype.asset → Dynamic circuits**. 기존 비회로 필드는 바꾸지 않고 아래19개만 추가했다. 단위1은 게임판의 짧은 화면축 전체 길이이며 해상도 픽셀 수에 묶지 않는다.

|필드|초기값|의미/적용|
|---|---:|---|
|dynamicCircuitsEnabled / circuitHitsEnabled|true / true|동적 회로/국소 전달 비교. 즉시, OFF면 반응 정리/원본 PNG 비교|
|circuitDensity|8|루트 수. 다음 라운드 또는 실제 화면비/카메라 크기 변경 때만 재구성|
|circuitLineWidth|.0026|짧은 축 대비 선 굵기. 즉시/클램프|
|circuitNormalContrast / circuitFeverContrast|.12 / .13|일반/피버 기본 알파·대비. 즉시|
|circuitHitRange|.13|가장 가까운 선을 찾는 실제 타격점 범위|
|circuitTransferSpeed|1.65|연결 선을 따라 전달되는 짧은축 단위/게임초|
|circuitHitDecay / circuitHitSeconds|.26 / .24|거리 감쇠 길이/선의 한 지점에서의 반응초|
|circuitMaxEmphasis / circuitReactionLimit|.18 / 4|추가 강조 상한/동시 슬롯(하드8). 여러 반응의 밝기는 최강값만|
|circuitBuildSpeed / circuitRestoreSpeed|1.8 / 2.1|선 끝의 구축/복원 속도. 각 진입/종료 시작에 캡처|
|circuitPathStagger|.018|루트별 제한된 시작 시차. 진입/종료 시작에 각각 캡처|
|circuitEntryPush / circuitRecallBend|.075 / .65|경계 밀려남/흡입 휘어짐. 각 전환 시작에 캡처|
|circuitDeathSpeed / circuitDeathStagger|1.6 / .018|후퇴 속도/시차. 마지막 타격 때 캡처·전체.60초 이하|

굵기/대비·타격 반응 값은 즉시 적용하며 변경해도 메시/재질을 새로 만들지 않는다. 전환 시간 관련 회로 값은 시작 때 캡처해 진행 중 Inspector 편집으로 선끝이 갑자기 바뀌지 않게 한다. 화면비 변경은 회로 그래프/반응만 재정렬하며 게임 값을 변경하지 않는다. 메시/재질은 계속 재사용한다.

기존 **Tools → CursorDash → 넥서스 수동 검수**에 ‘동적 회로 (현재 라운드)’/‘회로 국소 타격 (현재 라운드)’ 임시 토글과 경로/반응 수 표시를 추가했다. 영구 프로필과 구분하며 Retry 후 프로필 기본을 사용한다. ‘회로 프로필 기본값만 복원’은 **회로19필드만** Undo 기록 후 복원/dirty 표시하며 자동 저장하지 않는다. 사용자가 다른 미저장 자산까지 일괄 저장할 필요가 없다. 임시 토글 override는 별개이므로 다시 켜거나 Retry한다. 기존 프리셋/JSON171키/저장 데이터는 변경하지 않았다.

기존 fx.enabled OFF에서는 회로 타격 반응을 비우고, user.flash/OnFlash는 강조 밝기를, user.distortion은 밀려남/휘어짐을 제한한다. 기본 경로는 차분하게 유지한다. Pause에서는 전달/구축 시계가 같은 게임 delta=0으로 멈춘다. Retry/Home/씬 해제에서는 이벤트 구독·메시/재질/CommandBuffer·회로 오브젝트를 소유자와 함께 해제한다. 다른 Unity 프로젝트나 사용자 앱을 종료하지 않았다.

## 5. 변경 파일

Unity 루트 `Cursor-Dash-master/Assets/` 아래:

- 신규 `Scripts/Gameplay/StageManager/CircuitPathGraph.cs`(+meta): 고정 그래프·선끝·유한 전달 슬롯/거리·순수 회수 수학.
- 신규 `Scripts/Gameplay/StageManager/CursorDashCircuits.cs`(+meta): 메시 소유/레이어·전환/표시 정리. 거대한 새 게임 관리 시스템이 아니다.
- 신규 `Resources/CursorDash/CircuitPath.shader`(+meta): 선/빈 접점 AA·기존 흑백 필드별 가림.
- 기존 `Scripts/Gameplay/StageManager/OriginalBackgroundFlash.cs`: 기존 PNG 비교용 억제 플래그만.
- 기존 `Scripts/Gameplay/StageManager/CursorDashFever.cs`: 회로 생성·같은 delta 전달·실제 보너스 타격점의 시각 통지4줄만.
- 기존 `Scripts/Gameplay/StageManager/FeverWorldTransition.cs`, `Resources/CursorDash/FeverWorld.shader`: 회로 전용 임시 RT/회색 선 합성. 기존 점수·HP·전환·광학 식 보존.
- 기존 `Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`, `Resources/CursorDash/VisualPrototype.asset`: 회로 설정만 추가.
- 기존 `Editor/CursorDashNexusReview.cs`: 기존 창의 회로 비교/기본 복원만.
- 신규 `Tests/Editor/CursorDashCircuitTests.cs`(+meta): 수학/분리·상한/기본값 보존8사례. 기존 공식 테스트 삭제·무시/기대값 변경 없음.

AGENTS와 STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES/TUNING_GUIDE 및 이 인계를 갱신한다. Before/이번만 패치·컴파일·순수 검사/정적 보존 결과는 무시된 `Logs/dynamic-circuits-20261027`에 둔다. 기존 씬/프리팹/PNG·기존 meta/패키지·프로젝트 설정은 수정하지 않았다.

## 6. 실행 결과와 미검증

- **PASS 실제 순수 .NET**: 실제 CircuitPathGraph/CircuitPulseField와 기존 FeverFlowMath/FeverState 소스로 연결·비연결 국소성, 분기 지연/약화,40회 이상 반복/상한·Pause/취소,5가지 화면비·밀도4/5/8/12/상한의 안정 레이아웃/가짜 교차 없음, 선끝 후퇴·.60초 종료, A 흡입0과 실제 중심 수렴을 검사했다. 육각 소스/타이밍 사례는 넣지 않았다.
- **PASS 참조 C#** 게임110/테스트13/Editor2 오류0, 런타임 기존 경고134. 새 테스트는 새 런타임 어셈블리를 참조해 컴파일했다. 공식 테스트는 작성/컴파일만이며 실행은 **NOT_RUN**이다.
- **PASS 정적/보존** 범위별 기존 함수·프로필 기존 키·점수/팔레트·파편/노드/육각/종료 시계·씬/PNG/meta/사용자 파일 해시, 새 GUID/자원 참조/소유 정리·임시 RT finally/피버 마스크 분리·형식 확인. 최종 상세는 preservation-final.json/static-check.json을 따른다.
- **NOT_RUN** Unity 실컴파일·Shader/GPU/Physics·새 게임 화면·Play·모바일/성능·실제 프리셋 저장/브라우저 게임·빌드/Git 게시. 승인 영상/오프라인 경로 그림은 새 구현의 GPU 증거가 아니다. 최대2메시/2재질·루트12/선60/반응8과 메시당 최대2064정점은 정적 상한이며 실측 성능이 아니다.

## 7. 사용자 Play 검수 순서

열 씬: **Assets/Scenes/Cyberpunk.unity**. 사용자가 Play→훈련에 진입한다. 기존 접촉 진단은 광학 비교 시 끈다. 다음은 미실행 항목을 확인하는 절차이며 통과 선언이 아니다.

1. **PNG 비교**: 기존 검수창 동적 회로OFF→ON. 원본이 이중으로 남지 않는지, 배경 큰 점수의 위치/4×4·Scale1/색·탄성·상단 배율/Pause와 배경 팔레트 흐름이 그대로인지 확인한다. 장식 접점을 실제 검정 노드와 혼동하지 않는지 본다.
2. **국소 타격**: 회로 근처의 실제 일반 노드/레이저 방어·피버 노드를 단발/연속으로 맞힌다. 가까운 부분 먼저→연결 가지의 약한 전달인지, 중앙의 회로 없는 타격에는 먼 반대편 선이 켜지지 않는지 확인한다. 반응 수4 이하/빛·잔류 무한 누적 없음/기존 파편 강도 보존을 본다.
3. **진입**: 자연 만충→기존 흰 경계에서 정상 회로가 밀려나고, 흰 영역 안에만 바깥 선끝부터 구축되는지 확인한다. 선 끝이 잠깐 읽힌 뒤 옅어지고 중앙 게임 요소/HUD를 가리지 않아야 한다.
4. **종료**: 자연8초/Gauge0 뒤 A에서 링만 등장하고 회로 흡입0인지, B부터 기존 검정 노드/경계·색수차와 같은 중심/속도로 선이 휘어 들어가는지 본다. 드러난 컬러 쪽에서만 경로 복원, 마지막 링 수축·복원·감속과 정상 복귀 타이밍을 확인한다. 남은 노드0/소수/상한도 기존 검수 절차로 비교한다.
5. **사망**: 일반/피버·진입/흡입 도중 각각 실제 마지막 피격을 재현한다. 회로는 추가 흡입/구축을 중단하고 선끝이 바깥으로 걷혀야 한다. 중간 변형의 연결감/갑작스러운 위치 변화가 없는지는 실제 화면에서 특히 본다. 기존 넥서스 파괴 방향·리듬·슬로모션/결과 숫자·버튼/결과 등장 시점은 유지돼야 한다.
6. **정지/해제**: 각 단계 Pause→Resume에서 선 진행이 뛰지 않는지, Retry/Home/사망/반복 피버 뒤 이전 펄스·회수·시차·콜백이 남지 않는지 확인한다. 새 라운드는 정상 고정 회로/프로필 기본이며 점수·기록·공급 규칙은 그대로다.
7. **비율/정합**: 세로/긴 모바일 비율/정사각/가로에서 실제 타격점·넥서스·흑백 경계와 선의 이동이 같은 위치인지 본다. 기존 흔들림/왜곡 중 접점이 따로 미끄러지거나 상단 HUD/점수에 회로 변형이 적용되면 실패다. 실제 휴대폰 성능/가독성은 별도 검수다.

다음 한 작업은 사용자 위 검수다. 새 Flash Background 그림과 무관한 사이버 공간/UI 재디자인, 일반 노드 겹침은 여전히 보류한다. 이번 결과 확인 전 추가 구현/커밋·push·빌드·배포를 시작하지 않는다.
