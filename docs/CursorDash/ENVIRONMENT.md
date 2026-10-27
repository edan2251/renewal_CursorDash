# CursorDash 기존 환경과 후속 보존·검증 조사

## 최신 추가 — 설정 UI (2026-10-28)

D-065: 기존Unity6000.3.23f1/Cyberpunk읽기진단PlayOFF·clean/imported6동일범위, 저장폐기없음. UI참조컴파일성공이나실제Editor확인중physicalEscape사용자중단으로native/Play미검수. 설치/새연결/전역변경/빌드/게시없음. UI전완성본bded6a4 source main push반영확인/배포브랜치불변. 상세 SETTINGS_PAUSE_UI_20261028.md.


## D-064 — 기존 Editor·정상 집중검수 (2026-10-28)

사용자이번Unity검수조작허용범위로기존6000.3.23f1/Cyberpunk/DX11/정상TestRunner·기존보호fixture만재사용했다. 최종18/18 PASS(04:05:30 KST), 최종JSON/XML과PlayOFF/timeScale1/씬clean·확인저장키6/imported6동일을대조했다. 최초activate timeout은새대상선택으로복구했고한번잘못반환된전경캡처에는입력하지않고Unity를재선택했다. 저장/폐기·다른프로젝트/전역·새연결/설치·빌드/Git없음. 참조117/test16/Editor2오류0·기존133경고,순수4107+4672999+106885/JS모의19. [이번인계](TUTORIAL_POWERON_FEVER_ORDER_20261028.md), `Logs/tutorial-poweron-fever-order-20261028/native-final-summary.json`. 이전검수·모바일/릴리스를이번시각승인으로재사용하지않는다.

## D-063 — 기존 Editor 집중검수 (2026-10-28)

이번 별도 회신으로 대상Unity 컴파일/TestRunner·Play 검수 조작만 허용됐다. 기존6000.3.23f1/Cyberpunk/DX11·기존정상TestRunner·보호fixture를 재사용하고 batchmode/다른프로젝트/저장폐기·패키지·전역·새연결/빌드/Git게시없이 진행한다. 최초창활성화timeout은 새창목록/같은대상재선택1회로복구했다. 최종18/18 PASS(02:55:28 KST), PlayOFF/timeScale1/Cyberpunkclean·확인6저장키/imported dirty6동일을 `Logs/tutorial-pace-notice-20261028/native-final-summary.json`에 기록했다.

기준main8d26194/index빈상태·1826파일/사본Backups/tutorial-pace-notice-20261028. 참조117/test16/Editor2오류0·기존134경고/순수3989+4672999+106885·JS모의19를 실제Native/GPU·손조작과구분한다. [이번인계](TUTORIAL_PACE_FEVER_NOTICE_20261028.md). 과거모바일/빌드 검수를 현재 수정의검증으로재사용하지않는다.

## D-062 — 기존 Unity 정상 검수 경로 (2026-10-28)

사용자이번검수조작명시승인으로 기존 대상Unity6000.3.23f1/Cyberpunk/DX11만 사용했다. 기존 TestRunner API EditMode/EnterPlayMode·ExitPlayMode를 선택했으며 포괄 SaveAssets/저장·폐기·batchmode·중복Editor/다른프로젝트 종료는 없다. 기존 보호 fixture가 실제 설정/프리셋/완료/BEST를 보관·복원하며 native-before/after에서 확인6문자열키 해시·imported6목록 동일/Cyberpunkclean·PlayOFF/timeScale1이다. 도구 목록 존재만으로 성공 처리하지 않는다.

최종 NativeRun4 14/14 PASS(02:12:41 KST, 소유권/실패·Retry 포함)의 결과는 [인계](AUTONOMOUS_GUIDE_20261028.md)·`Logs/autonomous-guide-20261028/native-final-summary.json` 참조. 참조컴파일117/test16/Editor2 오류0·기존134경고, 순수생산검사·JS모의 검사와 실제 Play/GPU 프레임을 구분한다. 1819파일 기준·복구사본 `Backups/autonomous-guide-20261028`, HEAD/index·기존사용자파일보존. 설치/패키지/전역/새연결/빌드/Git게시없음.

010703영상 프레임 추출은 Windows 실행정책 차단으로 관람못했으며 우회·정책변경하지 않았다. Native GameView PNG는 별도 실제증거/연속녹화아님. 손입력·모바일/WebGL·장시간성능·앱재실행저장 NOT_RUN. 일반 환경조사나 이전배포를 반복하지 않았다.

## D-061 로컬 구현·사용자 Play 인계 (2026-10-28)

main `8d26194`/빈 index 기준과 기존 미추적 보존, 시작1808파일 해시 및 `Backups/ghost-tutorial-20261028` 복구 사본을 남겼다. 기존 Unity6000.3.23f1/ .NET10.0.402/Node 도구만 사용했다. 격리 참조 컴파일 runtime116/test15/Editor2 오류0·기존134경고, 실제 순수 생산 C# 실행/모의 DOM·메모리 저장 bridge는 [D-061 검사](GHOST_GUIDE_20261028.md) 참조. 신규 설치/패키지·전역/연결·프로세스 종료/빌드/Git 게시 없다.

처음 대상 Cyberpunk 창/Play OFF/오류0을 확인한 범위와 현재 소스 컴파일을 구분한다. 이후 사용자가 Play를 직접 담당하므로 Unity/브라우저 조작은 중단했다. 현재 dirty 객체 목록·Native 컴파일/Play/GPU/공식 NUnit/모바일·WebGL·실 저장 재실행은 NOT_RUN이며 과거6 imported dirty 기록을 이번 새 조회로 사용하지 않는다. 사용자 씬/에셋을 저장·폐기하지 않았다. 영상은 파일 존재만 확인, 디코딩/관람 불가. 임시 영상용 루프백 서버는 화면에 열지 않고 자기 종료 요청으로 정리했다.

## D-060 실제 WebGL 빌드·기존 Pages (2026-10-27)

Unity6000.3.23f1 기존 Cyberpunk/WebGL Editor를 사용해 전용 새 폴더에 BuildPipeline 실행, 자동 Play/씬·에셋 저장 없음. 오류0/경고8/259.308초·입력1489파일 동일, 종료Cyberpunkclean/imported6·prefs동일. 기존 등록 Cyberpunk→legacy01/템플릿·비압축/단일스레드 설정 유지. 이전 고정 ID 메뉴를 대체하는 이번 릴리스용 No Play 메뉴를 기존 helper에 추가했다.

source `2c8ad699fc0138513f8112f2ec0703194438c575` / gh-pages `de9ec5fae6394f2dc64c36166206667b69f4ec53` / `cursordash-20261027-feedback-4694b4ff0c22`. 로컬24·공개25파일 해시/MIME·실제IAB 실행 확인. 첫 제한된 루프백 연결 실패는 전용127.0.0.1 서버의 정상 승인 경로로 해결했고 기존 실패 로그도 보존했다. 서버는 검사 후 정상 종료. 설치/전역·새연결/다른프로젝트 종료 없음. [전체 결과·8경고·미검증](RELEASE_FEEDBACK_20261027.md).

## D-059 로컬 수정·격리검사 (2026-10-27)

기존 .NET/Unity 참조만 사용해 runtime112/test14/Editor2 격리컴파일 오류0·기존134경고, 실제생산순수.NET4672999+106885/정적보존을 확인했다. 이는 대상Unity Native/Shader/GPU·Play 성공이 아니다. 실시간미저장조회·Unity창/Play/저장폐기/재임포트·앱재실행/다른프로세스종료·설치/전역·새연결/빌드/Git게시는없다.

시작1547파일/백업Backups/global-scorewave-final-hit-20261027·로그Logs/global-scorewave-final-hit-20261027, 사용자미커밋·미추적과HEAD/main/index보존. 신규화면/Native·모바일/WebGL성능은NOT_RUN이다. [구현13파일·전후값·검사/수동절차](SCORE_WAVE_FINAL_HIT_20261027.md); 다음은사용자Unity검수다.

## D-058 로컬 파일 수정·격리 참조 검사 (2026-10-27)

기존Unity/연결·설정은 변경하지 않았다. Unity창/Play·미저장상태 저장폐기·강제재임포트/재시작·다른프로세스종료 없음. 이번 시작/종료의 실시간 dirty 메모리 조회는 하지 않았으며 D-057 snapshot을 현재 검사로 간주하지 않는다. Assets 밖 `Backups/arc-rewind-fever-circuit-20261027`에 작업전 사본, `Logs/arc-rewind-fever-circuit-20261027`에1546파일 해시/참조·순수·정적 결과를 남겼다.

기존 .NET/Unity참조로 runtime112/test14/Editor2를 격리 컴파일(오류0·기존134경고)하고 생산 순수수학만 실행했다. 새 설치/새 연결/전역변경·빌드·Git게시 없음. 이번 Native/Shader/GPU/Play·모바일/WebGL성능은 NOT_RUN이며 사용자검수를 기다린다. [변경13파일·전후값·검사/수동절차](ARC_REWIND_FEVER_CIRCUIT_20261027.md).

## D-057 대상 인스턴스의 저장·강한 피해 효과 확인 (2026-10-27)

기존 CursorDash Unity6000.3.23f1/DX11/Cyberpunk를 그대로 사용했다. 정상 Assets Refresh/기존 TestRunner 메뉴로 최종컴파일 오류0·기존Native경고7/집중70PASS(2026-10-27 20:33:34 KST)를 확인하고 새 JSON/XML·DLL/종료상태를 교차검사했다. Play 재진입 검사는 테스트 로컬값이 domain reload 때 유실되어 SessionState에 비교자료를 보존하며 핵심 단언을 유지했다. 실제 저장값은 재진입 뒤에도 유지됐다. 앱 전체 재실행은 NOT_RUN이다.

기존 미저장 imported6개는 전후동일, 저장/폐기·강제재임포트/재시작 없음. Cyberpunkclean·PlayOFF/compileOFF/timeScale1·확인prefs7동일, 실제 점수 키는 보호fixture/격리QA맵에서 검증하고 QA 소유키만 정리했다. 다른 Unity/프로세스를 종료하지 않았다. 빌드/설치/새연결/전역설정/Git게시 없음·D-014 유지. [상세 원인·절대값·변경12파일·검수](NEXUS_HIT_RECORD_FIX_20261027.md).

## D-056 로컬 검사 종료 상태 — 2026-10-27

기존 대상 Unity6000.3.23f1/DX11 Cyberpunk 인스턴스만 사용. 새 연결/설치·다른 프로젝트 종료 없음. Native 최종컴파일오류0/기존7경고·정상TestRunner 집중67PASS/최종JSON/XML19:34:10KST, 실제GPU 국소red·획보호 및 Play 내부호출. 보존 끝상태 PlayOFF/compileOFF/timeScale1/Cyberpunk dirty=false·기존 imported dirty6/확인prefs7 동일. 저장·폐기·재임포트없음/D-014 확대없음. main18b25da/빈index·미커밋, 빌드/Git게시 없음.

이번 작업 전 사본 `D:/renewal_CursorDash/Backups/score-nexus-review-20261027`; 로그/93PNG/최종소스 해시/검사 `Cursor-Dash-master/Logs/score-nexus-review-20261027`. 앱재실행 실기록·모바일/WebGL/성능/손입력은 NOT_RUN. [최신 인계](SCORE_NEXUS_REVIEW_20261027.md). 이전 환경 조사부터 다시 반복할 필요는 없으며 다음은 사용자 검수다.


## D-055 CP6 실행 환경과 종료 상태 (2026-10-27)

사용자대상창활성화후기존Cursor-Dash-master/Cyberpunk/Unity6000.3.23f1DX11창658488만사용. 최종컴파일오류0·native집중62PASS/GPU/Play확인. 참고WaveGame6000.5.7f1창은조작/실행/종료하지않고원본보호해시유지. 새설치·전역·연결·빌드없음.

종료진단: PlayOFF/compileOFF/timeScale1/Cyberpunkdirty=false, 확인prefs전후동일, Assets dirty6동일(TMP shader3/LiberationTexture/이사만루Medium·Bold FontTexture). Library기본scriptdirty는별도진단항목/에셋변경으로저장하지않음. 포괄저장·폐기·재임포트없음/D-014확대없음. UI/GameView일시확대뒤Play종료시기존분할뷰로복귀. 실제훈련버튼클릭성공·drag변위확인은미완료.

시작1884/CP6재개3104파일보호·Before/외부복구본Backups/continuous-flow-20261027유지. 최종근거는Logs/continuous-flow-20261027. 실제모바일/WebGL성능·정상기록재실행은NOT_RUN. [상태·미검증·사용자검수](CONTINUOUS_FLOW_20261027.md).

## D-054 WebGL·기존 Pages 릴리스 실제 실행 (2026-10-27)

시작Unity프로세스/창/lock없음확인후기존6000.3.23f1 batchmode빌드만실행/정상Exit했다. 자동Play·저장폐기/다른프로젝트종료·설치/전역변경없음. 빌드입력1489파일전후해시동일·새출력cursordash-20261027-space-e0c721d84748. 기존main/gh-pages/URL유지, source f6e5f8a/pages3195915 정상push·Pages성공이다.

루프백8886검수서버·실제IAB로컬/공개메인·클릭/마우스이동·훈련·Dev/JSON/보고서·새ID와승인기본값을확인했다. 로컬24/공개25HTTP/hash·wasm MIME일치/warn-error0,DevOFF/Main복귀·서버정상종료. 제한된CLI네트워크의초기소켓차단은정상승인경로로해결했고설정완화없음. 실제모바일/다른엔진/장시간성능·실저장재실행은NOT_RUN. [소스/빌드/문서커밋·경고/근거](RELEASE_20261027.md).

## D-053 기존 대상 Unity native QA 완료 (2026-10-27)

기존 Unity6000.3.23f1/Cyberpunk/DX11 인스턴스만 사용했다. 정상 Refresh/컴파일·정상 TestRunner집중30/30·실GPU pixel검사/Play내부호출·ScreenCapture24장을 수행했다. 새Unity/다른프로젝트종료/설치·Volume·카메라·패키지·연결·전역설정 변경은 없다. 마지막 실제 DLL08:08:48KST→JSON/XML08:09:52KST→읽기진단08:15:52KST를 교차확인했다. D-052의 과거도구중단은 현재차단이 아니다.

전후 Cyberpunkdirty=false·PlayOFF·timeScale1, Assets imported6개(TMP Shader3·LiberationSans/이사만루Medium/Bold FontTexture3)와 읽기진단prefs 동일. 미저장 상태를 저장/폐기/재임포트하지 않았고D-014를 확대하지 않는다. 외부 `Backups/space-feedback-20261027`34파일 유지, 이번시작1880파일 manifest/Before·근거는 `Logs/space-polish-20261027`. Git main/HEAD1d11743/빈index·기존미커밋/미추적 보존.

