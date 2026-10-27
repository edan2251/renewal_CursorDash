# D-055 — 연속된 메인·점수·넥서스·피버·복귀

상태: **IMPLEMENTED / CP6 native 집중62 PASS / 사용자 시각·입력 검수 대기**. 최신 사용자 CP0~CP6 로컬 구현·Unity 컴파일/가능한 Play 검수 요청. 빌드·커밋·push·배포 금지. 기준 main `18b25da2a2f1d067a028a1ee855c9c8419fd63c4`, 원본 `D:/renewal_CursorDash/Cursor-Dash-master`, 참고 원본 `D:/Gamejam2026/Gamejam2026_wave`. 공통 프로필/독립 QA/Slack 경로 미설정·외부전달 없음.

## CP6 최종 결과 — 2026-10-27 18:23:51 KST

**PASS 정상 집중62/62(0FAIL/0skip), 실제Unity컴파일오류0/DX11·GPU/Play·최종GameView65 PNG.** 0·8·20자리중심/안전폭·같은숫자/기록보호·내부득점과피해·되감기/재충전·Pause·피버진입/종료·최종파괴→이번점수→BEST/Main·중단/Retry/Home·격리QA맵정상신기록/제외·중복정산·ring수명검사를실행했다. 실제shader픽셀변위/획보호도통과. 전체공식suite/실모바일/터치/성능통과가아니다. 원본씬·사용자데이터/참고원본보존·최종PlayOFF/timeScale1/씬clean/prefs동일·미커밋이다.

이번CP6의소스보완은6파일: **FeverFlowMath.cs/legacy표시helper의FeverState.cs**(끝값정확포화), **FeverNexusRig.cs/CursorDashFever.cs/NexusRingRhythm.cs**(종료중legacy회전재활성화방지), **CursorDashSpaceFlowTests.cs**(실중심/격리정산/owner수명검사). 공유Rotator/육각·입력파일은바꾸지않았다. 실패이력도아래에보존한다.

[실제 화면 갤러리](../../Cursor-Dash-master/Logs/continuous-flow-20261027/evidence.html) / [정상 TestRunner JSON](../../Cursor-Dash-master/Logs/continuous-flow-20261027/native-final.json) / [XML](../../Cursor-Dash-master/Logs/continuous-flow-20261027/native-final.xml). 이미지65장은실제UnityGameView1080×1920이며**연속영상이아니다**. 시작20자리BEST는표시메모리만, 득점/피해/fever는내부함수, record는고유QA맵/키로격리했다. 실제UI훈련클릭은성공/도구drag는변위확인못해이동·입력해제통과로표시하지않는다.

남은검수: 사용자손/모바일입력·전체전투·실제앱재실행저장·WebGL/성능, 점수크기/위치·득점탄성·자성/굴절·ambient체감. 새기능/빌드/Git게시는시작하지않고사용자검수를기다린다. 아래BLOCKED/NOT_RUN은**CP6재개전또는각회차실행당시의이력**이며현재차단으로재적용하지않는다.

## CP0 실제 확인

