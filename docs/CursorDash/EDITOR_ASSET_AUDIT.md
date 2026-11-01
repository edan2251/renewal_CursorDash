# D-072 에셋 참조 분류

## D-075 씬 인스턴스 정리 (2026-11-02)

현재 Cyberpunk의 미사용6묶음38개만 제거했다: GameObject(1), DeathUI(5), Pause UI(9), Hold Achivement(1), EnemyArrows(21), Cyberpunk_Background(1). 외부 직렬화 참조0·문자열/동적활성화 경로·현재 명시 View/풀과 구분해 확인했다. **원본 Prefab/스크립트/이미지 파일을 삭제한 것이 아니다.** 기존 풀/GeneratePositions/호환 RemakeHUD·coreDeathAnimation과 Flash PNG는 유지한다. 기존698개 전체 에셋 분류의217개 판단보류를 미사용 확정으로 바꾸지 않는다. [근거/실행검사/복구](HIERARCHY_CLEANUP_20261102.md).

## D-074 현재 Scene 표시 슬롯 선별 정리 (2026-11-02)

`Remake/Cyberpunk_Background.png`는 `Cyberpunk_Background` renderer에서, `Original/Cyberpunk_Background_Flash.png`는 `RemakeHUD/Background_Flash` Image에서 제거/비활성화했다. 실제 Play에서 해당 두 표시 OFF·LivingBackground와 동적 회로 ON을 먼저 확인했다. 배경은 같은 실제 shader/quad/profile을 BoardView에 저장했다. Original Flash는 명확한 비교·미지원 복구 전용 BoardView 참조로 유지해 기존 PNG 비교 기능을 보존했다.

**Remake/Cyberpunk_Background_Flash.png는 별도 자산이며 `Flash/FlashImage` 성공 타격에 실제 사용하므로 유지한다.** 이름이 비슷하다는 이유로 삭제하지 않는다. Nexus Inside/Outside/Dotted sprites와 노드 sprites는 geometry/정렬/판정·fallback 참조에 쓰이므로 미사용이 아니다. MapInformation/StageSelect/_Recovery 참조도 그대로 유지했다. 이번 삭제 파일0, 격리0. 아래 D-072 전체분류 숫자를 이번 실제 로드 수로 재사용하지 않는다.

Unity AssetDatabase의 실제 GUID 의존성과 활성 빌드 씬, 보존 씬, Resources/문자열/Editor/검사/WebGL 템플릿 경로를 교차 확인했다. 실행 시 모든 자산이 실제로 로드되었다는 뜻은 아니다.

- 참조/컴파일 연결 확인: 481개
- 불확실하여 유지: 217개
- 충분히 검증된 미사용: 0개 / 격리·삭제: 0개

참조가 없는 것만으로 미사용을 확정하지 않았다. 특히 _Recovery/옛 씬·Sprite fallback·PNG 비교·Resources·이전 테스트는 보존한다. 전체 경로와 근거는 같은 이름의 JSON에 있다.

## 추가 확인 후보

