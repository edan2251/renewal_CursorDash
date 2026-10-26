# D-052 — 공유 공간 메인·피격 피드백·회로 흐름

2026-10-27. **IMPLEMENTED / 실제 Unity Play·GPU 검수 대기.** 출처는 최신 사용자 「피격 피드백·회로 흐름·단일 씬 메인/게임 전환 및 자체 QA」와 컴퓨터 사용 재요청이다. 기존 미커밋 회로 작업 위에 이어서 구현했다. 분석만으로 종료하지 않았으나, 아래 실행 제약 때문에 실제 화면 해결 확인은 선언하지 않는다.

## 보존과 실행 제약

- Git 루트 `D:/renewal_CursorDash`, Unity 루트 `Cursor-Dash-master`. main/HEAD `1d11743d9417597d009c1f92aecb430eb5180b37`, 빈 스테이징 유지. 새 커밋·기존 커밋 수정·push·빌드·배포 없음.
- 시작 1,872개 파일의 SHA256/변경 목록을 `Cursor-Dash-master/Logs/space-feedback-20261027/before.json`과 `git-status-before.txt`에 기록했다. 기존 미추적·사용자 씬 변경은 보존하며, 이번만의 전후 diff와 범위 검사는 같은 로그 폴더에 있다.
- **구조 수정 전 프로젝트 밖 복구 사본:** `D:/renewal_CursorDash/Backups/space-feedback-20261027`. 기존 `Assets/Scenes/`의 씬·meta 및 `ProjectSettings/` 파일 34개와 SHA256 manifest. 디스크 사본이며 미저장 메모리 보존을 대신하지 않는다. 새 백업 폴더 자체의 `.gitignore`로 공개/스테이징에서 제외했다.
- 기존 Unity 6000.3.23f1의 읽기 전용 상태 진단은 성공했다. 정확한 대상 프로젝트·Cyberpunk, Play 아님/컴파일 중 아님, 열린 씬 dirty=false였다. **미저장 imported Assets 6개:** TMP_SDF SSD/TMP_SDF/TMP_SDF Overlay Shader, LiberationSans.ttf Font Texture, esamanru Medium.ttf Font Texture, esamanru Bold.ttf Font Texture. 저장·폐기·재임포트하지 않았다. D-014 예외는 기존 imported 4개만이며 이사만루 2개로 확대하지 않는다. 중단 후 최종 메모리 상태는 다시 조회하지 못했다.
- 로컬 Unity 실행/Play는 이번 요청에서 허용됐지만, 사용자의 재시도 허용 후 새 컴퓨터 사용 호출도 **physical Escape stop**을 반환했다. 이후 컴퓨터/브라우저 입력을 중단했다. 다른 Unity를 종료하거나 새 인스턴스를 열지 않았고, Play·테스트 실행/씬 저장은 하지 않았다. 이 제약은 파일 구현/참조 검사와 구분한다.
- 영상 실제 경로는 `D:/renewal_CursorDash/_incoming/references/90_current_full_review.mp4`(18,105,166 bytes, SHA256 `76aa4167ea4fb7cb6ef2537994c6d49ec7f3299085f8b6aa1447dc769533ec13`)다. 전달된 형제 폴더 경로는 없었다. 루프백 브라우저 디코딩 재요청도 timeout, WPF 영상 읽기 스크립트는 기존 실행 정책에 막혔다. 정책 변경/우회는 하지 않았다. **영상 프레임·오디오는 읽지 못했다.** 사용자 설명과 실제 소스를 기준으로 구현했으며 영상 확인/수정 후 촬영을 주장하지 않는다. 직접 시작했던 임시 영상 서버 2개는 종료했다.

## 실제 구조와 변경

### 한 씬의 메인 → 시작 → 플레이

최종 실행/검수 씬은 **`Assets/Scenes/Cyberpunk.unity`**다. 기존에는 `01.StageSelectScene`의 PNG 메인에서 Cyberpunk를 LoadScene하고 Home에서 메인 씬으로 돌아갔다. 이제 기존 StageManager에 Main/Starting 상태만 추가하고, 같은 카메라·매니저·그라데이션·회로에서 메인과 플레이를 진행한다. Playing/Paused/Ended의 기존 enum 값은 유지했다.

