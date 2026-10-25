# 근거와 참고 자료

## D-037 최신 종료 박자 정의 (2026-10-26)

최신 사용자 글이 기준: 생성 뒤첫회전1/3,3회2/3,5회완충,변형뒤6회에발사하며 **회전 종료 시점**을 뜻한다. 이번 추가 영상/이미지는 없으며 새로 실제화면을 확인했다고 하지 않는다. 기존지침/STATUS·MASTER/결정·환경·검증과 현재NodeRhythmMath/ArrowNode·관련테스트를 읽었다.
현재디스크의회전60도/.4초이동/.4초정지와비행.06을 기준으로6회시작4.0초와종료4.4초를 구분해 계산·검사했다. 실제Editor/Play 실행값의 측정이 아니다. 소스의 같은회전시계를 보존하면서충전·변형·발사 문턱만앞당겼고키/기본값·셰이더·피버는그대로다. [변경/근거](HEX_CHARGE_135_FIRE6_20261026.md).


## D-036 최신 육각 시점 수정 (2026-10-26)

사용자최신글: 생성0/3,2/4/6회째충전완료,6.5회쯤변형/7회공격. 이번영상·이미지추가는없으며새로실제화면을확인했다고하지않는다. 적용지침/최신STATUS·MASTER/결정·환경·검증을읽고현재ArrowNode/NodeRhythmMath/Rotator/HexagonalNodeRings연결과관련테스트를확인했다.
실제`Assets/Prefabs/Nodes/Cyberpunk/ArrowNode.prefab`의각Rotator60도/.4초이동/.4초정지,warningDuration.25/preFire.025/attack.06은디스크저장값이다. 6.5~7회는.20초여서실제변형.175/예비.025를앞에배분해7회에발사하도록연결했다. 프리팹/씬/Rotator자체는이번에변경하지않았다. GPU·실제DOTween·Play결과가아닌소스/순수/참조근거다. [검수/Before](HEX_CHARGE_246_20261026.md).

## D-035 최신 사용자 회전 연동 결정과 디스크 근거 (2026-10-26)

사용자최신글은이전‘회전비유’를직접대체한다: 생성1회→1/3→2회→2/3→2회→3/3→2회→변형/공격. 피버렌즈굴절/국소수평슬립/RGB도소폭강화. 이번새영상·이미지는없으며실제화면/컴퓨터조작·UnityPlay를확인했다고하지않는다.
읽은실제경로: `Assets/Prefabs/Nodes/Cyberpunk/ArrowNode.prefab`의Outside1/2 Rotator60도/.4초이동/.4초정지/반대방향, ArrowNode.Execute/Update/LateUpdate→기존Warning/BeginFire/Block/Impact, HexagonalNodeRings.Draw의원본회전transform복사. 충전은실제소유tween.Elapsed(true)로읽도록연결하며기본7회는5.2초다. 이후저장값변형.25/예비.025/비행.06은보존했다.
피버는VisualPrototype.asset/기본클래스→FeverWorldTransition.FinishBoard→기존FeverImpact.shader의방사렌즈+localband/실제RGB샘플이대상이다. 기존최종게임판순서·HUD제외/상한을바꾸지않고3계수만올렸다. 현재사용자실행오버라이드·GPU화면은미확인. [.NET/참조/범위와수동절차](HEX_ROTATION_CHARGE_20261026.md).


## D-034 사용자 지시와 실제 읽은 효과 경로 (2026-10-26)

최신 사용자 피드백: 테두리2회는연동지시가아닌박자비유, 2/3/완충후대기·검정원느린축소·피버Distortion/색수차 요청. 이번에새영상/이미지제공없음, 실제게임화면/PC조작 없음.
직접 읽은 NodeRhythmMath/ArrowNode의3.5마감, FeverBonusNode/VisualPrototype의.24퇴장, CursorDashFever.BonusFeel→FeverWorldTransition.Hit/SetFlow→FeverFlow.CDFContactOffset(.00055공간/.00065굴절/.002상한), FeverWorld의circuit(i.uv)미굴절/흑백재합성, CursorForeground와LocalImpactDistortion의최종순서를추적했다. 이전코드의작은필드/회로미굴절/색수차경로없음을확인한것이며 현재사용자Native설정값·화면을실측한것은아니다.
현재설치된Unity6000.3.23f1의참조DLL로C#컴파일만, 실제Shader컴파일/Unity·GPU/Play는미실행이다. 새게임판최종광학은FeverImpact.shader/FinishBoard이며 환경전환/공통일반왜곡과구분한다. [효과목록·값](FEVER_OPTICS_HOLD_20261026.md).

## D-033 소스와 최신 사용자 지시 (2026-10-26)

