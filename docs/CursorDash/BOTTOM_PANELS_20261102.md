# D-077 메인 하단 랭킹·설정 패널 개편 (2026-11-02)

## 최신 보완 — D-079 / 반대 버튼을 덮으며 끝까지 열기

**LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / EDITOR29_PASS / USER_PLAY_REVIEW_PENDING.** 첨부66fd0e80/6a7197f2와 추가 ‘사운드부터 마지막 슬라이더를 조금 내리기’가 최신 기준이다. 아래D078의 반대 제목 fade와95%높이·본문Y를 이 범위에서 대체한다.

- 두 헤더의 alpha는1로 유지한다. navigationPlacement를 본문 sheet보다 먼저 그리게 하고, alpha1의 본문 surface가 제자리의 반대 버튼을 아래부터 덮는다. divider도 fade하지 않는다. 실제로 완전히 덮힌 반대 버튼의 raycast만 막고 선택 헤더의 클릭·드래그는 기존 경로로 유지한다. 선택 제목만 돌출되는 단차/닫힌 화살표 숨김·JP/CN은 유지한다.
- 열린 높이 safe area의 **95%→100%**, .24초 SmoothStep 유지. 실제 노치·시스템바의 safe area는 존중한다. 화면 중심/점수·게임 상태는 바꾸지 않는다.
- 설정 block만 직전 로컬Y에서 **16 reference units 아래로** 이동한다. Audio -6→-22 / Effects -302→-318 / Slider행[-66,-145,-224,-362,-441,-520,-599]→[-82,-161,-240,-378,-457,-536,-615]. 두 reset도 같은 만큼 이동, 행 간격 유지. 이 값은 새로 높아진 패널 내부 기준이며 전체 패널이 더 위까지 올라가는 이동과는 별개다.
- Footer의1:1·글자18/globe18/flag24×18·간격6은 보존한다. 높이 확대에 따른 기준화면48units를 보정해 FooterY -742→-790, content높이790→838로 fit bounds에 하단 touch영역까지 포함했다. 540×960기준 Footer 화면 위치는 유지하며 줄어든 safe area에서 아래로 잘리지 않는 geometry 검사를 추가했다.

**변경 파일:** SettingsView.Navigation.cs(불투명/covered 입력), SettingsView.cs(기본높이1), SettingsView.NavigationLayout.cs(명시적 기존 authoring/fallback의 표시순서·높이·블록/footfit), SettingsView.prefab, BottomPanelTests.cs 및 기존 EditorUsability/SceneScreenTests2개. 기존 검사2개가 ‘Hierarchy 첫 TMP’를 선택해 AutoSize22인 새 헤더에32/24를 요구하던 중간FAIL을 남겼다. 테스트 대상을 원래 의도한 ContentAnimation의 본문 TMP 및 실제 SheetSurface로 바꿨으며 저장/Undo/글꼴/문구/색/이미지 유지 단언은 그대로다. 게임용 View 코드를 이 실패에 맞춰 바꾸거나 단언을 완화하지 않았다.

**보존:** D:/CursorDash_Backups/bottom-panels-cover-20261102/Before,1741파일/159,505,073bytes. 기존 미커밋/사용자 Scene·폰트·meta 포함. main d2801e8/index빈, 실제Scene 저장/다른Scene·게임소스/설정값/서버선택·Project_Brawl 조작 없음. 컴파일 중 한 번 CursorDash 연결 없음 응답이 있었고 이후 동일 명시 식별자로 복구, 실제 경로·Cyberpunk·PlayOFF를 다시 확인했다. 호출은 전부 per-call CursorDash 대상이며 다른 프로젝트로 fallback하지 않는다. 테스트의 파생font cache2만 이번 시작 파일로 복원한다.

**검사:** 최종 비Play **29/29**(job60ea7bf8fd6b408b9db0feab4228cb08,14.777초), 실제 compile/최종 Console0. 첫 run은29개 중 검사 대상 선택2FAIL, 수정 뒤 동일 검증 범위29/29다. Play 메서드는 제외한 기존 목록을 재사용했다. 반대 헤더 alpha 유지·형제 표시순서/본문 불투명도·선택 제목 top=safe top·줄어든 safe영역의 세로/가로 footer·기존언어/초기화/참조/배치·임시표시/사본 저장/Undo·기록 보호를 확인했다. 실제 scene clean/PlayOFF/compileOFF/timeScale1/previewOFF.