- 시작 씬 목록은 Cyberpunk → 기존 01.StageSelectScene 순서로 변경했다. `00.StartScene`·Flame·Replay 등 기존 씬과 리소스는 삭제·합치지 않았다. **기존 legacy 메인을 직접 열 경우** 기존 선택/설정 화면이 남으며, Cyberpunk 시작만 공유 공간으로 연결된다. canonical Cyberpunk에서는 기존 메뉴 PNG/하단 버튼·곡선 패널을 생성하지 않는다. 설정/가이드/훈련 기능은 기존 런타임 도구로 유지한다.
- Cyberpunk에 직접 진입하면 Main. 기존 원본 로고 `Assets/Art/UI/Branding/Logo.png`(GUID `f8abf33bd82f6b742a3716e6bce57786`)·넥서스 위치의 최고점·아래쪽 안내만 실행 중 표시한다. 기존 씬의 실제 RemakeMenu/Image 참조를 확인해 재사용했다. 창작 로고나 새 PNG가 아니다.
- Main/Starting에서는 적/패턴·공격/피해·생존시간·피버 Model.Tick·점수 획득을 진행하지 않는다. 기존 배경과 회로 표현 시계만 unscaled로 이어 간다. 표시용 최고점은 기존 PointsRecord.Read를 읽고 카운트다운하지 않는다.
- 시작 입력은 1회만 수락한다. 기존 joystick의 Selectable/설정 입력 소유 규칙을 새 시작 경로에도 사용해, 배경 Image의 raycast만으로 시작을 막지 않는다. 기존 최초 가이드 여부를 유지하되 도구에서 명시적으로 선택한 훈련/가이드/정상모드는 그 선택을 보존한다. 클릭/탭 위치에서 실제 넥서스를 뺀 방향으로 **실제 커서 하나**가 중심에서 빠져나온다. 방향이 없으면 위쪽. 거리 .95 world, 기존 화면 여백 안으로 clamp한다. 기존 TrailRenderer로 짧은 등장 궤적만 그리며 방어 collider는 만들지 않는다.
- 이동/회전/충돌 수학은 보존한다. 등장에서만 Rigidbody 위치·대기 MovePosition을 함께 맞춰 오래된 이동 명령을 취소하고, FinishEntry에서 기존 궤적 샘플의 연결점을 현재 머리로 맞춘다. 입력 인계는 연출 완료 **및 시작 입력 해제 후**다. 누르고 있으면 중립 준비 상태에서 기다리며 유효 플레이 시간은 소모하지 않는다.
- Retry/직접 훈련 진입은 .24초 빠른 준비만 사용한다. 로고/최고점 전체 전환을 반복하지 않는다. Home은 같은 씬에서 실제 위협·피버/회수/최종파괴·결과·트윈/정지·입력을 정리하고 메인으로 돌아온다. 진행 중 설정 패널이 열리면 시작을 취소해 메인으로 정리한다. 지연 생성 콜백을 두지 않는다.
- 같은 씬의 재시도에 맞춰 기존 노드 생성·플레이어/잔상 설정과 패턴 선택을 새 라운드에 다시 읽는다. 기존 프리셋을 자동 수정하지 않는다. 캐시된 원래 직렬화 값을 fallback으로 쓰며 기존 공격/패턴 데이터를 다시 디자인하지 않는다.

### 기본 시작 타임라인

명목값이며 실제 Unity 측정값이 아니다. 같은 `CursorDashStartTimeline` 진행값 하나를 사용한다. 총 **1.35초**, fast Retry **.24초**. 시작 입력을 계속 누르고 있으면 인계까지의 실제 길이는 더 길어진다.

|항목|기본 구간(초)|연결|
|---|---|---|
|안내 페이드|0–.216|첫 입력 뒤 종료|
|로고 퇴장|0–.297|오른쪽 150 reference-UI units 가속+페이드|
|기존 회로 걷기|0–.378|원래 경로 prefix를 바깥으로 후퇴|
|회로 재구축|.378–1.107|완전 후퇴 뒤 같은 메시에서 바깥부터 펼침|
|최고점 확대 / 퇴장|.135–1.107 / .405–1.107|최대 1.5배, 대비 감소·현재0과 교차|
|실제 현재점수 0 등장|.675–1.161|보상/최고점 복사 없이 별도 기존 점수 표시|
|넥서스 등장|.567–.945|시각 크기만 확대, 점선 가속 1회|
|커서 등장|.864–1.242|넥서스→입력 방향, 머리 1개/기존 궤적|
|플레이 인계|1.35 이후 입력 해제|그때부터 패턴·피해·생존/점수 진행|