| 경로 | 확인 수준 / 유지 이유 |
|---|---|
| `Assets/Animations/Nodes/AttackNode_Destroy.anim` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Animations/Nodes/DefenseNode_Destroy.anim` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Animations/Nodes/Fire_AttackNode_Effect_Burst_Sheet_0.controller` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Animations/Nodes/NormalNode.controller` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Animations/Nodes/NormalNode_Execute.anim` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Backgrounds/Original/background.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/OriginalSelection/#FFFFFF/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Player/Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/06_setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#000000/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#00A99D/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#2E3192/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#39B54A/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#662D91/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1C24/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#ED1E79/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#F15A24/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/00_Arrow.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/01_square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/03_triangle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/04_Pentagon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/05_Diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/06_Setting.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/07_Perforated square.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/08_Perforated circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Characters/Variants/#FCEE21/09_A crooked diamond.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Common/02_Circle.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Cyberpunk/Core/_Example.gif` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Cyberpunk/OriginalNodes/Cyberpunk_NormalNode_Sheet 1.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Cyberpunk/Types/Cyberpunk_AttackNode/_Example.gif` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Cyberpunk/Types/Cyberpunk_NormalNode/_Example.gif` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Fire/Core/_Example.gif` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Fire/Types/Fire_DeathNode/_Example.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Fire/Types/Fire_DeathNode/_Example_2.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/Fire/Types/Fire_NormalNode/_Example.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/OriginalObjects/NodeSheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/Nodes/OriginalObjects/NormalMaterial.mat` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Branding/cursordashICON.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/CharacterSelect/Customize/Character_color.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/CharacterSelect/Customize/Character_type.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/CharacterSelect/Customize/Character_type_lock_effect.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/CharacterSelect/Original/--.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/CharacterSelect/Original/Character_type_lock_effect.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Numbers/Number_0~9.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/OriginalMenu/Map_image_border.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/OriginalMenu/Menu.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/OriginalMenu/_.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Credit/Ui_Credit_All.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Credit/_Example.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Sample.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Ui_Top/Ui_Top_Creator.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Ui_Top/Ui_Top_Download page.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Ui_Top/Ui_Top_Facebook 1.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/Settings/Ui_Top/Ui_Top_Facebook.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/TutorialPages/TIP_BUTTON_Left.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Art/UI/TutorialPages/TIP_PAGE2.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Audio/SFX/Lazer.mp3` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Audio/SFX/button.wav` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Audio/SFX/효과음 가이드.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/DOTween.XML` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/DOTween.dll` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/Editor/DOTweenEditor.XML` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/Editor/DOTweenEditor.dll` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/Editor/Imgs/DOTweenIcon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/Editor/Imgs/Footer.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/Editor/Imgs/Footer_dark.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/Editor/Imgs/Header.jpg` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Demigiant/DOTween/readme.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/NaughtyAttributes/README.html` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/NaughtyAttributes/Samples/DemoScene/TestAssets/NaughtyScriptableObject.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/NaughtyAttributes/package.json` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Plugin/LitJson.dll` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Plugins/Demigiant/DOTween/DOTween.dll.mdb` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Plugins/Demigiant/DOTween/Editor/DOTweenEditor.dll.mdb` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Plugins/Demigiant/DOTween/Editor/Imgs/DOTweenMiniIcon.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Prefabs/VFX/NodeEffect.prefab` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Resources/0.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/Resources/DOTweenSettings.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Paperlogy-5Medium.ttf` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Paperlogy-7Bold.ttf` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/Style Sheets/Default Style Sheet.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Resources/TMP Settings.asset` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_Bitmap-Custom-Atlas.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_Bitmap-Mobile.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_Bitmap.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF Overlay.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF SSD.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP LIT.shadergraph` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP UNLIT.shadergraph` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Masking.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Overlay.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile SSD.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile-2-Pass.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Surface-Mobile.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Surface.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-URP Lit.shadergraph` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-URP Unlit.shadergraph` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMP_Sprite.shader` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMPro_Mobile.cginc` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Shaders/TMPro_Surface.cginc` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Sprites/EmojiOne.json` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/TextMesh Pro/Sprites/EmojiOne.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/ThirdParty/Gonggames/OFFICIAL_TERMS_KO.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/ThirdParty/Gonggames/README.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/Cyberpunk_AttackNode_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/Cyberpunk_AttackNode_Effect_Nexus Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/Cyberpunk_AttackNode_Effect_clogging_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/Cyberpunk_Core_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/Cyberpunk_NormalNode_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/Cyberpunk_NormalNode_Effect_Nexus Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/CharacterSelect/Original/stick color code.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Original/Cyberpunk_AttackNode_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Original/Cyberpunk_AttackNode_Effect_Nexus Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Original/Cyberpunk_AttackNode_Effect_clogging_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Original/Cyberpunk_NormalNode_Effect_Nexus Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Original/stick color code.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Remake/Cyberpunk_AttackNode_Effect/Cyberpunk_AttackNode_Effect_Nexus Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Remake/Cyberpunk_AttackNode_Effect/Cyberpunk_AttackNode_Effect_clogging_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Remake/Cyberpunk_Core_Effect/Cyberpunk_Core_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Remake/Cyberpunk_NormalNode_Effect/Cyberpunk_NormalNode_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Remake/Cyberpunk_NormalNode_Effect/Cyberpunk_NormalNode_Effect_Nexus Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Cyberpunk/Remake/stick color code.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Fire/Sprites/Fire_AttackNode_Effect/Fire_AttackNode_Effect_Burst_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Fire/Sprites/Fire_AttackNode_Effect/Fire_AttackNode_Effect_Node Attack_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Fire/Sprites/Fire_AttackNode_Effect/Fire_AttackNode_Effect_clogging_Sheet.png` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |
| `Assets/VFX/Fire/Sprites/stick color code.txt` | 정적 참조 미발견. 소유자/동적 로딩/보존 용도 확인 후 별도 제거 비교 필요 |

## 검증 한계와 후속 격리 방법

직렬화 GUID 의존성은 UnityEvent·Animation/Animator의 직렬화된 연결도 포함한다. 문자열로 계산되는 Resources/씬 이름, 외부 MCP 실행 경로, 조건부 컴파일의 실제 실행은 정적 검색만으로 모두 확정할 수 없다. JSON의 dynamicCalls를 함께 확인한다.

후속 격리는 후보별 실제 접근/씬·테스트·빌드 비교를 통과한 뒤 원본과 .meta를 저장소 밖에 함께 보존하고 GUID/복구 경로를 기록해야 한다. 이번에는 확정 근거가 없어 실행하지 않았다. 사용자 백업/복구 씬/참고 자료는 삭제하지 않았다.
