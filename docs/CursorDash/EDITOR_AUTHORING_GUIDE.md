# CursorDash 직접 편집 안내 — D~G 구조

## 새 하단 랭킹·설정 — D-077 (2026-11-02)

Tools→CursorDash→화면 편집에서 **메인 / 설정 / 랭킹**을 누른다. 기존 상태 버튼/실제 Scene 편집 방식 그대로다. 공통 원본은 `Assets/Prefabs/UI/SettingsView.prefab`, 실제 Scene 인스턴스는 `StageManager/SettingsView`다.

- 하단 두 제목/아이콘: `StageManager/SettingsView/Safe area/Main navigation placement/{Ranking,Settings} header animation`의 Menu title TMP와 Menu icon Image.
- 설정 본문/초기화/튜토리얼/언어 버튼: `.../Safe area/Main bottom sheet/Settings placement/Settings content`.
- 랭킹 본문: `.../Safe area/Main bottom sheet/Ranking placement/Ranking content`.
- View의 Navigation Height(72), Panel Travel Seconds(.24), sheetHeightFraction(.95)가 높이/이동 시간을 제어한다. Placement와 TMP/Image는 디자인 기준이며 header/sheet/content의 Animation offset은 코드 소유다.
- 한국어는 해당 TMP 원문, 나머지3언어는 SettingsView→Menu Texts에서 수정한다. 일본어/간체는 메뉴용 static Sarasa atlas이므로 새 문자를 추가할 때 atlas 재생성이 필요하다. 현재 KO/EN/JA/간체 문구는 glyph검사를 통과했으며 Pause/튜토리얼 본문 전체 번역은 하지 않았다.
- Google SVG 원본은 `Assets/Art/UI/MenuIcons/Sources`, 실제 Image sprite는 `Assets/Resources/CursorDash/MenuIcons`. 기존 Icon Kind는 기본 연결용이다. Image Sprite를 직접 편집하면 값이 있는 동안 매 프레임 기본 sprite로 덮어쓰지 않는다.

현재 검증은 비Play Editor26과 private 편집 렌더뿐이다. 실제 클릭/드래그·언어 저장/재실행·모바일 터치는 [수동 순서](BOTTOM_PANELS_20261102.md)에 따라 확인한다.

## Hierarchy에서 찾는 위치 — D-075 (2026-11-02)

현재 Scene 루트는 `----[Manager]----`, `----[Camera & Input]----`, `----[Background]----`, `----[Gameplay]----`, `----[Pool & Spawn]----`, `----[UI]----` 순서로 구분된다. 구분선은 빈 오브젝트이며 다른 요소의 부모가 아니다. 기존 편집 경로와 Tools→CursorDash→화면 편집 사용법은 그대로다. **StageManager/SettingsView는 Manager 구역 안의 기존 자식**, Nexus는 UI 구역의 UI/NexusPlacement, 메인 로고/문구/점수는 각각 MainLogo/MainMenuView/ProjectedScoreView에 있다. Nodes·AfterImageCollision·GeneratePositions는 실제 게임 풀이므로 비활성이어도 삭제하지 않는다. [이번 제거·보존 목록](HIERARCHY_CLEANUP_20261102.md).

## 가장 쉬운 현재 사용법 — D-074 (2026-11-02)

1. `Assets/Scenes/Cyberpunk.unity`를 연다. Play는 누르지 않는다.
2. **Tools → CursorDash → 화면 편집**에서 **메인 / 메인 옵션 / 인게임 / 일시정지 / 게임 종료·점수 대기**를 누른다. Scene 뷰가 실제 화면 구성으로 맞춰진다. 튜토리얼/첫 피버 안내도 같은 방식이다.
3. Scene 또는 Hierarchy에서 실제 오브젝트를 선택하고 Inspector로 수정한다. 창의 작은 **로고 / 점수 / 시작 문구 / BEST 문구 / 설정·Pause 배치 / 넥서스 / 안내 문구** 버튼으로 바로 선택할 수도 있다.
4. Ctrl+Z/Redo와 Scene 저장을 사용한다. 저장/Play 직전에 임시 표시 상태만 원래대로 정리되며 편집한 위치·글꼴·Image 등은 남는다. `편집 표시 종료` 또는 창닫기로 원래 편집 상태에 돌아간다.