최신 사용자는 피버에서 사라지는 노드에 축소 퇴장/가벼운 탄성 등장, 육각 내부 게이지에3회의 빠른 상승·멈춤·탄성을 요청했다. 효과음 없음/전체준비·공격시점 유지가 확정조건이다.
직접 읽은 경로: CursorDashFever.Transition→NodeCreator.StopNodes(진입 일반위협 무보상Reset), FeverBonusField.Tick→FeverBonusNode.Advance의2.8초 만료와 종료 ClearNodes. 생성기에는 상한/공간 대기만 있으며 살아있는 노드를 교체하는 경로 없음. ArrowNode.Update/LateUpdate의 준비elapsed/prepare와 NodeFlatGraphic.DrawHexagonA/_Charge의 중앙 확대 표현을 확인했다. Cyberpunk ArrowNode.prefab 저장값 준비3.5/변형.25/예비.025/비행.06 확인, 실제 열린Editor/Play 관찰은 하지 않았다. 이번 영상·이미지/외부 참고 추가 없고 과거 참고를 다시 확인했다고 하지 않는다. [변경/검수](NODE_RHYTHM_EXIT_20261026.md).

## D-032 후속 오류 근거 (2026-10-26)

최신 사용자 스택: `FeverBonusBurst.<Initialize>g__Polygon|25_0` 원본20행 → Initialize42행 → FeverBonusField.Collect114행 → FeverBonusNode.TryCollect53행/OnTriggerEnter2D46행. 사용자 실제 Play 실패이며 Codex Play 결과가 아니다. Before의12각형 호출/Vector4[k]를 소스로 확인했고 동일 본문을 실제 Unity 관리형 Vector4로 실행해 같은 메시지를 재현했다. 수정 후 관리형 생성 검사만 PASS, 실제 Unity Mesh/Material·GPU 성공은 미확인이다. 근거 `Logs/fever-burst-vector-fix-20261026/Geometry/result.json`.

## D-032 자료 역할·실제 관찰 (2026-10-26)

- `_incoming/references/40_current_fever_review.mp4`: 현재 문제 녹화(목표 아님), 실제34.299967초/668×1184. 0~14초2초간격,19~26초1초간격을 브라우저 디코드로 확인/프레임 시트 보존. 20초 머리 아래/검정 꼬리·파편 위,23~26초 배율 노이즈가 보인다. 이번 수정 후 Unity 화면이 아니다.
- `_incoming/references/41_issue_cursor_hit_alignment.png`: 머리·꼬리/파편 문제 이미지. 손그림은 주석. 넥서스 중심 기준 위아래 대칭과 가까운 위치 관계를 확인했다. GPU 변환 수정의 원인 후보를 뒷받침하지만 수정 후 GPU 입증은 아니다.
- `Fever_System_v1/06_FEVER_REFERENCE_ENTRY.mp4`와07: 기존 움직임 참고, 이번에도 제공 파일 표본 프레임 확인. 06 0~3.15초/.45,07 0~7.5초/1.25. 07의8.75 라벨은 실제 끝7.878초로 제한된 중복이므로 타이밍 근거에서 제외. 색/모양 복제 안함·오디오 검사 안함. 21_hitfeel은 기존 분석 이력 유지, 이번 별도 재생하지 않음.
- Unity6.3 공식 CommandBuffer.SetViewProjectionMatrices 예시는 논리 `camera.projectionMatrix`를 전달한다: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rendering.CommandBuffer.SetViewProjectionMatrices.html . 현재 이 경로의 사전 GPU변환을 제거했으나 최종 검증은 사용자 GPU/Play 대기.
- 로컬 기존 UGUI/TMP `TextMeshProUGUI.LoadFontAsset`와 TMP_Text.font/fontSharedMaterial 설정 경로를 읽었다. 글꼴 UV·아틀라스와 할당 재질의 일치가 필요하며, 점수/배율 재질을 각각 소유/재연결한다. 이전 런타임 재질·CanvasRenderer 상태는 미수집이라 정확한 글자 오류 원인은 확정하지 않는다.
- 배경 참고 영상은 없다는 사용자 정정을 유지하며 글의 팔레트/흐름 기준만 사용. [상세/근거](FEVER_STREAM_ALIGNMENT_20261026.md).

## 최신 — D-031 현재 자료와 실제 호출 경로 (2026-10-26)

