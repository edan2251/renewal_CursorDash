# D-076 화면 편집·Hierarchy·잔존 꼬리 최신본 릴리스 (2026-11-02)

**RELEASE_COMPLETE / NATIVE_COMPILE_PASS / EDITOR25_PASS / WEBGL_BUILD_PASS / LOCAL_WEB_PASS / PUBLIC_WEB_PASS.** 사용자 현본 게시 승인에 따른 기존 GitHub Pages 릴리스다. 새 게임 기능·디자인·밸런스 변경은 하지 않았다.

- 공개 주소: https://edan2251.github.io/renewal_CursorDash/
- buildId: `cursordash-20261102-editor-069d35f8b4b9`
- 실제 빌드 소스 main: `9bd15ba72966dfe71310c207a31a769758ef6b80` — `[Release] Publish editable views, hierarchy cleanup and retained trail defense` (145파일).
- 웹 배포 gh-pages: `404818e3c0bb5e256593065b61d49fff574e39af` — `[Build] Deploy editor-069d35f8b4b9 from 9bd15ba`.
- [Pages 작업41357603103](https://github.com/edan2251/renewal_CursorDash/actions/runs/41357603103) success, Pages API built/동일 SHA, 2026-11-02 07:32:54KST 게시 완료.
- 이 최종 기록은 빌드 소스 뒤 별도 `[Docs]` 커밋으로 main에 푸시한다. 문서만 바뀌므로 게임은 재빌드하지 않는다. 실제 빌드 sourceCommit은 계속9bd15ba다.
- Git 루트 `D:/renewal_CursorDash`, Unity 루트 `Cursor-Dash-master`, 기존 main / gh-pages 구조 유지.

## 포함한 구현과 보존 범위

D-067~069의 튜토리얼 문구/고스트·점수 연결과 잔상 표시·정지 꼬리 방어, D-070~074의 Scene/Prefab View·상태 버튼 기반 실제 화면 편집, D-075의 Hierarchy 분류·선별 미사용 씬 인스턴스 정리를 배포 대상으로 확정한다. [편집 방법](EDITOR_AUTHORING_GUIDE.md), [구현·검증 이력](EDITOR_REFACTOR_STATUS.md), [Hierarchy 목록](HIERARCHY_CLEANUP_20261102.md), [정지 꼬리 방어](RETAINED_TAIL_DEFENSE_20261029.md)를 따른다.

육각 타이밍/공격·반사, 점수·배율·피버·저장 규칙, 승인된 파편·커서·배경과 UI 연출은 추가 변경하지 않는다. 일반 노드 겹침, 새 Flash Background 그림, 별도 사이버 공간 재구축은 계속 보류한다. Remake Flash는 실제 성공 타격 사용 자산이므로 보존한다.

## 작업 전 보존과 선별

시작 main/HEAD와 origin/main은 `9121b786cb3e4fad98c44f8f304341a6ceaa8ba6`, origin/gh-pages는 `6909ed2c187e4e221e798b45ba4b513b6aed99c0`, index는 비어 있었다. 외부 `D:/CursorDash_Backups/release-editor-20261102`에 시작 manifest1720파일과 Git 상태를 기록했다. 기존 D-075 Final 사본과 delta2개를 조합해 복구하며 Unity Assets/Packages/ProjectSettings·관련 미추적/.meta·문서/tools를 포함한다.

선별 대상은 CursorDash 소스·새 View/Prefab/설정 자산·테스트·관련 인계 문서/자산감사 도구와 팔레트 기록이다. Backups·LegacyAssetArchive·_Recovery·참고 영상·로그·Captures·배포 checkout·별도 환경 진단 도구는 포함하지 않고 그대로 남긴다. 비밀정보 패턴 검사 발견0. 이번 릴리스 자체의 게임 파일 변경은 빌드 식별자와 기존 build helper의 릴리스 접두사다. 빌드가 자동으로 비운 View SDF glyph cache2개는 m_ClearDynamicDataOnBuild=1임을 확인하고 빌드 직전 사본으로 복원했다. 원본TTF·Material·폰트설정은 불변이며, 복원 뒤 Assets/Packages/ProjectSettings1606파일이 빌드입력 해시와 일치한다. SaveAssets·강제 재임포트·사용자 dirty 자산 저장/폐기는 하지 않았다. 보호6문자열/3float의 존재·해시도 전후 동일하다.

## 검사 결과

| 구분 | 결과와 범위 |
|---|---|
| 네이티브 컴파일 | PASS — 대상 Unity6000.3.23f1, 빌드 전 컴파일 오류0 |
| 이번 Editor 검사 | PASS — 비Play 메서드만 선별25/25. Hierarchy2, SceneScreen4, Authoring7, WorldAuthoring5, Usability4, 로고의 비Play3 |
| 이번 Native Play | NOT_RUN — 이번 승인으로 이전 Play 권한을 재사용하지 않음. D-075의 승인후 Native6은 과거 별도 증거 |
| WebGL 빌드 | PASS — Unity6000.3.23f1/WebGL/NoPlay, 오류0·경고8(기존 미사용 필드6·NexusGameboard gradient 경고 DX11/GLES3 각1),233.063초,86,353,798bytes,2026-11-02 07:26:04KST |
| 로컬 HTTP·실제 브라우저 | PASS — 23파일 HTTP200/SHA256·WASM MIME일치. 실제1280×720 메인/설정7슬라이더·100→70→100/닫기/시작·실튜토리얼객체/실드래그커서·잔상/Pause·계속·Home, 브라우저warn/error0 |
| 기존 Pages·공개 브라우저 | PASS — 작업success/Pages built·배포SHA동일,25파일HTTP200/Git blob SHA256·WASM MIME. 쿼리없는공개주소의 실제 새loader/framework ID·메인/설정7개/닫기/시작·실튜토리얼객체/드래그커서·잔상/Pause/Home, 브라우저warn/error0 |
| 실제 모바일/터치·성능·전체 전투·튜토리얼/피버 완주·앱 재실행 저장 | NOT_RUN |

## 증거와 다음 작업

로컬 기록: `Cursor-Dash-master/Logs/release-editor-20261102/`. 테스트 JSON/XML, 시작/빌드 입력 SHA256 manifest, commit-selection과 배포 파일·HTTP·브라우저 증거를 이 위치에 남긴다. 인증정보·실제 사용자 저장 데이터 값은 공개하지 않는다. 소스 커밋과 사후 문서 커밋을 구분하며 문서만 바뀌면 게임을 재빌드하지 않는다.

**다음 한 작업:** 사용자가 기존 공개주소에서 PC/실모바일로 정지 꼬리의 육각 방어·튜토리얼 완주·피버·최종파괴/점수저장과 UI를 검수한다. Editor 편집은 Cyberpunk에서 Tools→CursorDash→화면 편집의 상태버튼을 사용한다. 새 기능·추가 빌드/Git/배포는 새 지시를 기다린다.

## 공개 파일·복구·종료 상태

전용 배포 checkout `Deployments/cp14-pages-20261024-131718/checkout`의 기존25파일을 `Logs/release-editor-20261102/PagesBefore`로 보존했다. 이전 Build4파일만 새 식별자의 파일로 교체하고 `.nojekyll`·`version.json`·기존 TemplateData 구조를 유지했다. 배포25파일에 로그/백업/영상/사적진단/인증정보 없음. Git 줄바꿈 정규화가 적용된 텍스트는 실제 Git blob을 공개 해시 기준으로 썼다. 소스 에셋의 Git 비교도 줄바꿈46개 외 불일치0이다.

작업 시작1720파일과 종료 비교: 삭제0·예상외변경0. 이번에 바뀐 기존파일은 build helper/BuildIdentity, Editor검사로 갱신한 파생fontcache2개와 인계문서5개다. 신규 릴리스문서1개 외 새게임기능 없음. staged code/docs diffcheck PASS; Unity직렬화YAML의 빈값 뒤 공백은 기존생성형식을 유지했다(전체 diffcheck 경고는 코드오류로 처리하지 않음).

최종 Native 확인: Cyberpunk clean/PlayOFF/compileOFF/timeScale1, 보호6문자열3float 동일, 기존 미저장 imported 자산은 저장·폐기하지 않음. 이번 Native Play는0회다. 다른 Project_Brawl/Unity프로젝트·MCP서버/설정·사용자저장기록을 변경하거나 종료하지 않았다. 임시 localhost8888 서버와 로컬검수 탭은 정리했고 공개탭은 Main으로 남겼다. 로컬서버 첫실행의 네트워크제약/연결timeout은 정상승인경로로 재실행하여 해결했으며 게임수정/보안설정변경은 없었다.

작업ID release-editor-20261102 / 프로젝트 CursorDash / 명시프로젝트 `D:/renewal_CursorDash/Cursor-Dash-master` / 현재Windows PC / 공통프로필·Slack 목적지 미설정. Slack NOT_SENT·독립Claude QA NOT_RUN. 실제 증거는 PNG 정지화면이며 연속영상은 촬영하지 않았다. 공개접속·기본조작 성공을 전체게임/모바일/성능 통과로 확대하지 않는다.
