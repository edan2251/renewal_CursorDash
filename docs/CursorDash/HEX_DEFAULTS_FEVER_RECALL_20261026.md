# D-041 육각 확정 기본값·점선 링 주도 피버 종료 (2026-10-26)

상태: **IMPLEMENTED_STATIC_AND_UNIT_CHECKED / 수정 후 사용자 Unity 검수 대기**.
출처는 최신 사용자 글의 육각 값과 피버 종료 1~7번이다. D-040 도구를 먼저 마무리하고 같은 구조로 종료를 수정했다. 새 외부 참고자료/화면 확인/컴퓨터 UI 조작은 없다. 이번 결과는 로컬 소스이며 공개 D-038은 바꾸지 않았다.

## 1. 육각 기본값 반영

기준은 기존 Outside 1의 **-60도 시계방향 이동+정지=1연속박자**, 생성=0이다. 두 링을 합산하지 않는다. 저장된 이동 .4초+정지 .4초 / hex.spin=1이므로 1박=.8초다. 첫 회전 종료=.5박, 둘째=1.5박, 셋째=2.5박이며 정지 구간까지 포함한 연속 좌표를 사용한다. 이번 입력 .95/1.95/2.8/3.15를 옛 ‘회전 완료 횟수’로 재해석하지 않았다.

|기존 도구 항목/key|D-040 기본 → 이번 기본|생성 이후 명목 시간|
|---|---|---|
|hex.spin|1 유지|1박 .8초|
|hex.attack 화면 속도|16.667 → **10경로/초**|실제 도달시간 .06 → **.10초**|
|hex.bornCharge|33.333 → **10%**|0초|
|hex.charge1.at / target / length|.5 / 66.667% / .15 → **.95 / 60% / .20박**|.76초 시작 → .92초 완료|
|hex.charge2.at / target / length|1.5 / 100% / .15 → **1.95 / 100% / .20박**|1.56초 시작 → 1.72초 완료|
|hex.morphAt / hex.morph|2.5 / .3125 → **2.8 / .3125박**|2.24 → 2.49초|
|hex.fireAt|3 → **3.15박**|2.52초 발사 / 약2.62초 넥서스 도달|

실측 시간이 아니라 실제 저장 회전값과 타임라인에서 계산한 값이다. 변형 길이 .3125박=.25초와 회전속도는 미지정이므로 유지했다. 변형 완료→발사 대기는 .0375박=.03초다. 발사→잔상 접촉은 경로상 위치에 따라 .10초보다 짧으며, 잔상 수명/방어·반사·피해 규칙은 바꾸지 않았다.

도구 정의뿐 아니라 `HexRotationTimeline` 기본값과 `ArrowNode.Execute`의 실제 fallback이 같은 상수를 사용한다. `hex.attack` 저장 단위는 기존 도달초(.1)이고 화면만 역수 10경로/초다. 월드10유닛/초라는 뜻이 아니다. 과거 prefab/scene의 attackDuration=.06은 숨긴 직렬화 이력으로 남겼으며 실제 기본 경로에는 사용하지 않는다. 씬/프리팹을 일괄 저장하지 않았다.

기존 프리셋을 자동 덮어쓰거나 불러오지 않는다. 이전 프리셋을 명시 적용한 세션은 그 값을 유지한다. 새 기본값을 시험하려면 기존 개발자 도구→육각 회전·공격 타이밍→**이 타임라인 기본값 복원** 후 다음 육각을 생성한다. 이미 진행 중인 노드는 기존 불변 캡처대로 끝난다. 저장·불러오기/JSON/단위/순서 경고·발사 자동 지연 금지는 D-040 그대로다.

## 2. 종료 원인과 변경 흐름

이전에는 Exiting 전환 때 bonus.ClearNodes가 먼저 실행돼 실제 노드 위치가 사라졌고, 각 노드도 자신의 수명과 ‘라운드 잔여시간’ 중 작은 값으로 미리 축소됐다. 점선 복귀는 회수 후반의 별도 가속이었다. 이를 **링이 먼저 나타나고 같은 시계로 회수**하도록 바꿨다.

