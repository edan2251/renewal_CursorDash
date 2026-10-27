using System.Collections.Generic;
using UnityEngine;

/// <summary>Analog input planner for one cloned real player. Real pooled Nodes, Physics and trail reflection own all kills.</summary>
public sealed class CursorDashGhostDemonstration
{
    private readonly StageManager stage;private readonly CursorDashPractice practice;private readonly GameObject root;
    private readonly PlayerCharacterController actor;private readonly Material particleSource;
    private readonly Dictionary<Node,ulong> owned=new Dictionary<Node,ulong>();private readonly List<Node> expired=new List<Node>();
    private readonly List<CircularBreakVfx> circles=new List<CircularBreakVfx>();private readonly List<ImpactFragments> hexes=new List<ImpactFragments>();
    private readonly Dictionary<int,ulong> collected=new Dictionary<int,ulong>();
    private Node target,plannedTarget;private ulong plannedExecution;private Vector2 approach,cross,steering;
    private Vector2 routeStart,routeC1,routeC2,routeEnd;private float routeAt,routeSeconds,idleAngle;
    private bool crossing,stopped,outro;private float clock,nextSpawn,chooseAfter,steerDelta;private int nextKind;
    public bool Outro=>outro;
    public float Opacity {get;set;}=1;
    public float LastCircleContactCharge {get;private set;}
    public PlayerCharacterAfterImage Trail {get;}
    public bool Running=>!stopped&&!outro&&stage!=null&&stage.IsPlaying&&practice!=null&&!practice.Resetting;
    public Vector2 Head=>actor==null?Vector2.zero:(Vector2)actor.transform.position;
    public int CircleKills {get;private set;}
    public int HexKills {get;private set;}
    public int Reflections {get;private set;}
    public int Misses {get;private set;}
    public int ActiveCount=>owned.Count;
    public Color InkColor {get{var color=stage.Fever.Visuals==null?new Color(.55f,.58f,.62f):stage.Fever.Visuals.tutorialInk;color.a*=Opacity;return color;}}
    public float SketchAmount=>stage.Fever.Visuals==null?.30f:stage.Fever.Visuals.tutorialSketch;
    public CursorDashGhostDemonstration(StageManager owner,CursorDashPractice guide)
    {
        stage=owner;practice=guide;var real=PlayerCharacterController.instance;
        root=new GameObject("Tutorial autonomous player only"){hideFlags=HideFlags.DontSave};root.SetActive(false);root.transform.SetParent(stage.transform,false);
        var copy=Object.Instantiate(real.gameObject,root.transform);copy.name="Gray ghost — real player movement";
        actor=copy.GetComponent<PlayerCharacterController>();actor.SetDemonstration(this);
        Trail=copy.GetComponent<PlayerCharacterAfterImage>();
        var pool=new GameObject("Ghost trail collider pool");pool.transform.SetParent(root.transform,false);Trail.ConfigureDemonstration(this,pool);
        actor.transform.position=Camera.main.ViewportToWorldPoint(new Vector3(.5f,.24f,-Camera.main.transform.position.z));
        root.SetActive(true);
        var realBody=real.GetComponent<Collider2D>();var ghostBody=actor.GetComponent<Collider2D>();if(realBody!=null&&ghostBody!=null)Physics2D.IgnoreCollision(realBody,ghostBody,true);
        var style=copy.AddComponent<CursorDashGuideStyle>();style.Bind(this);
        particleSource=real.transform.Find("CursorVisual").GetComponent<SpriteRenderer>().sharedMaterial;
        nextSpawn=.38f;
    }
    public void Tick(float delta)
    {
        if(!Running||delta<=0)return;clock+=delta;steerDelta=delta;expired.Clear();
        foreach(var pair in owned)if(pair.Key==null||!pair.Key.gameObject.activeSelf||pair.Key.Demonstration!=this||pair.Key.ExecutionId!=pair.Value)expired.Add(pair.Key);
        foreach(var node in expired)owned.Remove(node);
        var visual=stage.Fever.Visuals;int cap=visual==null?2:visual.tutorialGhostLimit;
        if(CursorDashGuideRules.SupplyDue(clock,nextSpawn,owned.Count,cap)){
            nextSpawn=clock+(visual==null?3.1f:Mathf.Max(.4f,visual.tutorialGhostSupplySeconds));
            if(practice.TryPosition(nextKind,this,out var position)){
                var node=stage.NodeCreator.SpawnDemonstrationAt(nextKind,position,this);
                if(node!=null){owned[node]=node.ExecutionId;var style=node.GetComponent<CursorDashGuideStyle>();if(style==null)style=node.gameObject.AddComponent<CursorDashGuideStyle>();style.Bind(this);nextKind=1-nextKind;}
            }
        }
        if(target==null||!target.gameObject.activeSelf||target.Demonstration!=this||target.Phase==Node.NodePhase.Inactive||target.Phase==Node.NodePhase.Resolving&&!((target as ArrowNode)?.ReflectedReturnPending??false))target=null;
        if(clock<chooseAfter){actor.MoveVector=Vector2.zero;return;}
        ArrowNode urgent=null;float soon=float.PositiveInfinity;NormalNode nearest=null;float distance=float.PositiveInfinity;
        foreach(var pair in owned){var node=pair.Key;
            if(node is ArrowNode arrow&&(arrow.Phase==Node.NodePhase.Preparing||arrow.Phase==Node.NodePhase.Warning)){
                float remaining=UntilFire(arrow);if(remaining<soon){soon=remaining;urgent=arrow;}
            }else if(node is NormalNode circle&&(circle.Phase==Node.NodePhase.Preparing||circle.Phase==Node.NodePhase.Warning)){
                float d=Vector2.Distance(Head,circle.transform.position);if(d<distance){distance=d;nearest=circle;}
            }
        }
        if(target==null)target=urgent!=null&&(nearest==null||soon<1.1f)?(Node)urgent:nearest;
        else if(urgent!=null&&soon<.85f&&!(target is ArrowNode))target=urgent;
        if(target is ArrowNode hex){
            // A real return is already autonomous. Release this target once, then keep the cruise
            // route instead of re-planning an expired approach every frame while the return flies.
            if(hex.Phase==Node.NodePhase.Resolving){target=plannedTarget=null;routeSeconds=0;Cruise();return;}
            if(hex!=plannedTarget||hex.ExecutionId!=plannedExecution){plannedTarget=hex;plannedExecution=hex.ExecutionId;crossing=false;PlanCrossing(hex);PlanRoute(approach,Mathf.Max(.2f,UntilFire(hex)-.35f));}
            if(UntilFire(hex)<=.35f)crossing=true;
            if(crossing){Vector2 tangent=(cross-approach).normalized;MoveToward(cross+tangent*.6f);}
            else FollowRoute();
        }else if(target is NormalNode circle){
            if(circle!=plannedTarget||circle.ExecutionId!=plannedExecution){
                plannedTarget=circle;plannedExecution=circle.ExecutionId;
                Vector2 away=(Head-(Vector2)circle.transform.position).normalized;if(away.sqrMagnitude<.1f)away=Vector2.down;
                var body=circle.GetComponent<CircleCollider2D>();var own=actor.GetComponent<BoxCollider2D>();
                float clearance=circle.RestContactRadius+(own==null?.1f:own.bounds.extents.magnitude)+.06f;
                approach=(Vector2)circle.transform.position+away*clearance;
                PlanRoute(approach,Mathf.Max(.2f,(.90f-circle.PreparationProgress)*circle.PreparationSeconds));
                if(Vector2.Distance(Head,approach)<1.3f){
                    // A nearby target still needs one readable approach, not two seconds of crawling
                    // in place. Give that single curve room on the safe side of the target.
                    Vector2 bow=new Vector2(-away.y,away.x)*.85f;if(Vector2.Dot(bow,steering)<0)bow=-bow;
                    routeC1=Head+bow+away*.20f;routeC2=approach+bow*.8f+away*.20f;
                }
                // Both controls stay on the approach side: the curve cannot cut through this target early.
                routeC1=OutsideTarget(routeC1,circle.transform.position,away,clearance);
                routeC2=OutsideTarget(routeC2,circle.transform.position,away,clearance);
            }
            if(circle.PreparationProgress<.90f)FollowRoute();
            else MoveToward((Vector2)circle.transform.position+((Vector2)circle.transform.position-approach).normalized*.7f);
        }else {if(plannedTarget!=null){plannedTarget=null;routeSeconds=0;}Cruise();}
    }
    private float InputLimit=>Mathf.Clamp(stage.Fever.Visuals==null?.70f:stage.Fever.Visuals.tutorialGhostInput,.15f,.85f);
    private static Vector2 OutsideTarget(Vector2 p,Vector2 center,Vector2 away,float clearance)
    {float along=Vector2.Dot(p-center,away);return along<clearance?p+away*(clearance-along):p;}
    private static float UntilFire(ArrowNode node)=>node.Timeline==null?float.PositiveInfinity:node.SpawnedAt+(float)(node.Timeline.FireAt*node.BeatSeconds)-Time.time;
    private void PlanCrossing(ArrowNode node)
    {
        Vector2 origin=node.transform.position,direction=(stage.NexusPosition-origin).normalized;
        var middle=Vector2.Lerp(origin,stage.NexusPosition,.46f);var side=new Vector2(-direction.y,direction.x)*.52f;
        approach=middle-side;cross=middle+side;if(Vector2.Distance(Head,cross)<Vector2.Distance(Head,approach)){var temp=approach;approach=cross;cross=temp;}
    }
    private void PlanRoute(Vector2 end,float seconds)
    {
        routeStart=Head;routeEnd=end;routeAt=clock;routeSeconds=Mathf.Max(.2f,seconds);
        Vector2 d=end-Head,tangent=steering.sqrMagnitude>.002f?steering.normalized:d.normalized;
        float reach=Mathf.Min(d.magnitude*.34f,.9f);Vector2 side=new Vector2(-d.y,d.x).normalized*Mathf.Min(.55f,d.magnitude*.18f);
        if(Vector2.Dot(side,tangent)<0)side=-side;
        routeC1=Head+tangent*reach;routeC2=end-d.normalized*reach+side;
        if(d.magnitude<1.3f){
            // A close hex interception must use the waiting time as one broad approach as well.
            Vector2 bow=new Vector2(-tangent.y,tangent.x)*.85f;if(Vector2.Dot(bow,end-stage.NexusPosition)<0)bow=-bow;
            routeC1=Head+tangent*.8f;routeC2=end+bow;
        }
    }
    private Vector2 Route(float t){t=Mathf.Clamp01(t);float q=1-t;return q*q*q*routeStart+3*q*q*t*routeC1+3*q*t*t*routeC2+t*t*t*routeEnd;}
    private void FollowRoute()
    {
        float t=Mathf.Clamp01((clock-routeAt)/routeSeconds);Vector2 desired=Route(t),ahead=Route(t+.10f/routeSeconds);
        Vector2 input=(ahead-desired)/(.10f*Mathf.Max(.1f,actor.MoveSpeed))+(desired-Head)*.9f;
        Steer(input);
    }
    private void Steer(Vector2 input)
    {
        var v=stage.Fever.Visuals;float rate=v==null?7:v.tutorialGhostSteering;
        steering=Vector2.Lerp(steering,Vector2.ClampMagnitude(input,InputLimit),1-Mathf.Exp(-Mathf.Max(3,rate)*steerDelta));
        actor.MoveVector=steering;
    }
    private void MoveToward(Vector2 destination)=>Steer((destination-Head).normalized*InputLimit);
    private void Cruise()
    {
        if(routeSeconds<=0||clock-routeAt>=routeSeconds||Vector2.Distance(Head,routeEnd)<.18f){
            idleAngle+=1.65f;var camera=Camera.main;
            Vector2 end=camera.ViewportToWorldPoint(new Vector3(.5f+.28f*Mathf.Cos(idleAngle),.49f+.25f*Mathf.Sin(idleAngle),0));
            float period=stage.Fever.Visuals==null?8:stage.Fever.Visuals.tutorialGhostCruiseSeconds;
            PlanRoute(end,Mathf.Max(.65f,Vector2.Distance(Head,end)*1.2f/Mathf.Max(.1f,actor.MoveSpeed*InputLimit))*Mathf.Clamp(period/8,.7f,1.5f));
        }
        FollowRoute();
    }
    public void Reflected(ArrowNode node,Vector2 point)
    {
        if(!Owns(node))return;Reflections++;practice.TraceBeam(node,"ghost reflected",point);
    }
    private bool Owns(Node node)=>!stopped&&node!=null&&node.Demonstration==this&&owned.TryGetValue(node,out var execution)&&execution==node.ExecutionId;
    public void Killed(Node node,int kind,Vector2 point,Vector2 incoming,Vector3[] silhouette)
    {
        if(!Owns(node)||collected.TryGetValue(node.GetInstanceID(),out var old)&&old>=node.ExecutionId)return;
        collected[node.GetInstanceID()]=node.ExecutionId;if(kind==0){CircleKills++;LastCircleContactCharge=((NormalNode)node).LastRemovalCharge;}else HexKills++;
        practice.TraceBeam(node,kind==0?"ghost contact kill":"ghost returned kill",point);target=null;chooseAfter=clock;
        if(!CursorDashTuning.Toggle("fx.particles",true))return;
        if(kind==0){
            var burst=circles.Find(x=>!x.gameObject.activeSelf);
            if(burst==null&&circles.Count<3){var go=new GameObject("Ghost local circular break"){hideFlags=HideFlags.DontSave};go.transform.SetParent(root.transform,false);burst=go.AddComponent<CircularBreakVfx>();burst.GuidePool=true;circles.Add(burst);}
            if(burst!=null){burst.gameObject.SetActive(true);burst.Initialize(point,silhouette,incoming,12,.05f,1.5f,.32f,InkColor,particleSource,65,180);}
        }else{
            var burst=hexes.Find(x=>!x.gameObject.activeSelf);
            if(burst==null&&hexes.Count<3){var go=new GameObject("Ghost local hex break"){hideFlags=HideFlags.DontSave};go.transform.SetParent(root.transform,false);burst=go.AddComponent<ImpactFragments>();burst.GuidePool=true;hexes.Add(burst);}
            if(burst!=null){burst.gameObject.SetActive(true);burst.InitializeDirectional(point,silhouette,incoming,8,1,1.5f,.30f,InkColor,particleSource,65,180,false);}
        }
    }
    public void Missed(Node node){if(!Owns(node))return;Misses++;practice.TraceBeam(node,"ghost missed",node.transform.position);node.ResetObject();if(target==node)target=null;}
    public void BeginOutro(){outro=true;actor?.MoveStop();}
    public void Clear()
    {
        if(stopped)return;stopped=true;actor?.MoveStop();Trail?.ClearTrail();
        foreach(var pair in owned)if(pair.Key!=null&&pair.Key.gameObject.activeSelf&&pair.Key.Demonstration==this)pair.Key.ResetObject();
        owned.Clear();target=null;if(root!=null)root.SetActive(false);
    }
    public void Dispose(){Clear();if(root!=null)Object.Destroy(root);}
}