기존Python/.NET/실제Unity csproj참조로 runtime112/tests14/editor2 오류0·기존134경고/순수4,668,342단언. 실제마우스/터치·모바일/WebGL/성능·재실행기록저장은NOT_RUN이며 전체공식검사가 아니라집중30이다. 073054영상은 기존MediaPlayer일부시점관람, 전체정밀/오디오 아님. [환경·실제범위/파일](SHARED_SCORE_SPACE_POLISH_20261027.md).

## D-052 기존 Unity 읽기와 중단·외부 백업 (2026-10-27)

기존 Unity6000.3.23f1/대상 프로젝트·Cyberpunk/비Play/비컴파일/씬dirty=false는 기존 읽기 전용 진단으로 확인했다. imported Assets6개(TMP Shader3·LiberationSans/이사만루Medium/Bold Font Texture3)는 미저장 상태였으며 저장·폐기·재임포트하지 않았다. D-014를 확장하지 않는다. Unity/Play QA는 사용자 승인 범위이나 재승인 후 fresh Computer Use 호출도 physical Escape stop을 반환해 이후 입력을 중단했다. 새 Editor/다른 프로젝트 종료/Play·저장/공식테스트 실행은 없다. 파일 검사와 native 실행을 구분한다.

구조 변경 전 씬/meta·ProjectSettings34파일은 Unity 프로젝트 밖 `D:/renewal_CursorDash/Backups/space-feedback-20261027`와 manifest로 보존했다. 메모리 상태를 대신하는 백업이 아니다. main1d11743/빈index·시작1872파일/사용자 미커밋을 유지하며 기존 씬·프리팹/meta를 수정하지 않는다. canonical 시작 순서만 Cyberpunk→legacy01로 변경했다. 별도 카메라/Volume/패키지/설치·새연결·전역 변경은 없다.

참조 C#112/14/Editor2 오류0·기존134경고/테스트271참조(게임직접0), 순수.NET4,655,320단언. native DLL은 수정 소스보다 이전이므로 새 Unity 컴파일 PASS가 아니다.90영상 읽기는 브라우저timeout/WPF기존정책 차단·최종 UI중단으로 실패했고 우회하지 않았다. 임시 영상 서버는 종료했다. [정확한 자원·관찰 한계/검수](SPACE_DAMAGE_FEEDBACK_20261027.md), 로그 `Logs/space-feedback-20261027`.

## D-051 회로 밀도 로컬 검사 (2026-10-27)

기존 Python/.NET/읽기 전용 Unity 참조만 사용해 C#110/13/Editor2 오류0·기존경고134를 확인했다. Unity실행/Play·공식 실행·Shader/GPU/모바일/WebGL성능 NOT_RUN. 기존3메시/4재질/RT2 상한과 생성/해제를 재사용하며 기본 선56→82의 실제 성능은 미측정이다. main1d11743/시작1870파일·빈 스테이징 보존, 설치/새 연결·컴퓨터UI/저장폐기·빌드/Git게시 없음. [기록](CIRCUIT_DENSITY_20261027.md).

## D-050 로컬 회로 디자인 검사 범위 (2026-10-27)

Unity를 열거나 조작하지 않고 기존 Python/.NET/Roslyn·읽기 전용 Unity 참조DLL로 검사했다. 게임110/테스트13파일/Editor2 오류0/기존경고134. 별도 테스트 어셈블리는 저장된 csproj271참조·게임 직접참조0을 유지한다. 실제 Editor 재컴파일/Shader·GPU/Physics/Play·공식 실행/모바일·WebGL은 NOT_RUN이다.

main1d11743/빈 스테이징·시작1869파일 기준 보존. 새 게임 리소스/설치·전역변경·연결/씬 저장·폐기/빌드/Git 변경 없다. 이번 디자인은 기존3메시/4재질/지속RT2·1536상한·콜백/해제와 같은 알고리즘을 재사용하며 매타격/프레임 재질 생성은 추가하지 않았다. [검사/오프라인 도형 한계](CIRCUIT_DIAGONAL_DESIGN_20261027.md), 로그 `Logs/circuit-design-45-20261027`.

## D-049 기존 리소스·마스크 경로 (2026-10-27)

main/HEAD1d11743·시작1866파일/기존 미커밋·씬/미추적을 보존했다. 기존 .NET/Python/Pillow와 실제 Unity-generated 테스트 참조만 사용했다. 최신042144 파일 한 개를 루프백 전용 브라우저에서 읽고 탭/서버를 종료했다.040532는 읽지 않았다. 설치/새 연결/전역 변경·Unity 실행/Play/저장폐기는 없다.

회로 리소스는 재사용 메시3개/재질4개·지속 RGBA RT2개(각 긴축1536 이하)다. 점수 실제 메시/아틀라스와 카메라 직전 행렬을 읽는 마스크1재질/1RT만 추가했다. 실제 Native/GPU·SDF/RT방향·모바일/WebGL 부하는 NOT_RUN이다. 참조 C#110/13/Editor2 오류0·기존 경고134/테스트 실제271참조·게임 직접참조0 유지. [마스크 선택·정리/검수](CIRCUIT_GAUGE_RETREAT_GLYPH_MASK_20261027.md), 근거 Logs/circuit-fever-retreat-scoremask-20261027.

## D-048 기존 도구/보존 상태 (2026-10-27)

시작 main/HEAD1d11743·기존1863파일 해시/미커밋 목록을 보존했다. 설치/새 MCP·전역 변경·Unity 실행/Play/저장·폐기 없이 기존 .NET/Python/저장된 Unity 참조로 검사했다. 영상 한 파일만 루프백 전용 임시 브라우저로 디코딩했고 탭/서버는 종료했다. 실제 자료는 `_incoming/references/70~72`이며 전달 경로 CursorDash_incoming은 없었다.

합집합 표시용 정적 사각 메시/재질1·지속 RGBA RT1(긴축1536 이하)을 기존 회로에 추가했다. 원본 PNG fallback/비교는 보존한다. 이 경로의 실제 Unity/Shader·GPU/UV/모바일/WebGL 성능은 NOT_RUN, 참조 C#110/13/Editor2 오류0/기존 경고134다. 테스트는 실제 csproj271참조/게임 직접참조0을 유지했다. [조절값/리소스·검수 인계](CIRCUIT_JOINTS_INTAKE_FLOW_20261027.md), 근거 Logs/circuit-joints-flow-20261027.

## D-047 회로 원본 조사·로컬 검사 (2026-10-27)

새 설치/연결·Unity 실행/조작 없이 기존 Python/Pillow와 .NET/Roslyn/저장된 Unity 참조를 사용했다. 실제 씬 Sprite GUID를 따라 Original/Cyberpunk_Background_Flash.png/기본 UI 재질·알파87·전체화면 Rect와 기존 비교 합성을 확인했다. 원본·임포트/meta/씬은 변경하지 않았다. 로컬 Before1859파일/HEAD1d11743을 기록하고 현재 미커밋 작업을 이어갔다.

참조 컴파일은 게임110/테스트13/Editor2 오류0(런타임 기존 경고134). 테스트는 Unity-generated CursorDash.EditorTests.csproj의271참조를 그대로 사용하고 게임 DLL을 인위적으로 추가하지 않았다. 실제 Unity/Shader 컴파일·GPU·Play·공식 실행·모바일/WebGL 성능은 NOT_RUN이다. 도형 비교 이미지는 오프라인 정지 검사이며 게임 실행 화면이 아니다. 근거는 Logs/circuit-original-layout-20261027, [현재 인계](CIRCUIT_ORIGINAL_LAYOUT_20261027.md). Git 저장/게시·빌드·미저장 사용자 상태 저장/폐기 없음.

## D-046 후속 테스트 참조 검사 정정 (2026-10-27)

사용자의 CS0246 보고로 기존 검사의 한계를 확인했다. 이전 참조 컴파일 스크립트는 게임 DLL을 임시로 테스트 참조에 추가하여 실제 EditorTests asmdef의 게임 직접 참조 불가를 가렸다. 앞으로 테스트 참조에 게임 DLL을 인위적으로 추가하지 않는다. 이번에는 Unity가 이미 생성한 CursorDash.EditorTests.csproj의 HintPath271개를 읽기 전용으로 사용해 이전 동일 오류/수정 후13소스 오류0·경고0을 재현했다. csproj·asmdef·게임/씬·전역 설정은 수정하지 않았다. Unity실행/조작·Play/저장·폐기·공식 테스트 실행은 없다. 근거 Logs/circuit-test-assembly-fix-20261027/assembly-result.json.

## D-046 회로 구현·읽기/검사 범위 (2026-10-27)

기준main/1d11743·기존1850파일 Before/해시와 빈 스테이징을 보존했다. 기존 설치 Python/.NET/Roslyn/Unity 참조 DLL만 사용하고 설치/전역/패키지/연결을 변경하지 않았다. 제한된 셸의 Unity 프로세스0 조회와 실제 앱 목록의 대상 Unity 창을 구분하며, 내부 dirty 상태는 조회하지 못했다. Unity 창 조작/실행·Play·저장/폐기·강제 임포트는 없다.

앱 내 임시 탭과 루프백 전용 서버는 제공60 승인 영상의 디코딩 표본 읽기에만 사용했고 서버/정상 탭을 종료했다. 게임 브라우저 검증이 아니다. 실제 순수 회로 검사/참조 컴파일110·13·Editor2 오류0/기존 경고134. 공식 실행/실GPU·Shader/Physics/모바일/성능·빌드·새 커밋/수정·push/배포는 NOT_RUN. [구현·근거·사용자 검수](DYNAMIC_CIRCUITS_20261027.md).

## D-042 구현·영상 읽기·검사 환경 (2026-10-26)

main/HEAD8c357b8·기존dirty/미추적1847파일의해시/Before를기준으로작업했다. 사용자변경씬/복구·보관파일 유지/메모리dirty상태 조회나저장·폐기없음. Unity실행·Play·빌드/git게시 없음. 기존설치.NET/Roslyn·Unity참조DLL·번들Python/Node만사용/설치·패키지·전역·연결변경없음.
고지후앱내임시탭/루프백전용서버로 제공50/51영상만프레임디코딩/읽기,게임조작/클립보드접근없음. 정상열람탭닫음/서버종료. 실제게임브라우저/WebGL·Unity/GPU/Physics/모바일검수아님. 요청CursorDash_incoming경로없음/실제_incoming/references동명원본사용·해시유지.
정적31/실제순수2소스.NET3,126/JS모의DOM5+구문PASS,참조C#108/12/Editor2오류0·기존경고134/공식103컴파일만·실행NOT_RUN. 육각 소스·타이밍 사례는 순수 검사에 포함하지 않았다. 공개D-038불변. [근거/범위/검수](FEVER_EXIT_PROJECTED_SCORE_20261026.md).

## D-041 로컬 구현·검사 환경 (2026-10-26)

시작 main/HEAD8c357b8·기존D-039/040·미추적을Before1844파일 해시로 보호했다. 설치된.NET/Roslyn/Unity참조DLL·번들Python/Node만 사용. Unity실행/메모리dirty읽기·저장/Play·실제컴파일·Shader/GPU/Physics·브라우저/모바일·빌드/커밋/push/배포·컴퓨터UI 없음. 기존 미저장씬/TMP 상태를 저장·폐기하지 않았다.
참조 게임108/테스트12/Editor2 오류0, 런타임경고134(시작133+과거attackDuration미사용1). 실제순수4소스+Tuning와명시적메모리어댑터2,293단언/JS모의DOM23·구문/정적28PASS. 공식99작성·컴파일만/실행NOT_RUN. 실제PlayerPrefs/localStorage/자산저장·셰이더와화면은검증하지않았다. 공개D-038불변. [Before/실행근거/제약](HEX_DEFAULTS_FEVER_RECALL_20261026.md).

## D-040 도구 확장 검사 환경 (2026-10-26)

기존설치.NET/Roslyn·Unity참조DLL·번들Python/Node만사용했다. 새설치/패키지·전역/연결·Unity실행/Play·씬저장·빌드·Git커밋/push·배포·컴퓨터UI조작없음. 시작main/HEAD8c357b8·기존D-039변경/미추적파일을별도Before로보존한다.
실제타임라인수학1,277/Tuning소스+명시적메모리엔진·저장어댑터338 PASS. 실제PlayerPrefs/localStorage저장시험은아니다. tools.js실제코드를모의DOM/Unity메시지에서23검사하고구문검사PASS,실제브라우저화면검증은아니다. 참조C#108/12/2오류0,경고133=기존130+보존한과거직렬화필드3개미사용. 공식95작성·컴파일만/Unity·GPU·Physics/Play·모바일·실제저장지속성NOT_RUN.
웹템플릿소스도같은도구에연결했으나공개웹은D-038그대로다. [실행범위/근거](HEX_TIMELINE_TOOLS_20261026.md).

## D-039 로컬 수정/검사 환경 (2026-10-26)

기존Unity/렌더러/패키지/전역설정/연결을바꾸지않았다. Unity실행/Play·빌드·UI조작·씬저장/미저장포괄저장폐기·Git커밋/push/배포없음. 메모리dirty상태실측은미실행이며씬을수정하지않고player.turn의실제기본경로만코드로1800에통합했다. 기존씬의540값은더이상바인딩되지않는다.
기존설치.NET/Roslyn·Unity참조DLL로게임108/테스트12/수동Editor2소스컴파일오류0,경고130(기존동일개수). 실제세순수소스.NET5,425단언/정적21PASS. 공식93작성·컴파일만/실행과Unity컴파일·Shader/GPU/Physics/Play/모바일은NOT_RUN. python 기본별칭은실행불가여서기존번들Python을사용했으며설치하지않았다.
최신공개는D-038 buildId cursordash-20261026-hex6-c430e822d9a1 그대로다. 이번소스/프로필만의로컬변경이며웹에반영되지않았다. [근거/Before/권한](HEX4_FEVER_EXIT_20261026.md).

## D-038 실제 빌드·배포 환경 (2026-10-26)

