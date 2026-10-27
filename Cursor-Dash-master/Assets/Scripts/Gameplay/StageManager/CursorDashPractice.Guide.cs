using System.Collections.Generic;
using UnityEngine;

/// <summary>Concurrent real practice and independent autonomous demonstration, owned by the existing practice component.</summary>
public sealed partial class CursorDashPractice
{
    public bool Guided {get;private set;}
    public bool IsReplay {get;private set;}
    public CursorDashGuideFlow Flow {get;private set;}
    public CursorDashGhostDemonstration Ghost=>ghost;
    public bool Resetting=>Guided&&Flow!=null&&Flow.Current==CursorDashGuideFlow.Phase.Reset;
    public bool Completing=>Guided&&Flow!=null&&(Flow.Current==CursorDashGuideFlow.Phase.Settling||Flow.Current==CursorDashGuideFlow.Phase.Fading);
    public float ResetProgress=>Flow==null?0:Mathf.Clamp01((float)Flow.ResetAge/ResetSeconds);
    public string GuideInstruction=>Flow==null?"":$"원형노드 처치 {Flow.CircleWins}/2{(Flow.CircleWins==2?"  [완료]":"")}\n육각노드 처치 {Flow.HexWins}/2{(Flow.HexWins==2?"  [완료]":"")}";
    public IReadOnlyList<string> BeamTrace=>beamTrace;
    public IReadOnlyDictionary<Node,ulong> PracticeTargets=>targets;
    private readonly List<string> beamTrace=new List<string>();
    private readonly Dictionary<Node,ulong> targets=new Dictionary<Node,ulong>();private readonly List<Node> expired=new List<Node>();
    private readonly float[] footprints=new float[2];private System.Random practiceRandom,demoRandom;
    private CursorDashGhostDemonstration ghost;
    private float guideClock,nextSpawn;private int nextPracticeKind;private bool guideInitialized,resetStarted,dataReset,restoredRing,guideCancelled;
    private float ResetSeconds=>stage?.Fever?.Visuals==null?2.2f:Mathf.Max(1f,stage.Fever.Visuals.tutorialResetSeconds);
    private float PracticeGap=>stage?.Fever?.Visuals==null?1.2f:Mathf.Max(.3f,stage.Fever.Visuals.tutorialPracticeSupplySeconds);
    public void Initialize(StageManager owner)
    {
        if(guideInitialized)return;guideInitialized=true;stage=owner;Guided=CursorDashSession.CurrentMode==CursorDashSession.Mode.Tutorial;IsReplay=CursorDashSession.ReplayTutorial;
        movementOrigin=PlayerCharacterController.instance.transform.position;Step=Guided?1:3;Flow=new CursorDashGuideFlow();
        if(!Guided)return;
        stage.PracticeInvulnerable=true;stage.Fever.Model.PracticeOnly=true;
        PlayerCharacterAfterImage.instance?.ClearTrail();
        if(!IsReplay&&!CursorDashSession.FirstGuideNeeded)CursorDashSession.ReplayTutorial=IsReplay=true;
        practiceRandom=new System.Random(unchecked(System.Environment.TickCount^GetInstanceID()));demoRandom=new System.Random(practiceRandom.Next());
        footprints[0]=(stage.NodeCreator.PracticeTemplate(0) as NormalNode)?.TutorialFootprint??.45f;
        footprints[1]=(stage.NodeCreator.PracticeTemplate(1) as ArrowNode)?.TutorialFootprint??.45f;
        nextSpawn=PracticeGap;nextPracticeKind=0;ghost=new CursorDashGhostDemonstration(stage,this);
        stage.Fever.GuideView?.ShowHint(GuideInstruction,true);
    }
    private void UpdateGuide()
    {
        if(!guideInitialized||stage==null||guideCancelled)return;
        if(Completing){AdvanceGuideFinish();return;}
        if(Flow.Current==CursorDashGuideFlow.Phase.Reset||Flow.Current==CursorDashGuideFlow.Phase.Done){AdvanceGuideReset();return;}
        if(!stage.IsPlaying||Time.deltaTime<=0)return;guideClock+=Time.deltaTime;
        expired.Clear();
        foreach(var pair in targets){
            var node=pair.Key;var hex=node as ArrowNode;
            bool active=node!=null&&node.gameObject.activeSelf&&node.ExecutionId==pair.Value;
            if(CursorDashGuideRules.NeedsRecovery(active,hex!=null,node!=null&&node.Phase==Node.NodePhase.Resolving,hex!=null&&hex.ReflectedReturnPending,hex!=null&&hex.AfterglowOnly)){
                if(active){TraceBeam(node,"practice failed recovery",node.transform.position);node.ResetObject();}expired.Add(node);
            }
        }
        foreach(var node in expired)targets.Remove(node);if(expired.Count>0&&targets.Count==0)nextSpawn=guideClock+PracticeGap;RefreshLastTarget();
        ghost?.Tick(Time.deltaTime);
        if(CursorDashGuideRules.SupplyDue(guideClock,nextSpawn,targets.Count,1)){
            nextSpawn=guideClock+.3f; // No safe position: wait instead of evicting another target.
            int kind=CursorDashGuideRules.NextKind(nextPracticeKind,Flow.CircleWins,Flow.HexWins);SpawnGuide(kind);
        }
    }
    private void LateUpdate(){if(Guided&&stage!=null)stage.Fever.GuideView?.ShowHint(GuideInstruction,!guideCancelled&&stage.IsPlaying);}
    public bool CanAccept(Node source,int kind)=>!Guided||!guideCancelled&&Flow.Current==CursorDashGuideFlow.Phase.Practicing&&source!=null&&source.Demonstration==null&&targets.TryGetValue(source,out var execution)&&source.ExecutionId==execution&&kind==(source is ArrowNode?1:0);
    public void Confirm(Node source,int kind)
    {
        if(!Guided||!CanAccept(source,kind))return;
        bool accepted=Flow.Accept(kind,source.GetInstanceID(),source.ExecutionId);
        TraceBeam(source,accepted?"practice credited":"practice already complete",source.transform.position);targets.Remove(source);if(targets.Count==0)nextSpawn=guideClock+PracticeGap;RefreshLastTarget();
    }
    private void RefreshLastTarget(){LastSpawned=null;foreach(var pair in targets)if(pair.Key!=null&&pair.Key.gameObject.activeSelf)LastSpawned=pair.Key;WaitingForAction=targets.Count>0;}
    private Node SpawnGuide(int kind)
    {
        if(stage==null||stage.IsDeath||!stage.IsPlaying||Resetting||Completing||guideCancelled||targets.Count>=1||kind<0||kind>1)return null;
        if(!TryPosition(kind,null,out var position))return null;
        var node=stage.NodeCreator.SpawnPreviewAt(kind,position);
        if(node!=null){targets[node]=node.ExecutionId;nextPracticeKind=1-kind;nextSpawn=float.PositiveInfinity;LastSpawned=node;WaitingForAction=true;TraceBeam(node,"practice spawn",position);}
        return node;
    }
    public bool TryPosition(int kind,CursorDashGhostDemonstration demonstration,out Vector2 result)
    {
        var camera=Camera.main;result=Vector2.zero;if(camera==null||stage==null||kind<0||kind>1)return false;
        var random=demonstration==null?practiceRandom:demoRandom;float radius=footprints[kind],gap=stage.Fever.Visuals==null?.28f:stage.Fever.Visuals.tutorialNodeGap;
        var player=PlayerCharacterController.instance;
        for(int attempt=0;attempt<48;attempt++){
            var p=(Vector2)camera.ViewportToWorldPoint(new Vector3(Mathf.Lerp(.13f,.87f,(float)random.NextDouble()),Mathf.Lerp(.18f,.86f,(float)random.NextDouble()),0));
            var low=camera.WorldToViewportPoint(p-Vector2.one*radius);var high=camera.WorldToViewportPoint(p+Vector2.one*radius);
            if(low.x<.08f||high.x>.92f||low.y<.12f||high.y>.88f)continue;
            float clearance=stage.Fever.Visuals==null?.85f:stage.Fever.Visuals.tutorialNexusClearance;
            if(Vector2.Distance(p,stage.NexusPosition)<stage.NexusRadius+radius+clearance||stage.Fever.OverlapsHud(p,radius,camera)||stage.Fever.GuideView?.BlocksSpawn(p,radius,camera)==true)continue;
            if(player!=null&&Vector2.Distance(player.transform.position,p)<radius+.30f||ghost!=null&&Vector2.Distance(ghost.Head,p)<radius+.30f)continue;
            bool free=true;
            foreach(var other in stage.NodeCreator.TutorialPool){
                if(other==null||!other.gameObject.activeSelf)continue;
                if(!CursorDashGuideRules.Separated(p.x,p.y,radius,other.transform.position.x,other.transform.position.y,footprints[other is ArrowNode?1:0],gap)){free=false;break;}
            }
            if(!free)continue;result=p;return true;
        }
        return false;
    }
    public void TraceBeam(Node source,string action,Vector2 point)
    {
        if(!Guided)return;if(beamTrace.Count>=64)beamTrace.RemoveAt(0);
        beamTrace.Add($"{Time.time:F3} {source.GetInstanceID()}:{source.ExecutionId} {action} ({point.x:F2},{point.y:F2})");
    }
    private void AdvanceGuideReset()
    {
        if(stage.IsUserPaused)return;
        if(!resetStarted){resetStarted=true;Step=4;ghost?.Clear();stage.NodeCreator.StopNodes();targets.Clear();LastSpawned=null;WaitingForAction=false;PlayerCharacterAfterImage.instance?.ClearTrail();stage.Feedback.Clear();stage.SetSpaceState(StageManager.RoundState.TutorialReset);}
        Flow.AdvanceReset(Time.unscaledDeltaTime,false,ResetSeconds);
        if(ResetProgress>=.45f&&!dataReset){dataReset=true;stage.ResetTutorialData();}
        stage.Fever.DrawTutorialReset(ResetProgress,Time.unscaledDeltaTime);
        if(ResetProgress>=.57f&&!restoredRing){restoredRing=true;stage.Fever.StartRingPulse(800,.30f);}
        if(Flow.Current!=CursorDashGuideFlow.Phase.Done)return;
        if(!IsReplay)CursorDashSession.SetTutorialStatus("completed");
        CursorDashSession.ReplayTutorial=false;stage.Fever.GuideView?.ShowHint("",false);stage.FinishTutorial();guideCancelled=true;enabled=false;Destroy(this);
    }
    private void AdvanceGuideFinish()
    {
        if(stage.IsUserPaused)return;
        if(ghost!=null&&!ghost.Outro){ghost.BeginOutro();stage.NodeCreator.StopNodes();targets.Clear();LastSpawned=null;WaitingForAction=false;}
        var v=stage.Fever.Visuals;float settle=v==null?.35f:v.tutorialFinishSettle,fade=v==null?.4f:v.tutorialGhostFade;
        Flow.AdvanceFinish(Time.unscaledDeltaTime,false,stage.Feedback==null||!stage.Feedback.HasLiveFragments,settle,fade);
        if(ghost!=null)ghost.Opacity=Flow.Current==CursorDashGuideFlow.Phase.Settling?1:1-(float)CursorDashGuideFlow.Smooth(Flow.FinishAge/fade);
        if(Resetting)AdvanceGuideReset();
    }
    public double CircuitFront(double length)=>length*CursorDashGuideFlow.ResetCircuit(ResetProgress);
    public void Cancel(){guideCancelled=true;Flow?.Cancel();ghost?.Clear();targets.Clear();LastSpawned=null;WaitingForAction=false;stage?.Fever?.GuideView?.ShowHint("",false);}
    private void OnDisable(){if(guideInitialized)Cancel();}
    private void OnDestroy(){ghost?.Dispose();}
}
