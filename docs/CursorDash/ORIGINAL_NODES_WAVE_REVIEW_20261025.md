# 원본 노드 복원과 WaveGame 타격감 비교 — 분석·수정 계획

**후속 D-021 정정:** 실제 원형 Outside 반알파 윤곽은 네 개가 아닌 **다섯 화살촉 원호**다. 원호 맞춤 중심(좌상단100,96.5px)은 기존 커스텀 피벗과 일치하며, 아래 전체 경계 상자 중심 차이를 그대로 이동량으로 사용하면 안 된다. 본체/충전의 미세 오차를 공통 중심으로 정렬하는 원형 한정 구현은 [D-021 결과](CIRCLE_ORIGINAL_A_20261025.md)를 따른다. 아래 내용은 당시 분석 이력이며 육각/넥서스 구현은 여전히 보류다.

확인일: 2026-10-25. **분석 완료 / 게임 파일 수정·Unity 실행·Play·테스트·빌드 없음.** 이번 결과는 구현 완료 보고가 아니다. 후속 구현과 실제 Play 검수는 별도로 진행하며, 사용자가 Unity에서 Play 검수를 담당한다.

사용자 확정: 실제 프리팹 원본 A의 형태·비율을 기준으로 하고 **육각은 노란색 유지**. 원형의 중심 오차는 복제하지 않고 본체/내부 충전/회전축을 일치시키는 수정 대상으로 확정했다. 원형 .15초 변형/.05초 반동/.20초 돌진/.60배와 육각의 여섯 꼭짓점 변형·발사 방향·중앙 충전·뒤→앞 소모를 유지한다. 중앙 점0은 유지하며, 원본 파일의 Central은 독립 점이 아니라 본체의 외곽선이라는 점을 구분한다.

## 1. 원본 기준 후보와 실제 확인 범위

**A — 복원 기준:** 현재 프로젝트에 보존된 `Assets/Art/Nodes/Cyberpunk/Types/Cyberpunk_NormalNode/`, `Cyberpunk_AttackNode/`의 분리된 부품. `HEAD 8ceedbb`의 `Assets/Prefabs/Nodes/Cyberpunk/NormalNode.prefab`, `ArrowNode.prefab`이 이 PNG GUID를 직접 참조한다. 현재 Cyberpunk 씬의 같은 두 프리팹 참조20개도 확인했다. 경로 이름만 보고 원본을 추정하지 않았다.

**B — 다른 옛 버전:** `Assets/Art/Nodes/Cyberpunk/OriginalNodes/Cyberpunk_NormalNode_Sheet 1.png`, `Cyberpunk_AttackNode_Sheet 1.png`. 두꺼운 보라 원/금색 육각의 프레임 교체 방식이다. 옛 `.anim`도 남아 있으나 A의 회전 부품 디자인과 구분한다. 실제 프리팹의 기본 충전 외형 복원 기준으로 B를 대신 선택하지 않는다.

![A 원본 부품의 충전 단계](evidence/original-wave-review-20261025/A-prefab-charge-composite.png)

위는 원본 PNG의 알파·메타 피벗·부품 정렬을 이용한 **분석용 합성**이다. 회전은 정지한 기준 각도이며 Unity 실행 캡처/새 시안이 아니다. 최초 합성은 이미지 중심으로만 배치했으나 이후 실제 메타 피벗을 반영해 갱신했다. 블러가 있는 원본을 그대로 관찰하기 위한 자료다. 제작 시 이 블러를 복제한다는 뜻이 아니다.

[A 개별 원본 부품](evidence/original-wave-review-20261025/A-prefab-original-parts.png) / [B 옛 시트](evidence/original-wave-review-20261025/B-original-sprite-sheets.png).

제공한 실제 교체 전 영상4개를 각각12프레임, 타격감 참고 영상을40프레임으로 새로 추출해 총88프레임을 열람했다. 원본 영상은 수정하지 않았다. 원형3.723초/2.331초,육각5.463초/5.331초,참고8.203초,모두1080×2340이다. **영상 프레임 관찰은 레퍼런스 프로젝트의 현재 실행/Inspector 실측과 다르다.** 오디오를 재생하거나 정확한 역경직 ms를 영상만으로 측정하지 않았다. 녹화 버전과 현재 디스크 프로젝트가 바이트까지 같다는 증거도 없다.

## 2. 원본 대비 새 노드 외형 차이

