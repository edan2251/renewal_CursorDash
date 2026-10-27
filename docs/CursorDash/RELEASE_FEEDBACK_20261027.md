# D-060 현재 누적 피드백 버전 릴리스 (2026-10-27)

작업 ID: release-feedback-20261027 / 프로젝트 CursorDash.
원본 `D:/renewal_CursorDash/Cursor-Dash-master`, Git `D:/renewal_CursorDash`, main 기준 `18b25da2a2f1d067a028a1ee855c9c8419fd63c4`.
프로필 미설정(사용자 명시 경로 사용), 현재 Windows PC, Slack workspace/channel/thread 미설정·NOT_SENT, Claude 독립 QA NOT_RUN. 다른 프로젝트/공유 lastactiveproject는 사용하지 않는다.

이번 사용자 지시는 현재 D-055~059 누적본의 빌드·선별 커밋·정상 push·기존 Pages 게시만 허용한다. 새 기능·디자인·밸런스 변경은 하지 않는다. 자동 Unity Play/포괄 저장·폐기/설치·전역 설정/force push·이력 변경 금지는 유지한다.

## 승인 범위와 현재 구현

동일 Cyberpunk 공간과 점수 TMP의 메인→시작→플레이→최종 파괴→이번점수 확인→BEST 흐름, 정상 득점 효과 조회의 기록 제외 오판 수정, Nexus 호흡/자성 원호·표시 되감기, 실제 피해의 붉은 공간 굴절/회로 우선권, 처치점→전체 회로 전파, 피버 점수획 RGB, 이번점수 호흡을 포함한다. 일반 노드/육각 타이밍/전투·점수·피버 규칙은 유지한다. 조절값은 VisualPrototype.asset과 기존 개발자 도구·각 D-055~059 문서를 따른다.

## 보존과 빌드 전 확인

Cyberpunk 씬 dirty=false, Play=false, compiling=false. 기존 imported shader3/font texture3의 미저장 내부 상태를 저장·폐기하지 않고 유지한다. Library 내부 MonoScript dirty는 사용자 씬 변경과 구분한다. 백업/전체 Git 상태는 `Cursor-Dash-master/Logs/release-feedback-20261027/Before`, `before.json`, `git-status-before.txt`에 있다. 기존 미추적 Recovery/Archive/영상/캡처/진단/도구/배포 작업폴더는 보존하고 커밋·공개 배포에서 제외한다.

기존 빌드 helper의 안전 검증(대상 프로젝트/idle/저장된 씬/기존 WebGL 타깃/새 전용 출력)을 재사용하는 이번 릴리스 메뉴만 추가한다. 게임 실행/SaveAssets/씬 저장은 호출하지 않는다. 등록된 Cyberpunk→legacy01 씬 순서, 기존 템플릿/압축/패키지 설정을 유지한다.

## 검증·산출물·배포

**완료: 소스 main `2c8ad699fc0138513f8112f2ec0703194438c575` / 배포 gh-pages `de9ec5fae6394f2dc64c36166206667b69f4ec53` / buildId `cursordash-20261027-feedback-4694b4ff0c22`.** 소스41파일선별·정상push, 웹6개변경(기존Build4개교체/index/version) 정상push. [Pages 작업 40516160868 성공](https://github.com/edan2251/renewal_CursorDash/actions/runs/40516160868). 사후문서커밋은 이 빌드 소스 뒤에 별도로 기록하며 게임입력변경이없어재빌드하지않는다.

| 항목 | 실제 결과 |
|---|---|
| Unity WebGL | PASS 6000.3.23f1 / 오류0·경고8 / 259.308초 / 86,214,605바이트 |
| C# 참조컴파일 | PASS runtime112/test14/Editor2 / 오류0·기존134경고 |
| 순수 로직 | PASS 4,672,999 + 집중106,885 / Unity실행시험아님 |
| 입력 보존 | PASS 1,489파일빌드전후해시동일 / 자동파일변경0 |
| HTTP/해시 | PASS 로컬24·공개25 / 모두200·GitSHA256일치 / WASM application/wasm |
| 실제 IAB | PASS 메인·시작 클릭/정규진입·마우스이동/궤적·훈련·개발자패널/JSON·보고서/Dev해제·Main복귀 |
| 런타임 식별 | PASS 양쪽 새buildId/.15변형/.20돌진/.60커서/player.turn1800 |
| Browser console | PASS 로컬/공개 warn/error 각각0 |
| Native Play/공식NUnit | NOT_RUN 이번재실행없음/과거70PASS와구분 |

전용 빌드 폴더: `D:\renewal_CursorDash\Builds\LocalDevPreview\cursordash-20261027-feedback-4694b4ff0c22`. WASM SHA256 `a4a820dea68e7119cecd6d35468ab2332ef56e2ce227329a5ea12e3c9c678bb1`. 전체25개 배포Git해시는 `deployment-runtime-manifest.json`에 기록한다. Git정규화로 일부텍스트의CRLF/LF만다른항목은 별도기록하고 바이너리/공개검수는실제Gitblob기준이다.

경고8: 기존 ArrowNode 미사용 직렬화필드6개와 NexusGameboard gradient-loop 경고 DX11/GLES3 각1개. 게임동작을바꾸어숨기지않았다. 추가C++큰TMP메서드컴파일진단도로그에남기며 이번기능오류로단정하지않는다.

빌드뒤실제Editor: Cyberpunk dirty=false/Play=false/compile=false/timeScale1, imported6의대상동일·확인게임prefs동일. 시작4573파일중배포checkout교체를제외한삭제0. 진단중사용자포커스변경을감지한뒤새상태를읽고진행했으며사용자작업저장·폐기없음. 자기서버127.0.0.1:8887은HTTP종료200/프로세스정상종료0, Unity와다른프로젝트는그대로다.

최초 제한된네트워크범위의HTTP연결실패/오류탭은정상승인경로의전용루프백서버로해결했으며기존실패를성공으로덮어판정하지않는다. Pages대기는기존gh-pages허용정책과리뷰어요구없음을읽기확인했고설정변경없이정상완료됐다.

이번 실제증거는 Logs/release-feedback-20261027의 local/public-main.png, public-main-final.png, local/public-input.png, local-training.png, public-report.png와JSON·HTTP·console이다. 화면표본·짧은도구마우스조작이며연속영상·실휴대폰검수가아니다.

수동순서: 공개새버전확인→메인/시작·실손이동→처치전체회로/피격우선→충전되감기/HP0공용피격·기존파괴→피버점수RGB→이번점수호흡/확인클릭/BEST정상저장→Pause/Home반복→실모바일비율/터치. 저장값은검수목적으로초기화하지않는다.
과거 D-057 native70 PASS는 과거 실행 증거이며 D-058/059 최신 화면 통과가 아니다. 실제 모바일/터치/장시간 성능/전체 전투/저장 앱 재실행 검수는 별도 NOT_RUN이다.

배포 대상은 `https://edan2251.github.io/renewal_CursorDash/`, 기존 main 소스/gh-pages 산출물 구조. 직전 정상 배포 gh-pages `3195915a14e1e06b3867ed4cc0db530f3260b81a`를 복구 비교 기준으로 보존한다. 게시 전 실패 빌드는 사용하지 않는다.

## 다음 작업과 보류

배포 검수 후 사용자 공개 PC/모바일 검수 대기. 일반 노드 겹침, 새 Flash Background 그림, 별도 사이버 재구축, 설정창 전면 개편은 보류. 육각 타이밍 재검토 제외. 이번 한정 Git/빌드/배포 승인은 완료 후 자동 연장하지 않는다.
