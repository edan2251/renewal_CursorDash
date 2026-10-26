# CursorDash 수치 도구 사용법 — CP1~CP4 v2

## 최신 D-053 같은 숫자·출현·공간 피격·flow (2026-10-27)

기존 **VisualPrototype.asset** Inspector의 Shared main/gameplay / Confirmed Nexus health loss / Dynamic circuits를 사용한다. 별도도구·프리셋 자동덮어쓰기는 없다. 아래D-052의 `spaceBestScale/Opacity`는 숨김/호환보존/미사용이며 두숫자교차방식은폐기되었다. [모든전후값·적용시점·수동검수](SHARED_SCORE_SPACE_POLISH_20261027.md).

|조절|현재값|의미|
|---|---|---|
|spaceStartSeconds / NexusAt / CursorAt|1.35초 / .42 / .64 유지|같은 시작시계에서 준비·실입력해제후 플레이|
|spaceRingLead / BodyOvershoot / CursorAcceleration|.10 / .045 / .18|전체시계ring선행비율·한번body초과·커서초기가속비율, 다음시작캡처|
|spaceRingBoost / RingDecay|780 / .30초|기존 bounded spin에 출현pulse, 평상시로감쇠|
|spaceMainScoreScale / Opacity / Lift|.82 / .85 / .28world|같은숫자의메인시각크기·흰색alpha·Nexus위오프셋, 실제Rect4×4/Scale1유지|
|spaceLogoWidth / LogoY|840 / .79|Canvas기준폭/anchor, 실제폭86%제한|
|nexusDamageWaveWidth / Compression / ReleasePower|.12 / .85 / 1.4|파동폭·초기압축·감쇠곡선. width이전.075, 나머지신규|
|nexusDamageSeconds / Flash / Chromatic / WaveStrength|.24초 / .26 / .0055 / .008 유지|실제HP감소만 plainred/게임판광학, 사용자감소설정·상한존중|
|nexusGaugeRewindSeconds|.16초 유지|실제Gauge0과별개로표시만되감기, live새충전목표|
|circuitAmbientInterval / Strength|3.4→2.6초 / .055→.10|기본선대비변경없이quietflow만 보완|
|circuitAmbientTail / Head / Speed / Limit|.20 / .025 / .24 / 2|앞또렷·뒤감쇠, 경로거리/초, 하드3·묶음뒤휴지|

출현값은다음시작에캡처, 메인숫자/로고표시와flow는기존매Draw/Advance에반영한다. 숫자AutoSize1~3.5는시작전/플레이에서유지하고countdown중font잠금→0뒤smooth복원하므로중간Inspector의AutoSize OFF는의도다. Height2.5를2.5배로적용하지않았다. 파편·hex·공격·피버전투값은이번튜닝대상이아니다.

## 최신 D-052 공유 공간·피해·평상시 흐름 (2026-10-27)

기존 **Assets/Resources/CursorDash/VisualPrototype.asset**의 `Shared main / gameplay space` / `Confirmed Nexus health loss` / `Dynamic circuits` 항목을 사용한다. 새 별도 도구·프리셋은 없으며 기존 사용자 프리셋을 자동 변경하지 않는다. [전체 값·적용시점·검수](SPACE_DAMAGE_FEEDBACK_20261027.md).

|조절|필드·기본값|단위/적용|
|---|---|---|
|전체 시작|spaceStartSeconds **1.35**|초, 다음 시작 캡처; 빠른Retry는 .24초|
|회로걷힘/재구축 끝|spaceRetreatEnd **.28** / spaceRebuildEnd **.82**|전체에 대한 비율; 기본.378/1.107초|
|넥서스/커서 시작|spaceNexusAt **.42** / spaceCursorAt **.64**|전체에 대한 비율; 기본.567/.864초|
|커서거리/최고점후퇴|spaceCursorDistance **.95**, spaceBestScale **1.5**, spaceBestOpacity **.52**|world/배/alpha; 다음시작, Main alpha즉시|
|충전손실 되감기|nexusGaugeRewindSeconds **.16**|초; 실제값초기화는 즉시, 다음 실제피해|
|피해 플래시/RGB|nexusDamageSeconds **.24**, Flash **.26**, Tint **(1,.04,.08,1)**, Chromatic **.0055**|다음실제HP감소; 기존 user효과/접근성존중|
|피해 이동파동|nexusDamageWaveEnabled **true**, Strength **.008**, Width **.075**, Speed **2.6**, Range **.72**|짧은화면축 정규화/초; 변위.018·RGB.006상한|
|평상시 flow|circuitAmbientInterval **4.5→3.4초**, Strength **.025→.055**|속도.24/동시2·타격1.65/.18은 유지|

기존 넥서스 수동 검수창에 공유씬 상태·생존초/최고/현재·시작진행·원호되감기·피해광학 상태를 추가했다. 실제 검수는 사용자가 Play/훈련에서 수행한다. 훈련무적은 실제HP피해가 아니므로 붉은색 검수 시 해제한다. 검수조작/훈련·개발자모드는 기록 제외이며 결과에 이유가 표시된다. 육각/피버 전투값을 이번 작업에서 조정하지 않는다.

## 최신 D-051 회로 밀도·보조 폭 (2026-10-27)

기존 **VisualPrototype.asset → Dynamic circuits**에서 `circuitDensity` **8→12**(다음 라운드/화면비 재구성), `circuitSecondaryWidth` **.010185185→.013888889**(11→15px/1080, 즉시)를 사용한다. `circuitPrimaryWidth` **.018518519**(20px), 일반/피버 대비.075/.065·색/기존 흐름/타격·구축/게이지후퇴·획 가림은 그대로다. 기본12묶음/82선·4~12 범위, 기본 복원/fallback도 일치시켰다. 옛 비교 폭과 프리셋은 보존한다. [전후·검수](CIRCUIT_DENSITY_20261027.md).

## 최신 D-050 회로 디자인 — 현재 기본값 (2026-10-27)

**VisualPrototype.asset → Dynamic circuits**의 기존 항목만 사용한다. `circuitDensity`10→**8**(다음 라운드), `circuitPrimaryWidth`.021296296→**.018518519**(주20px/1080), `circuitSecondaryWidth`.011111111→**.010185185**(보조11px/1080, 폭은 즉시). 기본 대비/색/흐름·타격·구축/복원·게이지 후퇴/획마스크 값은 D-049 그대로다. 절대 시험값을 한 번 저장한 것이며 반복 감소하지 않는다.

현재 새 경로는8묶음/56선,4~12 조절/상한66선. 직각 L/T와 큰 원 없이 대각 Y분기·선폭의 둥근 끝을 사용한다. 두 단계 폭OFF는 새 경로에 이전23/12 폭을 비교한다. 이전3폭 필드는 호환용으로 보존한다. 별도 도구나 제어 항목은 추가하지 않았다. [전후/보존/사용자 검수](CIRCUIT_DIAGONAL_DESIGN_20261027.md).

## 최신 D-049 게이지 회로 퇴장·점수 획 가림 (2026-10-27)

**VisualPrototype.asset → Dynamic circuits**를 사용한다. 회로 중앙 흡입은 대체되어 실행하지 않으며, 검정 노드/공간/링의 기존 종료는 유지한다.

|조절|필드·현재 절대 시험값|적용|
|---|---|---|
|주회로/보조 두께|circuitPrimaryWidth **.021296296** / SecondaryWidth **.011111111**|23/12px(1080기준). 즉시. 옛46.5/28/10·12의 비율을 반복 곱하지 않음|
|일반/피버 기본 대비|NormalContrast **.075** / FeverContrast **.065**|기본알파×.8. 이벤트 추가강도는 별도|
|일반/피버 RGB|NormalTint **(.40,.47,.52)** / FeverTint **(.18,.19,.20)**|점수/배경 색은 변경하지 않음|
|후퇴 시작/분산|RetreatStart **.18** / Spread **.30**|실제 피버 진행비율. 다음 피버 진입 캡처|
|후퇴 곡선/읽기|RetreatPower **1.35** / ReadHold **.06**|경로별 고정 차이/구축 후 최소 읽기. 완료는.58~1분산·Gauge0 전부0|
|숫자 획 가림|ScoreMaskStrength **1**, Padding **.65**, Softness **.75**|숫자 표시알파와 독립·여유/경계는 게임판 렌더픽셀. 즉시|
|은은한 흐름|AmbientStrength **.025**, Interval **4.5초**|속도.24/동시2·분기 감쇠와 휴지기는 유지|

기존 검수창에서 **게이지 회로 후퇴(다음 피버)**/점수 획 가림/두 단계 굵기/은은한 흐름을 개별 비교한다. 토글은 현재 라운드 임시이며 후퇴만 다음 피버에 캡처한다. 후퇴OFF는 정적 구축 비교이며 중앙 흡입을 되살리지 않고 Gauge0에서 제거한다. 두 단계OFF는 보존된 옛3폭 비교다. 프로필 기본값 복원은 회로45필드(옛 숨김8 포함)만 Undo/dirty하며 자동 저장하지 않는다. 임시 override는 Retry/새 라운드에서 초기화된다. 원본PNG 비교는 정적 원본 보존용이며 새 경로 후퇴/획 마스크 적용 대상과 구분한다.

실제 Unity/GPU·SDF/웹·모바일은 검수 대기다. [마스크 구현·수치 전후·수동 순서](CIRCUIT_GAUGE_RETREAT_GLYPH_MASK_20261027.md).

## 최신 D-048 회로 접합·흐름·개별 흡입 (2026-10-27)

기존 **VisualPrototype.asset → Dynamic circuits**에서 조절한다. 큰 원 장식 필드는 호환 보존/숨김이며 이제 그리지 않는다. 구도/밀도10과 기존 PNG 비교는 유지한다. 폭의 단위는 짧은 화면축1이다.

