# 노드 색·파편 크기·전체 화면 출렁임 후속 (D-019)

상태: 2026-10-25 구현·회귀·로컬 WebGL 검증 완료, 이번 작업 종료. 미적 승인/실기기 검수는 사용자 대기이며 기존 공개 CP14나8879 빌드를 자동 교체하지 않는다.

## 원인과 수정

- 이전 실제 WebGL 기본 세션에서 원형 접촉 후 `fx.circleRemove.size=1`을 내보냈다. 의도한.05의20배다. HitFeedback가 실루엣 배열의 존재를 “크기1의 실루엣 파편”으로 해석했는데, 원형도 실루엣을 전달하도록 바꾼 뒤 이 기본값 분기가 남아 있었다. `BeforeWeb/default-after-contact-full.json`과 실제 `circle-*.png`가 재현 근거다. 파일 손상이나 Unity의 한계로 판정하지 않는다.
- 성공 효과의 숫자 기본값을 수치 도구 메타데이터에서 읽게 해 일반 게임·패널·보고서가 같은 값을 소비한다. 원형 조각의 기본 크기 인자는 본체 반경의24%로 제한하고, 외곽선 조각도 얇게 제한한다. 이전 크기1 저장값은 지우지 않고 물리적 조각 크기를 제한한다. 조각은 이동하며 작아진다.
- 파편 전용 단색 셰이더에 실제 다각형 외곽 변까지의 거리를 넣어 경계 AA를 적용한다. 내부 삼각 분할선은 경계로 취급하지 않는다. 기존 넥서스/방어 파편 경로는 유지한다.
- 육각의 충전/변형에서 고정 분홍색을 제거하고 본체 노란색을 유지한다. 기본 발사/반환 빔도 노란색이다. 명시적으로 저장된 Dev 색은 보존한다. 원형·육각의 독립 중앙점 기본값은0이며 셰이더도0에서 점을 그리지 않는다. 이전 중앙점 key는 가져오기 호환/명시적 조절용으로 유지한다.

## 왜곡과 밝기

프로젝트는 Built-in 렌더링이며 URP 패키지/Volume이 없다. 제공한 GameFeelManager의 `Volume`/`LensDistortion` 클래스는 그대로 붙일 수 없고, Volume.weight만으로 최종 왜곡량을 알 수도 없다(프로파일 설정에 의존). 패키지 설치/파이프라인 전환 없이 기존 Unity 카메라 OnRenderImage 후처리를 확장했다.
타격점의 월드→뷰포트 중심, 즉시 렌즈 킥→OutCirc 계열 복귀·약한 역방향 반동, 화면으로 전파되는 실제 픽셀 파동, 월드 밝기/선 강조를 사용한다. 이전 성공 왜곡은0에서 시작하는 시간 사인 파형이고 밝기 응답이 없어 빠른 타격의 첫 인상이 약했다. 합산 이동량/슬롯 상한과 가장자리 복귀는 유지한다.
기존 회로선은 HUD 장식으로 생성되므로 HUD와 함께 안정된 후단에 남으면 배경 왜곡과 분리될 수 있다. `WorldCircuitBackdrop`은 장식 회로선만 월드 렌더에 연결한다. 기존 Circuit 이미지가 있으면 모양/색을 옮겨 중복 렌더를 막고 종료 시 enabled 상태를 복원한다. 해당 자식이 없는 씬은 기존 Setup의 동일60구간/1080×1920/색·4px두께로 런타임 메쉬를 만든다. Setup 재실행/씬 재생성/배경 비트맵 변경은 하지 않는다. 텍스트/버튼/넥서스 UI는 기존 카메라에 남긴다.
`Time.timeScale=1` 강제 복귀나 `StopAllCoroutines`, 전역 KillAll은 참고 코드에서 복제하지 않았다. 기존 StageManager의 독립 정지/최소 진행 정책을 유지하며 왜곡·빛·파편은 실시간으로 복귀한다. “Unity 기능100%”라는 완료 지표는 사용하지 않고 실제 게임 화면과 GPU 결과·반복/복귀로 판단한다.

## 임시 수치와 호환

[최신 도구 안내](TUNING_GUIDE.md) D-019 표를 따른다. 기본 원형/반환 stop.065/.075초, 흔들림.15/.18월드, 왜곡.045/.050UV·반경2.2·복귀.50초다. 강한 시안 stop.075/.085·왜곡.055/.060·반경2.4, 원형 파편.045월드다. 중앙점0, 배경 밝기.65/파동.8·속도6·폭.35는 임시이며 사용자 시각 검수 대기다.
새4개 시각 key를 더해161개이며 정확한 기존141/157키 full은 새 키만 기본 보완한다. 임의 누락/잘못된 값은 여전히 원자적으로 거부한다. 사용자 저장소를 초기화하거나 프리셋을 자동 덮어쓰지 않는다. 이전 프리셋의 중앙점/색은 명시적 요청이므로 새 기본값과 다를 수 있다.

