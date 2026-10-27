# D-066 설정·일시정지·튜토리얼 최신본 릴리스 (2026-10-28)

**RELEASE_COMPLETE / USER_VERIFIED UI / WEBGL_BUILD_PASS / LOCAL_WEB_PASS / PUBLIC_WEB_PASS.**

- 공개 주소: https://edan2251.github.io/renewal_CursorDash/
- buildId: `cursordash-20261028-settings-bb06638d1e46`
- 실제 빌드 소스 main: `0ad2e9945435bfcb5e900e069995fb8fa0f6039a` — `[Release] Add settings sheet and pause controls` (31파일).
- 웹 배포 gh-pages: `6909ed2c187e4e221e798b45ba4b513b6aed99c0` — `[Build] Deploy settings-bb06638d1e46 from 0ad2e99`.
- Pages 작업 [40576817544](https://github.com/edan2251/renewal_CursorDash/actions/runs/40576817544) success, Pages API built/동일 SHA, 2026-10-28 05:18:41KST 게시 완료.
- 이 인계 기록은 위 빌드 소스 뒤의 별도 `[Docs]` 커밋으로 main에 보존한다. 문서 변경은 게임 입력에 포함되지 않아 재빌드하지 않는다.

## 승인과 구현 상태

사용자는 UI 조작이 정상임을 직접 확인했다(**USER_VERIFIED**). 이번 승인 범위는 현본 검사·WebGL빌드·관련 선별커밋/정상push·기존Pages게시와 공개검수까지다. 새 기능/디자인/밸런스는 변경하지 않았다. 이번 권한은 완료 뒤 자동 연장하지 않는다.

기준 main bded6a49a0292bb49302d6d59e4045e70d54bec5에 이미 보존된 독립 자동고스트/소유권분리·원형90%접촉·고스트입력.70·실습원형/육각교대2+2·첫흰확산즉시안내/구축후2초hold+2초fade·본래8초Fever를 유지한다. 이번31파일은 메인하단설정7슬라이더·중앙Pause2슬라이더, 기존Tuning/Storage 공유·해제게이트/음량1회출력·효과강도/완화경로·기록보호초기화·재보기, 입력가로채던shade첫형제배치, Slider12×12/Track4/터치48, 관련테스트·문서와빌드ID/helperguard를포함한다. 씬/프리팹/패키지/ProjectSettings는수정하지않았다.

육각타이밍/일반공급·피해·반사/점수·배율·피버8초/승인파편·커서·결과대기는보존한다. 일반노드겹침/새Flash그림/별도사이버재구축은계속보류다. 설정·조절위치는 [UI 인계](SETTINGS_PAUSE_UI_20261028.md)를따른다.

## 실제 실행한 검사

| 구분 | 결과와 범위 |
|---|---|
| 사용자 UI | USER_VERIFIED — 최신본문의 실제 UI조작 정상확인 |
| 정적·참조컴파일 | PASS — diffcheck, C# runtime118/test17/Editor2 오류0(기존참조경고133), 템플릿JS구문검사 |
| 기존 동일 게임소스 집중Unity | PASS — 정상TestRunner 고유7개 최종PASS(최초5PASS/2검사준비FAIL→핵심단언유지·준비수정뒤2개재검사PASS,04:46:47KST). EventSystem raycast/실UI이벤트·물리·GPU39PNG이며 OS손/터치와구분 |
| 이번 실제WebGL | PASS — 기존Unity6000.3.23f1/WebGL/NoPlay메뉴1회, 오류0/경고8,312.1745769초,86,340,928bytes,05:13:11KST. 경고8의이번Console상세분류는미조회 |
| 소스일치 | PASS — 빌드전후 Assets/Packages/ProjectSettings1518파일해시동일·추가게임변경없음 |
| 로컬파일 | PASS — 웹24파일HTTP200/SHA256, WASM application/wasm |
| 로컬실WebGL | PASS — 메인/설정7개·슬라이더100→70→100/닫기/시작·실튜토리얼객체/실드래그커서·궤적/Pause2개·공유100/계속·Home. 브라우저warn/error0 |
| 공개Pages | PASS — 작업success+Pages built/배포SHA일치,25파일HTTP200/Git blob SHA256·WASM MIME일치·쿼리없는URL의새ID/소스SHA확인 |
| 공개실WebGL | PASS — 실제메인·설정열기닫기·7슬라이더/시작·실객체/드래그/Pause2슬라이더/계속·Home, warn/error0. 공개저장값초기화·변경없음 |
| 미검증 | NOT_RUN — 실휴대폰/터치·다른브라우저엔진·전체전투/튜토리얼정상완주·피버전체·장시간성능·앱재실행저장·전체테스트·독립Claude QA |

로컬1280×720 및847×856에서확인했다. 중간창크기가변경되어첫Continue좌표가맞지않아최신화면기준으로다시선택한뒤정상복귀했다. 모바일검증으로간주하지않는다. 첫로컬서버의제한된네트워크접근WinError10013/탭timeout은임시서버·HTTP검사를정상승인경로로실행해해결했다. 게임변경이나보안설정변경은없다.

## 보존·공개 범위

시작main/원격동일·미푸시0/index빈,기존Pages de9ec5f를확인했다. 독립배포체크아웃 `Deployments/cp14-pages-20261024-131718/checkout`의gh-pages만사용했고소스브랜치전환·force·이력재작성없음. 이전25파일을 `Cursor-Dash-master/Logs/release-settings-20261028/PagesBefore`에보존하고,해시가일치한이전Build4파일만새웹파일로교체했다. 배포총25파일에로그·백업·진단·인증정보·참고영상은없다. Git의기존줄바꿈정규화5개텍스트는Git blob을공개해시비교기준으로사용했다. 생성framework.js의기존형식trailing whitespace1줄은실행오류가아니며생성코드를임의수정하지않았다.

작업전1609파일과현재상태비교PASS/삭제0·예상외변경0,빌드입력1518변경0. 사본 `Cursor-Dash-master/Logs/release-settings-20261028/Before`와로컬진단기록을보존한다. 기존미추적·Recovery/Archive·백업/영상·사용자작업은선별커밋밖에남긴다.

시작Unity읽기진단05:04:47KST: 대상Cyberpunk clean/PlayOFF/timeScale1,기존imported Shader3+FontTexture3보존. 빌드중PC사용재개뒤추가OS입력없음. 빌드뒤메모리dirty/prefs재조회는NOT_RUN(사용자PC작업을방해하지않음),디스크1518동일/Play·저장·폐기없음. 다른프로젝트종료없음. 임시loopback8888서버정상종료/로컬탭정리,공개탭은Main으로복귀해남긴다.

## 증거와 다음 한 작업

실제로그: `Cursor-Dash-master/Logs/release-settings-20261028/`의build-result/source-after-build-check/local-http-check/public-http-check/public-bare-url-check/pages-actions-result/pages-build-result/local-browser-check/public-browser-check/final-preservation/server-cleanup JSON. 실제화면 PNG는local-main/settings/slider-70/tutorial-entry/pause/resumed-847/home와public-main-loaded/settings/tutorial-entry/pause/resumed/home다. 연속영상은촬영하지않았다.

작업ID release-settings-20261028 / 프로젝트명CursorDash / 명시원본 `D:/renewal_CursorDash/Cursor-Dash-master` / Git `D:/renewal_CursorDash` / 프로필미설정(사용자경로) / 현재Windows PC. Slack미설정·NOT_SENT/독립QA NOT_RUN. 공유lastactiveproject·전역설정을변경하지않았다.

**다음 한 작업:** 사용자가공개주소에서PC/실모바일설정열기·드래그·닫기→게임시작→Pause조절·계속/Retry/Home를검수한다. 실터치안전영역/오디오체감·완화/재방문저장과튜토리얼/피버전체는사용자확인이필요하다. 새기능/추가빌드·Git/배포는새승인을기다린다.