|조절|필드·현재값|적용|
|---|---|---|
|굵은 주회로|circuitMainWidth **.043055556**|60/1080에서77.5%로 한 번 낮춘46.5/1080 시험값|
|중간 / 가는 폭|circuitLineWidth **.025925926** / circuitBranchWidth **.009259259**|이전과 동일. 즉시|
|일반 / 피버 기본 색|circuitNormalTint **(.46,.55,.60)** / circuitFeverTint **(.18,.19,.20)**|RGB만 편집·알파 별도|
|일반 / 피버 불투명도|circuitNormalContrast **.1125** / circuitFeverContrast **.12**|시작 .15/.16의75%. 반복 곱셈하지 않음|
|느린 평상시 흐름|circuitAmbientSpeed **.24**, Interval **4.5초**, Strength **.035**, Limit **2**|다음 흐름 간격은.85~1.50배로 분산·휴지기/동시상한0~3. Limit0=끔|
|실제 타격|circuitTransferSpeed **1.65**, HitDecay **.26**, MaxEmphasis **.18**, TipEmphasis **.14**|이전 값 유지. 기본 대비와 별개|
|개별 선 흡입|circuitRecallBend **.65**, RecallPower **2.6**, RecallStagger **.12**|다음 종료에 캡처. 시차는 기존B구간 내 비율|

구축/컬러 복원/바깥 퇴장 속도·회로 외의 배경/점수/피버 값을 변경하지 않는다. 현재값 복원 버튼은 회로32필드만 Undo/dirty로 복원하고 자동 저장하지 않는다. 임시 대비 토글은 기본OFF/현재 라운드만이다. 실제 GPU/휴대폰 검수 전인 시험값이며 [전후값·레이어·검수 절차](CIRCUIT_JOINTS_INTAKE_FLOW_20261027.md)를 따른다.

## 최신 D-047 회로 주선·가지·접점 조절 (2026-10-27)

기존 **Assets/Resources/CursorDash/VisualPrototype.asset → Dynamic circuits**에서 조절한다. 공간 단위는 화면 짧은 축1이다. 기본10묶음/62선·9단자(밀도4~12)이며 밀도만 다음 라운드/실제 화면비 변경부터 적용한다. 그 외 아래 크기·대비는 즉시 반영하며 타격 때 리소스를 만들지 않는다.

|필드|현재 값|조절 대상|
|---|---:|---|
|circuitLineWidth|.025925926|주회로 기준28/1080. 넓은 버스는 작성된60/28 비율|
|circuitBranchWidth|.009259259|가는 가지/평행선10/1080, 일부12/10 비율|
|circuitContactRadius|.02962963|원형 단자 반경32/1080|
|circuitContactStroke|.006481481|단자 테두리 실제 굵기7/1080|
|circuitNormalContrast / circuitFeverContrast|.15 / .16|일반/흰 피버 기본 대비. 별도 색면 설정은 보존|
|circuitTipEmphasis|.14|구축/후퇴 등 이동하는 선끝만 강조|

타격 전달/감쇠/상한·구축/복원/밀려남·휘어짐/후퇴 값과 전환 캡처 시점은 D-046과 같다. 기존 검수창의 동적 회로OFF/ON은 원본PNG/새경로를 번갈아 표시한다. 새 **회로 대비 검수**는 현재 라운드 임시1.8배/알파 .42 상한/기본OFF, 저장값을 바꾸지 않는다. ‘회로 프로필 기본값만 복원’은 회로23필드만 Undo/dirty 처리하며 자동 저장하지 않는다. 기존 JSON/사용자 프리셋·다른 승인 설정을 덮어쓰지 않는다. [전후값·원본 근거·검수](CIRCUIT_ORIGINAL_LAYOUT_20261027.md). 실제 화면/모바일 두께와 성능은 사용자 검수 대기다.

## D-046 동적 회로 조절 (2026-10-27)

기존 `Assets/Resources/CursorDash/VisualPrototype.asset → Dynamic circuits`를 사용한다. 밀도8루트/40선·굵기.0026·일반/피버 대비.12/.13, 국소 범위.13·전달1.65·거리감쇠.26·지속.24초·추가강조.18·동시4(하드8), 구축/복원1.8/2.1·시차.018초, 밀려남.075·휘어짐.65, 후퇴1.6·시차.018초가 첫 시안이다. 길이/속도의 공간 단위는 화면 짧은 축1이며, 판정이나 게임 규칙 값이 아니다.

밀도는 다음 라운드/실제 화면비·카메라 크기 변경 때, 구축·밀려남은 다음 진입, 복원·흡입은 다음 종료, 후퇴는 마지막 타격 시작에 캡처한다. 굵기/대비·타격 값은 즉시 클램프 반영하되 리소스를 재생성하지 않는다. 기존 넥서스 검수창의 동적 회로/국소 타격 토글은 **현재 라운드 임시**로 PNG/타격 비교에 사용한다. 기본 복원 버튼은 **회로 프로필19값만** Undo/dirty 처리하고 저장하지 않는다. 임시 토글은 별개라 다시 켜거나 Retry한다. 기존 JSON171/사용자 프리셋/승인된 다른 시각 설정은 자동 변경하지 않는다. [전체 필드·적용 시점·수동 검수](DYNAMIC_CIRCUITS_20261027.md).

## 최신 — D-044 배경 점수 World 단위·상단 총점 제거 (2026-10-27)

최신 사용자 스크린샷/글과 후속 ‘화면 위 하얀색 점수텍스트 제거’를 반영했다. `Projected total score`는 **Local Scale(1,1,1) / Rect Width4·Height4 / TMP Auto Size ON·Min1·Max3.5**이며 화면 중앙(.5,.5)을 유지한다. D-043의200×250/50~150을 대체한다.

이전 Scale은 화면 폭/높이 맞춤과 획득 탄성에 쓰였다. 이번에는 자동 화면 맞춤을 제거하고 World Rect/TMP 단위를 우선한다. 획득 반응은 기존 단일 탄성값을 **TMP OnPreRenderText의 글자 정점**에 적용하여 Transform Scale·Rect·AutoSize 범위를 바꾸지 않는다. 새 기본 메시에서 적용해 확대가 누적되지 않으며, 비활성화·종료 때 이벤트를 해제한다. 이전 backgroundScoreSize/Width 값은 호환 보존하되 Inspector에서 숨기고 더 이상 적용하지 않는다. 긴 점수의 폭은 이제4×4/Min1 범위이므로 극단 자릿수는 실제 화면 검수가 필요하다.

상단 총점은 TMP와 legacy Text 컴포넌트 두 경로 모두 Play 중 숨긴다. 글꼴 원본·점수 데이터·기록·결과 점수는 보존하고, **×2~×4와 Pause 버튼은 유지**한다. 숨긴 총점 폭을 예약하지 않아 배율만 상단 중앙에 배치한다. 배경 점수 토글을 꺼도 상단 총점이 자동으로 다시 나타나지 않는다. 씬/원본 객체를 삭제·저장하지 않고 Play 종료/해제 때 원래 사용자 표시 상태를 복원한다.

변경: FeverScoreHud.cs(설정/정점반응/상단표시), CursorDashFever.cs와StageManager.cs(legacy 표시 참조 전달만), CursorDashVisualProfile.cs(옛자동맞춤필드숨김), 기존FeverPolishTests의관련기대값1개. 새테스트없음/공식103은작성·컴파일만. **PASS** 정적설정·범위/기존파일보존·참조C#게임108·테스트12·Editor2 오류0/기존런타임경고134. **NOT_RUN** 실제 Unity·TMP/GPU·Play·공식실행·빌드·커밋/push/배포·컴퓨터UI. 근거/Before/이번만패치: `Logs/score-world-units-20261027`.

**다음 한 작업:** 사용자 새 Play에서 배경 점수 Scale1/Rect4×4/AutoSize1~3.5·화면중앙 → 단발/연속 획득 때 Scale과 Min/Max 고정/글자 반응 → 상단 총점 없음·배율/Pause 유지 → 일반/피버·Pause/Retry/Home·긴 점수를 검수한다. 피버 종료·육각/승인VFX·점수 규칙은 이번에 변경하지 않았고 기존D-042 미검수/보류는 유지한다. 자동 Unity실행/Play·빌드·Git/배포를 시작하지 않는다.

## 이력 — D-043 Projected total score 텍스트 설정 (2026-10-26)

최신 사용자 지시로 런타임 생성되는 배경 점수의 Rect Transform을 **Width200/Height250**, 중앙 pivot/가운데 정렬, 위치를 **카메라 뷰포트(.5,.5)**로 설정했다. TMP **Auto Size ON / Min50 / Max150**이며 수동 fontSize 시작값은150이다. 매 프레임 preferred 크기로 sizeDelta를 덮던 코드를 제거하고 고정 Rect에서 Auto Size를 먼저 계산하도록 했다. 기존 글꼴·재질/표시 순서·점수 데이터·획득 탄성/화면 폭맞춤은 유지한다.

변경 소스는 `Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs`, `CursorDashVisualProfile.cs`, `Assets/Resources/CursorDash/VisualPrototype.asset`의 배경점수Y .58→.5뿐이다. 런타임 생성 객체이므로 씬/프리팹 저장이나 미저장 상태 조작은 하지 않았다. **PASS** 정적 설정·범위 보존 및 참조 C#108/12/Editor2 오류0/기존 런타임 경고134. 새 테스트는 추가하지 않았으며 공식103 실행/Unity·TMP 실제 화면·Play·빌드/Git/배포는 **NOT_RUN**이다. 근거/Before는 `Logs/score-text-layout-20261026`.

**다음 한 작업:** 사용자가 새 Play에서 `Projected total score`를 선택해 Rect200×250/화면중앙/Auto Size50~150을 확인한다. 기존 점수 획득 반응·긴 숫자와 일반↔피버 가독성은 직접 검수한다. 이전 D-042의 피버종료/점수 검수 대기는 유지하며 다음 구현을 자동 시작하지 않는다.