30_current_play.mp4는44.6333초/626×1108의 현재 결과이며 목표가 아니다.31/32 PNG는 흡수 잔선/중앙×1·시작선·빈호 문제다. 요청 D:/renewal_CursorDash_incoming/references는 없고 같은 이름은 저장소 _incoming/references에서 확인했다.30전체표본/21~24.68진입/30~33.68종료,06진입.60~4.28/07유지0~4.83·종료4.70~7.69 프레임을 실제 읽었다. 초기46초 라벨 중복은 제외했고 일부 시트 아래행 크롭/오디오 미열람을 기록했다. 새 GPU화면의 증거가 아니다. 배경 참고는 사용자 답변으로 없었던 것으로 정정됐다.
소스 실제 확인: FeverBonusNode의 root1반경/Transform/Trigger·조기해제, Field의 풀/배치/수집·버스트목록, Player FixedUpdate의 예약 이동/박스, Stage·Fever 규칙과 시간/점수·저장, gauge중앙캡처/빈트랙, World 알파/마스크검색·상단판, 원래Trail/실제커서 합성, 원형·육각 Execute→준비·warning·공격과 기존 프리팹값/rotator·아트자식. 기존 노드 생성기와 일반·최종 VFX를 재활용하고 새로 제작하지 않는다.
Unity6.3 공식 [HideFlags](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/HideFlags.html)의 물리장면 제외와 [FindObjectsByType](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Object.FindObjectsByType.html)의 DontSave 검색 제외를 확인했다. 현재 메모리/PhysicsScene/셰이더 호출 결과는 보지 않았다. Logs/fever-contact-polish-20261026의 source-evidence.json·before/소스delta/순수·정적·Compile·프레임이 근거다. [구현과 미검수 구분](FEVER_CONTACT_POLISH_20261026.md).

## 최신 — D-030 현재 영상·공식 서체·우선순위 (2026-10-26)

lastVersion.mp4(42.866633초,624×1122)는 사용자 현재 촬영, 첨부 PNG는 수정 전 넥서스 문제 자료다. 현재 영상의 전반/22~24.3초 진입/30.4~33.39초 복귀를 확인했고 참고06(.6~4.28)/07(유지0~4.83,종료4.7~7.69) 프레임도 다시 확인했다. 요청 프레임 시점과 SHA는 Logs/fever-white-world-20261026/source-evidence.json, 캡처는 같은 폴더다. 초기 overview의44/46초 라벨은 끝 프레임으로 clamp된 중복이며 근거는0~42.32의 current-overview-valid.png로 교체한다. 새 Unity 결과를 확인한 영상이 아니다.
공식 https://www.gonggames.com/#csr 의 한국어 이용 조건/임베딩 허용과 /assets/fonts/esamanru_TTF(Win).zip 를 실제 읽었다. Bold/Medium 권장 용도·출처 권장을 확인했다. ZIP에는 Bold/Light/Medium TTF만 있어 필요한2개를 원본 그대로 쓰고 공식 조건을 별도 기록했다. OFL로 재분류하지 않는다. 폰트 비교는 실제 TTF의 CPU 래스터다.
최신 사용자8번이7번의 배경 보류를 대체한다. 반면 검은5원호의 시간 표시는 임시 우선 시안이다. 기존 승인 원형/육각·레일건·최종 VFX/규칙은 재제작 지시가 아니다. 관련 합성/기록/씬 저장값을 좁혀 확인하고 source별 정적 보존을 검사했다. [상세](NEXUS_WHITE_FEVER_20261026.md).

## 최신 — D-029 C 정책·참고 영상 재열람 (2026-10-25)

사용자 B 확인/C 승인·상한4/최신 연출 요청과 Fever_System_v1 00/08이 기준이다. B 행동 확인은 게이지 외형 최종 승인을 뜻하지 않는다. 원본 스케치/생성 시트의 역할은 D-028대로 유지한다.
실제 06_FEVER_REFERENCE_ENTRY.mp4의.60~4.28초를.16초 간격, 07_FEVER_REFERENCE_PLAY_EXIT.mp4의0~4.83초를.21초 간격/4.70~7.69초를.13초 간격으로 브라우저 디코딩 프레임에서 재검토했다. 진입의 즉각적인 전환/집중, 유지의 연속파괴·짧은 여운, 종료의 짧은 전환 뒤 안정감을 참고한다. 영상의 긴 설명 대기·색/도형·무적/게임규칙은 복제하지 않고 우리가 정한 흑백 곡선 에너지로 구성한다. 두 영상의 오디오 없음은 B에서 확인한 입력 파일과 동일 해시다.
이번 실제 소스 확인: FeverState/StageManager/NodeCreator/PatternReader의 호출·시간, 현재 Normal/Arrow Reset/비활성화/레이저 수명, 실제 플레이어·잔상 콜라이더, 넥서스3이미지/점선Rotator/커서 합성/안정HUD, 기존 기록·테스트 보호 경로. 대상 Cyberpunk/2프리팹은 현재 Normal/Arrow GUID이며 legacy Defence/Laser는 대상에 없다. 기존 승인 효과를 개편하지 않는다.
근거 `Logs/fever-c-20261025`: Before/before, reference-entry/hold/exit.png, source-evidence, static-check, 실제 순수 소스 검사/Compile, 이번 전용delta/manifest/보존 대조. GPU 진단 메뉴는 작성됐으나 호출되지 않았고 C 실제 게임 화면은 없다. [상세](FEVER_STAGE_C_20261025.md).