편집GPU10장(진행0/6/12/50/100%·양쪽패널/가로)을 생성했고 주요 초반/열림/가로 화면을 직접 열어 확인했다. 반대 버튼이 fade 없이 부분적으로 가려짐, 상단 도달·단차와 수정 본문/하단 표시를 확인했다. 별도 Play·실제 손 드래그·모바일/터치·WebGL/성능·저장재실행은 **NOT_RUN**, 연속 실제 Play영상도 아니다. `Logs/bottom-panels-cover-20261102`에 작업전manifest·첫실패/최종JSON·PNG·최종파일비교를 남긴다.

**사용자 검수:** 닫힌 랭킹/설정 → 천천히 아주 조금 위로 드래그해 반대 글자가 옅어지지 않고 아래부터 가려지는지 → 반쯤/끝까지 올려 선택 제목이 상단에 닿는지 → 역드래그로 버튼이 다시 드러나는지 → 반대 패널도 반복 → 설정 본문의 내려간 여백·Footer/JP·CN·슬라이더/reset·Pause/Home 확인. 빌드/커밋/push/배포 없이 사용자 검수를 기다린다.

## 최신 보완 — D-078 (2026-11-02)

**LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / EDITOR27_PASS / USER_PLAY_REVIEW_PENDING.** 최신 사용자 첨부41958355/d3432c28의1~5항을 구현했다. 6번은 빈 항목이므로 추가 요구를 추정하지 않았다. 아래 D077은 첫 구현 이력이며 현재 모양·국가코드/하단 치수는 이 절이 우선한다.

- 닫힌 하단의 화살표2개는 숨긴다. 열린/드래그 중 선택 제목의 닫기 화살표만 보인다. 언어 내부키 `ja`/`zh-Hans`는 보존하고 표시만 **JP/CN**으로 바꾼다.
- 이전의 sheet 루트 Image가 전체 사각형을 그려 선택하지 않은 제목 쪽 빈 공간을 덮고 있었다. 원래 Image는 컴포넌트를 지우지 않고 표시/입력만 끄고, 그 색·재질을 가진 명시적 `Panel body surface` 자식을 만든다. 본문은 navigationHeight72 아래부터 시작하고 선택 제목 절반만 위로 올라간다. 기존 .24초 이동·헤더/드래그·저장·입력잠금 경로는 유지한다.
- 튜토리얼/언어 버튼을 각각 **230×60**, X=-115/+115(reference540×960, content폭460의 정확한 절반)로 정렬했다. 튜토리얼 기본 글자20→18, 언어코드16→18. Globe22×22→18×18, 국기35×35(실화상4:3)→24×18. 글자·아이콘·국기의 시각적 높이를 맞추고 중심 Y를 일치시켰으며 국기/코드 간격은6으로 줄였다. 각 버튼의 실제 입력 영역은 크기 변경과 함께 1:1이다.
- 설정 항목/두 reset만 **위로18 reference units** 옮겼다. Audio heading -24→-6, Effects heading -320→-302. 행 Y[-84,-163,-242,-380,-459,-538,-617]→[-66,-145,-224,-362,-441,-520,-599]. Footer Y=-742와 각 행 사이 간격, Slider 두께·판정은 유지한다. 사용자 화면 검수용 시험값이며 반복 곱/누적 이동이 아니다.

**변경 파일:** SettingsView.Navigation.cs(국가표기/화살표/명시참조), SettingsView.cs(본문 상단), SettingsView.NavigationLayout.cs(명시적인1회 배치 보완·fallback 일치), SettingsView.prefab(기존 자산 내 body/배치/연결), Editor/CursorDashBottomPanelsSetup.cs(실제Scene을 저장하지 않는 보완 경로), Editor/CursorDashUIAuthoringPreview.cs(화살표 임시표시 복원), Tests/Editor/CursorDashBottomPanelTests.cs(닫힘/단차/균등 입력영역/높이·국가표기). 기타 게임 소스·Tuning·아이콘/폰트 소스·사운드·다른 패널은 변경하지 않는다.

