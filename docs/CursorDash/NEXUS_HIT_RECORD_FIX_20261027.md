# D-057 — 실제 원형 처치의 기록 제외 오류·강한 붉은 굴절 피격 시안

상태: **IMPLEMENTED / NATIVE_FOCUSED_70_PASS / 사용자 시각·손조작 검수 대기**. 사용자 최신 BEST0 재발·Editor 저장 질문·모든 실제 피격의 붉은 공간 파동/훨씬 강한 색수차·과장된 넥서스 반응 요청이다. 기존 미커밋 D-055/056, main `18b25da2a2f1d067a028a1ee855c9c8419fd63c4`를 보존한다. 빌드·커밋·푸시·배포는 하지 않는다.

## BEST 0의 추가 원인

지난 D-056은 피해 효과의 숫자 조회만 분리했다. **실제 원형 파괴의 색상 조회**가 남아 있었다. 새 검사는 정상 시작→일반 프레임1.2초→실제 `NormalNode.ContactAt/Interaction`→처치 점수1을 재현했다. 일반 프레임에서는 기록 가능했으나 처치 직후 기록 제외로 바뀌었다.

실제 실패 스택은 `NormalNode.ContactAt → Interaction → Stage.RegisterCircleKill → RegisterCircleSheetContact → HitFeedback.EmitCircle/EmitCore → Tuning.ColorValue/Read → RuleSessionChanged`다. float 보라색을 변환한 `#BF4CFFFF`와 메타데이터 `#BF4DFFFF`의 **초록 채널 1/255 차이**를 설정 변경으로 오인했다. `reproduced-color-failure.json/xml`에 원본 소스에서의 0/1 실패를 보존했다. D-056의 직접 모델 점수·수동 피해 검사는 이 실제 처치 경로를 포함하지 않아 이 오류를 잡지 못했다.

`FeedbackColorValue`를 기존 `FeedbackNumber`와 같이 연결해 **효과의 색을 읽기만 하는 행위**를 기록 제외에서 분리했다. 실제 반환 색·파편 외형/타이밍은 바꾸지 않았다. 색을 강제로 메타데이터 값으로 바꾸거나 훈련/DEV 기록을 정상 기록으로 허용하는 해결책이 아니다. 명시 Dev 활성화·프리셋/개발자 설정 변경·훈련·검수 조작의 기록 제외와 실제 게임 규칙 fallback 감시는 유지한다.

### Unity 저장 시점과 확인 방법

- 이 프로젝트는 **실제 게임오버 정산**에서 `CursorDashPointsRecord.Save`가 `PlayerPrefs.SetString`과 `PlayerPrefs.Save`를 호출한다. Unity Editor라고 저장을 막는 코드가 없다.
- 메인의 표시용0/countdown·현재 점수 초기화는 저장 최고점 키를 덮지 않는다. Play 중간에 Editor Stop으로 라운드를 끊는 것은 게임오버 정산이 아니며, 이번에 중도 종료 자동 저장 규칙을 추가하지 않았다.
- 훈련/DEV/수동 검수창의 피해·회복 버튼 등을 사용한 라운드는 기존대로 기록 제외다. 도구를 끈 뒤에도 해당 라운드의 제외는 유지되므로 새 정상 라운드에서 확인한다.
- 기존 **Tools → CursorDash → 넥서스 수동 검수** 창에 읽기 전용 `기록 자격 / 저장된 최고점 / 마지막 정산`을 추가했다. 읽는 것만으로 Dev를 활성화하거나 기록 제외하지 않는다. 창의 조작 버튼은 기존대로 검수 조작이다.
- 기존 개발자 보고서에도 `recordReason / pointsSaveStatus / pointsRecordKey / storedBestPoints / lastFallbackMismatch`를 추가했다. 별도 점수 관리자나 저장 체계를 만들지 않았다.
- 저장 직후 같은 키의 읽기 값을 확인하고 상태를 남긴다. 과거 저장되지 않은 점수는 가짜 값으로 복구하지 않는다. 사용자 기록을 초기화·삭제·샘플 값으로 덮지 않는다.