## 현재 — D-028 Fever_System_v1 및 후속 상한 결정 (2026-10-25)

사용자지정00/08/01/09와02생성시트·03~05원본스케치·06/07영상. 우선순위는최신사용자결정이최상위다. 4정책채택후④x4수정메시지가00의상한미정과최초상한없음제안을대체한다. 1~3은확정됐고C연결은보류다.
실제로4PNG/각12프레임MP4를열람했고MP4박스hdlr의오디오0을확인했다. 영상은연결감/명암/파괴밀도참고이며규칙복제/SFX근거가아니다. 원본은수정하지않았다. `source-evidence.json`에입력SHA/역할/패턴데이터가있다.
실제StageManager/ScoreManager/Node.BeginExecution·원형저지/육각반환호출,Death/MainUI기록,Canvas/합성/경계/점선,PatternReader와연결된Cyberpunk메인/서브TXT를읽었다. 저장패턴당2~4/1노드,대기0~5.2/3.2~3.6초/사이1.4초를임시충전값의근거로사용했으며실제성공빈도/난이도측정으로표현하지않는다.
근거 `Logs/fever-b-20261025`: Before/before,실제영상화면,source-evidence,static-check,순수코드/컴파일로그,모델도식,이번delta/신규manifest/보존대조. [A/B구현과정확한확인범위](FEVER_STAGE_B_20261025.md).

## 현재 — D-027 사용자 제공 현재 영상/실제 경로 (2026-10-25)

Downloads의 01_current_circle_break.mp4,02_current_hex_attack_break.mp4,03_current_nexus_destroy.mp4를 읽고 SHA/크기를 기록했다. 각16개 전체 샘플+24개 문제구간 프레임을 로컬 브라우저에서 실제로 디코딩·확인했다. 원본영상은 변경하지 않았다. 목표 참고 영상/외부 레일건 조사로 대체하지 않는다. 상세 샘플 시간은 캡처 안에 있다.
실제 현재 CircularBreakVfx/NormalNode→StageManager/HitFeedback A호출, NexusImpactVfx 최종 시계/Death/CancelFinale, ArrowNode 상태/프리팹 저장값/씬 override부재, PlayerCharacterAfterImage.TryBlockBeam/DistanceBetweenSegments/수명/반사, 기존 테스트와 수동창을 확인했다. 소스 확인과 수치 모델은 실행 화면이 아니다.
근거 `Logs/impact-rail-local-20261025`: before/Before, video-sources/화면캡처, static-check/circle-model/circle-geometry-model(모델임을 표기), Compile, D027-only-source-delta/preservation-final. [구현/보존/검수](IMPACT_RAIL_LOCAL_20261025.md).

## 현재 — D-026 직접 수정·방향 정정·공개 배포 승인 (2026-10-25)

사용자 원형시트그대로사용거부/폭발을뚫는새단색VFX·원형화면육각기준·최종방향만수정 요청. 이후첨부에대해 **지그재그쪽으로분출/글방향변경**, **지금분출의반대방향** 답변이최초진입반대문구를대체한다. 참고이미지 user-direction-reference.png는유저그림이며그안의표시를별도도구승인으로취급하지않는다.
실제 호출코드/시트기존참조/CameraFlash연결/공통WorldImpact식·Tuning기본값·B원형분기/12최종방향함수·원본파일해시를확인했다. 메인/설정/JSON만실제공개브라우저로읽어buildId·기본값일치/Console0을확인했다. 전투화면을확인했다고하지않는다. 근거 `Logs/circle-punch-release-20261025`: before/After·source delta·build-result·editor-build-proof·candidate/public manifests·Pages결과·runtimeJSON/실제스크린샷. [릴리스·수동검수](CIRCLE_PUNCH_RELEASE_20261025.md).


## 현재 — D-025 직접 피드백·원본 실제 파일 (2026-10-25)

사용자 Unity 테스트 재현(커서 실패/A약함/B승인/C강도좋음/D접촉개선/바깥정지/최종파괴)을 현재 요청으로 사용한다. 첨부 빨간선은 사용자 추가 답변으로 **진입 반대쪽 되튕김** 확정. 원본 이미지는 `Logs/event-vfx-20261025/user-rebound-reference.png`로 보관했다. 현재 사용자는 Play를 종료했다고 답했다.
실제 참조: NormalNode.prefab.effectObject → NoramlNode_Effect.prefab → NormalNodeEffect.controller(speed1.5) → NormalNode_Interaction.anim(12fps/8키/이벤트0) → VFX/Cyberpunk/Original/Cyberpunk_NormalNode_Effect_Burst_Sheet.png(PPU150). 실제 원본 PNG와8slice 분석 그림을 열람했다. 셰이더/씬/코드/참조/재생속도 소스 확인이며 Unity 실행 화면이 아니다.
실제 코드: CursorVisual SpriteRenderer/LocalImpactDistortion/원본UI3이미지/NexusGameboard 후합성·UIcamera, A/B/C/D호출/피해단발가드/ScoreManager setter/Death/Retry/Home. 기존 파일1722해시·Before백업·42정적검사·참조 컴파일과 원본분석은 `Logs/event-vfx-20261025`. Unity Camera.Render/OnRenderImage 공식문서는 최신 [사건별 결과](EVENT_VFX_20261025.md)에 연결했다. 레퍼런스 원본 프로젝트를 새로 실행/변경하지 않았다.


