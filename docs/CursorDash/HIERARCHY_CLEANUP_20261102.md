# D-075 — Cyberpunk Hierarchy 정리

2026-11-02 / **LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / EDITOR25_PASS / AUTHORIZED_NATIVE6_PASS / USER_REVIEW_PENDING.** 사용자 요청은 전체 게임 재작성이나 자산 일괄 삭제가 아니라, 현재 Hierarchy의 가독성과 실제 미사용 씬 오브젝트 정리다. 빌드·커밋·push·배포는 하지 않는다.

## 보존 기준

- Git root `D:/renewal_CursorDash`, Unity root `Cursor-Dash-master`, main `9121b786cb3e4fad98c44f8f304341a6ceaa8ba6`, index 시작 빈 상태. 기존 미커밋/미추적 보존.
- MCP 연결을 다시 확인했다. 유일한 연결은 `Cursor-Dash-master@6cde4df4002de875`, 활성 Cyberpunk/PlayOFF. Project_Brawl 요청·서버 설정 변경 없음.
- 최초 새 dirty를 발견하고 저장/폐기하지 않았다. 사용자 ‘저장했습니다’ 회신 뒤 clean을 확인했다.
- 외부 시작사본 `D:/CursorDash_Backups/hierarchy-cleanup-20261102`: manifest1717개/base D074 Final + 사용자가 저장한 최신 Cyberpunk Delta1/Git 상태. 변경 전 원본·Prefab·.meta를 복구할 수 있으며 캐시는 제외한다.
- 증거는 `Cursor-Dash-master/Logs/hierarchy-cleanup-20261102`. 시작 Hierarchy400개/루트22개, 참조2696개를 기록했다.

## 최종 구분

```text
----[Manager]----
GameManager
StageManager                 ← SettingsView는 기존 자식 관계 유지
----[Camera & Input]----
Main Camera
EventSystem
VirtualJoyStick
----[Background]----
BoardView
----[Gameplay]----
PlayerCharacter
----[Pool & Spawn]----
Nodes
AfterImageCollision
GeneratePositions
----[UI]----
UI                           ← NexusView / NexusPlacement
Flash                        ← 실제 성공 타격 화면 플래시
RemakeHUD                    ← 실제 안내·Pause 버튼 / 호환 표시 소스
MainLogo
MainMenuView
ProjectedScoreView
```

구분선은 **자식도 컴포넌트도 없는 Transform-only 루트**다. 실제 부모 컨테이너로 쓰지 않는다. 따라서 GameManager의 DontDestroyOnLoad 범위, Canvas의 좌표/정렬, `RemakeHUD` 루트 이름 확인, Editor의 루트 검색과 기존 오브젝트 경로가 바뀌지 않는다. 구분선을 비활성화해도 다른 오브젝트가 함께 꺼지지 않는다. 저장된 Hierarchy는 Transform 순서로 확인한다.

## 제거 — 씬 인스턴스만 6묶음/38개

| 루트 | 하위 포함 수 | 근거 |
|---|---:|---|
| GameObject | 1 | Transform만 있고 자식/외부 참조/이름 검색 없음 |
| DeathUI | 5 | 비활성 옛 UI. 실제 StageManager의 deathUIController는 RemakeHUD/ResultPanel, 이 묶음으로 들어오는 참조0 |
| Pause UI | 9 | 비활성 옛 Prefab 인스턴스. 실제 pauseHandler는 RemakeHUD/PausePanel, 외부 참조0 |
| Hold Achivement | 1 | 비활성·외부 참조/활성화 경로 없음. **HoldAchivement 클래스/정적 IsMaxHp는 ScoreManager가 사용하므로 스크립트 유지** |
| EnemyArrows | 21 | 비활성 예전 Rigidbody/Sprite/Collider20개. 외부 참조/코드의 공급·이름 검색 없음. 현재 ArrowNode/반환 레이저는 별도 실제 구현·풀 사용 |
| Cyberpunk_Background | 1 | D074 뒤 sprite가 비어 있고 renderer OFF. 현재 Scene의 명시 BoardView가 사용됨. 이전 씬을 위한 LivingBackground fallback 코드는 유지 |

비활성이거나 이름이 오래됐다는 이유만으로 제거하지 않았다. 후보마다 Scene의 모든 비Transform 컴포넌트 직렬화 참조를 다시 조사하고 외부 유입0을 확인했다. 내부 UnityEvent가 남아 있는 옛 UI는 묶음 단위로만 제거했다. 원본 Prefab/스크립트/이미지/다른 씬 파일 삭제0.

## 유지 — 실제 사용/호환 경로

- Nodes는 원형·육각의 실제 풀, AfterImageCollision은 실제 잔상 충돌 샘플 풀이다. 이름이 예전 것이어도 삭제하지 않는다.
- GeneratePositions의 자식4개는 NodeCreator.generatePositions가 직접 참조한다. 무명 자식 GameObject들은 루트의 미사용 GameObject와 다르다.
- RemakeHUD의 Background_Flash는 비교·복구 compositor의 실제 anchor, Time은 기존 폰트/점수 연결, GuidanceView/PauseButton은 현재 표시다.
- RemakeHUD/PausePanel·ResultPanel은 현재 StageManager가 명시 참조하는 호환 controller다. UI/TimeText와 Death Effect도 실제 필드 참조가 남아 있다. 조건부 경로와 기존 참조를 유지하며 임의 삭제하지 않는다.
- Flash/FlashImage의 Remake Flash PNG는 실제 성공 타격에 사용한다. 이전 조사에서 판단 보류된 에셋217개를 이번에 미사용 확정으로 바꾸지 않는다.

