# 편집 가능한 프로젝트 전환 — 단일 진행 기록

## 최신 — D-075 / Hierarchy만 가독성·미사용 정리 (2026-11-02)

6개 빈 루트 구분선/순서 정리, 현재 미사용6묶음38개 씬 인스턴스 제거. 기존 부모·좌표·생존문서/참조 유지, 실제 풀·호환 UI/표시와 원본자산은 보존한다. runtime C# 변경0. Editor25/승인후Native6 PASS, 최초혼합검사/권한실수도별도기록, 최종PlayOFF/clean·기록보존·빌드Git없음. [현재 작업의 전체 변경·증거·복구](HIERARCHY_CLEANUP_20261102.md). D074의 편집 화면·실제 View/프로필 기준은 그대로이며 추가 UI/노드 이전은 하지 않았다.

## 최신 — D-074 / 상태 버튼으로 실제 Scene 편집 (2026-11-02)

**LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / EDITOR_GPU_27_PASS / NATIVE_PLAY_9_DISTINCT_PASS / USER_REVIEW_PENDING.** 사용자 ‘Settings만 보이고 나머지 화면 사용법이 복잡함·시작 직후 할당과 저장 기본값 불일치’ 피드백을 우선한다. D-073의 개별 Prefab 표시 기능은 유지하되, 기본 작업은 **Tools → CursorDash → 화면 편집 → 메인 / 메인 옵션 / 인게임 / 일시정지 / 게임 종료·점수 대기**다. 튜토리얼/첫 피버 안내도 같은 줄에 추가했다. 별도 결과창이나 게임 상태를 만들지 않는다.

### 변경과 실제 편집 기준

- 버튼이 실제 Cyberpunk Scene의 View/Canvas를 함께 표시한다. 실제 TMP/Image/Placement를 선택해 Inspector에서 편집한다. 기존 private RenderTexture는 보조 비교 foldout으로 내렸다. Inspector의 참조 연결만 고르는 대신, 시작/BEST/안내 문구와 설정/Pause Placement를 직접 선택하는 버튼을 제공한다.
- Canvas의 표시 모드/연출 자식/동적 숫자/샘플 체력만 잠시 변경한다. 저장·Play·창닫기·리로드 전에 복원하고, 실제 배치·폰트·Image·문구 편집을 전체 스냅샷으로 되돌리지 않는다. Selectable 색 전환/Slider가 구동하는 handle/fill Rect도 표시 상태와 분리한다. 지연된 저장 재개 callback은 세션 번호로 취소한다.
- Scene의 `BoardView/Color field`는 기존 LivingBackground와 동일한 quad/셰이더/VisualPrototype의 시작값을 저장한다. 런타임은 이 표시의 재질만 복사해 기존 ApplyFrame/PlaceField/시계/타격 반응을 그대로 적용한다. 새 효과 관리자·카메라 파이프라인을 만들지 않았다. 이전 씬 fallback은 보존한다.
- Nexus HpBar에는 실제 시작 셰이더 `NexusBody.mat`를 연결했다. 기존 sprite/색/Rect/판정값과 합성은 유지한다. Energy Tint는 기존 NexusView, 동적 HP/호흡/흡입은 기존 소유자가 담당한다.
- SceneView에서는 게임판이 화면 일부를 차지하므로 fullscreen UV로 회로 mask를 샘플링하면 경로가 일부만 보였다. 기존 CircuitComposite에 Editor 전용 localUV 옵션을 추가해 같은 graph/geometry/mask를 게임판 Rect에 표시한다. 런타임 기본은0으로 기존 screenUV 경로다.
- Main은 실제 BEST를 읽기만 한다. 샘플·화면비 foldout에서 저장 BEST 대신 가상 숫자/체력·충전량/노드 대표형태를 선택할 수 있다. 사용자 기록·게임 시계·적 공급·실제 판정은 실행하지 않는다. 0% 게이지는 실제 규칙대로 숨겨진다.

### 옛 이미지 조사 결과

| 자산/슬롯 | 확인과 처리 |
|---|---|
| Remake/Cyberpunk_Background.png → `Cyberpunk_Background` SpriteRenderer | 실제 Play의 LivingBackground가 끄는 옛 배경. 현재 Scene의 sprite 할당을 제거/renderer OFF하고 같은 실행 배경을 BoardView에 저장 |
| Original/Cyberpunk_Background_Flash.png → `RemakeHUD/Background_Flash` | 실제 동적 회로가 대체하는 표시. Image의 sprite 할당 제거/disabled, 기존 Rect/alpha는 비교용 anchor로 유지. BoardView의 명확한 **비교·복구 전용** 참조로 옮겨 기존 PNG 비교/미지원 fallback을 보존 |
| **Remake/Cyberpunk_Background_Flash.png → `Flash/FlashImage`** | 사용자가 지목한 다른 PNG다. 현재 **성공 타격 플래시가 실제 사용**하므로 삭제/교체하지 않음. 이를 제거하면 승인된 타격 외형도 바뀌며 단순 미사용 정리가 아니다 |
| HpBar/Frame/Dotted sprites, 원형·육각의 source sprites | Nexus compositor/도형 크기/정렬/기하·fallback 참조에 사용. 미사용으로 단정해 제거하지 않음 |
| Maps/Cyberpunk.asset, 01.StageSelectScene, _Recovery 두 씬 | 과거 PNG 참조가 남아 있으며 보존 씬/비교 자료. 이번에 다른 씬을 손대거나 파일 삭제하지 않음 |

최초 광범위한 PNG/Nexus 동시 이전 호출은 자동 승인 검토가 외형 손상 위험으로 거부했고 실행되지 않았다. 실제 Native Play에서 옛 두 표시가 disabled·동적 회로 Active임을 입증한 후 **현재 Scene의 배경 표시 두 슬롯만** 한정해 이전했다. Nexus sprite 제거 계획은 폐기했고 원래 슬롯을 유지했다. PNG 정리를 전량 완료했다고 보고하지 않는다.

### 보존·검증

사용자 ‘저장했습니다’ 이후 clean을 확인하고 `D:/CursorDash_Backups/scene-screen-authoring-20261102`에 시작 manifest1701개/기준HEAD/Git 상태/사용자가 저장한 최신 Cyberpunk Delta를 보존했다. base는 D-073 Final이다. 사용자 작업과 이번 변경은 이 manifest 기준으로 대조하며 HEAD 전체 차이를 이번 변경으로 취급하지 않는다.

증거 경로: `Cursor-Dash-master/Logs/scene-screen-authoring-20261102`.

- PASS baseline3: Scene 상태/저장2 + 실제 Play의 옛 표시 비활성·동적 회로 사용1.
- PASS 배경 연결 후5: 실제 Main/Settings·배경 frame inputs·Nexus compositor/boundary 3 + 상태/저장2. 별도 run이다.
- PASS 확장 Editor25: 7상태×세로/가로 실제 source 표시, 여러 Editor update 유지, 실제 Scene copy의 위치·글꼴·Image 색 Undo/Redo·Save/재열기, 기존 Prefab 편집/로고/설정·안내·노드 데이터 검증. 숫자/사용자 설정/게임 초기화 미실행과 임시 Canvas/카메라/재질 저장 제외를 검사했다.
- 실제 SceneView Main/Settings/InGame/Pause PNG를 열어 확인했다. 동적 후처리·카메라 흔들림·회로 ambient/피버 퇴장·전투 물리는 정적 화면 표시와 구분한다. 캡처는 연속 영상이 아니다.
- 초기 오류: 새 partial import 순서에 의한 컴파일2 → asset import 후 해결. 꺼진 Canvas의 Graphic.canvas null → 명시 부모 Canvas로 수정. 숨긴 본체의 CanvasRenderer material slot0 오류 → 전투 표시 때만 슬롯 설정. 회로가 일부만 보임 → localUV 수정. 내부 TMP Color32/Normal style hash 및 Slider driven Rect의 지연 갱신은 실제 float 색/배치를 확인하고 원래 외형의 canonical 값으로 정리, 이후 장시간 Editor update 검사 통과. 실패를 화면 완료 근거로 사용하지 않는다.
- 모바일·터치·WebGL·성능·앱 재실행 저장·전체 전투/장시간 플레이는 이번 NOT_RUN. 빌드/커밋/push/배포 없음. 최종 실행 검사와 파일 대조 결과는 아래 후속 기록에 갱신한다.