**보존:** 작업전 외부 `D:/CursorDash_Backups/bottom-panels-polish-20261102/Before` 1741파일/159,484,830bytes. 이전 D077 미커밋 변경·사용자 저장 Scene/폰트/미추적/.meta 포함, HEAD d2801e8/index빈 유지. Unity 조회 모두 `Cursor-Dash-master@6cde4df4002de875`를 명시했다. 시작Cyberpunkclean/PlayOFF/compileOFF/사용자편집previewON을 확인하고 임시표시만Stop했다. 실제Scene 저장·재로드/다른프로젝트·서버변경 없음. 검사에서 추가된 기존 font2의 파생 glyph cache만 이번 시작 사본으로 복원한다. Scene/Packages/ProjectSettings·기타 자산과 기존 사용자 데이터/작업을 HEAD로 되돌리지 않는다.

**검사:** 실제 Unity 컴파일 완료/최종Console오류·경고0. 비Play 최종 **27/27**(job63c405b0c7aa4e22a7b306591f772232,14.893초): BottomPanel10/Authoring7/Hierarchy2/SceneScreen4/Usability4. Native Play 메서드를 제외하고 기존 보호 fixture·분리 사본 저장/Undo/임시표시 복원 검사를 재사용했다. 저장값을 쓰는 controller/게임 bootstrap은 실행하지 않았고 테스트의 userValues는 격리·복원한다. 실제 Scene missing0/clean/Overlay/PlayOFF/compileOFF/timeScale1/previewOFF.

편집 GPU 캡처8개(Closed,Settings/Ranking half/open,JP,CN,1920×1080)를 만들고 화면으로 확인했다. 1080×1920 및1920×1080에서 닫힌 화살표 없음, 제목 쪽만 높은 단차/배경 노출, Footer 균등 정렬/국기·문자 높이, 본문 위18이동과 JP/CN 표시를 확인했다. **편집 샘플 렌더이며 실제 연속 Play·드래그·손/모바일·WebGL·성능은 NOT_RUN**이다. 테스트 개수로 사용자 최종 시각 승인을 대신하지 않는다. 실행 증거 `Logs/bottom-panels-polish-20261102`, 최종 테스트JSON/작업전manifest·delta와 PNG를 보관한다.

**사용자 검수:** Cyberpunk Play에서 닫힌 두 헤더 → 설정을 반쯤 드래그(왼쪽 위는 배경, 오른쪽 제목만 돌출) → 끝까지 열기/닫기 → 랭킹도 반대 방향 단차 확인 → 설정 footer 좌우 터치와KO/EN/JP/CN 순환 → 사운드부터 마지막slider까지의 상단 여백 → Pause/Home 복귀 순서다. 실제 Play는 이번 수행하지 않았다. 빌드·커밋·push·배포 없이 여기서 검수를 기다린다.

**직접 편집:** `StageManager/SettingsView/Safe area/Main bottom sheet/Panel body surface`의 Image가 본문 외형이다(그 Rect 상단은 View.Navigation Height에 연결). `Settings placement/Settings content`의 heading·행·reset Y가 상단 여백, `Replay tutorial`/`Cycle menu language`가 Footer 절반 영역이다. 헤더의 `Close direction`은 runtime 표시용이므로 닫힌 기본 상태를 Inspector enabled로 고정하지 않는다. 위1회 보완 함수는 Inspector 편집값을 매 프레임 덮어쓰지 않는다.

**LOCAL_IMPLEMENTED / EDITOR26_PASS / EDITOR_GPU_CAPTURED / USER_PLAY_REVIEW_PENDING.** 실제 클릭·드래그·언어 저장 후 앱 재실행은 NOT_RUN이다. 빌드·커밋·push·배포 없이 미커밋 상태로 사용자 검수를 기다린다.

## 고정 문맥과 최신 결정

