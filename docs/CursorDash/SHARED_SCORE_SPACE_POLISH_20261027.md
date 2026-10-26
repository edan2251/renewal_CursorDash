# D-053 — 공유 점수·등장 박자·공간 피격·회로 흐름

2026-10-27. **로컬 구현 및 집중 Unity 검사 완료 / 사용자 시각·조작 검수 대기.** 빌드·커밋·푸시·배포하지 않았다. 다음 한 작업은 사용자 Cyberpunk 검수다.

## 최신 결정과 보존

- 출처: 최신 사용자 「현재 수정본을 기준으로 다음 네 가지」 및 073054 현재 결과 영상. **하나의 숫자 TMP가 최고점→표시용 0 전환→현재 점수를 담당**한다. D-052의 최고점/현재점 별도 텍스트 교차·카운트다운 금지는 이번 명시 결정으로 대체한다. 실제 최고점·현재 점수 데이터와 보상 이벤트는 감소시키지 않는다.
- 피격의 붉은 선 장식만 제거한다. 득점용 흰 플래시·공격선·회로는 보존한다. 실제 넥서스 좌표에서 배경/회로 텍스처를 눌렀다가 바깥으로 풀며, HUD와 점수 획은 보호한다.
- 링 선행→본체 성장/한 번 안착→실제 커서 가속 출현, 평상시 회로의 국소 흐름만 보완한다. 회로 경로·20/15px 폭·기본 대비/색·밀도와 일반 공격/노드/파편·피버/저장 규칙은 유지한다.
- 육각 타이밍은 **수정·재검토·관련 검사에서 제외**했다. 일반 노드 겹침, 새 Flash Background 그림, 별도의 사이버 공간 재구축은 계속 보류한다. 이번 기존 시작 회로 걷힘/재구축은 승인된 흐름의 유지다.
- 기존 Unity 한 인스턴스를 사용했다. 사용자 미저장 상태를 저장·폐기·초기화·재임포트하지 않았으며 D-014를 확대하지 않았다. 이후 Unity/Play·빌드/Git 게시의 무제한 승인이 아니다.

## 현재 구조와 원인별 수정

### 실제 HP 피해: 붉은 선 대신 평면 플래시 + 공간 변형

실제 Cyberpunk 씬의 `CameraFlash.flashImage`는 `Flash/FlashImage`를 가리킨다. Sprite GUID `5af4148e1d2c44c78b822b07b0cb022c`는 **`Cursor-Dash-master/Assets/Art/Backgrounds/Cyberpunk/Remake/Cyberpunk_Background_Flash.png`**였다. 이미지를 직접 열어 회로 그림이 들어 있음을 확인했다. 해당 이미지 전체를 붉게 착색한 것이 붉은 선 장식의 소스였다. 기존 동적 회로나 공격선에서 지운 것이 아니다.

`CameraFlash.ConfirmedDamage`에서만 기존 Flash Canvas의 Image 옆에 **sprite 없는 평면 Image 한 개를 재사용**한다. 범위는 기존 화면 Canvas 전체, raycast OFF, DontSave다. 성공 `FlashCore`와 원본 PNG/참조는 그대로다. 피해가 진행 중이면 성공 플래시가 덮지 않으며, 재피격은 기존 피해 Tween을 교체한다. Pause/Reset/disable/destroy는 양쪽을 정리한다. Flash Canvas는 기존 Screen Space Camera/UI 카메라·sorting1 경로를 사용한다.

기존 Built-in 게임판 최종 합성 `FeverImpact.shader`에 실제 UV 샘플링 변형을 보완했다. 패키지·Volume·카메라를 추가하지 않았다. `StageManager`의 실제 HP 감소→기존 확정피해 전달→`FeverWorldTransition` 경로는 그대로다. `_NexusDamage.xy`는 실제 NexusPosition의 카메라 투영이며 화면 중앙 상수가 아니다.