| 바꾸려는 것 | 실제 편집 위치 |
|---|---|
| 로고 위치·크기 / 이미지 | `MainLogo/PlacementRoot` / 그 아래 AnimationRoot/Logo의 Image |
| 시작 안내 / BEST 보조 글씨 | `MainMenuView/PromptPlacement/...`의 TMP / CaptionPlacement 하위 TMP. BEST/이번 점수 등 상태별 문자열은 MainMenuView 필드 |
| 설정 패널 배치 | `StageManager/SettingsView/Safe area/Main bottom sheet/Settings placement` 및 그 아래 실제 TMP/Image/Slider |
| Pause 패널 배치 | `StageManager/SettingsView/Safe area/Pause overlay/Pause placement` 및 그 아래 실제 TMP/Image/Slider |
| 점수 상태별 위치·색·크기 | `ProjectedScoreView`의 Main/Combat endpoints. 공통 위치 보정은 PlacementRoot. 폰트·Auto Size는 AnimationRoot의 TMP. 샘플 숫자를 바꾸어도 저장 점수는 바뀌지 않음 |
| 넥서스 위치·크기 / 색 | `UI/NexusPlacement` Rect / `UI`의 NexusView Energy Tint. 본체 재질은 `NexusBody.mat`, 기존 sprite는 compositor에 필요한 원본 기하 참조 |
| 튜토리얼 글꼴·위치·색 | `RemakeHUD/GuidanceView/Checklist placement/Checklist`. 횟수 문구는 GuidanceView 템플릿의 {0}를 유지 |
| 첫 피버 안내 글꼴·문구·배치 | `RemakeHUD/GuidanceView/First fever notice/First fever wording` TMP |
| 배경·회로 | Scene `BoardView` Inspector의 **배경 색·흐름·회로 대비 편집 / 회로 경로 편집**. 기존 VisualPrototype/CircuitLayout이 정식 출처 |

현재 Scene만 바꾸려면 Scene 인스턴스를 저장한다. 공통 기본 디자인은 Prefab 원본을 편집한다. Prefab Mode에서는 기존 설정/Pause/안내 표시 버튼도 계속 사용할 수 있다. `AnimationRoot`, opacity, sample health/gauge, 생성된 `PREVIEW ONLY` 회로·게이지 Mesh는 연출/표시 결과이므로 기준 배치 편집 대상으로 쓰지 않는다.

**정확한 차이:** 화면 선택은 실제 View/이미지/폰트/배치와 실제 배경 shader/profile·회로 graph/glyph mask를 사용한다. 게임의 Main/Starting/Playing 같은 상태는 실행하지 않는다. Main은 넥서스 숨김, 인게임은 정지된 기본 체력과 빈 게이지다. 체력/충전·점수/화면비 샘플은 접어 둔 고급 항목이다. 원형/육각은 필요할 때 대표 Mesh 샘플을 켠다. 호흡·회전·피버 이동·후처리·타격과 최종 파괴는 Play에서 검수한다. `게임 종료`는 기존 최종 점수 클릭 대기이며 삭제된 ROUND OVER 창이 아니다.

**옛 PNG:** 기본 배경/회로용 두 슬롯은 대체했지만 Remake `Cyberpunk_Background_Flash.png`는 성공 타격이 실제 사용하므로 유지했다. 비교·복구 전용 이미지와 다른 보존 씬도 삭제하지 않았다. 과거 자료를 전부 미사용으로 분류하지 않는다.

아래 D-073의 개별 원본 표시 순서는 이전 보조 사용법이다. 기본 흐름은 위 상태 버튼을 사용한다.

2026-11-01 / D-072. A~C는 사용자 검수 PASS. 아래는 최신 편집 구조이며 이전 안내는 하단 이력이다. 게임 시작·시간·점수·충돌은 기존 시스템이 담당하고, 편집 도구가 게임을 대신 실행하지 않는다. 전체 검증과 현재 단계는 [진행 기록](EDITOR_REFACTOR_STATUS.md).


## 최신 — UI를 실제로 보며 편집하기 (D-073 / 2026-11-02)