- 작업 ID bottom-panels-20261102 / 프로젝트 CursorDash / 원본 `D:/renewal_CursorDash/Cursor-Dash-master`, Git `D:/renewal_CursorDash`, main 기준 d2801e8. 프로젝트 프로필·Slack 목적지는 미설정, 현재 Windows PC. 독립 Claude QA/외부 전송 미요청·NOT_SENT.
- 사용자 원래 목표: 하단 `(아이콘) 랭킹 | (아이콘) 설정`, 선택한 제목만 올라가는 공통 bottom sheet. 설정 항목은 유지하고 사운드/화면효과 초기화 아이콘을 각각 배치, 하단 튜토리얼·국기형 언어 선택. 랭킹은 미래 Google Play Games 연동 전의 화면 틀만 만든다. 게임 규칙·연동 SDK/로그인/점수 제출은 이번 범위 밖이다.
- 첨부 2장은 구조·위치 설명이다. 노란 표시는 초기화 버튼 위치 근거이며 실제 UI를 노란 장식선으로 만드는 요구로 해석하지 않았다.
- 사용자 직접 Scene 저장 회신 뒤 Cyberpunk clean 확인. MCP 모든 Unity 요청에 `unity_instance=Cursor-Dash-master@6cde4df4002de875` 명시. 기본대상 Project_Brawl 변경·Editor 명령·서버 설정/재시작 없음.
- 최신 언어 범위 회신: **새 하단 버튼·랭킹·설정 패널만 4언어**. 앞서 ‘글꼴 선정 후 전체 언어’라는 포괄 회신보다 이 구체적 범위 회신이 우선한다. Pause·튜토리얼·피버 등 다른 플레이 문구는 이번 번역 범위 밖이다. 한국어/영어는 기존 이사만루체, 일본어/중국어 간체는 사용자 선정 Sarasa Gothic UI 지역형을 사용한다.
- 최신 아이콘 회신: 임시 코드 도형을 Google Material Symbols 공식 SVG 기반 아이콘으로 교체한다. 국기는 flag-icons의 MIT SVG. 원본 SVG·출처·라이선스 보존, 실제 uGUI는 원본에서 만든 256px Sprite 사용. Unity 패키지/OS 폰트 설치 없음.
- 빌드·커밋·push·배포 승인 없음. 이전 릴리스/Play 권한은 재사용하지 않는다. 이번 Native Play는 미실행이며 사용자의 수동 Play 검수를 인계할 예정이다.

## 작업 전 보존

외부 `D:/CursorDash_Backups/bottom-panels-20261102/Before`와 manifest1686파일(128,820,483bytes), Git 시작 목록을 보존했다. Assets/미추적/.meta·Packages·ProjectSettings·관련 docs/도구 포함, 로그·캐시·과거 evidence 제외. 시작의 View SDF font2개/직전 연결 STATUS 및 사용자 저장 Scene 변경을 사용자 작업으로 보존한다. 이번 변경은 HEAD 전체 diff가 아닌 이 사본과 비교한다.

## 구현 구조

기존 SettingsUI/View/Prefab과 Tuning/Storage 사용. 본문 SheetAnimation과 Safe area의 Main navigation placement를 분리해 선택한 header만 움직이고 다른 header는 제자리에 남아 먼저 페이드된다. 공통 이동은 기존 .24초 SmoothStep, 닫힌 높이72(reference540×960), 본문95%. Slider7개와 Pause2개 연결 유지. 사운드초기화는 master/bgm/sfx 3키, 화면초기화는 vfxReduction/shake/flash/distortion 4키만 변경한다. 언어 선택은 기존 UserKey JSON의 menu.language 메타값이며 개발자 규칙/프리셋의 Parameters 개수·DefaultsVersion은 바꾸지 않는다.

랭킹은 실제 기존 최고점수 읽기와 순위/플레이어/점수 열·온라인 준비중 상태다. 가짜 순위/플레이어·별도 점수 저장·네트워크 호출 없음. 새 View/Manager 중복 없이 기존 SettingsView의 명시참조를 확장한다. Tools→CursorDash→화면 편집에 랭킹 상태를 추가하고 임시 표시와 저장값을 분리한다.

## 실제 검사와 한계

