# D-080 하단 랭킹·설정 패널 릴리스 (2026-11-03)

**RELEASE_COMPLETE / EDITOR29_PASS / WEBGL_BUILD_PASS / LOCAL_WEB_PASS / PUBLIC_WEB_PASS.**

소스 `8c3e267aec9a289cae67e1120946098ab796b46b`(93파일), 배포 `912892574e1ad3dd9342950b64ecd9bb4b77a50c`를 기존 origin/main·gh-pages에 정상 push했다. Pages 작업 [41473536940](https://github.com/edan2251/renewal_CursorDash/actions/runs/41473536940) completed/success, Pages built 및 동일 배포SHA 확인. 공개 주소 https://edan2251.github.io/renewal_CursorDash/ . 이 문서의 완료 기록은 빌드에 사용한 소스 커밋 이후의 Docs 전용 커밋으로 저장하며 문서 때문에 재빌드하지 않는다.

작업ID release-bottom-panels-20261103 / CursorDash / 원본 D:/renewal_CursorDash/Cursor-Dash-master / Git D:/renewal_CursorDash / main 기준 d2801e8d9488e19ad35acb39a14e5a7520ca45e6 / 현재Windows PC. 공통프로필·Slack 목적지 미설정·외부 전송 NOT_SENT, 독립 Claude QA NOT_RUN. 사용자 ‘지금상태로 빌드 커밋 푸시 배포’가 이번 한정 게시 승인이다. 이후 무제한 개발/게시 승인이 아니다.

배포 대상은 기존 https://edan2251.github.io/renewal_CursorDash/ / origin https://github.com/edan2251/renewal_CursorDash.git / main 소스·gh-pages 빌드다. 원격 fetch 뒤 main대비0/0·미푸시0·index빈/기존 gh-pages404818e와 배포checkout clean을 확인했다. force/이력재작성·새호스팅 없음.

## 대상 버전과 보존

D077~079의 현재 하단 랭킹|설정·선택제목/단차/본문가림·100%safe높이·본문여백/하단1:1·섹션reset/튜토리얼·4언어(패널만)·Google아이콘/국기·Sarasa지역형을 배포한다. 실제 Google Play SDK/로그인/순위 연동은 없다. 게임 규칙/육각·커서·노드·피버/파편·점수·저장은 추가 변경하지 않는다. 일반겹침/새Flash그림·별도사이버재구축은 계속 보류한다.

작업전 외부 D:/CursorDash_Backups/release-bottom-panels-20261103/Before 1741파일/159,516,272bytes, Assets/Packages/ProjectSettings/.meta/미추적·관련문서·사용자 저장 Scene/font를 보존했다. 기본 씬 Cyberpunk clean/Unity6000.3.23f1/WebGL/PlayOFF·compileOFF/timeScale1. 이미 dirty인 TMP shader·font texture는 저장·폐기하지 않는다. Native Play는 이번 실행하지 않으며 비Play 검사와 실제 WebGL 브라우저 확인을 분리한다.

빌드식별자 cursordash-20261103-panels-a2436cc6417d. 이번 릴리스 코드 변경은 기존 build helper의 접두사와 BuildIdentity뿐이다. 로그·백업·_Recovery·참고영상·과거 evidence·별도환경검사 스크립트는 선별 커밋에서 제외하고 원본은 지우지 않는다. 라이선스와 재편집에 필요한 SVG/TTF 원본은 관련 자산으로 포함한다.

## 검사·빌드·Git·공개 검증

PASS: 최종 비Play Editor29/29(job657afb9b58b949aab74fb030fe0bc1e6,14.879초), 실제Unity컴파일 오류0. Native Play는0회다. 기존 delayCall의 이번 빌드 요청이 대기열에 머물러 출력이 없음을 확인한 뒤, 해당 목적지의 콜백1개만 제거·동일 ValidateOutput/Build helper를 직접 실행했다. MCP 호출 대기는 timeout이었으나 실제 BuildReport/완료JSON으로 성공을 따로 확인했다. 다른 callback/프로젝트를 실행하거나 제거하지 않았다.

PASS WebGL: Unity6000.3.23f1,270.805초/86,854,480bytes/오류0·경고10. 기존 C#미사용필드6+NexusGameboard shader DX11/GLES3경고2는 동일, 추가2는 빌드 중 MCP WebSocket keep-alive/closed 메시지다. 게임 코드 오류가 아니며 원문 build-warnings.json 보존. 빌드가 비운 font cache2는 BuildBefore로 복원한 뒤1668입력이 빌드 직전과 일치했다. 사용자 Scene/Packages/ProjectSettings/글꼴 원본·기존dirty자산 저장폐기 없음. 보호6문자열3float의 존재/해시 동일·Cyberpunkclean/PlayOFF/compileOFF/buildOFF/timeScale1/previewOFF/missing0 확인.

PASS 로컬웹: 산출물23파일HTTP200/SHA256/WASM MIME 일치, 실제 IAB1280×720 메인/두패널·클릭/드래그열닫기·사운드100→70→reset100·KO/EN/JP/CN→KO·게임진입/실커서입력과꼬리·Pause/계속/Home, consolewarn/error0·새ID loader 확인. 로컬새origin의BEST0은 기존Native저장값과 별개이며 최고점 초기화하지 않았다.

PASS 공개웹: 배포25파일(산출물23+.nojekyll+version.json) HTTP200/Git blob SHA256 일치, WASM MIME application/wasm. 쿼리 없는 공개 주소에서 실제 IAB1280×720 메인/설정/랭킹·KO→EN→JP→CN→KO/글꼴·국기/상단도달·하단정렬/게임진입·커서입력과잔상/Pause/Home 확인, warn/error0. DOM의 loader는 `Build/cursordash-20261103-panels-a2436cc6417d.loader.js`. version.json도 소스SHA·buildId와 일치한다. 공개 settings의 음량/효과는 바꾸지 않고 언어는 원래KO로 돌렸다. 최고점·완료값 초기화 없음. 최종 공개 페이지는 메인 상태다.

소스 대조 완료 전에 로컬 소스 커밋이 생성된 절차 오류를 즉시 보고했고 push를 보류했다. 이후 필수입력1664개와 HEAD blob이 일치함을 검증한 뒤에만 push했다(텍스트515개 CRLF↔LF만 정규화). 남은4개는 기존 Plugins/Demigiant 위치의 Git제외 `.mdb` 디버그 심볼/.meta이며 실제 runtime/editor DLL은 `Assets/Demigiant/DOTween`에서 커밋됨을 확인했다. 심볼을 강제편입하거나 DLL을 빠뜨리지 않았다. 관련없는 환경검사·영상·로그·사본·_Recovery는 남겨두고 커밋하지 않았다. 기존 gh-pages25파일은 PagesBefore로 보존하고 이전ID 빌드4개만 새ID로 교체했다. force·이력재작성 없음.

실제 모바일·터치/성능·전체전투/튜토리얼·피버 완주·앱 재실행 저장·독립QA는 NOT_RUN이며 로딩/기본조작 성공으로 확대하지 않는다.

로컬 증거: Cursor-Dash-master/Logs/release-bottom-panels-20261103. start-manifest/commit-selection/source-input-check/build-result/build-warnings/local-http/public-http/pages-run/pages-latest/native-after-build와 local/public PNG·browser JSON을 보존한다. 사용자 저장값은 존재/hash로만 비교하고 원문을 공개하지 않는다. 작업전1741파일의 최종대조에서 누락0·예상외변경0을 확인했다(final-audit.json). 변경은 릴리스ID/build helper·관련문서뿐이며 사용자 Scene/자산/설정은 작업전과 동일하다. 이 작업의 임시 로컬 HTTP 서버만 종료했다. 사용자 앱·Unity/다른 프로젝트·MCP 서버 종료 없음.

다음 한 작업은 사용자의 공개 PC/모바일 패널 검수다. 클릭·느린드래그/닫기 → 두 패널 상단과 본문 덮임 → 설정7개·각reset → 하단1:1/4언어 → 시작·Pause/Home 순서. 실제 온라인 순위·다른 UI 번역은 구현하지 않았다. 이번 릴리스 한정 권한은 완료 뒤 자동 연장하지 않는다. 추가 기능/Native Play·빌드·Git·배포는 새 사용자 지시를 기다린다.