## 붉은 효과의 의미와 이번 연결

이전 구성은 일반 피해에 낮은 `.025` 전체 붉은 wash와 약한 국소광, 사망에 기존 `.26` 전체 붉은 flash가 있었다. 따라서 사망의 붉은빛만 두드러질 수 있었다. 실제 사용자 `user.flash / distortion / shake`는 기본값이었다.

이번 일반 피해의 전체 붉은 wash는 **0**이다. 넥서스 실제 접촉점(직접 HP 편집 등 위치 정보가 없으면 실제 넥서스 위치)을 중심으로 **압축→배경·회로 픽셀을 실제로 휘게 하는 바깥 전파→감쇠**가 일어난다. 그 이동하는 넓은 굴절 파동에 기존 피해 tint를 이용한 붉은 빛과 강한 RGB 분리를 함께 적용한다. 붉은 원·선을 그리거나 전 화면을 같은 색으로 잠깐 덮는 표현이 아니다.

일반 원형 충돌과 육각 레이저의 **확정 HP 감소 통지 한 번**에서 호출한다. 무효 피해·반사 성공·점수만 증가하는 사건은 붉은 피해 파동을 만들지 않는다. 기존 승인된 처치의 흰 공간 반응·파편은 변경하지 않는다. 마지막 일격은 기존 최종 파괴·flash `.26/.24초`·기존 RGB `.0055`/상한`.006`·슬로모션을 보존해 새 일반 파동을 중첩하지 않는다.

일반 피해의 RGB 설정은 `.0055 → .018`(약3.27배), 일반 피해 RGB 출력 상한은 `.006 → .024`, 변위 상한은 `.018 → .035`로 바뀌었다. 다른 피버·성공 효과의 상한을 일괄 변경하지 않는다. 실제 점수 글자 획과 별도 HUD는 보호한다. `fx.enabled`, 사용자 distortion/flash/shake 및 OnFlash/OnVibe 설정을 계속 존중한다. 연속 피해는 유한한 현재 사건을 갱신하며 변위를 무한 합산하지 않는다.

## 과장된 넥서스 검수 시안

조절 위치는 기존 `Assets/Resources/CursorDash/VisualPrototype.asset`의 Inspector (`CursorDashVisualProfile`)다. 사용자가 강한 비교 시안을 요청했으므로 아래 값은 최종 승인값이 아니다.

| 설정 | 작업 시작 값 | 이번 값 |
|---|---:|---:|
| `nexusBreath` 본체 호흡 | .014 | .04 (±4%) |
| `nexusBreathLight` 내부 밝기 | .05 | .15 |
| `nexusChargeExpand` 충전 팽창 | .075 | .24 (약24%) |
| `nexusHitCompress` 피격 압축 | .14 | .32 (약32%) |
| `nexusMagnetSeconds` 반응 길이 | .40초 | .48초 |
| `nexusArcDrift` 작은 독립 움직임 | .012 | .028 |
| `nexusChargeGap` 충전 시 간격 | .04 | .08 |
| 충전 밝기 맥동 gain (NexusMagnetism) | .18 | .55 |
| `nexusDamageFlash` 전 화면 wash | .025 | 0 |
| `nexusDamageChromatic` RGB | .0055 | .018 |
| `nexusDamageSpatialLight` 붉은 파동광 | .35 | 1.15 |
| `nexusDamageWaveStrength` | .014 | .03 |
| `nexusDamageWaveWidth` | .18 | .24 |
| `nexusDamageWaveSpeed / Range` | 3.2 / .90 | 4.2 / 1.25 |
| `nexusDamageCompression` | .85 | 1.10 |
| `nexusDamageSeconds` | .36초 | .48초 |

기존 본체·점선 링·실제 충전된 원호 표시 경로를 재사용한다. 실제 HP·채움량·충돌 반경·점수·공격은 바꾸지 않는다. 새로운 보호막이나 빈 원호도 추가하지 않는다. 표시용 넥서스 시계는 **히트스톱 중에도 반응**하되 실제 Pause/Dev Pause에서는 멈추고, Home/Retry/사망/피버 전환에서 남은 충격을 정리한다. 기존 피버 종료의 링/노드/공간 연출은 유지한다.