1. **SettingsView Prefab:** Project의 `Assets/Prefabs/UI/SettingsView.prefab`을 연다. 실제 설정이 표시되고 화면에 맞춰진다. 루트 Inspector의 **설정 UI 표시 / 일시정지 UI 표시**로 같은 원본의 두 디자인을 고른다.
2. **Scene 인스턴스:** Cyberpunk → Tools → CursorDash → 화면 편집 → MainSettings 또는 InGamePause → **설정 원본 · Scene에서 표시**. 실제 `StageManager/SettingsView`를 선택/표시한다. 공통 기본값은 Prefab, Cyberpunk만 바꾸려면 Scene 인스턴스를 수정/저장한다. Override는 필요한 항목만 Apply/Revert한다.
3. **배치:** `Safe area/Main bottom sheet/Settings placement` 또는 `Safe area/Pause overlay/Pause placement`의 RectTransform. 그 아래 `Settings content` / `Pause content`의 실제 TMP/Image/slider Track/Handle에서 글꼴·글자 크기·문구·색·이미지·치수를 수정한다. 핸들12×12/터치48/track4는 기존 기준이다.
4. **안내:** `Assets/Prefabs/UI/GuidanceView.prefab` 또는 `RemakeHUD/GuidanceView`를 선택하고 Inspector의 **체크리스트 표시 / 첫 피버 안내 표시**. 체크리스트의 실제 문구는 View 템플릿({0} 유지), 첫 피버 문구는 `First fever notice/First fever wording` TMP.text다. 폰트·색·배치는 각 TMP/Rect에서 편집한다. 편집용 흰 대비판/환경Canvas는 저장되는 게임 배경이 아니다.
5. **공용 점수:** `ProjectedScoreView`의 Inspector에 **메인 색으로 표시 / 인게임 색으로 표시**. 상태별 위치/색/기준크기는 View Main/Combat, 글꼴·AutoSize는 `AnimationRoot` TMP. 4×4/Scale1/Auto1~3.5는 기존 기준이다. 숫자 하나를 실제 모델이 갱신하므로 TMP의 숫자를 실제 점수값 설정으로 쓰지 않는다.
6. **원본과 편집 표시:** root Canvas는 화면 대응용 frame이다(Settings540×960, Menu1080×1920/Scale1 기준). 위치·크기는 위 Placement를 수정한다. Safe area/Animation 자식 fit·sheet offset·CanvasGroup alpha는 실행 소유다. 편집 중만 평면 WorldSpace로 보이며, 저장·Play 전에 Overlay로 복원된다. 실제 File→Save와 Undo/Redo/재열기를 검사했다. 표시 종료가 Placement·폰트·이미지·문구·색 편집을 취소하지 않는다.
7. **배경:** 화면 창의 **실제 배경 흐름 미리보기 / 배경 표시 시간(초)**. 실제 LivingBackground shader/VisualPrototype 팔레트·흐름값과 공통 ApplyFrame/PlaceField를 쓴다. 게임 시간·저장값은 바꾸지 않는다. Main의 넥서스는 실제 Main 규칙에 따라 숨김이며 InGame에서 표시된다. Play 전 Game View의 원본 PNG/기본 UI와 실제 실행 Main을 혼동하지 않는다. 회로 전파·피버·후처리·동적 합성은 여전히 Play 전용이다.
Prefab/Scene의 직접 편집 표시는 기본 이미지·글꼴·완성 배치를 보이는 용도다. Shade Image의 디자인 색/알파는 그대로 보여주며 실제 게임에서 추가되는 .32/.82 가림·등장/fade는 Play/조합 미리보기에서 확인한다. 이를 보정하려고 디자인 색을 임의로 덮지 않는다.

8. **검수 순서:** 설정/Pause/안내 원본을 눈으로 확인 → 작은 위치·폰트·이미지 수정/Undo/Redo·저장/재열기 → 원본에 남는지 → Play 설정열기/닫기·Pause/Continue/Retry/Home → 종료. 임시 검수 수정은 원하는 기본값으로 직접 복원/저장한다. 모바일·실터치/이번 WebGL 재실행은 별도 검수다.

이번 실제 캡처와 검사 범위는 [진행 기록](EDITOR_REFACTOR_STATUS.md)의 최신 D-073를 따른다. 아래 문서의 D-072는 구조 설명이며 실제 UI 표시법은 이 절이 우선한다.


## 화면 선택과 원본 편집

