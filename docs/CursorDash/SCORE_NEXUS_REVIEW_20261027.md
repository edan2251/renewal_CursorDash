# D-056 — 최고점 저장·공유 점수 전환·넥서스 반응·클릭 대기 복귀

상태: **IMPLEMENTED / NATIVE_FOCUSED_67_PASS / 사용자 시각·손조작 검수 대기**. 최신 사용자 6개 피드백 및 메인에서도 BEST 0이라는 답변에 따른 로컬 수정이다. 기준 main `18b25da2a2f1d067a028a1ee855c9c8419fd63c4`와 기존 D-055 미커밋 변경을 보존했다. 빌드·커밋·푸시·배포하지 않았다.

## 확인한 원인과 수정

1. **최고점 저장: 실제 오류를 먼저 재현했다.** 정상 시작 직후 기록 가능이던 라운드가 비치명 넥서스 피격 한 번으로 기록 제외가 됐다. 호출은 `DamageNexusAt → DamageNexusContact → HitFeedback.EmitNexusCommitted/EmitCore → Tuning.Read → RuleSessionChanged → DisqualifyRecord`였다. 피격의 기존 효과 fallback 왜곡 `.008`과 도구 메타데이터 `.006` 차이를 설정 변경으로 취급했다. 효과를 읽기만 했는데 저장 대상에서 빠지는 것이 원인이다. 전환의 표시용 0/countdown은 PlayerPrefs를 쓰지 않는다.
   - `FeedbackNumber`를 통해 **피드백 수치 읽기만** 기록 자격 판정에서 분리했다. 반환하는 효과 수치와 프리셋 적용은 그대로다. 게임 규칙 fallback 검사는 유지하고 명시적 Dev 활성화/설정 변경/가져오기·훈련의 기록 제외도 유지한다.
   - 저장 시 이전 `LastError`가 다음 정산의 신기록 판정에 남지 않게 초기화한다. 점수 키 `CursorDash_BestPoints_v1_<map>`·저장 방식·기록 제외 규칙을 새로 만들거나 바꾸지 않았다.
   - 사용자 기록과 분리한 고유 QA 맵에서 **일반 피격 → 3점 정산/저장 → 중복 Death 방지 → 메인 BEST 3 읽기**와 이후 기록 제외 라운드의 보존을 검증했다. 사용자 실제 BEST를 3이나 가짜 값으로 덮지 않았다. 과거 저장되지 않은 점수는 추정해 복구하지 않는다. 수정 후 정상 기록 대상 라운드를 완료해야 새 기록이 생긴다.

2. **시작 점수 전환:** 기존에는 이동이 전체 시작 연출의 70%(.945초)에 먼저 끝나고, 1.35초의 플레이 전환에서 Auto Size/폰트 처리 방식도 바뀌었다. 반환 중에는 변하는 중간 숫자를 다시 endpoint로 측정하는 경로도 있었다.
   - 같은 TMP/Rect4×4/Scale1/AutoSize1~3.5를 유지한다. 안정된 시작·목표 글자 크기만 측정하고, Auto Size 결과에 정점 보정비를 적용해 하나의 목표 크기로 보간한다. 상태 전환 마지막에 다른 폰트 모드로 재설정하지 않는다.
   - 크기·위치·색은 같은 **1.35초 Cubic 진행**을 사용한다. 점수만 먼저 멈추는 .405초 구간을 없앴다. 표시 숫자 최고점→0은 표시 전용이며 실제 점수는 0이다. 점수 전환에는 스프링/오버슈트를 넣지 않는다.
   - 실제 TMP 메시의 전환 직전/직후 bounds 중심·크기 오차 <.0001 world, 동일 색·0점·Scale1을 0/8/20자리에서 검사했다. 연속 영상의 체감 승인까지 의미하지는 않는다.