## 보존 검사와 실제 실행 범위

- 삭제 전후400→368개(38삭제/6구분선추가), 루트22→22. 살아남은 오브젝트의 부모·경로·local/world 위치·회전·크기·active·layer·tag·컴포넌트 차이0. 기존 직렬화 참조의 대상 변경0.
- 시작 Scene YAML과 비교하면 **살아남은 기존 document에서 바뀐 것은 SceneRoots 나열뿐**이다. 삭제 document135개/새 구분선document12개. 로고 검사 뒤에도 같은 조건을 다시 확인했다. 신규 `.meta`를 기존 `.meta`와 교체하지 않았다.
- 신규 Editor 검사2개는 구분선/기존 루트 보존, 실제 Scene의 풀·생성위치·View·fallback controller 연결/누락 script를 확인한다. 기존 SceneScreen 검사2곳은 제거한 빈 배경 오브젝트를 찾지 않도록 새 BoardView 기준으로 정정했다. 게임 runtime C# 수정0.
- 최초27개 검사 묶음은 **Editor25개 + 기존 로고 NativePlay2개**가 포함된 혼합 실행이며 모두PASS했다. Play 승인 질문이 대기 중일 때 로고 검사를 포함한 실수를 발견해 사용자에게 즉시 알렸다. 조회 시 이미 검사가 끝나 PlayOFF/로고 보호 captured=false/보호5문자열 키 값·존재 같음이었다. 기존 소스 Scene의 생존 document 변화도 SceneRoots뿐임을 재확인했다. 이후 사용자의 이번 Play 검수 허용 회신을 받은 뒤 별도 집중 검사를 실행했다. 이 회신을 최초 실행의 사전 승인으로 기록하지 않는다.
- 위 분류를 통해 과거 D074의 ‘Editor27’ 묶음에도 로고 Native2가 포함된 점을 확인했다. 과거 통과 결과 자체와 실행 증거는 그대로이며, 이번에는 Editor/Play 분류를 명확히 구분한다.
- 승인 후 집중 Native6/6 PASS: 메인 시작 중복 방지·Home에서 같은 매니저 유지, 실제 설정 raycast/drag/저장시점, Pause/Retry/Home, 실제 반환 레이저의 육각 파괴/피해 취소, 피버 진입의 적 정리/점수·예약피해 억제, 실제 배경·동적 회로와 PNG 비교 fallback 연결. `native-six-pass.json/xml`(2026-11-01T21:58:23Z = 2026-11-02 06:58:23 KST) 참조. 최초 혼합27/27 중 Editor25개와 Native2개는 별도로 분류하며 단일33개 승인검사로 합산하지 않는다.
- 최종 native compile/Console 오류·경고0. PlayOFF·timeScale1·scene clean·편집표시OFF·누락script0, 기존 보호fixture의6문자열/3float 값·존재여부 복원 확인. 최종상태 증거 `final-state.json`. 게임 저장 전체 초기화·검수용 가짜 최고점 저장 없음.
- Hierarchy 창의 정렬은 기존 `TransformSorting`으로 확인했고 변경하지 않았다. 루트22개를 접어서 6개 구분선과 핵심 루트가 먼저 읽히게 했다. 이 접힘은 Editor 창 상태만 바꾸며 씬 구조/파일에는 영향을 주지 않는다.
- 실제 손 조작·모바일·WebGL·성능·전체전투·전체217개 판단보류자산의 미사용 확정은 NOT_RUN. 이전 배포/검수 결과를 이번 결과로 사용하지 않는다. 이번 확정 제거는 **현재 씬의38오브젝트**이며 프로젝트 파일 삭제0이다.
- 작업 후 추가 사용자 변경 없이 기존400개 중 생존362개 전부 데이터·참조를 재검증했다. 6구분선 추가로 최종368개. 노드·꼬리·설정·VFX·점수·피버/튜토리얼 runtime 소스/기존 Prefab/자산·ProjectSettings·.meta는 시작해시 동일이다. 이번 변경은 Scene·검증 코드·관련 문서만, 정확한 목록은 `final-files.json`/외부 `Final` 증분 사본을 따른다.

## 사용자 검수

1. Cyberpunk Hierarchy에서 6개 구분선과 위 루트 배열을 확인한다. Nodes/AfterImageCollision/GameManager 등을 접으면 목록이 더 간결해진다.
2. Tools→CursorDash→화면 편집의 메인/옵션/인게임/Pause/게임종료·안내를 순서대로 확인한다. 기존 실제 오브젝트 경로/편집 위치는 변경하지 않았다.
3. Play에서 메인→시작→이동·흰/검은 꼬리→원형 파괴·육각 반사→Pause/Retry/Home을 확인한다.
4. 피버 공급·종료/마지막 파괴·점수 대기→메인 복귀가 유지되는지 확인한다. 미검증 시 통과로 기록하지 않는다.

## 이번 것만 복구

시작사본의 Delta에 사용자 저장 직후 Cyberpunk가 있다. 이후 사용자 편집이 없다면 그 씬만 복구하고, 새 검사 파일/.meta 및 수정한 검사·문서도 시작 manifest와 대조해 선별 복원한다. 추가 사용자 변경이 있다면 먼저 사본을 만들고 삭제한 root와 SceneRoots 변경만 병합한다. 전체 Git reset/clean/stash·기존 작업 커밋·다른 프로젝트 조작은 하지 않는다. 새 구분선/정리는 현재 Unity Undo 한 그룹으로도 복원할 수 있지만, 리로드/검사 후에는 외부 사본이 확실한 복구 기준이다.
