# CursorDash 노드 그래픽·방향 파괴·타격감 시안 (D-018)

진행 상태: **구현·Editor 검증·로컬 WebGL 검증 완료 / 사용자 시각·실기기 검수 대기**. [같은 PC에서 시안 열기](http://127.0.0.1:8879/). 최종 buildId `nodefx-20261024-a4818b5046e2`. 커밋/푸시/공개 배포는 하지 않았다. 과거 공개 CP14에는 이번 변경이 없다.

## A. 구현과 남은 범위

원형 Preparing 충전/회전 테두리를 선명한 단색 도형으로 교체했다. 기존 원→중간→꼬리 파인 커서의 정점/곡선·별도 반동·본체 돌진·앞점 연속 충돌 판정은 유지했다. 최종 커서만.60배이며 .15/.05/.20초를 유지한다.
육각은 여섯 꼭짓점을 유지하는 모서리 절단 도형이다. 세 짧은 변이0이 되지 않고 앞 짧은 변의 중점이 넥서스를 향한다. 중앙 확대 충전, 테두리 흡수, 발사 중 뒤→앞 직선 경계 소모, 에너지와 독립된 중앙 코어/외곽선을 분리했다.
원형 제거는 실제 접촉 직전 이동 방향(짧은 최근 유효 방향/접촉 관계 보완), 육각 파괴는 돌아오는 빔 방향으로 불규칙 단색 다각형/테두리 조각을 방출한다. 짧은 접촉 섬광과 시각 압축 후 흩어지며 파괴/반환 판정은 지연하지 않는다. 원형 접촉의 기존 셸·파티클·별도 연출 중복을 제거했다. 넥서스 돌진 충돌의 기존 연출은 유지했다.
성공 전용 방향 킥/되튕김과 넓은 실제 화면 왜곡을 별도 시안에 연결했다. UI 카메라는 안정된 상태를 유지한다. 강도/지속/동시 상한/정지 최소 진행 정책과 사용자 효과 배율을 유지한다. 육각 방어와 넥서스 효과는 기존 값/경로다.
남음: 실제 휴대폰 터치/발열/장시간 성능·사운드 실청·사용자 미적 승인, 모바일 보고서 복사 권한/붙여넣기 확인. Windows 보고서 복사 비교는 아래 후속 승인 검증으로 해소했다. 과거 USER_VERIFIED를 새 기능의 승인으로 재사용하지 않는다.

## B. 파일과 편집 가능한 원본

| 파일(프로젝트 Assets 기준) | 역할 |
|---|---|
| Scripts/Gameplay/Object/Node/NodeFlatGraphic.cs (신규), Resources/CursorDash/NodeFlat.shader (신규) | 해상도 기반 AA 도형, 코어·외곽선·충전·피스톤·회전 테두리 |
| Scripts/Gameplay/Object/Node/NormalNode.cs | 도형 연결, 접촉 연출 통합, 현재 실루엣 파편; 공격 정점/타이밍 유지 |
| Scripts/Gameplay/Object/Node/ArrowNode.cs | 6각 변형·발사구·소모; 논리 빔과 시각 선 분리 |
| Scripts/Gameplay/Object/Character/PlayerCharacterController.cs | 실제 이동 방향 읽기 전용 추적·접촉 전달; 이동 계산 불변 |
| Scripts/Gameplay/StageManager/StageManager.cs, HitFeedback.cs, ImpactFragments.cs | 단일 방향 사건/파편 전달·수명 |
| Scripts/Gameplay/StageManager/CameraShake.cs, LocalImpactDistortion.cs, Resources/CursorDash/LocalImpact.shader | 성공 전용 방향 킥/넓은 픽셀 왜곡·가장자리 복귀; 기존 넥서스 분기 유지 |
| Scripts/Gameplay/StageManager/CursorDashTuning.cs, CursorDashRuntimeUI.cs | 16개 시각 키, 호환 처리, 내장 시안 버튼 |
| WebGLTemplates/CursorDash/TemplateData/cursordash-tools.js | 웹 시안 버튼; 기존 자동 생성 수치/JSON/보고서 UI 재사용 |
| Resources/CursorDash/BuildIdentity.txt | 새 로컬 빌드 식별자 |
| Tests/Editor/CursorDashSceneRegression.cs, CursorDash.EditorTests.asmdef | 옛 삼각형/중복 셸 기대를 새 외형으로 변경, 기존 설치 JSON 라이브러리 참조 |
| Tests/Editor/CursorDashGraphicsComparison.cs, CursorDashGraphicsTests.cs (신규) | 실제 시퀀스 캡처·기하/방향/픽셀/호환 회귀 |
| Editor/CursorDashGraphicsWorkbench.cs (신규) | 읽기 전용 상태 기록·기존 로컬 빌드 함수 호출 메뉴 |

신규 Unity 파일에는 대응.meta를 유지한다. 원본 PNG와.meta, 기존 씬/프리팹을 이번 변경으로 덮어쓰지 않았다. 그래픽 원본은 위 C#/셰이더이며 외부 서비스/새 패키지를 쓰지 않았다. 실행 시 생성한 Mesh/Material만 해당 오브젝트가 소유하고 파괴 시 정리한다. 도형용 사각형은 실제 반경에 맞춰 기존 허용 범위 .1/.8/3에서 잘림 없이 확장되며, 에너지0의 코어/외곽선도 유지한다. 관련 문서는 AGENTS/STATUS/MASTER_SPEC/DECISIONS/VALIDATION/ENVIRONMENT/SOURCES/TUNING_GUIDE다. 최종 실제 변경 목록은 작업 근거 폴더의 `changed-files.txt`/`final-state-comparison.json`을 따른다.

## C. 흐림 근거와 조정

실제 원형 외곽 이미지190×188, 부분 알파17,772/35,720픽셀,526종 RGBA를 확인해 원본의 부드러운 알파 전이를 확인했다. 작은 크기로 보일 때 발광과 경계가 섞이는 모습도 변경 전 화면에서 관찰했다. mipmap off/filterMode -1/textureCompression1은 임포터 설정이며 현재 GPU 압축·필터별 흐림 기여율까지 측정한 것은 아니다.
이번에는 해당 이미지를 런타임 렌더에서 제외하고 fwidth AA 도형을 사용했다. 기본 발광0이며 약한 발광은 별도 시각 값이다. 전역 Point/압축/렌더 품질·배경/UI 디자인을 바꾸지 않았다.

## D. 규칙과 테스트

공식 테스트 항목별 최신 결과 **32 PASS / 0 FAIL / 0 NOT_RUN**(기존27+신규5). 단일32개 올패스 실행으로 표현하지 않는다. 전체 Run-03은31PASS/1FAIL, 해당 실패는 기존 JSON의.45초 실험값을 시각 프리셋이 유지했는데 테스트가.20으로 잘못 단정한 사례였다. 테스트를 강화해 모든 비시각 키 보존을 검사하고 기본 세션을 명시적으로 분리했다. 이후 관련3개 Run-04가3PASS다. Run-05는 셰이더 정수 연산 경고의 동등 분기 교체 후1PASS, Run-06은 도형 반경별 잘림 방지 보완 후 영향받는 GPU/기하2PASS다. 각 실행의 새 native JSON/XML 이름/결과와 Editor 상태를 교차 확인했다. `verified-latest-cases.json`의 Run-03/04/05/06 최신32개 집계이며 수동/클립보드 검증은 이 수에 포함하지 않는다.

| 새 검증 | 결과 / 핵심 근거 |
|---|---|
| Graphics_ComparisonSequence | PASS, 실제 기본 공격 시계/접촉 물리/잔상 빔의 전후 연속 화면·CSV |
| Graphics_SixConvexVerticesPistonAndFourOutletsKeepBeamRules | PASS, 변형101지점의 볼록6각/비영 변/외접 반경,4방향 발사구·논리 시작점, 소모·코어·피해1회 |
| Graphics_DirectionalSingleContactAndCleanupKeepUserPause | PASS,4방향 평균 파편 속도·사건1회·중복 셸off·사용자 정지 보존·동적 오브젝트 수 복원 |
| Graphics_BroadShaderBendsRealPixelsAndEdgesRecoverWithoutMovingUI | PASS, 접촉점에서 먼 실제 GPU 샘플 변위·가장자리 동일성, 모서리 연속 타격 상한/복귀·UI 카메라 고정 |
| Graphics_Legacy141FullImportAndNewPresetRoundtripAreAtomic | PASS, 정확한141키 보완/누락 거부/원자성, 비시각 키 모두 유지,157키 왕복과.20초 새 예시 |

기존 검증은 원형 충전/변형/반동 제거, Fired 잔상 무시/본체 돌진/레이저off/앞점 피해1회, pause/Retry/Home/풀 복원, 육각 반환 도착 파괴/.06초/방어 폭/피해 취소, 이동/잔상/훈련·가이드/기록 제외/사용자 설정을 포함한다. 공격 검사 단언을 느슨하게 하거나 삭제하지 않았다. 변경한 옛 시각 기대는 삼각형→6변, 선의 시각 시작점→앞 변 중점, 중복 셸→단일 파편 사건이다.
시작 저장값: 원형2.5/.15/.05/.20초·.60배, 육각3.5/.25/.25/.06초·파괴 후1.333초·외접 반경.8. 넥서스 기존 본체 경계는1080×1920 테스트에서 반경.3255208로 유지됐다. 논리 빔 origin/root·hitWidth/경계는 바꾸지 않았다.
시작 캡처 테스트의 초기 비활성 노드 Awake 전 Reset 호출 실패, 임시 편집 중 컴파일 오류, 새 테스트의 DontSave 검색 방식/호환 목록 조기 고정/실험값 오해도 로그에 남겼고 수정 후 검증했다. 실패 이력은 삭제하지 않았다.

## E. 실제 비교 화면

[원형 공격 GIF](evidence/nodefx-20261024/circle-attack-comparison.gif) · [원형 접촉 파괴 GIF](evidence/nodefx-20261024/circle-contact-comparison.gif) · [육각 반환 파괴 GIF](evidence/nodefx-20261024/hex-return-comparison.gif).
[변경 전 시트](evidence/nodefx-20261024/before-contact-sheet.jpg) · [변경 후 시트](evidence/nodefx-20261024/after-contact-sheet.jpg) · [형태 확대](evidence/nodefx-20261024/after-regression-shape-crops.jpg).

원본은 Editor Game1080×1920, 훈련 Cyberpunk, 노드(0,2). 원형 접촉은 플레이어(-2,2)에서 게임1.2초 뒤 MoveVector 오른쪽, 나머지는(-2,-3)에 둔다. 육각은 기존 RecordMovement로(-1,1)→(1,1) 잔상을 유지해 실제 방어 판정을 통과한다. 내부 입력/보조 재현이며 실제 마우스/휴대폰 터치로 집계하지 않는다. 기본 게임 배율1, 사용자 효과 배율1, 변경 전 기본 타격감/후 강렬한 프리셋이 의도적인 비교 차이다. Editor 오디오는 음소거이며 사운드 실청 증거가 아니다.
GIF는 최초 기록 사건 시점에 맞추고 실시간 타임스탬프 간격을 유지하며50ms마다 직전 캡처를 표시한다. PNG 저장 부하로 원본 간격이 약.08~.32초인 구간이 있어 고프레임 정상 속도 직접 녹화나 정확한 멈춤 측정과 동일하지 않다. 빠른 .06초 반환/반동은 코드·고프레임 공식 검증을 병행했다. 제공 영상1080×2340과 다른 해상도이므로 전후 비교는 같은 Editor 조건의 새 자료를 사용했다.

최종 실제 WebGL [육각 충전→피스톤→반환 파괴 GIF](evidence/nodefx-20261024/hex-complete-realtime.gif), [타임스탬프 시트](evidence/nodefx-20261024/hex-complete-sheet.jpg), [원형 접촉 파편 GIF](evidence/nodefx-20261024/circle-peak-realtime.gif), [시트](evidence/nodefx-20261024/circle-peak-sheet.jpg)도 캡처·열람했다. 새6각의 앞 짧은 변, 에너지 직선 소모, 원형과 반환 파편의 분리·복귀를 관찰했다. 훈련 보조 재현의 기존 플레이어와 노드가 겹친 프레임의 흰 커서는 중앙 코어가 아니라 플레이어다. 강한 효과라도 실시간 캡처가 최고점/히트스톱 모든 프레임을 포착했다고 주장하지 않는다. 전체1280×720 원본과 타임스탬프는 Logs의 Web에 유지하고, GIF는 좌우 여백만 잘라40ms 기준으로 직전 프레임을 유지한다. GPU 검증이 넓은 실제 픽셀 변위/화면 가장자리 복귀를 별도로 검사했다. 연속 타격은 공식 GPU/수명 검증으로 확인했으며 연속 수동 입력 영상은 미확보다.

## F. 타격감 프리셋

프로젝트 기본 효과 수치는 출발점 기준을 유지한다. **강렬한 노드 시안**은 Dev에서 명시적으로 선택하는 제안이며 사용자 저장 프리셋·공개 기본값을 자동 덮어쓰지 않는다.
원형/반환: 정지.04/.05→.075/.085초, 셰이크.055/.065→.15/.18월드, 지속.14→.22/.25초, 왜곡.008/.01→.055/.06UV, 반경.14→1.05/1.15짧은변, 지속.18→.26/.28초, 파편6/6→10/12개. 세부 범위·되튕김·회전·복귀값은 [수치 도구 안내](TUNING_GUIDE.md)를 따른다.
기존 방어0초/약한 효과, 넥서스0.08초/실행 왜곡.008은 유지한다. 넥서스 메타.006/실행.008의 이전 차이도 자동 수정하지 않았다. 기존 사운드 사건1회/볼륨.8·피치1을 유지하고 새 음원을 넣지 않았다. 현재 Editor 음량0/음소거로 실제 음질/클리핑은 미검증이다.

## G. JSON·보고서

[강렬한 시각46키 JSON](examples/nodefx-strong-visual.json) · [비교 기준46키 JSON](examples/nodefx-baseline-visual.json) · [실제157키 세션 변경 보고서](examples/nodefx-strong-report-ko.txt).
46키 예시는 실제 최종 런타임 내보내기에서 시각 키만 추린 changes JSON이다. **기존 가져오기는 full/changes 모두 Dev 세션을 교체한다.** 따라서 파일에 없는 공격/이동 Dev 실험값은 출하/일반 설정으로 돌아갈 수 있다. 사용자 저장소 자체를 덮어쓰지는 않는다. 다른 실험값을 유지하며 타격감만 비교하려면 두 **내장 시안 버튼**을 사용한다.157키 full은 현재 전체 세션 전달용이며 최종 웹 원본은 Logs의 `Web/strong-full.json`, 앞선 Editor 원본은 `Presets`에 있다. 한국어 보고서 예시는 최종 웹의 실제 생성 텍스트이며 기존 넥서스.006/.008 차이와 성공 시안 변경을 구분한다.
schema1/defaultsVersion 유지,141키 기존 full 정확 일치 시16키 보완, 그 외 누락/알 수 없는 값 거부. `fx.triangleBreak.*` 호환 키와 `hex.radius` 외접 반경 의미를 유지한다. 새157키/시각46키 프리셋은 아직 옛 공개 CP14에서 지원되지 않는다. 웹/Editor의 이름·단위·기본/요청/유효/범위/적용 시점을 공유한다.

## H. 로컬 웹·모바일·기존 이슈

최종 WebGL **Succeeded/errors0/warnings0**,79,632,633bytes/170.525초, 완료 UTC `2026-10-24T12:51:23.7696536Z`다. 기존 빌드는 덮어쓰지 않고 `Builds/LocalDevPreview/nodefx-20261024-a4818b5046e2`에 만들었다.120개 입력 해시가 현재 소스와 같고 실행19파일 HTTP200/WASM MIME을 확인했다. 초기 빌드의 셰이더 int 나머지 연산 경고2개를 동등 분기로 제거해 최종0개다.
웹 PASS: 기본 정규 게임(기록 가능)→훈련/Dev(기록 제외), 실제 보조 물리 원형 접촉/빔 방어·반환 파괴(각 사건1회·HP3/3 유지), 두 시안 전환·157키/.15/.20/.60 내보내기·46키 텍스트 가져오기, 별도 `NodeFX QA 20261024` 저장 후 새로고침/명시적 불러오기, 외곽선.05 요청/실제 적용 후.035 복원, 한국어 보고서 생성/전체 선택. Console warn/error0건이며 순간 관측135~179FPS는 장시간 성능 보증이 아니다.
[390×844 패널](evidence/nodefx-20261024/mobile-390.png)은 DOM/실제 캡처로 패널 폭380.4/가로 넘침 없음까지 확인했다. 같은 크기의 [육각 연속 화면](evidence/nodefx-20261024/mobile-hex-sheet.jpg)(177원본 프레임), [원형 접촉 화면](evidence/nodefx-20261024/mobile-circle-sheet.jpg)(27원본 프레임)도 실제 생성·열람했다. 외곽선/피스톤 경계와 방향 파편을 구분할 수 있었고 표본 프레임에서 검은 가장자리나 잔류 효과는 보이지 않았다. CSS390×844/실제 캡처높이843~844의 반올림 차이가 있으며 작은 프레임 전체가 모든 순간의 왜곡 최고점을 포착한 것은 아니다. 뷰포트는 종료 전에 복원했다. 현재 도구의 캔버스 드래그/홀드 입력은 지원되지 않아 이번 실제 마우스 이동·터치는 NOT_RUN이다. 기존 보조 재현/공식 이동 테스트를 실제 입력으로 표시하지 않는다. 실제 휴대폰 터치·사운드·키보드·이번 새 빌드 파일 선택/다운로드·발열/장시간 성능은 NOT_RUN이다.
보고서 복사 후속: **PASS(줄바꿈 정규화)**. 최초 UI의 `클립보드 복사 완료`와 브라우저 도구 읽기가 달랐고, Windows 추가 읽기를 자동 승인 검토가 사적 데이터 위험으로 거부한 BLOCKED 이력은 유지한다. 이후 사용자가 “보고서 일치 검사 허용”을 명시하고 조작 중지에 응답했다. 같은 로컬 시안 화면의 보고서를 다시 복사한 직후 정상 승인 경로로 Windows Get-Clipboard -Raw에서 비교해 exactMatch=false/newlineNormalizedMatch=true를 확인했다(UTC2026-10-24T13:54:14Z). LF/CRLF 외 문자열은 동일하며 클립보드 원문을 출력하거나 저장하지 않았다. 비교 대상 파일은 화면 DOM에서 읽은 게임 보고서이며 클립보드 덤프가 아니다. 근거 `Logs/node-graphics-20261024-192424/Clipboard-followup/comparison.json`/`copy-result.png`. 이 결과는 새 공식 테스트 수에 더하지 않고 게임 파일·수치·빌드도 바꾸지 않았다. 과거 공개 빌드의 복사 PASS를 이번 결과로 재사용하지 않는다.
새 기능의 사용자 미적 승인은 없다. 기존 Editor의 `Deleting invalid font reference.` 및 Editor 전용 미사용 필드 경고는 별도 이슈이며 이번에 폰트를 초기화/전체 저장하지 않았다. 기존 DOTween 집계26건은 과거 이력이고 이번 실행의 경고 수와 혼동하지 않는다. 이번 반복 검증에서 DOTween 집계는 관찰되지 않았으나 전체 프로젝트의 해결을 주장하지 않는다.
최종 읽기 전용 상태 `editor-state-20261024-131438.json`: 에셋 미저장은 D-014 imported4개만, Cyberpunk 씬clean/Playoff/compileoff/pauseoff/timeScale1이다. 저장·폐기·재시작·초기화하지 않았다. 최고기록/음량/v2 저장은 기존 fixture의 키별 백업·복원 경로를 유지했고 빌드 전후7키가 일치한다(최고36.93275/BGM0/SFX0, 나머지 없음). 과거 문서의 다른 최고기록으로 되돌리지 않는다. 시작 전 전체 PlayerPrefs 덤프는 없으므로 전 구간 원본 바이트 동일성을 주장하지 않는다.
main/HEAD8ceedbb 유지, 기존 변경20개/미추적294개와467개 보호 파일 중 누락0이다. 이번 게임/문서 변경은 `final-state-comparison.json`에 분리했다. 기존 씬/프리팹/설정/원본 이미지/.meta의 새 차이0, 고정 결과/캡처13개와 기존 로컬 빌드 결과 파일은 별도 백업 바이트로 복원했다. 최종 새 결과는 이번 Logs에 남겼다. 시작 이후 생긴 `Assets/_Recovery.meta`, `_Recovery/0.unity`, `0.unity.meta`는 원인 미확정으로 보존했다. 19:27:58 생성된0.unity는 기존 Cyberpunk와 SHA256이 같으며 로드/삭제/빌드 씬 등록하지 않았다. 다른 변경과 섞어서 정리하지 않았다.

## I. 사용자 시각 확인과 재개 지점

1. 휴대폰 화면 크기에서 충전 외곽선/코어가 선명하고, 원형의 기존 변형·반동·돌진 느낌이 유지되는지.
2. 육각 앞 짧은 변이 충분히 남고 커서와 구별되는지, 피스톤 경계가 반듯하며 코어가 중심에 남는지.
3. 원형은 진행 방향, 육각은 반환 입사 방향으로 깨지는지; 꽃잎/늦은 큰 가시가 사라졌는지.
4. 비교 기준↔강렬한 시안 전환 후 넓은 휨/멈춤/되튕김이 충분한지 또는 과한지. 가장자리/연속 타격에서 검은 여백·고착이 보이는지.
5. 사용자 효과0/감소 옵션, 실제 터치 취소/일시정지/재시도, 사운드 중복/클리핑·장시간 발열을 실기기에서 확인한다. 모바일 브라우저의 보고서 복사 권한과 시스템 붙여넣기도 확인한다.

다음 한 작업은 사용자 시각 검수와 시각46키 JSON/보고서 피드백이다. 기본값 승격/공개 배포/다음 기능은 별도 요청 후 진행한다.

전체 근거: `D:/renewal_CursorDash/Cursor-Dash-master/Logs/node-graphics-20261024-192424`.