### 최종 실행·인계

- 최종 소스 기준 **Editor/GPU 27개 PASS**, Native Play **9종 PASS**는 분리 실행 결과다. `editor27-pass-legacy-transform-fail.json/xml`의 Editor/GPU27은 전부PASS, 같은 run의 낡은 Pause검사는FAIL이었다. Pause검사는 게임 변경 없이 명시 NexusView 참조/공유 시작 완료/바깥 링 이후 실제 안쪽 흡수 시점을 기다리도록 수정했고 `native-pause-final-pass.json/xml`에서1/1PASS. 최초 NULL 경로 실패 `native-eight-pass-one-fail`도 보존한다. Native9는 그 run의8PASS와 수정 후Pause1을 합친 **고유 항목**이며 단일9PASS run이라고 하지 않는다. 중간확장Editor25/25와 연결직후5/5는 이전 별도 증거다.
- Native9 확인: 실제 Main/배경 동일 frame입력·Settings 스타일, EventSystem 실제 raycast/slider저장시점, Pause/Continue/Retry/Home, 중복 시작과 같은 매니저 유지, Nexus compositor·실제 경계 보존, 실제 첫 피버 안내/회수·취소·Pause, 동적 회로 Active/옛 화면표시 OFF 및 **PNG 비교 재활성화**. 내부 테스트 입력이며 손·터치 조작 확인과 구분한다.
- 실제 Scene 캡처7종: `scene-main/settings/ingame/pause/gameover/tutorial/fever-notice.png`. Play 캡처 `runtime-main/settings.png`, `board-runtime.png`는 동일 렌더링 데이터를 사용한 실제 Game View다. SceneView의 게임 카메라 바깥 여유 영역은 실제 게임 화면이 아니며 배경 overscan이 더 보일 수 있다. 비교 영상은 만들지 않았다.
- 최종 종료 조회(`final-native-state.json`): CursorDash/Cyberpunk, PlayOFF·compileOFF·timeScale1·scene clean·표시 sessionOFF·PrefabStage없음·Editor임시root0·missingScript0. 기존 fixture6문자열/3float 값과 존재여부 동일·보호세션해제. 다른 Project_Brawl/서버/패키지/설정·사용자기록을 변경하지 않았다.
- `git diff --check` 전체 HEAD 비교에는 **이번에 바꾸지 않은 VisualPrototype.asset 14행 공백**이 남아 있다. 이를 이번 변경으로 오인해 사용자 설정을 재저장/정리하지 않는다. 시작 manifest 대비 소스·보존 파일 해시와 이번 추가 변경을 따로 대조한다.
- 이번 변경은 게임/Editor 소스·표시 자산/씬·검사·관련 문서만이며 정확한 목록은 `final-files.json`. 원형·육각 타이밍/공격·꼬리·고스트·입력·점수·피버 규칙·기존 Prefab/폰트/Packages/ProjectSettings는 시작해시 동일. Assets 삭제0/기존파일 누락0, 커밋·index·브랜치 변경0/빌드·게시0. 최종 변경본은 외부 시작사본과 같은 위치의 `Final/Files`에 별도 증분 보존한다.

### 이번 변경만 되돌리기

Git HEAD 전체 reset/checkout을 사용하지 않는다. `final-files.json`의 changed/new 목록을 시작manifest와 대조한다. 변경 파일은 시작 Delta(사용자 저장 Cyberpunk) → D-073 Final/Files → D-073 Delta → 이전 DG/BC base 순서에서 **시작 SHA가 맞는 사본**을 선택해 필요한 파일만 복원한다. 새 파일16개(.meta포함)는 참조를 먼저 되돌린 뒤 명시 목록만 정리한다. 작업 후 추가 사용자 편집이 있으면 먼저 별도 보존/차이 병합한다. 폴더 전체 삭제나 예전 버전 일괄 되돌림은 하지 않는다.

다음 한 작업은 사용자 실제 상태 버튼 → Scene 직접 선택/편집/Undo → 저장 → Play 반영 확인이다. 승인된 게임 요소 재디자인이나 추가 UI 이전으로 진행하지 않는다.



## 최신 — D-073 / 실제 UI 편집 사용성 복구 (2026-11-02)

**LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / EDIT_89_PASS / FINAL_EDITOR_11_PASS + SAVE_PATH_4_PASS / NATIVE_PLAY_4_PASS / USER_REVIEW_PENDING.** 사용자가 게임·전투/전환·설정·저장·WebGL은 정상이라고 확인했으나 D~G의 실제 편집 사용성은 FAIL이라고 보고했다. 이번 범위는 이 사용성 보완이며 기존 D~G 검사를 실제 디자인 편집 전체 통과로 확대하지 않는다. 빌드·Git 커밋/push/배포 없음.

### 실제 원인과 수정

- **SettingsView/MainMenuView:** 생성 당시 Canvas를 끈 채 저장한 원본의 root RectTransform에 Scale/size0가 직렬화되어 있었다. Settings의 실제 등장 연출은 root Scale0이 아니라 `sheetAnimation.anchoredPosition`, contentOpacity, contentAnimation/pauseAnimation의 fit·offset, pauseOpacity다. Runtime은 Canvas를 켜고 Layout/Present를 적용하므로 정상으로 보였고, 편집 원본만 보이지 않았다. 두 원본의 reference frame을 Scale1/Settings540×960·Menu1080×1920/CanvasON으로 바로잡고, 씬에 남은 두 Canvas root의 오래된0값 default Override22개만 제거했다. 사용자가 저장한 로고·UI 배치·텍스트·색은 유지했다.
- **Prefab Mode:** Game display가 없는 Overlay Canvas는 켜기만 해도0크기로 평가될 수 있다. `CursorDashUIAuthoringPreview`는 실제 원본을 편집할 동안만 flat WorldSpace로 보여주고 프레임을 맞춘다. 저장/Prefab닫기/Play/리로드 전에 Overlay·Scaler·애니메이션 표시값을 복원한다. Placement/TMP의 글꼴·이미지·문구·스타일을 전체 스냅샷으로 덮어쓰지 않는다. 실제 File→Save 경로와 Undo/Redo/재열기를 검사했다.
- **Settings/Pause:** 같은 실제 Prefab의 Inspector 버튼으로 완성된 설정/Pause를 따로 표시. 기존 Placement와 Animation 자식 구조를 재사용하며 두 패널/저장 관리자/새 게임 상태를 만들지 않았다. 샘플 슬라이더75%(완화0%)는 편집 표시만, 게임 사용자 설정을 읽거나 저장하지 않는다. 실제 Shade Image의 기준 RGBA는 원래(.01,.02,.045,1) 그대로다. 한때 migration의 Present 호출이 .32를 저장한 부작용은 작업 전 해시와 RGBA 확인 후 해당 값만 복구했다. Editor 표시에서는 이제 Present로 디자인 색을 변경하지 않는다.
- **안내/점수:** 안내는 숨김 alpha/TMP.disabled와 단독 Canvas 부재가 겹쳤다. Prefab Stage의 환경 Canvas를 재사용하거나 필요한 경우 임시 Canvas만 제공하고, 안내용 흰 대비판은 DontSave로 표시/저장 전 제거한다. 실제 runtime 흰 배경/시계는 바꾸지 않는다. 공용 점수는 Renderer의 숨김 기본값만 ON으로 바로잡았고, Show 버튼이 Main/Combat 기준색을 표시한다. 실제 숫자는 하나/기존 점수·배율·카운트 연출 그대로다.
- **원본 선택:** 화면 창 버튼은 실제 Scene View 원본을 보이게 하고 필요 없는 다른 알려진 UI Canvas만 잠시 가린다. 종료 때 이 표시만 복원한다. 실제 Cyberpunk Settings 원본의 표시→종료에서 기준 Placement·Overlay 복원/Scene dirty 불변을 Native로 확인했다. 편집본을 과거 씬 사본으로 되돌리지 않는다.
- **배경:** 이전에도 LivingBackground shader/VisualPrototype 팔레트를 읽고 있었다. 차이는 preview의 하드코딩 Clock4·정지 상태와 런타임 시계/합성 경로였다. runtime/preview가 동일한 `ApplyFrame`/`PlaceField`를 사용하도록 묶고, 창에 Editor 전용 흐름 재생/시간 입력을 제공했다. HitEnergy/clock/접근성·게임의 시간 처리는 그대로다. 새 PNG나 비슷한 대체색을 만들지 않았다.