- 현재 CursorDash: Cyberpunk 한 실행 씬의 Main→Starting→Playing/Paused→Ended. Death에서 기존 생존기록과 별도 BigInteger 점수 기록을 저장하고 .64초 finale 뒤 기존 결과 UI를 표시한다. Home은 기존 같은씬 Space.Home/PrepareSpace로 돌아간다. 새 점수 관리자/저장 체계를 추가하지 않는다.
- 참고 시작 씬: enabled **Assets/Scenes/WaveGame.unity** 하나. Unity **6000.5.7f1**, 대상은6000.3.23f1. 참고 원본 실행/업그레이드/파일 수정은 하지 않았다.
- 실제 연결: WaveGame의 UIManager→TweenController, scoreRect/scoreTxt는 같은 TMP UGUI. 별도 UI Canvas(Overlay sorting2)의 scoreRect는 scale1, center anchors, anchoredY **-180**, Rect600×450, AutoSize18~300/font300. 전투 grid/ShakeTarget은 카메라 Canvas이며 orthographic camera size5다. CursorDash 점수는 world TMP/실제 glyph mask 경로이므로 UI pixel 좌표/URP Volume을 통째로 복사하지 않는다.
- 참고 씬 저장값: transitionDuration **1초**, scoreTargetPosY **530**(코드 기본400과 다름), gridInitialScale **.8**/alpha **.7**(기본.7/.5와 다름), titleTargetScale .5, blink .7초/.2alpha. score는 -180→530을 **.7초 InOutCubic**, countdown **.35초 OutQuad**, title/최고보조 fade .5초와 겹친다. 돌아올 때 scoreInitialY로 .7초, 로고/보조1초·alpha.5초/격자.9초가 겹친다.
- 실제 득점 호출: UIManager.AddScore에서 실제currentScore 즉시 증가/최고기록 갱신 후 UpdateScoreUI. 표시 roll은 gained*.15를 .2~.6초로 제한/OutQuad. gained1~5로 punch.35~.95/.25~.45초/vibrato4~8, color 강조 뒤 .55~.75초 복원. OnGameStart/이후득점/Return에서 코드가 상태를 다시 쓴다. 이 값은 호출 코드/저장값이며 영상만으로 산출한 공식이 아니다.
- 목표 대응: CursorDash 시작 숫자는 **무반동** Cubic 이동·단시간 표시 countdown, 실제 득점은 제한된 하나의 확대→작은undershoot→안착·BigInteger 표시 roll. 참고의 여러 진동/붉은 득점색/아트·게임 규칙/float점수 저장은 복제하지 않는다.
- 실제 영상: 요청의 형제 CursorDash_incoming 폴더 대신 **D:/renewal_CursorDash/_incoming/references/90_current_mobile.mp4**(3,887,703bytes)와 **91_reference_wave_transition_score.mp4**(4,069,411bytes)에 있다. 기존 Windows Media Player로 직접 확인했다. 모바일은 일반 전투/큰 배경점수/흰 피버·검은 노드/궤적 표본, 참고는0초 전투00/20초11/27초22/32초 로고·최고22 복귀를 관찰했다. 참고 clip은 처음부터전투라 초기 Main→start 전체를 본 것으로 주장하지 않는다. 전체 프레임 분석/오디오 검수도 아니다. 추가 관찰과 수정 후 검수는 아래에 구분해 갱신한다.
- 대상 Editor 읽기진단: 정확한 대상 프로젝트/Cyberpunk/비Play·비컴파일·씬dirty=false. TMP Shader3와LiberationSans FontTexture1만dirty(기존D-014 4대상), 저장/폐기없음. 시작1884파일/Before·현재Git상태는 `Cursor-Dash-master/Logs/continuous-flow-20261027`, 참고 C#/WaveGame·설정/manifest 해시는 reference-before.json에 보존한다. 기존 미추적/Recovery/Archive/영상은 보존한다.

## 구현 대응과 초기 시안 선택