- PASS: Unity6000.3.23f1의 실제 스크립트 컴파일 완료, 최종 Console 오류·경고0. Native Play/EnterPlayMode는 실행하지 않았다.
- PASS: 최종 비Play Editor **26/26**, job `90b9d2f94e394be7ab63ff6c8f49e9a9`, 14.020초. BottomPanel9 + Authoring7 + Hierarchy2 + SceneScreen4 + Usability4. 이전 26/26 재검사와 합산하지 않는다. 전체 fixture의 Play 진입 메서드를 제외하고 실행할 메서드를 먼저 확인했다.
- 첫 run25개 중 Canvas mode 관련3FAIL은 실패 이력이다. 업그레이드 저장 경로의 임시 WorldSpace 저장을 수정해 Prefab/실제 Scene Overlay를 확인한 뒤 같은 단언으로 최종 통과했다. 테스트 완화 없음.
- 첫 아이콘 CanvasRenderer 누락, SVG 기본 검정 fill은 수정한 중간 실패다. 최종은 Google 원본 SVG를 보존하고 흰 Sprite로 변환, 9개 Sprite/Renderer/참조를 검사했다.
- PASS (편집 GPU 렌더만): 1080×1920 메인·설정·랭킹·기존Pause, KO/EN/JA/ZH 설정과 반쯤 열린 상태, 1920×1080 설정의 PNG9개를 생성해 직접 열어 확인했다. 선택 제목만 이동하며 다른 제목은 사라진다. 글자 깨짐/잘림과 국기·아이콘을 확인했다. 이는 실제 Play·입력·터치 또는 연속 영상 검수가 아니다.
- 기존 이사만루체의 Font.HasCharacter는 일부 문자를 지원한다고 반환했지만 실제 TMP glyph 생성에서는 일본어·중국어 다수 누락을 확인했다. 단순 HasCharacter를 완전한 지원 증거로 쓰지 않는다.
- PASS: Sarasa UI v1.0.43 공식 J/SC Unhinted SemiBold 소스2개만 Editor 폴더에 보존하고, 메뉴 glyph+ASCII만 1024 atlas의 static SDF로 제작했다. 4언어 모든 표시 문자의 실제 TMP glyph 생성·한국어 원문/이사만루체 복원과 Editor 소스 TTF에 대한 runtime 의존성0을 검사했다. Windows 폰트/패키지 설치 없음.
- 중간 Console에서 회로 LateUpdate/합성 NRE를 조회했다. 당시 자동 Play는 실행하지 않았고 재현·원인은 미확정이다. UI 범위 밖 게임 코드는 수정하지 않았다. 이후 컴파일 뒤 Console0을 확인했으나 과거 오류 해결 보장으로 쓰지 않는다. 전역 Editor.log는 다른 Unity 인스턴스 기록일 수 있으므로 이후 근거로 사용하지 않는다.

- PASS: 보호 저장6문자열·3float의 존재/해시가 작업 전과 동일. 기록·튜토리얼·기존 설정을 초기화하거나 테스트 언어로 저장하지 않았다. 테스트의 Tuning 변경은 in-memory 데이터만 격리·복원했다. 실제 SaveUser/재실행 저장 경로 검수는 NOT_RUN이다.
- 코드·문서 whitespace 검사 PASS. 전체 git diff --check는 Unity가 쓰는 Prefab의 빈 m_Name/m_text 뒤 공백으로 FAIL이며 기능/컴파일 실패와 구분한다. 기존 YAML 전체를 공백 정리하지 않았다. 캡처의12345는 편집 전용 샘플이며 저장된 BEST가 아니다.
- 최종 기준 Scene은 작업 전 사용자 저장본과 동일하다. 편집 검사/업그레이드 저장 중 섞였던 점수·슬라이더 표시값은 사본으로 되돌리고 새 패널은 기존 GUID의 Prefab 참조로 연결한다. 기존 View font2의 추가 파생 glyph cache도 작업 전 사용자 파일로 복원한다. 기존 사용자 변경을 HEAD로 되돌리지 않는다.
- Reload 알림으로 MCP 응답이 일시 중단되어 사용자가 Reload를 눌렀다. 재연결 뒤 정확한 CursorDash 경로·Cyberpunk/clean/Overlay·PlayOFF·compileOFF·previewOFF를 재확인했고 기준 복원 후에도26/26 통과했다. Project_Brawl 요청·기본선택/서버 변경 없음.
- 마지막 Native 조회: Cyberpunk clean/PlayOFF/compileOFF/timeScale1/previewOFF/missing scripts0/Overlay. 실제 Play·손 입력·모바일/터치·WebGL·성능·앱 재실행 저장·전체 전투 회귀는 **NOT_RUN**. UI 수정 후 사용자 검수 전이며 강제 완료 승인으로 취급하지 않는다.

