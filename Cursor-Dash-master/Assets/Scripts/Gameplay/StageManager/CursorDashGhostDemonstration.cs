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
    private readonly float[] routeLengths=new float[33];private float routeLength,routeTravel;
    private bool crossing,stopped,outro;private float clock,nextSpawn,chooseAfter,steerDelta;private int nextKind;
    public bool Outro=>outro;
    public float Opacity {get;set;}=1;
    public float LastCircleContactCharge {get;private set;}
    public float LastDefenseDistance {get;private set;}
    public float CruiseInput=>InputLimit;
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
        steering=Vector2.right*InputLimit;actor.transform.LookAt2D(Head+Vector2.right);Trail.SeedVisualTail(Vector2.right);
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
        else if(urgent!=null&&soon<Mathf.Max(.85f,Vector2.Distance(Head,stage.NexusPosition)/MotionSpeed+.6f)&&!(target is ArrowNode))target=urgent;
        if(target is ArrowNode hex){
            // A real return is already autonomous. Release this target once, then keep the cruise
            // route instead of re-planning an expired approach every frame while the return flies.
            if(hex.Phase==Node.NodePhase.Resolving){target=plannedTarget=null;routeSeconds=0;Cruise();return;}
            if(hex!=plannedTarget||hex.ExecutionId!=plannedExecution){
                plannedTarget=hex;plannedExecution=hex.ExecutionId;crossing=false;PlanCrossing(hex);
                PlanRoute(approach,Mathf.Max(.1f,UntilFire(hex)-DefenseLead),(cross-approach).normalized);
            }
            if(!crossing&&UntilFire(hex)<=DefenseLead){
                crossing=true;Vector2 tangent=(cross-approach).normalized;
                PlanRoute(cross+tangent*.45f,0,tangent);
            }
            if(!crossing&&RouteFinished)Steer((approach-cross).normalized+(approach-stage.NexusPosition).normalized*.12f);else FollowRoute();
        }else if(target is NormalNode circle){
            if(circle!=plannedTarget||circle.ExecutionId!=plannedExecution){
                plannedTarget=circle;plannedExecution=circle.ExecutionId;crossing=false;
                Vector2 away=(Head-(Vector2)circle.transform.position).normalized;if(away.sqrMagnitude<.1f)away=Vector2.down;
                var own=actor.GetComponent<BoxCollider2D>();
                float clearance=circle.RestContactRadius+(own==null?.1f:own.bounds.extents.magnitude)+.14f;
                approach=(Vector2)circle.transform.position+away*clearance;
                PlanRoute(approach,Mathf.Max(.1f,(.90f-circle.PreparationProgress)*circle.PreparationSeconds),-away,circle,clearance);
            }
            if(!crossing&&circle.PreparationProgress>=.90f){
                crossing=true;Vector2 inward=((Vector2)circle.transform.position-Head).normalized;
                PlanRoute((Vector2)circle.transform.position+inward*.7f,0,inward);
            }
            // If a bounded curve finishes a little early, keep coasting past its side. Never
            // stop, crawl at an anchor or switch to a higher-speed contact/defense dash.
            if(!crossing&&RouteFinished){
                Vector2 away=(approach-(Vector2)circle.transform.position).normalized;
                Vector2 tangent=new Vector2(-away.y,away.x);if(Vector2.Dot(tangent,steering)<0)tangent=-tangent;
                Steer(tangent+away*.12f);
            }else FollowRoute();
        }else {if(plannedTarget!=null){plannedTarget=null;routeSeconds=0;}Cruise();}
    }
    private float InputLimit=>Mathf.Clamp(stage.Fever.Visuals==null?.375f:stage.Fever.Visuals.tutorialGhostInput,.15f,.85f);
    private float MotionSpeed=>Mathf.Max(.1f,actor.MoveSpeed*InputLimit);
    private float DefenseLead=>Mathf.Clamp(.65f*.50f/InputLimit,.35f,(Trail==null?.6782609f:Trail.Lifetime)+.20f); // Ghost route only; the actual shot clock is unchanged.
    private bool RouteFinished=>routeLength<=.001f||Vector2.Distance(Head,routeEnd)<.23f||routeTravel>=routeLength-.05f;
    private static Vector2 OutsideTarget(Vector2 p,Vector2 center,Vector2 away,float clearance,Vector2 lo,Vector2 hi)
    {
        float along=Vector2.Dot(p-center,away);if(along>=clearance)return p;
        // Project inside the intersection of the viewport and the target's safe half-plane.
        // An unrestricted push could put a control beyond the screen and strand the real cursor.
        Vector2 corner=new Vector2(away.x>=0?hi.x:lo.x,away.y>=0?hi.y:lo.y);
        return Vector2.Lerp(p,corner,(clearance-along)/Mathf.Max(.0001f,Vector2.Dot(corner-center,away)-along));
    }
    private static float UntilFire(ArrowNode node)=>node.Timeline==null?float.PositiveInfinity:node.SpawnedAt+(float)(node.Timeline.FireAt*node.BeatSeconds)-Time.time;
    private void PlanCrossing(ArrowNode node)
    {
        Vector2 origin=node.transform.position,outward=(origin-stage.NexusPosition).normalized;
        float gap=stage.Fever.Visuals==null?.60f:Mathf.Clamp(stage.Fever.Visuals.tutorialDefenseNexusGap,.3f,1.2f);
        float distance=Mathf.Min(stage.NexusRadius+gap,Vector2.Distance(origin,stage.NexusPosition)-node.TutorialFootprint-.15f);
        var middle=stage.NexusPosition+outward*Mathf.Max(stage.NexusRadius+.2f,distance);
        var side=new Vector2(-outward.y,outward.x)*.60f;
        approach=middle-side;cross=middle+side;if(Vector2.Distance(Head,cross)<Vector2.Distance(Head,approach)){var temp=approach;approach=cross;cross=temp;}
    }
    private void PlanRoute(Vector2 end,float seconds,Vector2 arrival=default,NormalNode avoid=null,float clearance=0)
    {
        var camera=Camera.main;Vector2 lo=camera.ViewportToWorldPoint(new Vector3(.1f,.14f,0)),hi=camera.ViewportToWorldPoint(new Vector3(.9f,.86f,0));
        Vector2 away=avoid==null?Vector2.zero:(Head-(Vector2)avoid.transform.position).normalized;
        end=PlayerCharacterController.ClampPosition(end,lo,hi);if(avoid!=null)end=OutsideTarget(end,avoid.transform.position,away,clearance,lo,hi);
        routeStart=Head;routeEnd=end;routeAt=clock;routeTravel=0;
        Vector2 d=end-Head,tangent=steering.sqrMagnitude>.002f?steering.normalized:d.normalized;
        if(tangent.sqrMagnitude<.1f)tangent=Vector2.up;if(arrival.sqrMagnitude<.1f)arrival=d.normalized;
        float reach=Mathf.Clamp(d.magnitude*.34f,.18f,.9f),wanted=Mathf.Max(d.magnitude,seconds*MotionSpeed);
        Vector2 side=new Vector2(-d.y,d.x).normalized;if(side.sqrMagnitude<.1f)side=new Vector2(-tangent.y,tangent.x);
        if(Vector2.Dot(side,tangent)<0)side=-side;
        float error=float.PositiveInfinity;Vector2 best1=Head,best2=end;
        // Fit one bounded broad curve to the available wait, rather than varying speed along it.
        for(int sign=0;sign<2;sign++)for(int n=0;n<=12;n++){
            float bow=n/12f*Mathf.Min(wanted,Vector2.Distance(lo,hi))*(sign==0?1:-1);
            routeC1=PlayerCharacterController.ClampPosition(Head+tangent*reach+side*bow,lo,hi);
            routeC2=PlayerCharacterController.ClampPosition(end-arrival.normalized*reach+side*bow,lo,hi);
            if(avoid!=null){routeC1=OutsideTarget(routeC1,avoid.transform.position,away,clearance,lo,hi);routeC2=OutsideTarget(routeC2,avoid.transform.position,away,clearance,lo,hi);}
            float length=0;Vector2 previous=routeStart;
            for(int j=1;j<routeLengths.Length;j++){Vector2 p=Route(j/(float)(routeLengths.Length-1));length+=Vector2.Distance(previous,p);previous=p;}
            float difference=Mathf.Abs(length-wanted);if(difference<error){error=difference;best1=routeC1;best2=routeC2;}
        }
        routeC1=best1;routeC2=best2;routeLengths[0]=0;Vector2 prior=routeStart;
        for(int i=1;i<routeLengths.Length;i++){Vector2 p=Route(i/(float)(routeLengths.Length-1));routeLengths[i]=routeLengths[i-1]+Vector2.Distance(prior,p);prior=p;}
        routeLength=routeLengths[routeLengths.Length-1];routeSeconds=routeLength/MotionSpeed;
    }
    private Vector2 Route(float t){t=Mathf.Clamp01(t);float q=1-t;return q*q*q*routeStart+3*q*q*t*routeC1+3*q*t*t*routeC2+t*t*t*routeEnd;}
    private void FollowRoute()
    {
        float closest=float.PositiveInfinity,along=routeTravel;
        for(int i=1;i<routeLengths.Length;i++){
            if(routeLengths[i]<routeTravel)continue;
            Vector2 a=Route((i-1)/(float)(routeLengths.Length-1)),b=Route(i/(float)(routeLengths.Length-1)),ab=b-a;
            float t=Mathf.Clamp01(Vector2.Dot(Head-a,ab)/Mathf.Max(.00001f,ab.sqrMagnitude));float arc=Mathf.Lerp(routeLengths[i-1],routeLengths[i],t);
            float distance=(Vector2.Lerp(a,b,t)-Head).sqrMagnitude;
            if(arc>=routeTravel&&distance<closest){closest=distance;along=arc;}
        }
        routeTravel=Mathf.Max(routeTravel,along);float look=Mathf.Min(routeLength,routeTravel+.18f+MotionSpeed*.075f);Vector2 ahead=routeEnd;
        for(int i=1;i<routeLengths.Length;i++)if(routeLengths[i]>=look){
            float t=(i-1+Mathf.InverseLerp(routeLengths[i-1],routeLengths[i],look))/(routeLengths.Length-1);ahead=Route(t);break;
        }
        Steer(ahead-Head);
    }
    private void Steer(Vector2 input)
    {
        var v=stage.Fever.Visuals;float rate=v==null?7:v.tutorialGhostSteering;
        if(input.sqrMagnitude<.00001f)input=steering.sqrMagnitude>.001f?steering:Vector2.up;
        float current=steering.sqrMagnitude>.001f?Mathf.Atan2(steering.y,steering.x)*Mathf.Rad2Deg:Mathf.Atan2(input.y,input.x)*Mathf.Rad2Deg;
        float angle=Mathf.LerpAngle(current,Mathf.Atan2(input.y,input.x)*Mathf.Rad2Deg,1-Mathf.Exp(-Mathf.Max(3,rate)*steerDelta))*Mathf.Deg2Rad;
        steering=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*InputLimit;
        actor.MoveVector=steering;
    }
    private void Cruise()
    {
        if(routeSeconds<=0||RouteFinished){
            idleAngle+=1.65f;var camera=Camera.main;
            Vector2 end=camera.ViewportToWorldPoint(new Vector3(.5f+.28f*Mathf.Cos(idleAngle),.49f+.25f*Mathf.Sin(idleAngle),0));
            float period=stage.Fever.Visuals==null?8:stage.Fever.Visuals.tutorialGhostCruiseSeconds;
            PlanRoute(end,Vector2.Distance(Head,end)/MotionSpeed*Mathf.Clamp(period/8,1,1.25f));
        }
        FollowRoute();
    }
    public void Reflected(ArrowNode node,Vector2 point)
    {
        if(!Owns(node))return;Reflections++;LastDefenseDistance=Vector2.Distance(point,stage.NexusPosition);practice.TraceBeam(node,"ghost reflected",point);
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