- 초반 중심 주변 압축 → 한 번의 바깥 진행 변위 → 감쇠. 색 선/원형 geometry를 그리지 않고 원래 게임판 텍스처를 변형한다. 반복 사인 진동·화면 전체 배율 확대가 아니다.
- 기존 단일 최강 필드 선택/변위 .018·RGB .006 상한을 유지했다. 수위/본체 근처는 보호하고, 같은 실제 점수 획 마스크로 글자를 보호한다. 고정 HUD는 이후 UI 합성으로 남는다.
- 일반 피해 파동과 최종 파괴를 구분한다. 최종 파괴에 새 바깥 파동을 추가하지 않는다. 승인된 최종 파편/슬로모션은 변경하지 않았다.
- `OnFlash`, `user.flash`, `fx.enabled`, 왜곡·흔들림 감소 설정의 기존 경로를 존중한다. 실제 접근성 전체 조합의 육안 검수는 남아 있다.

피격 충전 원호의 `NexusGaugeRewind`는 이번에 다시 작성하지 않았다. 실제 Gauge/배율 초기화는 즉시, **표시만 .16초 역감소**한다. 매 Tick의 새 실제 목표를 사용하며 완료 콜백으로 옛 값을 복원하지 않는다. 재피격·감소 중 충전/Pause 순수 검사와 실제 Play 내부호출 검사를 통과했다. 피버 시간 소진 종료와 별개다.

### 하나의 Projected total score

기존 `FeverBackgroundScore`의 **Projected total score / TextMeshPro 하나**를 재사용한다. 별도 `Shared-space best score` numeric renderer/재질 생성은 없앴다. 작은 BEST 안내는 같은 숫자 주변의 보조 UGUI 글자이며 시작 초반에 사라진다.

- 현재 RectTransform은 **world 단위 4×4 / Transform scale(1,1,1)**, TMP AutoSize **Min1 / Max3.5**다. 기존 이사만루 Bold/아틀라스/재질을 유지한다. Height2.5는 검토 제안이므로 2.5배나 확정 Rect 변경으로 적용하지 않았다.
- 메인 숫자는 기존 숫자 메시의 **.82배 시각 크기**, 흰색 alpha .85, 실제 NexusPosition에서 위로 .28 world다. 로고는 원본 비율을 유지해 기준 폭780→840, y anchor .76→.79로 조정하고 실제 Canvas 폭의86%로 제한한다.
- 시작 때 같은 Renderer를 기존 인게임 위치·색/alpha로 이동시킨다. 표시용 BigInteger countdown은 고정 시간에 유효 자릿수도 줄이며 0에 수렴한다. 0 최고점은 그대로 0이다. `PointsAwarded`, 배율, Gauge나 PlayerPrefs를 호출하지 않는다.
- 메인에서는 AutoSize로 한 번 맞추고 긴 최고점은 안전 폭에 추가 fit한다. 숫자가 바뀌는 동안 **AutoSize를 잠시 끄고 계산된 font size를 고정**한다. 0이 된 후 정상 폰트 크기까지 부드럽게 복원하고 플레이에 인계하며 AutoSize를 다시 켠다. Transform scale은 항상1이다.
- 회로 가림은 이 **같은 TMP의 실제 현재 메시/UV/아틀라스/변환**만 읽는다. 이동·자리수 감소·확대에서도 별도 숫자 계산/가림 텍스트를 만들지 않는다. 획 구멍·숫자 사이 공간을 남기며 점수 알파와 가림 강도는 별개다.
- 실제 득점 시 정확한 현재 총점과 기존 pulse/색/위치는 그대로다. 기존 상단 점수 총합은 숨긴 상태, ×2~×4 배율은 유지한다.

### 링·본체·실제 커서의 한 타임라인

기존 `CursorDashStartTimeline`과 `CursorDashSpaceFlow`만 확장했다. 별도 관리 시스템·커서 복사본은 없다. 본체/링의 entry scale/alpha는 기존 Nexus presentation의 시각 속성으로 분리했으며 실제 HP/공격 경계/충돌은 바꾸지 않는다.

기본 전체 **1.35초**, 빠른 Retry/명시 훈련 **.24초**는 유지했다. 아래는 기본값의 **계산된 배치 시간**이지 각 캡처 프레임에서 실측한 시간이 아니다.

