# 회로 수정 전 로컬 체크포인트 — D-045 (2026-10-27)

## 기준과 권한

사용자가 현재 로컬본을 회로 수정 전 기준 버전으로 확정했다. 이 문서를 포함한 **[Checkpoint] Preserve CursorDash baseline before circuit changes** 커밋을 기준으로 한다. 시작 브랜치는 main, 이전 HEAD는 `8c357b876f03e805e3a0f0544fbbf1bb00f4bb2d`다. 정확한 새 해시는 Git 이력/완료 보고와 무시된 `Logs/checkpoint-before-circuit-20261027/result.json`에 기록한다. 자기 자신을 포함하는 커밋의 해시를 이 문서에 미리 기입하지 않는다.

이번 승인은 관련 저장 파일의 선별 **로컬 커밋 한 건**이다. Push·빌드·배포·자동 Unity 실행/Play·회로 수정은 하지 않는다. 다음 작업은 사용자의 회로 수정 지시문을 받은 뒤 판단한다. 게임 코드·설정은 이번 기록 작업에서 새로 바꾸지 않았다.

## 보존한 구현

- D-039~041: 기존 요청에 따른 플레이어 방향 회전 기본값1800, 육각 회전 기준 타임라인·기존 개발자 도구/웹템플릿과 저장·경고/다음 생성 적용, 사용자 확정 기본값. 이번에는 다시 튜닝하지 않는다.
- D-041~042: Gauge0/실제 피버 판정·공급 종료 뒤 링 등장→실제 위치의 무판정 표시 노드·흑백 공간 흡입→압축·반동·정상화. 명목 .20/.51/.24초/합계.95초이며 실제 Unity 프레임 실측과 구분한다.
- D-042~044: 기존 점수 시스템의 정확한 총점을 World TMP 배경 숫자로 투사/제한된 획득 탄성/폰트 아틀라스·기존 합성 경로. 최신은 Local Scale1, Rect4×4, 중앙(.5,.5), Auto Size1~3.5이고 점수 반응은 글자 정점에만 적용한다.
- 상단 총점 TMP/legacy 표시는 숨기며 ×2~×4와 Pause는 유지한다. 기존 점수·최고 기록·결과 화면·배율/HP/전투·반사·승인 피버 파편/커서·일반 노드 외형/VFX는 이번에 추가 수정하지 않았다.

## 포함·제외

선별 대상은 게임 소스19, 기존 Editor도구2, 관련 테스트6, WebGL 도구 템플릿2, 셰이더/시각 프로필5, AGENTS1, 개발 문서12 = **47파일**이다. 미추적 인계4개와 이 체크포인트 기록1개를 포함한다. 새 패키지·바이너리·빌드 산출물은 없다.

기존 사용자 `Cursor-Dash-master/Assets/Scenes/Cyberpunk.unity`는 제외하고 디스크 상태를 보존한다. 차이는 옛 `turnDegreesPerSecond: 540` 직렬화 한 줄 삭제이며, 최신 코드가 이미 공유 기본값1800을 사용하는 상황의 기존 사용자 씬 변경이다. 이 체크포인트를 위해 씬을 저장/복원하거나 변경을 폐기하지 않는다.

기존 `_Recovery`와 .meta, Captures, LegacyAssetArchive, 프로젝트 안의 자산정리 문서/manifest, Deployments, _incoming, docs/CursorDash/evidence, tools와 Logs도 이번 커밋에 넣지 않는다. 해당 파일과 사용자 변경은 삭제·이동·되돌리지 않는다. 참고 영상/캡처·진단 결과를 Git에 일괄 추가하지 않았다.

## 확인 범위

- **PASS** 시작 Git 루트/브랜치/HEAD/변경 목록·빈 스테이징 확인, 대상 파일별 관련 범위 확인, 고확률 비밀값 패턴 검사(발견0). 최종 스테이징 목록·형식·파일 보존 결과는 로컬 result.json에 남긴다.
- Git 작업 전 Unity Editor 실행 프로세스는 **0개**였다. 현재 Editor의 미저장 메모리 상태를 임의로 저장·폐기하지 않았고 복구 자료도 보존했다. Unity를 새로 열어 확인한 작업은 아니다.
- 이전 D-044 참조 컴파일 기록: 게임108/테스트12/Editor2 소스 오류0, 런타임 기존 경고134. 이번 체크포인트 작업에서 컴파일/테스트를 다시 실행하지 않았다.
- **NOT_RUN** 이번 Unity 실행/공식 테스트/Play·GPU·모바일·WebGL 빌드/Push/배포. 사용자 기준 버전 확정을 미실행 검사의 통과로 취급하지 않는다. 기존 시각/긴 점수/종료 중 Pause·Retry·Home 검수의 미실행 기록은 이력으로 유지한다.

## 다음 한 작업

사용자 지시 대기. 회로 구현이나 다른 기능을 자동 시작하지 않는다. 일반 노드 겹침, 새 Flash Background 그림, 사이버 공간 재구축은 계속 보류다. D-014의 TMP imported4개 한정 예외와 기존 작업·인증정보/전역 환경 보존 규칙을 유지한다. 공개 배포는 변경하지 않았다.

## 선별 파일 목록

- `AGENTS.md`
- `Cursor-Dash-master/Assets/Editor/CursorDashFeverPreview.cs`
- `Cursor-Dash-master/Assets/Editor/CursorDashNexusReview.cs`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverFlow.cginc`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverImpact.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/FeverWorld.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/LivingBackground.shader`
- `Cursor-Dash-master/Assets/Resources/CursorDash/VisualPrototype.asset`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Animator/Rotator.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Character/PlayerCharacterController.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/ArrowNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/FeverBonusNode.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/Object/Node/NodeRhythmMath.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashFever.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashLivingBackground.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashRuntimeUI.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashTuning.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/CursorDashVisualProfile.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverBonusField.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverElementMask.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverFlowMath.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverGaugeView.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverNexusRig.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverScoreHud.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverState.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/FeverWorldTransition.cs`
- `Cursor-Dash-master/Assets/Scripts/Gameplay/StageManager/StageManager.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeedbackRevisionTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeverPolishTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashFeverRulesTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashGraphicsTests.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashSceneRegression.cs`
- `Cursor-Dash-master/Assets/Tests/Editor/CursorDashTestPolishTests.cs`
- `Cursor-Dash-master/Assets/WebGLTemplates/CursorDash/TemplateData/cursordash-tools.css`
- `Cursor-Dash-master/Assets/WebGLTemplates/CursorDash/TemplateData/cursordash-tools.js`
- `docs/CursorDash/CHECKPOINT_BEFORE_CIRCUIT_20261027.md`
- `docs/CursorDash/DECISIONS.md`
- `docs/CursorDash/ENVIRONMENT.md`
- `docs/CursorDash/FEVER_EXIT_PROJECTED_SCORE_20261026.md`
- `docs/CursorDash/HEX4_FEVER_EXIT_20261026.md`
- `docs/CursorDash/HEX_DEFAULTS_FEVER_RECALL_20261026.md`
- `docs/CursorDash/HEX_TIMELINE_TOOLS_20261026.md`
- `docs/CursorDash/MASTER_SPEC.md`
- `docs/CursorDash/SOURCES.md`
- `docs/CursorDash/STATUS.md`
- `docs/CursorDash/TUNING_GUIDE.md`
- `docs/CursorDash/VALIDATION.md`