### 배경 비교의 정확한 범위

실제 Native Main 캡처는 보라·청록 색 영역과 회로/로고/BEST를 표시했고 Nexus는 숨겨졌다. `Space.Home → SetNexus(0,0)`가 실제 규칙이다. 사용자 메시지의 스크린샷 두 파일은 도구 입력에 없어 직접 보았다고 하지 않는다. 푸른 PNG와 Nexus가 보이는 화면은 Play 전 저장 Scene/다른 게임 상태일 가능성이 있으나 동일 상황으로 단정하지 않는다.

Play 전 원본 `Cyberpunk_Background` PNG는 runtime의 Living color field가 생성되면 꺼진다. 미리보기는 같은 shader·palette·frame inputs/실제 회로 graph·glyph mask를 그리며, Canvas는 private camera로 합성한다. 실제 Main과 동일한 상태/clock를 비교해야 한다. hit·회로 ambient·Nexus compositor/피버 RGB·시간별 전환/최종 카메라 후처리는 Play 전용이다. 정적/배경 미리보기를 최종 Game View 픽셀 동일성으로 부르지 않는다.

### 검증·실패 이력

증거: `Cursor-Dash-master/Logs/editor-usability-20261102`.

- PASS native compile/최종 Console0. 확장 Edit89/89(`edit-final89-pass.json/xml`), 마지막 창닫기 정리까지 포함한 Editor 변경 후11/11(`edit-final11-pass`, 2026-11-01T16:25:18Z), 실제 File/Save 검사를 강화한 마지막4/4(`edit-save-path-final`, 2026-11-01T16:16:38Z). 원본 표시/dirty/기록 불변, 실제 Prefab copy의 위치·폰트·이미지·색·문구 Undo/Redo·저장/재열기, Overlay 저장/임시Canvas 제거를 포함한다. 검수 사본만 GUID 고유 Assets/Tests/Editor/UsabilityFixture_*에서 만들고 제거했으며 사용자 원본을 테스트 데이터로 덮지 않았다.
- PASS 실제 Native Play4: Main/공통배경 parameter·스타일유지, 실제Settings EventSystem raycast/drag/저장시점, Pause/Continue/Retry/Home, SharedSpace 시작중복방지/매니저 유지. `mixed-first-7pass-1fail.json/xml`의 Native4는 모두PASS, Pure쪽1은 빈 TestRunner Scene을 준비하지 못한 검사 오류였다. 해당 준비를 Cyberpunk clean 가드/명시 load로 고쳐 마지막4/4에서 재검사PASS. 앞선2PASS2FAIL도 stage환경Canvas 존재·reflection Show 오버로드를 잘못 가정한 검사 오류, 핵심 단언 유지 후89PASS로 정정. 단일8PASS run으로 합치지 않는다.
- 실제 캡처를 열어 확인: Settings/Pause/Guidance **원본 Prefab Stage SceneView 카메라** 1010×756, Native Main/Settings/Pause Game View1080×1920, Main preview0/23초의 실제색영역 변화. 초기 capture가 비어 있던 이유는 비활성 SceneView의 camera가 Frame pivot을 아직 반영하지 않았기 때문이며, 해당 Unity SceneView 탭 focus/재그림 뒤 원본이 실제 보이는 캡처로 갱신했다. OS 입력·다른 앱 조작은 없었다. 연속 영상은 만들지 않았다.
- PASS 게임 저장: 기존 보호 Fixture가6문자열/3float/Map memory를 SessionState 보관·복원하고, Editor 검사도 전후 게임 설정/완료/BEST 값을 비교했다. 사용자 기록 초기화 없음. OS registry 전량 조회·별도 저장 체계/전체 초기화는 하지 않았다. 앱 전체 재실행/모바일·터치·이번 WebGL 실행·성능은 NOT_RUN(사용자의 정상 확인과 구분).
- 폰트2파일은 실제 Editor의 동적 glyphcache 갱신이다. Medium95→113/Bold95→98, 이전 문자는 모두 유지/Material 불변/서체·FaceInfo·SDF설정은 그대로. `font-cache-changes.json`. sourceTTF와 .meta는 불변, cache 강제재임포트·포괄 SaveAssets·dirty 폐기는 하지 않았다.
- 자동 승인 검토가 shade 알파1 설정을 외형 변경으로 처음 거부했다. 사본 해시와 이전 RGBA1/우리 부작용 .32를 읽기 전용으로 증명한 뒤, 현재값 guard를 둔 원래값 복구만 승인되어 완료했다. 차단을 우회하지 않았다.

### 보존·변경 범위·복구

착수 시 새 Cyberpunk dirty를 발견해 저장하지 않았고, 사용자 ‘저장했습니다’ 답변 후 clean을 재확인했다. 그 저장본을 새 기준으로 `D:/CursorDash_Backups/editor-usability-20261102`에 보존했다. manifest1697개/base DG Final/Delta2(사용자가 저장한 Scene·Bold 캐시), Git 상태를 포함한다. 작업 도중 Prefab 갱신의 Scene dirty는 별도 메모리 사본/차이(`scene-after-prefab-refresh.unity`, `scene-preview-refresh.diff`)를 확인한 뒤 이번 Canvas 정리에 필요한 값만 저장했다.

파일 보존 최종 대조: 기존 파일16수정/신규4(.meta포함)/총20개, 누락0. 별도보호42파일(원형·육각/꼬리·Tuning·점수·FeverState·로고/원본TTF·Packages·ProjectSettings)은 시작해시동일. 폰트2개는 위의 glyphcache 차이로 구분한다. 최종 Unity 값 조회는 Scene clean/PlayOFF/compileOFF/timeScale1/PrefabStage없음/표시세션없음/privatePreview0/검수사본0/missingScript0, 기존 TMP shader3+FontTexture3 dirty는 저장·폐기하지 않고 보존했다. 보호fixture의6문자열/3float 값·존재여부도 마지막 baseline과 동일/캡처 복원완료를 별도 조회했다.

실제 변경: 기존 Editor 창2개·신규 UI Authoring Preview/Inspector1개(.meta), MainMenu/Settings/ProjectedScore Prefab3개, Cyberpunk의0 frame Override/Unity UI 내부 직렬화 갱신, 공통 LivingBackground 표시 함수1개, 신규 집중 검증1개(.meta), 동적 폰트 cache2개, 관련 상태·편집 문서. 게임의 Tuning·노드/꼬리·공격/반사·점수/기록/피버 규칙·로고 source는 수정하지 않았다. 정확한 시작 대비 목록·해시는 `final-files.json`, Final 증분 사본을 따른다. 이번 것만 되돌릴 때 Before/Delta/base chain과 신규 목록을 함께 비교하며 기존 사용자 작업에 Git reset/clean을 쓰지 않는다.

최종 PlayOFF/timeScale1/Cyberpunkclean, Prefab Stage·표시 세션·임시 원본/검수 사본 정리, 커밋/push/빌드/배포 없음. 다음 한 작업은 사용자의 **SettingsView Prefab → 설정/Pause → 실제 원본 편집/저장 → Play 확인**이다. 다른 구조 개선으로 확대하지 않는다.