| 항목 | 실제 A 원본 | 현재 D-019 시안 | 복원 방향 |
|---|---|---|---|
| 원형 회전 테두리 | 화살촉이 있는 네 개의 끊어진 원호 | 화살촉 없는 세 구간 원호 | 네 원호의 길이·간격·화살촉·회전축 복원 |
| 원형 본체/충전 | 밝은 보라 외곽 원과 더 진한 내부 원이 분리 | 한 색의 원/충전으로 명도 구분 감소 | 외곽/내부의 명도·크기 비율 복원 |
| 원형 색 | 내부 주요 불투명색 약#CB49FF,외곽 약#E87CFF | 기본#AD1FFF를 여러 부분에 공통 사용 | 원본 보라 팔레트/명도차 참조 |
| 원형 비율 | 본체·내부 반알파 윤곽 약104px,바깥 테두리 약154×156px | 반경.54/바깥 원호 중심 반경.73의 이상적 도형 | PNG 전체 크기가 아니라 실제 윤곽·PPU·피벗으로 보정 |
| 원형 중심 | 부품의 피벗과 그려진 원 중심이 동일하지 않음 | 새 기본 원 자체는 계산 중심0 사용 | 원본 윤곽을 복원해도 기존 오차는 재도입하지 않음 |
| 육각 테두리 | 두 겹의 끊어진 육각,서로 반대 방향 회전 | 한 겹의 연속 육각 테두리,한쪽 회전각만 사용 | 두 겹의 끊김·간격·반대 회전 복원 |
| 육각 색 | 분홍/자주 두 계열 | 노란색 | 사용자 확정대로 노랑 계열 유지 |
| 충전 표현 | 독립 내부 도형이 중앙에서 확대 | 셰이더로 영역 확대 | 별도 층·비율·명도차를 원본에 맞춤 |
| 선명도 | 저해상도 가장자리/넓은 알파 번짐 포함 | 날카로운 절차적 도형 | 원본 윤곽을 정확히 다시 그리고 얇은 경계 AA만 적용 |

부품 수치 근거는 `original-sprite-measurements.json`,GUID/스케일/순서는 `original-node-parts.json`이다. 모든 관련 PPU는100. NormalNode 루트 스케일.5,육각의 Node 자식 스케일.5도 구분했다. 이미지의 투명 여백까지 도형 크기로 취급하지 않는다.

### 원형 중심 보정

메타 피벗: 본체/Inside는(.5,.5),Outside는 **(.526316,.486702)**. Outside190×188px의 피벗은 약(100,91.5)px인 반면,알파128 경계 상자의 중심은 Unity 좌표로 약(95,94)px다. 약(-5,+2.5)px 차이가 있다. 본체/내부도 반픽셀 수준의 윤곽 차이가 있다.
이는 **오차 단서/경계 상자 추정**이며 화살촉을 포함한 경계 상자 자체를 정확한 원 중심으로 확정하지 않는다. 실제 원호를 맞춰 중심을 정하고 본체·충전·회전을 같은 로컬 원점에서 그린다. 원본 PNG/.meta나 노드/콜라이더 위치를 덮어써서 맞추지 않는다. 변형 시작 원과 충전 완료 원도 같은 중심·반경으로 연결해 순간 이동/크기 점프를 막는다. 아직 게임에 적용한 수정은 없다.

### 구현 방식 판단

새 디자인이 목적이 아니므로 원본의 본체·내부·바깥 테두리를 독립 층으로 취급한다. 현재 `NodeFlatGraphic`의 원호/육각 테두리를 A 윤곽으로 바꾸는 제한적 수정이 우선이다. 단순 원/육각/원호/화살촉은 코드 도형으로 또렷하게 재현할 수 있고,원형 변형 Mesh와 자연스럽게 연결할 수 있다. 기존 PNG는 대조 원본으로 보존한다.
기본 충전 외형까지 한 가지 Mesh 형태로 강제할 필요는 없다. 정확한 Sprite 표현이 더 안정적이면 부품 SpriteRenderer와 변형 Mesh를 함께 사용할 수 있다. 다만 현재 저해상도 PNG를 단순 확대/필터 변경만으로 선명하게 만들었다고 보지 않는다. 기존 상태 시계와 풀을 유지하고,옛 Animator/새 코드가 같은 속성을 동시에 쓰지 않도록 한다. 새 도구·슬라이더·전역 시스템 추가를 선행하지 않는다.