1. 기존 SpaceFlow/StartTimeline: 실제 glyph 중심을 정렬하고 메인 .82→.68시각크기·화면중앙으로 정돈한다. 게임 점수는 기존 .50에서 .58viewport Y로 시안 이동해 중앙→인게임 이동이 읽히게 한다(참고530px를 그대로 이식하지 않음). 같은 TMP/Rect4×4/Scale1/Auto1~3.5·전환font잠금·무반동 이동을 유지한다. 전체시작1.35초/링·본체·실제커서 흐름·입력해제가드는 재사용한다.
2. 기존 background score/pulse 수학: 모델 총점은 즉시, 표시는 BigInteger 별도 roll·현재목표로 따라간다. 기존1.22크기/.24alpha상한 내에서 한 번 undershoot를 추가하고 정상색/반응색을 청록계열로 제한한다. 실제 TMP 메시에서 회로/flow 가림을 계속 계산한다.
3. 기존 Fever/Nexus 표시 소유자: 작은 호흡/밝기·charge/damage 반응을 합성하는 **표시용 수학**만 확장한다. 실제HP/Gauge/경계는 불변. 기존5 gauge arc/안쪽점선의 역할을 구분하고 phase전환·finale가 우선한다. 승인된 fever enter/exit 곡선은 유지한다.
4. 기존 확정HP피해 경로: 짧은 보조 redwash와 공간압축/외향release·색수차를 분리 조정한다. 실제 glyph/HUD 보호가 반응을 지우는지 실제 GPU/Play의 red제외 검수로 확인한다. 성공 타격/파편/최종파괴는 보존한다. ambient flow는 기본선이 아니라 이동강조만 조정한다.
5. Death의 실제 정산/저장 1회를 유지하고 같은 SpaceFlow의 Returning으로 이어진다. 기존.64 finale→같은숫자 최종점수 이동/읽기→실제best/조건부신기록→Main입력해제후 새 입력. 결과 UI 자산은 삭제하지 않고 이 공유씬 경로에서 표시하지 않는다. legacy다른씬은 필요범위 밖에서 전면 변경하지 않는다.

육각 타이밍/일반노드·전투/반사/공급·점수/배율/피버 규칙, 승인 파편·기존피버종료는 유지한다. 일반겹침·설정/상단메뉴 재설계·새Flash그림·별도사이버재구축은 범위 밖이다.

## 진행·검사

CP1~CP6 구현/실제 실행 결과·조절값·변경파일·미검증·검수 순서는 완료 후 갱신한다. 이전 native30/공개검증을 이번 수정본의 통과로 재사용하지 않는다. 정적·순수·참조 컴파일·실제 Unity/GPU·내부 호출·손 조작을 구분한다. 실제 저장값은 초기화/가짜 값으로 덮어쓰지 않는다.

## CP1~5 구현값 (최종 native 검수는 위 참조)

- 같은 TMP의 실제 glyph bounds 중심을 맞춘다. 시작 travel InOutCubic(전체의 .70), 표시 countdown OutQuad(.35); 1.35초 중 .945초 이동/.4725초 countdown. 메인 scale .68/정중앙, 인게임 viewport(.5,.58), Rect4×4/Transform1/AutoSize1~3.5 유지. 긴 숫자 font잠금 후 0 도달 뒤 smooth복원. Nexus/cursor 기존 탄성은 유지한다.
- 실제 총점 즉시 정산/별도 BigInteger display roll .24초; 진행 중 목표 갱신은 기존 roll시계를 재시작하지 않는다. 기존 단일 spring 감쇠 .65→.55/음의 바닥 -.06→-.20으로 작은 undershoot, 시각 scale .955~1.22/alpha≤.24, 청록 강조. 사망 즉시 실제 최종 숫자 snap. glyph가림은 같은 최종 TMP메시를 사용한다.
- 기존 Fever 소유 표시 수학 NexusMagnetism: 호흡 .008/3.2초·밝기 .025, charge .045/반응 .34초, inner 지연 .025/outer .055초, damage .095압축/실제contact면 우선·이웃감쇠/정보없으면중앙, arc drift .008·charge gap .025. Normal/Closing에만 적용, 전환/죽음이 우선/재설정마다 clear. 체력/fill/실경계 불변.
- 피해red .26→.12/.10초(기존optics.24→.30초와 분리), 기존 texture-displacement wave .008→.014(상한.018)/width.12·RGB.0055/접근성 유지. 성공/최종파괴 효과 경로와 파편은 수정하지 않았다.
- 기본회로 두께·색·밀도 불변. ambient speed .24→.28/interval2.6→2.2/strength.10→.12/tail.20→.28/head.025→.032, 동시2·hard3/가지감쇠/획가림/퇴장clip유지.
- Death 정산·기록 저장 한 번 유지→기존 .64초 finale→Returning(.70초 숫자이동 + .80초 최종 읽기 + .45초 BEST/로고 복원)=1.95초→Main. 동일숫자/이번점수·기록제외·실제saved newbest의 1회 보조강조, 별도ROUNDOVER UI 없음(원본legacy자산보존). Returning은 비전투/unscaled, 커서숨김·잔상/적/공격/FX정리 후 회로복원, Home에서 inputrelease gate/이전routine취소.
- 조절위치: Assets/Resources/CursorDash/VisualPrototype.asset(기존 Inspector 프로필), 필드의 space*/backgroundScore*/nexus*/circuitAmbient*. 점수·저장·피버·배율·육각 타이밍은 기존 모델/구조를 유지한다.

