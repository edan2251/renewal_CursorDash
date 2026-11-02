# D-080 하단 랭킹·설정 패널 릴리스 (2026-11-03)

**RELEASE_IN_PROGRESS — 빌드·게시 완료로 기록하지 않는다.**

작업ID release-bottom-panels-20261103 / CursorDash / 원본 D:/renewal_CursorDash/Cursor-Dash-master / Git D:/renewal_CursorDash / main 기준 d2801e8d9488e19ad35acb39a14e5a7520ca45e6 / 현재Windows PC. 공통프로필·Slack 목적지 미설정·외부 전송 NOT_SENT, 독립 Claude QA NOT_RUN. 사용자 ‘지금상태로 빌드 커밋 푸시 배포’가 이번 한정 게시 승인이다. 이후 무제한 개발/게시 승인이 아니다.

배포 대상은 기존 https://edan2251.github.io/renewal_CursorDash/ / origin https://github.com/edan2251/renewal_CursorDash.git / main 소스·gh-pages 빌드다. 원격 fetch 뒤 main대비0/0·미푸시0·index빈/기존 gh-pages404818e와 배포checkout clean을 확인했다. force/이력재작성·새호스팅 없음.

## 대상 버전과 보존

D077~079의 현재 하단 랭킹|설정·선택제목/단차/본문가림·100%safe높이·본문여백/하단1:1·섹션reset/튜토리얼·4언어(패널만)·Google아이콘/국기·Sarasa지역형을 배포한다. 실제 Google Play SDK/로그인/순위 연동은 없다. 게임 규칙/육각·커서·노드·피버/파편·점수·저장은 추가 변경하지 않는다. 일반겹침/새Flash그림·별도사이버재구축은 계속 보류한다.

작업전 외부 D:/CursorDash_Backups/release-bottom-panels-20261103/Before 1741파일/159,516,272bytes, Assets/Packages/ProjectSettings/.meta/미추적·관련문서·사용자 저장 Scene/font를 보존했다. 기본 씬 Cyberpunk clean/Unity6000.3.23f1/WebGL/PlayOFF·compileOFF/timeScale1. 이미 dirty인 TMP shader·font texture는 저장·폐기하지 않는다. Native Play는 이번 실행하지 않으며 비Play 검사와 실제 WebGL 브라우저 확인을 분리한다.

빌드식별자 cursordash-20261103-panels-a2436cc6417d. 이번 릴리스 코드 변경은 기존 build helper의 접두사와 BuildIdentity뿐이다. 로그·백업·_Recovery·참고영상·과거 evidence·별도환경검사 스크립트는 선별 커밋에서 제외하고 원본은 지우지 않는다. 라이선스와 재편집에 필요한 SVG/TTF 원본은 관련 자산으로 포함한다.

## 검사·빌드·Git·공개 검증

PASS: 최종 비Play Editor29/29(job657afb9b58b949aab74fb030fe0bc1e6,14.879초), 실제Unity컴파일 오류0. Native Play는0회다. 기존 delayCall의 이번 빌드 요청이 대기열에 머물러 출력이 없음을 확인한 뒤, 해당 목적지의 콜백1개만 제거·동일 ValidateOutput/Build helper를 직접 실행했다. MCP 호출 대기는 timeout이었으나 실제 BuildReport/완료JSON으로 성공을 따로 확인했다. 다른 callback/프로젝트를 실행하거나 제거하지 않았다.

PASS WebGL: Unity6000.3.23f1,270.805초/86,854,480bytes/오류0·경고10. 기존 C#미사용필드6+NexusGameboard shader DX11/GLES3경고2는 동일, 추가2는 빌드 중 MCP WebSocket keep-alive/closed 메시지다. 게임 코드 오류가 아니며 원문 build-warnings.json 보존. 빌드가 비운 font cache2는 BuildBefore로 복원한 뒤1668입력이 빌드 직전과 일치했다. 사용자 Scene/Packages/ProjectSettings/글꼴 원본·기존dirty자산 저장폐기 없음. 보호6문자열3float의 존재/해시 동일·Cyberpunkclean/PlayOFF/compileOFF/buildOFF/timeScale1/previewOFF/missing0 확인.

PASS 로컬웹: 산출물23파일HTTP200/SHA256/WASM MIME 일치, 실제 IAB1280×720 메인/두패널·클릭/드래그열닫기·사운드100→70→reset100·KO/EN/JP/CN→KO·게임진입/실커서입력과꼬리·Pause/계속/Home, consolewarn/error0·새ID loader 확인. 로컬새origin의BEST0은 기존Native저장값과 별개이며 최고점 초기화하지 않았다. 공개 게시/확인과 정확한 소스·배포 커밋은 다음 단계다.

실제 모바일·터치/성능·전체전투/튜토리얼·피버 완주·앱 재실행 저장·독립QA는 NOT_RUN이며 로딩/기본조작 성공으로 확대하지 않는다.

로컬 증거: Cursor-Dash-master/Logs/release-bottom-panels-20261103. 사용자 저장값은 존재/hash로만 비교하고 원문을 공개하지 않는다. 빌드소스와 배포후 인계문서 커밋은 구분한다. 문서만 바뀌면 불필요하게 재빌드하지 않는다. 다음은 검사→성공한 빌드 로컬 확인→선별Git/기존Pages→공개파일/브라우저→최종기록이다.