### 이번 작업 파일 — 착수 사본 대비

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.Board.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.cs`
- `Cursor-Dash-master/Assets/Prefabs/UI/MainMenuView.prefab`
- `Cursor-Dash-master/Assets/Prefabs/UI/ProjectedScoreView.prefab`
- `Cursor-Dash-master/Assets/Prefabs/UI/SettingsView.prefab`
- `Cursor-Dash-master/Assets/Resources/CursorDash/Fonts/esamanru Bold View SDF.asset`
- `Cursor-Dash-master/Assets/Resources/CursorDash/Fonts/esamanru Medium View SDF.asset`
- `Cursor-Dash-master/Assets/Scenes/Cyberpunk.unity`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashLivingBackground.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/EDITOR_AUTHORING_GUIDE.md`
- `docs/CursorDash/EDITOR_REFACTOR_STATUS.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashUIAuthoringPreview.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashUIAuthoringPreview.cs.meta`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashEditorUsabilityTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashEditorUsabilityTests.cs.meta`


### 캡처

- [설정 원본 Scene View](../../Cursor-Dash-master/Logs/editor-usability-20261102/settings-prefab-scene.png) / [실제 설정 Game View](../../Cursor-Dash-master/Logs/editor-usability-20261102/settings-runtime.png)
- [Pause 원본 Scene View](../../Cursor-Dash-master/Logs/editor-usability-20261102/pause-prefab-scene.png) / [실제 Pause Game View](../../Cursor-Dash-master/Logs/editor-usability-20261102/pause-runtime.png)
- [안내 원본 Scene View](../../Cursor-Dash-master/Logs/editor-usability-20261102/guidance-prefab-scene.png)
- [실제 Main](../../Cursor-Dash-master/Logs/editor-usability-20261102/main-runtime.png) / [Main preview0초](../../Cursor-Dash-master/Logs/editor-usability-20261102/main-preview-after.png) / [23초](../../Cursor-Dash-master/Logs/editor-usability-20261102/main-preview-time23.png)

아래 D-072는 이전 구조 구현/검수 범위 이력이다. 그때의 부분 시각 PASS는 이번 사용성 FAIL을 반박하는 근거로 사용하지 않는다.


## 최신 — D-072 / D~G 구현 완료·통합 검수 일부 미확인 (시작 2026-11-01 / 인계 2026-11-02)

**LOCAL_IMPLEMENTED / NATIVE_COMPILE_PASS / EDIT_85_PASS / INTEGRATION_18_DISTINCT_PASS / LOCAL_WEBGL_BUILD_PASS / PARTIAL_VALIDATED.** 기존 C는 사용자 직접 검수 **USER_VERIFIED PASS**다. D~G의 구현·가능한 검사를 완료했고, 최종 Game View 픽셀 검사의 환경 제약과 사용자 손·모바일·브라우저 실행·재실행 저장 검수는 남는다. 이를 전체 시각·기기 통과로 기록하지 않는다. 다음 한 작업은 아래 수동 순서의 사용자 검수다. 새 기능·자동 추가 정리는 시작하지 않는다.

출처: 사용자 첨부 b2ddb16e. D-071의 C 대기/사용자 전담 Play 제한을 이번 범위의 Unity MCP 테스트·Play·로컬 WebGL 승인으로 대체했다. Git 커밋/브랜치/push/배포, 다른 프로젝트·서버·패키지·파이프라인 변경은 하지 않았다. 시작/종료 main `9121b786cb3e4fad98c44f8f304341a6ceaa8ba6`, index 빈 상태. 시작 추적21수정과 기존 미추적/A~C/로고를 보존했다. 이번 차이는 HEAD가 아니라 DG 시작 사본과 비교한다.

### 단계별 결과

| 단계 | 구현·실행 결과 | 남은 범위 |
|---|---|---|
| D | PASS: 기존 화면 편집 창을 Main/MainSettings/InGame/InGamePause/Returning/Tutorial/FeverNotice로 확장. 실제 View·Prefab·원본 선택, 세로/가로·점수/HP/충전/노드 샘플, 회로·숫자 획 가림의 정적 GPU 합성 | 왜곡/흑백 전환·시간에 따른 애니메이션은 Play 전용. 두 번째 게임 상태 시스템을 만들지 않음 |
| E | PASS: Nexus 기준 루트/명시 참조, 실제 Mesh 외형 자산, 커서 명시 SpriteRenderer/편집 Scale, 기존 팔레트·회로 좌표 데이터, 안내 Prefab과 private font 연결. 기본값 동일 이전 및 관련 Native 경계/처치/피버 안내 확인 | 전체 편집 조합을 손으로 반복한 결과와 최종 렌더 순서 픽셀 검사는 미확인 |
| F | PASS: 동일 회로 버퍼 공유, pure sample/실행 Override 분리, profile 호환 필드 표시 정리, 정식 외형 편집이 기록 제외를 만들지 않는 실제 처치 검사. 기존 명시 Dev 제외 유지 | 게임 규칙 Inspector/메타 비교·JSON/프리셋·저장키 정책은 원래대로 유지 |
| G | 부분 완료: 자산698개 분류(연결481/불확실217/확정미사용0/격리0), 최종 Edit85PASS, 통합 Native 고유18항목 최종PASS, 로고 Native5PASS, 로컬 WebGL 오류0/경고8 | 실제 브라우저 WebGL/모바일·터치·성능/전체 연속 영상·앱 재실행 저장은 NOT_RUN. 최종 GameView 캡처 BLOCKED |

### 보존·복구 지점

- D~G 시작: `D:/CursorDash_Backups/editor-refactor-DG-20261101`의 manifest-start1664파일, Git 상태, 당시 Editor prefs12/standalone0와 Persistent3파일, UserInstructions. BC milestone과 정확히 같아 Delta0이며 전체 캐시를 중복 복사하지 않았다.
- base: `D:/CursorDash_Backups/editor-refactor-20261101-2050/Milestones/BC-implemented`; 그 이전 base는 `D:/CursorDash_Backups/main-logo-view-20261101-1710/Workspace`다. 신규/미추적/.meta를 포함한다.
- 중요 단위: DG 사본의 `Milestones/E`, `Milestones/Final`에 변경·신규 파일과 manifest 보존. 삭제/누락0. 세부 파일 목록은 `Cursor-Dash-master/Logs/editor-refactor-DG-20261101/final-files.json`.
- 이번 변경만 되돌릴 때: PlayOFF/사용자 후속 편집 유무를 먼저 비교하고, final-files 목록의 기존 파일만 DG 시작/base chain에서 복구한다. 신규 파일은 이 목록과 .meta를 한 단위로 분리하고 Scene 참조도 함께 복구한다. 다른 변경에 reset/clean/revert를 적용하지 않는다. 이번 후 새 사용자 수정은 먼저 별도 보존해야 한다.

### 주요 구조·정식 출처

1. `Tools → CursorDash → 화면 편집`: 실제 원본을 선택하고 Inspector에서 편집한다. private PreviewScene/Camera/RT는 표시만 하며 Stage/GameManager·Collider·EventSystem을 만들지 않는다. 샘플은 게임 prefs/HP/점수와 분리. 닫기/Play/재로드 때 사적 자원을 정리한다.
2. `UI/NexusPlacement`의 Rect가 전체 위치·uniform Scale 기준이다. 기존 HpBar/HpFrame/HpCentral의 fileID·Image·크기·색·world 위치를 유지하고 함께 묶었다. 이동 전후 좌표차≤1e-5, 본체211×211/중앙0. `UI/CursorDashNexusView`에 원본 Image 참조/energyTint·Gauge Radius1.86/Thickness.14/Gap4를 둔다. 실제 HP/피격경계/자성/피버 시계와 원래 카메라 합성은 기존 소유자가 담당한다.
3. `NodeAppearance.asset`이 실제 Mesh 기본값이다: outline.035/core0/glow0, 원형fill(203,73,255)/rim(232,124,255), 육각 두 번째 링.82/stroke.071768/fillRatio1.014927. 기존 크기·반경·충전/공격·반사·파편은 그대로다. NodeArtwork의 원래 Draw API와 Graph3인자 생성자도 유지했다.
4. 커서 `PlayerCharacter/CursorVisual`의 명시 Renderer와 기본 Vector3 Scale을 사용한다. 기본 비균등 표시도 보존하며 명시 Dev visualScale은 기존 uniform Override다. 실제 이동/회전/입력·꼬리 표시/방어 로직은 변경하지 않았다.
5. `VisualPrototype.asset`의 원래 navy/teal/violet을 동일 색상의 shader uniform으로 이전. `CircuitLayout.asset`94좌표/12root/82edge는 기존 연결·좌표·길이와 일치한다(좌표≤2e-7/길이≤1e-6). topology/전파·퇴장·숫자 마스크/피버·게임오버는 그대로. `CircuitStrokeBuffer`로 동일 vertex 생성기를 runtime/Editor가 공유한다.
6. `RemakeHUD/GuidanceView` 실제 Prefab: Checklist/첫 피버 문구의 배치·TMP·템플릿만 담당. 기존 Practice/Fever가 시계·횟수·완료/입력 잠금을 소유한다. 저장된 TMP는 private font 연결 전 비활성으로 두고 렌더링 준비 뒤 활성화한다. 안내 배경은 clear, 실제 흰 세계는 기존 전환에서 제공한다.
7. 로고/메뉴/점수/설정의 A~C View는 그대로 사용한다. View가 정식 기준이며 VisualPrototype의 옮긴 필드는 미연결 씬 호환 fallback만 담당, 기본 Inspector에서는 숨기고 선택적으로 노출한다. 사용자 설정·명시 Dev/JSON·게임 규칙·저장 정책은 재작성하지 않았다.

### 실제 검사와 증거

증거 폴더: `Cursor-Dash-master/Logs/editor-refactor-DG-20261101`.

- PASS native 컴파일: 최종 Console error/warning0. WebGL 별도 경고는 아래8건이다.
- PASS 최종 **EditMode85/85**, 2026-11-01T14:53:06Z: `G-final-edit85-after-build.json/xml`. 원본 Scene/prefs/글꼴 표 불변·미리보기7상태/두 화면비/샘플·missingScript/graph·fever/점수 수학 포함. 이85는 실제 손/물리검사 수가 아니다.
- PASS Native 통합 고유18: `G-integration18-final-index.json`에서 각 항목의 최종 실행 JSON을 연결한다. 여러 실행의 최종 결과이며 단일18PASS run으로 합치지 않는다. Main→시작중복방지/Home, 설정 real UI pointer/drag/Pause/Retry, 실제 커서 입력·꼬리 방어/BakeMesh/옛경로 제거, 원형 morph/dash/파괴/풀 초기화, 육각 rings/실반환, 실습2+2/본게임, 피버 진입/충돌/회수/광학, gauge되감기/두 피격 경로, 파괴→점수대기와 격리 QA map 정산·Play재진입 저장 포함.
- PASS 별도 실행: D 로고5/5(실제Play2포함), E 경계·정식처치기록·첫안내3 + Retry재검사1, F 외형outline.063/다른Nexus색·비균등cursor표시 상태에서 실제처치 점수1/RecordEligible=true/RuleSessionChanged0, 명시 EnableDev 뒤 기록제외는 유지.
- PASS 제한적 시각: private GPU 정적 InGame/Tutorial/FeverNotice/Pause 세로·가로를 실제 열어 확인했다. `E-ingame.png` vs `G-final-ingame.png`1080×1920 픽셀차0. 실제 Native Pause와 첫 피버 안내 PNG도 열어 확인했다(연속 녹화 아님). 파일은 아래 링크/원래 QA 경로에 남긴다.
- PASS 저장 보존(모든 Native Play 뒤/빌드 전): `storage-preservation.json`, Editor12/standalone0/Persistent3파일 값·타입/해시 동일. 사용자 BEST/완료키를 초기화하지 않았고 가짜 검수 기록은 격리 QA map/key에서만 사용·복원했다.
- PASS 로컬 WebGL: `G-webgl-build.json`, **Succeeded / error0 / warnings8 / 670.613초 / 86,415,324바이트**, 완료14:41:43Z. 출력 `D:/renewal_CursorDash/Builds/LocalDevPreview/editor-refactor-DG-20261101-local`. `G-webgl-files.json`23파일/SHA256·index의 loader/data/framework/wasm 참조 존재 확인. 배포/브라우저 게임 실행은 하지 않았다. buildId는 기존 `cursordash-20261028-settings-bb06638d1e46`을 유지하므로 이번 로컬판 식별은 출력 폴더·파일 해시를 사용한다.
- PASS 파일 보존: DG 시작1664→최종1697, 기존변경34/신규33/누락0(문서·.meta포함). 별도보호59파일의 로고/게임규칙·꼬리/정식폰트/Packages/ProjectSettings는 시작해시동일. 빌드입력 대비 최종 차이는 Medium View SDF의 작업중 glyphcache를 시작판으로 복구한 것과 Editor검사의 native동일성단언 정정2개뿐, runtime 소스/Scene/Shader 추가변경 없음.
- 정적 형식 주의: `git diff --check`는 Unity 직렬화 YAML 빈 필드의 trailing space10곳을 경고했다(기존 사용자 변경 포함 HEAD비교). 컴파일/게임 오류는 아니며 전체Scene/기존사용자라인을 포맷정리하지 않았다. 이를 diffcheck PASS로 기록하지 않는다.
- WebGL 경고8은 기존 ArrowNode 미사용 직렬화필드6 + NexusGameboard gradient-loop DX11/GLES3 각1, `G-webgl-messages.json`. 무관한 필드삭제/셰이더 재작성은 하지 않았다.

### 해결한 검사 실패와 잔여 제약

- 기존 Retry의 새Stage 기대는 단일 씬 정책과 충돌 → 같은Stage/Playing/정상HP/빈Gauge를 단언. 게임 Retry 코드는 변경하지 않음.
- 꼬리 검사는 Clear 뒤 검수용 순간이동으로 새 샘플을 스스로 만들었음 → 순간이동 뒤 Clear. 실제 방어 단언은 유지. 중간 Pause 객체상실 실패는 재현되지 않아 이력 보존 후 실제 방어/표시/초기화 재실행PASS.
- 원형 root고정 추론은 폐기. 실제 NormalNode는 기존대로 Warning에서1→.92축소한다. Warning 첫 프레임morph0과 .3변형+.05반동 대기를 검사에 반영해 수식·상태·Fired/풀복원 검증, 소스 NormalNode 변경 없음.
- 점수 전환 검사는1.35초보다 .0001초 이른 시점에 완전0을 요구했음 → 실제 끝1.35초로 정정. 색/위치/크기/0·handoff 무점프 단언 유지. 실제 점수 코드 변경 없음.
- 글꼴 참조 실패는 native atlas/Material ID와 GUID/local fileID는 모두 같은데 C# wrapper의 ReferenceEquals만 다른 경우였다. 실제 Unity 동일 리소스 + instance/GUID/localID 검증으로 정정했고85최종PASS. 런타임 font helper는 DG 시작과 동일하다. 별도 사적 복사 probe도 source와 copy atlas가 다른 실제 Texture임을 확인했다.
- **BLOCKED:** `Cursor_FinalGameViewShowsCursorAboveNexusAndBelowSettings`는 unfocused Editor의 CaptureScreenshotAsTexture가 Texture 생성을 거부했다. 최종 후처리 뒤 커서 순서를 픽셀로 확인했다고 쓰지 않는다. 원인 숨기기 위한 renderer/게임 변경 없음.
- TMP 기본 `ClearDynamicDataOnBuild`가 빌드 중 두 View SDF의 동적 글리프 캐시를 비움. 비운 파일은 Logs/BuildClearedFontCaches에 보존하고, DG 시작 BC 사본과 해시 확인 후 해당 두 캐시 파일만 원래 내용으로 복구했다. .meta/폰트디자인/기존6imported dirty는 저장·폐기하지 않았다. `font-cache-restoration.json`. 이 과정은 runtime/game source 변경이 아니어서 재빌드는 하지 않았다.
- 최종 Scene clean/PlayOFF/compileOFF/timeScale1/missingScript0/6View configured. 기존 dirty6(TMP shader3/TTF FontTexture3)에 추가로 자체 Bold View SDF Atlas의 메모리 cache dirty1을 관찰했다. 디스크 두 View font는 작업 전 해시와 같다. 자동 승인 검토가 강제재임포트를 미저장 상태 보존 위험으로 거부해 재시도/SaveAssets/폐기하지 않았다. 사용자 기존 dirty6의 원본 디스크도 작업 전과 동일.
- **BLOCKED 추가 저장 재조회:** 빌드 후 registry/persistent 최종 재비교 요청은 자동 승인 검토가 원시 registry/base64 접근·진단 산출물 위험을 이유로 거부했다. 재시도하지 않았다. 위 빌드 전 실제 보존PASS와 구분하며, 그 이후 Play/사용자저장 조작은 없었다. 최종 재조회PASS로 추정하지 않는다.
- NOT_RUN: 실제 브라우저 WebGL·모바일/터치·성능, 모든 전투를 잇는 손 플레이/연속 녹화, 앱 전체 재실행 저장, 최종 GameView 픽셀 순서. 테스트의 내부 함수/realPointer 입력과 손 조작은 구분한다.

### 사용자 확인 순서·다음 한 작업

1. Cyberpunk에서 **Tools → CursorDash → 화면 편집**, Main→MainSettings→InGame→InGamePause→Returning→Tutorial→FeverNotice. 원본 선택/작은 편집/Undo·저장/다시 전환이 유지되는지. 샘플은 실제 게임 기록에 반영되지 않는지.
2. Play: 메인→설정 슬라이더→시작→숫자 하나/로고/Nexus·커서 등장, 커서가 Nexus 위/HUD 아래인지. 임시로 수정한 배치가 유지되고 복귀에도 기준으로 돌아오는지. 검수 후 승인된 기본값으로 직접 복원/저장한다.
3. 원형 접촉·돌진/육각 실제 잔상 반사·정지꼬리 방어, 점수·배율/충전/피격 되감기, 피버·실습/첫안내, 파괴→이번점수대기→BEST/Home을 확인한다. 육각 타이밍 재조정은 하지 않는다.
4. Pause/Continue/Retry/Home/종료중상태 전환 반복·세로/가로·긴숫자. 앱 재실행 기록 보존 및 모바일/WebGL은 별도 실제 환경에서 확인한다.

직접 수정할 경로와 우선순위는 [편집 안내](EDITOR_AUTHORING_GUIDE.md), 자산 분류는 [에셋 조사](EDITOR_ASSET_AUDIT.md). 일반 노드 겹침·새 Flash 그림·별도 사이버 재구축은 계속 보류한다. 이번 결과는 미커밋이며 추가 구현/게시 승인은 없다.

### 이번 작업 파일 목록 — DG 시작 사본 대비

HEAD 전체 차이가 아니다. 기존 변경 파일34개와 신규33개(.meta포함)다.

기존 파일 수정:

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.cs`
- `Cursor-Dash-master/Assets/Resources/CursorDash/LivingBackground.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scenes/Cyberpunk.unity`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Character/PlayerCharacterController.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/CircularNodeArtwork.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/HexagonalNodeRings.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeFlatGraphic.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CircuitPathGraph.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashCircuits.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGuideView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashLivingBackground.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashPractice.Guide.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashTypography.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverGaugeView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverNexusRig.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NexusEnergy.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NexusGameboardPresentation.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/NodeCreator.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/StageManager.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAuthoringTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAutonomousGuideTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashNexusPresentationTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSceneRegression.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSpaceFlowTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/EDITOR_AUTHORING_GUIDE.md`
- `docs/CursorDash/EDITOR_REFACTOR_STATUS.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`

신규 파일:

- `Cursor-Dash-master/Assets/Editor/CursorDashCircuitLayoutEditor.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashCircuitLayoutEditor.cs.meta`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.Board.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.Board.cs.meta`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.Circuits.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashScreenAuthoringWindow.Circuits.cs.meta`
- `Cursor-Dash-master/Assets/Editor/CursorDashVisualProfileEditor.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashVisualProfileEditor.cs.meta`
- `Cursor-Dash-master/Assets/Editor/CursorDashWorldAuthoringSetup.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashWorldAuthoringSetup.cs.meta`
- `Cursor-Dash-master/Assets/Prefabs/UI/GuidanceView.prefab`
- `Cursor-Dash-master/Assets/Prefabs/UI/GuidanceView.prefab.meta`
- `Cursor-Dash-master/Assets/Resources/CursorDash/CircuitLayout.asset`
- `Cursor-Dash-master/Assets/Resources/CursorDash/CircuitLayout.asset.meta`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NodeAppearance.asset`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NodeAppearance.asset.meta`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/CursorDashNodeAppearance.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/CursorDashNodeAppearance.cs.meta`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CircuitStrokeBuffer.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CircuitStrokeBuffer.cs.meta`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashCircuitLayout.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashCircuitLayout.cs.meta`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGuidanceView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashGuidanceView.cs.meta`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashNexusView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashNexusView.cs.meta`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAuthoringPlayTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashAuthoringPlayTests.cs.meta`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashWorldAuthoringTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashWorldAuthoringTests.cs.meta`
- `docs/CursorDash/EDITOR_ASSET_AUDIT.json`
- `docs/CursorDash/EDITOR_ASSET_AUDIT.md`
- `tools/cursordash_asset_audit.py`