최고점은 4×4 Rect/AutoSize 1–3.5, 실제 TMP 폭/화면 safe area로 긴 자리수를 제한한다. 새 최고점 표시의 크기만 제한하며 승인된 플레이 배경 점수의 Rect4×4/Scale1·글자 펄스·색은 유지한다. 회로 획 마스크는 최고점/현재점수의 **실제 메시·아틀라스·변환**을 각각 읽고 전환 weight만 반영하므로 이전 숫자를 별도 계산/보상 모델로 복제하지 않는다. 가림 구멍·획 가장자리/GPU 동기화는 실기 미검수다.

### 실제 넥서스 피해

확정 경로는 `StageManager.DamageNexusContact → ScoreManager.Hp 감소 → Damaged → OnNexusHealthLost → Fever.NexusHit / HitFeedback.ConfirmedNexusDamage`다. 기존 공격별 한 번 피해 가드는 유지한다. NaN/Infinity/0 이하·비플레이/피버 억제·이미 HP0 피해를 거부한다. 같은 HP로 반복 setter 호출은 Damaged가 발생하지 않는다. 서로 다른 실제 연속 피해는 각각 유효하다.

- 성공 원형/육각 파괴의 기존 흰 플래시/승인 파편은 보존한다. 반사/점수 증가에서 붉은 신호를 호출하지 않는다. 훈련 무적의 가상 접촉도 실제 HP 감소가 없으므로 붉은 신호가 없다.
- 기존 CameraFlash의 같은 이미지/단일 fade를 재사용한다. 저장된 damagedColor가 보라색인 것을 확인했으며, 씬을 바꾸지 않고 새 피해 프로필의 붉은색을 전달한다. 기본 **RGB(1,.04,.08), opacity .26, .24초**. 해당 시간에는 성공 흰 플래시가 붉은 플래시를 덮지 않는다. 재피격은 교체하며 밝기를 합산하지 않는다.
- 실제 비치명 피해의 기존 흔들림은 .24초에 맞춘다. 기존 피해 강도/히트스톱·사운드·접촉 파편은 유지하며 user.flash/shake/distortion·OnFlash/OnVibe·fx.enabled를 존중한다.
- **왜곡 채택:** 기존 Built-in 최종 게임판 `FeverImpact` pass에서 실제 넥서스 본체 경계부터 바깥으로 이동하는 annulus를 사용한다. 별도 카메라·URP Volume·패키지·전역 zoom이 아니다. 게임판/넥서스/실제 커서 합성 뒤, 고정 HUD 카메라 앞에 적용한다.
- 기본 wave strength .008 / width .075 / speed2.6(short-screen-axis/sec) / max range .72 / chromatic .0055 / duration .24. wave range는 상한이고 기본 .24초 동안 전선 이동은 .624다. 변위 .018/RGB .006 hard cap과 기존 효과 중 가장 강한 필드 선택으로 누적을 제한한다. 피버 기본 RGB가 일반 모드로 새는 것도 차단한다. 실제 shader 지원/전파감·HUD 안정성은 아직 GPU 검수 전이다.
- 최종 피해는 StopGameplay 정리 후 확정 플래시/RGB를 한 번 호출한다. 승인된 최종 파괴의 기존 local distortion을 유지하고 **새 annulus는 추가하지 않아** 이중 왜곡을 피한다. 최종 파편·슬로모션·방향/결과 전환은 바꾸지 않는다.
- Pause/Retry/Home/사망 정리에서 ClearDamage/FlashReset 등 기존 소유 경로로 제거한다. 기록상 API의 옛 public 인자 수는 유지하고 새 `...Committed` 경로를 구분했다.

### 게이지 되감기와 회로 흐름