|요소|기본 구간/동작|
|---|---|
|기존 회로|0~.378초 후퇴 후 .378~1.107초 구축, 동시 겹침 없음|
|로고/문구|기존 오른쪽 가속 이동·fade, 문구 fade 유지|
|점수 표시 숫자|.108~1.053초 안에 표시만 0으로 수렴|
|점수 위치/색|.135~1.107초 같은 메시의 이동/변색|
|점수 font 복원|1.080~1.323초, 자릿수 변화가 끝난 뒤 smooth 복원|
|점선 링|.432초부터 선행 출현·빠른 회전, 본체보다 .135초 먼저 시작|
|본체|.567초 성장 시작→약 .839초에 최대1.045→.945초에1로 한 번 안착|
|커서|.864~1.242초 실제 머리가 중심에서 .95world 이동, 처음 .068초 가속 후 감속|
|플레이 인계|1.35초 준비 완료 **및 시작 입력 해제 뒤** 실제 시간·공격·입력 시작|

출발 방향은 기존 시작 입력을 Nexus 기준 world 방향으로 변환하고 입력 방향이 없으면 위쪽을 사용한다. 끝점은 기존 이동 여백/clamp 규칙을 재사용한다. 같은 실제 커서에 기존 entry trail을 연결하고 끝점에서 조작으로 넘긴다. 시작 클릭 중복/누른 채 대기/상호작용 UI 입력 소유 규칙은 보존한다. 실제 손 조작 인계는 사용자 검수 대기다.

### 회로 평상시 흐름

기존 flow가 활성화되어 있었지만 첫 대기2.4초와 짧은 감쇠 거리 때문에 경로를 흐르기 전에 약해졌다. 기존 pulse pool/명시 graph 연결 전달을 재사용하고 **평상시 flow만** 별도 앞/뒤 envelope와 경로 길이 기준 감쇠를 사용한다. 실제 타격의 기존 `Advance/Level` 구현은 동일하다.

첫 대기 .65초, 출발부근에서 시작하는 날카로운 앞 .025/부드러운 꼬리 .20의 유한 강조다. speed .24에 각 pulse별 .8~1.2배 속도차, interval2.6초에 제한된 출발 간격차, 같은 시작 경로 연속 선택 방지, 소수 묶음 뒤 조용한 구간을 둔다. 동시2/하드3, 연결 분기 .70 전달, 비연결 경로0, 밝기는 max로 합성한다. main과play 모두 advance하며 Pause는 진행을 멈춘다.

**12묶음/82선·주20/보조15px·기본 색/alpha/밀도는 그대로다.** 흐르는 부분만 별도 강조색/강도로 보인다. 같은 회로 메시/획 마스크·visible prefix 안에서 그리므로 피버 퇴장/사망으로 걷힌 구간이나 숫자 뒤에서 강조만 다시 나타나지 않는다. 회로·재질을 매 타격/프레임 새로 생성하지 않는다.

## 조절 위치와 전후 값

기존 **`Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`** Inspector를 사용한다. 별도 도구·프리셋·자동 저장 프리셋 변경은 없다. Shared space 값은 다음 시작에서 캡처되는 timeline 값과 즉시 표시 값으로 나뉜다. 세부 기본식은 기존 같은 파일의 timeline이다.

|분류|필드|이전 → 이번 값 / 의미|
|---|---|---|
|시작|spaceStartSeconds / spaceNexusAt / spaceCursorAt|1.35 / .42 / .64 유지, 초 / 전체진행 비율|
|링 선행|spaceRingLead|신규 .10, 전체진행 비율 = 기본 .135초|
|본체 안착|spaceBodyOvershoot|신규 .045, 한 번 최대4.5% 초과|
|커서 가속|spaceCursorAcceleration|신규 .18, 커서 출현 이동 중 초기 가속 비율|
|링 회전|spaceRingBoost / spaceRingDecay|신규780 / .30초, 기존 bounded ring pulse의 최대 가속/감쇠|
|메인 숫자|spaceMainScoreScale / Opacity / Lift|신규 .82 / .85 / .28world|
|로고|spaceLogoWidth / spaceLogoY|780/.76 → 840/.79, canvas 기준 폭/anchor|
|피해 플래시|nexusDamageSeconds / Flash / Tint|.24초 / .26 / (1,.04,.08,1) **유지**, sprite 없는 화면 wash|
|피해 RGB|nexusDamageChromatic|.0055 유지, 최종 상한 .006|
|공간 왜곡|nexusDamageWaveStrength / Width / Speed / Range|.008 유지 / **.075→.12** / 2.6 유지 / .72 유지|
|압축·해소|nexusDamageCompression / ReleasePower|신규 .85 / 1.4, 중심 압축/감쇠 곡선|
|표시 원호 손실|nexusGaugeRewindSeconds|.16초 유지, 다음 실제 피격|
|평상시 flow|circuitAmbientSpeed / Interval / Strength|.24 유지 / **3.4→2.6초** / **.055→.10**|
|flow 윤곽|circuitAmbientTail / Head|신규 .20 / .025, 경로 정규화 거리|
|동시 flow|circuitAmbientLimit|2 유지 / 하드3|

