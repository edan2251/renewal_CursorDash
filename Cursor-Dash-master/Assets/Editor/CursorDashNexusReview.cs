using UnityEditor;
using UnityEngine;

/// <summary>User-operated training checks and fever run settings. Never starts Play or saves scenes/assets.</summary>
public sealed class CursorDashNexusReview : EditorWindow
{
    private StageManager burstStage;
    private ArrowNode lastReviewedHex;
    private int hitsLeft;
    private double nextHit;
    private Vector2 scroll;
    [MenuItem("Tools/CursorDash/넥서스 수동 검수")]
    public static void Open(){GetWindow<CursorDashNexusReview>("넥서스 검수");}
    private void OnGUI()
    {
        scroll=EditorGUILayout.BeginScrollView(scroll);
        try{
        EditorGUILayout.HelpBox("사용자가 Play → 훈련에 진입한 뒤 버튼으로 검수합니다. 체력 조작은 현재 훈련 라운드에만 적용됩니다.",MessageType.Info);
        var stage=StageManager.instance;
        bool available=EditorApplication.isPlaying&&stage!=null&&CursorDashSession.IsPractice;
        using(new EditorGUI.DisabledScope(!available))
        {
            if(!available)return;
            EditorGUILayout.LabelField("실제 체력 / 최대",stage.scoreManager.Hp.ToString("0.###")+" / "+stage.scoreManager.MaxHp.ToString("0.###"));
            EditorGUILayout.LabelField("표시 수위",stage.Energy.DisplayedLevel.ToString("0.000"));
            EditorGUILayout.LabelField("표면 눌림",stage.Energy.SurfacePress.ToString("0.00000"));
            var view=stage.NexusPresentation;
            EditorGUILayout.LabelField("게임판 연결",view!=null&&view.CanComposite?"연결됨":"UI 원본 표시 — 셰이더/Console 확인");
            if(view!=null)EditorGUILayout.LabelField("넥서스 등록 상태",view.BindingStatus);
            var cursor=stage.Feedback?.Distortion?.CursorLayer;
            if(cursor!=null){
                EditorGUILayout.LabelField("실제 커서 경로",cursor.Status);
                EditorGUILayout.LabelField("실제 커서 원본",cursor.SourceDescription);
                EditorGUILayout.LabelField("커서 합성 호출",cursor.RenderedFrames.ToString());
            }
            if(stage.Feedback.Recent.Count>0){var evt=stage.Feedback.Recent[stage.Feedback.Recent.Count-1];EditorGUILayout.LabelField("최근 사건",evt.sequence+" / "+evt.kind+" / "+evt.cause);}
            EditorGUILayout.LabelField("최종 파괴 / 시간",stage.Finale==null?"없음":stage.Finale.Age.ToString("0.000"));
            EditorGUILayout.LabelField("실제 timeScale",Time.timeScale.ToString("0.000"));
            if(view!=null&&view.CanComposite){
                EditorGUILayout.LabelField("중심 / 반경 오차",Vector2.Distance(view.WorldCenter,stage.NexusPosition).ToString("0.000000")+" / "+Mathf.Abs(view.BodyRadius-stage.NexusRadius).ToString("0.000000"));
                EditorGUILayout.LabelField("실제 공격 반경",stage.NexusRadius.ToString("0.000000"));
                var canvas=view.SourceImages[0].canvas;
                EditorGUILayout.LabelField("원본 Canvas",canvas.renderMode+" / 정렬 "+canvas.sortingOrder);
                foreach(var source in view.SourceImages)EditorGUILayout.LabelField("원본 "+source.name,"enabled="+source.enabled+" / active="+source.gameObject.activeInHierarchy);
                EditorGUILayout.LabelField("원본 UI 카메라",canvas.worldCamera==null?"없음":canvas.worldCamera.name);
                EditorGUILayout.LabelField("게임판 카메라",view.GameboardCamera==null?"없음":view.GameboardCamera.name);
                EditorGUILayout.LabelField("넥서스 합성 호출",view.RenderedFrames.ToString());
                EditorGUILayout.LabelField("커서 앞 표시 연결",view.CursorForegroundBound?"연결됨":"대기/원본 Sprite 확인");
                foreach(var image in view.SourceImages)if(image.name=="HpFrame"){
                    var rhythm=image.GetComponent<NexusRingRhythm>();
                    if(rhythm!=null)EditorGUILayout.LabelField("바깥 테두리",rhythm.IsResting?"정지 (회전 연출 없음)":"원본 Rotator 확인 필요");
                }
            }
            var hex=Object.FindFirstObjectByType<ArrowNode>();
            if(hex!=null)lastReviewedHex=hex;else hex=lastReviewedHex;
            if(hex!=null){
                EditorGUILayout.LabelField("현재 육각",hex.IsPreFiring?"변형 완료 · 짧은 조준 유지":hex.AfterglowOnly?"도달 완료 · 판정 없는 광선 잔상":hex.Phase.ToString());
                EditorGUILayout.LabelField("육각 차징 기준",hex.ChargeFollowsRotation?"실제 첫 테두리 회전":"회전 없음 · 준비시간 안전 대체");
                EditorGUILayout.LabelField("육각 회전 / 충전단계",hex.ChargeRotationTurns.ToString("0.00")+" / 6 종료 · "+hex.ChargeStepsCompleted+" / 3");
                EditorGUILayout.LabelField("육각 변형시작 / 완료 / 발사",hex.MorphStartedAt.ToString("0.000")+" / "+hex.WarningCompletedAt.ToString("0.000")+" / "+hex.FiredAt.ToString("0.000"));
                EditorGUILayout.LabelField("실제 변형 / 예비 시간",hex.MorphSeconds.ToString("0.000")+" / "+hex.PreFireSeconds.ToString("0.000"));
                EditorGUILayout.LabelField("육각 잔상접촉 / 넥서스",hex.BlockedAt.ToString("0.000")+" / "+hex.NexusReachedAt.ToString("0.000"));
            }
            var fever=stage.Fever;
            if(fever!=null&&fever.Model!=null){
                var model=fever.Model;
                EditorGUILayout.Space();EditorGUILayout.LabelField("피버 C — 전투·곡선 흑백 전환 (사용자 검수 대기)",EditorStyles.boldLabel);
                EditorGUILayout.LabelField("점수 / 배율",model.PointsText+" / "+model.MultiplierText+" (최대 x4)");
                EditorGUILayout.LabelField("상태 / 게임시계",model.Current+" / "+(model.Paused?"정지":"진행"));
                EditorGUILayout.LabelField("게이지 / 정상 완료",model.Gauge.ToString("0.000")+" / "+model.CompletedFevers);
                EditorGUILayout.LabelField("마지막 초기화 원인",model.LastReset);
                EditorGUILayout.LabelField("일반 위협 / 보너스",(stage.NormalThreatsSuppressed?"생성·피해 정지":"진행")+" / "+fever.ActiveBonuses);
                EditorGUILayout.LabelField("정리한 위협 / 일반 전투초",fever.LastClearedThreats+" / "+stage.NormalCombatTime.ToString("0.000"));
                EditorGUILayout.LabelField("흑백 후처리 / 호출",fever.World==null?"카메라 없음":(fever.World.Supported?"연결":"셰이더 미지원")+" / "+fever.World.RenderedFrames);
                EditorGUILayout.LabelField("피버 광학후처리 / 호출",fever.World==null?"없음":(fever.World.OpticsSupported?"연결":"미지원/Console 확인")+" / "+fever.World.OpticsFrames);
                if(fever.ContactDiagnostics)EditorGUILayout.HelpBox("판정 진단 중에는 최종 왜곡·색수차를 잠시 우회해 원래 좌표를 비교합니다.",MessageType.None);
                EditorGUILayout.LabelField("점선 소유 / 기준 속도",(fever.DottedControlled?"전용 제어":"원본/미연결")+" / "+fever.DottedSpeed.ToString("0.0")+"도/초 × nexus.spin");
                EditorGUILayout.LabelField("실제 글꼴·텍스처",fever.HudAtlasStatus);
                EditorGUILayout.LabelField("보너스 공급 / 수집 / 연속 경로",fever.SpawnedBonuses+" / "+fever.BonusCollections+" / "+fever.BonusSweepCollections);
                fever.ContactDiagnostics=EditorGUILayout.Toggle("접촉 위치 진단 (기본 꺼짐)",fever.ContactDiagnostics);
                if(fever.ContactDiagnostics)EditorGUILayout.HelpBox("초록: 실제 머리 Box / 자홍: 실제 노드 Circle / 빨강: 최근 접촉점 / 청록: 파괴한 노드·효과 원점. 과거 파편은 머리를 따라가지 않습니다.",MessageType.None);
                EditorGUILayout.HelpBox("D-032: 중앙 배율·시작선·빈 원호 없음. 점수 옆 ×2~4, 충전된 5원호만 표시합니다. 아래 내부 버튼은 실제 충돌 확인이 아닙니다.",MessageType.None);
                if(fever.View!=null){
                    EditorGUILayout.LabelField("링 반경 / 머리 회전",fever.View.Radius.ToString("0.000")+" / "+fever.View.HeadTurn.ToString("0.000"));
                    EditorGUILayout.LabelField("링 중심 오차",Vector2.Distance(fever.View.Center,stage.NexusPosition).ToString("0.000000"));
                }
                using(new EditorGUI.DisabledScope(stage.IsDeath)){
                    EditorGUILayout.HelpBox("아래 버튼은 모델 내부 검수이며 실제 노드 처치/충돌이 아닙니다. 조작 즉시 현재 라운드는 기록 제외. 단계 진행 버튼은 게임의 일시정지/입력 상태를 바꾸지 않습니다.",MessageType.Info);
                    if(GUILayout.Button("모델 처치 +1 (현재 배율 점수/충전)"))fever.Review("kill");
                    if(GUILayout.Button("만충 → 원호 흡수 준비"))fever.Review("fill");
                    if(GUILayout.Button("현재 단계만 끝까지 진행 (내부 시계)"))fever.Review("step");
                    if(GUILayout.Button("피격 초기화만 (HP 보존)"))fever.Review("hit");
                    if(GUILayout.Button("피버 취소 → 일반 복귀 (배율 상승 없음)"))fever.Review("abort");
                    EditorGUI.BeginChangeCheck();var c=fever.Settings;
                    double charge=EditorGUILayout.DoubleField("처치당 충전량",c.chargePerKill),grace=EditorGUILayout.DoubleField("감소 유예 (게임초)",c.decayGrace),decay=EditorGUILayout.DoubleField("초당 자연 감소",c.decayPerSecond);
                    double close=EditorGUILayout.DoubleField("만충 정착",c.closeSeconds),enter=EditorGUILayout.DoubleField("흡수·확산 (C 임시)",c.enterSeconds),life=EditorGUILayout.DoubleField("피버 소모",c.feverSeconds),exit=EditorGUILayout.DoubleField("색 회수·점선 복귀 (C 임시)",c.exitSeconds);
                    if(EditorGUI.EndChangeCheck()){
                        stage.DisqualifyRecord();c.chargePerKill=FeverSettings.Safe(charge,.125,0);c.decayGrace=FeverSettings.Safe(grace,4,0);c.decayPerSecond=FeverSettings.Safe(decay,.03,0);
                        c.closeSeconds=FeverSettings.Safe(close,.22,.001);c.enterSeconds=FeverSettings.Safe(enter,1.05,.001);c.feverSeconds=FeverSettings.Safe(life,8,.01);c.exitSeconds=FeverSettings.Safe(exit,.95,.001);
                    }
                }
                EditorGUILayout.Space();
            }
            bool invulnerable=EditorGUILayout.Toggle("기존 훈련 무적",stage.PracticeInvulnerable);
            if(invulnerable!=stage.PracticeInvulnerable)stage.PracticeInvulnerable=invulnerable;
            using(new EditorGUI.DisabledScope(stage.IsDeath)){
                EditorGUILayout.LabelField("실제 전투 재현 (사용자 버튼 조작)");
                if(GUILayout.Button("A/C 원형 1개 생성"))stage.GetComponent<CursorDashPractice>()?.Spawn(0);
                if(GUILayout.Button("B/D 육각 1개 생성"))stage.GetComponent<CursorDashPractice>()?.Spawn(1);
                EditorGUILayout.HelpBox("A: 원형 충전 중 직접 접촉. B: 잔상으로 육각 레이저 반환. C/D: 해당 노드를 방치. 실제 피해는 훈련 무적을 해제해야 적용됩니다. 마지막 일격은 HP를 낮춘 뒤 C 또는 D로 확인하세요.",MessageType.None);
                EditorGUILayout.LabelField("표시 단계 (기존 HP setter 직접 호출)");
                if(GUILayout.Button("최대 100%"))SetHealth(stage,1);
                if(GUILayout.Button("중간 50%"))SetHealth(stage,.5f);
                if(GUILayout.Button("낮음 15%"))SetHealth(stage,.15f);
                if(GUILayout.Button("0% — 기존 게임오버 경로"))SetHealth(stage,0);
                EditorGUILayout.Space();EditorGUILayout.LabelField("피격 (기존 DamageNexus 호출 / 최대 체력의 10%)");
                using(new EditorGUI.DisabledScope(stage.PracticeInvulnerable||!stage.IsPlaying)){
                    if(GUILayout.Button("단발 피해 1회")){hitsLeft=0;stage.DisqualifyRecord();stage.DamageNexus(stage.scoreManager.MaxHp*.1f);}
                    if(GUILayout.Button("연속 피해 3회 · 0.04초 간격")){stage.DisqualifyRecord();burstStage=stage;hitsLeft=3;nextHit=EditorApplication.timeSinceStartup;}
                }
            }
            if(GUILayout.Button("재시도")){hitsLeft=0;stage.Retry();}
            if(GUILayout.Button("메인 복귀")){hitsLeft=0;stage.ReturnHome();}
        }
        }finally{EditorGUILayout.EndScrollView();}
    }
    private void SetHealth(StageManager stage,float fraction){hitsLeft=0;stage.DisqualifyRecord();stage.scoreManager.Hp=stage.scoreManager.MaxHp*fraction;}
    private void Update()
    {
        if(hitsLeft>0){
            var stage=StageManager.instance;
            if(!EditorApplication.isPlaying||stage==null||stage!=burstStage||!CursorDashSession.IsPractice||stage.IsDeath||stage.PracticeInvulnerable)hitsLeft=0;
            else if(stage.IsPlaying&&EditorApplication.timeSinceStartup>=nextHit){stage.DamageNexus(stage.scoreManager.MaxHp*.1f);hitsLeft--;nextHit=EditorApplication.timeSinceStartup+.04;}
        }
        if(EditorApplication.isPlaying)Repaint();
    }
    private void OnDisable(){hitsLeft=0;burstStage=null;lastReviewedHex=null;}
}