- 실제 FeverState.NexusHit의 Gauge/배율 초기화는 즉시 그대로다. 표시 원호만 기본 **.16초 역방향**으로 감소한다. 매 프레임 실제 새 충전값을 읽어 반복 피격/새 충전 시 이전 0 목표가 뒤늦게 덮지 않는다. 표시로 피버/보상을 발동하지 않는다. Pause는 정지, hitstop 동안 짧은 시각 되감기는 unscaled로 진행한다. 기존 피버 시간 소모/종료는 변경하지 않는다.
- 회로 그래프 **12묶음/82선·주20/보조15px·색/기본 대비**는 그대로다. 평상시 흐름의 시작 간격 **4.5→3.4초**, 추가 강도 **.025→.055**만 조정했다. 속도 .24 / 동시2(하드3), 실제 타격 전달1.65 / 강도 .18 / 감쇠 .26 등은 유지한다. 평상시 흐름의 분기 감쇠와 휴지기·상한을 재사용한다.
- 같은 onPreCull 마스크로 기본선/흐름/타격 강조를 가린다. 피버 실제 Gauge에 따른 바깥 후퇴·Gauge0 제거, 기존 일반 공간 복원과 게임오버 후퇴는 보존한다. 사라진 구간에서 flow가 선을 되살리지 않는다. 메인·시작의 회로 걷기/구축만 새 상태 front로 제어한다.

## 최고점 조사

`CursorDashPointsRecord` 저장/키/계산은 변경하지 않았다. 정상 라운드는 같은 map의 점수용 키로 저장하며 예전 생존시간 기록과 분리된다. 기존 **가이드·훈련·개발자/검수 조작은 기록 제외** 규칙을 유지한다. 결과창에 제외 사유(또는 기존 저장 오류)를 표시하도록 보완했고, 메인은 기존 PointsRecord.Read로 재조회한다. 최초 자동 가이드 정책도 그대로다.

영상은 읽지 못했고, 사용자 데이터·실제 재실행 저장을 검사하거나 초기화하지 않았다. 따라서 **‘영상의36점/최고0 = 특정 모드/저장 결함’으로 확정하지 않는다.** 정상 모드·개발자 OFF·검수 조작 없이 종료→최고 갱신→Home→재실행 유지 검수가 남았다. 실제 오류가 있으면 그 조건/LastError를 기준으로 다음 최소 수정을 판단한다.

## 조절 위치

기존 **`Assets/Resources/CursorDash/VisualPrototype.asset`**의 `Shared main / gameplay space`, `Confirmed Nexus health loss`, `Dynamic circuits`를 선택한다. 새 별도 수치 도구/프리셋 체계는 만들지 않았다. Play 중 프로필 편집의 영구 저장 여부는 Unity asset 저장 방식이며 기존 사용자 프리셋을 자동 덮어쓰지 않는다.

|기존 프로필 필드|기본값 / 적용|
|---|---|
|spaceStartSeconds|1.35초; 다음 시작 때 캡처|
|spaceRetreatEnd / spaceRebuildEnd|.28 / .82; 총 길이에 대한 비율, 다음 시작|
|spaceNexusAt / spaceCursorAt|.42 / .64; 총 길이에 대한 시작 비율, 다음 시작|
|spaceCursorDistance / spaceBestScale / spaceBestOpacity|.95 world / 1.5 / .52; 시작 캡처, Main opacity는 즉시|
|mainLogo|기존 실제 로고 Sprite 참조|
|nexusGaugeRewindSeconds|.16초; 다음 피격|
|nexusDamageSeconds / Flash / Tint|.24 / .26 / (1,.04,.08,1); 다음 실제 피해|
|nexusDamageChromatic|.0055; 다음 실제 피해, GPU 상한 .006|
|nexusDamageWaveEnabled / Strength / Width / Speed / Range|true / .008 / .075 / 2.6 / .72; 다음 실제 피해|
|circuitAmbientInterval / Strength|4.5→3.4초 / .025→.055; 즉시(활성 흐름을 무한 누적하지 않음)|

`Tools → CursorDash → 넥서스 수동 검수`의 기존 창에 공유 씬 상태/생존초·최고/현재 점수·시작 진행/직전 길이·되감기·피해 광학 진행/중심을 읽는 항목만 추가했다. 사용자가 Play할 때 사용하는 창이며 자동 Play/저장을 호출하지 않는다. 피해 버튼은 훈련에서 무적을 해제해야 작동한다. 본 창의 기존 육각 항목·타이밍은 변경/재검토하지 않았다.

## 변경 파일 (이번 시작 대비)

기존 변경27파일 + 새 소스/검증3개와 meta3개다. 아래 경로는 Unity 루트 기준이다. 기존 사용자 씬·프리팹·meta는 바꾸지 않았다.