기존 asset의 직렬화 값은 Interval/Strength/WaveWidth **3개만** 바뀌었다. 새 필드는14개다. 옛 `spaceBestScale/Opacity`는 호환 보존/숨김/미사용이며 새 요구로 부활시키지 않는다. 접근성/사용자 효과 강도 설정은 프로필의 상위 제한으로 유지한다.

## 이번 변경 파일

Unity 루트 `D:/renewal_CursorDash/Cursor-Dash-master/` 기준 **기존15파일만** 변경했다. 이번 범위 외 기존 사용자 변경은 그대로다.

|파일|역할|
|---|---|
|Assets/Scripts/Gameplay/StageManager/CameraFlash.cs|확정 피해만 plain flash, 성공 원본 보존|
|Assets/Scripts/Gameplay/StageManager/CursorDashStartTimeline.cs|ring/body/cursor 비선형 진행·표시 전용 countdown|
|Assets/Scripts/Gameplay/StageManager/CursorDashSpaceFlow.cs|숫자 하나 공유·BEST/로고·등장 인계|
|Assets/Scripts/Gameplay/StageManager/NexusGameboardPresentation.cs|entry 본체/링의 시각 scale/alpha 분리|
|Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs|기존 projected TMP의 best→current 전환·font 잠금/복원|
|Assets/Scripts/Gameplay/StageManager/CursorDashCircuits.cs|단일 실제 글자 마스크·quiet flow 연결|
|Assets/Scripts/Gameplay/StageManager/CircuitPathGraph.cs|기존 graph 보존, ambient envelope/출발/휴지기|
|Assets/Scripts/Gameplay/StageManager/FeverWorldTransition.cs|실제 Nexus damage shape/text mask·entry 표시 연결|
|Assets/Resources/CursorDash/FeverImpact.shader|게임판 픽셀의 압축/이동 파동·글자/본체 보호|
|Assets/Scripts/Gameplay/StageManager/FeverFlowMath.cs|기존 bounded ring에 출현 전용 pulse 진입점만 추가|
|Assets/Scripts/Gameplay/StageManager/FeverNexusRig.cs|같은 ring entry pulse 전달|
|Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs|같은 ring entry pulse 전달|
|Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs|위14필드/기존 기본값3개|
|Assets/Resources/CursorDash/VisualPrototype.asset|같은 프로필의 기본값|
|Assets/Tests/Editor/CursorDashSpaceFlowTests.cs|표시/실제 shader/Play 집중 회귀·캡처·정상 TestRunner 메뉴|

문서: AGENTS.md, STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES/TUNING_GUIDE와 이 인계 파일. 새 런타임 에셋·패키지·씬·prefab·meta·ProjectSettings 변경은 없다. 기존 Git 변경 목록에 있는 Cyberpunk 씬 등은 이번 변경과 구분한다.

## 실행한 검사와 한계

모든 근거는 Unity 루트 **`Logs/space-polish-20261027`**에 있다. Logs는 공개 대상이 아니고 미추적 소스에 추가하지 않는다.