|게이지0 이후 시각 시간|동작|
|---|---|
|0|실제 피버8초 종료/Gauge0. 공급·수집 판정 종료. 남은 노드의 실제 위치·크기·불투명도를 캡처하고 실제 Node를 무보상 Reset. Collider/Node 없는 표시 복사본만 남김|
|0~.18초|흰 공간과 노드 위치는 유지. 두 번째 점선 HpCentral이 회전하며 등장(알파0→1/크기.94→1), 24→1260도/초 가속|
|.18~.73초|고속 회전 유지. 노드가 실제 위치에서 한 번 휘는 2차 곡선으로 링에 접근. 거리/개별 차이로 도착은 약.64~.73초, 마지막은 .73초. 흑백 공간도 실제 넥서스로 굽으며 가속 회수|
|.73초|마지막 영역·노드 흡수와 함께 압력1/색수차·흔들림 정점/링 크기90%. 수축은 마지막 .05초부터 한 번만 진행|
|.73~.95초|링 크기1 복원·회전24도/초로 지수 감쇠(기존 nexus.spin 존중). 광학 압력은 .87초까지0. .95초 정상 복귀/기존 배율 한 단계 상승1회(상한×4)|

총 종료 연출 **.95초 유지**. 피버 판정·점수·공급8초에 더하지 않는다. 실제 게이지0/모델 Exiting이 시작점이며 끝나기 전 색 회수는 하지 않는다. 기본 모델의 종료 대기 구간을 재사용한다.

회수용 복사본은 Quad/기존 FeverBonus 재질/MaterialPropertyBlock만 사용하며 Node·Collider·점수·파편 호출이 없다. 원래의 고유2.8초 수명/마지막.55초 축소는 유지하고, **라운드 종료를 이유로 미리 축소하던 항목만 제거**했다. 이에 따라 마지막.55초 동안 고유수명이 남은 목표가 이전보다 크게 남아 있을 수 있다. 개별 보상·공급간격 .12·상한14·피버8초·점수/×4 규칙은 그대로지만, 이 마지막 구간의 실제 타격 가능성은 달라질 수 있다. 보류된 일반 노드 겹침 배치는 수정하지 않았다.

노드 도착 반경은 원래 HpCentral 표시의 복원 크기에서 기존 좌표 매핑으로 구하며, 각 도착 시점의 링 수축까지 보정한다. HP 수위/실제 넥서스 피해 반경과는 별개다. 검정 원은 초반 형태·크기를 유지하고 마지막 경로 구간에서 축소·소멸한다. 진행 곡선은 .18u+.82u^2.6, 한 번 휘는 경로이며 다회전/장식 띠를 만들지 않는다.

CPU 영역 판별과 공통 FeverFlow.cginc가 같은 흡입 진행·곡선을 사용한다. 실제 NexusPosition을 중심으로 둥근 기존 영역을 회전·수축하고 기존 곡선 에너지 표시를 함께 회수한다. 단순 전체 알파 페이드가 아니다. 마지막 흡수 후 마스크0/광학0, 일반 배경의 기존 시간 흐름은 보존한다.

색수차·렌즈·흔들림은 기존 **커서 포함 최종 게임판 pass**에 한 갈래로 적용한다. 이전 피버 접촉 광학은 종료 진입 때 정리해 중복 강화하지 않는다. 기존 CameraShake를 반복 호출하지 않고 같은 압력/모델 시간으로 화면 좌표 흔들림을 처리하여 Pause 때 정지한다. 안정된 점수·버튼·설정창은 기존 후속 HUD 경로를 유지한다. fx.enabled/user.distortion/user.shake/OnVibe·기존 상한·접촉 진단 우회를 존중한다. 접촉 진단이 켜져 있으면 광학이 의도적으로 안 보인다.

## 3. 조절 위치와 값

Unity Project에서 **Assets/Resources/CursorDash/VisualPrototype.asset** → Inspector의 **Fever exit — one ring-led clock**. 새 관리 시스템/중복 개발자 도구는 없으며 기존 시각 프로필에만 추가했다. 종료 시작 때 한 번 캡처하므로 실행 중 편집은 다음 종료부터 반영한다. 기존 상위 효과/접근성 설정은 계속 적용된다.

