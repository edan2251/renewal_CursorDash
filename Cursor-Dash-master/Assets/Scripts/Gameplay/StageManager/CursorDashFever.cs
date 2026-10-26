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
    }
    public void Advance(float delta,bool paused)
    {
        Model.Paused=paused;Model.Tick(delta);
        float step=paused?0:Mathf.Max(0,delta);Clock+=step;bonus?.Tick(step);projection?.Advance(step);
        Circuits?.Advance(step);
        rig.Advance(step,Model.Current,(float)Model.Progress,Model.Elapsed);
        if(Model.Current==FeverState.Phase.Entering&&Model.Progress>=.66&&!entryImpulse){
            entryImpulse=true;
            if(CursorDashTuning.Toggle("fx.enabled",true)){
                stage.GetComponent<CameraShake>()?.Kick(Vector2.up,.08f*CursorDashTuning.Number("user.shake",1),.16f,2,24,.30f);ownsShake=true;
                GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion,.65f,.80f);
            }
        }
        stage.SetTimeScore(stage.scoreManager.TimeScore);
    }
    public void AdvanceSpaceVisuals(float delta)
    {
        float step=Mathf.Max(0,delta);Clock+=step;Circuits?.Advance(step);rig.Advance(step,FeverState.Phase.Normal,0);
    }
    public void PulseRoundRing()=>rig.Pulse(true);
    public void StartRingPulse(float strength,float decay)=>rig.EntryPulse(strength,decay);
    public void ResetSpace()
    {
        Abort();Model.ResetRun();rig.Restore(false);Exit=null;feelGate.Reset();entryImpulse=ownsShake=false;
        ContactDiagnostics=false;projection?.ResetRound();World?.Clear();Circuits?.ResetSpace();
    }
    private void Transition(FeverState.Phase before,FeverState.Phase after)
    {
        if(after==FeverState.Phase.Entering){
            // Suppression is already true before resetting any node. Cleanup is never a kill callback.
            LastClearedThreats=stage.NodeCreator.ActiveCount;
            float retire=Mathf.Min(Visuals==null?.18f:Visuals.feverThreatRetireSeconds,(float)FeverSettings.Safe(Settings.enterSeconds,1.05,.001)*.55f);
            stage.NodeCreator.RetireForFever(retire);bonus?.ClearNodes();rig.Begin();entryImpulse=false;
        }else if(after==FeverState.Phase.Fever){stage.NodeCreator.ClearFeverDepartures();rig.Draw(after,0);feelGate.Reset();bonus?.Begin();}
        else if(after==FeverState.Phase.Exiting){
            Exit=Visuals==null?new FeverExitMotion():Visuals.CaptureExit();settings.exitSeconds=Exit.Total;
            rig.BeginExit(Exit);World?.Clear();
            if(ownsShake){stage.GetComponent<CameraShake>()?.Reset();ownsShake=false;}
            bonus?.BeginRecall(Exit,stage.NexusPosition,rig.RestRadius>0?rig.RestRadius:stage.NexusRadius*1.38f);
        }
        else if(after==FeverState.Phase.Normal||after==FeverState.Phase.Stopped){
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
        Circuits?.Hit(point);
        rig.Pulse(false);
        float gain=BonusImpactGain;
        if(feelGate.Stop(Clock,FeverSettings.Safe(Settings.bonusStopGap,.18,.08)))stage.RequestFeedbackStop(Mathf.Clamp((float)FeverSettings.Safe(Settings.bonusStop,.012,0)*gain,0,.035f));
        if(feelGate.Screen(Clock,FeverSettings.Safe(Settings.bonusScreenGap,.10,.06))){
            stage.GetComponent<CameraShake>()?.Kick(incoming,Mathf.Min(.40f,(float)FeverSettings.Safe(Settings.bonusShake,.06,0)*3*gain)*(Visuals==null?.45f:Mathf.Clamp01(Visuals.feverShakeScale))*CursorDashTuning.Number("user.shake",1),.14f,2,28,.35f);ownsShake=true;
            GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion,.6f,Mathf.Lerp(.97f,1.12f,Mathf.Repeat(Clock*.7f,1)));
        }
    }
    public void NexusHit()=>Model.NexusHit();
    public void Abort()=>Model?.Abort();
    public void Review(string command)
    {
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
    private void OnDisable(){if(Model!=null)Abort();rig.Release();hud?.Restore();projection?.Clear();}
    private void OnEnable(){if(stage!=null)rig.Bind(stage.NexusPresentation);}
    private void OnDestroy(){if(Model!=null)Model.Transitioned-=Transition;if(stage!=null&&stage.Feedback!=null)stage.Feedback.Signaled-=OnFeedback;rig.Release();bonus?.Dispose();projection?.Dispose();if(World!=null)Destroy(World);hud?.Dispose();}
}