## 검증과 보존

공식 단일 전체 실행 **35 PASS / 0 FAIL / 0 SKIP**(기존32+신규3),Run-02-Full/UTC2026-10-24T15:03:50.0947244Z. JSON/XML의35개 이름·상태가 일치한다. 초기 집중3PASS는 Run-01-Focus에 별도 보존했다. 새 검증은 기본.05/이전명시1의 실제 파편 메쉬 폭<.28월드·방향/AA·섬광0 첫 프레임,육각 Preparing/Warning/Fired·빔 노란색/중앙점0/정확한157 full 보완·임의 누락 원자 거부,실제 카메라 배경 파동·밝기·복귀/사용자 정지·UI 고정을 검사한다.24%는 불규칙 조각의 기본 크기 인자 상한이며 모든 꼭짓점의 반경이 정확히24%라는 뜻은 아니다.
실제 월드 GPU 밝기 검사: 밝아진114,132픽셀/평균밝기1.77185559배/효과 후 평균오차0. 이는 렌더 작동 근거이며 사용자 체감 만족도의 점수가 아니다. 기존 규칙·Pause/Retry/Home/풀/이동·잔상/훈련·기록 분리도 같은35개에 포함됐다.
최종 WebGL **nodefx-20261025-565495e82312**: Succeeded/오류0/경고0,79,685,334bytes.1346개 입력+identity 해시 일치/19실행파일HTTP200·동일바이트·WASM MIME. [새 미리보기](http://127.0.0.1:8880/)는 별도이며 이전8879/공개CP14는 보존했다.
실제 브라우저 기본 세션161키/.05파편/.15변형/.20돌진/.60배/중앙점0,50키 시각JSON 가져오기,노란 육각 충전→변형→실제 반환 파괴,원형 물리접촉·배경 파동·복귀,한국어 보고서 생성을 확인했다.390×844에서161육각프레임과원형 연속프레임/패널을 열람했고 가로넘침0/최종탭warn·error0이다. 대표 파동 요청1의 적용대기→.8복원 표시는 확인했지만 요청1의 실제 소비 확인은 중단되어 PASS로 주장하지 않는다. UI 재현 버튼은 내부 노드·잔상 생성이며 실제 마우스 이동·휴대폰 터치 검증이 아니다.
WebGL/35테스트에서 새 DOTween 집계는 관측하지 않았다. 과거26건의 발생 범위·전체 원인을 해결했다는 뜻은 아니다. Editor의 기존 폰트 참조 메시지/unused 필드 경고 이력도 남겨 두었다. 이번에는 전역 KillAll/경고 숨김/StopAllCoroutines를 추가하지 않았다.
사용자 허용으로 미저장 Cyberpunk 씬만 File/Save로 저장했다. 이전 디스크/.meta는 `Before`, 저장 직후는 `Cyberpunk-user-saved.unity`/`.meta`와 `BeforeFiles`에 보존했다. 구현 기준 씬 SHA256은8404f908b2e0af18b1ddc771fb646b4e3443a3932c9489e69c1fd93efd631af0이다. 다른 미저장 TMP imported4개는 저장/폐기하지 않았다.
시작 보호 파일1655개/기존 고정 출력13개를 기록/백업했다. 최고기록/일반 설정/프리셋은 기존 테스트 fixture의 개별 백업·복원 경로를 유지한다. 원형 공격.15/.05/.20초/.60배와 육각.06초 반환/피해·잔상·기록 규칙은 바꾸지 않았다. 커밋/푸시/공개 배포·새 설치는 하지 않는다.

근거 루트: `D:/renewal_CursorDash/Cursor-Dash-master/Logs/hitfeel2-20261024-231337`.

## 실제 전후 자료와 사용법

![원형 파편 실제 웹 전후](evidence/feedback2-20261024/circle-before-after.png)

위 두 장은 같은 내부 접촉 재현의 서로 다른 캡처 시점이다. 이전은기본1,수정은.05다. 시점을 동기화한 동일 마우스 실험으로 표시하지 않는다. 원본과 타임스탬프는 BeforeWeb/Web에 보존했다.

![노란 육각과 전체 파동 실제 연속 화면](evidence/feedback2-20261024/hex-yellow-impact-sequence.png)

![실제 웹 파동](evidence/feedback2-20261024/yellow-world-ripple.gif)

[390×844 실제 화면 시트](evidence/feedback2-20261024/mobile-sequence.png),[실제 월드GPU 전/중/후](evidence/feedback2-20261024/actual-world-gpu.png). GIF는 관측한 캡처 간격을 반영하나 짧은 사건 일부를 놓칠 수 있고 오디오는 없다.

8880 → 훈련 → 개발자 도구 → **강렬한 노드 시안** → 훈련·실행 조작의 원형 접촉/레이저 방어 재현으로 비교한다. **CP14 타격감 비교 기준**은 이전 낮은 효과 수치로 비교하며 새 외형은 유지한다. 노드 그래픽 그룹에서 파동강도.8/속도6/폭.35/밝기.65를 바꾼 뒤 다음 타격에 소비되는 요청·실제를 확인한다. 수치 단위/기본·강한 값은 [안내](TUNING_GUIDE.md) 표에 있다.
[시각50키JSON](examples/feedback2-strong-visual.json)을 텍스트에 넣고 **텍스트 적용**하면 공격/사용자 설정은 보존된다. [실제 변경 보고서](examples/feedback2-strong-report.txt)는 최종 빌드가 생성했다. 내보낸 기본/강한 full161 JSON은 근거 Web에 보관했다. 이번 클립보드 확인은 다시 수행하지 않았다.

## 이번 변경 파일과 종료 상태


- `AGENTS.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashGraphicsWorkbench.cs`
- `Cursor-Dash-master/Assets/Resources/CursorDash/BuildIdentity.txt`
- `Cursor-Dash-master/Assets/Resources/CursorDash/LocalImpact.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/NodeFlat.shader`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/ArrowNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeFlatGraphic.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashTuning.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/HitFeedback.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/ImpactFragments.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/LocalImpactDistortion.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashGraphicsComparison.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashGraphicsTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/ENVIRONMENT.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/SOURCES.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/TUNING_GUIDE.md`
- `docs/CursorDash/VALIDATION.md`
- `Cursor-Dash-master/Assets/Resources/CursorDash/ImpactFlat.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/ImpactFlat.shader.meta`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/WorldCircuitBackdrop.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/WorldCircuitBackdrop.cs.meta`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeedbackRevisionTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeedbackRevisionTests.cs.meta`
- `docs/CursorDash/FEEDBACK_REVISION_20261024.md`
- `docs/CursorDash/evidence/feedback2-20261024/README.txt`
- `docs/CursorDash/evidence/feedback2-20261024/actual-world-gpu.png`
- `docs/CursorDash/evidence/feedback2-20261024/circle-before-after.png`
- `docs/CursorDash/evidence/feedback2-20261024/circle-small-fragments.gif`
- `docs/CursorDash/evidence/feedback2-20261024/hex-yellow-impact-sequence.png`
- `docs/CursorDash/evidence/feedback2-20261024/mobile-panel.png`
- `docs/CursorDash/evidence/feedback2-20261024/mobile-sequence.png`
- `docs/CursorDash/evidence/feedback2-20261024/yellow-world-ripple.gif`
- `docs/CursorDash/examples/feedback2-strong-report.txt`
- `docs/CursorDash/examples/feedback2-strong-visual.json`

새근거이미지/JSON/보고서와문서 최종 갱신도 포함한다. 게임 기능을 위한 씬·프리팹 재작성은 없으며 위 명시 승인 씬 저장을 구분한다. 기존1655파일 누락0/저장7키 전후 일치,고정13출력·기존빌드결과 복원,Play/compile/pauseoff·씬clean·D014dirty4개 유지. 상세 최종 파일별차이는 final-state-comparison.json을 따른다.

남은 수동 확인: 실제 휴대폰에서 이동하면서 작은 파편의 가독성,연속 반사/모서리 타격 때 출렁임과 빛의 강도·어지러움,사운드와 정지 타이밍,장시간 발열/프레임. 이전 USER_VERIFIED는 이번 사용자 승인으로 재집계하지 않는다. 실제 폰트/컴파일 차단은 관측하지 않았으나 사용자 미적 승인은 대기다.
**다음 한 작업은 사용자와 앞으로의 개발 방식·작업 단위·시각 검수/종료 기준을 논의하는 것**이다. 사용자가2026-10-25 이를 요청했으며 새 규칙은 아직 합의하지 않았다. 이번 구현은 종료하고 추가 기능·수치 변경·커밋·푸시·공개 배포로 자동 진행하지 않는다.