## CP6 재개 전 검사 이력

- PASS: 최종소스 순수.NET 4,672,999단언(새 표시 roll/시작무반동/득점탄성 상한·undershoot/자성 baseline복귀 포함 + 기존회로/모델). 실제 Unity/GPU/Physics/터치 증거가 아니다.
- PASS: 참조 컴파일 runtime112/test14/Editor2 오류0. 실제 Unity Library DLL 2026-10-27 16:43:42 생성과 Editor.log reload/compile 완료도 확인했으나 후속 테스트 수정 뒤 최종 native 상태는 다시 확인해야 한다.
- 실시간 UI제약: 대상 창 식별658488과 다르게 screenshot은 열린 참고프로젝트+MediaPlayer를 반환했고 activate_window timeout. 다른 프로젝트를 조작하지 않았다. 사용자에게 대상 CursorDash만 앞으로 가져오기를 요청함. 해당 입력 답변 전까지 Unity 메뉴/Play 실행은 진행하지 않는다.
- 새 tests는 정적컴파일만이며 공식 실행은 아직NOT_RUN. 기존 native30 PASS를 이번 결과로 사용하지 않는다. 실제 모양/모바일/WebGL성능/손조작/정상 기록의 재실행 지속성도 아직NOT_RUN.

## CP별 인계와 수동 검수

| 단계 | 로컬 결과 | 남은 확인 |
|---|---|---|
| CP0 | 소스·씬/저장값·참고 영상 표본/백업·해시 확인 | 참고 전체 시작 영상/오디오·실행은 미확인, 참고Editor를 Codex가 실행하지 않음 |
| CP1 | 같은 TMP/실제 glyph 중심·메인 작게/무반동 Cubic 이동·표시 countdown/기존 등장·release guard | 0·한자리·장문 best의 실제 화면/터치·홀드 |
| CP2 | 실제 즉시 점수와 BigInteger 표시 roll 분리/기존 단일 spring에 작은 undershoot/상한·청록 강조/사망 exact snap | 연속 득점의 실제 리듬·GPU glyph가림 |
| CP3 | 표시에만 호흡·charge·방향성 hit·자성 arc 합성/phase·death·Pause가 우선 | 실제 화면·눈에 읽히는 강도·연속 충전/피격/fever 복원 |
| CP4 | 일반 damage red 낮고 짧게/wave 확대·기존 actual center/RGB/HUD guard·국소 ambient head-tail | red를 잠시 제외한 실제 GPU/Play와 모바일 가독성 |
| CP5 | finale 뒤 같은 score final→best/main, 정산·기록경로 한 번/복귀 중 비전투·unscaled/inputrelease/legacy UI 자산 보존 | 정상/제외/신기록·중단 경로의 native Play·실저장 지속성 |
| CP6 | 최종 native 집중62/62·실GPU·Play통합10·실화면65장·전후보존 확인 | 사용자손/모바일/전체전투·실제앱재실행·WebGL/성능 |