3. **득점:** D-055에서 바뀐 감쇠 `.55`/하강 하한 `-.20`/크기 하한 `.955`를 이전 D-053 `.65`/`-.06`/`.98`로 복원하고 추가 청록 재착색을 제거했다. 크기 상한1.22/알파 상한.24는 유지한다. 단독 +1은 즉시, +2 이상은 기존 `.24초` 표시 roll로 실제 총점까지 빠르게 정수 단계를 따라간다. 연속 획득은 현재 표시 목표만 갱신하고 시계를 매번 리셋하지 않는다. 실제 점수·배율·피버 보상은 즉시 기존대로 정산한다. 프레임 간격보다 많은 정수는 모두 한 프레임씩 보일 수 없으며 최종값은 정확히 일치한다.

4. **넥서스 CP3:** 실제 연결은 되어 있었으나 기존 유한 맥동 곡선의 최고값이 약 `.579`여서 Inspector 진폭이 그대로 보이지 않았다. 곡선을 정규화하고 아래 시안값을 적용했다. 본체/점선/실제 충전된 5원호의 기존 표시 경로에 적용하며 체력·수위 채움량·실제 충돌 경계는 바꾸지 않는다. 방향은 실제 접촉점이 있을 때만 사용한다. 피버/사망/중단 우선순위와 원상 복귀는 유지한다.

5. **넥서스 CP4:** 승인된 성공 처치는 `LocalImpactDistortion.AddSuccess + WorldImpact`의 국소 굴절/주변 빛을 쓴다. 피해는 기존 `FeverWorldTransition/FeverImpact`의 실제 게임판 굴절·색수차와 별도 plain 전체 붉은 wash였다. 이를 같은 효과라고 보지 않는다.
   - 성공 경로·파편은 수정하지 않는다. 피해는 실제 접촉점(없으면 실제 넥서스 위치)을 캡처해 한 번의 압축→바깥 전파→해소를 실행한다. 중심 주변을 과도하게 보호해 초기 압력이 가려지는 범위를 좁혔다. 실제 점수 획과 고정 HUD는 계속 보호한다.
   - 넓은 압력장에 동기화된 붉은 **국소광**을 추가하고 전체 붉은 wash는 크게 줄였다. 붉은 선/원형 도형을 그리지 않는다. 가장 강한 변위만 사용하고 최대 .018의 변위/RGB .006 상한 및 사용자 효과·접근성 설정을 유지한다.
   - 최종 파괴의 기존 `.26/.24` flash, .24초의 기존 chromatic envelope와 파편/슬로모션은 분리 보존했다. wave=false 경로는 기존 폭.12/속도2.6/범위.72/감쇠1.4·기존 core guard를 유지한다.

6. **게임 종료 후:** 기존 파괴→같은 최종점수의 메인 위치/크기/색 이동과 회로 재구축은 유지한다. .70초 점수 이동, 총1.50초까지 회로 복원/읽기 뒤 **Returning 상태로 계속 기다린다**. 이 대기에는 적·점수·피버 진행이 없다.
   - 기존 입력을 놓고 새 클릭/탭 한 번을 해야 같은 숫자가 저장된 BEST로 `.45초` 빠르게 변하며 로고/시작 문구가 돌아온다. 최종 점수=BEST면 숫자는 유지한다. 확인 클릭으로 즉시 게임을 시작하지 않고, 입력을 다시 놓은 뒤 다음 새 입력으로 시작한다.
   - 자동으로 BEST/Main으로 넘어가던 D-055의 .70+.80+.45 흐름은 이 결정으로 대체한다. Home/Retry는 대기·확인 상태를 정리해 오래된 반환 동작이 다음 라운드를 덮지 않는다.

## 조절 위치와 전후 값

프로필: `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset` (`CursorDashVisualProfile` Inspector). 기존 설정 체계를 재사용했다.

