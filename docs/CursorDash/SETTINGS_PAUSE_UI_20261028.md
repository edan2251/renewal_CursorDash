# 메인 설정·일시정지 UI — D-065 (2026-10-28)

## 최신 후속 — 패널 입력·슬라이더 수정 (2026-10-28)

**UI_INPUT_FIX / NATIVE_COMPILE_PASS / FOCUSED_NATIVE_PLAY_PASS / 사용자 최종 시각·손조작 검수 대기.** 아래 최초 NOT_RUN은 이전 중단 이력이며 이번 실행 결과로 대체한다. UI 전 기준 main `bded6a4`와 기존 미커밋/미추적을 보존하고 빌드·커밋·push·배포는 하지 않았다.

### 원인과 최소 수정

1. `Build()`가 Safe area를 먼저, Menu shade를 나중에 생성해 전체 화면 shade가 조작 요소보다 앞에 그려지고 raycast도 먼저 받았다. 패널이 열리거나 Pause가 되면 shade.raycastTarget이 켜져 모든 조작을 가로챘다. **같은 Canvas에서 Menu shade를 첫 번째 형제로 배치**해 게임판은 막되 패널의 버튼·슬라이더·손잡이는 앞에서 동작하게 했다. 전투 입력을 막는 규칙/실제 일시정지 소유권은 유지한다.
2. Unity Slider는 손잡이의 세로 anchor를 0~1로 구동한다. 이전 Handle area 높이42 + sizeDelta14가 합쳐져 실제56 높이가 됐다. 이동 영역의 세로 크기를0으로 두고 실제 손잡이12×12로 고정했다.
3. 막대의 Track/Fill 2→4 UI단위(1080×1920 기준 실제8px), 트랙 RGBA(.32,.43,.50,.60)→(.36,.47,.54,.82). 손잡이 폭8→12/실제 높이56→12, 터치 영역 높이42→48로 분리했다. 레이아웃/글꼴/색계열/음량·효과 기본값 변경 없음. 모바일 실제 물리 크기는 미측정이다.

### 검사와 실제 확인 범위

- 참조 컴파일 런타임118/테스트17/Editor2 오류0, 기존 런타임 경고133. 실제 대상 Unity6000.3.23f1/DX11 새 게임·테스트 DLL 확인/Console 컴파일 오류0.
- 정상 `Tools → CursorDash Remake → Run Settings UI Focused`: 04:44:49 KST **5 PASS / 2 FAIL**. 메인 실제 화면좌표 EventSystem.RaycastAll의 최상단 hit가 7개 슬라이더/두 버튼/닫기손잡이인지 확인하고 이벤트를 전달했다. 이전의 대상함수 직접호출만 하는 검사에 있던 가림 검출 공백을 보완했다.
- 두 FAIL은 검사 준비 결함이었다. 일반 비Dev 라운드에서 `SpawnPreviewAt`가 정상적으로 거부한 것→기존 `CreateNode`로 노드를 준비해 동일 Pause/좌표/상태 단언을 유지했다. EditMode enumerator에서 `CaptureScreenshotAsTexture`를 프레임 밖에 호출한 것→고유 파일명으로 실제 Game View PNG를 비동기 캡처한 뒤 픽셀을 검사하도록 수정했다. 일반 전투·생성 정책·핵심 단언 수정/삭제 없음.
- 그 두 항목만 정상 `Run Settings Pause Recheck`로 재실행: **04:46:47 KST 2 PASS / 0 FAIL**. 게임 코드는 첫 실행과 동일하다. 최종 집계는 고유7항목 모두PASS(5+2 재검사)이며 한 실행에서7PASS였다고 혼합 기록하지 않는다. 최초 JSON/XML·실패 내용도 보존했다.
- 확인: 메인 열기/거리·속도 정착/중간취소·닫기 입력차단, 모든 조작 위치의 가림 순서, 실제 Slider 포인터 위치31%와 표시값/해제저장, 손잡이12×12·바4·터치48, 음량의 한 번 곱/사건음량 보존, 완화0→50→100→0/세부값 보존, 선택적초기화의 기록 보존, Pause 정지상태에서 두 슬라이더 조절·실제 버튼 이벤트 Continue/Retry/Home, 튜토리얼 재보기/기존 실제2+2→본게임/첫피버 순서, 완화100·flash0에서도 흰 안내 GPU픽셀 유지, 첫안내 고유시간 정지로 복귀.
- 실제 Game View PNG 39장(메인/일시정지/열림닫힘·복귀 표본/첫안내)을 읽었다. 연속 동영상이나 실기기 터치 검수가 아니다. 내부 좌표 raycast·UI 이벤트 전달/실제 물리 회귀를 OS 손조작과 구분한다.
- 추가 OS 마우스 확인을 시작하려던 키 입력은 `user input was detected`로 전송되지 않았다. 재관찰에서 사용자가 다른 앱을 사용 중인 것을 확인해 포커스를 다시 빼앗지 않고 컴퓨터 조작을 반환했다. 새로운 Play는 시작하지 않았다.
- 종료: PlayOFF/timeScale1/Cyberpunkdirty=false, 보호6키 hash동일/imported Shader3·Font Texture3 dirty 동일. 사용자 저장·폐기/다른 프로젝트·앱 종료 없음. 대상 Unity 조작 종료/반환을 사용자에게 즉시 알린 뒤 문서만 정리했다.
- 미검증: OS 게임 마우스·키보드의 전체 수동 흐름, 실제 터치·모바일/WebGL/성능/앱 재실행 저장/다른 화면비, 전체 테스트 모음. 이번 통과를 이 범위로 확대하지 않는다.