## 검사와 증거

작업전1588파일 해시와 사본: `Backups/nexus-hit-save-20261027`, 로그: `Cursor-Dash-master/Logs/nexus-hit-save-20261027`. 기존 imported dirty6/씬 clean을 확인한 뒤 같은 Unity6000.3.23f1 DX11 인스턴스를 사용했다. 사용자 미저장 에셋을 저장·폐기하지 않았다.

정적/순수 로직/실제 Unity·GPU/Play 내부 호출/사용자 손조작은 구분한다. 모바일/WebGL/다른 GPU·성능·Editor 앱 전체 종료/재실행은 별도 검수이며 이번에 빌드하지 않는다.

중간 실패도 보존했다. `native-first.json/xml`은 68/70으로, 재진입 뒤 테스트의 로컬 문자열 유실과 새 붉은 파동의 먼 영역 잔광 단언이 실패했다. 테스트의 비교 자료는 domain reload를 넘는 기존 SessionState로 보존하고 원래 저장·사용자 키 보호 단언을 유지했다. 실제 QA 최고점은 이미 재진입 뒤에도 1이었다. shader의 붉은 빛은 이동 전면에서 멀어지면 0이 되도록 제한해 먼 영역의 잔광을 제거했다. `native-diagnostic.json/xml`은 2/3이며 남은 null/빈문자열 비교 역시 테스트의 임시 변수 수명이 원인이었다. 게임의 저장 키·저장 규칙을 바꾸어 검사를 통과시킨 것이 아니다.

### 이번에 변경한 파일 (이전 미커밋과 구분)

| 파일 | 이번 역할 |
|---|---|
| `Assets/Scripts/Gameplay/StageManager/CursorDashTuning.cs` | 효과 색상 읽기 전용 경로·마지막 fallback 오인 진단 |
| `HitFeedback.cs` (같은 폴더) | 파편 색상 읽기만 분리·일반 피해 wash 0 |
| `CursorDashPointsRecord.cs` | 기존 저장 직후 읽기 확인·마지막 정산 상태 |
| `CursorDashRuntimeUI.cs` | 기존 보고서의 기록 상태·키·저장값 진단 |
| `Assets/Editor/CursorDashNexusReview.cs` | 기존 창에 읽기 전용 저장 상태 표시 |
| `CursorDashFever.cs` | 큰 시각 진폭·히트스톱 중 표시 반응용 시계 |
| `FeverFlowMath.cs` | 충전 밝기 맥동 gain |
| `FeverWorldTransition.cs` | 일반 피해 optical 상한·tint 전달·실행 수 진단, 최종 파괴 별도 보존 |
| `CursorDashVisualProfile.cs` | 기존 Inspector 범위·강한 시안 기본값 |
| `Assets/Resources/CursorDash/VisualPrototype.asset` | 위 Inspector 실제 저장값 |
| `Assets/Resources/CursorDash/FeverImpact.shader` | 실제 공간 변위·강한 RGB·전파하는 국소 붉은 빛 |
| `Assets/Tests/Editor/CursorDashSpaceFlowTests.cs` | 정상 실제 원형 처치/격리 저장·재진입, 실제 양쪽 피해 경로·GPU/원호 검사 |

위 12개 외의 D-055/056 게임 변경은 시작 상태를 유지했다. 이번 인계 문서 및 AGENTS/STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES만 함께 갱신한다. 씬·프리팹·메타·입력/노드·프로젝트 설정·패키지·사용자 프리셋은 이번 수정 대상이 아니다.

## 사용자 수동 확인