| 항목 | 이전 | 이번 시안/설명 |
|---|---:|---|
| `spaceStartSeconds` | 1.35초 | 1.35초 유지, 숫자 포즈도 끝까지 같은 진행 |
| `spaceReturnMove / Hold / Reveal` | .70/.80/.45초 | 값 유지. 이동·복원 후 무기한 새 클릭 대기, 클릭 뒤 .45초 |
| `spaceMainScoreScale / Opacity` | .68/.85 | 유지 |
| 점수 탄성 `backgroundScoreReaction / Response / ScaleCap` | .22/.32/1.22 | 유지. 감쇠 수학만 D-053 복원 |
| `backgroundScoreRollSeconds` | .24초 | 유지. +1 즉시 / 큰 획득 표시만 roll |
| `nexusBreath / BreathLight` | .008/.025 | .014/.05 |
| `nexusChargeExpand / HitCompress` | .045/.095 | .075/.14, 정규화된 맥동의 표시 진폭 |
| `nexusMagnetSeconds` | .34초 | .40초 |
| `nexusArcDrift / ChargeGap` | .008/.025 | .012/.04 |
| `nexusDamageFlash` | .12 | .025, 전체 화면 보조 신호 |
| `nexusDamageFlashSeconds` | .10초 | 프로필 .08초. 현 CameraFlash의 최소 .10초가 적용됨 |
| `nexusDamageSeconds` | .30초 | .36초, 국소 굴절/빛/색수차의 공통 시간 |
| `nexusDamageSpatialLight` | 없음 | .35, 새로운 **피해 전용** 국소 붉은광 |
| `nexusDamageWaveStrength / Chromatic` | .014/.0055 | 유지. 상한도 유지 |
| `nexusDamageWaveWidth / Speed / Range / ReleasePower` | .12/2.6/.72/1.4 | .18/3.2/.90/.85, 보통 피해 전용 |

회로 기본값·흐름·점수 디자인, 일반 노드/육각 타이밍과 공격, 피버 공급/시간/판정·승인 파편·커서는 이번에 수정하지 않았다. 일반 노드 겹침·새 Flash 그림·별도 사이버 재구축·설정창 개편은 계속 보류다.

## 변경 파일과 보존

이번 시작본 대비 기존 게임/검증 파일13개:

- `Assets/Scripts/Gameplay/StageManager/`: `CursorDashPointsRecord.cs`, `CursorDashTuning.cs`, `CursorDashSpaceFlow.cs`, `CursorDashStartTimeline.cs`, `FeverScoreHud.cs`, `FeverFlowMath.cs`, `CursorDashVisualProfile.cs`, `FeverWorldTransition.cs`, `HitFeedback.cs`, `StageManager.cs`.
- `Assets/Resources/CursorDash/`: `VisualPrototype.asset`, `FeverImpact.shader`.
- `Assets/Tests/Editor/CursorDashSpaceFlowTests.cs`.

상대 경로는 Unity 프로젝트 기준이다. 관련 문서와 AGENTS 최신 안내도 갱신했다. 씬·프리팹·메타·패키지/프로젝트 설정 및 기존 사용자 미추적 파일은 수정·삭제하지 않았다. 작업 전 소스·설정·씬·문서 사본은 **`D:/renewal_CursorDash/Backups/score-nexus-review-20261027`**, 기준1587파일 해시는 `Logs/score-nexus-review-20261027/before-hashes.json`이다. 새 로그/사본은 Assets 밖에 있다.

## 실제 검사와 미검증