- 최종 일반피격 red .12/.10초와 **기존 finale red .26/.24초**를 분리했다. 최종파괴 리듬/방향/파편/slow/성공 effect 코드는 그대로다.
- 자성 pulse는 Pause·panel/dev pause 때 clear해 재개 뒤 오래된 charge/hit가 재생되지 않게 한다. OnFlash/user.flash가 0이면 새 core 밝기 반응도0이다.
- 공식 사례는 기존 Editor-only 반사 경계와 PlayerPrefs/Map 메모리 보호 fixture를 재사용한다. 검수용 긴 best는 `Space`의 표시 메모리만 바꾸고 실제 기록키를 가짜값으로 덮지 않는다. 결과창 기대값만 확정된 Returning으로 변경/기존 단언을 제거하지 않으며, spring 바닥은 요청된 undershoot에 맞춰 .98→.955로 갱신하고 상한·알파·Pause·fit 단언을 보존했다. 전체 suite 실행으로 주장하지 않는다.
- 대상 Unity UI가 정상화되면 `Tools → CursorDash → Run Shared Space Review`가 현재 집중 검사 진입점이다. 검사는 Play를 직접 들어갔다 나가므로 사용자 Play 세션과 함께 실행하지 않는다. 씬dirty이면 자동중단/저장폐기없음.

수동 순서(최종 실행 씬 **Assets/Scenes/Cyberpunk.unity**):
1. 비Play에서 수정 스크립트 import/Console 확인→Play→Main 3~5초 대기: 작은 중앙BEST/국소 circuit head+tail. 실제 최고점 0/여러자리 확인은 사용자의 기존 값이나 테스트 표시 메모리를 사용하고 PlayerPrefs를 조작하지 않는다.
2. 시작 누름/연타/홀드 후 손떼기: 같은 숫자가 .4725초까지0/ .945초까지 이동·변색, 점수만 무반동/링→body→실cursor는 기존 탄성, 준비·release 후 새 조작/생존시계 시작.
3. 훈련/Dev(기록 제외)에서 일반 모델 처치 +1/연속으로 countup·한 번의 탄성/자성 반응. 실제 점수와 표시를 구분하고 .24초 뒤 일치 여부 확인. 숫자 획 뒤 circuit/highlight 가림도 확인.
4. `Tools → CursorDash → 넥서스 수동 검수` 기존 훈련 무적 해제/단발·연속 피해: 짧은 red·중심 굴절/외향release·압축/원호·실충전0과 .16초 표시 역감소. 감소 중 새 charge/추가hit 후 값이 되살아나지 않아야 한다. red제외 검수는 런타임 DamageImage 알파만 잠시0으로 하며 프로필/설정을 저장하지 않는다.
5. 만충/피버 진입→연속 획득→0게이지→승인된 ring/node/space 종료·정상 색/자성 baseline. 모델/공급/배율은 기존 규칙 유지.
6. 체력0: 기존 .64초 파괴/slow→.70초 같은 숫자가 중앙복귀→.80초 final 읽기→.45초 BEST/로고/안내. 결과창·cursor잔류가 없어야 한다. 기록제외는신기록표시/저장없음. 정상 기록과 실제 신기록의 저장·재실행은 사용자 실제플레이에서 확인(기존데이터 초기화금지).
7. 시작 홀드/연타, Pause·panel·Retry·Home, fever exit/파괴/Returning 도중 Home·Retry 및 효과OFF/낮은강도. 오래된 callback/색·왜곡·pulse·cursor·pool 잔류와 중복 manager가 없어야 한다.

## 자체 QA 후속 (새 구현하지 않음)