대상Unity실행프로세스/창·프로젝트잠금이없음을확인해사용자미저장작업을저장/폐기하지않고자기빌드전용Unity6000.3.23f1을한번숨김batchmode로실행했다. 정상종료했고다른프로젝트는종료하지않았다. WebGL/Built-in/기존템플릿·압축설정을유지,새설치/패키지/전역/연결변경없음.
기존GitHTTPS/ghCLI로main/gh-pages·Pages legacy root/기존URL을확인했다. 실제빌드Succeeded/오류0/경고4. 로컬제한실행환경의첫HTTP서버연결실패는게임오류와구분했고그서버를종료후같은127.0.0.1:8882에호스트검증서버를숨김실행해23파일검사/브라우저확인을마쳤다. 마지막에자체종료요청으로닫았다.
실제브라우저는데스크톱앱IAB이다. 로컬1280×720/공개847×856확인,실휴대폰·다른엔진/장시간은NOT_RUN. 자동UnityPlay/공식90실행·사용자설정/최고기록초기화·기존프리셋저장/삭제는하지않았다. [정확한버전/로그/미해결](RELEASE_20261026.md).

## D-037 실행 범위 (2026-10-26)

육각 기존 상태흐름과 실제 회전 읽기를 유지한 파일 수정만 수행했다. 새 패키지/연결·전역·설정/씬·프리팹 저장·Unity UI 조작은 없다. PASS 정적49/실제순수.NET1,135/참조컴파일108·12·2 오류0, 기존 경고130 동일. 공식90 작성·컴파일만/실행NOT_RUN. Unity컴파일·GPU/Shader·nativeDOTween/Physics·Play·모바일·실제타이밍은 미검수다. 자동Play·빌드·커밋/push·배포도 하지 않았다. [공유상수·조절/검수](HEX_CHARGE_135_FIRE6_20261026.md).


## D-036 실행 범위 (2026-10-26)

새설치/연결/전역·패키지·프로젝트설정변경없음. 기존Rotator와Sequence는파일그대로보존했고육각의기존LateUpdate상태흐름에서Preparing과Warning에같은시계를읽는다. 회전시계는발사때자기두컴포넌트만중지한다. 변형/예비동작의실제시간은마지막반회전안에맞춰지고검수창에서실제변형/예비시간·시작/완료/발사를구분해볼수있다.
파일검사/실제순수.NET1,200단언/정적48·기존참조DLL에대한C#컴파일108/12/2오류0,경고130은이전과같다. 공식90작성·컴파일만/실행NOT_RUN. Unity실행·UI/Play·Shader/GPU·Physics·실제시각검사·씬/프리팹저장·빌드/git/배포는하지않았다. 보고시각은현재디스크저장값계산이며사용자의실행오버라이드를실측한것이아니다. [값/조절위치](HEX_CHARGE_246_20261026.md).

## D-035 실제 회전 읽기와 실행 범위 (2026-10-26)

기존DOTween을그대로쓰며첫육각Rotator의소유tween.Elapsed(true)/실행duration·interval을읽는다. 공유컴포넌트의회전Sequence/SetLoops/방향·Ease/Interval표현은바꾸지않았고육각Execute에서자기두링만재시작한다. 원형/HP/기타회전의원래구동은유지한다. 새패키지/설정/연결없음. 기존Built-in 피버광학3기본값만asset/클래스/무assetfallback에동일하게올렸다. Shader/렌더패스/RT수는변경하지않았다.
PASS 정적53/실제순수NodeRhythmMath.NET782/참조C#108·12·2오류0. 미사용옛직렬화충전필드2경고추가(보존목적), Unity에실제로컴파일을요청하거나Play/공식테스트·UI를조작하지않았다. 공식89작성·컴파일만, Unity·GPU/Shader·nativeDOTween/Physics·실제프레임/모바일·Play NOT_RUN. 현재사용자Inspector/Prefs실행값은실측하지않고디스크프리팹·asset기본값을기록한다.
`hex.spin`은다음생성의실제회전/차징속도, `hex.prepare`는둘다회전없을때의대체시간으로기존키/기본값/프리셋스키마는유지한다. 기본5.2초/구체검수는 [기록](HEX_ROTATION_CHARGE_20261026.md). 자동Play·빌드·git·배포/설치·전역·새연결·포괄저장/폐기없음.


## D-034 실행·렌더링 연결 (2026-10-26)

Unity/컴퓨터UI/Play·씬저장·빌드·커밋/push/배포는 실행하지 않았다. 기존 Built-in의 LocalImpactDistortion/Graphics.Blit/임시RenderTexture를 활용해 넥서스·흑백세계·커서 다음/HUD카메라 이전에 FeverImpact.shader를 연결한다. URP Volume을 사용했다고 보고하지 않는다. 새shader/.meta는 프로젝트리소스이며 설치·패키지/연결·전역 변경이 아니다.
정적46/실제순수.NET116,402/참조C#108-12-2 오류0. 공식87 작성·컴파일만/실행NOT_RUN, Unity셰이더컴파일·GPU·RT부하/모바일/이번Play·실제화면은미확인. 현재사용자Prefs/Inspector실행값을읽지않았고 보고값은코드/asset기본값이다. FXoff/왜곡0 또는 접촉위치진단ON이면 최종광학을우회하므로 사용자검수때구분한다. 검수창‘피버광학후처리/호출’은 연결상태/실제Blit호출카운터이지 픽셀품질통과표시는아니다. [검수](FEVER_OPTICS_HOLD_20261026.md).

## D-033 실행 범위 (2026-10-26)

Unity/MCP/컴퓨터UI를 조작하거나 Play/씬 저장하지 않았다. 기존 도구만 사용한 파일 수정과 .NET 순수곡선·참조컴파일이다. 새 NodeRhythmMath.cs/.meta는 프로젝트 내 작은 시각 계산 helper, 신규 패키지/연결·전역설정/플레이어 빌드가 아니다. 게임108/테스트12/Editor2 참조컴파일 오류0·순수161,907/정적41 PASS. 공식85 작성·컴파일만/실행NOT_RUN. Unity컴파일·Shader/GPU·Physics·현재표시·모바일·실제타이밍은 미검수다.
조절은 기존 VisualPrototype.asset의 Fever Node Appear/Retire/Threat Retire Seconds와 육각 ArrowNode의 Charge Beat Rise/Settle Seconds·Elasticity. 기존 저장 프리팹 준비3.5/변형.25/예비.025/비행.06을 읽었고 변경하지 않았다. 개발자 hex.prepare 오버라이드가 있으면 그 실제 준비 길이 안에서 곡선을 재배분한다. [검수/Before](NODE_RHYTHM_EXIT_20261026.md).

## D-032 후속 예외 수정의 실행 범위 (2026-10-26)

Unity나 컴퓨터 UI를 조작하지 않았다. 기존 .NET/Unity 관리형 Vector3/Vector4로 이전 Polygon의 동일 예외를 재현하고 수정한 순수 생성 계산640경우 통과. 네이티브 Unity/Play/GPU 검사와 다르다. C# 참조컴파일107/12/2 오류0, 공식82 작성·컴파일만/실행NOT_RUN. 설치·연결·전역 변경·씬/에셋 저장·빌드·커밋/push/배포 없음. Before·결과는 `Logs/fever-burst-vector-fix-20261026`. 미저장 상태/D-014/기존 작업 보존 규칙 유지.

## D-032 실행 범위 (2026-10-26)

기존 Unity를 열거나 조작하지 않았다. 자료만 읽는 임시 루프백8891 브라우저로 제공 MP4의 프레임을 디코드했으며, 확인 후 전용 탭과 정확히 일치하는 자체 보기 프로세스만 종료했다. Unity·기존 사용자 브라우저·저장 데이터는 건드리지 않았다. 별도 설치/설정·연결 변경 없음.
기존 .NET10.0.402/Unity6000.3.23f1의 로컬 참조 DLL로 격리 C# 컴파일 게임107/공식테스트소스12/Editor2 오류0. Unity 컴파일/셰이더·GPU·Physics 검증을 뜻하지 않는다. 순수 실제4소스.NET34,512단언 PASS, 정적63 PASS. 자동 Play/공식실행/빌드·웹·커밋·push·배포 NOT_RUN.
기존 VisualPrototype.asset에 피버 공급/밀도·수명·응축·배경 반응 조절값을 추가했다. 기존 FeverSettings의 초기18/6개/.85초는 이전 묶음 공급 이력이며 현재 공급은 VisualPrototype의 .12초/동시14를 사용한다. 반경/수명/점수/피버시간은 기존 설정을 따른다. 씬·프리팹·meta/전역 환경은 보존. [검수 인계](FEVER_STREAM_ALIGNMENT_20261026.md).

## 이력 — D-031 파일 기반 구현·제공 영상 확인 (2026-10-26)

환경 재설치/재조사·Unity 실행/재시작·새연결 없음. 기존 AGENTS/STATUS/관련 소스·저장 프리팹 시간을 확인하고 main/HEAD8ceedbb·1,814파일/변경/미추적을 먼저 보존했다. 사용자 dirty/Console/Prefs를 읽거나 포괄 저장·폐기하지 않았다. 새4코드+작은 시각 데이터.asset/전용trailshader/테스트를 기존 Assets에 추가했으며 기존 씬/프리팹/기존asset/meta/설정/패키지는 보존한다.
제공30/06/07만 읽는 루프백8887 뷰어를 숨김 실행해 실제 디코딩·프레임으로 확인하고 정확한 이번 PID/스크립트만 종료했다. 영상 탭 닫음, 초기 연결 실패 페이지 정리는 URL정책에 막혀 재시도하지 않고 임시탭 자동수명에 맡김. Unity/사용자 앱 조작은 없다. 배경 영상은 없었다는 사용자 정정 반영.
기존6000.3.23f1 DLL 참조 컴파일107/12/2 오류0, 실제 순수4소스.NET27,501/정적62 PASS. 공식76·Unity 컴파일/Shader/GPU/Physics·Play·모바일/오디오/실제저장/웹/빌드/git/배포는NOT_RUN. Unity 공식 API의 DontSave 물리 장면/검색 제외만 문서로 확인했으며 이것은 네이티브 재현 실행이 아니다. [수동 절차](FEVER_CONTACT_POLISH_20261026.md). D-014 유지.

## 이력 — D-030 공식 폰트/파일 기반 검사 (2026-10-26)

기존 지침/STATUS와 관련 소스를 확인하고 환경 재조사/새 연결은 하지 않았다. 공식 공게임즈 사이트 HTML의 한국어 라이선스/임베딩 표와 공식 TTF ZIP을 읽었다. 제한된 첫 다운로드는 DNS 실패, 정상 승인 경로의 공개 공식 URL 읽기로 성공했다. 실행 파일·새 패키지·시스템 폰트를 설치하지 않았다. TTF2굵기 원본과 Web 복제본은 ZIP 바이트와 일치하며 조건은 Assets/ThirdParty/Gonggames에 있다. 런타임 소유 SDF 아틀라스를 만들어 기존 TMP .asset/재질/임포트 원본을 저장하지 않는다.
제공 영상3개만 읽는 루프백8886 뷰어의 실제 디코딩/프레임으로 현재 결과와 참고를 구분했다. 작업 탭/정확한 이번 뷰어 PID만 종료했다. Unity/사용자 앱을 조작·실행·종료하거나 Play/테스트/빌드/저장하지 않았다. 자동 재임포트/컴파일이 Editor에서 일어났는지와 메모리 dirty/Console/Prefs는 관측하지 않았다.
기존 Unity6000.3.23f1 DLL의 별도 C# 참조 컴파일103/11/2 오류0. 실제 Unity 컴파일·Shader/GPU 결과와 다르다. .NET 순수2소스11,289단언/정적47/폰트 글리프/JS문법 PASS. 공식68/Unity·오디오·모바일/웹은 NOT_RUN. D-014·전역 설정/사용자 파일 보존. [실제 한계와 수동 순서](NEXUS_WHITE_FEVER_20261026.md).

## 이력 — 피버 보너스 Bind 오류 수정 검사 (2026-10-25)

사용자가 Unity Play 진입의 실제 컴포넌트 충돌/NullReferenceException 로그를 제공했다. 파일 기반으로 원인을 확인하고 표시 Mesh를 자식에 분리했다. 기존 Unity6000.3.23f1 참조 DLL로 게임100/테스트11/Editor2 소스를 컴파일해 오류0이며, 추가 패키지/설치/연결은 없다. 새 EditMode 구성 회귀 테스트는 작성·컴파일만 했다. Unity 실제 상태/Console/Play·GPU·테스트를 자동 조회·실행하지 않았고 저장/폐기/컴퓨터 UI 조작도 하지 않았다. 사용자 미저장 상태를 정리/재시작하지 않았다. 사전1,779파일과 관련 원본은 `Logs/fever-bonus-bind-fix-20261025/Before`, 비교 결과는 `preservation-final.json`이다.

## 이력 — D-029 파일 기반 C 검사 / Unity 화면 미확인 (2026-10-25)

기존 환경 조사를 반복하지 않았다. 도구 목록에는 Unity 실시간 MCP가 없었으며 새 연결/설치는 하지 않았다. 기존 Unity6000.3.23f1 참조를 사용한 별도 C# 컴파일은 게임100/테스트11/Editor2 오류0이다. 직렬화/미사용 필드 경고는 실제 씬 연결 판정으로 해석하지 않는다. 순수 규칙2소스는 기존 .NET에서8,274단언을 실행했고 Unity/Prefs/씬을 호출하지 않았다. JS문법도 통과했다.

제공06/07만 제공하는 루프백8885 뷰어에서 진입/유지/종료를 다시 읽었다. 작업 탭을 닫고, 일반 조회에서 보이지 않던 뷰어는 정상 권한으로 PID/이번 스크립트 경로가 일치함을 확인해 그 프로세스 하나만 종료했다. 기존 사용자 미리보기에는 손대지 않았다. 영상 캡처는 원본 참고이며 이번 C 게임 화면이 아니다.
목적/대상을 먼저 안내하고 컴퓨터 사용 스킬로 기존 Cursor-Dash-master/01.StageSelectScene/6000.3.23f1 창을 식별했다. 캡처는 해당 Unity 내용으로 확인되지 않았고, 창 활성화 도중 사용자 Escape로 컴퓨터 조작이 중단됐다. 추가 컴퓨터 조작을 중지했다. 사용자 현재 PC 이용 불가에 따라 파일 기반 검사만 완료했다. Play/셰이더 GPU 미리보기/Unity 실제 컴파일·Console·dirty/Prefs 메모리는 **NOT_RUN**이다. 저장·폐기·재시작·중복실행/빌드 없음.
`Tools/CursorDash Graphics/9 Fever C GPU Preview (No Play)`는 소스가 검토·참조 컴파일된 선택적 진단이다. 대상 프로젝트/비Play/비컴파일 가드 후 예전 정적 캡처에 실제 셰이더를 Graphics.Blit하고 Logs에만 출력한다. Codex는 호출하지 않았다. 실행되더라도 실제 C 전투 캡처를 대신하지 않는다.
기존 일반 후처리→넥서스/커서 합성 뒤 C 전용 OnRenderImage, 그 다음 고정HUD라는 소스 연결을 확인했다. 디스크 씬·기존meta/설정/패키지·D-014는 보존했다. [상세/검수](FEVER_STAGE_C_20261025.md).