---

## 이전 A~C 기록 (이하의 C 대기/Play 미승인은 D-072로 대체)

# 편집 가능한 프로젝트 전환 — 단일 진행 기록

## 최신 상태 — B/C 구현·편집 검수 완료, C 실제 Play 관문 대기

**전체 결과는 부분 완료다.** 구현 승인 자체는 계속 유효하다. 문서가 요구한 ‘대표 화면 실제 실행 후 확대’와 사용자의 ‘Play는 직접 담당’을 함께 적용하여 D/E/F/G로 아직 확대하지 않았다. 다음은 아래 사용자 Play 검수 결과 확인이며 작은 구현 결정을 재승인받으려는 대기가 아니다.

| 단계 | 현재 결과 |
|---|---|
| A | PASS: 최신 로고 포함 증분 보존/대상 확인/Console. 이번 Play 기준은 NOT_RUN, D-070 프레임은 과거 자료 |
| B | 구현·native 컴파일·정적/참조 확인 PASS. 게임 실행 보존은 사용자 검수 대기 |
| C | 메인/메인+설정 실제 View와 창 구현. EditMode10PASS, 실제 Scene 원본 편집/Undo/Redo/저장·재열기/선택 전환/기본 복원 PASS. 합성 GPU 캡처 확인. 실제 Play는 NOT_RUN이므로 단계 전체 통과는 아님 |
| D | 미착수. Pause 외형은 Settings의 공통 Build를 분리하면서 함께 저장했지만, 창의 인게임/Pause/종료 지원과 나머지 표시 이전은 미완료 |
| E/F/G | 미착수. 월드/절차 표시 확대·전반 폴더 정리·미사용 격리·통합/로컬 WebGL은 대표 실행 관문 뒤에 진행 |

