# D-034 — 육각 완충 대기·느린 축소·피버 렌즈/색수차 (2026-10-26)

상태: **소스 구현·정적/순수/참조컴파일 통과 / 사용자 Unity·GPU 검수 대기**. 자동 Play/빌드/커밋·push·배포/컴퓨터UI 조작은 하지 않았다. 이전 다른 항목의 승인을 추정하지 않는다.

## 육각 — 회전과 연동하지 않는 두 번의 대기

사용자가 말한 테두리2회는 박자 비유다. Rotator/테두리 각도·속도·트윈·이벤트는 참조하지 않는다. 기존 준비3.5초를 유지하며 앞쪽3박자를 재배분한다. 기본 상승.18/탄성안착.14/탄성.06·세번째1.35배는 그대로이고 새 Charge Beat Hold Seconds=.95를2/3와완충 뒤 각각 사용한다.

| 구간 | 이전(D-033) | 이번(D-034) |
|---|---|---|
| 첫번째 상승/안착 |.11~.43 |.11~.43 |
| 두번째 상승/안착 |1.59~1.91 |.96~1.28 |
|2/3 정지 |1.91~3.18 |1.28~2.23 (.95초) |
|세번째 상승/안착 |3.18~3.50 |2.23~2.55 |
|완충 정지 |없음 |2.55~3.50 (.95초) |
|변형 시작/실제준비끝 |3.50 |3.50 |
|명목 발사/넥서스도달 |3.775/3.835 |동일 |

실제공격조건은 여전히 `elapsed>=prepare`이며 완충 표시가 먼저 끝나도 변형을 시작하지 않는다. 준비·변형.25/예비.025/비행.06과 기존프레임처리코드는 유지했다. 위시간은 저장값/설정의 합계로 실제Editor프레임측정이 아니다. 개발자hex.prepare가다르면 그길이안에서 대기를 줄여겹침없이 분배한다. 파괴·취소·재사용은기존Reset/elapsed/Visible경로로처리하며 예약공격·효과음을 추가하지 않았다.

## 검정 원 — 더 긴 퇴장 예고

축소시간을.24→.55초로 늘려기존2.8초수명의 마지막.55초에 걸쳐서서히줄어든다. 첫축소시작은2.56→2.25초로앞당겨지고 만료는2.8초 그대로다. 완전크기로있는시간은.31초짧아지지만 축소방향을더오래볼수있다. 표시와실제원판정이같이줄며 남은크기에맞으면수집가능하다. 마지막.14초 알파감쇠/등장.12/실제수명/생성간격.12·상한14/노드당점수·피버8초·×4는 유지한다. 일반노드의진입정리/몸체퇴장.18은바꾸지않았다. 피버종료전에도남은시간에맞춰.55초축소한다. 내부단계강제건너뛰기는자연퇴장검수가아니다.

## 기존 피버 타격 경로 — 코드 기본값

현재사용자Prefs/실행중Inspector값을읽지않았으므로아래는기본강도/슬라이더1/FX ON 기준이다. 실제확인은사용자Play에서한다.

`실제머리 Trigger/연속경로 → removed/실행ID 1회가드 → 점수 → FeverBonusBurst → BonusFeel(실제 접촉점)`

| 요소 | 기존 적용 | 이번 |
|---|---|---|
|파편 |전용18개/강도2.5/.014압축/.48수명 |보존 |
|히트스톱 |.012×2.5=.030초/최대.035·최소간격.18 |보존 |
|카메라 방향쉐이크 |기본.40월드·.14초/28Hz/감쇠2·반동.35/간격.10 |피버만.45배→기본.18월드, 다른값/상한보존 |
|접촉점 명암 |짧은 국소잉크반응/.24초·Flash설정존중 |보존; 전체흰색플래시가아님 |
|점선/효과음 |점선bounded가속/기존폭발SFX볼륨.6·피치.97~1.12 |보존 |
|왜곡 |아주작은필드(exp공간.00055)/약.00065굴절·gain2.5/최대.002·빠른감쇠.24 |구형미세왜곡0으로대체, 새최종광학패스 |
|색수차 |없음 |실제RGB채널분리 |

기존왜곡은코드에있었지만주요영역이너무작았고, 흰세계를다시그릴때회로를원래UV로샘플해회로는굴절되지않았다. 이때카메라쉐이크가상대적으로두드러질수있다. 사용자화면에서이원인만발생했다고단정한것은아니다.

## 새 왜곡·색수차 — Unity Built-in 최종 게임판 후처리

새URP Volume이나패키지설치없이기존카메라/RenderTexture/Graphics.Blit의후처리셰이더를쓴다. 합성은 **일반게임판/기존효과→넥서스→흰피버세계/회로·노드/파편→실제커서→FeverImpact(새)→고정HUD·설정/일시정지UI** 순이다. 흑백재합성뒤에RGB를분리하므로색수차가다시검정으로덮이지않는다. 숫자·버튼은이RT처리에포함하지않는다.