## 현재 — D-028 A/B 파일 구현·독립 논리 검사 (2026-10-25)

Unity환경조사를반복하지않았다. 기존6000.3.23f1참조DLL을이용한C#컴파일94/10/수동창1은오류0이다. 순수FeverState.cs는별도.NET에서실제실행3032단언PASS이며Unity/PlayerPrefs를호출하지않는다. 새패키지/연결/전역설정변경없음. 공식56개는미실행이고UnityEditor실제상태·메모리dirty·Console·GPU검증은NOT_RUN이다.
원본PNG4개는파일보기로,MP4는목적안내후127.0.0.1:8884의지정두파일만제공하는뷰어로확인했다. 둘다오디오트랙0. 최초뷰어설정복사수정실패는열람전종료/교정했으며확인캡처는최종정상두파일이다. 뷰어/작업탭만종료했고Unity/기존미리보기는조작하지않았다.
새게이지는런타임gameboard layer0 Mesh+WorldSpace 배율Canvas,기존넥서스합성/안정된HUD카메라는보존한다. 기존HpFrame의참조/디스크상태는유지하고런타임색만복원가능하게숨긴다. TMP공유원본재질·글꼴자산은저장/초기화하지않는다. D-014유지.
모든실제씬/기존meta/프리팹/설정/패키지보존은`Logs/fever-b-20261025/preservation-final.json`. 공개/기존WebGL은D-026으로이번B를포함하지않는다. [상세](FEVER_STAGE_B_20261025.md).

## 현재 — D-027 로컬 소스/정적 참조 컴파일 (2026-10-25)

전역 `C:/Users/molph/.codex/AGENTS.md`(내용 없음), 상위/루트/하위 지침 존재를 확인했고 저장소 AGENTS/최신 STATUS와 필요한 개발 문서를 읽었다. Unity6000.3.23f1의 기존 참조 DLL로 별도 C# 컴파일만 수행했다. 게임90/테스트9/수동창1 오류0, 기존 직렬화/미사용 필드 경고만 있다. Unity를 실행/중복실행/재시작하거나 Play/테스트/빌드/저장하지 않았다. 실제 Editor 상태·dirty·Console·Prefs 메모리는 읽지 않았으므로 성공 상태를 추정하지 않는다.
기존 번들 Python에 cv2/영상 디코더가 없어 설치하지 않고, 목적을 먼저 알린 후 제공3영상만127.0.0.1:8883에서 브라우저 디코딩/프레임 화면으로 읽었다. 제한된 최초 서버는 브라우저 접근 실패로 종료했고 정상 승인 경로의 루프백 전용 뷰어를 사용했다. 프레임 PNG 다운로드 도구는 시간초과였고 화면 캡처로 대체했다. 종료 시 작업 전용 서버/탭만 닫았다. 기존 미리보기/Unity 프로세스에는 손대지 않았다.
이전 WebGL/public buildId는 D-026 그대로며 이번 수정의 실행 검증이 아니다. 소스/프리팹의 국소 변경과 기존 작업 보존 증거는 `Logs/impact-rail-local-20261025`. [실제 범위](IMPACT_RAIL_LOCAL_20261025.md).

## 현재 — D-026 기존 Editor 빌드·Pages 로딩 검증 (2026-10-25)

제한된 프로세스 조회가 기존 Unity를 보여주지 못했다. 정상 권한의 제목/ID 조회에서 대상 Cursor-Dash-master/Cyberpunk/6000.3.23f1/Web를 발견해 중복 실행을 막고, 실제 열린 Editor에서 새 메뉴로 Play 없이 빌드했다. 초기 배치 실행은 산출물/로그 생성 전 종료, 추가 실행은 중복 방지 가드로 중단되어 이를 성공 빌드로 집계하지 않는다. 목적을 먼저 안내한 뒤 기존 Unity UI를 사용했으며 Save/Discard/Play를 선택하지 않았다. Scene clean/저장7키 동일/TMP imported4개dirty전후동일이다.
WebGL 새 전용출력 circlefx-20261025-60e4c4e52c0d/Succeeded errors0 warnings2. 기존 넥서스셰이더 gradient loop 경고이며 실제 공개메인/JSON로딩Console0이다. 테스트48개는 자동Play금지로미실행. 로컬19파일 전체바이트/공개21HEAD와WASM형식·공개6텍스트커밋바이트/Pages완료·runtimebuildId일치 확인. 실제 전투/터치/모바일/오디오 미검증.
기존 Deployments/cp14-pages-20261024-131718/checkout clean 상태를 재사용해 웹파일만7ead4b3으로커밋·정상push. 원본main8ceedbb 유지. 설치/전역설정/패키지/새MCP연결·원본씬저장 없음. 컴퓨터 UI는 기존메뉴/읽기전용기록/브라우저메인·설정·JSON만 사용. [상세](CIRCLE_PUNCH_RELEASE_20261025.md), 근거 Logs/circle-punch-release-20261025.


## 현재 — D-025 소스 수정·정적 확인만 (2026-10-25)

사용자 Play 종료 통보 후 파일 기반 작업을 진행했다. Unity 실시간 MCP 읽기 도구가 없어 실행 Inspector/Console/렌더 캡처는 NOT_RUN이다. Editor.log 마지막100줄만 읽었고 이번 GPU/셰이더 성공 근거가 없어 성공으로 쓰지 않았다. 컴퓨터 UI·브라우저·Unity Play/테스트/저장·빌드·설치/설정/패키지·Git 변경 명령은 실행하지 않았다.
실제 씬의 CursorVisual(표시만/layer0/SpritesDefault/order2)과 nexus3UI/카메라를 좁혀 읽었다. 새 런타임 커서 표시 자식만 이름 없는 미점유 레이어에 잠시 두고 실제 보조 카메라→투명RT→넥서스 뒤/HUD 앞 순서로 표시한다. ProjectSettings 레이어/원본 씬/콜라이더를 바꾸지 않으며 해제 시 레이어/마스크를 복원한다. 기존 Canvas/HUD 구조는 유지한다. 등록/packed/원본 enabled와 합성 호출을 수동 검수창에서 볼 수 있지만 호출은 GPU/시각 PASS가 아니다.
별도 참조 컴파일 게임90/테스트9/Editor창1파일 오류0, 정적42항목 PASS. 공식48개/Unity/GPU/실제 화면은 NOT_RUN. 원본시트8장·12fps/Controller1.5와 원본 참조를 읽어 기록했다. `Logs/event-vfx-20261025`에1722파일 사전 해시/Before 백업·원본 분석·컴파일·사후 대조가 있다. main/HEAD8ceedbb와 기존 변경 보존. [최신 사용자 검수](EVENT_VFX_20261025.md).


## 현재 — D-024 파일 수정/렌더 소스 확인 (2026-10-25)

기존 환경 조사는 반복하지 않았다. 이 세션 도구 목록에 Unity MCP 읽기 기능이 없음을 확인했고, UI/브라우저 조작으로 대체하지 않았다. Unity/Play/Console/Inspector를 직접 조작·조회하지 않았으며 씬/미저장 상태를 저장/폐기하지 않았다. 설치/전역 설정/패키지/새 연결/빌드/커밋/배포도 없다.
현재 씬 정적값: Main Camera 직교5/depth-1, UI ScreenSpaceCamera/원래 Main Camera 연결/정렬0. 실행 C#은 UI 카메라(depth0·UI layer만)를 만들고 UI Canvas를 그쪽에 연결한다. 넥서스3 Image.enabled만 임시 끄고 실제3이미지는 main 후처리의 NexusGameboard에서 그린다. 소스 Canvas 잔존은 의도된 배치/참조 보존이다. RemakeHUD는 Overlay/정렬25다. 새 수동창에 실제 Canvas/카메라·합성 호출 수·커서 연결·바깥 리듬 상태를 읽어 표시한다. 호출 수가 GPU 성공/시각 승인을 뜻하지 않는다.
게임88파일/테스트8파일/수동창1파일 별도 참조 컴파일 오류0. Unity 셰이더/GPU/공식43개/Play/웹 검수는 NOT_RUN이다. 이전 미리보기/공개 웹은 이 소스가 아니다. `Logs/test-polish-20261025`에 씬 근거/정적검사/Compile/사전1715파일 보존 대조를 남겼다. [검수 절차](TEST_POLISH_20261025.md).

## 현재 — D-023 넥서스 연결·정적 검사 (2026-10-25)

Unity/브라우저/컴퓨터 UI를 직접 조작하지 않았다. 씬/미저장 상태를 저장·폐기하거나 Play/공식 테스트/빌드를 실행하지 않았다. 새 수동 검수창은 소스만 추가했으며 창/버튼을 실행하지 않았다. 현재 Editor Console/GPU 상태를 새로 확인한 것으로 보고하지 않는다.
같은 설치 DLL/Roslyn으로 게임86파일·테스트7파일·새 Editor 검수창1파일을 별도 참조 컴파일해 오류0이다. 진단 출력은 `Logs/nexus-presentation-20261025/Compile`에만 있다. 셰이더/실제 렌더 검증과 구분한다. 설치/패키지/전역 설정·새 연결·Unity/WebGL 빌드·커밋·배포 없음.
소스 씬 정적 확인: UI의 HpBar211×211/HpFrame277×282/HpCentral169×169, Canvas1080×1920·가로 일치·비PixelPerfect·ScreenSpaceCamera. 이미지3개만 분리 표시하며 원본 부모/Rect/스프라이트/머티리얼 자산은 유지한다. Sprite packing/다른 원본 머티리얼 등 지원하지 않는 구성은 원래UI 표시를 유지한다. 넥서스 표시를 기존 안정된 공격경계에 정렬해 새 CanvasScaler 이탈을 막는다. 레이아웃/참조는 `nexus-scene-input.json`, 전체 보존은 `preservation-final.json`을 따른다.
과거8879/8880/공개 게임은 새 소스가 아니다. 실제 수동 Unity 검수가 다음 한 작업이며 공통 타격감 추가 튜닝은 금지다.

## 현재 — D-022 육각 외형 파일 수정·정적 확인 (2026-10-25)

환경 조사를 반복하지 않고 A 원본/현재 ArrowNode·NodeFlatGraphic·저장 프리팹만 읽었다. Unity/브라우저를 조작하거나 씬을 저장하지 않았다. 미저장 상태를 조회·저장·폐기했다고 보고하지 않는다. 원형 외형 승인과 타격감 미승인을 별도 기록했다.
설치된 DLL/Roslyn으로 게임85파일/테스트6파일 C# 참조 컴파일 오류0. Unity Editor/셰이더/GPU/Play 검증은 NOT_RUN이다. 테스트 코드를 작성했지만 자동 실행하지 않았다. 진단 DLL은 `Logs/hex-originalA-20261025/Compile`에만 있으며 게임 빌드가 아니다. 설치/패키지/전역 설정·새 연결·Unity/WebGL 빌드·커밋·배포는 없다.
정확한 사전1694파일/HEAD/허용 수정·신규 파일 대조는 `Logs/hex-originalA-20261025/preservation-final.json`을 따른다. 승인 원형 코드/셰이더·타격감/수치·넥서스·씬·프리팹·원화·기존.meta를 보존했다. 기존8879/8880/공개 웹은 이번 소스를 포함하지 않는다.

## 현재 — D-021 파일 수정·정적 확인만 (2026-10-25)

기존 환경 조사를 반복하거나 Unity/브라우저를 조작하지 않았다. 설치된6000.3.23f1의 DLL과 설치된 Roslyn으로 별도 C# 참조 검사를 수행했다(게임83파일/테스트5파일 오류0). 처음 검사 도구의 중복 monolithic DLL/누락된 netfx facade 참조를 정정했으며 이를 게임 오류로 기록하지 않는다. 최종 로그 `Logs/circle-originalA-20261025/Compile`. 생성된 검사 DLL은 이 Logs 안의 진단 산출물이며 Unity/WebGL 빌드가 아니다. 프로젝트 설정/패키지/전역 설정은 변경하지 않는다.
Unity 자동 Play/공식 테스트/빌드/씬 저장/미저장 상태 조회·저장·폐기를 실행하지 않았다. 현재 Editor 메모리 상태/Console을 새로 확인한 것으로 보고하지 않는다. D-014 imported4개 예외는 유지한다. 레퍼런스 프로젝트는 이번 구현에서 열거나 변경하지 않는다.
Built-in OnRenderImage를 유지하며 URP Volume/새 패키지는 도입하지 않는다. 원형 전용 셰이더와 기존 Background_Flash 이미지 카메라 합성만 추가했다. 실제 GPU 컴파일/UV 방향/화면비/alpha 보존/성능은 사용자 Unity 검수 대기다. 기존 이미지 참조·씬·.meta·설정과 HEAD 보존은 `preservation-final.json`을 따른다. 이전8879/8880/공개 웹은 변경 전 버전으로 남긴다.

## 최신 읽기 전용 분석 — D-020 (2026-10-25)

레퍼런스 `D:/Gamejam2026/Gamejam2026_wave`의 ProjectVersion6000.5.7f1/URP17.6.0,Quality의UniversalRP→Renderer2D 연결과WaveGame카메라PostOn/ScreenSpaceCamera 게임판·Overlay 텍스트를 확인했다. 같은6000.3.23f1 CursorDash Editor로열거나새Unity를실행하지않았다. 레퍼런스파일443개의크기/수정시각과텍스트·메타해시를기록했다. 외부작업경로Git조회는Permission denied로실행되지않아Git상태확인과파일보존확인을구분한다. 권한/전역설정은변경하지않았다.
컴퓨터사용전알림후앱내브라우저의새영상전용탭/127.0.0.1:8882 뷰어로지정영상5개의88프레임을읽었다. 영상원본은읽기전용이며추출PNG/타임스탬프는새분석로그에만저장했다. Unity게임/레퍼런스Play/설치/빌드/클립보드접근은없다. 영상뷰어는분석후닫고해당서버만종료한다. 기존8879/8880미리보기/사용자Unity창은조작하지않는다.
CursorDash의게임파일/씬/.meta/프로젝트설정/저장값은변경하지않았다. 관련문서·에셋합성비교·진단로그만변경한다. 기존D019Unity상태기록은과거증거로만인용한다. 현재Unity메모리상태를새로읽었다고하지않는다. [결과](ORIGINAL_NODES_WAVE_REVIEW_20261025.md).


## 최신 후속 — D-019 구현 종료 (2026-10-25)