1. **P1 / 남은 입력·저장 검수 위험:** native focused Play/GPU·격리QA맵정산은최종통과했다. 실제사용자손/모바일입력·전체전투·실제앱재실행지속성은미검수다. 도구drag의변위도확인못해입력버그라고확정하거나PASS로확대하지않는다.
2. **P2 / 디자인 검수:** reference overlay px→CursorDash worldTMP로 이식한 main.68/gameY.58 및 return1.95초는 임시 선택이다. 작은 모바일에서 score와Nexus간 거리·final읽기 여유를 사용자 영상으로 판단해야 한다.
3. **P2 / 시각 검수:** ambient .12/head-tail 확대와 wave.014는 이벤트/기본선 상한을 지키지만, 모바일 디스플레이와 실제 shader 합성에서 읽힘/멀미성 여부는 미측정이다. red제외와 effects낮춤 비교가 필요하다.

## 변경 파일

기존 게임·테스트 파일19개: VisualPrototype.asset, StageManager.cs, CursorDashSpaceFlow.cs, CursorDashStartTimeline.cs, CursorDashVisualProfile.cs, CursorDashFever.cs, FeverScoreHud.cs, FeverFlowMath.cs, FeverGaugeView.cs, NexusGameboardPresentation.cs, FeverWorldTransition.cs, HitFeedback.cs, CursorDashCircuits.cs, CursorDashSpaceFlowTests.cs, CursorDashSceneRegression.cs, CursorDashFeverRulesTests.cs, FeverState.cs, FeverNexusRig.cs, NexusRingRhythm.cs. 기존 Assets/Resources 및 Scripts/Gameplay/StageManager, Tests/Editor 안에서만 수정한다. 새 MonoBehaviour/manager/씬/프리팹·패키지·셰이더를 추가하지 않았다.

문서는 AGENTS/STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES와 이 인계서에 최신결정을 기록한다. 삭제·이동·index변경·Git작업 없음. Unity프로젝트 밖 복구본은 `D:/renewal_CursorDash/Backups/continuous-flow-20261027`에 Before/기준manifest·ProjectSettings5개로 보존했고 기존 복구본도 유지한다. 실제 소스수정 후 GameView 캡처/연속녹화는 아직 생성하지 못했다.

최종 보존 확인: 시작1884파일 삭제0·참고보호파일 변경0·씬/프리팹/meta·노드/입력/규칙 파일 변경0, HEAD18b25da/main·index비어있음. 새 인계서와 허용된 소스/프로필/테스트/문서만 미커밋이다. 최종 reference112/tests14/Editor2 오류0·기존runtime경고134/테스트경고0, git diff --check 오류없음(LF→CRLF 안내만). 수정 후 native 공식실행·GPU·Play·새 캡처/영상은 도구 문제로 미완료이며, 이번에 Play를 시작하지 않았다. 대상 창 복구 요청은 대기 중이고 같은 활성화 오류를 반복하거나 다른 경로로 Play를 실행하지 않았다. 다음 한 작업은 대상 CursorDash 창에서 새 focused 정상 TestRunner/시각·입력 검수다. 사용자 검수 전 새 기능·추가 단계를 시작하지 않는다.

## CP6 재개 — 2026-10-27 (실행 중 기록)

사용자가 대상 Cyberpunk 창을 앞으로 가져온 뒤 기존6000.3.23f1/DX11 창만 읽고 조작했다. native 첫진단09:07:38UTC: PlayOFF/compileOFF/timeScale1/Cyberpunkdirty=false. Assets dirty6(TMP shader3/Liberation FontTexture/이사만루 Medium·Bold FontTexture), Library 기본scriptdirty는 별도. 저장/폐기/재임포트없음. 기존 동일한 imported 상태를 보존하며 씬clean guard/기존prefs 복원fixture의 정상 TestRunner 메뉴만 실행했다.

첫 공식집중59개: **57PASS/2FAIL/0skip**, native-first.json/xml 보존. Scene통합7개는 전부PASS/실제Play와GameView PNG 생성(최종개수는아래기록). 보고서 aggregate duration2.18초는 domainreload를 포함한 전체벽시계 시간이 아니므로 실행시간으로 인용하지 않는다(각Scene사례는7~16초).