- **FAIL 재현 후 수정:** `OrdinaryConfirmedDamageKeepsNormalRecordEligibility` 이전 소스로 0/1. 원인 호출 스택을 `reproduced-record-failure.json/xml`에 남겼다. 테스트 실패를 제외하거나 핵심 단언을 줄이지 않았다.
- **PASS 실제 Unity6000.3.23f1 / DX11:** 최종 C# 컴파일 오류0, 기존 경고7. 정상 메뉴의 TestRunner **집중67/67, 실패0/skip0**, JSON/XML 완료 UTC **2026-10-27T10:34:10.4430169Z**(19:34:10 KST). 정확한 전체 시간 문자열은 `native-final.json`을 따른다. 전체 프로젝트 테스트나 모바일 합격이 아니다.
- **PASS 주요 범위:** 정상 비치명 피격의 기록 자격/명시Dev의 제외 유지, 고유QA맵 저장·메인읽기·중복정산·제외 기록 보호, +1 즉시/+4 정수 단계·연속 목표 갱신, 0/8/20자리 TMP handoff geometry·color, 표시용 본체 charge>1.05/hit<.92·경계/HP 불변·복귀, 클릭 대기/중복 확인 방지/Home·Retry, 기존 되감기·피버 종료/Pause·회로/수명 검사.
- **PASS 실제 GPU:** 텍스처 픽셀 굴절·획 보호. 피해 국소광 유무 비교에서 붉은 채널 증가 합477.408, 먼 화면 평균 변화0, 보호 획 RGB 변화0 (`nexus-spatial-gpu.txt`). 합산 수치는 강도 체감 점수가 아니다.
- **PASS 참조 컴파일/순수 로직:** 런타임112/테스트14/보조Editor2 오류0, 기존 참조 컴파일 경고134. 기존 생산 수학 회귀 .NET4,672,999 단언. 정적·순수 검사와 실제 Unity/GPU/Play 내부 호출을 구분한다.
- **실제 화면:** 이번 실행의 GameView1080×1920 PNG93장. [캡처 검수 자료](../../Cursor-Dash-master/Logs/score-nexus-review-20261027/evidence.html). 시간 간격 캡처이며 연속 촬영 영상/실제 터치가 아니다. `flow-no-red-pressure` 파일은 전체 붉은 wash만 뺀 표본으로, 이번 국소 붉은광까지 제거한 것은 아니다. DEV·기록 제외 표본/0은 사용자 실제 정상 플레이 기록이 아니다.
- **PASS 보존/종료:** 최종 PlayOFF/compileOFF/timeScale1/Cyberpunk dirty=false. Assets의 imported dirty6와 확인 prefs7 전후 동일. 저장·폐기·재임포트 없음. 테스트의 실제 점수 키 전후 보호 단언도 통과했다. 시작 파일 삭제0. 다른 Unity 프로젝트를 종료하지 않았다.
- **NOT_RUN:** 실제 손 전투/터치·전체 전투, 실제 사용자 최고점의 앱 종료·재실행 유지, 모바일/WebGL·다른 GPU/성능, 최종 사용자 시각 승인, 연속 검수 영상. 같은 프로세스의 격리 키 저장·읽기 통과를 재실행 검증으로 부풀리지 않는다.

## 사용자 수동 순서와 다음 한 작업

1. `Assets/Scenes/Cyberpunk.unity`를 열고 Play. 훈련/DEV를 켜지 않은 정상 라운드에서 득점·비치명 피격·체력0을 거친다. 최종점수 이동/회로 복원 후 가만히 두어 유지되는지 확인한다.
2. 화면을 새로 한 번 클릭: 로고/안내 복귀 및 같은 숫자의 BEST 전환. 신기록이면 숫자가 그대로인지 확인한다. 입력을 놓고 새 클릭으로 다시 시작해 BEST가 표시용0으로 변하는지 본다. 실제 종료·재실행 뒤 기록 유지는 별도 확인한다.
3. 메인→플레이 숫자의 **끝부분**과 +1/×2~4 득점 반응을 비교한다. 중앙 호흡·한 번의 충전 팽창·접촉 방향 압축/복귀, 게이지 표시 역감소/새 충전을 본다.
4. 일반 처치의 승인된 흰 반응과 넥서스 보통 피격의 접촉점 중심 굴절·붉은 국소광/RGB를 비교한다. 마지막 파괴와 피버/회로·Pause·Retry·Home 뒤 잔류가 없는지 확인한다.

다음 한 작업은 **사용자 Unity 시각·손조작 검수**다. 현재 결과를 최종 시각 승인으로 간주하지 않는다. 후속 우선순위는 (1) 실제 사용자 기록의 앱 재실행 유지, (2) 모바일 크기에서 새 CP3/4 체감·가독성, (3) 극단적으로 긴 BEST의 확인 roll 중 글자 맞춤이다. 첫째와 둘째는 미검증 위험, 셋째는 추가 화면비/긴 자리수 검수이며 자동으로 추가 구현하지 않는다.