`Assets/Scenes/Cyberpunk.unity` → **Tools → CursorDash → 화면 편집**. Main / MainSettings / InGame / InGamePause / Returning / Tutorial / FeverNotice를 고른다. Returning은 파괴 후 이번 점수 대기 대표 화면이다. 별도 결과창을 추가하지 않았다.

세로1080×1920/가로1920×1080, 점수·체력·게이지·노드 충전/육각 형태 샘플은 창에만 저장된다. 게임 저장값·실제 체력/공격/기록을 바꾸지 않는다. 원본 선택 버튼으로 실제 Scene/Prefab/설정 자산을 선택하고 Inspector에서 편집한다. 미리보기 사본은 원본이 아니며 닫으면 정리된다. Undo/Redo는 원본에 적용된다. Scene을 저장하면 화면을 바꾸거나 재실행해도 편집값이 남는다. Play 중 값은 기본값 저장으로 간주하지 않는다.

**정적 지원:** 저장된 로고/메뉴/점수/설정·Pause, 원본 Nexus 이미지/에너지·게이지 셰이더, 실제 Node Mesh 제작 함수, 실제 커서 sprite, 실제 회로 graph·union/glyph-mask 셰이더. **Play 전용:** 실제 충돌/잔상 방어, 시간이 흐르는 호흡·충전·회전·전파/퇴장, 피버의 동적 흑백/고스트 색상/공간 왜곡·RGB/최종 합성, 시작·사망 타임라인. 정적 미리보기는 픽셀 단위의 최종 후처리 재현이나 실기기 검수가 아니다.

## 무엇을 어디에서 수정하는가

| 대상 | Unity에서 수정할 정식 원본 | 실행 중 담당 / 주의 |
|---|---|---|
| 로고 | MainLogo/PlacementRoot의 RectTransform / AnimationRoot/Logo Image | 기존 AnimationRoot 오른쪽 이동/fade, 원본 배치는 덮지 않음 |
| 시작/BEST 라벨 | MainMenuView/PromptPlacement·CaptionPlacement의 Rect/TMP, View 문구 | 상태별 alpha·caption 추종만 controller가 소유 |
| 공용 점수 | ProjectedScoreView의 Main/Combat endpoints·Tint·MainGlyphScale, AnimationRoot TMP | 실제 숫자·득점 pulse/글자 중심은 FeverBackgroundScore. 숫자는 하나 |
| 설정/Pause | StageManager/SettingsView의 Settings placement / Pause placement 아래 Rect·TMP·Image | Safe area/fit/연출 자식은 runtime 소유. value는 사용자 저장값 |
| Nexus 본체·점선 | UI/NexusPlacement의 RectTransform(전체 위치·uniform Scale), 그 아래 HpBar·HpCentral의 RectTransform/Image와 UI의 CursorDashNexusView | HP 경계는 기존 UI 크기/solidFraction으로 계산. 본체는 정사각·전체Scale은균등을 유지한다. 화면 흔들림 때문에 UI 원본을 움직이지 않음. HpFrame은 실제5원호 게이지로 교체됨 |
| Nexus 에너지/게이지 외형 | UI → CursorDashNexusView의 Energy Tint / Gauge Radius Ratio(기본1.86) / Gauge Thickness / Gauge Gap Degrees | 기준색과 Image tint가 기존처럼 곱해짐. 체력/실충전·자성/흡입·회전 연출은 기존 소유자 |
| 원형·육각 실제 Mesh | Assets/Resources/CursorDash/NodeAppearance.asset | outline/core/glow·원형 fill/rim·육각 두 겹 명암/몸체 stroke/fill. 반경/판정·공격 시계는 옮기지 않음 |
| 원형·육각 풀/Prefab 크기 | 기존 Nodes의 각 원형·육각 Prefab 인스턴스와 그 원본 | Sprite는 fallback/원본 크기·정렬 참조이며 실제 원형/육각은 Mesh다. Sprite 교체가 Mesh 디자인 변경은 아님. 풀·NodePhase 유지 |
| 커서 | PlayerCharacter/CursorVisual SpriteRenderer·Transform | 기존 CharacterSetting 선택 sprite/color 및 사용자 cursorColor가 있으면 우선한다. 실제 입력/이동·충돌은 Controller. 편집한 표시 Scale/비율은 기본으로 유지하고 명시 Dev visualScale만 기존 uniform Override를 사용한다. 꼬리는 AfterImage의 별도 표시/방어 경로이며 바꾸지 않음 |
| 배경 팔레트·흐름 | Assets/Resources/CursorDash/VisualPrototype.asset의 Background Navy/Teal/Violet 및 backgroundSpeed/Mix/Bend | 기존 shader·시계/타격 반응을 재사용. 본 작업은 같은 초기색 이전 |
| 회로 좌표 | Assets/Resources/CursorDash/CircuitLayout.asset의 points | 중앙0, normalized X/Y(+위). root/branch 연결·점 순서는 유지하고 좌표만 수정. profile.circuitLayout 명시 연결 |
| 회로 굵기·대비·흐름 | VisualPrototype의 Circuit 항목들 | 실제 경로 기반 재생·숫자 획 가림·피버 게이지 퇴장/게임오버·복원 유지 |
| 튜토리얼 체크리스트 | RemakeHUD/GuidanceView/Checklist placement/Checklist TMP + GuidanceView 템플릿 문자열 | {0}가 실제 횟수. 완료 조건·실제 공급/충돌은 기존 Practice/GuideFlow. 기본 텍스트는 runtime 동적 횟수로 대체됨 |
| 첫 피버 문구·배치 | RemakeHUD/GuidanceView/First fever notice/First fever wording TMP | 정확한 두 줄 유지가 기본. opacity/root active는 기존 notice 시간표가 제어. 배경 Image는 clear이며 실제 피버 세계가 흰색 제공 |
| 주요 반응/VFX 시간 | VisualPrototype의 Nexus/Space/Fever/회로/점수 반응 항목 / 기존 Tuning 효과 항목 | 절차적 VFX를 Scene에 다 배치하지 않음. 기존 effect strength/접근성·Dev Override·JSON 체계 유지 |