### 변경 파일·근거·다음 한 작업

이번 후속에서 바꾼 게임 소스는 `Assets/Scripts/Gameplay/StageManager/CursorDashSettingsUI.cs` 하나다. `Assets/Tests/Editor/CursorDashSettingsTests.cs`에 실제 raycast 순서·손잡이/바 크기·Pause 버튼 검사를 보완했고 `CursorDashTestReport.cs`에 필요한2개 재검수 메뉴와 이번 로그 목적지만 연결했다. 문서는 이 인계/STATUS/MASTER_SPEC/DECISIONS/VALIDATION/AGENTS를 갱신한다. 다른 게임 파일·씬·프리팹·설정·패키지·기존 사용자 파일은 이번 시작 해시와 동일하다.

사본 `Backups/settings-input-fix-20261028`. 증거 `Cursor-Dash-master/Logs/settings-input-fix-20261028/final-summary.json`, `NativeRun1/CursorDash-regression.json|xml`, 최종 `CursorDash-regression.json|xml`, native-before/after, editor-before, Compile/, Frames/. 검수용 수치가 보이는 PNG는 테스트 fixture의 값이며 사용자 저장값을 배포 기본값으로 바꾼 증거가 아니다.

**다음 한 작업:** 사용자 Cyberpunk에서 메인 손잡이→슬라이더·닫기→새 입력으로 시작→Pause의계속하기/Retry/Home을 직접 확인한다. 일시정지에서 전투가 멈추는 것은 의도이며, UI가 반응하고 계속하기 후 복귀하는지를 확인한다. 컴퓨터 조작은 이미 반환했고 새 요청 없이 추가 조작하지 않는다. UI·문서 미커밋 유지/빌드·push·배포 없음.

---

아래는 최초 구현·중단 이력이다.


## 현재 상태

구현은 로컬 파일에 반영했고 사용자 검수 대기다. **REFERENCE_COMPILE PASS / NATIVE_COMPILE NOT_RUN / PLAY NOT_RUN / VISUAL NOT_RUN.** CP7 컴퓨터 조작 시작 때 도구가 `Computer Use was stopped by the user with the physical Escape key`를 반환했다. 이후 UI 조작·Unity 실행·Play는 중단했다. 기존 DLL 날짜는 04:02:16 KST로 새 UI 컴파일의 증거가 아니다. 테스트를 작성·참조 컴파일한 것을 실행 통과로 집계하지 않는다. 실제 화면·마우스·터치·모바일·WebGL·성능·앱 재시작 저장은 미검증이다. 빌드·배포는 수행하지 않았다.

## CP0 보존과 푸시

UI 수정 전 완성된 D-061~064의 관련 소스·GuideInk·VisualPrototype·테스트·문서 **67개**만 선별했다. 비밀정보 패턴 검사 발견 0, diff check PASS. 출발 main8d26194 / 원격 동일 / 기존 미푸시 0 / 빈 index였다.