## 변경 파일과 조절 위치

| 파일/자산 | 변경 이유 |
|---|---|
| SettingsView.prefab | 기존 GUID·Slider7/Pause2 참조를 보존하면서 두 헤더·랭킹·섹션별 초기화·언어 버튼을 직렬화 |
| CursorDashSettingsUI.cs / CursorDashSheetHandle.cs | 선택 헤더 클릭·드래그, 기존 입력잠금/저장 경로 재사용, 실제 최고점 읽기 |
| CursorDashSettingsView.cs / .DefaultLayout.cs / 새 .Navigation.cs / .NavigationLayout.cs | 배치와 상대 연출, 선택 패널/언어/아이콘 연결. 한 번의 authoring upgrade와 호환 fallback만 추가 |
| CursorDashTuning.cs | 기존 사용자 JSON에 menu.language 저장, 음향3키/효과4키 독립 초기화. 개발자 설정 schema·기본 전투값은 불변 |
| CursorDashViewFonts.cs | 지역형 글꼴을 공유 자산 대신 View 소유 복사본에 연결하고 같은 언어에서 재생성하지 않음 |
| 새 CursorDashMenuIcon.cs / SVG9·Sprite9·고지 | Google 공식 아이콘5+국기4. 원본·사용 조건 보존 |
| 새 Sarasa Editor 소스2 / static Menu SDF2 / OFL | 선택한 글꼴의 일본어·간체 메뉴 지원. 원문 이사만루체·타이틀 로고 유지 |
| CursorDashScreenAuthoringWindow.cs / UIAuthoringPreview.cs / .Screen.cs | 기존 Tools 상태 버튼에 랭킹을 추가, 두 헤더의 임시 표시만 저장/종료 때 복원 |
| 새 Editor/CursorDashBottomPanelsSetup.cs | 명시적 1회 자산 변환/연결용. 자동 실행·새 창·새 runtime Manager 없음 |
| 새 Tests/Editor/CursorDashBottomPanelTests.cs | 새 패널·언어 glyph/원문 복구·섹션 초기화·참조와 저장 보호 회귀 |

기본 패널 이동 .24초(SmoothStep)/닫힌 하단 높이72/reference540×960/본문 높이 safe area의95%다. 선택하지 않은 제목은 이동하지 않고 초기25% 진행 안에 사라진다. 실제 Slider 시각12×12 손잡이·4px track 및48 터치영역은 보존했다. 새 reset52×48·언어156×60(reference units), JA/SC48pt/6padding static1024 atlas다.

**편집**: Tools→CursorDash→화면 편집의 메인/설정/랭킹. 실제 `StageManager/SettingsView/Safe area/Main navigation placement`의 두 `... header animation/Menu title` TMP·Menu icon Image에서 외형, `Main bottom sheet/Settings placement`와 `Ranking placement`에서 본문 배치를 바꾼다. 헤더/본문 Animation의 실시간 offset보다 Placement/TMP/Image를 편집한다. 공통 원본은 `Assets/Prefabs/UI/SettingsView.prefab`, Scene만의 변경은 Cyberpunk 인스턴스 Override다. View의 Navigation Height/Panel Travel Seconds로 높이/시간을 조절한다.