## 기본값과 Override의 우선순위

- Scene/Prefab View 배치·Typography·스타일이 정식 기본값. Scene 인스턴스를 편집하면 이 씬만, Prefab 원본은 공통 기본값을 바꾼다. 기존 Override가 우선하므로 Apply/Revert는 의도한 항목만 수행한다.
- NodeAppearance/VisualPrototype/CircuitLayout은 공유 표시 데이터다. 런타임 생성 Mesh/Material은 해당 값을 읽는 결과이며 편집 원본이 아니다.
- AnimationRoot, 생성 Mesh의 Transform/Material, CanvasGroup alpha, 게이지 표시값/글자 mesh는 실행 중 연출값이다. 이 값을 바꿔 기본 배치를 수정하지 않는다.
- 사용자 설정은 기존 UserKey/CharacterSetting, 명시 Dev/프리셋/JSON은 기존 Override다. 외형 자산을 읽는 것과 명시 Dev 변경은 기록 인정에서 구분한다.
- 전투·이동·피해·공격 시점 같은 **게임 규칙 Inspector/개발자 값**은 이번 외형 이전과 다르다. 기존 메타 기본값 비교/기록 제외 정책은 유지하며, 이를 바꾸는 요청은 별도 판단이 필요하다.
- VisualPrototype의 옮긴 로고/점수 기본값은 미연결 옛 씬의 호환 경로다. 기본 Inspector에서는 숨기고 ‘미연결 씬의 호환 기본값 표시’로만 볼 수 있다. 삭제/API 변경은 하지 않았다.

## 코드에서 찾을 위치

기존 Gameplay/StageManager 폴더를 유지한다. StageManager/SpaceFlow=상태·시간·입력 게이트, ScoreManager/FeverState=정산·규칙, *View/Artwork=표시·기준, PlayerCharacterController/Node=실제 조작·전투, CursorDashTuning/Storage=Override·저장, Assets/Editor=편집 도구다. CircuitPathGraph는 순수 연결/경로 수학, CircuitStrokeBuffer는 vertex 업로드만, CursorDashCircuits는 기존 전파·퇴장·합성 수명 관리다. 새 노드의 공급/소유권은 기존 NodeCreator/NodePhase에 붙이고 표시만 별도 Artwork/데이터로 추가한다. 새 게임 매니저나 저장 시스템을 만들 필요가 없다.


## 이번 검수에서 먼저 확인할 것