1. `Assets/Scenes/Cyberpunk.unity`에서 Play. DEV/훈련/검수 조작 없이 **일반 원형을 실제로 파괴**하고 비치명 피격을 거친 뒤 게임오버까지 진행한다. 확인 클릭 후 BEST, Editor Play 종료·재진입 뒤 BEST를 확인한다.
2. 정상 생존 중 원형 충돌과 육각 레이저를 각각 맞는다. 죽을 때뿐 아니라 두 공격의 비치명 피해에서도 **붉은 빛을 띤 공간 굴절/RGB**가 읽혀야 한다. 화면 전체 빨간 섬광으로 대체되지 않아야 한다.
3. 필요하면 별도 기록 제외 라운드에서 기존 수동 검수 도구로 단발/연속 피해를 비교한다. 본체의 큰 압축·복귀와 충전 팽창, 점선/원호의 반응을 보고 과한 항목을 알려 준다. 이 버튼 사용은 최고점 저장 검사와 구분한다.
4. Pause/Retry/Home/피버 종료·최종 파괴 후 크기·색수차·시간·입력·링이 정상 복귀하는지 확인한다. 손플레이/시각 최종 승인은 사용자 검수 대기다.

육각 타이밍 재검토, 노드 겹침, 새 Flash 그림·별도 사이버 재구축, 설정창 개편은 계속 제외/보류다. 로컬 구현·검사·문서 뒤 사용자 확인을 기다리며 추가 구현/빌드/Git게시를 시작하지 않는다.

## 최종 실행 결과와 종료 상태

- **PASS 실제 Unity6000.3.23f1 / DX11 집중70/70**, 오류0/실패0/skip0. `native-final.json/xml` 완료 2026-10-27 20:33:34 KST, 현재 DLL과 실제 Editor 종료 상태를 교차확인했다. 기존 Native 경고7은 그대로다. 전체 게임 테스트나 실제 손전투의 통과는 아니다.
- **PASS 실제 게임 경로를 내부호출한 Play 검사:** 일반 프레임/실제 원형 ContactAt/점수1/기록 자격 → 원형 비치명 피해 → 정상 정산 → 고유QA키 저장1 → 확인 클릭 후 Main BEST1 → Play OFF/재진입 뒤 같은 키1. 실제 사용자 최고점 키는 보호했고 QA키만 정리했다. 원래 맵 에셋은 복원했으며 파일로 저장하지 않았다. Editor 앱 전체 재시작은 하지 않았다.
- **PASS 양쪽 비치명 피해:** 원형 충돌·육각레이저의 실제 DamageNexusFromCircle/Laser → HP감소1회/광학1회/일반wash0·RGB.018/큰 본체 반응·실제반경불변. 무효0피해/중복·Pause clear, 마지막치명타의 기존 최종파괴 RGB.0055·waveOFF/timeScale1복원도 별도 단언했다.
- **PASS 실제GPU/카메라:** RGB의 실제 채널분리 차이 .0055의7.8588→.018의29.4784, 국소red 합5016.2553/먼영역 평균0/보호획 차이0. 게임 OpticsFrames 증가·input/shape/light 수치와 시간감쇠를 `live-optics.txt`에 기록했다. GameView PNG117장을 남겼다. [실제 표본 갤러리](../../Cursor-Dash-master/Logs/nexus-hit-save-20261027/evidence.html)는 내부호출 표본이며 연속영상/실제손조작/모바일이 아니다.
- **PASS 컴파일/순수:** 참조runtime112/test14/Editor2 오류0·기존134경고; 실제 생산 수학 .NET4,672,999단언. Native와 독립 결과다.
- **PASS 종료 보존:** PlayOFF/compileOFF/timeScale1/Cyberpunkclean·Assets imported dirty6개 전후동일·확인prefs7동일. 작업전1588파일 삭제0, 이 작업의 게임변경12파일 외 시작상태 유지. 씬/프리팹/meta/패키지/설정·노드/입력·참고원본·사용자 미추적 보존. 백업 SHA256 확인. main18b25da/index비어있음, 빌드·커밋·push·배포 없음.
- **NOT_RUN:** 실제 사용자 정상 손전투의 최고점 갱신/Editor 앱 전체 종료 후 지속성·실모바일/터치·WebGL/다른GPU·성능·연속영상·강한 시안의 최종 시각승인. 저장되지 않은 과거 점수의 복구는 하지 않는다.

다음 한 작업은 사용자 Cyberpunk 시각·정상라운드 저장 검수다. 이 문서의 큰 반응값은 최종 승인값이 아니며 검수 결과를 기다린다.
