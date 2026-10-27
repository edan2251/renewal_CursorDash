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

진행 중: 실제 소스·배포 커밋, buildId/해시, Unity 빌드 결과, 로컬/공개 브라우저와 파일 검수는 완료 후 이 문서에 기록한다. 현재 미실행을 PASS로 간주하지 않는다.
과거 D-057 native70 PASS는 과거 실행 증거이며 D-058/059 최신 화면 통과가 아니다. 실제 모바일/터치/장시간 성능/전체 전투/저장 앱 재실행 검수는 별도 NOT_RUN이다.

배포 대상은 `https://edan2251.github.io/renewal_CursorDash/`, 기존 main 소스/gh-pages 산출물 구조. 직전 정상 배포 gh-pages `3195915a14e1e06b3867ed4cc0db530f3260b81a`를 복구 비교 기준으로 보존한다. 게시 전 실패 빌드는 사용하지 않는다.

## 다음 작업과 보류

배포 검수 후 사용자 공개 PC/모바일 검수 대기. 일반 노드 겹침, 새 Flash Background 그림, 별도 사이버 재구축, 설정창 전면 개편은 보류. 육각 타이밍 재검토 제외. 이번 한정 Git/빌드/배포 승인은 완료 후 자동 연장하지 않는다.