**문구**: 한국어 원문은 자식 TMP, EN/JA/중국어 간체는 SettingsView의 Menu Texts 항목에 있다. 번역 문구에 새 일본어/중국어 글자를 추가하면 static 메뉴 atlas 재생성이 필요하다. 현재 InstallMenuFonts는 이미 만든 atlas를 덮어쓰지 않는 안전한 최초 설치용이므로 재실행만으로 새 glyph가 추가된다고 안내하지 않는다. Editor 소스 TTF로 TMP Font Asset Creator에서 추가 문자 포함 atlas를 다시 생성·검사해야 한다. 이번 4언어 현재 문구의 glyph는 모두 확인했다.

## 사용자 수동 Play 순서

1. Cyberpunk에서 Play → 하단 랭킹/설정이 함께 표시되는지 확인한다. 각 제목의 클릭·위로 드래그·닫기/아래 드래그·Escape를 확인한다. 선택하지 않은 제목이 함께 올라오지 않아야 한다.
2. 설정 Slider를 각각 움직이고 사운드 reset은 음향3개만, 화면효과 reset은 효과4개만 복원하는지 확인한다. 작은 아이콘 주변도 눌릴 수 있어야 한다.
3. globe/flag를 눌러 KO→EN→JA→ZH→KO를 확인한다. **하단 버튼/랭킹/설정만** 번역되며 Pause·튜토리얼 본문은 현재 한국어를 유지한다. 언어를 선택한 뒤 닫기/재실행 시 유지 여부를 확인한다.
4. 랭킹은 실제 내 최고점과 온라인 준비중 화면만 보여야 한다. 가짜 순위·로그인·점수 제출은 없다. 최고점 변화 후 다시 열어 확인한다.
5. 튜토리얼 플레이 → 기존 튜토리얼 진입, 본게임 → Pause/계속/Home에서 하단 패널 잔류·동시 클릭으로 게임 시작·슬라이더가 멈추는 문제가 없는지 확인한다.
6. 휴대폰 실제 세로 화면과 화면비/안전영역, 터치·드래그를 최종 확인한다. 이번에는 빌드하지 않았다.

## 보존·복구와 다음 한 작업

작업 전 manifest1686개 중 삭제0, Packages/ProjectSettings·노드·피버·점수·공격·커서·배경·타격 코드 변경0이다. main d2801e8/빈 index 유지. 사용자 저장 Scene 및 View font2의 작업 전 변경도 보존한다. 최신 파일 목록/해시는 `preservation-audit.json`, 실제 최종 조회는 `end-state.json`에 있다.

이번 변경만 되돌릴 때에는 해당 목록의 기존 파일만 **Before 사본**과 비교해 복구하고, 새 MenuIcons/Sarasa/Navigation/BottomPanel 검사 파일은 이 작업에서 만든 .meta와 함께 별도로 옮긴다. 이후 사용자의 편집이 있다면 전체 사본 복구나 HEAD reset을 하지 말고 먼저 diff로 병합한다. 로그·외부 원본·과거 자산을 일괄 삭제하지 않는다.

다음 한 작업은 사용자 새 하단 패널 Play/터치·언어 저장 검수다. Google Play SDK/실랭킹 연결·전체 게임 번역·다른 UI 개편은 새 요청을 기다린다. 일반 노드 겹침/새 Flash 그림·별도 사이버 재구축은 계속 보류, 육각 타이밍은 제외했다.

## 자료와 라이선스

- Google Play leaderboard 구조 확인: https://developer.android.com/games/pgs/leaderboards (연동 구현 아님)
- Google Material Symbols: https://fonts.google.com/icons / Apache-2.0 https://github.com/google/material-design-icons/blob/master/LICENSE
- flag-icons: https://flagicons.lipis.dev/ / MIT https://github.com/lipis/flag-icons/blob/main/LICENSE
- Sarasa UI: https://github.com/be5invis/Sarasa-Gothic/releases/tag/v1.0.43 / OFL https://github.com/be5invis/Sarasa-Gothic/blob/master/LICENSE
- 아이콘 원본 `Assets/Art/UI/MenuIcons/Sources`, Sprite/배포용 고지 `Assets/Resources/CursorDash/MenuIcons`.
- 실행 근거 `Cursor-Dash-master/Logs/bottom-panels-20261102`, 첫 검사 job 5bd944f9efd54a9a9d97b8a620f616dc.