## 3. 레퍼런스 실제 호출 경로와 적용값

읽은 프로젝트: `D:/Gamejam2026/Gamejam2026_wave`. `ProjectVersion`은 **6000.5.7f1**,URP 패키지 **17.6.0**. CursorDash는6000.3.23f1/Built-in이다. 레퍼런스를 CursorDash Editor로 열거나 업그레이드하지 않았다.

### A. 파도 발사와 최초 사람 접촉

`WaveAnimationSpawn.SpawnWave` → `WavePoolManager.SpawnFromPool` → waveLaunchClip 재생 → `WaveFront.Initialize/SetTargetWalls`.
`WaveFront.Update`가 파도 HitBox와 사람 RectTransform의 월드 사각형을 비교한다. 일반 Physics2D 충돌 이벤트가 아니다. `PersonState.Moving`이고 이번 파도에 아직 잡히지 않은 사람만 처리한다.

접촉 프레임의 순서:

1. `Person.HitByWave`: 상태Swept,이동 경로 점 제거.
2. 사람을 파도 자식으로 이동하고 sweptPersons에 등록.
3. `GameFeelManager.TriggerHitStop`: timeScale0 → **실시간.05초** 후1.
4. `SoundManager.PlaySFX(HitImpact.mp3)`: SFX AudioSource의PlayOneShot,요청 음량1/피치1.

이 접촉 경로에서 바로 TriggerShake/일반 배경 Flash를 호출하지 않는다. 별도의 파괴 Mesh/ParticleSystem도 이 경로에 없다.

### B. 득점 정산 이벤트의 큰 반응

Wave1/2/3 프리팹 → 각각 AnimatorController → Wave/Wave2/Wave3.anim을 실제 GUID로 연결했다. 공통 **Animator state speed.9**,prefab updateMode1(Fixed),clip 길이.5,`WaveEnd` 이벤트는 **clip시간.25**,`OnAnimationFinished` 풀 반환은 **.5**다. 정지 없는 게임시간 환산은 약.278초/.556초이며 히트스톱·FixedUpdate 단위 때문에 실제 벽시계와 다르다.

`WaveAnimationController.WaveEnd` → `WaveFront.ProcessWaveEndScoreAndCleanup`:

1. 잡은 사람 수 n을 계산하고 위험한 벽 여부 검사. 위험하면GameOver로 분기한다.
2. 정상 득점이면 `UIManager.AddScore(n)` → 점수/최고점 갱신(최고점은PlayerPrefs.Save).
3. 점수음 ScoreUP.mp3:음량.8,피치`1+.15n`;숫자 롤링.2~.6초/OutQuad,점수 크기 펀치.25~.45초 및 색상 복귀.
4. `GameFeelManager.TriggerShake(1+.3n, wave.transform.position)`.
5. 벽 전환 요청,사람 제거. 파도 오브젝트는 별도.5 이벤트에서 풀로 반환한다.

| 효과 | C# 초기값 | WaveGame 저장값/연결 | 호출 시 바뀌는 값 |
|---|---|---|---|
| 일반 히트스톱 | .05초 | .05초 | timeScale0→실시간 대기→1 |
| 셰이크 | .2초/강도50/진동15 | **.35초/50/20**,InGameCanvas/GameObject | 강도`50×(1+.3n)`,DOShakeAnchorPos/SetUpdate(true) |
| 왜곡 복귀 | .5초 | **2초**,DistortionVolume | weight=`Clamp01(.8+.03n)`→0,OutCirc/실시간 |
| LensDistortion | 프로파일 의존 | intensity **-.75**,overrideON | center.value를뷰포트값으로 변경하려는 코드 |
| ChromaticAberration | 프로파일 의존 | intensity **1**,overrideON | 같은Volume weight에 의해 함께 변화 |
| 일반 플래시 | 호출 없음 | FlashOverlay는BGCanvas의비활성Image | 일반 접촉/정산에서켜는 경로 없음 |
| 파편 | 생성 경로 없음 | WaveGame/파도3프리팹에서ParticleSystem없음 | 물보라는Sprite프레임 애니메이션 |

한 명이면 **UI 셰이크 요청65,Volume weight.83**,두 명이면80/.86,세 명이면95/.89다. 이것은 실제 저장값과 코드 경로로 계산한 값이며 실행 중 Inspector를 읽은 결과가 아니다. 참조 프로젝트의 다른 C# 사용자 코드에서 GameFeelManager 수치를 재설정하는 경로는 찾지 못했다. 런타임에 직접 바뀌는 것은 weight/center/timeScale/RectTransform/음량·피치 등이다.