## 현재 — D-024 여섯 항목 직접 요청 (2026-10-25)

사용자 원문: 커서/Nexus 표시 순서만 변경, Canvas/카메라/정렬/후처리 확인, 둘째 어두운 노랑, 변형→짧은 반동/멈칫→발사, 바깥 넥서스만 엇박 회전/휴지, 원형만 밀어 부수는 VFX. 승인 형상/육각 파괴 보존·기존 준비/레이저속도/반사 유지·공통 화면 효과 일괄 강화 금지·자동 Play/빌드/커밋/배포 금지다.
실제 현재 파일과 사전 백업을 대조했다. Scene UI/Camera/HpFrame(-360도4초)/HpCentral(+360도3초) 저장값, NexusGameboard/LocalImpact/UIcamera 코드, player CursorVisual, ArrowNode 상태/풀/반사와 승인 ImpactFragments/ImpactFlat/옵션 경로를 읽었다. `Logs/test-polish-20261025/nexus-scene-input.json`, `static-check.json`, Compile/보존 로그가 근거다. 레퍼런스 프로젝트/영상 재조사·Unity 실행은 하지 않았다. 런타임 값/화면 검수는 사용자가 수행한다.

## 현재 — D-023 노드 승인·넥서스 표시 연결 (2026-10-25)

출처: 사용자 원형·육각 현재 형태/동작 승인, 한 겹만 밝고 연한 노랑, 넥서스 본체/수위/장식의 게임판 효과 연결/HUD 고정, 평상시 물결 제거/빠른HP감소/단발 반응/연속교체, HP·판정·사망 규칙 유지 요청. 공통 타격감 미승인·추가튜닝 금지·자동Play/빌드/커밋/배포 금지다.
실제 소스: Cyberpunk의 UI/HpBar/HpFrame/HpCentral·UI Canvas/스프라이트3GUID·StageManager/ScoreManager/NexusEnergy·기존 LocalImpact·Background 합성·입력의 Selectable만 막는 경로를 읽었다. 레퍼런스 WaveGame/제공 영상을 다시 열지 않았다. 새 근거는 `Logs/nexus-presentation-20261025/nexus-scene-input.json`, `static-check.json`, `Compile/*-result.json`, `preservation-final.json`. 설정 파일/코드/정적 모델을 실제 Unity 화면 검증으로 표시하지 않는다.

## 현재 — D-022 사용자 승인과 육각 원본 A (2026-10-25)

사용자 직접 승인: 원형 외형 유지, 육각 A 형태/비율·노랑·두 겹 끊어진 역회전 복원. 충전/6꼭짓점/발사 방향/뒤→앞 배출 유지. 타격감 미승인·이번 튜닝/확장 금지·넥서스 보류·자동 Play/빌드/커밋/배포 금지.
A 실제 부품: `Assets/Art/Nodes/Cyberpunk/Types/Cyberpunk_AttackNode/`의 Central/Inside/Outside/Outside_2 PNG/.meta, 기존 ArrowNode.prefab의 Node 스케일.5/회전±60도·.4초. 이전 A 원본 근거와 현재 파일을 대조했으며 B 옛 시트로 대체하지 않았다. 새 `Logs/hex-originalA-20261025/original-hex-contours.json`에4개 원화SHA/피벗/반알파 윤곽·실측이 있다. 정적 비교 PNG는 Unity 화면이 아니다. 레퍼런스 WaveGame을 다시 열거나 타격감 조사로 확대하지 않았다.

## 최신 — D-020 원본 복원·WaveGame 실물 근거 (2026-10-25)