### 실제 원본과 변경

완료된 MainLogo prefab/View/기본값/코드는 시작점과 동일하다. 새 MainMenuView(시작/BEST 보조 문구), SettingsView(설정/Pause 외형), ProjectedScoreView(숫자 하나/상태별 pose)를 Cyberpunk에 명시 참조했다. StageManager는 중심 흐름을 유지하며 참조만 추가했다. Placement/TMP/Image/pose를 기준으로 하고 자식 animation/opacity/fit만 실행 중 갱신한다.

기존 게임 파일 변경6개:

- `Assets/Scenes/Cyberpunk.unity`: 새 View3개 참조·인스턴스. 기존 scene 블록 중 변경은 StageManager 컴포넌트(1040675965), 자식 SettingsView가 추가된 StageManager Transform(1040675966), SceneRoots뿐이다.
- `Assets/Scripts/Gameplay/StageManager/StageManager.cs`: serialized View3개 참조/getter만 추가.
- `CursorDashSpaceFlow.cs`: 메뉴 View에 표시 위임/명시 Pause 참조, 로고와 기존 타임라인·입력 유지.
- `CursorDashSettingsUI.cs`: 외형 생성/배치를 View/DefaultLayout으로 위임. 기존 SetSheet/BeginHandle/DragHandle/EndHandle/SetValue/Sync/Save/ResetSettings/ReplayTutorial/Resume/Retry/Home **12메서드 본문은 시작점과 동일**(정적 비교이며 실행 검증 아님).
- `CursorDashTypography.cs`: 새 정식 View3종 아래의 TMP는 font scan이 덮어쓰지 않는 명시적 예외. 다른 텍스트는 기존 경로.
- `FeverScoreHud.cs`: 기존 숫자 한 개·정산/보간/vertex pulse를 유지하면서 저장된 projected View를 사용. glyph 중심 계산은 같은 연산의 공유 함수로 추출. `FeverElementMask`와 회로는 이미 BackgroundScoreRenderer를 명시 조회하므로 해당 경로/코드는 변경하지 않았다. 실제 피버/마스크 검수는 대기.

