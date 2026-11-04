# D-084 배율 난이도·하단 고정 버튼 WebGL 릴리스

**RELEASE_COMPLETE / WEBGL_BUILD_PASS / PUBLIC_ARTIFACTS_PASS / USER_PLAY_PENDING.** 사용자 ‘이 상태로 빌드 커밋 푸시 배포, 테스트는 직접 WebGL에서’ 승인에 따른 현본 릴리스다. 이번에 Play·자동 테스트·브라우저 게임 조작을 하지 않으며, 빌드/게시 완료와 실제 게임 검수를 분리한다.

## 대상과 범위

- Git: D:/renewal_CursorDash, main 기준 d7cabbe84b6fe790d37bbc698af35817c448e6d3.
- Unity: D:/renewal_CursorDash/Cursor-Dash-master, 6000.3.23f1, Cyberpunk → 01.StageSelectScene의 기존 등록 순서, 기존 WebGL 설정.
- 원격: https://github.com/edan2251/renewal_CursorDash.git, 소스 main / 빌드 gh-pages.
- 공개: https://edan2251.github.io/renewal_CursorDash/ . 새 호스팅/브랜치/force push/이력 재작성 없음.
- 버전: D081/082 하단 랭킹·설정 버튼 고정, D083 배율×1~×4 일반 전투 조합·생성 간격·배치 여유, 관련 폰트 현재본·검사 소스·문서.
- 이번 릴리스에서 게임 디자인·밸런스 추가 변경 없음. 실제 랭킹 연동·일반 겹침 전면 개선·새 Flash/별도 재구축은 계속 보류.
- 빌드 ID: `cursordash-20261105-difficulty-f70fb565edb7`. 테스트 생략이 게임 정상 동작 판정을 뜻하지 않는다.

## 보존과 실행 경위

작업 전 `D:/CursorDash_Backups/release-multiplier-20261105/Before`에 Assets/Packages/ProjectSettings/.meta/미추적 자산·관련 문서·AGENTS/.gitignore 1,755파일(155,396,513바이트)을 보존했다. manifest와 Git 시작 상태는 같은 릴리스 로그에 있다. 백업·Logs·검증 사본·참고 영상·_Recovery·별도 환경 도구는 커밋하지 않는다.

원격 fetch 후 main 대비0/0·미푸시0·index빈, 기존 배포 checkout gh-pages9128925 clean, Pages source gh-pages / 및 기존 URL을 확인했다. 최초 제한 권한 프로세스 조회가 빈 결과여서 Editor가 꺼져 있다고 안내했으나, 최종 권한 있는 조회에서 실행 중인 CursorDash/Project_Brawl을 확인해 안내를 정정했다. 중복 배치 실행은 안전 가드에서 중단되어 새 Editor를 열지 않았다.

사용자가 CursorDash MCP를 연결한 뒤 식별자 `Cursor-Dash-master@6cde4df4002de875`를 **모든 프로젝트 요청에 명시**하고 실제 Assets 경로/Cyberpunk clean/PlayOFF/compileOFF/WebGL·새 BuildIdentity를 확인했다. 기본 인스턴스 선택·서버 설정/재시작·Project_Brawl 요청 없음. 기존 TMP shader/font texture/atlas의 dirty8개는 임의 저장·폐기하지 않는다. `CursorDashLocalPreviewBuild`의 ValidateOutput/Build를 사용하며 Play/씬 저장 없이 전용 새 출력 폴더에 빌드한다. CLI 종료 메서드는 실행 중 Editor에 호출하지 않는다.

## 검증 구분

- 이번 Play/Test Runner/관리형 테스트 재실행: **NOT_RUN — 사용자 직접 WebGL 검수 선택**.
- 이전 D083 참조 컴파일/순수 로직 결과는 해당 구현 문서의 별도 증거이며 이번 실플레이 통과로 사용하지 않는다.
- 빌드 전 실제 CursorDash Console 오류0, 프로필 존재/유효성·빌드 경로/저장 상태 확인.
- 보호 저장: CursorDash Editor 레지스트리12항목·Standalone 없음·persistent3파일을 읽어 작업 전 기록, 값 초기화/게임 실행 없음.
- WebGL **PASS**: 2026-11-05 00:37:26 KST 완료, 오류0·경고8, 234.413초, 86,877,288바이트. MCP HTTP timeout은 빌드 중 전송 대기만 실패한 것으로, 재시작 없이 실제 BuildReport의 Succeeded를 확인했다.
- 빌드 입력1,680파일은 전후 해시가 모두 동일했다(이번 cache 복원0). 빌드 후 Cyberpunk clean/PlayOFF/compileOFF/buildOFF, 기존 dirty 자산8개 그대로, Console 오류·경고 조회0. 사용자 Editor 저장12값·persistent3파일 전후 동일.
- 스테이징32파일/필수 빌드 입력1,676파일 Git blob 일치(줄바꿈 정규화만 허용). 기존 gitignore된 중복 DOTween debug symbol4개는 제외하고 실제 DLL은 추적 상태 유지. 비밀키 패턴/50MB 초과 파일 발견0.
- 전체 `git diff --cached --check`는 신규 Unity YAML의 비어 있는 필드19줄 후행 공백만 지적했다. `.cs/.shader/.md/.txt/.json` 공백 검사는 PASS. 빌드 입력을 바꾸지 않기 위해 Unity 직렬화 원문은 보존한다.
- BuildReport 경고는 기존 ArrowNode 미사용 필드6건(CS0414)과 NexusGameboard gradient 루프 경고2건(d3d11/gles3)이다. 새 난이도 코드 경고는 없다.
- 선별 소스32파일 커밋 및 배포 커밋을 기존 원격에 정상 푸시했고, 이번 배포 SHA의 Pages 완료와 공개25파일 반영을 확인했다. 상세 결과는 아래와 같다.
- 실제 난이도/연속 수비·메뉴 터치·튜토리얼·피버·모바일/WebGL 성능·브라우저 플레이: **NOT_RUN**, 사용자 검수 예정.