기존 Unity6000.3.23f1 같은 프로젝트 인스턴스/정식 Editor TestRunner를 사용했다. Unity MCP 호출 도구는 이번 세션에 노출되지 않아 MCP 성공으로 보고하지 않는다. 새 설치·연결·파이프라인/전역 설정 변경은 없다. Built-in 카메라 OnRenderImage/Graphics.Blit/실시간 효과와 별도 UI 카메라를 유지했다.
새 전용 빌드 `Builds/LocalDevPreview/nodefx-20261025-565495e82312`는 Succeeded/errors0/warnings0/79,685,334bytes,UTC2026-10-24T15:14:34.9168877Z 완료다. 입력1346개+identity 해시 일치/실행19파일HTTP200·동일바이트/WASM MIME을 확인했다. [새 로컬 미리보기8880](http://127.0.0.1:8880/)는 해당 빌드 폴더에만127.0.0.1 바인딩/목록 금지이며 이전8879/공개주소를 교체하지 않는다. 검증19파일과JSON/보고서/390×844 화면·최종웹Console0은 새 근거에 기록했다.
최종 읽기 전용 `editor-state-20261024-152720.json`: Cyberpunk clean,Play/compile/pauseoff,timeScale1;Assets dirty는 D-014 Shader3개/FontTexture1개만이다. 저장7키가 첫 검증 기준과 일치한다(Best36.93275/BGM0/SFX0/v2존재 상태 등). OnFlash는 새 테스트가 별도 개별 복원했으며 전체 저장소 덤프의 동일성은 주장하지 않는다.
사용자가 명시 승인한 Cyberpunk 씬만 백업 후 File/Save했다. 현재 저장본 SHA8404f908b2e0af18b1ddc771fb646b4e3443a3932c9489e69c1fd93efd631af0을 유지했다. 이전 디스크/.meta와 저장본은 새 run에 둘 다 남겨 두었고 과거 디스크로 되돌리지 않았다. 보호1655파일 누락0/기존 사용자 변경·미추적 보존/그 뒤 씬·프리팹·설정·패키지·기존meta 추가 차이0. 고정13검증출력/기존 빌드결과만 새 결과 보관 후 사전 백업으로 복원했다. main/HEAD8ceedbb 유지/커밋·푸시·배포 없음.
최초8880 서버는 브라우저와 격리돼 접속 실패했고 해당 서버만 종료한 뒤 정상 승인으로 같은 루프백 전용 서버를 실행했다. LINE 접근은 의도치 않은 창 선택 단계에서 도구가 차단했고 읽기/전송은 하지 않았다. 이후 정확한Unity경로·제목·유일한창만 사용했다. 이번 클립보드 읽기/승인 반복은 없다. 최종 사용자 탭은8880이며 이전8879 사용자 탭은 조작/종료하지 않았다.
근거 `Logs/hitfeel2-20261024-231337`, [구현 결과](FEEDBACK_REVISION_20261024.md). 이번 작업은 종료하며 다음 한 작업은 사용자와 개발 방식 논의다.


## 최신 실행 — D-018 노드 그래픽 시안 (2026-10-24)

환경 조사를 다시 시작하지 않고 같은 Unity6000.3.23f1/Cursor-Dash-master 인스턴스를 사용했다. 이 세션의 도구 목록에는 Unity MCP 호출 기능이 노출되지 않아 기존 네이티브 Editor의 정식 TestRunner/프로젝트 내부 메뉴를 사용했다. MCP 성공으로 집계하지 않는다. 설치/패키지/새 연결/수동 전역 설정 변경은 없다.
최종 WebGL `Builds/LocalDevPreview/nodefx-20261024-a4818b5046e2`: Succeeded/errors0/warnings0,79,632,633bytes, 완료 UTC2026-10-24T12:51:23.7696536Z. 새 전용 폴더로 빌드했고 이전 출력은 보존했다. `Logs/node-graphics-20261024-192424/build-source-manifest.json`의120개 해시가 현재 입력과 일치한다.
[현재 루프백 미리보기](http://127.0.0.1:8879/)만 제공하며127.0.0.1에만 바인딩했다.19파일HTTP200/정확한WASM MIME,실제 웹 시안/수치/저장/보고서·390×844 검수를 완료했다. 서버는 이 빌드 폴더만 읽으며 디렉터리 목록/외부공개/터널/방화벽 변경은 없다. 사용자 공개CP14 탭은 새 버전으로 바꾸지 않았다. 브라우저 프리셋은8879저장소의 별도이름만 생성했고 기존공개/Editor저장소를 초기화하지 않았다.
최종 읽기 전용 스냅샷 `editor-state-20261024-131438.json`: Cyberpunk clean/Playoff/compileoff/pauseoff,timeScale1,NodeFlatshader지원true. Assets dirty는 D-014의Shader3개/FontTexture1개만이다. 저장/폐기/재시작/TMP초기화하지 않았다. 빌드 전후 최고36.93275/BGM0/SFX0 및 v2관련7키 존재·값은 일치한다. 시작 전 전체 저장 덤프는 없어 전체 세션 바이트 동일성은 주장하지 않는다.
main/HEAD8ceedbb 유지,기존변경20개/미추적294개와467개보호파일 누락0. 기존씬/프리팹/설정/원본이미지/.meta의 새차이는 없다. 자동/출처미확정 `_Recovery/0.unity`와메타등3개는 보존했고 현재Cyberpunk와SHA256이 같다. 고정검증출력13개/기존빌드결과는 새결과보관후백업바이트로복원했다. 구체적보존은 `final-state-comparison.json`을 따른다.
브라우저 복사완료UI와 도구읽기가 달랐으며 최초 Windows 추가클립보드읽기는 자동승인검토가 사적데이터위험으로 거부했다. 우회하지 않았다. **후속 사용자 “보고서 일치 검사 허용” 응답 후 정상 승인 경로의 Windows 로컬 비교 PASS(줄바꿈 정규화)**로 해소했다. 화면의 보고서 재복사 직후 LF/CRLF 외 동일한 문자열임을 확인했고, 클립보드 원문은 출력/저장하지 않았다. UTC2026-10-24T13:54:14Z 결과는 `Clipboard-followup/comparison.json`에 있다. 이번 후속은 문서/진단 결과만 갱신했으며 게임 파일/수치·Unity/테스트/빌드·설정은 바꾸지 않았다. 도구의캔버스드래그/홀드미지원·실기기/오디오/장시간성능NOT_RUN은 환경설치요청으로 확대하지 않는다. [구현·검증 결과](NODE_GRAPHICS_RESULTS_20261024.md).

## 2026-10-24 후속 공개 배포 (D-017)

기존 검증 빌드/85개 소스 해시 일치로 재빌드하지 않고 별도 `Deployments/cp14-pages-20261024-131718/checkout`에서 gh-pages에 실행 파일만 배포했다. Unity 실행/재컴파일/Play/테스트/패키지·전역 설정 변경 없음.
https://edan2251.github.io/renewal_CursorDash/ 의 최신 런타임 ID,21파일HTTP200/WASM MIME, 실제 공개 시작/정규/훈련/Dev/대표 수치/JSON/보고서 복사를 확인했다. 원본 main 브랜치/HEAD와 원격 main은 유지했다. 기존 Pages source gh-pages/루트도 유지했다.
localhost 프리셋과 github.io 프리셋 저장은 별개이며 JSON으로 이동한다. 실기기 모바일는 NOT_RUN. 세부 경로·Pages 실행·보존은 [배포 기록](D:/renewal_CursorDash/docs/CursorDash/DEPLOYMENT_CP14_20261024.md)을 따른다. 아래는 배포 전 환경 이력이다.

<!-- cp14:start -->
## 최신 실행 — CP1~CP4 로컬 WebGL 검증 완료 (2026-10-24)

기존 Unity6000.3.23f1, 대상 `D:/renewal_CursorDash/Cursor-Dash-master`, 기존 `Cursor-Dash-master@6cde4df4002de875`/unityMCP를 그대로 사용했다. 환경 조사를 처음부터 반복하지 않았으며 설치/패키지/새 연결/전역 설정 변경 없음.
컴파일·Play/Stop·공식 run_tests·인메모리 상태 확인·ScreenCapture PNG가 실제 성공했다. 공식 최종27/0/0은 새 JSON/XML/완료 로그·Editor 종료로 교차 확인했다. MCP 진행 캐시는 도메인 재로드 뒤 stale일 수 있어 단독 완료 근거로 쓰지 않았다.

로컬 전용 BuildPipeline helper `Assets/Editor/CursorDashLocalPreviewBuild.cs`는 새 출력 폴더/깨끗한 씬/idle Editor/WebGL을 확인하며 SaveAssets/씬 재생성/기존 ApplySettings를 실행하지 않는다.
최종 출력 `D:/renewal_CursorDash/Builds/LocalDevPreview/cp14-20261024-031826-fe9e3b0bcb8d`, buildId 동일, Succeeded/errors0/warnings0, 79,591,684bytes, 완료 UTC2026-10-23T18:19:04.1718842Z.
첫 두 빌드는 별도 폴더로 보존했다. 첫 빌드의 첫 CanvasScaler 이전 경계 포착을 보완했고, 마지막 빌드는 HTML 삭제 확인 UI만 보완한 캐시 빌드다. `build-final-source-sha256.txt`와 `build-input-audit-final.json`으로 소스 일치를 확인한다.

현재 서버는 **127.0.0.1:8765**만 바인딩했다. localhost 미리보기 외 LAN/외부 공개·터널·방화벽 변경은 하지 않았다. 서버를 다시 시작해야 할 때 다른 프로세스가 포트를 사용 중이면 기존 서버부터 확인한다. 같은 Unity를 batchmode로 중복 실행하지 않는다.

```powershell
python -m http.server 8765 --bind 127.0.0.1 --directory 'D:\renewal_CursorDash\Builds\LocalDevPreview\cp14-20261024-031826-fe9e3b0bcb8d'
```

브라우저 실제 검증: 기존 Chrome와 앱 브라우저에서 HTTP/WebGL, 실제 마우스 이동/해제, 세로390×844 HTML 레이아웃, 이름 프리셋·새로고침·일반 설정·JSON파일 선택/다운로드·텍스트·한국어 클립보드 일치를 확인했다. 브라우저 자동화는 기존 computer-use 경로를 사용했다. 설치나 입력 자동화 패키지 추가 없음.
앱 브라우저 활성 상태 약125~179FPS, 비활성 Chrome 약1FPS 관측은 브라우저 활성/제한 차이와 함께 기록하며 실기기 성능 보증으로 해석하지 않는다. 실제 모바일 터치/키보드/파일 UI는 NOT_RUN이다.
웹 v2설정/프리셋은 **브라우저+정확한 주소의 localStorage**다. Editor PlayerPrefs v2와 기존 Unity Web IndexedDB 최고기록은 별개다. 새 localhost 저장소의 검증용 이름 프리셋1개/예시 파일은 남겼고 복제본만 선택 삭제했다. 다운로드한 `CursorDash-preset (1).json`을 실행 폴더 Examples에 복사했다. 사용자 다른 다운로드/탭/데이터는 정리하지 않았다.

보존 최종: 사전 최고/음량4키·새v2키3개 존재/값 차이0, Resource SO10개 메모리 차이0. Cyberpunk 메모리 highScore11.5285225와 디스크0의 차이를 사전 상태로 보존하며 전체 SaveAssets하지 않았다. 기존 출력13개는 새 결과 별도 보존 후 시작 바이트로 복원했다.
Editor Cyberpunk clean, Play/pause/compile off/timeScale1. dirty는 D-014의 TMP Shader3개+LiberationSans FontTexture4개다. 다른 TMP 폰트 .asset/.meta를 초기화·재생성하지 않았다. 기존백업/미추적241개 유지.
자동 변경: Unity 빌드 시 TimeManager.asset이 .02초 구형 필드를 serializedVersion2/tickCount2822399/ticksPerSecond141120000으로 이관했다(약.0199999929초). 최대 timestep.1은 같고 의도한 밸런스 변경과 구분한다. Git 복구/사용자 diff 일괄 되돌림 없음.
현재 Console에서 DOTween 집계 미관측. Editor Play 종료 `Deleting invalid font reference.`는 스택/원인 미확정 별도 후속 조사로 남긴다. dirty만으로 게임 표시/컴파일 오류라고 판정하지 않는다.

실행 증거: `Cursor-Dash-master/Logs/autonomous-cp14-20261024-011805`, 최신 공식 `CP3-CP4/Run-04`, 실제 웹 `Web-Final`, 최종 빌드 `build-final.json`, 보존 `file-audit-final.json`/`editor-final-check.json`/`preferences-final-check.json`.
[결과](D:/renewal_CursorDash/docs/CursorDash/CP14_RESULTS.md) · [도구 사용법](D:/renewal_CursorDash/docs/CursorDash/TUNING_GUIDE.md). 커밋/푸시/공개 배포 없음. 아래는 과거 이력이다.
<!-- cp14:end -->

<!-- circle1:start -->
## 최신 실행 환경 — 원형 시안 검증 완료 (2026-10-24)

기존 Unity6000.3.23f1, 대상 `D:\renewal_CursorDash\Cursor-Dash-master`,
인스턴스 `Cursor-Dash-master@6cde4df4002de875`, 기존 unityMCP를 재사용했다.
환경 조사 재수행·설치·추가 연결·전역 설정/패키지/ProjectSettings 변경은 하지 않았다.
이번 D-015 승인 범위의 게임6파일·개발 문서5개만 수정했다. D-014는 유지한다.

| 기능 | 실제 호출 결과 |
|---|---|
| 프로젝트/씬/오브젝트 읽기 | PASS. project/info, editor/state, 인메모리 읽기로 대상 경로·실제 씬/프리팹 값 확인 |
| 컴파일 | PASS. 기존 refresh_unity compile=request, Editor compile 종료와 Console 오류 없음 확인 |
| Play/Stop | PASS. 기존 manage_editor로 시작·종료, 원형 화면 관찰. 종료 상태 확인 |
| 공식 테스트 | PASS. 정상 run_tests로 시작된 실제 전체3회 중 최종16/0/0. 결과 JSON/XML/완료 로그 교차 확인 |
| GameView 캡처 | PASS. 기존 ScreenCapture API로 PNG 저장·로컬 열람. 새 캡처 패키지/연결 없음 |
| 실제 마우스/터치 자동 조작 | 이번 NOT_RUN. 내부 콜백·입력 함수·Physics2D 배치 검증과 구분 |

Cyberpunk의 허용된 경계 필드 한 줄을 디스크에 추가했을 때 Unity의 외부 씬 변경 Reload 대화상자가 떴다.
사용자가 Reload를 선택한 뒤 MCP 응답이 돌아왔다. 임의 저장/폐기는 하지 않았다.
테스트 중 도메인 재로드로 get_test_job 진행 캐시가 남거나 session superseded가 발생했다.
tests_running 응답은 실행 실패/BLOCKED 요청으로 기록하고 내부 runner/캐시/권한 우회는 하지 않았다.
기존 MCP가 오래된 작업을 자체 정리한 로그 이후 정상 요청이 성공했다. 코드/연결을 수정하지 않았다.
세부 시행 이력·정확한 최종 Job/완료 시각은 STATUS/VALIDATION을 따른다.

별도 Play 전 snapshot(editor-before-run2.json)과 종료(editor-final.json)의 게임 ScriptableObject9개+
DOTweenSettings가 같고 clean이다. Play가 바꾼 MapInformation 점수/해금 메모리2개만 사전값으로 복원했고 SaveAssets는 하지 않았다.
최고36.93275, Flame 최고 키 없음, BGM0/SFX0과 기존 출력13개 시작 바이트를 보존했다.
종료 Cyberpunk clean, Play/Pause/compile/update false, timeScale1.
D-014 대상 Shader3개 dirty만 남으며 새 저장/재임포트/초기화/dirty 해제를 하지 않았다.
기존 백업·원본/.meta·미추적 파일은 유지했다. 파일 차이와 보호 설정 해시는 file-comparison.json을 따른다.

증거: `D:/renewal_CursorDash/Cursor-Dash-master/Logs/circle-prototype-20261024-000641`.
<!-- circle1:end -->

<!-- reload17:start -->
## 최신 결과 — 대상 재임포트·기존11개 PASS (2026-10-23)

현재 상태: `BASELINE_REIMPORT_AND_EXISTING_TESTS_PASS_WITH_KNOWN_WARNINGS`.
기존 Unity6000.3.23f1과 MCP를 재사용했다. 게임 기능·씬·프리팹·설정·패키지 수정, 빌드·커밋·푸시·배포 없음.
2단계 구현은 시작하지 않고 이 결과 기록 후 중단한다.

### 적용된 보존 조건 — D-014

사용자가 `TMP_SDF SSD.shader`·`TMP_SDF.shader`·`TMP_SDF Overlay.shader`의 Shader와
`Fonts/LiberationSans.ttf`의 Font Texture 하위 오브젝트 **4개만** 미저장 내부 메모리 보존 조건을 해제했다.
현재 로컬 원본/.meta/임포트 설정 기준 재생성을 허용하며 원본/기존 사용자 수정과 백업은 보존한다.
다른 미저장 씬·프리팹·머티리얼·TMP Font .asset·별도 임포트 설정은 예외 밖이다.
전체 저장/폐기·원본 삭제·Git 복구·TMP/Atlas 전체 초기화·Library 삭제·패키지 재설치는 금지다.
개별 저장 API 반복·별도 메모리 추출/복원 도구·dirty 강제 해제는 하지 않는다.
이전 “위4개 메모리 보존 저장 입증 전 차단” 조건은 D-014로 대체됐다.
상세 범위는 DECISIONS D-014 및 MASTER_SPEC의 최신 작업 범위 예외를 따른다.

### 재로드·참조·표시 확인

AssetDatabase.ImportAsset(ForceUpdate|ForceSynchronousImport)를 위4개 파일에만 호출했다.
별도 dirty importer/다른 dirty 에셋/dirty 씬·프리팹 stage가 없음을 먼저 확인했다.
재시작/전체 Refresh/전체 저장 대신 이 경로를 선택했다.
재임포트 뒤 Font Texture는 clean, Shader3개 dirty는 남았지만 별도 저장 입증을 반복하지 않았다.
Shader3개+관련 Mobile Shader는 supported=true, GetShaderMessages0개(테스트 전·후).
원본/.meta22개 해시, importer JSON, 관련 FontAsset/Material/Atlas의 GUID/localID·참조와 clean 상태를 확인했다.
FontAsset/Material의 메모리 직렬화는 시작값과 같다. Atlas는 디스크 바이트·참조·크기로 비교했고 GPU 메모리 전체 보존을 주장하지 않는다.

새 Test-Outputs/Captures의 main/play/pause/result PNG를 실제 열람했다.
한글 시작 안내·일시정지/재시도/홈 버튼·결과 설명과 숫자가 정상 표시되고 해당 화면에 오류 재질은 관찰되지 않았다.
이는 기존 공식 테스트가 만든 화면 검수이며 이미 USER_VERIFIED인 전투를 새 실제 입력으로 재검수한 것이 아니다.

### 공식 테스트 실행·결과

정상 `run_tests(EditMode, assembly_names=[CursorDash.EditorTests])` **1회 승인·실행**.
필터 축소·테스트/기대값 변경·내부 메서드 우회 호출 없음.
Job ID `c4f2be8b96bc45ec92ca4358e6474d8e`.
새 native callback JSON과 XML의 완료 UTC는 `2026-10-23T14:42:28.0619936Z`,
총11개 모두Passed, 실패0·skip0다. 개별 결과는 아래/VALIDATION 또는 결과 JSON을 따른다.
이전 독립 검증 함수2개 PASS는 별도 이력이며 이번 공식11개와 합산하지 않는다.

MCP get_test_job은 도메인 재로드 중 session superseded 오류가 있었고 이후 running/completed0 캐시를 반환했다.
이를 완료 근거로 쓰지 않고, 새로운 native XML의11개 test-case/fullName/Passed와 JSON,
RunFinished 11 passed/0 failed 및 Interaction Mode restored 로그, 실제 Play 종료 상태를 교차 확인했다.
테스트 재실행이나 도구 상태 강제 조작은 하지 않았다. duration 필드는 도메인 전환을 포함한 전체 실시간으로 해석하지 않는다.

### Console·남은 이슈

재임포트 직후 새 셰이더/컴파일 오류 없음.
테스트 마지막 종료 Console에는 기존 계열 DOTween Safe Mode 집계8건 경고가 있다.
과거26건을 해결했다고 주장하지 않으며 개별 오류의 발생 시점/대상은 아직 미확정이다.
Safe Mode/로그 설정 변경·KillAll·게임 코드 수정 없음.
MCP가 `Saving results to: ...xml`을 Exception으로 분류했지만 공식 Test Framework의
ResultsWriter.cs:34는 LogType.Log/NoStacktrace로 이 문구를 출력한다. 게임 예외와 구분했다.

### 보존·종료와 다음 구현 영향

원래 TMP 백업22개를 유지했고 현재 로컬 파일22개도 새 Backup-Current-Disk에 보존했다.
테스트는 기존 출력13개 중12개를 바꿨다. 새로운 PNG/JSON/XML13개는 Test-Outputs에 보관하고,
Captures11개·기존 regression JSON/XML 원본13개는 시작 바이트로 복원했다.
이 출력 복원은 TMP 원본/과거 TMP 백업 덮어쓰기와 관계없다.
최고36.93275/Flame 최고 키 없음/BGM0·SFX0, 게임 ScriptableObject9개가 시작값·clean이다.
종료는 원래 Cyberpunk clean, Play/Pause/compile 꺼짐, timeScale1이다.
원본·참조·설정·기존 미추적240개·HEAD/main 및 전체 변경 비교는 file-comparison.json을 따른다.
Library/Temp/사용 중 캐시·로그 전체를 해시 조사한 것은 아니다.

**TMP 내부 상태 보존 입증과 공식11개 미실행이라는 기존 차단은 해소됐다.**
다음 수치 조정 기반 작업을 막는 확정된 TMP 표시/셰이더/게임 회귀 결함은 이번 범위에서 발견하지 못했다.
DOTween은 별도 후속 이슈로 관리하고 씬 전환·풀/트윈 수명 변경 및 최종 회귀 때 추적한다.
MCP 진행 조회 문제는 다음 자동 검증 전에 native 실행 상태와 함께 확인할 도구 한계다.
이미 사용자 확인한 이동/입력 해제·잔상·원형 제거/폭발·육각 방어/반사/파괴·Pause/Retry 정상은 USER_VERIFIED를 유지한다.
이 판정은 신규 구현·빌드/배포 승인으로 확대하지 않는다.

증거 루트: `D:\renewal_CursorDash\Cursor-Dash-master\Logs\tmp-reload-tests-20261023-233739`.
주요 파일: results.json, reimport-validation.json, Test-Outputs/Logs/CursorDash-regression.json/.xml,
Test-Outputs/Captures, output-preservation.json, file-comparison.json, generated-files.json.
<!-- reload17:end -->

---

## 이전 후속 조사 이력 — 당시 보존 조건이며 현재 위4개에는 적용하지 않음


갱신: 2026-10-23 KST. 환경 재설치/재조사는 반복하지 않았다.
적용 AGENTS/기준 문서를 확인하고 기존 Unity6000.3.23f1, 정확한 프로젝트 경로,
Cursor-Dash-master@6cde4df4002de875 한 인스턴스를 그대로 사용했다.
새 Editor·MCP 연결·전역 설정·Play·재컴파일 없음.

## 후속 TMP 보존 결과

대상4개 ShaderImporter/TrueTypeFontImporter 객체와 관련 리소스를 실제 읽기로 식별했다.
정확한 이름/경로/localID/참조/dirty는 STATUS A와
`D:\renewal_CursorDash\Cursor-Dash-master\Logs\baseline-followup-20261023-230507\tmp-before-inventory.json`을 따른다.
Native Font Texture0×0와 TMP SDF Atlas(4096²/1024²/1²)는 다른 오브젝트다.
후자의 FontAsset/Material/Atlas는 clean이며 저장 대상으로 삼지 않았다.

대상4개 원본과 필수 관련7개 원본, 각 .meta를 포함한22개를
`D:\renewal_CursorDash\Cursor-Dash-master\Logs\baseline-followup-20261023-230507\Backup-Disk-Before-TMP`에 보존하고 해시를 검증했다.
처음 백업 명령의 메타 경로 조합 오류는 저장 전 중단됐고, 경로 수정 후22개 전체를 검증했다.
완성된 정확한 목록은 tmp-disk-backup-manifest.json이다. 디스크 복사/JSON을 완전한 메모리 보존으로 취급하지 않는다.

전체 저장 대신 대상을 지정하는 [Unity SaveAssetIfDirty API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.SaveAssetIfDirty.html)를
라이브 reflection/공식 문서로 확인했다. 위4개 오브젝트에만 각각 호출했으나
모두 dirty=true 유지, 대상과 .meta 해시 변화0, 메모리 JSON/참조 변화0이다.
API 반환 성공과 실제 저장 완료가 다르므로 보존 상태는 **NOT_CONFIRMED**다.
원인을 사용자 편집/자동 생성으로 단정하지 않으며, imported/NotEditable이라는 사실만 기록했다.

SaveAssets/Save All·씬 저장·재임포트·TMP 초기화·Atlas 재생성·모드/참조 변경·dirty 강제 해제는 하지 않았다.
InternalEditorUtility 직렬화 API는 존재 여부만 읽었으며 아카이브 생성/재로드를 수행하거나
복구 가능하다고 주장하지 않았다. 과거 백업으로 현재 내용을 덮어쓰지 않았다.
보존 완료 전 상태를 새 저장 기준선으로 확정하지 않는다.

## 테스트·저장값과 현재 차단

상태 보존 전제가 실패해 이번 run_tests 호출0, 공식11개 모두 BLOCKED/미실행.
이전 자동 승인 거부와 이번의 선행조건 미충족은 별개다.
다른 실행 경로로 우회하거나 테스트/기대값/필터를 바꾸지 않았다.
이전 독립 함수 PASS2개와 공식 테스트를 분리한다.
기존 SetUp/TearDown·출력 쓰기 코드를 재검토했지만 실행하지 않았다.
Prefs 현재값(최고36.93275/Flame 키 없음/BGM0/SFX0)은 이전 기록과 같고 이번 변경 호출은 없다.

현재 필요한 조치는 위4개의 메모리 내용을 실제 복구할 수 있는 지원되는 보존 경로의 확정이다.
동일 권한 승인 반복, 재시작/재임포트/Save All로 대체하지 않는다.
이 문제가 직접 막는 것은 러너/재컴파일·재시작 작업이며 모든 숫자 정의·작은 조정안 검토를 일괄 차단하지 않는다.

## DOTween·사용자 확인과 종료

기존 Editor.log/Console에서 집계1개·missing target or field26건만 확인했다.
개별 대상/발생 단계는 미확정이며 Play 종료는 **집계 출력 시점**이다.
Rotator32개 등의 소스/씬 후보와 낮은 우선순위 후보를 followup-results.json 및 STATUS C에 기록했다.
기존 ErrorsOnly/Safe Mode 설정 유지, 추가 Play·코드 수정·경고 숨김·KillAll 없음.
필요한 후속 정보는 개별 경고/예외와 생성 스택·instanceID·씬·단계별 증가량이다.

이번 사용자 보고의 조작·전투5개 항목은 VALIDATION의 USER_VERIFIED로 반영했다.
Codex 자동검증으로 집계하지 않았고 재검증 입력 도구를 설치/실행하지 않았다.
현재 Cyberpunk clean/Play off, TMP dirty4개 메모리 동일이다.
새 디스크 변경은 문서3개와 위 Logs 폴더의 결과/백업뿐이며 전후 비교는 file-comparison.json을 따른다.
1.5단계 당시 자동 레이아웃/로그 변화는 이번 생성 변화와 구분한다. 이후 구현은 하지 않는다.

---

## 이전 1.5단계 실행 이력 — 과거 상태, 현재 판정은 위 후속 기록을 따름


갱신: 2026-10-23 (KST). main / `8ceedbb7ead4c2b97417739f62fd7904e1a51497`.
**아래는 이전1.5단계 이력이다. 현재 결과는 문서 맨 위 후속 기록을 따른다.**

## 1.5단계 실제 호출·상태

기존 인스턴스 `Cursor-Dash-master@6cde4df4002de875`, Unity `6000.3.23f1`,
프로젝트 `D:/renewal_CursorDash/Cursor-Dash-master`를 먼저 대조했다.
이미 열려 있던 Cyberpunk는 clean, Play 꺼짐이었다. 같은 Editor를 사용했고 새 Editor/batchmode/서버를 시작하지 않았다.
기존 서버 별칭2개를 인스턴스2개로 계산하지 않았다. 전역 설정 해시는 종료 시 동일하다.

| 구분 | 실제 결과 |
|---|---|
| 프로젝트/씬/오브젝트 | PASS: project/info·instances·get_active, execute_code로 런타임 객체/HP/씬/dirty 읽기 |
| Console | PASS: 시작5개(기존 deprecation/MCP 로그), 실행 중 빈 스냅샷, 종료 DOTween warning1개 |
| Play enter/exit | PASS: manage_editor play/stop. Editor pause/resume도 성공. 마지막 playing=false/paused=false/timeScale1 |
| 기존 테스트 | 독립 검증 함수2개 PASS. 공식 Test Runner4+7개 BLOCKED/NOT_RUN |
| Game 화면 | PASS: manage_camera game_view가 Logs 내 PNG8개 실제 생성. 비동기 요청 뒤 실제 파일/크기/해시 확인 |
| 실제 입력 | 기존 @oai/sky의 마우스 Start/Continue/Pause/Retry/Pause Home PASS. drag API는 반환했으나 실제 이동/잔상 성공 증거 없음. 터치 NOT_RUN |
| 종료 상태 | Cyberpunk clean/root18 복귀, TMP dirty4개 직렬화 해시 보존. Game view 최대화는 Play 종료 때 원래 분할 화면으로 돌아옴 |

기존 Computer Use skill을 읽고 해당 기존 런타임으로만 창 관찰·입력을 수행했다.
도구 목록34개라는 사실을 각 도구 실행 성공으로 바꾸지 않았다.
Play 중 editor/state가 stale playmode_transition을 반환한 경우에는 직접 현재 값을 재확인했다.
Console은 수동 clear하지 않았지만 기존 Clear on Play에 의해 시작 항목이 지워져 시작 JSON을 별도로 보관했다.
라이선스/앱 배포 버전·독립 강제 컴파일·WebGL 빌드/브라우저의 검증으로 확대하지 않는다.

## 1.5단계 실행 전 부작용 검토와 제외

기존 ENVIRONMENT 초안 명령, ControlFeelTests·SceneRegression·TestReport·독립 검증 함수와
Test Framework1.6.0/MCP 테스트 진입부를 실행 전에 읽었다.
러너가 dirty 씬을 저장하는 경로, Play 테스트 bootstrap 씬/SaveAssets,
리포터가 기존 Captures/Logs를 덮어쓸 수 있는 점을 확인했다.
기존 Captures11개·regression 결과2개를 새 Logs 폴더로 **복사만** 했고 원본을 옮기거나 지우지 않았다.

순수 4개로 제한한 `run_tests(EditMode, CursorDash.EditorTests, CursorDashControlFeelTests)`부터
자동 승인 심사가 “기존 dirty TMP 에셋을 저장해 미저장 사용자 상태를 덮어쓸 위험”으로 거부했다.
테스트 작업은 시작되지 않았다. 재요청·메서드 직접 호출·별도 배치 등 우회하지 않았고,
같은 진입점의 씬 회귀7개도 호출하지 않았다. 공식11개는 BLOCKED/NOT_RUN이다.
사용자의 미저장 내용 보존 조치와 별도 재실행 승인 전에는 이 경로를 진행하지 않는다.

`ValidateInput`·`ValidateTrailAndPatterns`는 임시 HideAndDontSave 객체/계산/읽기 검사이며
프로젝트 저장/Setup/Apply를 호출하지 않음을 검토한 뒤 기존 독립 함수 자체만 실행했다.
13+17개 assertion, 두 함수 PASS이며 공식11개 테스트와 별개다.
MigrationValidation의 씬 일괄 열기, Setup/Apply/빌드/패키지 변경은 미실행이다.
초안의 batch 명령도 실행하지 않았다.

## 1.5단계 데이터 보호·자동 변경

- 시작 TMP dirty: Assets/TextMesh Pro/Shaders의 TMP_SDF SSD.shader·TMP_SDF.shader·TMP_SDF Overlay.shader,
  Fonts/LiberationSans.ttf의 Texture2D 하위 오브젝트. 종료 때 dirty와 메모리 해시가 동일하며 저장/폐기 안 함.
- PlayerPrefs4개 존재 여부·float 값은 전후 동일(최고36.93275/Flame 키 없음/BGM0/SFX0).
  저장 API/초기화/기록 리셋을 직접 호출하지 않았다. 자연 사망의 점수가 기존 최고를 넘지 않도록 관찰했다.
  입력 격리 중 점수80.94 라운드는 사망 전에 Pause Home으로 종료해 최고 저장을 피했다.
- Play가 바꾼 맵 메모리 highScore/isUnlock만 시작 스냅샷으로 복원했다. SaveAssets 호출 없음.
  game ScriptableObject9개 직렬화 동일·clean, 기존 dirty TMP4개 동일. 디스크 Assets는 해시 동일이다.
- 1588개 기존 파일 비교 중 문서3개와 자동 레이아웃1개 해시 변경.
  자동 로그4개는 잠겨 있어 메타데이터 변경만 관찰, 다른 잠긴 로그1개는 메타데이터 동일/내용 미검증.
  Library/Temp 캐시는 이 전수 비교 범위 밖이다. 자동 변경 파일을 임의로 되돌리지 않았다.
- 레이아웃: UserSettings/Layouts/CurrentMaximizeLayout.dwlt.
  로그: Logs/AssetImportWorker0.log·AssetImportWorker2.log·shadercompiler-AssetImportWorker0-0.log·shadercompiler-AssetImportWorker2-0.log.
- Git main/HEAD·기존 미추적240개·추적 변경0 유지. 비무시1568개 중 문서3개 이외 변경0.
  전역 config.toml/AGENTS/.gitignore 해시 동일. 원본 캡처/결과13개 동일.

## 1.5단계 남은 제약과 필요한 조치

종료 Console은 DOTween Safe Mode가 missing target or field26건을 집계했다.
집계 로그가 Play 종료 OnDestroy에서 출력됐으며 개별 오류 발생 시점·대상/트윈은 로그 부족으로 미확정이다.
정상 UI 조작과 진단 정지·입력 격리가 한 세션에 섞였으므로 진단 영향을 배제한 최소 재현이 필요하다.
시작 Input Manager deprecation은 기존 상태이며 MCP Error/native Warning 표기 차이를 기록했다.
드래그가 매우 짧은 held 신호만 만들어 이동·잔상·반사 검수는 수동 확인 필요다.

현재 필수 설치·추가 연결·전역 설정 변경의 근거는 없다.
필요한 것은 사용자 미저장 TMP 상태 보존 결정, 별도 테스트 재실행 승인, 사람의 짧은 이동/전투 검수,
종료 경고의 분리 조사다. 코드/패키지 변경안으로 임의 변환하지 않는다.

결과 폴더 `D:\renewal_CursorDash\Cursor-Dash-master\Logs\baseline-1_5-20261023-215203`의 verification-results.json,
captures-manifest.json, file-comparison.json, manual-checks.md, generated-files.json과 STATUS/VALIDATION을 함께 본다.
이번에는 결과 기록 후 중단하며 2단계 구현으로 진행하지 않는다.

---

## 1단계 환경 조사 이력 — 아래 1~7절은 과거 시점 기록

아래의 “이번/현재/미실행/다음 승인”은 **1단계 문서 조사 당시**를 가리킨다.
설치 경로·부작용·초안 명령의 근거를 보존하기 위해 남겼다. 실제 1.5단계 판정은 위 표를 따른다.

## 1. 작업 위치와 기존 지침

| 항목 | 확인 결과 |
|---|---|
| 실제 작업 폴더 / Git 최상위 | `D:\renewal_CursorDash` / `git rev-parse --show-toplevel` 일치 |
| Unity 프로젝트 | `D:\renewal_CursorDash\Cursor-Dash-master` |
| 시작 자료 | `_incoming/CursorDash_Dev_Starter_v1`의 START_HERE, 시작 지시문, 템플릿 8개 모두 읽음 |
| 자료 무결성 | MANIFEST의 대상 10개 파일 SHA-256 일치. 원본 미변경 |
| 시작 Git 변경 | 추적 파일 변경 0개, 미추적 파일 232개. 상세 분류는 STATUS |
| 참고 커밋과 관계 | 로컬 객체의 `413f842`는 HEAD 다음의 README 수정 1개. 게임 파일 차이 없음. fetch/pull 안 함 |

지침·설정 출처는 다음과 같다. 인증파일·토큰·환경변수의 비밀값은 열거나 출력하지 않았다.

| 경로 / 적용 범위 | 시작 시 상태와 처리 |
|---|---|
| `C:\Users\molph\.codex\AGENTS.md` / 전역 | 존재, 0바이트. 읽어서 빈 내용 확인, 보존 |
| 같은 폴더의 `AGENTS.override.md` | 없음 |
| `D:\AGENTS.md`, `D:\AGENTS.override.md`, `D:\.codex\config.toml` | 없음 |
| 저장소·Unity 루트와 하위의 AGENTS/override | 시작 시 없음. 캐시·빌드·외부 보관 폴더를 광범위하게 읽지 않고 이름 탐색 |
| `D:\renewal_CursorDash\AGENTS.md` | 이번에 새로 작성하고 현재 대화에서 명시적으로 다시 읽음 |
| 저장소 및 Unity 루트의 `.codex/config.toml` | 없음. 새 설정 파일을 만들지 않음 |
| `C:\Users\molph\.codex\config.toml` / 사용자 설정 | 존재, 조사 시작 4,985바이트. 존재·범위를 먼저 확인한 뒤 MCP 이름/명령 경로/비밀값을 제거한 URL과 해당 저장소 trust 항목만 선별 조사 |
| 기존 문서 | 루트 README, Unity README, `Cursor-Dash-master/docs/CursorDash-asset-inventory-and-cleanup.md`, `folder-reorg-manifest.json` 확인·보존 |

발견된 Codex 설정 루트는 `C:\Users\molph\.codex`이다. 현재 저장소를 trusted로 표시한 경로 표기가
2개 존재하며 그대로 보존했다. 실행 세션의 전체 유효 설정을 이 파일만으로 확정하지 않는다.
현재 세션은 Windows native / PowerShell 7.6.5, 제한된 파일 쓰기 권한을 사용한다.
프로젝트 설정은 신뢰 조건과 상위 정책·실행 옵션의 영향을 받는다. [공식 설정 우선순위](https://learn.chatgpt.com/docs/config-file/config-basic)

새 AGENTS를 작성한 뒤 이 대화에서 다시 읽었다. 다음 개발은 새 대화에서 적용 지침 경로와 STATUS의
승인 범위를 다시 확인해야 한다. [AGENTS 적용 기준](https://learn.chatgpt.com/docs/agent-configuration/agents-md)

## 2. 발견 / 구성 / 연결 / 실제 읽기 검증

`PASS`는 표에 적힌 조사 항목만 통과했다는 뜻이다. 연결과 게임 동작 검증을 분리한다.

| 대상 | 발견 | 구성 | 연결 | 실제 확인 / 남은 검증 |
|---|---|---|---|---|
| Git | `C:\Program Files\Git\cmd\git.exe`, 2.55.0.windows.5 | 기존 저장소·main | 원격 통신 NOT_RUN | 루트·HEAD·상태·로컬 커밋 비교 PASS |
| Codex | 현재 데스크톱 채팅 및 CLI 경로 발견 | 기존 사용자 설정, 저장소 trusted 항목 | 현재 세션 동작 | CLI `--version`은 0.162.1 반환. 앱 배포 버전은 미확인 |
| PowerShell | Windows native, Core 7.6.5 | LocalMachine RemoteSigned, 다른 정책 범위 Undefined | 해당 없음 | 기존 정책을 바꾸지 않고 진단 PS1 실행 PASS. Windows PowerShell 5.1 별도 실행 NOT_RUN |
| Unity Editor | 6000.3.23f1 설치 파일 | ProjectVersion과 실행 중 인스턴스 버전 일치 | 기존 MCP에 대상 1개 | 프로젝트 정보·편집 상태·활성 씬 읽기 PASS. 새 컴파일 NOT_RUN |
| WebGL | 모듈 디렉터리·확장 DLL·BuildTools 존재 | MCP 프로젝트 플랫폼 WebGL | 해당 없음 | 모듈 존재 PASS, 빌드·브라우저 NOT_RUN |
| Unity MCP | 로컬 패키지 10.0.0 및 package.json | 기존 HTTP 서버 별칭 2개 | 대상 프로젝트 응답 | 리소스와 실제 `manage_scene(get_active)` PASS. 변경 도구·테스트 실행 NOT_RUN |
| Newtonsoft | 로컬 패키지 3.2.1 및 package.json | manifest·lock 모두 local | 해당 없음 | 경로·버전 존재 PASS. 새 로드/복원 NOT_RUN |
| Test Framework | manifest·lock·로컬 캐시 모두 1.6.0 | EditorTests 어셈블리, 소스상 11개 | 조회 당시 테스트 실행 중 아님 | 파일 확인 PASS. 테스트 전부 NOT_RUN |
| Unity 라이선스 | 인증·라이선스 파일 조사 제외 | 기존 에디터 응답만 확인 | 해당 없음 | 새 실행·배치/빌드 사용 가능 여부 NOT_RUN |

Unity 실행 파일은 `C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe`이다.
파일 버전 `6000.3.23.643820`, 제품 버전 `6000.3.23f1_09d2ecc7fb28`이며 ProjectVersion의 revision과 일치한다.
WebGL 경로는 같은 Editor 아래 `Data\PlaybackEngines\WebGLSupport`이며
`UnityEditor.WebGL.Extensions.dll`과 `BuildTools`도 존재한다. 이번 작업에서 Unity를 시작하거나 종료하지 않았다.

### 기존 MCP의 실제 관측

- 사용자 설정의 `unityMCP`, `unity-mcp-bosstest`가 모두 `http://127.0.0.1:8080/mcp`를 가리킨다.
  이름 2개를 Unity 인스턴스 2개로 세지 않는다. 변경·중복 제거·새 연결은 하지 않았다.
- `mcpforunity://instances`: `Cursor-Dash-master@6cde4df4002de875` 1개, Unity 6000.3.23f1.
- `mcpforunity://project/info`: `D:/renewal_CursorDash/Cursor-Dash-master`, 빌드 플랫폼 WebGL.
- `mcpforunity://custom-tools`: 제공 도구 34개를 응답. 목록 확인만 수행했다.
- `mcpforunity://editor/state`: idle, Play 꺼짐, pause 꺼짐, 컴파일/도메인 재로드/에셋 업데이트/테스트 실행 중 아님,
  관측 응답 stale 아님. 이 상태가 직전 컴파일이나 테스트의 성공을 증명하지는 않는다.
- 실제 읽기 도구 `manage_scene(action=get_active)`: `Cyberpunk`, `Assets/Scenes/Cyberpunk.unity`,
  buildIndex 1, `isDirty=false`, loaded, 루트 18개. 해당 시점 활성 씬에 대한 응답이며 다른 씬·프리팹의 미저장 상태는 확정하지 않는다.
- 샌드박스의 `Get-Process -Name Unity`와 진단 스크립트는 PID를 발견하지 못했지만 기존 MCP는 살아 있는 편집기를 응답했다.
  셸 프로세스 목록만으로 Unity가 닫혔다고 판단하지 않는다. OS PID·실행 인자와의 대조는 미확인이다.
- 기존 `node_repl`, `blender` 구성도 발견했다. 관련 없는 프로젝트의 서버를 조사·시작하지 않고 설정을 보존했다.

클라이언트별 구성 공유에 대한 일반 기준은 [공식 MCP 문서](https://learn.chatgpt.com/docs/extend/mcp?surface=cli)를 참고한다.
이번 연결 판정은 위 실제 로컬 응답에 근거한다. 씬 읽기 성공을 Play 조작·스크린샷·새 테스트의 성공으로 확대하지 않는다.

## 3. 패키지와 선택 도구

| 패키지 | manifest의 실제 대상 | 확인 |
|---|---|---|
| `com.coplaydev.unity-mcp` | `C:/Users/molph/Documents/Codex/2026-10-03/task/unity-mcp-v10.0.0/MCPForUnity` | 폴더·package.json 존재, name 일치, 10.0.0 |
| `com.unity.nuget.newtonsoft-json` | `C:/Users/molph/AppData/Local/Unity/cache/packages/packages.unity.com/com.unity.nuget.newtonsoft-json@3.2.1` | 폴더·package.json 존재, name 일치, 3.2.1 |

두 의존성 모두 packages-lock의 `source=local`과 대상 경로가 일치한다. 현재 PC의 필수 복구는 발견되지 않았다.
절대 경로이므로 다른 PC 이관 때 재조사는 필요하지만 이번에 Packages를 바꾸거나 캐시를 삭제하지 않는다.
MCP 패키지가 선언한 Newtonsoft 3.0.2 / Test Framework 1.1.31과 프로젝트의 직접 선택 3.2.1 / 1.6.0은 구분한다.
표기 차이만으로 오류나 다운그레이드 필요성을 단정하지 않는다.

기존 MCP 소스의 `Server/pyproject.toml`은 `mcpforunityserver` 10.0.0, Python `>=3.10`,
`mcp-for-unity` 진입점을 선언한다. 실제 서버 프로세스의 Python 경로·버전은 이번에 읽지 않았다.

| 선택 도구 | 발견 경로 | 이번 필요성 / 검증 |
|---|---|---|
| uv | `C:\Users\molph\.local\bin\uv.exe` | 기존 Python MCP 경로와 관련될 수 있음. 위치만 확인, 실행·설치 안 함 |
| Python | PATH의 `C:\Users\molph\AppData\Local\Microsoft\WindowsApps\python.exe`, `py` 런처 미발견 | 앱 실행 별칭만으로 정상 Python 설치를 확정하지 않음. 연결은 이미 응답하므로 추가 설치 근거 없음 |
| Node | `C:\Program Files\nodejs\node.exe` | 게임이 Node 프로젝트를 요구하지 않음. 버전 명령·npm 작업 안 함 |
| .NET SDK | `C:\Program Files\dotnet\dotnet.exe` | 별도 SDK는 현재 Unity 조사·검증의 필수 선행 조건으로 확인되지 않음. 실행 안 함 |
| GitHub CLI | `C:\Program Files\GitHub CLI\gh.exe` | 이번 로컬 조사에 불필요. 로그인·원격 조회·실행 안 함 |

Codex CLI는 `C:\Users\molph\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe`에 있다.
`--version`은 exit 0으로 버전을 반환했지만 내부 임시 arg0 정리·PATH 별칭 생성에 접근 거부 경고가 있었다.
권한을 넓히거나 재실행하지 않았다. CLI 버전 조회 외의 정상 동작을 보증하지 않는다.
데스크톱 앱 버전의 Appx 메타데이터 조회도 접근 거부되어 미확인으로 남겼다.
이는 자동 승인 심사 거부가 아닌 현재 셸 조회의 접근 제한이다. 앱 재설치나 보안 완화의 근거가 아니다.

## 4. 보조 진단 도구의 검토와 실행

파일: `tools/cursordash/Test-CursorDashEnvironment.ps1`.
원본을 전부 읽고 동적 Git 호출·환경변수 복원·후보 경로·파일 읽기 범위를 검토했다.
Unity/MCP 실행, 설치·다운로드·네트워크·파일 쓰기 명령이 없으며 JSON을 표준 출력으로만 반환한다.
새 파일로 원본 그대로 복사했고 원본/복사본 SHA-256은 모두
`45A19847142A59F30C96619F9D3EB5C517EFF0470FA6CF53010CBC0BBEEB5DDB`이다.
PowerShell AST 구문 오류 0개를 확인한 뒤 기존 실행 정책 아래 다음 목록 조사를 실행했다.

```powershell
& 'D:\renewal_CursorDash\tools\cursordash\Test-CursorDashEnvironment.ps1' -RepoRoot 'D:\renewal_CursorDash'
```

결과: exit 0, `generatedAt=2026-10-23T21:21:02.5458782+09:00`, `issues=[]`,
두 로컬 경로·Unity/WebGL 후보 발견, `changesPerformed=[]`. 이때는 새 AGENTS 작성 전이었다.
스크립트 자체는 MCP를 조회하지 않아 `mcpConnection=NOT_TESTED`를 출력하며, 실제 연결 결과는 별도 읽기 조사로 위에 기록했다.
PID 미발견, 표준 설치 후보만 확인한다는 한계도 유지했다. 검사 결과 파일을 자동 저장하지 않았다.

## 5. 기존 Editor 도구·테스트의 부작용

| 기존 진입점 | 소스로 확인한 동작 | 이번 실행 |
|---|---|---|
| `MigrationValidation.Validate()` | 모든 씬을 Single로 열고 프리팹/참조/셰이더/빌드 씬 검사 | NOT_RUN. 열린 씬 상태를 바꿈 |
| `MigrationValidation.BuildWeb()` | 활성 빌드 씬으로 WebGL, 출력 `Builds/Web` | NOT_RUN. 실제 빌드 |
| `CursorDashRemakeValidation.ValidateInput()` | 수학·입력 소유 검사, 임시 GameObject 생성·제거 | NOT_RUN. 이름이 validation이어도 Unity 내부 실행 |
| `CursorDashRemakeValidation.ValidateTrailAndPatterns()` | 잔상 기하·만료·패턴/에셋 검사 | NOT_RUN |
| `CursorDashRemakeSetup.Apply()` | 플레이·메뉴 씬 구성 후 저장, PlayerSettings·빌드 씬 변경 | NOT_RUN. 조사나 필수 복구 절차로 사용 금지 |
| `CursorDashWebGLSettings.Apply()` | PlayerSettings·빌드 씬·템플릿 변경 후 SaveAssets | NOT_RUN. 현재 정상 설정에 자동 재적용 안 함 |
| `Tools/CursorDash Remake/Run Scene Regression` | TestReport의 기존 메뉴. EditorTests 어셈블리 전체 선택 | NOT_RUN. 현재 소스상 총 11개, Play 전환 포함 |

`CursorDashControlFeelTests`의 4개는 순수 EditMode 검사다.
`CursorDashSceneRegression`은 7개이며 공통 SetUp이 씬을 열고 Play에 들어간다.
회귀 실행은 캡처 PNG, `Logs/CursorDash-regression.json`·`.xml`을 쓰고 PlayerPrefs·맵 상태를 변경·복원한다.
테스트 이름·Editor 어셈블리라는 이유로 읽기 전용이라고 취급하지 않는다.

## 6. 첫 구현 전 실제 검증 명령 — 미실행 초안

**아래 명령은 이번에 실행하지 않았다. 실행 승인 후 해당 시점의 HEAD·변경·연결·미저장 상태를 다시 확인한다.**
우선 현재 열린 Unity를 이용하는 검증 경로를 선택할 수 있다. 실제 제공되는 기존 메뉴와 Test Runner의
클래스 필터를 사용하고, 이번에 확인한 경로/버전과 연결 대상을 재확인한다.
배치 방법을 선택한다면 미저장 씬·프리팹·Inspector 변경의 저장 여부를 사용자가 정하고 정상 종료한 다음에만 실행한다.
같은 프로젝트를 열린 Editor와 배치에서 동시에 열지 않는다. [Unity 6000.3 명령행 기준](https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html)

기존 Test Framework 1.6.0의 `SettingsBuilder.cs`에서 아래 필터·결과 인자를 직접 확인했다.
해당 구현이 `-quit` 사용을 경고하므로 테스트 실행에는 `-quit`을 넣지 않는다.
온라인 1.6 문서 조회는 접근 실패였으며, 로컬 공식 패키지 소스를 근거로 한 초안이다. 실행 종료 동작은 아직 검증하지 않았다.

```powershell
# 미실행: 실행·출력 폴더 생성 승인 및 기존 Editor 정상 종료 후에만 사용.
$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'
$Project = 'D:\renewal_CursorDash\Cursor-Dash-master'
$ResultDir = Join-Path $Project ('Logs\CursorDash-baseline-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$null = New-Item -ItemType Directory -Path $ResultDir
```

### A. 순수 EditMode — 기존 조작감 수학 4개

```powershell
# 미실행 초안
& $UnityExe -batchmode -projectPath $Project -runTests -testPlatform EditMode `
  -assemblyNames CursorDash.EditorTests -testFilter CursorDashTests.CursorDashControlFeelTests `
  -testResults "$ResultDir\editmode-control.xml" -logFile "$ResultDir\editmode-control.log"
```

### B. PlayMode 동작 — 기존 EditMode 실행기를 통한 씬 회귀 7개

```powershell
# 미실행 초안: 아래 EditMode 분류 안에서 실제 EnterPlayMode/ExitPlayMode를 수행한다.
& $UnityExe -batchmode -projectPath $Project -runTests -testPlatform EditMode `
  -assemblyNames CursorDash.EditorTests -testFilter CursorDashTests.CursorDashSceneRegression `
  -testResults "$ResultDir\scene-regression.xml" -logFile "$ResultDir\scene-regression.log"
```

순수 EditMode와 같은 실행 조건으로 오해하지 않는다. 렌더링·화면 캡처가 있어 `-nographics`를 넣지 않았다.
독립 `-testPlatform PlayMode`용 어셈블리는 발견하지 못했으므로 그 명령의 유효 실행 대상은 `BLOCKED`이다.
새 테스트를 만들어야만 기존 7개를 실행할 수 있다는 뜻은 아니다.
실행 뒤 종료 코드와 XML의 발견 개수·결과를 함께 확인하며 0개 발견을 PASS로 처리하지 않는다.

### C. 참조 감사와 수동 시각·입력 검수 — 별개 결과

```powershell
# 미실행 초안: 씬을 교체해 여는 참조 감사. 테스트나 시각 검수를 대체하지 않는다.
& $UnityExe -batchmode -quit -projectPath $Project `
  -executeMethod MigrationValidation.Validate -logFile "$ResultDir\reference-audit.log"
```

시각·입력 검수는 승인된 Play 진입 후 메뉴→시작→일시정지/복귀→결과→재시도/홈,
원형 경고 중 접촉 제거, 육각의 살아 있는 잔상 방어와 반환 도착, 드래그·터치·버튼/UI 입력 차단,
포커스 상실·놓기/관성·급반전 경로를 실제로 보고 기록한다. 테스트가 만든 캡처만으로 연속 동작·터치 검수를 완료하지 않는다.
기존 캡처와 과거 로그는 이번 실행 증거로 승격하지 않는다.

### D. WebGL 빌드와 브라우저 — 별도 빌드 승인 후

```powershell
# 미실행 초안: BuildWeb은 기존 Builds/Web을 쓸 수 있으므로 기존 결과 보존 방법부터 확인.
Push-Location $Project
try {
  & $UnityExe -batchmode -quit -projectPath $Project -buildTarget WebGL `
    -executeMethod MigrationValidation.BuildWeb -logFile "$ResultDir\webgl-build.log"
}
finally { Pop-Location }
```

빌드 로그·종료 코드·출력 확인 후 승인된 로컬 HTTP 제공 방법과 gzip 설정을 조사한다.
현재는 서버 시작·브라우저 실행 명령을 확정하거나 실행하지 않았다. 실제 브라우저 로딩·입력·화면·저장·포커스는
에디터 결과와 분리해 검수한다. 배포는 이 초안에도 포함하지 않는다.

## 7. 부족한 점과 변경 필요 여부

| 항목 | 필요한 조치 / 이유 | 대상·영향 | 복귀·보존 방법 |
|---|---|---|---|
| 현재 PC 필수 설치·패키지·MCP 수정 | 필요하다는 증거 없음. 현재 구성 보존 | 변경 대상 없음 | 해당 없음 |
| 실제 기준 검증 | 다음 사용자 승인 후 A/B 및 수동 검수. 새 컴파일·기능 동작 증거가 아직 없음 | Unity 상태, Captures, Logs, PlayerPrefs/맵 런타임 값 | 시작 상태·기존 캡처 보존, 테스트 복원 결과 확인, 정상 Play 종료. 자동 Git 복구 명령 사용 안 함 |
| 배치 검증을 선택할 경우 | 같은 프로젝트 중복 실행 방지를 위해 사용자 저장 결정과 정상 종료 필요 | 열린 씬/프리팹·Editor 상태 | 미저장 내용 강제 저장·폐기·강제 종료 안 함. 종료 전 사용자가 정한 상태 보존 |
| 절대 file 경로의 PC 종속성 | 현재는 유효. 다른 PC 이관 때 경로·원본 버전을 재확인하고 필요 시 별도 최소 변경안 작성 | 향후 manifest/lock 후보, 이번 변경 없음 | 기존 파일·정확한 10.0.0/3.2.1 출처 보존, 승인된 변경분만 되돌릴 수 있게 준비 |
| 앱 버전·OS Unity PID | 현재 셸의 메타데이터 접근/가시성 제한. 필요할 때 기존 앱 About/OS 읽기 경로로 재확인 | 문서 정확도 보완, 게임 작업의 설치 선행 조건 아님 | 추가 권한 확대·재설치 없이 미확인 유지 가능 |
| 별도 PlayMode 어셈블리 | 현재 없음. 독립 러너 확장이 실제로 필요할 때만 제안 | 향후 테스트 코드/asmdef 후보 | 기존 11개 기준을 먼저 보존. 이번 새 테스트 작성 없음 |

현재 단계의 문서·조사는 완료 가능한 상태다. 설치·새 MCP 연결·전역 설정 교체를 요구할 실제 장애는 발견하지 못했다.
다음 한 작업은 **승인 후 기존 환경의 실제 기준 검증**이며 2단계 구현보다 먼저 한다. 이번 작업은 문서 정리 후 중단한다.