- 실제타격점에서넓은렌즈압축과바깥으로이동하는곡선굴절전면을만든다. 국소적인짧은수평미끄러짐도같은타격점근처에만있다. 프레임마다무작위로화면전체를옮기는효과가아니다.
- 완료된게임판텍스처하나를굴절하므로회로·넥서스·노드·파편·커서에같은화면좌표변환이적용된다. 실제게임오브젝트/입력·충돌은움직이지않는다. 작은색경계가원래판정을바꾸는것도아니다.
- 빨강/초록/파랑을서로다른UV에서실제로읽어경계의색을분리한다. 평평한흰영역은흰색, 검은본체내부는검정으로유지된다. 별도의그림/색오버레이를붙인방식이아니다.
- 피버유지중얕은색수차, 적중때강한색분리/굴절이.30게임초감쇠한다. 시작.12/끝.18초weight로일반모드복귀때끊김을줄인다. 기존작은왜곡은꺼두어중복하지않는다.
- 최대6개의최근실제접촉점/픽셀마다가장강한필드를선택한다. 변위는짧은화면축UV.018, 색분리는.006상한이며연속타격이더해져무한강화되지않는다. 정지·쉐이크의기존간격/상한도유지한다.
- FX OFF/사용자왜곡0/일반모드/진단ON은새광학을우회한다. 진단ON에서는원래머리·노드/접촉점좌표를보도록하는것이며정상효과상태와구분한다. 일시정지/히트스톱은게임시계를멈춰굴절도그상태로정지한다. 종료·취소·Retry/Home/체력0에서는위상/접촉목록과함께광학이빠진다.
- C#는연결/리소스해제만참조컴파일확인했으며 실제Shader컴파일·GPU픽셀/부하·모바일은미확인이다. 리소스존재/호출카운터만으로실제효과성공을선언하지않는다.

## 조절 위치

육각 Inspector **Charge Beat Hold Seconds=.95**(2/3/완충각각), 기존Rise.18/Settle.14/Elasticity.06. 기본준비3.5/변형·발사설정은변경하지않았다.
기존 `Assets/Resources/CursorDash/VisualPrototype.asset`:

| 항목 | 초기값 |
|---|---|
|Fever Node Retire Seconds |.55초 |
|Fever Optics Seconds |.30초 |
|Fever Lens Strength |.014(짧은화면축UV) |
|Fever Lens Radius |.58(짧은화면축에맞춘확산범위) |
|Fever Chromatic Hit |.0035(타격추가RGB거리) |
|Fever Chromatic Ambient |.00065(유지RGB거리) |
|Fever Shake Scale |.45(기존피버쉐이크만배율) |

짧은화면축이720픽셀이면기본유지색분리는약.47px, 타격추가최대약2.52px이고각필드감쇠/사용자왜곡값에따라실제는달라진다. 상한RGB.006은4.32px이다. 렌즈.014는직접모든픽셀이10.08px이동한다는뜻이아니며공간/시간함수와상한으로제한한다. 숫자들은검수용초기값이다.

## 확인과 사용자 검수

PASS 정적46, 실제순수NodeRhythmMath/FeverFlowMath/FeverState .NET116,402단언, C#참조컴파일108/12/2오류0. 공식87작성·컴파일/실행NOT_RUN. 기존육각곡선단언은최신대기조건으로갱신했고핵심단언을삭제/무시하지않았다. Shader문법구조/경로확인은GPU컴파일성공이아니다. Native셰이더·GPU/Physics·Play·실제카메라·화면비/모바일·음향·저장/빌드·git·배포는NOT_RUN.

1. 직접 `Assets/Scenes/01.StageSelectScene.unity` → Play → 훈련(Cyberpunk). 일반육각한개를방치해2/3정지→완충/탄성→약.95초완충정지→기존변형/예비/발사를본다. 테두리회전횟수보다2/3·완충정지구간이잘읽히는지본다. 회전속도는수정하지않았다. 실제발사시간은기존검수창 WarningCompletedAt/FiredAt/NexusReachedAt로필요할때비교한다.
2. 피버에서검정원을건드리지않고2.8초수명/마지막.55초의느린축소를본다. 작은원에접촉하면보이는크기와판정이맞고점수/파괴가1회여야한다. 수명을늘려해결한것은아니다.
3. 훈련에서만충으로자연피버진입. FX ON/사용자왜곡1/접촉위치진단OFF에서검정원을단발로친다. 타격점주변회로·주변노드·넥서스/커서가국소적으로굽고, 도형의경계에RGB가분리되는지본다. 상단점수/배율/일시정지버튼에는굴절/색테두리가생기면안된다.
4. 연속으로여러원을타격한후잠시멈춘다. 왜곡/RGB가누적해화면을덮지않고가라앉는지, 검정파편/커서와다음원이읽히는지본다. 얕은유지색수차는피버동안남고일반모드에는남지않아야한다.
5. 효과가안보이면사용자설정을자동초기화하지말고왜곡값/FX/진단ON여부와Console을확인한다. `Tools/CursorDash/넥서스 수동 검수`의‘피버광학후처리/호출’에서연결및호출숫자를본다. 미지원/셰이더오류는별도로보고한다. 호출증가는픽셀품질통과증거가아니다.
6. 접촉위치진단ON에서는광학이잠시우회되는지, 머리/실제원/접촉·효과원점은원래좌표에서맞는지본다. OFF후정상광학으로돌아온다. 일시정지/종료·취소/Retry/Home·체력0에서시간·입력·색/RGB·광학이정리되는지도확인한다.

이전승인된일반외형/등장/공격/반사·파괴/넥서스최종·입력/HP·점수/기록·공급·Vector4수정은보존했다. 기록·Before·이번만의차이는 `Logs/fever-optics-hold-20261026`이며 다음단계로진행하지않고사용자의실제영상/Play검수를기다린다.

## 이번 변경 파일

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashNexusReview.cs`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverFlow.cginc`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/ArrowNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/FeverBonusNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeRhythmMath.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverFlowMath.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverWorldTransition.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/LocalImpactDistortion.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeverPolishTests.cs`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/ENVIRONMENT.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/SOURCES.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/VALIDATION.md`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverImpact.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverImpact.shader.meta`
- `docs/CursorDash/FEVER_OPTICS_HOLD_20261026.md`

Before기준1,835파일 중1,816동일/허용19수정·3신규, 누락·예상외변경·백업불일치0. Git루트/main/HEAD·스테이징없음은유지했다. Logs는배포/커밋대상이아니다.