## 이력 — D-042 피버 종료·배경 총점 (2026-10-26)

**Assets/Resources/CursorDash/VisualPrototype.asset**의기존Inspector를사용한다. 종료appear.20/absorb.51/settle.24(총.95초),appearSpin180/max1260/decay.10,squeeze.12/recoilHold.06,bend.28/power3.2/twist.90,nodeStretch.65/nodeShrinkStart.22(남은반경거리)/spacePull.65,RGB.006/lens.012/shake.0035. 종료시캡처/실제피버8초별개/판정·공급은Gauge0에끝난다.
Projected total score그룹: enabledtrue,position(.5,.58),size.42(짧은축높이)/width.8,tint(.36,.76,.84),opacity.105/boost.14/cap.24,reaction.22/scaleCap1.22,response.32. 실제획득량으로한개의탄성상태만갱신/정확한총점즉시표시. 이사만루Bold·게임판MeshRenderer -900/기존상단HUD유지,피버알파마스크에서저대비회색.
기존Tools→CursorDash→넥서스수동검수의‘배경 큰 점수 (현재 라운드)’토글은임시비교용/Retry때프로필기본복귀다. 웹템플릿기존피버검수에도같은토글만추가(현재공개웹에는미반영). 프로필자산설정과이름있는프리셋저장은별개/기존171키·사용자프리셋자동변경없음. fx.enabled/OnFlash/user.flash·기존광학접근성/상한존중. [전체수치·조절·사용자검수](FEVER_EXIT_PROJECTED_SCORE_20261026.md). 육각타이밍은이번검토·조정대상아니다.

## 이력 — D-041 적용 기본값·피버 종료 조절 (2026-10-26)

기존 개발자 도구→육각 회전·공격 타이밍: 기본속도10경로/초(실제hex.attack=.1초), 생성10%, 충전1 .95박→60%/.2박, 충전2 1.95박→100%/.2박, 변형2.8박/길이.3125박, 발사3.15박. 회전1배/Outside1 -60도 이동+정지1박=.8초. 명목2.24~2.49변형/2.52발사/2.62도달. 옛저장프리셋은자동변경하지않으므로기본시험은‘이 타임라인 기본값 복원’후새노드로확인한다. 진행중노드는캡처값유지/저장은이름있는프리셋/재실행후명시불러오기다.
피버종료는 **Assets/Resources/CursorDash/VisualPrototype.asset**의Fever exit그룹에서조절한다. appear.18/absorb.55/settle.22=총.95초,spin1260도초/감쇠.10/수축.10/곡선bend.28/가속power2.6/twist.90/RGB.0055/shake.0035/lens.009. 같은종료시계/다음종료캡처이며프리셋171키에중복추가하지않았다. 링등장→회수→마지막압축동기화→복원. 실제피버판정/공급8초와시각종료시간은별개다.
회수중표시는무판정/무보상,고유수명2.8초/퇴장.55는유지·라운드잔여시간선행축소만제거했다. FX접근성/상한을존중하며접촉위치진단이켜져있으면왜곡·RGB는우회한다. 사용자Play검수는[값표/씬·0·소수·14개/중단·복원절차](HEX_DEFAULTS_FEVER_RECALL_20261026.md). 공개웹은D-038그대로/이번자동Play·빌드/git/배포없음.

## 이력 — D-040 육각 회전·공격 타이밍 (2026-10-26)

기존개발자도구→‘육각 회전·공격 타이밍’. 생성0/60도이동+정지=1연속박자, 기본.8초다. **회전1끝=.5박/2끝=1.5박/3끝=2.5박/4시작=3박**을각도/방향·초와표시한다. 초기생성33.3%,충전실행.5/1.5·목표66.7/100·길이각.15박,변형시작2.5/길이.3125박,발사3박이다. D-039의3끝변형완료는최신변형시작요청으로대체됐다.
기존hex.spin은육각이동·정지전체속도,hex.morph는박자길이로재사용한다. hex.attack은기존도달초를저장하고화면에는1/초=경로초속도로편집한다(16.667→도달.06초,25→.04초). 둘은독립이며새속도키/도구없음. 각시점/길이는소수·초환산,다음생성만적용한다. 순서충돌은경고/새생성보류이며자동발사지연없음.
검수표시hex.timelineDebug기본OFF/Dev만,현재박자·충전·단계·다음사건을표시한다. 임시변경은기존프리셋저장으로지속되며재실행후명시적으로불러온다. ‘타임라인 기본값 복원’은해당13항목만복원/저장본보존. JSON171키/defaultsVersion hex-timeline-v1-20261026,구141/157/161은morph초→박환산. [13항목/초기표·저장·구버전·검수](HEX_TIMELINE_TOOLS_20261026.md). Unity/브라우저/모바일실화면검수는대기,이번자동Play/빌드/git/배포없음.

## 이력 — D-039 4박·회전1800·피버종료/배경 (2026-10-26)

현재로컬은[HEX4_FEVER_EXIT_20261026.md](HEX4_FEVER_EXIT_20261026.md)의값을우선한다. 아래D-024이하‘현재’표기는과거시점의이력이며새요구가아니다. 공개D-038은재배포하지않았다.
- 육각 첫링60도/.4이동+.4휴지: 생성1/3→1끝2/3→2끝완충→3끝변형완료→4시작발사. `hex.spin`은이동속도와박자를함께변경/휴지유지, `hex.morph=.25`는셋째이동안의상한이다. `preFireDuration`저장값은이력이며현재대기는Rotator실제interval을사용한다. `hex.prepare=3.5`는두링회전없음대체시간이다.
- `player.turn`기본 **1800도/초**. PlayerCharacterController의공유상수→실제FixedUpdate와도구정의가사용한다. 옛씬Inspector540은미사용/씬수정없음. 명시적Dev프리셋은보존되므로새기본검수때player.turn1800을확인한다. 다른이동값/조작기준은유지.
- `VisualPrototype.asset`: backgroundSpeed .22/Mix .84/Bend .30/HitSpeed2.6/HitRecovery.38/HitColor.65/HitShift.075/circuitRestContrast.62. 같은프로필의승인피버파편/공급값은변경하지않았다. 배경색세부팔레트/영역경계는LivingBackground.shader다.
- FeverFlowMath/FeverFlow.cginc: 기존종료.95초중진행.06까지0게이지표시, u=(p-.06)/.84,가속이동.18u+.82u^2.2/진행.90까지회수완료. 피버판정/공급8초불변. 점선재등장진행.56에기존FeverRingSpin.Begin1회/24도초로감쇠한다.
- 새슬라이더패널·새프리셋저장/초기화/그림은없음. 실제GPU/모바일·사용자Play검수대기. 노드겹침배치·새Flash그림/사이버재구축은보류.

## 현재 — D-024 다음 테스트 조정 (2026-10-25)