실패는 기존순수helper 끝값2개: EntrySpread(1) 내부 .34나눗셈이1 바로아래가 되어 exponential Out이 .998046875, DrainHead(1)1.0000000000000002. 검사단언은 그대로 두고 Out의1e-12끝값포화/DrainHead1정확반환+정규화만 보완. FeverState의 모델규칙/시간/점수는 변경하지 않고 legacy 표시helper만 수정했다. 새회귀로0/8/20자리 실제glyph중심/안전폭, 실제사용자기록과분리한 일회성Map복사본·고유QA키의 정상정산/신기록/중복death/제외정산 사례를 추가했다. QA키는 생성전없음확인/생성한키만finally정리, 실제Cyberpunk/Flame최고기록에가짜best쓰기없음.

보완후순수.NET4,672,999단언/참조112·test14·Editor2 오류0. 정상Assets/Refresh(강제ReimportAll아님)로native최종컴파일한뒤새집중61개를실행할예정이다. 이아래최종기록으로갱신할때까지61PASS로기록하지않는다.

두번째공식61개:60PASS/1FAIL/0skip(native-second.json/xml). 두수학끝값·신규0/8/20자리중심/고유QA키정산·신기록/제외·중복death는통과. ExplicitTraining...의TearDown에서씬닫을때[DOTween] 재생성오류를관찰했다. 실제로그는누수오브젝트명만주므로특정OnDestroy호출스택이있다고주장하지않는다. 코드에서FeverNexusRig.Release/외곽NexusRingRhythm.OnDisable이원본Rotator.enabled를켜고 OnEnable→DOTween.Sequence로들어갈수있는경로를확인했다.

최소보완: Fever비활성화는rig.Suspend로소유권·기준값을보관하고LegacyRotator를켜지않음/OnDestroy는Release(false),외곽정지링도OnDisable시정지유지. 공유Rotator·육각타이밍파일은미수정. 새회귀로owner disable/re-enable/Home뒤두원본Rotator가꺼지고점선제어가다시연결되는지확인한다. 최종집중62개재실행예정; 경고숨김/LogAssert.Expect/Ignore/globalKillAll은사용하지않는다.

## CP6 최종 보존·한계

- 완료기록은native-final JSON/XML09:23:51UTC/실제Editor상태·생산소스와새DLL해시로교차확인. 클래스별Circuit15/FeverRules23/SpaceFlow14/SceneRegression10=62. reports의2.16초aggregate duration은domainreload벽시계시간이아니므로전체실행시간으로인용하지않는다.
- 종료imported dirty6와조회prefs7키는before/after동일. 점수키는각통합사례+보호fixture로유지/가짜값쓰지않고QA고유키만finally정리. imported JSON목록동일이메모리전체직렬화보존증명이라는뜻은아니다. 실제저장/폐기하지않았다.
- 65PNG헤더/크기(1080×1920)/해시를native-captures.json에기록. 갤러리는실제PNG를연결한정적표본이며가상녹화/GIF/모바일영상으로표시하지않는다. 실제훈련클릭·GameView확대도구입력은별도제한검수, 드래그이동/입력해제는변위확인못해NOT_RUN/미확인이다.
- 직접시작한Play는끝냈고기존분할뷰복귀/참고프로젝트조작·종료없음. Assets/Refresh만사용·Install/전역/새연결/빌드/commit/push/deploy없음. 사용자검수다음단계자동진행없음.

CP6 재개 전의 미커밋 소스6개는 `D:/renewal_CursorDash/Backups/continuous-flow-cp6-20261027`에도 별도 보관했다. 각 바이트는 cp6-before.json의SHA256과 일치해 검수 보완만 따로 비교·복구할 수 있다. 활성소스는 되돌리지 않았고 기존 CP0복구본도 유지한다. 최종 JSON/XML62case·실제 DLL/생산소스 해시·편집 종료상태를 확인했다.