- 기준 커밋: `bded6a49a0292bb49302d6d59e4045e70d54bec5`
- 메시지: `chore: checkpoint gameplay before settings and pause UI`
- 기존 origin `https://github.com/edan2251/renewal_CursorDash.git`, source main에 정상 push 성공. 원격 main 같은 SHA, ahead/behind 0/0 확인 **후** UI 수정 시작.
- gh-pages `de9ec5fae6394f2dc64c36166206667b69f4ec53` 변경 없음. Pages 배포 요청/빌드 없음.
- 작업 전 사본: `Backups/settings-pause-20261028/checkpoint/`. 파일·해시·Git 전후 증거: `Cursor-Dash-master/Logs/settings-pause-20261028/`.
- 기존 미추적 백업·영상·Recovery·LegacyAssetArchive·tools·배포/증거 폴더 제외·보존. 사용자 파일 저장/폐기/삭제 없음.
- 시작 읽기 진단: Play OFF / 컴파일 중 아님 / Cyberpunk.unity dirty=false. 기존 imported Shader3/Font Texture3의 dirty는 그대로 보존 대상이며 전체 저장·discard하지 않았다. D-014 범위 확대 없음.

## CP1 방향과 참고

시안 이미지는 미전달로 사용자 확인. 텍스트 구조를 기준으로 기존 이사만루체·남색/청록/보라 배경에 흰 글자·청록 포인트, 얇은 선과 작은 손잡이, 넓은 실제 조작 영역을 사용한다. 로고·게임 배경·넥서스 재디자인 없음.