증거 위치: `Cursor-Dash-master/Logs/release-multiplier-20261105/`. 상태·커밋·파일 해시를 기록했다. 이 완료 기록을 저장하는 사후 문서 커밋은 빌드 소스 커밋과 구분한다. 문서만 변경되면 재빌드하지 않는다.


## 실제 게시 결과

| 항목 | 결과 |
|---|---|
| 빌드에 사용한 소스 | `62c7d31110a891dd45cded6dd2491ef9dd08c5c7` — main, 관련32파일 |
| 배포 커밋 | `cd36b3f748a67f25684f78c2bdf0daf177d685ed` — gh-pages |
| 공개 주소 | https://edan2251.github.io/renewal_CursorDash/ |
| Pages | [41861139546](https://github.com/edan2251/renewal_CursorDash/actions/runs/41861139546) completed/success, Pages API built·동일 배포 SHA |
| 빌드 | Unity6000.3.23f1, 오류0·기존 경고8, 234.413초 |
| 공개 파일 | 일반 URL25/25 HTTP200·배포 Git blob SHA256 일치, WASM application/wasm |
| 게임 플레이 | **NOT_RUN** — 사용자가 직접 WebGL 검수 |

빌드 산출물은 `D:/renewal_CursorDash/Builds/LocalDevPreview/cursordash-20261105-difficulty-f70fb565edb7`에 있다. 전용 기존 gh-pages checkout의 이전25파일은 `Logs/release-multiplier-20261105/PagesBefore`에 보존했다. 이전 해시명이 붙은4파일만 새 산출물로 교체하고 기존 `.nojekyll`과 배포 구조를 유지했다.

배포 대상25파일을 Git index/commit과 대조했다. 첫 대조에서 텍스트5파일의 CRLF→LF 차이를 발견했으며, 코드/내용이 동일한 줄바꿈 정규화임을 확인해 기록했다. 바이너리/data/wasm은 정확히 일치한다. 공개 확인은 이 실제 배포 Git blob의 해시로 수행했다. 첫 공개 검사에서 폰트2개의 공백 URL 인코딩이 누락됐고, 확인 스크립트만 수정한 뒤25/25를 통과했다. 이 두 검수 도구 문제는 게임·빌드 실패로 기록하지 않는다.

작업 전1,755파일에 삭제0·예상 밖 변경0을 확인했다. 릴리스에서 추가로 변경한 게임 측 파일은 BuildIdentity와 기존 빌드 진입점의 ID 가드뿐이며, 이미 완성된 UI/난이도/폰트 소스를 그대로 빌드·커밋했다. 사용자 저장12값·persistent3파일 동일, 미저장 imported 자산8개 보존. Cyberpunk clean/PlayOFF·compileOFF/buildOFF를 확인했다. Unity 종료·사용자 저장/폐기·다른 프로젝트/서버 조작·컴퓨터 입력 없음.

## 사후 기록과 다음 한 작업

이 문서·STATUS·DECISIONS·MASTER_SPEC·AGENTS의 완료 기록은 **소스62c7d31 뒤의 별도 `[Docs]` 커밋**으로 main에 푸시한다. 게임 소스는 변경하지 않으며 문서만으로 재빌드하지 않는다. 정확한 사후 문서 커밋은 Git 이력과 최종 응답에 남긴다.

다음 작업은 사용자 공개 WebGL 검수다. 권장 순서는 하단 랭킹/설정 열기·같은 버튼 닫기 → 정상 전투×1 → 피버 후×2~×4 조합·연속 수비/원형 이동 → 피격 시×1 복귀 → Pause/Retry/Home이다. 실제 재미·난이도·물리/시각·모바일 터치·브라우저 실행/성능·튜토리얼/피버 전체 회귀·재실행 저장은 이번에 확인하지 않았다. 테스트 생략을 정상 동작 보장으로 해석하지 않는다.

실제 GooglePlay 랭킹 연동, 전면적인 일반 노드 겹침 개선, 새 Flash 그림/별도 사이버 재구축은 보류다. 이번 한정 빌드/커밋/푸시/배포 권한은 완료 후 자동 연장하지 않는다. 추가 개선은 구현하지 않고 사용자 검수를 기다린다.