|상태|검사|실제 확인 범위|
|---|---|---|
|PASS|대상 native Unity 컴파일|6000.3.23f1 / DX11. 최종 DLL 08:08:48KST가 최종 소스보다 새로움, Console 오류0. 참조 컴파일과 별개|
|PASS|정상 TestRunner 집중 **30/30**|최종 JSON/XML 완료 08:09:52KST, 실패/skip0. 회로15·타임라인/표시/실GPU11·공유씬 통합4. 전체 테스트 실행 주장은 아님|
|PASS|실제 GPU sampling 검사|비중앙(.37,.56) 입력의 texture pixel 이동, 흰 글자 마스크가 해당 영역을 보호. native DX11이며 모바일/WebGL shader 검증 아님|
|PASS|실제 Play + 내부 호출|Main 대기3.2초/flow활성·점수/생존0, 시작1회/같은 Renderer/표시 감소·실제0, 실제 HP감소/red/원호역감소/재충전, Pause/사망/result/Retry/Home, fever진입→Gauge0→종료중Pause정지→복귀/배율2 한 번|
|PASS|기록 보호|가짜20자리 최고점은 presentation 메모리만 변경. 실제 저장키 유지 단언. 시험 전후 읽기진단 prefs 동일. 정상 최고점 저장·재실행 유지 검증을 대신하지 않음|
|PASS|참조 C# compile|runtime112/165참조, tests14/실제csproj271참조, editor2; 모두 오류0. 런타임 기존134경고, 테스트/Editor0. 게임 어셈블리 직접 참조를 테스트에 추가하지 않음|
|PASS|생산 소스 순수 .NET|4,668,342단언. same timeline·1~300자리 countdown/0·단계/상한·원호live목표/재피격·기존graph/분기/퇴장/flow. Unity/GPU/물리 증거와 별개|
|PASS|파일/Editor 보존|시작1880파일 해시·HEAD/main/빈index·기존 미추적·원본 PNG/scene/meta/승인 코드 보존. 종료 Play OFF·컴파일 OFF·timeScale1·Cyberpunk dirty=false|
|NOT_RUN|실제 마우스/터치 플레이·모바일·WebGL/성능·재실행 저장|자동 입력으로 일반 전투를 플레이하지 않았고 디바이스/프레임 성능 측정 없음. 내부 UI/Review 함수 호출을 실제 손 조작으로 보고하지 않음|
|NOT_RUN|전체 전투 회귀·감소 설정 전체 조합·전체 화면비 시각|허용 집중 범위만 실행. 육각 시점 검사 제외. 기본 native 1080×1920 시각 표본, 여러 비율은 순수/graph 검사|

집중 실행 첫30PASS 후 실제 긴 최고점 캡처에서 자릿수가 늦게 줄어드는 표현을 보완했고, **최종 동일30개를 다시 실행**했다. 예외/실패를 무시하거나 tests를 삭제해 통과시키지 않았다. 최종 `native-result.json/xml`, 첫 결과 `native-result-first.*`, 최종 source 해시 `native-proof.json`, `pure-result.json`, `Compile/*result.json`을 분리 보존한다. 결과 duration 합계는 domain reload 때문에 실제 테스트 대기시간/성능 지표로 쓰지 않는다.

시작·종료 읽기진단 `editor-before/after.json`에서 **Assets imported dirty6개가 동일**하다. TMP shader3, LiberationSans/이사만루 Bold·Medium의 Font Texture3이며 저장/폐기하지 않았다. 씬은 전후 clean이다. 기존 외부 백업 `D:/renewal_CursorDash/Backups/space-feedback-20261027` 34파일은 유지했고, 이번 시작1880파일 manifest/소스·meta 사본은 `Logs/space-polish-20261027/Before`에 있다. 디스크 백업이 메모리 dirty 상태를 직렬화했다는 뜻은 아니다.

## 실제 화면 근거와 참고 영상 범위

073054 파일은 전달 경로의 형제 폴더가 아니라 **`D:/renewal_CursorDash/_incoming/references/녹음 2026-10-27 073054.mp4`**(4,487,070bytes)에 있었다. 기존 Media Player에서 제목과 약13초 길이/0·4·8·9·11초 부근 표본 및 짧은 재생을 실제 봤다. 현재 결과의 문제 자료로만 사용했다. 전체 프레임 정밀 분석/오디오 검수로 보고하지 않는다.

이번 새 화면은 실제 Unity `ScreenCapture`의 **NativeCaptures PNG 24장**이다. `review-captures.png`는 이 표본을 묶은 비교판이며 **연속 녹화 영상이 아니다**. 내부 호출/가짜 presentation best20자리·기록 제외 검수 상태임을 구분한다.