### C. 게임오버

`UIManager.GameOver` → `TriggerGameOverFeel`:
게임오버음Death.mp3(요청1.2),실시간 **.2초** 정지,배율2 셰이크(100/.35초),Volume weight.9→0/2초,따뜻한 색Image Flash(알파0→1/.05초,1→0/1초).
UIManager는 이어서BGM을.5초 동안 저장값 **.02**로 낮추고2초 뒤.8초 동안 시작 음량.05로 복귀한다. 코드상 GameOverSequence가같은게임오버음을 다시 호출하는 경로도 있다. 레퍼런스에서 수정하지 않았다. 일반 파괴에 게임오버 플래시를 그대로 복사하는 것은 과하다.

### 중요한 미확인: 왜곡 중심

프로파일은 center의 **overrideState=0**이고,value만변경하는 코드에는이를켜는호출이없다. Unity의Volume은 override가꺼진파라미터를혼합에서제외한다([공식 VolumeParameter 문서](https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@17.0/api/UnityEngine.Rendering.VolumeParameter.html)). 따라서 이디스크상태에서는타격점을따라중심이반영된다고보장할수없다. 화면이휘는것은영상으로확인했지만실제중심위치는별도실행확인이필요하다. 레퍼런스파일을고쳐해결하지않는다.

![참고 영상에서 실제로 보인 격자 휨·색 분리·게임오버 빛](evidence/original-wave-review-20261025/wave-observed-frames.png)

영상에서1.056~1.266초의보드휨/색분리,4.615초부근의추가반응,6.289초이후밝은게임오버전환을관찰했다. 희소프레임만으로진동횟수/히트스톱.05초/정확한호출시각을실측했다고하지않는다.

## 4. CursorDash와 타격감이 달라진 이유

| 항목 | WaveGame | 현재 CursorDash | 이식 판단 |
|---|---|---|---|
| 렌더링 | URP17.6/Renderer2D,Quality전단계에UniversalRP할당,카메라postON | Built-in,OnRenderImage | Volume오브젝트만복사해선작동안함. URP전환은별도작업 |
| 실제 게임판 | BGCanvas와InGameCanvas가MainCamera로렌더 | 기본배경Sprite는월드,회로이미지Background_Flash는RemakeHUD Overlay | 기존회로이미지의렌더경로를게임판으로옮기는것이우선 |
| 흔들림 | UI보드그룹RectTransform,기준1080×2400 | 월드카메라Transform,좌표단위가다름 | 강도50/65를월드단위로복사하지않고화면변위비율로환산 |
| 왜곡 | 넓은LensDistortion+색수차,OutCirc2초 | 렌즈킥+전파사인파동/.5초,색수차없음 | 큰렌즈변형과감쇠의인상을먼저비교. 이동파동을필수로보지않음 |
| 회로 표현 | 기존이미지전체가카메라후처리에포함 | 실제회로이미지는후단,추가60구간Mesh만월드왜곡 | 별도WorldCircuitBackdrop을제거/연결해제하고기존그림을활용 |
| 밝기 | 일반타격Flash없음;게임오버만따뜻한전면Flash | 월드셰이더밝기+.035초접촉점흰빛;전체Flash기본0 | 사용자요청Background_Flash의타격점밝기는별도이식항목 |
| 타이밍 | 최초접촉정지/음,clip.25정산때보드반응 | 원형파괴한사건에정지·흔들림·왜곡·파편·음동시요청 | 원형은단발접촉이므로순서/시점은원형사건에맞춰구성 |
| 시간 소유권 | timeScale0→1직접복귀,StopAllCoroutines | StageManager의사용자정지/패널/개발자정지합성 | 기존StageManager를유지하고참고의강제1복귀는복제하지않음 |
| 사운드 | PlayOneShot,접촉음과정산음이분리 | 사건별AudioSource.Play,사용자음량적용 | 동시소리중첩과사건타이밍참고;참조오디오파일무단복사안함 |

직접 재사용할 것은 타격 중심 좌표의 의도,실시간 복귀,OutCirc 감쇠,사건별 강약 구분이다. URP 의존 API·Canvas용 셰이크 강도·충돌 방식·직접timeScale복귀는 이식이 필요하다. 원형 단발을 고치기 위해 렌더 파이프라인 전체를 먼저 바꾸는 것은 최소안이 아니다.