- 신규: `Assets/Scripts/Gameplay/StageManager/CursorDashSpaceFlow.cs`, `CursorDashStartTimeline.cs`, `Assets/Tests/Editor/CursorDashSpaceFlowTests.cs` 및 각각 새 meta.
- 상태/연결: `Assets/Scripts/Gameplay/StageManager/StageManager.cs`, `CursorDashSession.cs`, `CursorDashRuntimeUI.cs`, `CursorDashFever.cs`, `PatternReader.cs`, `NodeCreator.cs`, `Assets/Scripts/Menu/StageSelectSceneManager/MainSceneController.cs`, `ProjectSettings/EditorBuildSettings.asset`.
- 표시/피드백: 같은 StageManager 폴더의 `CameraFlash.cs`, `HitFeedback.cs`, `LocalImpactDistortion.cs`, `FeverWorldTransition.cs`, `FeverGaugeView.cs`, `FeverScoreHud.cs`, `CursorDashCircuits.cs`, `CursorDashLivingBackground.cs`, `CursorDashTypography.cs`, `DeathUIController.cs`.
- 시작 커서: `Assets/Scripts/Gameplay/Object/Character/PlayerCharacterController.cs`, `PlayerCharacterAfterImage.cs`.
- 설정/셰이더: `Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`, `Assets/Resources/CursorDash/VisualPrototype.asset`, `FeverImpact.shader`, `CircuitScoreGlyphMask.shader`.
- 검수/회귀: `Assets/Editor/CursorDashNexusReview.cs`, `Assets/Tests/Editor/CursorDashSceneRegression.cs`, `CursorDashEventVfxTests.cs`.
- 문서: 루트 AGENTS와 STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES/TUNING_GUIDE/본 인계.

## 검사 결과

|구분|상태·실제 범위|
|---|---|
|정적·보존|**PASS** 시작1,872파일/HEAD·브랜치·빈 index·허용 변경/새 GUID·34백업 해시. 승인 노드/공격/피버 모델·파편·최종VFX·PointsRecord 소스, 커서 이동 수학·기존 CreateNode/배경 팔레트 등 보존. 이번만의 diff 별도 저장|
|참조 C# 컴파일|**PASS** 게임112소스/165참조·테스트14파일/실제271참조·Editor2소스 오류0. 런타임 기존134경고, 테스트/Editor0. 테스트 asmdef에 게임 직접 참조를 추가하지 않음|
|실제 순수 .NET|**PASS 4,655,320단언** 생산 시작 clock/반복·건너뛴 프레임·구간 인계/되감기·새 충전/기존 회로 그래프·전달/게이지 후퇴/flow·상한. Unity 프레임/Physics/입력·GPU 증거가 아님|
|공식 회귀 테스트|**NOT_RUN** 신규 수학6사례+공유씬/명시훈련/실제피해3사례 작성·컴파일만. 기존 시작/Home 기대를 공유씬 상태로 수정했고 핵심 단언/승인VFX 검사를 삭제하거나 Ignore하지 않음. dirty씬 guard는 preference 변경 전, 미캡처 teardown은 이전값을 복원하지 않도록 보강|
|Unity native compile|**NOT_RUN** 기존 DLL 타임스탬프가 새 소스보다 이전. 읽은 로그 끝에 오류가 없다는 사실만으로 새 native compile PASS로 하지 않음|
|실제 Play·화면·GPU/물리·실입력|**BLOCKED** 재승인 뒤 도구가 사용자 Escape 중단 반환. 자동 Play/테스트·저장·캡처 없음|
|90 영상/짧은 결과 영상|**NOT_RUN** 정상 디코딩·녹화 못함. 파일 해시/존재 확인은 관람 증거가 아님|
|실제 최고점 저장·재실행/모바일/WebGL 성능|**NOT_RUN** 사용자 기록/설정을 테스트 목적으로 변경하지 않음|
|빌드/커밋/push/배포|**NOT_RUN** 이번 금지 범위|

참조 검사 로그: `Logs/space-feedback-20261027/Compile/{result,tests-result,editor-result}.json`. 순수 결과 `pure-result.json`, 보존/이번만의diff `preservation-final.json`/`this-task-only.patch`, 최초 Editor `editor-before.json`, 영상 `video-review-status.json`. 과거 테스트/USER_VERIFIED는 이번 native 실행 결과로 재사용하지 않는다.

## 사용자가 확인할 순서