1. 화면 편집 창에서 일곱 상태를 바꾸고 실제 원본 선택 버튼을 사용한다. 숫자·HP·게이지는 샘플이며 Play 기록과 분리된다.
2. UI/NexusPlacement를 조금 이동/균등 확대하거나 NodeAppearance의 색·outline을 잠시 수정해 정적 표시와 Play가 같은 기준을 사용하는지 확인한다. 위치·크기는 실제 피격 경계에도 영향을 줄 수 있으므로 확인 후 승인 기본값으로 복원한다. 원호·몸체의 런타임 변형 Transform을 기본 배치로 쓰지 않는다.
3. 일반·튜토리얼·첫 피버 안내의 배치/문구와 커서가 Nexus 위·설정 아래에 보이는지 실제 Play에서 확인한다. 정적 창이 최종 후처리의 픽셀 순서를 보장하지 않는다.
4. 원형·육각 방어/파괴→피격 되감기→피버 진입·종료→사망·이번점수대기→Home, Pause/Retry를 확인한다. 육각 시계/기존 규칙은 이번에 수정하지 않았다.
5. 모바일·브라우저 WebGL·터치·앱 재실행 저장은 별도 환경에서 검수한다. 이번 로컬 빌드는 `D:/renewal_CursorDash/Builds/LocalDevPreview/editor-refactor-DG-20261101-local`이며 배포하지 않았다.

기존6 imported dirty 항목과 자체 Bold View SDF atlas의 메모리 캐시1은 저장/폐기하지 않았다. 두 View SDF 디스크는 DG 시작 내용으로 보존됐다. 자동 승인 검토로 강제 재임포트와 추가 저장 데이터 재조회는 실행하지 못했으므로, 이를 최종 통과로 읽지 않는다. 최종 실행·복구·미검증 범위는 [진행 기록](EDITOR_REFACTOR_STATUS.md)의 최신 D-072를 기준으로 한다(2026-11-02 인계).

---

## A~C 당시 안내 (아래의 미지원·Play 대기 제한은 위 최신 상태가 대체)

# CursorDash 편집 안내 — 현재 A/B/C 대표 단계

2026-11-01. 전체 전환은 아직 완료되지 않았다. 메인·메인+설정의 실제 View와 공용 점수 기준을 먼저 이전했다. 사용자 Play 검수 후 D(나머지 화면), E(월드/절차적 표시), F/G(정리/통합 검수)로 이어간다. 자동 Play·빌드·Git게시 없음. 현재 단계/실제 검사/복구는 [단일 진행 기록](EDITOR_REFACTOR_STATUS.md)을 먼저 확인한다.

## 화면 편집 창

1. `Assets/Scenes/Cyberpunk.unity`를 연다. 미저장 변경은 사용자가 보존한다.
2. Unity 메뉴 **Tools → CursorDash → 화면 편집**.
3. `편집 중인 화면`: Main / MainSettings. 패널을 각각 켜고 끌 필요 없이 조합을 선택한다.
4. 1080×1920 또는 1920×1080을 선택한다. 실제 Game View 해상도나 프로젝트 설정은 바꾸지 않는다. 샘플 점수는 미리보기 전용이며 기록을 읽거나 저장하지 않는다.
5. 원본 선택 버튼으로 Scene의 실제 오브젝트를 선택하고 Inspector/Scene에서 수정한다. 임시 사본은 편집 원본이 아니다. Inspector 편집/Undo/Redo 뒤 미리보기가 갱신되며, 필요하면 ‘미리보기 새로고침’을 누른다.
6. ‘미리보기 종료’ 또는 창 닫기로 임시 카메라·View·폰트/재질·Mesh·RenderTexture를 정리한다. Play 진입·스크립트 리로드·씬 전환에서도 정리한다.

**지원 범위:** 실제 로고/메뉴/설정/공용 숫자의 정적 합성, 기존 LivingBackground 셰이더의 대표 색면. **미지원:** 회로와 숫자 획 마스크·후처리/왜곡·피버·전투·전환 애니메이션. 미리보기의 단순한 배경을 실제 게임 회로가 삭제된 것으로 해석하지 않는다. 현재 Game/Pause/종료 상태는 창에서 선택할 수 없다. 해당 지원은 D/E 단계에 남아 있다. 화면비 미리보기는 모바일 실기기 검수가 아니다.

## 실제로 수정하는 원본