승인 형상·육각 파괴는 유지한다. 둘째 육각 테두리는 기본색RGB .82배(기준#FFD466→약#D1AE54), 첫째는 그대로다. 다른 색/형상/공통 효과 기본값과161키 JSON 계약은 바꾸지 않았다.
육각 신규 Inspector `preFireDuration=.08`, `preFireRecoilRatio=.08`은 1차 검수용이며 .05초 당김/.03초 정지, 기본 약.02944월드다. 준비3.5/변형.25 이후 발사 시작이 명목3.83초가 되며 기존 도달.25/반환.06을 유지한다. 이 신규2항목은 현재 Inspector 전용이며 기존161키 내보내기에는 추가하지 않았다. 프리팹/씬 저장값을 일괄 변경하지 않았다.
바깥 넥서스 리듬은30/45/60도·이동.24/.28/.30·휴지.22/.34/.16초다. 기존 방향/시작 시 nexus.spin 배율을 따른다. 다른 테두리/노드 회전은 유지한다. 원형 VFX는 기존 fx.circleRemove.*를 사용하며 기본8개·최대12개, 주 파편2개/지수 감속은 원형 전용이다. 기존 육각 프리셋/파괴 경로·공통 셰이크/왜곡은 바꾸지 않았다.
사용자가 Unity Play→훈련 후 **Tools→CursorDash→넥서스 수동 검수**에서 Canvas/카메라·실제 경계 오차·합성 호출·커서 연결·바깥 휴지/현재 육각 예비 상태도 볼 수 있다. 창은 스크롤 가능하며 자동 Play/저장/빌드를 하지 않는다. [검수 전체 순서](TEST_POLISH_20261025.md).

## 현재 — D-023 옅은 육각 한 겹·넥서스 빠른 HP 절삭 (2026-10-25)

승인 노드 형태/동작을 유지한다. 육각 둘째 테두리만 기본색에 흰색18% 혼합이며 첫째는 기존색이다. 타격감은 미승인이고 원형/육각 파괴 수치·공통 효과 강도는 바꾸지 않았다.
넥서스 수위는 HP 비율을 즉시 표시한다. `nexus.waveAmplitude` 기존 키는 **피격 순간 수위 눌림**으로 재사용하며 기본.015/상한.025, .12초에 단조롭게 사라진다. 평상시 파도·사인파는 없다. `nexus.smooth`와 `nexus.waveSpeed`는 이전 JSON 호환용/미사용이며 큰 값을 가져와도 느린 배수나 자동 파도를 재개하지 않는다. 161키/기존 기본값/저장 프리셋은 보존했다. 별도 타격감 프리셋이나 새 게임 슬라이더는 추가하지 않았다.
Unity에서 **Tools → CursorDash → 넥서스 수동 검수** 창을 연 뒤 사용자가 Play→훈련에 진입하면 최대/중간/낮음/0·단발/연속피해·Retry를 직접 시험할 수 있다. 기존 훈련 무적을 끄면 피해 버튼이 활성화된다. 버튼은 현재 훈련 라운드의 기존 API만 호출하고 저장/빌드/Play 시작을 하지 않는다. [상세 검수·내부호출과 실제 공격 구분](NEXUS_PRESENTATION_20261025.md).
이전 웹 링크는 변경 전 빌드다. 이번에는 소스만 반영했으며 실제 Unity 검수는 사용자 담당이다.

## 현재 — D-022 육각 A 외형만 복원 (2026-10-25)

원형 외형은 사용자 승인으로 유지한다. 타격감은 **미승인**이며 이번 수치/효과 확장을 하지 않았다. 육각 기본 노랑#FFD466·공격3.5/.25/.25/.06초·최종 반경.8/짧은 변.18·전투 규칙/161키/기존 프리셋은 유지한다. 새 슬라이더/프리셋을 추가하지 않았다.
새 기본 충전 외형은 원본 A 반경.555349/외곽선.071768/충전비1.014927을 사용한다. 기존 `visual.outline=.035`에 비례해 A 외곽선을 그리고 변형 종료 시 기존 최종 두께로 이어진다. 이 공유 수치를 바꾸면 승인 원형에도 적용되므로 외형 검수 때 임의 조정하지 않는다. 회전 테두리 2겹은 `hex.spin` 배율과 기존 두 Rotator의 반대 방향을 따르며 둘째 명도만.82배다. 원형/타격감 기본값은 이 작업에서 바꾸지 않았다.
Unity 소스에만 반영했다. 기존 웹 미리보기/공개 주소는 새 버전이 아니다. [정적 비교·변경 파일·Unity 검수](HEXAGON_ORIGINAL_A_20261025.md)를 따른다. 아래 D-021 타격감 설명은 현재 적용 코드 설명이며 사용자 만족/승인을 뜻하지 않는다.

## 현재 — D-021 원형만 최소 수정 (2026-10-25)

이번 버전은 **Unity 소스에만 반영**됐으며 자동 Play/새 WebGL 빌드/웹 검수는 하지 않았다. 기존 링크8879/8880/공개 게임에 반영된 것으로 판단하지 않는다. [원형 검수 절차/파일/도형 비교](CIRCLE_ORIGINAL_A_20261025.md).
공격 기본값.15변형/.05반동/.20돌진/.60배는 그대로다. 원형 제거만 stop.05초,shake.20월드/.35초/20Hz,렌즈.045UV/반경2.2/2초/recovery2로 조정했다. `fx.circleRemove.*` 기존 항목을 재사용하며 새 슬라이더는 없다. `visual.outline`은 원형 본체 외곽선(.035로컬), 회전 원호는 원본 윤곽 데이터, 충전색은A원본#CB49FF/외곽#E87CFF, `circle.color`는 기존 최종 돌진 커서색이다.
기존 Background_Flash의 **저장 alpha87은 바꾸지 않는다**. 원형 타격 시 .25초 동안 기존 그림의 알파만 공간 마스크로 가산한다. 밝기 배율은 기존 `visual.impactExposure`(.65)/`user.flash`/`OnFlash`, 렌즈는 `user.distortion`/`fx.enabled`를 따른다. `visual.rippleStrength/Speed/Width`는 이번 원형 렌즈에는 적용하지 않으며 기존 육각 파동용으로 유지한다. `fx.circleRemove.recovery`2는 OutCirc 복귀이며 다른 값은 곡선의 지수를 바꾼다.
기존 **strong/baseline/저장·가져온 프리셋**은 여전히 이전 원형 수치를 명시 적용한다. 새 기본값과 비교하려면 먼저 현재 세션을 다른 이름으로 내보낸 다음 **원형 제거 타격 그룹만 복원**하거나 [원형 전용 changes JSON 예시](examples/circle-originalA-feedback.json)를 선택해서 가져온다. 저장 프리셋/일반 설정/기록을 삭제할 필요가 없다. 이 JSON은 파일로 작성한 예시이며 실제 게임에서 내보내기·가져오기 검증한 결과는 아니다. `visual.impactExposure=0`/섬광 끔이면 밝기 반응이 보이지 않는 것은 기존 설정 영향이다.

## 최신 수정 — D-019 노란 육각·작은 파편·전체 파동 (2026-10-24)

[최신 로컬 시안8880](http://127.0.0.1:8880/) / buildId `nodefx-20261025-565495e82312` / [시각50키JSON](examples/feedback2-strong-visual.json) / [실제 보고서](examples/feedback2-strong-report.txt) / [결과·전후 화면](FEEDBACK_REVISION_20261024.md).

현재 후속은 기존 일반 게임에도 새 성공 피드백을 적용한다. 공격·반환 시간은 유지한다. 육각은 충전부터 반환까지 기본 노란색이며 독립 중앙점은 기본0이다. 기존 명시적 색/중앙점 Dev 값은 저장 호환을 위해 유지한다.
원형 파편의 기본 크기.05가 실루엣 때문에1로 바뀌던 오류를 수정했다. 크기 key는 월드 단위를 유지하고, 원형 조각의 기본 크기 인자는 원본 실루엣 반경의24% 이내로 제한한다(불규칙 꼭짓점 반경 자체가 정확히24%라는 뜻은 아니다). 큰 값이 남은 이전 JSON도 거대한 파편으로 터지지 않는다. 파편 경계는 AA를 적용하고 이동하며 작아져 사라진다.

| 항목 | 현재 기본 원형 / 육각 반환 | 강렬한 노드 시안 원형 / 육각 반환 |
|---|---|---|
| 히트스톱 | .065 / .075초 | .075 / .085초 |
| 흔들림 | .15 / .18월드, .22 / .25초 | 같음 |
| 렌즈·파동 왜곡 | .045 / .050 UV, 반경2.2 | .055 / .060 UV, 반경2.4 |
| 출렁임·복귀 | .50초 | .50초 |
| 파편 | 8개·.05월드 / 10개·.85실루엣배율 | 10개·.045월드 / 12개·.9실루엣배율 |
| 중앙점 | visual.core=0 | 0 |

네 가지 새 key를 포함해161개다. `visual.rippleStrength`=.8(0~1.5배), `visual.rippleSpeed`=6(1~12화면 짧은 변/초), `visual.rippleWidth`=.35(.10~1화면 짧은 변 비율), `visual.impactExposure`=.65(0~1강도). 모두 **노드 그래픽** 그룹/다음 타격이며 기존 JSON/보고서/요청·실제 UI를 공유한다. 배경 밝기는 사용자 섬광 배율/기존 OnFlash, 왜곡은 사용자 왜곡 배율을 따른다. 동시 상한/누적 상한과 UI 분리는 유지한다.
**CP14 타격감 비교 기준** 버튼은 이전 약한 수치와 파동/밝기0을 명시 적용하며, 그래픽이나 공격 규칙을 되돌리지는 않는다. **강렬한 노드 시안**은 새 강한 프로필이다. 두 버튼은 세션 전용이며 사용자 저장 프리셋을 덮어쓰지 않는다. 일반 게임 새 시각 기본값과 미적 확정은 구분한다.
정확한 이전141/157키 full은 누락된 새 키만 기본 보완한다. 기타 full 누락/범위 오류는 계속 원자적으로 거부한다. 예전 프리셋의.1 중앙점/다른 색은 명시적 오버라이드이므로 다시 불러오면 표시될 수 있다. 중앙점0/노란색의 현재 기본 세션에서 비교하거나 해당 값만 변경해 새 이름으로 저장한다. 아래 D-018 및 CP14 표는 이전 버전 이력이다.

## 노드 그래픽·강렬한 타격감 시안 — D-018 (2026-10-24)

이 시안은 **로컬 전용**이며 아래 공개 CP14에는 아직 없다. `STATUS` 최상단의 로컬 빌드/검증 결과를 따른다. 원형.15/.20/.60 및 반동.05, 육각3.5/.25/.25/.06초는 유지한다.

[현재 로컬 시안](http://127.0.0.1:8879/) / buildId `nodefx-20261024-a4818b5046e2`. [전후 화면·검증 결과](NODE_GRAPHICS_RESULTS_20261024.md), [강한 시각46키 JSON](examples/nodefx-strong-visual.json), [비교 기준 JSON](examples/nodefx-baseline-visual.json), [실제 웹 보고서](examples/nodefx-strong-report-ko.txt).

훈련 → 개발자 도구 → **강렬한 노드 시안** → 닫기·재개 → 원형 접촉 또는 레이저 방어 재현 순서로 확인한다. **CP14 타격감 비교 기준**은 출발점의 수치로 돌아가는 세션 프리셋이다. 두 버튼은 공격 수치/사용자 효과 배율/저장 프리셋을 바꾸지 않는다. 그래픽은 두 프리셋 모두 새 도형이다. 원래 이미지와 비교하려면 이번 전후 GIF를 사용한다.
시안을 보존하려면 별도 이름으로 현재 저장 또는 전체/변경 JSON 내보내기를 선택한다. 자동 저장·자동 활성화·기본값 승격은 하지 않는다. 개발 수치 전체 복원은 세션 오버라이드를 해제하며, 새 그래픽 자체를 과거 이미지로 돌리는 기능은 아니다.

| 사건 | 기준 → 강렬한 시안 | 단위/시점 |
|---|---|---|
| 원형 제거 정지 | .04 → .075 | 실제 초 / 다음 타격 |
| 육각 반환 파괴 정지 | .05 → .085 | 실제 초 / 다음 타격 |
| 원형/반환 셰이크 | .055/.065 → .15/.18 | 월드 / 다음 타격 |
| 셰이크 지속 | .14/.14 → .22/.25 | 실제 초 / 다음 타격 |
| 원형/반환 왜곡 | .008/.010 → .055/.060 | 화면 UV / 다음 타격 |
| 왜곡 반경 | .14/.14 → 1.05/1.15 | 화면 짧은 변 비율 / 다음 타격 |
| 왜곡 지속 | .18/.18 → .26/.28 | 실제 초 / 다음 타격 |
| 파편 수 | 6/6 → 10/12 | 개 / 다음 타격 |
| 파편 크기 | .05/1 → .105/.9 | 원형 월드 / 육각 실루엣 배율 |
| 파편 속도·수명 | 1.5·.3 / 1.5·.3 → 2.4·.32 / 2.8·.34 | 월드/초·실제 초 |

방어 정지0·약한 효과와 넥서스 기존 값/경로는 그대로다. 공통 정지 상한.1초/최소 진행.04초, 셰이크.2월드/.35초, 왜곡 슬롯4(최대8), 파편 묶음16을 유지한다. 성공 왜곡 합산 상한은 별도.06 UV이고 기존 넥서스/방어의.025 상한은 유지한다.

신규16키(총157개)는 기존 검색/그룹/요청·실제/JSON/보고서에 포함된다.

| key | 기본 / 범위 | 단위 / 적용 |
|---|---|---|
| visual.outline | .035 / .01~.12 | 로컬 / 즉시 |
| visual.core | .10 / .03~.20 | 로컬 반경 / 즉시 |
| visual.glow | 0 / 0~.15 | 별도 약한 발광 알파 / 즉시 |
| visual.hexShortEdge | .18 / .03~1 | 짧은 변/긴 변 비율 / 다음 생성 |
| visual.energyCurve | 1 / .25~3 | 뒤→앞 에너지 소모 지수 / 다음 생성 |
| visual.successDistortionCap | .06 / 0~.08 | 합산 UV 상한 / 다음 타격 |
| fx.circleRemove.frequency / fx.triangleBreak.frequency | 26 / 5~45 | Hz / 다음 타격 |
| fx.circleRemove.rebound / fx.triangleBreak.rebound | .35 / 0~1 | 되튕김 배율 / 다음 타격 |
| fx.circleRemove.recovery / fx.triangleBreak.recovery | 2 / .5~4 | 왜곡 복귀 지수 / 다음 타격 |
| fx.circleRemove.spread / fx.triangleBreak.spread | 65 / 0~85 | 진행 방향 ±도 / 다음 타격 |
| fx.circleRemove.spin / fx.triangleBreak.spin | 180 / 0~720 | 파편 도/초 / 다음 타격 |

강렬한 시안은 원형/반환 frequency22/24, rebound.55, recovery1.4, spread55/65, spin220/260이다. FX 전체/파편 토글과 사용자 흔들림/왜곡/섬광/음량 옵션을 존중한다. 볼륨.8·음높이1은 이번에 올리지 않았다.
기존 `fx.triangleBreak.*`는 계속 같은 호환 키이며 화면 이름만 **육각 반환 파괴**다. `hex.radius`는 계속 로컬 외접 반경.8이다. 짧은 변 비율을 바꿔도 그 의미를 반경 대신 폭으로 재해석하지 않는다.
schema1/defaultsVersion은 유지하고 정확한 기존141키 full은 새16키 기본 보완 후 가져온다. 임의로 누락된 full·알 수 없는 키·범위 오류는 계속 거부한다. 새157키 프리셋은 옛 공개 CP14에서 지원하지 않으므로 이번 로컬 시안에 가져온다. 기존 사용자 저장소를 초기화할 필요는 없다.

**가져오기는 changes도 현재 Dev 세션을 교체한다.** 시각46키 파일에 없는 다른 Dev 실험값은 기본/일반 설정으로 돌아갈 수 있다. 그 실험값을 유지하며 시각값만 비교하려면 두 내장 시안 버튼을 사용한다. 파일로 전체 현재 세션을 전달하려면157키 전체 JSON을 내보낸다. 이번 새 웹에서 텍스트 가져오기/내보내기와 이름 저장→새로고침→불러오기를 확인했다. 보고서 생성/텍스트 전체 선택과 **Windows 실제 클립보드 비교도 PASS(줄바꿈 정규화)**다. 최초 추가 읽기 거부 이후 사용자의 명시 승인으로 정상 경로에서 화면 보고서와 로컬 비교했고 LF/CRLF 외 동일함을 확인했다. 클립보드 원문은 출력/저장하지 않았다. 모바일 복사 권한·시스템 붙여넣기는 실기기 검수 항목이다. 상세 이력은 결과 문서 H에 있다.

**공개 테스트 플레이: https://edan2251.github.io/renewal_CursorDash/** (D-017, 2026-10-24).
훈련/설정 → 개발자 활성화 → 수치 변경 → 안전 재생성·시험 → 변경 보고서/JSON 전달 순서다. [배포 검증 결과](D:/renewal_CursorDash/docs/CursorDash/DEPLOYMENT_CP14_20261024.md).
localhost와 github.io 저장은 별개다. 옮길 프리셋은 JSON으로 내보내고 공개 주소에서 가져온다. 사용자 저장소 전체 삭제는 필요하지 않다.
현재 넥서스 왜곡의 메타 기본.006과 미지정 실행 fallback.008 차이는 보고서에 함께 나타날 수 있다. 사용자가 새로 편집한 값인지 구분해 전달하고, 배포 기본값 정합성 수정은 별도 후속 작업으로 남긴다.

사용자 확정 원형: 흡수·변형 **0.15초**, 전진 **0.20초**, 변형 커서만 **0.60배**.
반동0.05초·최종 월드 길이8%, 부드러운 곡선, 반환0.06초, 사건별 타격감은 시안용 임시 선택이다.
원본 에셋/배포 기본값으로 자동 승격하지 않는다. 최종 실행 결과와 빌드 위치는 STATUS 최상단을 따른다.

최종 buildId: `cp14-20261024-031826-fe9e3b0bcb8d`, defaultsVersion: `cp14-v2.1-20261024`.
[루프백 실행](http://127.0.0.1:8765/) · [전체 결과/화면](D:/renewal_CursorDash/docs/CursorDash/CP14_RESULTS.md).
실제 내보내기 예시: [전체 JSON](D:/renewal_CursorDash/docs/CursorDash/examples/cp14-full.json), [변경 JSON](D:/renewal_CursorDash/docs/CursorDash/examples/cp14-changes.json), [한국어 보고서](D:/renewal_CursorDash/docs/CursorDash/examples/cp14-changes-ko.txt).
예시의 전진0.45초는 파일 왕복 검증용 실험이다. 배포 기본0.20초/사용자 확정값과 구분한다.

## 열기 → 수정 → 시험

1. 메인 상단 **훈련** 또는 **가이드**로 들어간다. 실제 Cyberpunk 씬의 이동·잔상·노드 풀·전투 코드를 그대로 사용한다.
2. 첫 시작은 이동 → 원형 생성 후 접촉 제거 → 육각 생성 후 실제 잔상 방어·반환 파괴 순서다. 완료하면 같은 장면에 자유 훈련으로 남는다. 완료/건너뜀/미완료는 별도 저장한다. 미완료는 강제 반복하지 않으며 상단 가이드로 언제든 재입장한다.
3. **설정**에서는 조작 반경·데드존·반응, 커서/잔상 색(가시성·대비), 시각 강도·품질·음량만 바꾼다. **사용자 설정 저장**은 새 v2 키에만 기록한다. 기존 BGM/SFX/최고기록/업적 데이터는 초기화하지 않는다.
4. 설정 또는 훈련의 **개발자 활성화**를 명시적으로 선택한다. DEV·프리셋 이름·기록 제외 표시가 나온다. 저장된 개발자 프리셋은 자동 활성화하지 않는다.
5. 그룹 선택/검색/변경만/즐겨찾기로 좁힌다. 숫자 직접 입력+적용, 슬라이더, ±, 토글, RGBA, 제한된 곡선을 사용한다. 요청/실제 값과 적용 대기 표시를 비교한다.
6. 패널을 열면 전투가 멈춘다. 스크롤·숫자 입력은 게임 조작이 아니다. 닫은 뒤 새 터치/마우스 제스처부터 이동한다. 사용자 일시정지·개발자 정지·히트스톱은 별개이므로 패널 닫기로 다른 정지를 풀지 않는다.
7. 다음 생성 값은 현재 공격에 끼어들지 않는다. **안전 재생성·시험**은 현재 노드/잔상/효과를 명시적으로 지우고 같은 종류·위치의 노드를 새 값으로 다시 생성한다. 잔상 풀 용량도 다시 계산한다. 다음 라운드 값은 **새 라운드**에서 적용된다.
8. 훈련의 원형/육각 단일·반복 생성, 정리, 무적/HP 회복/현재 HP, 개발자 시간 배율·정지로 관찰한다. HP/활성 노드/잔상 용량/최근 사건/FPS/왜곡 렌더 프레임을 함께 본다. HP 0은 실제 종료 경로다. 실제 피격 원 표시는 Dev의 시험 그룹에서 켠다.
9. **원형 접촉 재현 (내부 생성)**은 현재 플레이어 위치에 원형을 생성해 실제 물리 접촉을 사용한다. **레이저 방어 재현 (내부 잔상)**은 실제 빔 경로에 기존 RecordMovement로 잔상을 남겨 같은 방어 판정을 거친다. 이 두 조작을 실제 마우스/터치 검증으로 집계하지 않는다. 사용자 정지/재개는 개발자 정지와 별도 버튼이다.

기록 자격은 라운드 내에서 되돌아오지 않는다. 훈련/Dev/규칙 변경 후 값을 복원하거나 Dev를 꺼도 기록 제외다. **기본 정규 게임**은 Dev를 끄고 새 라운드로 복귀한다. 개발자 진입은 인증/치트 방지 장치가 아니다.

## 프리셋 · JSON · 변경값 전달

- 프리셋은 이름1~40자, 최대8개/총512KiB. 이름 저장·목록 불러오기·새 이름 복제·선택 삭제가 가능하다. 손상/실패/초과는 오류로 표시하고 기존 저장소를 전체 초기화하지 않는다.
- 삭제를 누르면 패널 안에 정확한 프리셋 이름과 **이 프리셋 삭제 / 삭제 취소**가 나온다. 브라우저 기본 확인창을 사용하지 않는다. 선택한 로컬 프리셋만 대상으로 한다.
- 웹 v2 값은 해당 브라우저/주소의 **localStorage**, Editor는 별도 PlayerPrefs v2 키다. 기존 Unity Web PlayerPrefs/IndexedDB와 구분한다. 계정 동기화/기기 백업이 아니다. 주소/포트가 달라지면 별도 저장소다.
- **전체 JSON** 또는 **변경 JSON** → **파일로 내보내기**. 막히면 HTML 텍스트 영역을 전체 선택해 복사한다. 파일 선택/붙여넣기 후 **텍스트 적용**은 64KiB/스키마/기본값 버전/허용 key/타입/유한값/범위를 전체 검증한 후 적용한다. 실패한 가져오기는 현재 정상값을 부분 변경하지 않는다.
- schemaVersion/buildId/defaultsVersion/presetName/createdAt/kind/values를 포함한다. buildId는 소스 내용 해시와 시각을 포함하며 Git HEAD만으로 식별하지 않는다. 전체/변경분 구분은 kind다. 날짜는 문자열로 검증한다.
- **변경 보고서** → **클립보드 복사**. Promise 성공 때만 완료 표시한다. 권한/보안/미지원 실패는 오류와 **텍스트 전체 선택** 대안을 제공한다. [Clipboard API 제약](https://developer.mozilla.org/en-US/docs/Web/API/Clipboard/writeText).
- 보고서는 출하 메타데이터 기본값→현재 요청값/단위/적용 시점/현재 유효값이다. 기존 사용자 음량 등 기본값과 다른 상속 설정도 포함될 수 있다. 이것만으로 새 실험 변경이라고 단정하지 않는다.
- 같은 JSON을 Editor Play의 개발자 패널 텍스트 영역에 붙여넣고 가져오면 동일 allowlist로 비교한다. 원본 .asset/.prefab/.unity는 저장하지 않는다.
- 기본값 반영 요청은 내보낸 JSON/보고서와 원하는 변경 범위를 전달한다. 명시적인 다음 작업에서 diff와 회귀 검증을 거쳐 반영한다. 이번 모바일 저장은 배포가 아니다.

브라우저 연결은 프로젝트 내부 [.jslib 경로](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/interactingwithbrowserscripting/web-interacting-browser-js)를 사용한다. 기존 Unity [PlayerPrefs 저장 방식](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerPrefs.html)은 새 localStorage 프리셋과 별개로 보존한다.

## 적용 범위와 지원 제한

- 넥서스 경계: 빛 번짐을 뺀 본체 중심(0,0), Editor 반경 약0.325520843. 첫 CanvasScaler 레이아웃 후 한 번 확정하며 그 전에는 전투가 진행되지 않는다. 최종 앱 브라우저에서는 현재 표시 반경과 피격 반경 모두0.32422였다(화면 크기에 따른 UI 픽셀 정렬 차이). UI/카메라 흔들림/수위 물결은 판정을 바꾸지 않는다. 본체 비율/최대HP 변경은 다음 라운드다.
- 원형 돌진은 본체 앞점 연속 교차 판정이며 피해1회. 충전/변형/반동은 접촉 제거 가능, Fired는 제거·잔상 방어 불가다. 전진0.20초와 반동 시간은 별개다.
- 육각은 제자리에서 여섯 꼭짓점의 변형 육각으로 변해 빔을 쏜다(D-018; 옛 CP14는 삼각형). 보이는 빔 끝점이 접촉점에서 앞 짧은 변의 발사구로 돌아온다. 논리 반환/파괴 시간은 기존.06초다. 빔 길이는 생성 위치와 공통 경계로 계산하므로 독립 길이 슬라이더는 제공하지 않는다. 표시 폭과 실제 방어 폭은 별개다.
- 파편/왜곡/소리는 사건별 값이다. 왜곡은 Built-in 카메라 실제 샘플링이고 UI 카메라는 제외한다. [OnRenderImage 지원 범위](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnRenderImage.html). SRP로 교체하는 프로젝트 변경은 지원 범위 밖이다.
- 추가 효과/파편 토글은 HitFeedback 효과에 적용된다. D-018 원형 접촉은 단일 방향 파편 사건으로 통합했고, 넥서스 돌진 충돌의 기존 연출은 별도로 유지한다.
- 패턴 선택은 기존 메인/서브/양쪽 조합이다. 고정 seed는 생성 위치/흔들림과 패턴 선택만 고정하고 파편/카메라 난수는 제외한다. 새 생성 비율 기믹은 만들지 않았다. 최대 활성값은 실제 기존 종류별 풀 잔량에도 제한된다.
- BGM 재생 비활성은 기존 코드의 상태다. BGM 음량을 켜도 새 음악 재생 기능을 추가한 것으로 표시하지 않는다. SFX와 사건별 음량/음높이는 실제 소스에 적용한다.
- 범용 곡선·씬 편집기/패키지/설치/인증·원격 실행은 없다. 개발자 기능은 플레이어 안의 명시적인 allowlist만 사용한다.

## 모바일 수동 검수

실제 휴대폰에서만 확정할 항목: 세로 안전영역·주소창/키보드 변화, 소수 입력 후 스크롤과 새 터치, 여러 손가락 중 UI 터치의 이동 차단, 프리셋 저장 후 새로고침, JSON 파일/복사 대안, 빠른 반환·왜곡의 가독성과 발열/프레임 유지. 데스크톱 반응형 확인과 실제 기기 터치는 구분한다.

## 파라미터 목록

아래 표는 실행 중 `CursorDashTuning.Parameters`에서 생성한다. UI와 같은 key/타입/단위/범위/기본값/적용 시점이다. 원형 Inspector는 기존 필드를 유지하고 Dev 미활성 시 저장된 Inspector 값을 우선 사용한다.

<!-- parameters:start -->
| 그룹 · 항목 / key | 타입 · 단위 | 기본값 | 범위 · 간격 | 적용 |
|---|---|---|---|---|
| 원형 · 원형 충전 / `circle.prepare` | number · 초 | 2.5 | 0.05~20 / 0.05 | 다음 생성 |
| 원형 · 원형 흡수·변형 / `circle.morph` | number · 초 | 0.15 | 0.01~3 / 0.01 | 다음 생성 |
| 원형 · 원형 반동 시간 / `circle.recoil` | number · 초 | 0.05 | 0~1 / 0.005 | 다음 생성 |
| 원형 · 반동/최종 월드 길이 / `circle.recoilRatio` | number · 비율 | 0.08 | 0~0.5 / 0.01 | 다음 생성 |
| 원형 · 반동 곡선 / `circle.recoilCurve` | curve · 0선형/1부드럽게/2빠르게 | 1 | 0~2 / 1 | 다음 생성 |
| 원형 · 원형 전진 시간 / `circle.dash` | number · 초 | 0.2 | 0.01~5 / 0.01 | 다음 생성 |
| 원형 · 커서 기준 길이 / `circle.length` | number · 로컬 | 1.6 | 0.2~4 / 0.05 | 다음 생성 |
| 원형 · 커서 기준 폭 / `circle.width` | number · 로컬 | 1.05 | 0.1~3 / 0.05 | 다음 생성 |
| 원형 · 변형 커서 배율 / `circle.scale` | number · 배 | 0.6 | 0.1~2 / 0.05 | 다음 생성 |
| 원형 · 형태 변형 곡선 / `circle.morphCurve` | curve · 0선형/1부드럽게/2빠르게 | 1 | 0~2 / 1 | 다음 생성 |
| 원형 · 원형 폭발 유지 / `circle.impact` | number · 초 | 0.4 | 0.05~3 / 0.05 | 다음 생성 |
| 원형 · 원형 피해 / `circle.damage` | integer · HP | 1 | 0~10 / 1 | 다음 생성 |
| 원형 · 커서 색 / `circle.color` | color · RGBA | #AD1FFFFF | #RRGGBB[AA] | 다음 생성 |
| 원형 · 원형 테두리 회전 배율 / `circle.spin` | number · 배 | 1 | 0~4 / 0.1 | 다음 생성 |
| 육각 · 육각 충전 / `hex.prepare` | number · 초 | 3.5 | 0.05~20 / 0.05 | 다음 생성 |
| 육각 · 육각 흡수·삼각 변형 / `hex.morph` | number · 초 | 0.25 | 0.01~3 / 0.01 | 다음 생성 |
| 육각 · 레이저 외곽 도달 / `hex.attack` | number · 초 | 0.25 | 0.01~5 / 0.01 | 다음 생성 |
| 육각 · 반사 반환 / `hex.return` | number · 초 | 0.06 | 0.005~2 / 0.005 | 다음 생성 |
| 육각 · 삼각 파괴 후 풀 복귀 / `hex.destroy` | number · 초 | 1.333 | 0.05~5 / 0.05 | 다음 생성 |
| 육각 · 변형 육각 외접 반경 / `hex.radius` | number · 로컬 | 0.8 | 0.1~3 / 0.05 | 다음 생성 |
| 육각 · 삼각형 방향 보정 / `hex.rotation` | number · 도 | 0 | -180~180 / 1 | 다음 생성 |
| 육각 · 육각 테두리 회전 배율 / `hex.spin` | number · 배 | 1 | 0~4 / 0.1 | 다음 생성 |
| 육각 · 빔 표시 폭 / `hex.visualWidth` | number · 월드 | 0.0657304765 | 0.005~0.5 / 0.005 | 다음 생성 |
| 육각 · 빔 방어 판정 폭 / `hex.hitWidth` | number · 월드 | 0.10112381 | 0.005~0.5 / 0.005 | 다음 생성 |
| 육각 · 육각 피해 / `hex.damage` | integer · HP | 1 | 0~10 / 1 | 다음 생성 |
| 넥서스 · 넥서스 최대 HP / `nexus.maxHp` | integer · HP | 3 | 1~20 / 1 | 다음 라운드 |
| 넥서스 · 넥서스 아트 본체 비율 / `nexus.solidFraction` | number · 비율 | 0.59241706 | 0.1~1 / 0.01 | 다음 라운드 |
| 넥서스 · 에너지 수위 보간 / `nexus.smooth` | number · 초 | 0.25 | 0~2 / 0.01 | 즉시 |
| 넥서스 · 에너지 물결 높이 / `nexus.waveAmplitude` | number · UI 정규화 | 0.015 | 0~0.1 / 0.001 | 즉시 |
| 넥서스 · 에너지 물결 속도 / `nexus.waveSpeed` | number · 위상/초 | 3 | 0~15 / 0.1 | 즉시 |
| 넥서스 · 넥서스 장식 회전 배율 / `nexus.spin` | number · 배 | 1 | 0~4 / 0.1 | 다음 라운드 |
| 효과 품질 · 추가 타격 효과 전체 / `fx.enabled` | toggle ·  | True | false / true | 다음 타격 |
| 효과 품질 · 추가 파편·스파크 / `fx.particles` | toggle ·  | True | false / true | 다음 타격 |
| 효과 품질 · 국소 왜곡 동시 상한 / `fx.distortionSlots` | integer · 개 | 4 | 1~8 / 1 | 다음 타격 |
| 효과 품질 · 왜곡 이동량 상한 / `fx.distortionCap` | number · 화면 UV | 0.025 | 0~0.05 / 0.001 | 즉시 |
| 효과 품질 · 왜곡 패스 해상도 / `fx.renderScale` | number · 배 | 1 | 0.5~1 / 0.05 | 즉시 |
| 효과 품질 · 연속 히트스톱 상한 / `fx.stopCap` | number · 초 | 0.1 | 0.01~0.3 / 0.005 | 다음 타격 |
| 효과 품질 · 히트스톱 사이 최소 진행 / `fx.stopGap` | number · 초 | 0.04 | 0.01~0.3 / 0.005 | 다음 타격 |
| 효과 품질 · 누적 흔들림 상한 / `fx.shakeCap` | number · 월드 | 0.2 | 0~0.4 / 0.01 | 다음 타격 |
| 효과 품질 · 연속 흔들림 지속 상한 / `fx.shakeMaxDuration` | number · 초 | 0.35 | 0.05~1 / 0.01 | 다음 타격 |
| 효과 품질 · 파편 묶음 동시 상한 / `fx.burstCap` | integer · 개 | 16 | 0~32 / 1 | 다음 타격 |
| 플레이어 · 최대 이동속도 / `player.speed` | number · 월드/초 | 5.75 | 1~15 / 0.05 | 즉시 |
| 플레이어 · 가속 / `player.acceleration` | number · 월드/초² | 60 | 1~200 / 1 | 즉시 |
| 플레이어 · 입력 해제 감속 / `player.release` | number · 월드/초² | 18 | 1~100 / 1 | 즉시 |
| 플레이어 · 방향 회전속도 / `player.turn` | number · 도/초 | 540 | 90~1800 / 10 | 즉시 |
| 플레이어 · 화면 이동 여백 / `player.margin` | number · 뷰포트 | 0.08 | 0~0.25 / 0.01 | 즉시 |
| 플레이어 · 커서 표시 크기 / `player.visualScale` | number · 로컬 | 0.5 | 0.1~1.5 / 0.05 | 다음 라운드 |
| 플레이어 · 실제 충돌 폭 / `player.colliderX` | number · 로컬 | 0.75 | 0.1~2 / 0.05 | 다음 라운드 |
| 플레이어 · 실제 충돌 높이 / `player.colliderY` | number · 로컬 | 0.75 | 0.1~2 / 0.05 | 다음 라운드 |
| 사용자 조작 · 조이스틱 반경 / `user.stickRadius` | number · 화면 짧은 변 비율 | 0.12 | 0.04~0.25 / 0.01 | 즉시 |
| 사용자 조작 · 조이스틱 데드존 / `user.deadZone` | number · 비율 | 0.12 | 0~0.5 / 0.01 | 즉시 |
| 사용자 조작 · 작은 입력 반응 지수 / `user.response` | number · 지수 | 1 | 0.5~2 / 0.05 | 즉시 |
| 사용자 효과 · 화면 흔들림 강도 / `user.shake` | number · 배 | 1 | 0~1 / 0.05 | 다음 타격 |
| 사용자 효과 · 국소 왜곡 강도 / `user.distortion` | number · 배 | 1 | 0~1 / 0.05 | 즉시 |
| 사용자 효과 · 섬광 강도 / `user.flash` | number · 배 | 1 | 0~1 / 0.05 | 다음 타격 |
| 오디오 · BGM 음량 (기존 재생 비활성) / `user.bgm` | number · 비율 | 0.5 | 0~1 / 0.05 | 즉시 |
| 오디오 · SFX 음량 / `user.sfx` | number · 비율 | 0.5 | 0~1 / 0.05 | 즉시 |
| 잔상 · 잔상 실제 수명 / `trail.lifetime` | number · 초 | 0.6782609 | 0.05~10 / 0.01 | 안전 재생성 |
| 잔상 · 잔상 실제 판정 폭 / `trail.hitWidth` | number · 월드 | 0.0675 | 0.005~0.5 / 0.005 | 안전 재생성 |
| 잔상 · 잔상 샘플 간격 / `trail.sample` | number · 초 | 0.02 | 0.005~0.2 / 0.005 | 안전 재생성 |
| 잔상 · 실제 샘플 최소 거리 / `trail.distance` | number · 월드 | 0.015 | 0.001~0.2 / 0.001 | 안전 재생성 |
| 잔상 · 잔상 표시 폭 / `trail.visualWidth` | number · 월드 | 0.0675 | 0.005~0.5 / 0.005 | 즉시 |
| 잔상 · 잔상 시작 알파 / `trail.alpha` | number · 비율 | 1 | 0~1 / 0.05 | 즉시 |
| 잔상 · 잔상 중간 알파 / `trail.tailAlpha` | number · 비율 | 0.35 | 0~1 / 0.05 | 즉시 |
| 잔상 · 잔상 감쇠 프리셋 / `trail.fade` | curve · 0선형/1부드럽게/2빠르게 | 1 | 0~2 / 1 | 즉시 |
| 생성 · 패턴 사이 간격 / `spawn.interval` | number · 초 | 1.4 | 0~10 / 0.05 | 다음 라운드 |
| 생성 · 생성 위치 흔들림 / `spawn.jitter` | number · 월드 | 0.35 | 0~1 / 0.01 | 다음 라운드 |
| 생성 · 최대 활성 노드 (기존 풀 한도 내) / `spawn.limit` | integer · 개 | 128 | 1~256 / 1 | 다음 라운드 |
| 생성 · 기존 패턴 선택 / `spawn.pattern` | curve · 0양쪽/1메인/2서브 | 0 | 0~2 / 1 | 다음 라운드 |
| 생성 · 패턴 반복 / `spawn.repeat` | toggle ·  | True | false / true | 다음 라운드 |
| 생성 · 생성·패턴 고정 seed / `spawn.seedEnabled` | toggle ·  | False | false / true | 다음 라운드 |
| 생성 · 생성 seed (효과 난수 제외) / `spawn.seed` | integer · 정수 | 1234 | 0~1000000 / 1 | 다음 라운드 |
| 시험 · 개발자 시간배율 / `debug.timeScale` | number · 배 | 1 | 0.1~2 / 0.05 | 즉시 |
| 원형 제거 타격 · 히트스톱 / `fx.circleRemove.stop` | number · 초 | 0.04 | 0~0.2 / 0.005 | 다음 타격 |
| 원형 제거 타격 · 셰이크 강도 / `fx.circleRemove.shake` | number · 월드 | 0.055 | 0~0.3 / 0.005 | 다음 타격 |
| 원형 제거 타격 · 셰이크 지속 / `fx.circleRemove.shakeDuration` | number · 초 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 원형 제거 타격 · 셰이크 감쇠 / `fx.circleRemove.shakeDecay` | number · 지수 | 1.8 | 0.5~4 / 0.1 | 다음 타격 |
| 원형 제거 타격 · 국소 왜곡 강도 / `fx.circleRemove.distortion` | number · 화면 UV | 0.008 | 0~0.03 / 0.001 | 다음 타격 |
| 원형 제거 타격 · 국소 왜곡 반경 / `fx.circleRemove.radius` | number · 짧은 변 비율 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 원형 제거 타격 · 국소 왜곡 지속 / `fx.circleRemove.duration` | number · 초 | 0.18 | 0.02~0.8 / 0.01 | 다음 타격 |
| 원형 제거 타격 · 전체 섬광 (기본 꺼짐) / `fx.circleRemove.flash` | number · 알파 | 0 | 0~0.12 / 0.01 | 다음 타격 |
| 원형 제거 타격 · 파편·스파크 수 / `fx.circleRemove.count` | integer · 개 | 6 | 1~24 / 1 | 다음 타격 |
| 원형 제거 타격 · 파편 크기 / `fx.circleRemove.size` | number · 월드 | 0.05 | 0.01~2 / 0.01 | 다음 타격 |
| 원형 제거 타격 · 파편 이동속도 / `fx.circleRemove.speed` | number · 월드/초 | 1.5 | 0~5 / 0.1 | 다음 타격 |
| 원형 제거 타격 · 파편 수명 / `fx.circleRemove.life` | number · 초 | 0.3 | 0.05~2 / 0.01 | 다음 타격 |
| 원형 제거 타격 · 사건 음량 / `fx.circleRemove.volume` | number · 배 | 0.8 | 0~1 / 0.05 | 다음 타격 |
| 원형 제거 타격 · 사건 음높이 / `fx.circleRemove.pitch` | number · 배 | 1 | 0.25~3 / 0.05 | 다음 타격 |
| 원형 제거 타격 · 파편·스파크 색 / `fx.circleRemove.color` | color · RGBA | #BF4DFFFF | #RRGGBB[AA] | 다음 타격 |
| 육각 방어 타격 · 히트스톱 / `fx.hexBlock.stop` | number · 초 | 0 | 0~0.2 / 0.005 | 다음 타격 |
| 육각 방어 타격 · 셰이크 강도 / `fx.hexBlock.shake` | number · 월드 | 0.018 | 0~0.3 / 0.005 | 다음 타격 |
| 육각 방어 타격 · 셰이크 지속 / `fx.hexBlock.shakeDuration` | number · 초 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 육각 방어 타격 · 셰이크 감쇠 / `fx.hexBlock.shakeDecay` | number · 지수 | 1.8 | 0.5~4 / 0.1 | 다음 타격 |
| 육각 방어 타격 · 국소 왜곡 강도 / `fx.hexBlock.distortion` | number · 화면 UV | 0.002 | 0~0.03 / 0.001 | 다음 타격 |
| 육각 방어 타격 · 국소 왜곡 반경 / `fx.hexBlock.radius` | number · 짧은 변 비율 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 육각 방어 타격 · 국소 왜곡 지속 / `fx.hexBlock.duration` | number · 초 | 0.18 | 0.02~0.8 / 0.01 | 다음 타격 |
| 육각 방어 타격 · 전체 섬광 (기본 꺼짐) / `fx.hexBlock.flash` | number · 알파 | 0 | 0~0.12 / 0.01 | 다음 타격 |
| 육각 방어 타격 · 파편·스파크 수 / `fx.hexBlock.count` | integer · 개 | 4 | 1~24 / 1 | 다음 타격 |
| 육각 방어 타격 · 파편 크기 / `fx.hexBlock.size` | number · 월드 | 0.05 | 0.01~2 / 0.01 | 다음 타격 |
| 육각 방어 타격 · 파편 이동속도 / `fx.hexBlock.speed` | number · 월드/초 | 1.5 | 0~5 / 0.1 | 다음 타격 |
| 육각 방어 타격 · 파편 수명 / `fx.hexBlock.life` | number · 초 | 0.3 | 0.05~2 / 0.01 | 다음 타격 |
| 육각 방어 타격 · 사건 음량 / `fx.hexBlock.volume` | number · 배 | 0.8 | 0~1 / 0.05 | 다음 타격 |
| 육각 방어 타격 · 사건 음높이 / `fx.hexBlock.pitch` | number · 배 | 1 | 0.25~3 / 0.05 | 다음 타격 |
| 육각 방어 타격 · 파편·스파크 색 / `fx.hexBlock.color` | color · RGBA | #FFD933FF | #RRGGBB[AA] | 다음 타격 |
| 삼각 파괴 타격 · 히트스톱 / `fx.triangleBreak.stop` | number · 초 | 0.05 | 0~0.2 / 0.005 | 다음 타격 |
| 삼각 파괴 타격 · 셰이크 강도 / `fx.triangleBreak.shake` | number · 월드 | 0.065 | 0~0.3 / 0.005 | 다음 타격 |
| 삼각 파괴 타격 · 셰이크 지속 / `fx.triangleBreak.shakeDuration` | number · 초 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 삼각 파괴 타격 · 셰이크 감쇠 / `fx.triangleBreak.shakeDecay` | number · 지수 | 1.8 | 0.5~4 / 0.1 | 다음 타격 |
| 삼각 파괴 타격 · 국소 왜곡 강도 / `fx.triangleBreak.distortion` | number · 화면 UV | 0.01 | 0~0.03 / 0.001 | 다음 타격 |
| 삼각 파괴 타격 · 국소 왜곡 반경 / `fx.triangleBreak.radius` | number · 짧은 변 비율 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 삼각 파괴 타격 · 국소 왜곡 지속 / `fx.triangleBreak.duration` | number · 초 | 0.18 | 0.02~0.8 / 0.01 | 다음 타격 |
| 삼각 파괴 타격 · 전체 섬광 (기본 꺼짐) / `fx.triangleBreak.flash` | number · 알파 | 0 | 0~0.12 / 0.01 | 다음 타격 |
| 삼각 파괴 타격 · 파편·스파크 수 / `fx.triangleBreak.count` | integer · 개 | 6 | 1~24 / 1 | 다음 타격 |
| 삼각 파괴 타격 · 파편 크기 / `fx.triangleBreak.size` | number · 실루엣 배율 | 1 | 0.01~2 / 0.01 | 다음 타격 |
| 삼각 파괴 타격 · 파편 이동속도 / `fx.triangleBreak.speed` | number · 월드/초 | 1.5 | 0~5 / 0.1 | 다음 타격 |
| 삼각 파괴 타격 · 파편 수명 / `fx.triangleBreak.life` | number · 초 | 0.3 | 0.05~2 / 0.01 | 다음 타격 |
| 삼각 파괴 타격 · 사건 음량 / `fx.triangleBreak.volume` | number · 배 | 0.8 | 0~1 / 0.05 | 다음 타격 |
| 삼각 파괴 타격 · 사건 음높이 / `fx.triangleBreak.pitch` | number · 배 | 1 | 0.25~3 / 0.05 | 다음 타격 |
| 삼각 파괴 타격 · 파편·스파크 색 / `fx.triangleBreak.color` | color · RGBA | #FFD466FF | #RRGGBB[AA] | 다음 타격 |
| 넥서스 피격 타격 · 히트스톱 / `fx.nexusDamage.stop` | number · 초 | 0.08 | 0~0.2 / 0.005 | 다음 타격 |
| 넥서스 피격 타격 · 셰이크 강도 / `fx.nexusDamage.shake` | number · 월드 | 0.1 | 0~0.3 / 0.005 | 다음 타격 |
| 넥서스 피격 타격 · 셰이크 지속 / `fx.nexusDamage.shakeDuration` | number · 초 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 넥서스 피격 타격 · 셰이크 감쇠 / `fx.nexusDamage.shakeDecay` | number · 지수 | 1.8 | 0.5~4 / 0.1 | 다음 타격 |
| 넥서스 피격 타격 · 국소 왜곡 강도 / `fx.nexusDamage.distortion` | number · 화면 UV | 0.006 | 0~0.03 / 0.001 | 다음 타격 |
| 넥서스 피격 타격 · 국소 왜곡 반경 / `fx.nexusDamage.radius` | number · 짧은 변 비율 | 0.14 | 0.02~0.5 / 0.01 | 다음 타격 |
| 넥서스 피격 타격 · 국소 왜곡 지속 / `fx.nexusDamage.duration` | number · 초 | 0.18 | 0.02~0.8 / 0.01 | 다음 타격 |
| 넥서스 피격 타격 · 전체 섬광 (기본 꺼짐) / `fx.nexusDamage.flash` | number · 알파 | 0 | 0~0.12 / 0.01 | 다음 타격 |
| 넥서스 피격 타격 · 파편·스파크 수 / `fx.nexusDamage.count` | integer · 개 | 6 | 1~24 / 1 | 다음 타격 |
| 넥서스 피격 타격 · 파편 크기 / `fx.nexusDamage.size` | number · 월드 | 0.05 | 0.01~2 / 0.01 | 다음 타격 |
| 넥서스 피격 타격 · 파편 이동속도 / `fx.nexusDamage.speed` | number · 월드/초 | 1.5 | 0~5 / 0.1 | 다음 타격 |
| 넥서스 피격 타격 · 파편 수명 / `fx.nexusDamage.life` | number · 초 | 0.3 | 0.05~2 / 0.01 | 다음 타격 |
| 넥서스 피격 타격 · 사건 음량 / `fx.nexusDamage.volume` | number · 배 | 0.8 | 0~1 / 0.05 | 다음 타격 |
| 넥서스 피격 타격 · 사건 음높이 / `fx.nexusDamage.pitch` | number · 배 | 1 | 0.25~3 / 0.05 | 다음 타격 |
| 넥서스 피격 타격 · 파편·스파크 색 / `fx.nexusDamage.color` | color · RGBA | #1ACCFFFF | #RRGGBB[AA] | 다음 타격 |
| 육각 · 변형 육각 색 / `hex.color` | color · RGBA | #FFD466FF | #RRGGBB[AA] | 다음 생성 |
| 육각 · 발사 빔 색 / `hex.beamColor` | color · RGBA | #FF66D9FF | #RRGGBB[AA] | 다음 생성 |
| 육각 · 반환 빔 색 / `hex.returnColor` | color · RGBA | #FFF299FF | #RRGGBB[AA] | 다음 생성 |
| 넥서스 · 에너지 색 / `nexus.color` | color · RGBA | #0AB2E6FF | #RRGGBB[AA] | 즉시 |
| 플레이어 · 플레이어 커서 색 / `user.cursorColor` | color · RGBA | #FFFFFFFF | #RRGGBB[AA] | 즉시 |
| 잔상 · 잔상 색 / `user.trailColor` | color · RGBA | #FFFFFFFF | #RRGGBB[AA] | 즉시 |
| 생성 · 훈련 반복 간격 / `practice.repeatInterval` | number · 초 | 2 | 0.25~10 / 0.05 | 다음 생성 |
| 시험 · 실제 넥서스 피격 원 표시 / `debug.boundary` | toggle ·  | False | false / true | 즉시 |
<!-- parameters:end -->