- [Returnal UI/UX 개발자 설명](https://blog.playstation.com/2021/05/11/unpacking-returnals-ux-design-gameplay-first-ui-retro-futuristic-tech-and-accessibility/): 기능 중심·최소 메뉴·청록 강조·정보 위계 참고. 메뉴를 없애는 구조, CRT 깜빡임, 대규모 카테고리는 채택하지 않음.
- [Returnal 설정 공개 화면](https://www.notebookcheck.net/Returnal-review-Laptop-and-desktop-benchmarks.700344.0.html): 검색 이미지의 어두운 면·흰 항목과 수치 열 정렬을 참고.
- [Rez Infinite 컨트롤 설정 화면](https://www.shacknews.com/article/134261/psvr2-rez-infinite-impressions): 검색 이미지에서 얇고 간결한 목록/여백만 참고. 노란 색·와이어프레임 장식은 채택하지 않음.

기사 본문·이미지 검색 결과를 확인했다. 일부 원본 이미지 직접 열기는 cache miss/403으로 실패했으며 전환 애니메이션을 관찰했다고 주장하지 않는다. 패널 모션은 현재 요청을 기준으로 설계했다. 외부 이미지·폰트·에셋 다운로드/편입 없음. 현재 게임 위에서의 최종 대비는 실제 화면 검수 전이다.

## CP2~6 로컬 구현

`CursorDashSettingsUI`는 Stage 아래 한 개의 화면 컴포넌트다. 새로운 설정/점수/시간 관리자를 만들지 않고 CursorDashTuning/Storage, Stage.SetPaused, Space.Retry/Home/LaunchSelectedMode를 사용한다.

- 메인: 하단 손잡이 클릭·드래그, 거리/속도 정착, 약 .24초 무반동 슬라이드, 7개 슬라이더, 튜토리얼부터 시작/선택적 초기화. 손잡이만 패널 드래그를 소유한다. 슬라이더는 별도 가로 조절이며 SafeArea/폭에 맞춰 내용 크기를 맞춘다. 패널/닫기 입력과 해제 대기는 게임 시작/조작 입력에서 분리한다.
- 일시정지: 기존 우상단 버튼/Stage pause 경로 유지, Esc 처리 통합. 약 .16초 중앙 메뉴와 어두운 면, 계속하기/전체 음량/전체 VFX 완화/다시 시작/메인으로. 빈 배경 클릭으로 재개하지 않는다. 기존 legacy Pause 화면만 숨기고 콜백/설정/리소스는 보존한다.
- 음량: 기존 BGM/SFX 실제 AudioSource 출력에 master를 한 번 적용한다. 사건별 SFX 배율도 보존한다. 기본 master=1이라 기존 음량을 변경하지 않는다. BGM은 원래 PlayBGM이 비활성인 상태를 유지하므로 배경음 슬라이더의 실제 음원 청취는 미검증/현재 무음 경로다.
- VFX: `CursorDashTuning.EffectGain(detail) = detail × (1 - reduction)`. 기존 shake/flash/distortion 읽기 지점에 적용. 왜곡에는 RGB 포함. 세부 사용자 값은 곱한 결과로 덮어쓰지 않는다. 기존 상한·OnFlash/OnVibe·fx.enabled 유지. 필수 공격/체력/게이지/궤적/노드와 흑백 상태 마스크·안내 글자는 이 감소율로 제거하지 않는다. 평상시 회로/기본 회로는 삭제하지 않고 타격 강조 경로만 감소율을 적용한다. 원래 파편의 크기·분포는 변경하지 않는다.
- 저장: 기존 `CursorDash.v2.user` JSON을 공유한다. master1 / vfxReduction0 새 키, 기존 bgm/sfx .5·shake/flash/distortion1. 기존 값/legacy 오디오 fallback 유지. 조작 중 즉시 미리보기, 포인터 해제·키보드 조절 완료·닫기/이탈에서 저장한다. 프레임별 저장 없음. reset은 이 7개 값만 초기화하며 조작/최고점/안내 완료/프리셋 키는 지우지 않는다. 옛 171-key full JSON도 정확한 전체 집합이면 새 두 키를 기본값으로 보충해 읽는다.
- 재보기: 기존 실행 중 ReplayTutorial 플래그와 Tutorial 모드 경로를 사용한다. 완료 기록 삭제 없이 시트 닫힘·입력 해제 후 한 번만 진입한다.

## 검사·다음 한 작업

참조 컴파일: 런타임 118소스 / 기존 Unity 참조165, 테스트17소스 / 게임 어셈블리 직접 참조 없이 기존 generated test csproj, Editor2소스 모두 오류0. 런타임 기존 직렬화 경고는 로그에 유지. 실제 Unity native compile/Shader/GPU/물리 검증과 구분한다. `git diff --check` PASS.

정상 TestRunner 메뉴 `Tools → CursorDash Remake → Run Settings UI Focused`를 추가했다. 신규 4개 UI 통합 검사와 기존 튜토리얼 실제2+2·첫 피버·독립 pause 회귀 3개를 요청하도록 작성했으나 **실행 NOT_RUN**이다. 새 결과 JSON/XML이나 수정 후 캡처는 아직 없다. 새 UI를 완성 검증 상태로 기록하지 않는다.

다음 한 작업은 대상 Unity의 미저장 상태를 다시 읽고, 새 컴파일 확인 → 위 집중 검사 → 실제 메인/일시정지 화면 대비·슬라이더·Esc·드래그 검수다. 사용자 중단 후 재개 지시 전 컴퓨터 조작을 반복하지 않는다. 실패 시 핵심 단언을 제거하지 않고 원인/수정을 구분한다.

수동 순서: 메인 손잡이 탭·드래그/중간 취소/연타 → 7개 설정/닫기 클릭 시작 차단 → 플레이/Pause/두 슬라이더 동기화·재개 → 완화0→50→100→0 및 세부값 보존 → 초기화의 기록 보존 → 튜토리얼 재보기2+2/첫 피버 흰 안내(플래시0) → 피버·슬로 구간Pause/Retry/Home → 결과 대기/메인. 실기기 터치와 재실행 저장, 모바일 화면비, WebGL 성능은 별도 검수다.

## 권한·보존

UI 작업 이후는 전부 미커밋이다. UI 커밋/push·빌드·배포 승인 없음. 육각 타이밍/전투/공급/점수/배율/피버8초·승인 파편·고스트/첫 안내 순서·결과 대기는 보존 범위. 일반 노드 겹침/새 Flash 그림/별도 사이버 재구축은 계속 보류한다.

## 이번 파일 목록

- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CircularBreakVfx.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashCircuits.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashRuntimeUI.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashTuning.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverBonusBurst.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverWorldTransition.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/HitFeedback.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/ImpactFragments.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/LocalImpactDistortion.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NexusImpactVfx.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/OriginalBackgroundFlash.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/StageManager.cs`
- `Cursor-Dash-master/Assets/Scripts/Services/GameManager/SoundManager.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashTestReport.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashSettingsUI.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashSettingsUI.cs.meta`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSettingsTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSettingsTests.cs.meta`