경로는 Cyberpunk Hierarchy 기준이다. 이 씬만 바꾸려면 **Scene 인스턴스**를 수정/저장한다. 공통 기본값을 바꾸려면 아래 Prefab 원본을 편집한다. Scene Override가 있으면 Prefab보다 우선하므로 의도한 항목만 Apply/Revert한다.

| 수정 내용 | 정확한 위치 / 항목 |
|---|---|
| 로고 위치·크기 | `MainLogo/PlacementRoot` → RectTransform. 완료된 D-070 구조 그대로 |
| 로고 이미지·색 | `MainLogo/PlacementRoot/AnimationRoot/Logo` → Image Source Image/Color |
| 시작 문구 위치·크기 | `MainMenuView/PromptPlacement` → RectTransform |
| 시작 문구 내용·글꼴·색·글자 크기 | `MainMenuView/PromptPlacement/AnimationRoot/Start prompt` → TMP |
| BEST 보조 라벨 크기·추종 위치 보정 | `MainMenuView/CaptionPlacement` → RectTransform. 실제 점수 위 위치에 대한 상대 오프셋 |
| BEST/이번 점수 문구와 점수와의 간격 | `MainMenuView` → CursorDashMainMenuView의 라벨 필드 / Caption Score Gap |
| 보조 라벨 글꼴·색 | `MainMenuView/CaptionPlacement/AnimationRoot/BEST caption` → TMP. 내용/alpha는 상태에 따라 갱신됨 |
| 점수 공통 이동·크기 | `ProjectedScoreView` → RectTransform. 현재4×4 / Scale1 |
| 점수 상태별 위치·색·메인 배율 크기 | 같은 오브젝트의 CursorDashProjectedScoreView → Main endpoint / Combat endpoint. 위치는 viewport(0~1)에 공통 루트 위치를 더함 |
| 점수 폰트·AutoSize 범위 | `ProjectedScoreView/AnimationRoot` → TMP. 현재 AutoSize ON / Min1 / Max3.5 |
| 설정 내용의 기준 위치·크기 | `StageManager/SettingsView/Safe area/Main bottom sheet/Settings placement` → RectTransform |
| 설정 제목·항목·슬라이더 형태 | 위의 `Settings content` 아래 실제 TMP/Image/RectTransform. `user.master slider/Track` 등. 손잡이12×12 / track4 / touch48이 보존 기준 |
| 설정 높이·화면 여백 대응 | `StageManager/SettingsView` → CursorDashSettingsView의 Sheet Height Fraction/Inset 값 |
| Pause 기준 내용 배치 | `StageManager/SettingsView/Safe area/Pause overlay/Pause placement` 아래. 표시도 함께 분리했지만 실제 Pause 검수와 창 지원은 아직 미완료 |

정식 Prefab: `Assets/Prefabs/UI/MainLogo.prefab`, `MainMenuView.prefab`, `SettingsView.prefab`, `ProjectedScoreView.prefab`.

Prefab 단독 편집에서는 네이티브 Unity 저장 기능을 사용한다. 저장 후 Cyberpunk로 돌아와 조합을 새로고침한다. Prefab Stage 편집 중 조합 미리보기는 잠시 중단된다. 창이 수정값을 과거 씬 스냅샷으로 되돌리지 않는다.

## 편집 기준과 runtime 값

