# D-076 화면 편집·Hierarchy·잔존 꼬리 최신본 릴리스 (2026-11-02)

**RELEASE_IN_PROGRESS.** 사용자 ‘현재 상태로 빌드 커밋 푸시 배포’ 승인에 따른 기존 GitHub Pages 릴리스다. 새 게임 기능·디자인·밸런스 변경은 하지 않는다. 결과가 확인된 단계만 PASS로 갱신한다.

- 공개 주소: https://edan2251.github.io/renewal_CursorDash/
- buildId: `cursordash-20261102-editor-069d35f8b4b9`
- 빌드 소스·배포·사후 문서 커밋: 완료 후 아래에 구분 기록.
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
| 기존 Pages·공개 브라우저 | NOT_RUN — 배포 후 확인 |
| 실제 모바일/터치·성능·전체 전투·튜토리얼/피버 완주·앱 재실행 저장 | NOT_RUN |

## 증거와 다음 작업

로컬 기록: `Cursor-Dash-master/Logs/release-editor-20261102/`. 테스트 JSON/XML, 시작/빌드 입력 SHA256 manifest, commit-selection과 배포 파일·HTTP·브라우저 증거를 이 위치에 남긴다. 인증정보·실제 사용자 저장 데이터 값은 공개하지 않는다. 소스 커밋과 사후 문서 커밋을 구분하며 문서만 바뀌면 게임을 재빌드하지 않는다.

현재 다음 작업은 성공 빌드·로컬 WebGL 검수본의 소스 선별커밋/push → 기존 gh-pages 게시/공개 확인이다. 실패한 결과는 게시하거나 완료로 기록하지 않는다. 이번 릴리스 완료 뒤 새 기능·추가 빌드/Git/배포는 새 지시를 기다린다.