사용자직접요청: 원본프리팹/에셋외형복원,레퍼런스경로WaveGame/21_ref_hitfeel.mp4 실제호출흐름분석,분석/계획후중단,사용자UnityPlay/컴퓨터사용전알림. 추가선택: **A형태·비율+육각노랑유지**. 추가요청: 원형중앙원중심오차보정,넥서스/UI역할분류.
HEAD프리팹본문을읽기전용git show로새로그에추출해현재에셋GUID와대조했다. Types의A부품과OriginalNodes의B시트를구분한다. 분석용PNG합성은Unity실행이아니며실제메타피벗반영여부를명시했다. 바깥원피벗/반알파윤곽상자추정과정확한원중심맞춤도구분한다.
레퍼런스GameFeelManager/WaveFront/Person/WaveAnimationController/Spawn/Pool/UIManager/SoundManager,WaveGame저장컴포넌트·VolumeProfile·UniversalRP/Renderer2D·Quality,Wave1~3프리팹/컨트롤러/클립이벤트를읽었다. 사용자메시지의코드기본값을씬값으로대체하지않았다. 원본4영상12프레임씩/참고40프레임은이번에새로디코드/열람했다. 실제Unity런타임Inspector/오디오확인은아니다.
Unity공식 [VolumeParameter overrideState](https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@17.0/api/UnityEngine.Rendering.VolumeParameter.html)는비활성override를혼합하지않음을지원한다. referencecenter값쓰기와실제중심반영을구분한다. [전체분석/파일근거](ORIGINAL_NODES_WAVE_REVIEW_20261025.md),새로그 `Logs/original-wave-review-20261025`. 아래D019의레퍼런스경로미제공기록은당시이력으로보존한다.


## 후속 시각 피드백과 GameFeelManager (D-019)

사용자 직접 메시지: 육각 노란색 통일, 원형 파괴 조각 과대 문제, 타격점에서 배경 반투명 선이 일렁이고 밝아지는 강한 피드백. 이어서 독립 중앙점 제거와 전체 화면 Distortion을 요청했다. 제공 GameFeelManager 전문은 채팅에서 읽었으며 다른 프로젝트 경로/씬/프로파일은 받거나 열지 않았다.
참고한 원리: impactPos의뷰포트중심,시간정지중에도진행되는효과/OutCirc복귀,킥과밝기응답. 현재Built-in이고URP패키지/Volume이없으므로동일URP클래스를설치하거나붙였다고하지않는다. GameOverFlash/사운드/전역코루틴중지/TimeScale강제1복귀는범위확대또는기존정지정책충돌로복제하지않았다.
실제 이전WebGL기본경로의크기1/히트스톱.04/중앙점.1은 `Logs/hitfeel2-20261024-231337/BeforeWeb/default-after-contact-full.json`과 실제22프레임으로 확인했다. 파편이20배커지는 fallback 분기는 소스/실행이 함께 뒷받침한다. 실제배경GPU검증/전후자료/새빌드는 [후속결과](FEEDBACK_REVISION_20261024.md)를 따른다. Unity기능사용률100%를측정했다고주장하지않는다.

## 노드 그래픽·타격감 참고 (D-018, 2026-10-24)

사용자 원문: `_incoming/references/00_CursorDash_Node_Graphics_Hitfeel_Instructions.txt`(469줄). 실제 시트3장 열람: `01_ref_hex_flow.png`, `02_ref_circle_break.png`, `03_ref_hex_reflect_break.png`. 설명용 번호·문구·화살표·배경은 게임에 삽입하지 않는다. 시트의 낮은 에너지 코어 이동은 적용하지 않고 중앙 코어 요구가 우선한다.
영상5개를 기존 브라우저 디코더로 각각12개의 서로 다른 타임스탬프 프레임으로 확인했다. 원형 공격3.723233초, 원형 파괴2.331초, 육각 공격5.463초, 육각 반사파괴5.331초, 타격감 참고8.203초. 실제 마지막 디코드 시점/크기는 `Logs/node-graphics-20261024-192424/ReferenceFrames/reference-review.json` 및 같은 폴더의 프레임에 있다. 처음 추출의0초 고정 문제를 발견해 유효 프레임으로 재추출했으며 무효 추출은 동작 증거로 사용하지 않았다.
관찰: 기존 원형의 꽃잎형 셸·후반 긴 가시, 금색 삼각 육각, 참고 영상의 넓은 보드 휨과 되튕김. 희소 프레임에서 정확한 히트스톱/반환 ms·셰이더 기술·오디오를 확인했다고 주장하지 않는다. 참고 영상 사운드는 청취하지 않았다. 시트/영상은 Assets/빌드에 복사하지 않았다.
흐림 확인: 원형 외곽 PNG190×188에 부분 알파 픽셀17,772/35,720과526종 RGBA가 있어 원본의 부드러운 알파 전이를 확인했다. importer mipmap off/filterMode -1/textureCompression1은 설정 기록이며 실제 GPU 포맷·필터가 흐림을 얼마나 유발했는지는 미확인이다. 이번 노드는 텍스처 재압축/Point 전환 대신 해상도 기반 AA 도형으로 교체했다. 전역 품질/필터/배경/UI는 바꾸지 않았다.

확인일: 2026-10-23 (KST). 로컬 HEAD `8ceedbb7ead4c2b97417739f62fd7904e1a51497`, 브랜치 `main`.
파일을 읽은 근거와 실제 실행 결과를 구분한다. 이번에 Unity 컴파일·테스트·Play·빌드를 실행하지 않았다.

## 1. 요청과 시작 자료