기존 대응에서 실제 회로 이미지를 후처리에 넣는 대신 별도 회로 Mesh를 추가한 것은 사용자 의도와 어긋났다. 현재 GPU 검증의밝기1.77배는새월드메쉬/월드패스가반응한다는증거이며,원래회로이미지전체를제대로처리했다는증거로대체할수없다.

## 5. UI와 게임판 분류 — 넥서스 포함

**Canvas는그리는방법이고,HUD는역할이다.** UI Image여도WorldSpace또는ScreenSpaceCamera로게임카메라안에그릴수있다. 모든Canvas를없애거나모든대상을Mesh로바꿀필요가없다.

| 대상 | 역할/권장 렌더 영역 | 이유 |
|---|---|---|
| 플레이어/원형/육각 | 게임판 | 이동·접촉·공격위치와시각이같은좌표 |
| 잔상/레이저/파편/접촉빛 | 게임판 효과 | 충돌점/입사방향과동일하게반응 |
| **넥서스 본체·내부체력수위·회전테두리** | **게임판의보호대상** | 실제맞는대상이며왜곡·흔들림에서주변과일관돼야함 |
| 체력숫자/점수/최고기록/시간 | 고정HUD | 읽기와화면기준배치가목적 |
| 배경원화/회로선/Background_Flash | 게임판 배경·효과 | 원본이미지를유지한채후처리에포함 |
| 조이스틱/일시정지버튼/설정/Dev/결과창 | 고정화면UI | 입력영역·가독성을왜곡에서보호 |

현재 넥서스는 UI/HpBar의 Image로 그리며, StageManager가 RectTransform에서 중심·반경을 측정해 공격 경계로 사용한다. 부모나 카메라만 바꾸면 측정 반경·표시·착탄점이 달라질 수 있다.

후속안은 같은 외형의 NexusRoot를 게임 좌표에 두고 본체·공격 경계의 공통 기준으로 삼는 것이다. 기존 수위 셰이더를 유지하려면 자식에 WorldSpace Canvas/Image를 두어도 된다. 체력 관리는 ScoreManager/StageManager를 유지한다. 본체 원의 외곽125/211 기준을 재현하고 원형 돌진 앞점과 육각 빔을 모두 확인한다. 이는 **구조 이동**이며 외형 재디자인이 아니다. 이번에는 실행하지 않고 첫 원형 파괴의 배경 수정과 분리한다.

## 6. 원형 파괴 하나를 먼저 개선하는 최소 수정안

다음은 제안이며 아직 구현하지 않았다. 기존 훈련의 원형 한 개를 사용하고 신규 훈련장·통합 패널·슬라이더부터 만들지 않는다.

1. **원형 충전 외형·중심부터 맞춘다.** A의 네 화살촉 원호,밝은 본체 외곽/진한 충전,실제 비율을 기존 그리기 컴포넌트에 적용한다. 모든 원의기준중심을통일한다. 현재변형→반동→돌진의상태·시간·판정은유지한다. 충전완료와변형첫프레임의윤곽/중심/크기를맞춘다.
2. **기존 회로 그림을 실제 효과 입력으로 연결한다.** 월드배경Sprite는유지하고,RemakeHUD의Background_Flash를후처리가가능한배경용Camera Canvas/게임판표시로분리한다. 현재의다른Flash/FlashImage와혼동하지않는다. WorldCircuitBackdrop의추가60구간은연결해제한다. 배경PNG를다시그리거나새회로를추가하지않는다.
3. **Background_Flash가 타격점에서 반응하게 한다.** 기존Image/같은텍스처에타격점마스크와짧은명도반응을주고정확히복귀한다. 저장된알파87/255는보존한다. 지금은정적으로87이므로무조건0으로저장하거나전역밝기만올리지않는다. 원본의정적표현은유지하며실행인스턴스의재질/효과증분으로시험한다. 국소플래시약.03초상승/.18~.25초복귀는**첫시안제안값**이며WaveGame의일반타격실측값이아니다.
4. **원형 사건만 레퍼런스 방식의 인상으로 비교한다.** 기존StageManager정지에.05초,원형접촉점에넓은렌즈킥/OutCirc복귀를적용하는비교안을만든다. 레퍼런스의실제2초감쇠를출발비교로삼되,장면템포에맞는최종시간은사용자Unity검수로정한다. 현재이동사인파동을먼저줄여렌즈변형/원본회로선의움직임을분리해판단한다. 셰이크는현재화면변위와참고65Canvas단위의화면비율을맞춘다. 색수차도원본에있는요소이지만핵심이미지경로/렌즈가맞은뒤약하게비교하고블러로대체하지않는다.
5. **작은 파편·접촉음은 기존 경로를 재사용한다.** 큰조각오류수정/크기상한은유지한다. 파괴음을맞는프레임에재생하고,압축·파편출발과정지시점을함께확인한다. reference의물보라프레임을노드파편으로오인해복사하지않는다. 기존정지상한·Pause/Retry/풀복원정책도유지한다.