|필드|초기값|단위·역할|
|---|---:|---|
|feverExitAppear|.18|초/링 등장·가속|
|feverExitAbsorb|.55|초/노드와 공간 흡입|
|feverExitSettle|.22|초/반동 복원·안정화|
|feverExitSpin|1260|도/초/최대 속도, nexus.spin 및 상한 적용|
|feverExitSpinDecay|.10|초/안정화 구간 지수 감쇠, 종료 시 평상 속도 보장|
|feverExitSqueeze|.10|복원 크기 대비 수축 비율(90%까지)|
|feverExitBend|.28|경로 거리 대비 휘어짐|
|feverExitPower|2.6|가속 곡선 지수|
|feverExitTwist|.90|라디안/흑백 경계의 흡입 굽힘|
|feverExitChromatic|.0055|짧은 화면축 비율/RGB 분리 최대|
|feverExitShake|.0035|짧은 화면축 비율/게임판 흔들림 최대|
|feverExitLens|.009|짧은 화면축 비율/중심 굴절, 기존 변위 상한 이내|

appear+absorb+settle가 전체 시각 종료시간이다. 값 범위/비유한 입력에는 안전한 기본·상한이 있다. Inspector 자산 저장과 도구의 이름 있는 프리셋 저장은 다른 경로다. 이번 새 시각값은 VisualPrototype 자산이 지속 설정이며, 기존 JSON171키에 별도 종료 키를 중복 추가하지 않았다.

## 4. 변경 파일

모두 `Cursor-Dash-master/Assets/` 아래 기존 파일이며 새 런타임 파일/에셋/.meta는 만들지 않았다.

- 육각 기본/실제 fallback: `Scripts/Gameplay/Object/Node/NodeRhythmMath.cs`, `ArrowNode.cs`, `Scripts/Gameplay/StageManager/CursorDashTuning.cs`.
- 회수 시계/표시/풀 정리: `Scripts/Gameplay/StageManager/FeverFlowMath.cs`, `CursorDashFever.cs`, `FeverBonusField.cs`, `FeverNexusRig.cs`, `FeverElementMask.cs`, `Scripts/Gameplay/Object/Node/FeverBonusNode.cs`.
- 기존 프로필/저장값: `Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`, `Resources/CursorDash/VisualPrototype.asset`.
- 영역·최종 광학: `Scripts/Gameplay/StageManager/FeverWorldTransition.cs`, `Resources/CursorDash/FeverFlow.cginc`, `FeverWorld.shader`, `FeverImpact.shader`.
- 기존 표시/격리 미리보기 호환: `Editor/CursorDashNexusReview.cs`, `Editor/CursorDashFeverPreview.cs`, `Scripts/Gameplay/StageManager/CursorDashRuntimeUI.cs`, `WebGLTemplates/CursorDash/TemplateData/cursordash-tools.js`.
- 기존 테스트6파일: `Tests/Editor/CursorDashFeverRulesTests.cs`, `CursorDashFeverPolishTests.cs`, `CursorDashSceneRegression.cs`, `CursorDashFeedbackRevisionTests.cs`, `CursorDashGraphicsTests.cs`, `CursorDashTestPolishTests.cs`.
- 문서: AGENTS 및 STATUS/MASTER_SPEC/DECISIONS/ENVIRONMENT/VALIDATION/SOURCES/TUNING_GUIDE, 이 인계.

25개 게임/도구/테스트 파일+기존문서8개 수정, 이 문서1개 신규. 검사 로그/Before/이번만의 패치는 무시된 로컬 Logs/fever-ring-recall-20261026에 보관한다. 시작 main/HEAD8c357b8와 기존 D-039/040 변경·미추적 파일을 기준으로 비교한다. 기존 사용자 파일 정리/삭제/되돌리기/스테이징 없음.

## 5. 검사 결과와 한계