- 이번 사용자 메시지: 지정 시작 지시문을 먼저 읽고 문서·기존 환경 조사까지만 수행, 기존 지침·설정·변경 보존,
  허용 문서·보조 도구만 작성, 게임·설치·연결·실행·테스트·빌드·Git 쓰기·배포 금지, 완료 후 중단.
- `_incoming/CursorDash_Dev_Starter_v1/CODEX_START_PROMPT_KO.txt`
- `_incoming/CursorDash_Dev_Starter_v1/START_HERE_KO.md`
- 같은 자료의 `templates/AGENTS.project.md`, 문서 6개, 진단 PS1을 전부 읽었다.
- `MANIFEST.json`이 명시한 파일 10개 SHA-256은 로컬 원본과 전부 일치했다.

MASTER_SPEC의 게임 목표·과거 사용자 결정은 **제공된 시작 자료가 전한 요청**이다.
해당 원래 대화를 이번에 열어 확인하지 않았고, 이번 사용자 메시지의 실행 허가와 구분했다.
초안의 PROPOSAL·OPEN을 새 결정으로 바꾸지 않았다.

## 2. 지침·설정·기존 문서

- `C:\Users\molph\.codex\AGENTS.md`: 0바이트, 내용 읽음. 전역 override 없음.
- 상위 `D:\`, 저장소·Unity 루트·하위의 AGENTS/override 존재 여부를 조사했다. 시작 시 저장소 지침 파일 없음.
- `C:\Users\molph\.codex\config.toml`: 존재·범위 먼저 확인. MCP 이름/실행 파일 경로·URL의 인증/쿼리 제거값,
  해당 저장소 trust 등 비밀값을 제외한 허용 항목만 선별 확인. 인증파일·환경변수의 비밀값 미열람.
- `README.md`, `Cursor-Dash-master/README.md`
- `Cursor-Dash-master/docs/CursorDash-asset-inventory-and-cleanup.md`: 2026-10-13 정리·검증의 과거 보고.
  그 보고의 테스트 3/8개 통과·캡처 설명을 이번 결과로 재사용하지 않았다.
- `Cursor-Dash-master/docs/folder-reorg-manifest.json`: 이동 64건, movedFiles 804건 등의 기록 구조 확인. 재실행·복구 안 함.

이번 새 AGENTS는 현재 대화에서 작성 후 다시 읽었다. 사용자 설정과 기존 문서는 수정하지 않았다.

## 3. 이번에 직접 읽은 프로젝트 근거

아래 경로의 `P/`는 `D:\renewal_CursorDash\Cursor-Dash-master/`를 뜻한다.

| ID | 실제 파일 / 읽은 내용 |
|---|---|
| L-01 | `P/ProjectSettings/ProjectVersion.txt`: 6000.3.23f1 / 09d2ecc7fb28 |
| L-02 | `P/ProjectSettings/EditorBuildSettings.asset`: 선택 씬·Cyberpunk 활성 |
| L-03 | `P/Packages/manifest.json`, `packages-lock.json`: 두 local 의존성, Test Framework 1.6.0 |
| L-04 | `P/Assets/Scripts/Gameplay/Object/Node/NormalNode.cs`, `ArrowNode.cs`, `Node.cs`: 경고/공격·접촉/잔상·반환/Impact·풀 초기화 |
| L-05 | `P/Assets/Scripts/Gameplay/Object/Character/PlayerCharacterAfterImage.cs`: 살아 있는 선분과 방어 폭·만료/초기화 |
| L-06 | 같은 Character 폴더의 `VirtualJoyStick.cs`, `PlayerCharacterController.cs`: 입력 소유·UI 차단·조건부 키보드·관성/반전·원형 접촉 |
| L-07 | `P/Assets/Scripts/Gameplay/StageManager/StageManager.cs`, `ScoreManager.cs`: 넥서스 위치·피해·HP 표시·상태·결과/재시도/홈 |
| L-08 | 같은 StageManager 폴더의 `NodeCreator.cs`, `PatternReader.cs`: 기존 풀·생성 개발 버튼·패턴/대기·종료 |
| L-09 | 같은 StageManager 폴더의 `PauseHandler.cs`, `DeathUIController.cs`, `P/Assets/Scripts/Menu/StageSelectSceneManager/MainSceneController.cs`: 메뉴·설정·음량·일시정지·결과 |
| L-10 | `P/Assets/Scenes/Cyberpunk.unity`의 해당 MonoBehaviour YAML 및 스크립트 .meta GUID: 연결과 직렬화 값 대조. 씬 전체 재작성 안 함 |
| L-11 | `P/Assets/Prefabs/Nodes/Cyberpunk/NormalNode.prefab`, `ArrowNode.prefab`: 저장된 시간·피해 필드, 씬 시간 필드 override 검색 |
| L-12 | `P/Assets/Editor/MigrationValidation.cs`, `CursorDashRemakeValidation.cs`, `CursorDashWebGLSettings.cs`: 구현 읽음. `CursorDashRemakeSetup.cs`는 진입점·쓰기/설정 변경 위치를 조사했으며 실행 안 함 |
| L-13 | `P/Assets/Tests/Editor/CursorDash.EditorTests.asmdef`, `CursorDashSceneRegression.cs`, `CursorDashTestReport.cs`: 현재 11개, Play 진입·씬/선호/캡처/로그 쓰기 부작용 |

원형/육각 본체 변형·돌진·완전 테두리 흡수·넥서스 가장자리·에너지 HP·새 훈련 UI의 실행 증거는 없다.
기존 코드는 기존 상태로, 새 목표는 요구·제안·미정으로 유지한다.

### 설치·캐시의 좁은 범위 읽기

- 설치된 Unity.exe의 FileVersion/ProductVersion, WebGLSupport 디렉터리·확장 DLL·BuildTools 존재.
- 두 manifest file 경로의 `package.json`: MCP 10.0.0, Newtonsoft 3.2.1. 전체 캐시·드라이브 탐색 없음.
- 기존 MCP 소스 `C:\Users\molph\Documents\Codex\2026-10-03\task\unity-mcp-v10.0.0\Server\pyproject.toml`:
  소스 버전 10.0.0 / Python >=3.10. 살아 있는 서버의 실행 파일 버전은 별도 확정하지 않음.
- `P/Library/PackageCache/com.unity.test-framework@bd7f943e9647/package.json`: 공식 패키지 1.6.0.
- 같은 패키지의 `UnityEditor.TestRunner/CommandLineTest/SettingsBuilder.cs`: testPlatform/testFilter/assemblyNames/testResults 인자와 quit 경고 확인.
  캐시 안의 이 특정 패키지만 읽었으며 수정·복원하지 않았다.

## 4. 실제 수행한 읽기·보조 조사

- Git: 최상위·HEAD·브랜치·porcelain 상태, 로컬 커밋 관계/차이, 추적·미추적 파일 목록.
  `413f842` 객체가 로컬에 존재하므로 네트워크 없이 비교했다. HEAD 다음의 README 수정 1개이며 게임 diff 없음.
- 시작 시 기존 비무시 파일 1,560개 SHA-256 기준을 메모리에 보관해 종료 시 비교. 인증파일은 조사 대상으로 추가하지 않음.
- `git --version`, `codex --version`, 도구의 PATH 위치·설치 파일 메타데이터, PowerShell 실행 정책 조회.
- 검토한 새 환경 PS1의 AST 파싱과 목록 조사 실행: 구문 오류 0, exit 0, issues 없음. 결과 상세는 ENVIRONMENT.
- 이미 연결된 `unityMCP` 리소스: custom-tools, instances, project/info, editor/state.
- 실제 읽기 도구: `manage_scene(action=get_active)`. 대상 경로·버전·활성 씬을 확인했고 변경 도구는 호출하지 않음.

샌드박스 프로세스 목록의 Unity 미발견과 MCP의 인스턴스 1개 응답을 함께 기록했다.
새 연결/서버 시작 없이 기존 연결을 읽었다. 과거 로그·캡처를 이번 실행 성공으로 해석하지 않았다.

## 5. 공식 문서 — 이번에 직접 조회한 것

- [O-03 AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md): 전역/프로젝트 지침과 새 실행 시 확인.
- [O-05 MCP](https://learn.chatgpt.com/docs/extend/mcp?surface=cli): 사용자/프로젝트 설정 범위와 클라이언트 공유.
- [O-07 Config basics](https://learn.chatgpt.com/docs/config-file/config-basic): 신뢰된 프로젝트 설정·사용자 설정·실행 옵션·정책 관계.
- [U-01 Unity 6000.3 command-line reference](https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html):
  static executeMethod와 같은 프로젝트 Editor/배치 중복 실행 제한.

OpenAI Docs 스킬은 설정 적용 범위 해석에만 사용했다. 설치·연결 지시를 실행하지 않았다.
Unity Test Framework 1.6/1.6.0 온라인 명령행 문서는 조회 실패하여, 실제 설치된 공식 1.6.0 패키지 소스를 근거로 초안을 작성했다.
시작 자료의 다른 추천 링크를 이번에 전부 재확인했다고 주장하지 않는다.

## 6. 참고 커밋과 웹 주소의 한계

자료가 지정한 저장소: [renewal_CursorDash](https://github.com/edan2251/renewal_CursorDash).
자료의 참고 커밋: `413f8427293c66e275963172ecfdada8a3c0a8e2`.
자료가 전한 [웹 배포 주소](https://edan2251.github.io/renewal_CursorDash/)는 이번에 열거나 검수하지 않았다.
현재 원격 main·배포 내용·동기화 여부를 확인하지 않았으며 이 주소들은 출처 보존용이다.