직접 영향 파일 예상: `NodeFlatGraphic.cs/NodeFlat.shader`, 원형 그리기 접점(`NormalNode.cs`), `HitFeedback.cs`, `LocalImpactDistortion.cs/LocalImpact.shader`, Cyberpunk의 배경 표시 연결. `WorldCircuitBackdrop.cs`의 새 선 생성 호출을 해제하며 전체 재작성은 필요하지 않다. NexusRoot 이동·육각 기본 외형 복원은 같은 분석 계획에 포함하되 첫 원형 단발 비교에 모두 섞지 않는다.

## 7. 사용자 Unity 검수와 미실행 항목

| 확인 종류 | 결과 |
|---|---|
| 원본에셋/프리팹GUID/피벗/회전/색/충전클립 추적 | PASS — 정적 확인 |
| WaveGame 충돌→효과/Prefab→Controller→AnimEvent/씬·프로파일 저장값 | PASS — 정적 확인 |
| 지정5영상의88타임스탬프프레임 | PASS — 녹화영상시각관찰,실행실측아님 |
| 레퍼런스 실행중Volume/중심override/셰이크실제변위 | NOT_RUN — Unity실행·조작안함 |
| 현재 작업 컴파일/Play/공식테스트/새빌드 | NOT_RUN — 분석만승인,기존35PASS재실행으로표시안함 |
| 오디오실청/실기기/장시간검수 | NOT_RUN |
| 원형중심보정/복원/플래시연결 구현 | NOT_RUN — 수정할항목으로확정/계획이며게임파일미변경 |

구현 후 사용자가 Unity에서 확인할 최소 순서:

1. 기존 훈련에서 원형 한 개 생성. 충전0/중간/완료에서 중심·외곽·네원호를A와대조한다.
2. 직접접촉해파괴. 맞은점의회로선/밝기/화면휨/소리를보고,효과종료후원상태인지확인한다.
3. 연속접촉과Pause/Resume/Retry를한번씩확인한다. 원형.15/.05/.20/.60과공격판정을유지했는지확인한다.
4. 넥서스구조이동은그다음작업에서본체가맞는점과원형/육각공격경계를함께검수한다.

레퍼런스 실행 확인을 사용자가 원할 때는 해당6000.5.7f1에서WaveGame을열고,한명접촉/득점의두시점을나눠weight0→.83→0와center override/실제렌더중심,RectTransform변위를관찰한다. UIManager는최고점을저장하므로이번분석을위해자동Play하지않았다.

## 8. 보존과 근거

근거 루트: `Cursor-Dash-master/Logs/original-wave-review-20261025`. CursorDash시작main/HEAD8ceedbb,1672보호파일해시/기존미추적목록을기록했다. 레퍼런스Assets/Packages/ProjectSettings443파일의크기·수정시각과텍스트/메타해시를기록했다. 참조Git조회는작업경로외권한오류로실행되지않아Git상태확인완료라고하지않는다. 파일기준보존검사로구분한다.
핵심 근거: original-node-parts.json, original-sprite-measurements.json, circle-center-estimates.json, node-scene-overrides.json, reference-feel-settings.json, reference-render-audio.json, wave-animation-chain.json, VideoFrames의 타임스탬프 JSON. 최종 보존 결과는 preservation-final.json을 따른다.
이번에 생성·변경한 것은 분석 로그·비교용 이미지·관련 Markdown이다. 레퍼런스 원본과 CursorDash 게임 파일/.meta/씬/설정/저장값은 변경하지 않았다. 커밋·푸시·공개 배포는 하지 않았다.