1. Unity에서 `Assets/Scenes/Cyberpunk.unity`를 연다. 미저장 작업이 있으면 먼저 사용자 판단으로 보호한다. 컴파일/Console 오류를 확인한 뒤 Play. **메인:** 원본 로고·넥서스 위치의 최고점·안내 밝기/회로 흐름, 플레이 시간0/노드0/실충전0. 기존 설정 접근과 저장값 보존 확인.
2. 클릭/탭 1회와 연속 클릭/길게 누르기. 로고 오른쪽 퇴장·회로 완전후퇴 뒤 재구축·최고점 확대/현재0의 겹침·넥서스+점선·중심에서 나온 실제 커서 연결을 확인한다. `시작 진행/직전 길이`로1.35초(+입력 해제 대기)를 비교하고 준비 전 공격/피해/생존시간이 없는지 확인. 최고점0/여러 자리에서도 현재 점수는0이어야 한다. 기록을 만들기 위해 기존 값을 삭제/초기화하지 않는다.
3. **성공/피해 비교:** 일반 노드 성공·반사는 흰 플래시. 훈련에서 무적 OFF 후 실제 원형/육각 공격을 넥서스에 맞혀 붉은 플래시·RGB/중심→바깥 전선·원래색 복귀를 확인한다. HP 변동 없는 가상접촉/반사/무효 피해에는 붉은 신호 없음. 효과 강도0/낮음에서도 입력/HUD가 유지되어야 함.
4. **게이지:** 중간/만충 직전 실제 피해 → 즉시 실제0/배율1, 표시만.16초 역방향 감소. 감소 중 새 충전·연속 피해로 이전 목표에 되돌아가지 않는지 비교. 검수창 `피격 초기화만`은 내부 모델 검수이며 실제 공격 확인과 구분한다.
5. **회로:** 정상 크기에서 소수의 느린 국소 flow/휴지기 → 실제 타격의 빠른 전달 → 점수 획 가림/구멍 → 기존 피버 구축·게이지 감소 후퇴/Gauge0 → 기존 노드/공간/링 종료/복원. 기본 구도·두께/점수/팔레트는 그대로여야 한다.
6. **반복:** Main→Start→Play→Pause/Resume→Result→Retry(.24초)→Home→Start. 시작 중 설정 열기/연타, 피격 중 Pause/Retry/Home, 피버 중/최종 사망 뒤 Home까지 확인. 같은 Stage/camera 하나·timeScale/input/효과·링/회로·설정 재적용·결과 지연 재등장 없음 확인. 훈련/가이드 진입도 기존 기능 검수.
7. **기록:** 개발자 OFF·훈련/가이드 제외의 정상 플레이에서 실제 점수 획득/사망 → 결과의 최고점·Home 최고점·사용자 재실행 유지. 제외 모드는 결과 이유가 표시돼야 하며 기록 규칙을 바꾸지 않는다. 모바일/다중 화면비에서 숫자 폭·HUD/커서 순서·wave/flow·성능은 별도 실기 검수한다.

## 자체 QA — 다음 후보만, 추가 구현 없음

1. **P1 / 미검증 위험:** 새 공유씬 시작·Rigidbody/Trail 인계는 소스/수학 검사만 완료했다. 실제 입력/FixedUpdate·GPU 순서와 첫 프레임을 볼 수 없어 위치 점프·중복 머리·일시정지 반복에 대한 native QA가 최우선이다. 관찰된 게임 버그로 단정하지 않는다.
2. **P2 / 미측정 위험:** 기존 회로/획/최종 합성에 피해 annulus가 연결된다. 새 피해용 RT/카메라는 없지만 mobile/WebGL 다중 RT·SDF 가장자리/긴 점수·색수차 대비는 측정하지 않았다. 여러 비율/낮은 효과 강도와 프레임 비용을 실기 측정할 필요가 있다.
3. **P3 / 디자인 제안:** 기존 최초 자동 가이드·개발자 모드의 기록 제외가 최고점0의 원인일 수 있으나 영상 사례는 미확정이다. 이번에는 결과 이유만 노출했다. 필요하면 추후 시작 전에 기록 대상 여부를 더 명확히 알릴 수 있으나 새 UI는 임의 추가하지 않았다.

**다음 한 작업:** 사용자의 실제 Unity 공유 공간/피격·되감기/회로·기록 검수. 새 기능·일반 노드 겹침·육각 타이밍 재검토·새 Flash 그림/무관한 사이버 재구축·빌드/Git 게시로 진행하지 않는다. 이번 Unity QA 승인도 다음 작업의 무제한 승인으로 확대하지 않는다.