- `00-main-long-best` / `01-main-flow`: 로고·BEST·한 숫자·원본 회로 구도. 긴 숫자 안전 폭 확인.
- `start-00/02/04/06/08/10/12/14/16/18`: 같은 숫자 자릿수 감소/변색, ring/body 출현, 실제 커서 등장 표본. 숫자 두 Renderer 겹침 없음.
- `02-charged-internal-model`, `hit-00~05`: 충전 후 실제 HP 감소 함수 호출, 붉은 회로 artwork 없는 wash/색수차, 원호 역감소와 정상화 표본.
- `03-result` / `04-home`: 사망/결과·동일 씬 복귀. 기록 제외 이유 표시와 저장키 보존.
- `05-fever` / `06-fever-exit` / `07-color-restored`: 흰 공간/검은 도형과 종료 복원 표본. 승인된 피버 체감 전체를 재승인한 것은 아님.

## 사용자 수동 검수 순서

**열 씬: `Cursor-Dash-master/Assets/Scenes/Cyberpunk.unity`.** 기존 다른 씬은 삭제하지 않았다. 도구 자동 테스트가 아니라 평소 Play/기존 훈련·수동 검수 기능을 사용한다. 검수용 최고점 덮어쓰기/초기화는 하지 않는다.

1. Main에서 5~10초 대기: 차분한 기본 회로 위에 몇 경로만 느리게 흐르고 휴지가 있는지, 로고·BEST·최고점 하나가 읽히는지 확인한다.
2. 클릭 한 번/연타/길게 누르기: 하나의 숫자가 커지고 이동·변색·빠른 자릿수 감소로0이 되는지, ring→body→중심에서 나오는 실제 커서가 읽히는지 본다. 준비 전 적 공격/HP손실/실제 점수·생존 진행이 없어야 한다. 시작 입력을 놓은 뒤 기존 커서 조작에 연결되어야 한다.
3. 실제 득점→Gauge 충전: 흰 성공 플래시는 기존대로, 수위 손실은 없어야 한다. 숫자 획 안 회로/흐름 가림과 0/8의 구멍을 비교한다.
4. 실제 넥서스 피해: 훈련 무적을 해제하고 공격을 맞거나 기존 수동 검수의 실제 피해를 사용한다. 붉은 선 artwork 없이 붉은 wash/RGB·Nexus 위치에서 짧은 눌림/전파, HP 즉시감소와 Gauge 표시 역감소를 구분한다. 감소 중 득점/연속 피격에서 옛 Gauge가 되살아나지 않아야 한다.
5. Pause/복귀·체력0·Retry·Home: 피해광학/Flash가 남지 않는지, Retry 빠른 준비/HP/입력과 Main의 최고점 하나/회로 흐름이 복원되는지 확인한다. 정상 기록 대상 플레이 후 최고점 저장은 별도 확인한다.
6. 기존 피버 진입→공급/파편→게이지0→링/노드/공간 종료 중 Pause→복귀→일반색/링: 점수 획 가림/회로게이지 후퇴와 기존 종료에 회귀가 없는지 본다. 연출 중 Retry/Home도 확인한다.
7. 실제 모바일·다른 화면비·효과 감소/0: 글자 fit·HUD 대비·출현 조작·왜곡 강도와 성능을 최종 검수한다. 이번 native 통과만으로 모바일 통과를 판단하지 않는다.

## 자체 QA — 다음 제안만, 추가 구현 없음

1. **P1 / 미검증 위험:** 테스트는 실제 Play/렌더를 사용했지만 시작·피해 재현은 내부 함수다. 실제 터치/마우스 홀드 해제→빠른 조작 인계, 다양한 화면비와 효과 감소 조합이 우선이다. 입력감/기기 문제의 존재를 확정한 것은 아니다.
2. **P2 / 관찰된 시각 잔여:** `03-result.png`에서 결과의 현재 점수 부근에 커서 머리가 남아 겹쳐 읽힌다. 이번 공유 숫자/피격 수정과 별개인 기존 결과 표시 경로이므로 결과창 커서 표시 정책을 다음 검수 후보로 남긴다. 점수/결과 디자인을 이번에 임의 변경하지 않았다.
3. **P3 / 디자인 검수 후보:** 실제 `01-main-flow`·회로 표본에서 일부 끝의 강조가 쐐기처럼 읽힌다. 기본 폭/밀도를 다시 키우기보다 사용자 영상에서 흐르는 앞/꼬리의 선명도·끝 인상을 먼저 판단하는 편이 낫다. 이번 authored graph/끝 geometry는 보존했고 새 디자인을 추가하지 않았다.

남은 작업은 위 사용자 검수다. 추가 기능·배포·수치 조정은 자동 시작하지 않는다.