새 runtime 파일(+.meta): CursorDashMainMenuView / CursorDashSettingsView / CursorDashSettingsView.DefaultLayout / CursorDashProjectedScoreView(공유 CursorDashScoreGeometry 포함) / CursorDashViewFonts / CursorDashSheetHandle / CursorDashSettingSlider. 모두 기존 StageManager 폴더에 두었으며 대규모 폴더 재편 없음. 새 Editor 파일(+.meta): CursorDashAuthoringSetup(한 번만 적용하는 guarded 생성), CursorDashScreenAuthoringWindow. 새 검사(+.meta): CursorDashAuthoringTests. Prefab3개(+.meta): Assets/Prefabs/UI/MainMenuView, SettingsView, ProjectedScoreView. Fonts의 기존 공식 TTF로 같은 파라미터의 Medium/Bold View SDF2개(+.meta, atlas/material subasset)를 추가했다. 기존 TMP/TTF/Shader를 일괄 저장하지 않았다.

새 정식 폰트/스타일을 runtime scan이 다시 덮지 않으며, 실제 표시와 미리보기는 전용 font/atlas/material 복사본으로 렌더링한다. 기존 Tuning 키/JSON/프리셋/웹 도구/저장 키/게임 규칙은 변경하지 않았다. View 미연결인 기존 다른 씬에는 호환 생성 경로를 남겼다. 새 View의 style/pose와 기존 프로필 legacy 경로는 조건부이며 동시에 정식 기본값으로 사용하지 않는다.

### 실행 근거 / 실패 이력

근거는 Unity 루트 `Logs/editor-refactor-20261101/` 아래에 보관한다.

- native 컴파일 후 Console error/warning0, Play를 한 번도 시작하지 않음. EditMode 집중10건(새7/기존 로고3) 전부 PASS. `tests-final.json/xml` 참조. 최종 시각은 파일 finishedUtc를 사용한다. 초기 7PASS/3FAIL, 다음6PASS/4FAIL도 별도 보존했다.
- 초기 실패 중 reflection의 EditorWindow.Show 오버로드 선택 오류를 검사 준비에서 수정했다. **실제 새 Prefab 회귀**는 runtime-only였던 Slider/SheetHandle이 독립 MonoScript 파일이 없어 `m_Script:{fileID:0}`으로 저장된 문제다. 두 클래스를 이름/동작 그대로 각 `.cs`로 분리하고, 새 SettingsView prefab의9+1 Script 참조만 올바른 새GUID로 연결했다. 최초 수정은 null-script 행을 놓쳤고 재실패했으므로, 파일뿐 아니라 asset/scene 재열기 후 slider9/handle1/missing0 및 각 View의 누락 Script0을 다시 검증했다. 실패를 skip하거나 핵심 단언을 약화하지 않았다.
- `actual-source-edit-roundtrip.json`: 실제 MainMenuView.PromptPlacement의 위치/크기 임시 편집 → Undo/Redo → Scene 저장/재열기 → Main/MainSettings 선택 후 유지 → 기본값/불필요 Override 복원. Scene clean/PlayOFF. 전체 씬 스냅샷으로 미리보기를 복원하지 않았다.
- `settings-controller-parity.json`: 위12개 기존 사용자 조작/저장 메서드 원문 동일.
- `final-main-portrait.png`, `final-settings-portrait.png`, `final-settings-landscape.png`, `final-main-landscape-zero.png`: 실제 private PreviewScene/Camera에서1080×1920/1920×1080 합성. 로고·숫자·문구/설정 스타일·레이아웃을 육안 확인. 회로/숫자 마스크·후처리/왜곡·애니메이션은 해당 미리보기에서 미지원이며 창/안내에 표시했다. 실기기 또는 실제 Play 프레임이 아니다.
- 새 검사에서 화면/샘플 전환 전후 Scene dirty/root count·저장값·원본 font/atlas/glyph count 불변, 별도 PreviewScene의 게임 매니저/EventSystem 없음 및 정리 확인. Undo/Redo는 isolated prefab 검사와 위 실제 Scene 원본 검사로 구분했다.
- `original-file-check.json`: 시작1,632개 기존 파일 삭제0, 변경은 위6게임파일(문서 갱신 전 대조)뿐. 이후 문서 갱신은 별도로 추가한다. 기존 VisualPrototype/로고/노드·입력·꼬리·튜토리얼/피버 상태/공격·파편·카메라·배경/회로 소스/Packages/ProjectSettings는 시작점과 동일.

### 미검증 / 보존 / 현재 한계

**NOT_RUN:** 실제 시작·설정 클릭/드래그·Pause/Retry/Home·점수 획득/피버/사망·저장 재실행, 새 View의 Play 유지와 기존 연출 비교, 모바일·터치·WebGL·성능. 기존 Tuning.Read/기록 제외 정책 위험은 미해결 이전 이력이며 이번에 수정/검증했다고 하지 않는다. 원형/육각·겹침·새 Flash 그림·별도 사이버 재구축은 그대로다. 새 결과창/사운드/규칙 없음.

**격리0/삭제0:** G에 아직 도달하지 않았으므로 미사용 판정을 완료했다고 하지 않는다. Legacy/Pause/Result/Flame/Variants 및 사용자 보존자료를 추정으로 정리하지 않았다. 로컬 WebGL도 C 실제 실행 관문 전에는 빌드하지 않았다. 기존 배포 산출물·공개 URL은 변경하지 않음.

**복구:** 초기 manifest-start의 파일이 기준이다. baseBackup/Workspace에 새 시작점 Delta를 적용한 조합에서 수정한 기존6파일과 해당 문서만 골라 복원하고, 이번에 새로 만든 파일만 목록을 대조해 제거한다. 로고 이전 전 베이스만 복원하거나 HEAD/reset/clean/stash를 쓰지 않는다. 이후 사용자 편집이 생기면 이번 차이만 역적용한다. 단계별 파일/차이 목록은 milestone/manifest 및 task-start diff에 보존한다. 복구 실행은 하지 않았다.

### 다음 정확한 작업 / 사용자 검수