- Placement/pose/TMP 스타일이 기준이다. AnimationRoot·content/pause의 연출 자식은 runtime 이동·scale·fade/반응용이다. 이 자식의 현재 위치/scale/CanvasGroup alpha를 기본 배치로 편집하지 않는다.
- Main bottom sheet/Safe area는 화면 대응 프레임이다. 내용을 옮기려면 Settings placement를 수정한다. runtime이 프레임의 크기·safeArea·자식 fit을 계산한다.
- Canvas의 enabled, Renderer enabled, 슬라이더 value, 점수/배율 text는 상태·실제 데이터에 의해 갱신된다. 이 값을 Inspector에서 바꿔 게임 기본 설정이나 점수를 바꾸려고 하지 않는다.
- 점수는 TMP 하나다. 숫자 계산/표시 보간/획득 반응·확대는 기존 FeverBackgroundScore가 담당한다. Text Color 자체 대신 View의 Main/Combat Tint가 정식 색상 기준이다. glyph 중심 정렬은 기존 정책을 유지한다.
- 저장된 이사만루 TTF를 같은 SDF 파라미터(64/6/SDFAA/1024/Dynamic)로 사용한 View SDF 자산이 추가되었다. 실제 렌더링에는 전용 복사본을 써 원본 atlas를 채우지 않는다. 기존 다른 TMP의 runtime Typography 경로는 그대로다. 새 View 아래 텍스트의 정식 폰트는 global scan이 덮어쓰지 않는다.
- 기존 VisualPrototype의 옮긴 점수 위치/기본 색/메인 배치 필드는 View 미연결 호환 경로에서만 사용한다. 반응 시간·강도/점수 규칙은 기존 체계에 남는다. 로고도 D-070대로 View가 연결된 경우 Scene/Prefab 기준이 우선이다.
- 음량·효과 완화 등 사용자 값은 기존 Tuning/저장 체계, 개발자 Override는 기존 JSON/프리셋/API를 사용한다. 저장/기록 제외 정책은 바뀌지 않았다. Slider.value는 사용자 저장값의 새 기본값이 아니다.

## 아직 이전하지 않은 항목

배경/회로/노드/Nexus/커서/튜토리얼/피버 안내의 편집 구조 확장은 D/E에 남았다. 현재는 기존 `Assets/Resources/CursorDash/VisualPrototype.asset`, 씬·Prefab·기존 코드를 사용한다. 원형·육각 Mesh 외형이나 회로 전용 미리보기를 완료했다고 해석하지 않는다. 살아 있는 배경·피버 연출/타격 파편·공급/입력/피해/반사/점수·저장 규칙은 이번에 재설계하지 않았다.

새 UI는 실제 Prefab과 명시 참조 → 기준 Placement → 연출 자식 → 기존 Controller 연결 순서로 추가한다. MonoBehaviour를 Prefab에 저장하려면 클래스와 맞는 독립 `.cs` 파일을 사용한다. 새로운 화면 관리자나 별도 점수 관리자를 만들지 않는다. 새 노드는 기존 풀/NodePhase/소유권·처치 이벤트를 유지하는 계획이며 아직 예시 구현을 추가하지 않았다.

## 사용자 Play 검수 — C 관문

자동 Play는 하지 않았다. 다음은 실제 실행에서 확인이 필요한 항목이다. 사용자 기록/설정을 초기화하지 않는다.

1. **편집→실행:** MainMenuView의 PromptPlacement 위치를 조금 변경/Scene 저장 → 창에서 Main↔MainSettings 전환 → Play Main에서도 유지되는지. 확인 후 기본 배치로 복원/저장한다.
2. **기본 화면:** 로고·BEST 숫자·시작 안내가 이전 크기/색/위치이고 숫자가 중복되지 않는지. 한 자리/여러 자리 점수와 가로/세로를 확인한다.
3. **설정:** 하단 handle 클릭/드래그 → 7개 슬라이더·닫기·초기화/튜토리얼 버튼. 패널 뒤 시작 입력이 전달되지 않는지. 음량을 변경했다면 원하는 원래 값으로 직접 돌려 저장한다.
4. **시작:** 패널을 닫고 시작 → 로고 오른쪽 퇴장·하나의 점수 전환·넥서스/커서 등장. 연타/홀드로 중복 시작하지 않는지.
5. **일반 실행:** 득점 숫자·색·확대/되튐·중심·숫자 획 뒤 회로 가림. Pause의 계속/Retry/Home과 슬라이더 조작. Home에서 로고/문구/설정 복원이 정상인지.
6. **대표 회귀:** 피버에서 점수·색수차·숫자 마스크, 넥서스 파괴→이번 점수 대기→메인. 기존 정지 꼬리 방어/튜토리얼이 달라진 점이 있으면 별도 보고한다.
7. Play 종료 후 편집 기준 유지, 임시 preview 카메라/사본이 게임에 남지 않는지. 에러가 있다면 첫 스택과 발생 상태를 전달한다.

C 대표 관문 통과 전 D/E 일괄 확장·에셋 격리·로컬 WebGL 빌드는 진행하지 않는다. 구현 승인을 매번 다시 받으려는 것이 아니라, 문서가 요구한 실행 비교를 사용자가 맡는 분담이다.
