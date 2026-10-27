using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>C owner: B rules, threat suspension, bonus pool, dotted rig and final gameboard transition.</summary>
public sealed class CursorDashFever : MonoBehaviour
{
    [SerializeField] private FeverSettings settings=new FeverSettings();
    public FeverState Model {get;private set;}
    public FeverSettings Settings=>settings;
    public CursorDashVisualProfile Visuals {get;private set;}
    public float BonusImpactGain=>Visuals==null?2.5f:Mathf.Clamp(Visuals.feverBreakStrength,.5f,4);
    public FeverGaugeView View {get;private set;}
    private StageManager stage;
    private ulong reviewSerial;
    private FeverBonusField bonus;private readonly FeverNexusRig rig=new FeverNexusRig();
    private readonly FeverFeedbackGate feelGate=new FeverFeedbackGate();
    private bool entryImpulse,ownsShake;
    private readonly FeverFirstNotice notice=new FeverFirstNotice();
    private bool entryNotice,noticeQueued,replayPending;private float noticeReveal=1;
    public bool NoticePending=>entryNotice||noticeQueued||replayPending;
    public string NoticeReplayStatus {get;private set;}="";
    public float NoticePresentation=>NoticeBlocking?0:noticeReveal;
    public bool NoticeBlocking=>entryNotice||noticeQueued||notice.Active;
    public Color TutorialInk=>Visuals==null?new Color(.55f,.58f,.62f):Visuals.tutorialInk;
    public float TutorialPower=>stage?.Practice?.Guided==true?(TutorialResetProgress>=0?(float)CursorDashGuideFlow.ResetPower(TutorialResetProgress):0):1;
    public CursorDashGuideView GuideView {get;private set;}
    public bool NoticeActive=>notice.Active;
    public float NoticeTimeScale=>(float)notice.TimeScale;
    public float TutorialResetProgress {get;private set;}=-1;
    private readonly NexusMagnetism magnet=new NexusMagnetism();private double previousCharge;private float nexusFeedbackClock;
    private bool MagnetActive=>stage.IsPlaying&&(Model.Current==FeverState.Phase.Normal||Model.Current==FeverState.Phase.Closing)&&CursorDashTuning.Toggle("fx.enabled",true);
    private double MagneticSeconds=>Visuals==null?.48:Mathf.Clamp(Visuals.nexusMagnetSeconds,.15f,.6f);
    public float MagneticBody=>MagnetActive?(float)magnet.Body(nexusFeedbackClock,Visuals==null?.04:Visuals.nexusBreath,Visuals==null?.24:Visuals.nexusChargeExpand,Visuals==null?.32:Visuals.nexusHitCompress,MagneticSeconds):1;
    public float MagneticLight=>MagnetActive?(float)magnet.Light(nexusFeedbackClock,Visuals==null?.15:Visuals.nexusBreathLight,MagneticSeconds)*Mathf.Clamp01(CursorDashTuning.EffectGain("user.flash"))*(PlayerPrefs.GetString("OnFlash","true")=="true"?1:0):0;
    public float MagneticRing=>MagnetActive?(float)magnet.Ring(nexusFeedbackClock,(Visuals==null?.24:Visuals.nexusChargeExpand)*.65,Visuals==null?.32:Visuals.nexusHitCompress,MagneticSeconds):1;
    public float MagneticArc(float radians)=>MagnetActive?(float)magnet.ArcShaped(nexusFeedbackClock,radians,Model.Gauge,Visuals==null?.028:Visuals.nexusArcDrift,Visuals==null?.24:Visuals.nexusChargeExpand,Visuals==null?.32:Visuals.nexusHitCompress,Visuals==null?.08:Visuals.nexusChargeGap,MagneticSeconds,Visuals==null?.8:Visuals.nexusArcMotion,Visuals==null?.12:Visuals.nexusArcStagger):1;
    public void NexusVisualHit(bool directionKnown,Vector2 side)
    {
        magnet.Hit(nexusFeedbackClock,directionKnown&&side.sqrMagnitude>.00001f,Mathf.Atan2(side.y,side.x));
        Circuits?.NexusDamaged(); // Same confirmed HP-loss path as the body, not a score/reflection callback.
    }
    public void ClearNexusResponses(){magnet.Clear();previousCharge=Model.Gauge;}
    public void SnapDisplayedScore()=>projection?.SnapScore();
    private FeverScoreHud hud;private FeverBackgroundScore projection;
    public bool BackgroundScoreVisible {get=>projection!=null&&projection.Visible;set{if(projection!=null)projection.Visible=value;}}
    public MeshRenderer BackgroundScoreRenderer=>projection?.View;
    public string BackgroundScoreText=>projection?.Text??"";
    public double BackgroundScorePulse=>projection?.Pulse??0;
    public bool BackgroundScoreAtlasMatches=>projection!=null&&projection.AtlasMatches;
    public FeverWorldTransition World {get;private set;}
    public CursorDashCircuits Circuits {get;private set;}
    public float Clock {get;private set;}
    public int ActiveBonuses=>bonus==null?0:bonus.ActiveCount;
    public int LastClearedThreats {get;private set;}
    public bool DottedControlled=>rig.Bound;
    public double DottedSpeed=>rig.Speed;
    public float DottedRestRadius=>rig.RestRadius; // Read-only geometry for circuit intake; no ring state is changed.
    public string HudAtlasStatus=>hud==null?"대기":hud.AtlasStatus;
    public int SpawnedBonuses=>bonus==null?0:bonus.Spawned;
    public bool ContactDiagnostics {get=>bonus!=null&&bonus.Diagnostics;set{if(bonus!=null)bonus.Diagnostics=value;}}
    public bool OverlapsHud(Vector2 point,float radius,Camera board)=>hud!=null&&hud.BlocksSpawn(point,radius,board);
    public int BonusCollections=>bonus==null?0:bonus.Collected;
    public int BonusSweepCollections=>bonus==null?0:bonus.SweptCollections;
    public IReadOnlyList<FeverBonusBurst> BonusBursts=>bonus?.Bursts;
    public IReadOnlyList<MeshRenderer> RecallViews=>bonus?.RecallViews;
    public int RecallingBonuses=>bonus==null?0:bonus.RecallCount;
    public FeverExitMotion Exit {get;private set;}
    public void SweepBonuses(PlayerCharacterController player,Vector2 start,Vector2 end)=>bonus?.Sweep(player,start,end);
    public void Initialize(StageManager owner,Image outer,TMP_Text fontSource,Text legacyScore=null)
    {
        stage=owner;Visuals=Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");Model=new FeverState(settings);Model.ResetRun();
        View=gameObject.AddComponent<FeverGaugeView>();View.Initialize(owner,this,outer,fontSource);
        rig.Bind(stage.NexusPresentation);bonus=new FeverBonusField(this,stage);
        if(Camera.main!=null){World=Camera.main.gameObject.AddComponent<FeverWorldTransition>();World.hideFlags=HideFlags.DontSave;World.Initialize(this,stage);}
        Model.Transitioned+=Transition;
        if(stage.Feedback!=null)stage.Feedback.Signaled+=OnFeedback;
        gameObject.AddComponent<CursorDashLivingBackground>().Initialize(stage);
        hud=new FeverScoreHud(this,fontSource,legacyScore);projection=new FeverBackgroundScore(this,stage,fontSource);
        Circuits=gameObject.AddComponent<CursorDashCircuits>();Circuits.Initialize(stage,this);
        var guideParent=fontSource!=null&&fontSource.canvas!=null?fontSource.canvas.transform:GameObject.Find("RemakeHUD")?.transform;
        GuideView=new CursorDashGuideView(guideParent); // Reuses existing HUD; never creates a camera, canvas or raycaster.
    }
    public void Advance(float delta,bool paused)
    {
        if(replayPending&&stage.IsPlaying&&stage.Practice?.Guided!=true){replayPending=false;CursorDashSession.ReplayFeverNotice=true;if(Model.Current!=FeverState.Phase.Normal)Model.CancelFever();Model.FillForReview();}
        if(entryNotice&&Model.Current==FeverState.Phase.Entering){
            // White expansion and circuit construction continue, while only real input is blocked.
            // Holding the model here would freeze the very entry that must complete before reading.
            float entryStep=stage.IsUserPaused?0:Time.unscaledDeltaTime;Model.Paused=stage.IsUserPaused;Model.Tick(entryStep);
            Clock+=entryStep;AdvanceDecorations(entryStep);GuideView?.ShowNotice(1);DrawNoticePresentation();stage.RefreshPresentationClock();return;
        }
        if(noticeQueued){
            if(Circuits==null||Circuits.FeverConstructionComplete)BeginNotice();
            else{
                // First fever already shows instructions instead of briefly re-showing the nexus.
                // Only the existing circuit presentation finishes; input/rewards/8-second clock hold.
                Model.Paused=Model.PresentationHold=true;float buildStep=stage.IsUserPaused?0:Time.unscaledDeltaTime;
                Clock+=buildStep;AdvanceDecorations(buildStep);GuideView?.ShowNotice(1);stage.RefreshPresentationClock();return;
            }
        }
        if(notice.Active){
            double previous=notice.PreviewAge;
            bool finished=notice.Advance(Time.unscaledDeltaTime,stage.IsUserPaused);
            float preview=(float)System.Math.Max(0,notice.PreviewAge-previous);
            // Keep the exact 8-second fever and node lifetime intact during the instruction pre-roll.
            if(preview>0)bonus?.TickPreview(preview);
            GuideView?.ShowNotice((float)notice.Alpha);Model.Paused=Model.PresentationHold=true;
            if(finished){Model.Paused=stage.IsUserPaused;Model.PresentationHold=false;CursorDashSession.CompleteFeverGuide();bonus?.EndPreview();noticeReveal=0;stage.ResetPresentationInput();NoticeReplayStatus="안내 완료 · 8초 피버 시작";}
            DrawNoticePresentation();stage.RefreshPresentationClock();return;
        }
        if(stage.IsPlaying)nexusFeedbackClock+=Mathf.Max(0,Time.unscaledDeltaTime);
        Model.Paused=paused;Model.Tick(delta);
        float step=paused||notice.Active?0:Mathf.Max(0,delta);Clock+=step;bonus?.Tick(step);projection?.Advance(step);
        if(Model.Gauge>previousCharge+.00001&&(Model.Current==FeverState.Phase.Normal||Model.Current==FeverState.Phase.Closing))magnet.Charge(nexusFeedbackClock);previousCharge=Model.Gauge;
        if(noticeReveal<1){noticeReveal=Mathf.MoveTowards(noticeReveal,1,step/.25f);DrawNoticePresentation();}
        AdvanceDecorations(step);
        stage.SetTimeScore(stage.scoreManager.TimeScore);
    }
    private void AdvanceDecorations(float step)
    {
        Circuits?.Advance(step);
        if(TutorialResetProgress>=0)rig.GuideReset(TutorialResetProgress,0);else rig.Advance(step,Model.Current,(float)Model.Progress,Model.Elapsed);
        if(Model.Current==FeverState.Phase.Entering&&Model.Progress>=.66&&!entryImpulse){
            entryImpulse=true;
            if(CursorDashSession.FirstFeverGuideNeeded&&!Model.PracticeOnly){entryNotice=true;noticeReveal=0;DrawNoticePresentation();GuideView?.ShowNotice(1);stage.ResetPresentationInput();stage.RefreshPresentationClock();}
            if(CursorDashTuning.Toggle("fx.enabled",true)){
                stage.GetComponent<CameraShake>()?.Kick(Vector2.up,.08f*CursorDashTuning.EffectGain("user.shake"),.16f,2,24,.30f);ownsShake=true;
                GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion,.65f,.80f);
            }
        }
    }
    public void AdvanceSpaceVisuals(float delta)
    {
        float step=Mathf.Max(0,delta);Clock+=step;Circuits?.Advance(step);rig.Advance(step,FeverState.Phase.Normal,0);
    }
    public void PulseRoundRing()=>rig.Pulse(true);
    private void BeginNotice()
    {
        noticeQueued=false;
        if(GuideView?.Bound!=true){Model.PresentationHold=false;Debug.LogWarning("CursorDash: existing HUD for first fever notice is unavailable; completion is not saved.");return;}
        var v=Visuals;
        notice.Begin(0,v == null ? 2 : v.feverNoticeHold,v == null ? 2 : v.feverNoticeFadeOut,0,0);
        Model.PresentationHold=Model.Paused=true;bonus?.Begin();noticeReveal=0;DrawNoticePresentation();stage.ResetPresentationInput();stage.RefreshPresentationClock();GuideView?.ShowNotice(1);NoticeReplayStatus="공간·회로 완성 → 안내 2초 → 문구 2초 페이드·노드 공급 → 조작 복귀";
    }
    private void DrawNoticePresentation()
    {
        var view=stage.NexusPresentation;if(view==null)return;
        float t=Mathf.SmoothStep(0,1,NoticePresentation);view.EntryBodyAlpha=view.EntryRingAlpha=t;
        // Only the first-fever return has a short visual reveal. Tutorial power-on keeps size fixed.
        view.EntryBodyScale=Mathf.Lerp(.88f,1,t);
    }
    private void ClearNoticePresentation(){noticeReveal=1;if(stage?.NexusPresentation!=null){stage.NexusPresentation.EntryBodyScale=1;stage.NexusPresentation.EntryBodyAlpha=stage.NexusPresentation.EntryRingAlpha=1;}}
    public void ReplayNotice()
    {
        if(stage==null||stage.IsDeath){NoticeReplayStatus="사망/결과 연출 뒤 메인에서 다시 선택해 주세요.";Debug.LogWarning(NoticeReplayStatus);return;}
        if(stage.Space?.AtmosphereActive==true||stage.Practice?.Guided==true){CursorDashSession.CurrentMode=CursorDashSession.Mode.Normal;stage.Space?.LaunchSelectedMode();}
        CursorDashTuning.EnableDev(true);stage.DisqualifyRecord();CursorDashSession.ReplayFeverNotice=true;replayPending=true;
        NoticeReplayStatus=stage.IsUserPaused?"재개 후 첫 피버 안내 검수 예약":"첫 피버 안내 검수 시작 예약 (기록 제외)";Debug.Log(NoticeReplayStatus);
    }
    public void DrawTutorialReset(float progress,float delta)
    {
        TutorialResetProgress=Mathf.Clamp01(progress);rig.GuideReset(progress,delta);
        if(stage.NexusPresentation!=null){stage.NexusPresentation.EntryBodyScale=(float)CursorDashGuideFlow.ResetBody(progress);stage.NexusPresentation.EntryBodyAlpha=1;}
    }
    public void CompleteTutorialReset(){TutorialResetProgress=-1;if(stage.NexusPresentation!=null){stage.NexusPresentation.EntryBodyScale=stage.NexusPresentation.EntryRingScale=1;stage.NexusPresentation.EntryBodyAlpha=stage.NexusPresentation.EntryRingAlpha=1;}rig.Restore(false);StartRingPulse(800,.30f);}
    public void StartRingPulse(float strength,float decay)=>rig.EntryPulse(strength,decay);
    public void ResetSpace()
    {
        Abort();CursorDashSession.ReplayFeverNotice=false;Model.ResetRun();rig.Restore(false);Exit=null;feelGate.Reset();entryImpulse=ownsShake=false;
        ContactDiagnostics=false;nexusFeedbackClock=0;magnet.Clear();previousCharge=0;projection?.ResetRound();World?.Clear();Circuits?.ResetSpace();TutorialResetProgress=-1;GuideView?.Clear();
    }
    private void Transition(FeverState.Phase before,FeverState.Phase after)
    {
        if(after==FeverState.Phase.Entering){
            magnet.Clear();
            // Suppression is already true before resetting any node. Cleanup is never a kill callback.
            LastClearedThreats=stage.NodeCreator.ActiveCount;
            float retire=Mathf.Min(Visuals==null?.18f:Visuals.feverThreatRetireSeconds,(float)FeverSettings.Safe(Settings.enterSeconds,1.05,.001)*.55f);
            stage.NodeCreator.RetireForFever(retire);bonus?.ClearNodes();rig.Begin();entryImpulse=false;
        }else if(after==FeverState.Phase.Fever){stage.NodeCreator.ClearFeverDepartures();rig.Draw(after,0);feelGate.Reset();if(CursorDashSession.FirstFeverGuideNeeded&&!Model.PracticeOnly){entryNotice=false;noticeQueued=true;Model.PresentationHold=true;noticeReveal=0;DrawNoticePresentation();GuideView?.ShowNotice(1);stage.ResetPresentationInput();stage.RefreshPresentationClock();}else bonus?.Begin();}
        else if(after==FeverState.Phase.Exiting){
            Exit=Visuals==null?new FeverExitMotion():Visuals.CaptureExit();settings.exitSeconds=Exit.Total;
            rig.BeginExit(Exit);World?.Clear();
            if(ownsShake){stage.GetComponent<CameraShake>()?.Reset();ownsShake=false;}
            bonus?.BeginRecall(Exit,stage.NexusPosition,rig.RestRadius>0?rig.RestRadius:stage.NexusRadius*1.38f);
        }
        else if(after==FeverState.Phase.Normal||after==FeverState.Phase.Stopped){
            notice.Cancel();entryNotice=noticeQueued=false;Model.PresentationHold=false;GuideView?.ShowNotice(0);ClearNoticePresentation();
            magnet.Clear();previousCharge=Model.Gauge;
            stage.NodeCreator.ClearFeverDepartures();bonus?.ClearNodes();bonus?.ClearEffects();rig.Restore(before==FeverState.Phase.Exiting&&after==FeverState.Phase.Normal&&Model.LastReset!="Cancelled"&&Model.LastReset!="NexusHit");World?.Clear();
            if(ownsShake){stage.GetComponent<CameraShake>()?.Reset();ownsShake=false;}
            Exit=null;if(after==FeverState.Phase.Stopped)projection?.Clear();
        }
    }
    public bool Award(Node source)=>source!=null&&Model.Kill(source.GetInstanceID(),source.ExecutionId);
    public bool AwardBonus(Node source)
    {
        if(!stage.IsPlaying||Model.Current!=FeverState.Phase.Fever||source==null)return false;
        bool accepted=Model.Kill(source.GetInstanceID(),source.ExecutionId);stage.SetTimeScore(stage.scoreManager.TimeScore);return accepted;
    }
    public void BonusFeel(Vector2 point,Vector2 incoming)
    {
        if(!CursorDashTuning.Toggle("fx.enabled",true))return;
        World?.Hit(point);
        Circuits?.ScoreHit(point);
        rig.Pulse(false);
        float gain=BonusImpactGain;
        if(feelGate.Stop(Clock,FeverSettings.Safe(Settings.bonusStopGap,.18,.08)))stage.RequestFeedbackStop(Mathf.Clamp((float)FeverSettings.Safe(Settings.bonusStop,.012,0)*gain,0,.035f));
        if(feelGate.Screen(Clock,FeverSettings.Safe(Settings.bonusScreenGap,.10,.06))){
            stage.GetComponent<CameraShake>()?.Kick(incoming,Mathf.Min(.40f,(float)FeverSettings.Safe(Settings.bonusShake,.06,0)*3*gain)*(Visuals==null?.45f:Mathf.Clamp01(Visuals.feverShakeScale))*CursorDashTuning.EffectGain("user.shake"),.14f,2,28,.35f);ownsShake=true;
            GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion,.6f,Mathf.Lerp(.97f,1.12f,Mathf.Repeat(Clock*.7f,1)));
        }
    }
    public void NexusHit()=>Model.NexusHit();
    public void Abort(){notice.Cancel();entryNotice=noticeQueued=replayPending=false;ClearNoticePresentation();if(Model!=null){Model.PresentationHold=false;Model.Abort();}GuideView?.Clear();}
    public void Review(string command)
    {
        if(stage?.Practice?.Guided==true)return;
        if(stage?.Space!=null&&stage.Space.AtmosphereActive)return;
        if(stage==null||stage.IsDeath||(!CursorDashSession.IsPractice&&!CursorDashTuning.DevEnabled))return;
        stage.DisqualifyRecord();
        switch(command){
            case "kill":Model.Kill(int.MinValue,++reviewSerial);break;
            case "fill":Model.FillForReview();break;
            case "step":bool paused=Model.Paused;Model.Paused=false;Model.Tick(System.Math.Max(0,Model.PhaseSeconds-Model.Elapsed)+.000001);Model.Paused=paused;break;
            case "hit":Model.NexusHit();break;
            case "abort":Model.CancelFever();break;
        }
        stage.SetTimeScore(stage.scoreManager.TimeScore);
    }
    private void OnFeedback(HitFeedback.EventInfo hit){if(!CursorDashTuning.Toggle("fx.enabled",true))return;if(hit.kind=="circleRemove"||hit.kind=="triangleBreak")rig.Pulse(false);else if(hit.kind=="nexusDamage")rig.Pulse(true);}
    private void LateUpdate(){hud?.Draw();bonus?.SweepHead();projection?.Draw();}
    private void OnGUI()=>bonus?.DrawDiagnostics(); // After score updates and CanvasScaler layout, before rendering.
    private void OnDisable(){if(Model!=null)Abort();rig.Suspend();hud?.Restore();projection?.Clear();}
    private void OnEnable(){if(stage!=null)rig.Bind(stage.NexusPresentation);}
    private void OnDestroy(){if(Model!=null)Model.Transitioned-=Transition;if(stage!=null&&stage.Feedback!=null)stage.Feedback.Signaled-=OnFeedback;rig.Release(false);bonus?.Dispose();projection?.Dispose();if(World!=null)Destroy(World);hud?.Dispose();GuideView?.Dispose();}
}