종료 대조: Editor PlayerPrefs12개/standalone0개의 존재·타입·값과 persistentData3파일 해시가 시작점과 동일하여 복원/초기화가 필요 없었다(`storage-preservation.json`). PlayOFF/timeScale1/compileOFF/Cyberpunkclean·새 View4종 연결 유효·preview 오브젝트0. imported dirty는 시작의 동일6개이며 저장·폐기하지 않았다(`native-final-state.json`). 기존1,632파일 삭제0, 예상한 게임6+문서5만 변경/새파일32. 단계 사본은 `D:/CursorDash_Backups/editor-refactor-20261101-2050/Milestones/BC-implemented`의 Files/manifest.json에 변경·신규만 보존했다. 전체 현재 작업 목록은 `Logs/editor-refactor-20261101/task-file-manifest.json`, 텍스트 차이는 `changes-from-task-start.patch`에 저장한다. 큰 Font/atlas 자산은 패치 대신 원본 사본·해시로 보존한다.

[편집 안내와 짧은 Play 순서](EDITOR_AUTHORING_GUIDE.md)의 ‘사용자 Play 검수 — C 관문’을 수행한다. 최소 Main→설정7개/닫기→시작→득점/점수 하나→Pause→계속/Retry/Home, Scene에서 바꾼 기준의 Play 유지와 로고/폰트·레이어를 확인한다. 실패하면 해당 연결만 수정/분리하고, 통과하면 별도 구현 재승인 없이 문서 D/E/F/G로 이어간다. 현재는 사용자 검수 가능한 Cyberpunk 편집 상태/임시 리소스 정리 상태로 멈춘다.

---

## 착수 시 계획과 조회 기록 (아래는 이력)

## 현재 단계 / 권한

2026-11-01, D-071. 첨부 `C:/Users/molph/Downloads/CursorDash_Full_Refactor_Instructions.txt` 전체가 실행 기준이다. 최신 본문에서 실제 Play 검수는 사용자가 담당한다고 정했으므로 자동 Play 및 EnterPlayMode 테스트는 실행하지 않는다. EditMode 컴파일/관련 순수·표시 검사는 진행한다. Git 커밋·브랜치·push·배포·패키지/서버 변경은 금지다.

- A: 시작점 보존 PASS, 대상/Console 조회 PASS. 이번 native Play 기준은 NOT_RUN(사용자 담당); D-070 실제 프레임은 과거 비교 근거로만 사용.
- B: 연결/표시 기준 분리 진행 중.
- C: 메인·메인+설정 대표 구성 구현 예정. 문서의 ‘대표 구성 실제 실행 후 확대’ 관문은 사용자 Play 결과 전까지 미통과로 둔다.
- D/E/F/G: 미착수. C 기반 검수 실패나 미검증을 통과로 간주해 일괄 이전하지 않는다. 독립적인 문서·정적 검사는 가능.

## 현재 기준과 복구

- main / HEAD `9121b786cb3e4fad98c44f8f304341a6ceaa8ba6`, 시작 추적 수정19개 및 기존 미추적 보존. 완료된 D-070 로고도 이번 시작점에 포함된다.
- Unity `D:/renewal_CursorDash/Cursor-Dash-master`, instance `Cursor-Dash-master@6cde4df4002de875`, Cyberpunk clean/PlayOFF/Console error·warning0. 다른 인스턴스는 목록 메타데이터만 확인했으며 Project_Brawl에 요청하지 않음.
- 새 증분 사본 `D:/CursorDash_Backups/editor-refactor-20261101-2050`의 manifest-start.json/git-status-start.txt/Delta. 베이스는 `D:/CursorDash_Backups/main-logo-view-20261101-1710/Workspace`다. 전체 현재1,632파일 중 달라진15파일(1,106,254바이트)을 덧보존했고 베이스 누락0. 복구 시 먼저 베이스를 기준으로 삼고 새 Delta를 덮어야 **현재 로고 포함 시작점**이 된다.
- Assets/.meta/Packages/ProjectSettings·문서/tools·AGENTS와 .gitignore 포함. 저장소 밖 PlayerPrefs 타입/값 및 persistentData3파일 보존. Library/Temp/기존 빌드·영상은 복제하지 않는다.
- imported TMP shader3/TTF Font Texture3의 dirty는 시작부터 존재한다. SaveAssets/포괄 저장·폐기하지 않는다.

## 선택한 편집 구조

| 요소 | 정식 원본 | 실행 중 값 | 편집 접근 |
|---|---|---|---|
| 로고 | 완료된 MainLogo Prefab/PlacementRoot/Image | 기존 AnimationRoot 이동·fade | 기존 경로 유지 |
| 시작 문구/BEST 보조 문구 | Scene에 연결할 MainMenu View Prefab | 자식 opacity·기록 pulse·점수 추종 offset | 실제 Scene/Prefab 선택 |
| 설정/Pause | Settings View Prefab의 Rect/TMP/Image/행 참조 | safeArea·자식 responsive fit·sheet/pause 진행 | 실제 Scene/Prefab 선택 |
| 공용 점수 | 실제 TMP 하나와 상태별 pose 데이터 | 기존 숫자 보간/vertex pulse/이동·색 | 대표 Main 미리보기에 필요한 최소 이전; 게임 점수 규칙 불변 |
| 배경/회로/노드/넥서스 | 이번 대표 단계에서는 기존 코드·VisualPrototype·씬 유지 | 기존 흐름 유지 | D/E 이후 확대; 미리보기의 한계 명시 |

Scene의 편집 기준을 코드가 덮어쓰지 않고 연출 자식만 갱신한다. 기존 사용자 설정/Tuning/JSON/웹 API/기록 제외 의미는 변경하지 않는다. 옮긴 표시만 명시적 참조를 쓰고 다른 씬의 실제 호환 생성 경로는 유지한다. 기존 이사만루 TTF를 재사용하되 runtime-owned font를 편집 모드에서 호출·저장하지 않는다. 새 정식 폰트 자산이 필요한 경우 원본 파라미터로 생성하고 미리보기 리소스와 분리한다.

## 실행 계획과 관문

1. 대표 View 연결과 편집 기준을 먼저 구현한다. 설정 저장/슬라이더/입력/시간 제어는 기존 Controller에 남긴다.
2. 두 상태만 지원하는 작은 Editor 창을 만든다. 실제 View의 임시 렌더링 사본만 미리보기하고 원본 선택으로 편집한다. 게임 매니저·저장·이벤트를 실행하지 않으며 scene 전체 snapshot 복원은 하지 않는다.
3. EditMode에서 상태 전환·샘플값·원본 편집/Undo·저장·리소스 정리를 확인한다. 1080×1920/1920×1080 합성 캡처와 미지원 카메라 효과를 구분한다.
4. 사용자가 실제 Main→설정→시작→Home을 검수할 항목을 인계한다. C 실제 실행 관문 통과 후 D/E/F/G로 계속한다. 이번 승인은 남아 있지만 미검증 관문을 임의 통과시키지 않는다.

## 실행한 조회 / 발견한 기존 위험

- 지시문1~끝/AGENTS/STATUS→MASTER_SPEC→DECISIONS/ENVIRONMENT/VALIDATION/SOURCES/D-070 읽음, Git root/HEAD/branch/status 조회.
- MCP instances/custom-tools/project-info/editor-state 및 Cyberpunk root/dirty 참조 조회. Console0 확인.
- 기존 Typography는 runtime SDF 생성과 전체 TMP scan으로 font를 대체한다. 새 View의 정식 font는 이 scan의 명시적 예외로 보존해야 한다. 기존 다른 텍스트의 폰트 경로는 유지.
- SettingsUI는 Build에서 외형·콜백을 함께 생성하고 Layout에서 기준 배치를 덮는다. 표시 데이터와 responsive 자식을 분리하고 기존 key/슬라이더/입력 코드를 유지한다.
- Tuning RuleSessionChanged 정책 위험은 기존 이력으로 유지. 정책 변경/무관한 버그 수정은 하지 않는다.

## 다음 한 작업

B/C 대표 View·참조를 구현하고 native 컴파일 및 EditMode 표시 검사를 수행한다. 변경 목록·단계별 차이·결과는 이 문서에 이어 쓴다. 현재 아직 새 게임 코드/씬 변경 없음.