- **PASS 정적28**: 실제 fallback/같은 회전시계·기존 beam 판정구간/공급·규칙·승인 파편·일반 아트/입력/배경·씬/프리팹/meta/패키지/설정 보존, 회수용 표시의 판정/보상 없음, 모델시계 공유, 효과 상한/종료정리, CPU·셰이더 곡선 연결. 실제 렌더 검사가 아니다.
- **PASS 순수 .NET 2,293단언**: 실제 exit/rules/timeline 소스1,953+실제 Tuning와 명시적 메모리 Unity/저장 어댑터340. 0/1/14개 곡선의 실제 시작/종점·가속·최종 압력/수축 동기화, 다양한 화면비의 수학 영역 잔여0, Gauge0 이후 Exiting/8초 유지·Pause/Abort/Cancel/Reset·완료1회, 소수 타임라인/다음 생성 캡처·프레임 건너뜀·구 프리셋 변환·저장 원자성. 실제 물리/네이티브 저장은 아니다.
- **PASS JavaScript 구문/모의DOM23**: 실제 웹 도구 소스에 현재 C# 매개변수 자료를 전달해 속도10 표시·역수 입력/소수·전용 복원/경고/검수 표시를 검사했다. 실제 웹 브라우저/레이아웃/GPU 검사가 아니다.
- **PASS 참조 C# 컴파일**: 런타임108/테스트12/Editor2 소스 오류0. 런타임 경고134=시작133+보존한 옛 attackDuration 미사용1. 테스트/Editor 경고0. 설치된 .NET/Roslyn과 Unity 참조 DLL만 사용하고 Editor는 실행하지 않았다.
- **공식99개 작성·컴파일, 실행 NOT_RUN**: 기존95개 기대값/비행 설정 경로 갱신+순수3/네이티브 회수·무보상·취소1 추가. 테스트 삭제/무시/핵심 단언 제거 없음. 공식 테스트를 통과했다고 집계하지 않는다.
- **NOT_RUN** Unity실행/실제 Unity컴파일·DOTween/Physics/Shader컴파일/GPU·Play·실제 조작/화면·모바일/저장지속성, 빌드/브라우저/커밋/push/배포/컴퓨터UI. 화면·링 실제 반경/색수차 강도·HUD 결과는 사용자 검수 대기다.

## 6. 사용자 Unity 검수 순서

현재 작업환경에서 사용자가 `Assets/Scenes/00.StartScene.unity` → Play → 훈련으로 들어가면 실제 게임 씬은 `Assets/Scenes/Cyberpunk.unity`다. 이미 진행 중인 육각에는 새 값을 덮지 않으므로 새 생성으로 확인한다. 기존 이름 있는 프리셋이 다르면 타임라인만 기본값 복원한다.

1. 육각: 개발자 도구의 생성10/충전 .95→60/.2,1.95→100/.2/변형2.8·.3125/발사3.15/속도10을 확인. 검수 표시를 켜고 새 육각의 충전 유지·변형 완료 뒤 대기·레이저/잔상 방어를 확인한다. 실제 회전/발사 시간은 `Tools → CursorDash → 넥서스 수동 검수`에서도 읽는다.
2. 자연 피버: 게이지가 완전히0이 된 뒤 링이 먼저 등장·가속하고, 남은 검정 원이 그 위치에서 굽어 들어가는지 본다. 회수 중 커서를 원에 겹쳐도 점수/파괴 파편이 늘면 실패다.
3. 0/소수/상한 검수는 기존 내부 버튼으로 별도 확인할 수 있다(실제 전투 결과와 구분, 훈련 기록 제외). **0개**: Pause 상태에서 검수창 ‘만충’→‘현재 단계만 끝까지’로 Entering→Fever까지 진행하고 공급 업데이트 없이 Fever를 끝낸 뒤 Resume. **소수**: 자연 Fever에서 보너스2~3개일 때 Pause→현재 단계 끝→Resume. **상한**: 가만히 있어 보너스14를 확인한 뒤 같은 절차. 버튼이 단계를 건너뛰므로 자연8초 소진은 2번에서 따로 본다.
4. 마지막 흡수와 링90% 수축·색수차/흔들림 정점이 같은 순간인지, 곧 링1배/평상 회전·화면색/왜곡0으로 돌아오는지 확인. 접촉 진단은 끄고 FX/왜곡/흔들림 설정을 확인한다. HUD는 안정적으로 읽혀야 한다.
5. 종료의 등장/흡입/안정화 각각에서 Pause→Resume. 같은 구간에서 새 라운드마다 Retry/Home/검수창0%로 전환해 표시/마스크·가속/색수차가 남지 않는지, 재시도 입력/시간이 정상인지 확인한다. 최대×4/피격×1·점수/최고점수 구분도 기존대로 유지되어야 한다.

승인 파편 크기·강도·분포/커서, 일반 원형·육각 외형/등장·조준·반사·파괴, 넥서스 최종 파괴·바깥 테두리 정지와 기존 배경 설정은 재디자인하지 않았다. 공개 상단 도구바 겹침·기존 경고/BGM도 이번 대상이 아니다. **보류: 일반 노드 겹침, 새 Flash Background 그림, 사이버 공간 재구축**.

다음 한 작업은 사용자 위 Unity·영상 검수다. 결과 전 새 기능/다음 단계/추가 튜닝·Unity실행/Play·빌드/git/배포를 시작하지 않는다. D-014와 기존 사용자 미저장 상태 보존 규칙을 유지한다.
