using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using P=CircuitPathGraph.Point;

/// <summary>Two reusable decorative meshes on the existing board. Owns no combat/score/fever clocks.</summary>
[DefaultExecutionOrder(80)]
public sealed class CursorDashCircuits : MonoBehaviour
{
    private StageManager stage;private CursorDashFever fever;private Camera board;private OriginalBackgroundFlash legacy;
    private CircuitPathGraph graph;private CircuitPulseField reactions;private CircuitAmbientFlow ambient;private CircuitFeverRetreat retreat;
    private Material normalMaterial,feverMaterial,compositeMaterial,scoreMaskMaterial;private RenderTexture normalMask,scoreMask;
    private Mesh normalMesh,feverMesh,displayMesh;private GameObject artwork;private MeshRenderer view;private CommandBuffer buffer;
    private readonly Geometry normal=new Geometry(),mono=new Geometry();
    private float clock,deathAge,worldUnit,layoutAspect=-1,layoutSize,flashGain,distortionGain;private Vector2 restCenter;
    private float[] feverStarts,restoreStarts;private double[] normalFronts,feverFronts,deathFronts;private bool restoring,dying,lastEnabled,deathFever;
    private FeverState.Phase previousPhase=FeverState.Phase.Normal;
    private float buildSpeed=1.8f,restoreSpeed=2.1f,buildStagger=.018f,restoreStagger=.018f,entryPush=.075f,deathSpeed=1.6f,deathStagger=.018f;
    private float retreatStart=.18f,retreatSpread=.30f,retreatPower=1.35f,retreatHold=.06f;private uint retreatCycle;private bool cycleRetreat=true;
    private bool maskFailed;private int maskWidth,maskHeight;
    private bool? enabledOverride,hitsOverride,retreatOverride,scoreMaskOverride,ambientOverride,twoTierOverride;
    private readonly ScoreRoot scoreRoot=new ScoreRoot();private bool scoreMaskReady;
    private float nexusWaveAge=-1;private P nexusWaveOrigin; // One replaceable wave, not an accumulating set.
    public Texture ScoreGlyphMask=>scoreMaskReady&&scoreMask!=null?(Texture)scoreMask:Texture2D.blackTexture;
    private bool glyphMaskFailed;private int glyphWidth,glyphHeight;
    public bool Enabled {get=>enabledOverride??(fever.Visuals==null||fever.Visuals.dynamicCircuitsEnabled);set=>enabledOverride=value;}
    public bool HitsEnabled {get=>hitsOverride??(fever.Visuals==null||fever.Visuals.circuitHitsEnabled);set{hitsOverride=value;if(!value)reactions?.Clear();}}
    public bool Active=>isActiveAndEnabled&&fever!=null&&fever.isActiveAndEnabled&&Enabled&&normalMaterial!=null&&normalMask!=null&&normalMask.IsCreated()&&graph!=null&&fever.World!=null&&fever.World.Supported&&legacy!=null&&legacy.Ready;
    public int EdgeCount=>graph==null?0:graph.Edges.Length;
    public int ReactionCount=>reactions==null?0:reactions.Count;
    public int AmbientCount=>ambient==null?0:ambient.Count;
    public bool RetreatEnabled {get=>retreatOverride??(V==null||V.circuitFeverRetreatEnabled);set=>retreatOverride=value;}
    public bool ScoreMaskEnabled {get=>scoreMaskOverride??(V==null||V.circuitScoreMaskEnabled);set=>scoreMaskOverride=value;}
    public bool AmbientEnabled {get=>ambientOverride??(V==null||V.circuitAmbientEnabled);set{ambientOverride=value;if(!value)ambient?.Clear();}}
    public bool TwoTierWidths {get=>twoTierOverride??(V==null||V.circuitTwoTierWidthsEnabled);set=>twoTierOverride=value;}
    public bool ScoreMaskReady=>scoreMaskReady;
    public bool Withdrawing=>dying;
    public bool ContrastPreview {get;set;} // Current round only. Does not write the profile or amplify global screen effects.
    private CursorDashVisualProfile V=>fever.Visuals;
    private static float Value(float x,float fallback,float lo,float hi)=>float.IsNaN(x)||float.IsInfinity(x)?fallback:Mathf.Clamp(x,lo,hi);
    public void Initialize(StageManager game,CursorDashFever owner)
    {
        stage=game;fever=owner;board=Camera.main;if(board==null)return;
        legacy=board.GetComponent<OriginalBackgroundFlash>();var shader=Resources.Load<Shader>("CursorDash/CircuitPath");var composite=Resources.Load<Shader>("CursorDash/CircuitComposite");
        if(shader==null||!shader.isSupported||composite==null||!composite.isSupported||legacy==null||!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32)){Debug.LogWarning("CursorDash: dynamic circuits unavailable; keeping original PNG compositor.");return;}
        normalMaterial=new Material(shader){hideFlags=HideFlags.DontSave};feverMaterial=new Material(shader){hideFlags=HideFlags.DontSave};feverMaterial.SetFloat("_CircuitFever",1);
        compositeMaterial=new Material(composite){hideFlags=HideFlags.DontSave};
        var glyph=Resources.Load<Shader>("CursorDash/CircuitScoreGlyphMask");if(glyph!=null&&glyph.isSupported)scoreMaskMaterial=new Material(glyph){hideFlags=HideFlags.DontSave};else Debug.LogWarning("CursorDash: circuit glyph mask unavailable; score display is kept intact.");
        normalMesh=new Mesh{name="Circuit paths / normal",hideFlags=HideFlags.DontSave};normalMesh.MarkDynamic();
        feverMesh=new Mesh{name="Circuit paths / fever",hideFlags=HideFlags.DontSave};feverMesh.MarkDynamic();
        displayMesh=new Mesh{name="Circuit union display",hideFlags=HideFlags.DontSave};
        // Outside the StageManager hierarchy: these lines must never enter the combat-element silhouette mask.
        artwork=new GameObject("Dynamic background circuits"){hideFlags=HideFlags.DontSave,layer=stage.gameObject.layer};
        artwork.AddComponent<MeshFilter>().sharedMesh=displayMesh;view=artwork.AddComponent<MeshRenderer>();view.sharedMaterial=compositeMaterial;view.sortingOrder=-950;
        buffer=new CommandBuffer{name="CursorDash circuit-only layer"};restCenter=board.transform.position;CaptureEntry();CaptureExit();CaptureRetreat();RebuildLayout();
        stage.Feedback.Signaled+=OnHit;fever.Model.Transitioned+=Transition;
        Camera.onPreCull+=BeforeBoardRender;
    }
    private void RebuildLayout()
    {
        var previousGraph=graph;var previousRetreat=retreat;var oldStarts=feverStarts;var oldRestore=restoreStarts;var oldNormal=normalFronts;var oldFever=feverFronts;var oldDeath=deathFronts;
        layoutAspect=board.aspect;layoutSize=board.orthographicSize;worldUnit=2*layoutSize*Mathf.Min(1,layoutAspect);
        graph=new CircuitPathGraph(Math.Max(1,layoutAspect),Math.Max(1,1/layoutAspect),V==null?12:V.circuitDensity);reactions=new CircuitPulseField(graph);ambient=new CircuitAmbientFlow(graph);retreat=new CircuitFeverRetreat(graph);retreat.Capture(retreatCycle,retreatStart,retreatSpread,retreatPower,retreatHold);if(previousRetreat!=null)retreat.CopyCycle(previousRetreat);
        feverStarts=new float[graph.Roots.Length];restoreStarts=new float[graph.Roots.Length];
        normalFronts=new double[graph.Roots.Length];feverFronts=new double[graph.Roots.Length];deathFronts=new double[graph.Roots.Length];
        for(int i=0;i<feverStarts.Length;i++){
            feverStarts[i]=oldStarts!=null&&i<oldStarts.Length?oldStarts[i]:float.PositiveInfinity;restoreStarts[i]=oldRestore!=null&&i<oldRestore.Length?oldRestore[i]:float.PositiveInfinity;
            if(previousGraph!=null&&i<previousGraph.RootLengths.Length){double factor=graph.RootLengths[i]/previousGraph.RootLengths[i];normalFronts[i]=oldNormal[i]*factor;feverFronts[i]=oldFever[i]*factor;deathFronts[i]=oldDeath[i]*factor;}
        }
        float x=worldUnit*Mathf.Max(1,layoutAspect)*.5f,y=worldUnit*Mathf.Max(1,1/layoutAspect)*.5f;
        displayMesh.vertices=new[]{new Vector3(restCenter.x-x,restCenter.y-y,0),new Vector3(restCenter.x-x,restCenter.y+y,0),new Vector3(restCenter.x+x,restCenter.y+y,0),new Vector3(restCenter.x+x,restCenter.y-y,0)};
        displayMesh.triangles=new[]{0,1,2,0,2,3};displayMesh.RecalculateBounds();EnsureMask();
    }
    public void Advance(float delta)
    {
        if(graph==null)return;clock+=Mathf.Max(0,delta);
        if(!HitsEnabled||!CursorDashTuning.Toggle("fx.enabled",true)){reactions.Clear();nexusWaveAge=-1;}
        if(nexusWaveAge>=0&&stage.IsPlaying){nexusWaveAge+=Mathf.Max(0,Time.unscaledDeltaTime);if(nexusWaveAge>=(V==null?.80f:Value(V.circuitNexusSeconds,.80f,.3f,1.2f)))nexusWaveAge=-1;}
        reactions.Advance(delta,V==null?1.65f:Value(V.circuitTransferSpeed,1.65f,.3f,3),V==null?.34f:Value(V.circuitHitSeconds,.34f,.08f,.6f),V==null?.40f:Value(V.circuitHitDecay,.40f,.05f,.5f),V==null?4:V.circuitReactionLimit);
        bool quiet=AmbientEnabled&&!dying&&Active&&(!stage.IsDeath||stage.Space!=null&&stage.Space.IsReturning)&&CursorDashTuning.Toggle("fx.enabled",true)&&(fever.Model.Current==FeverState.Phase.Normal||fever.Model.Current==FeverState.Phase.Fever||stage.Space!=null&&stage.Space.IsReturning);
        ambient.AdvanceShaped(delta,quiet,V==null?.28f:Value(V.circuitAmbientSpeed,.28f,.08f,.6f),V==null?2.2f:Value(V.circuitAmbientInterval,2.2f,1.5f,12),V==null?2:V.circuitAmbientLimit,V==null?.28f:Value(V.circuitAmbientTail,.28f,.06f,.35f),V==null?.032f:Value(V.circuitAmbientHead,.032f,.008f,.06f));
    }
    private void CaptureEntry(){buildSpeed=V==null?1.8f:Value(V.circuitBuildSpeed,1.8f,.4f,4);buildStagger=V==null?.018f:Value(V.circuitPathStagger,.018f,0,.05f);entryPush=V==null?.075f:Value(V.circuitEntryPush,.075f,0,.2f);}
    private void CaptureExit(){restoreSpeed=V==null?2.1f:Value(V.circuitRestoreSpeed,2.1f,.4f,4);restoreStagger=V==null?.018f:Value(V.circuitPathStagger,.018f,0,.05f);}
    private void CaptureRetreat(){cycleRetreat=RetreatEnabled;retreatCycle=(uint)fever.Model.CompletedFevers+1;retreatStart=V==null?.18f:Value(V.circuitRetreatStart,.18f,.08f,.4f);retreatSpread=V==null?.30f:Value(V.circuitRetreatSpread,.30f,0,.45f);retreatPower=V==null?1.35f:Value(V.circuitRetreatPower,1.35f,.55f,2.4f);retreatHold=V==null?.06f:Value(V.circuitRetreatReadHold,.06f,.02f,.15f);retreat?.Capture(retreatCycle,retreatStart,retreatSpread,retreatPower,retreatHold);}
    private void Transition(FeverState.Phase before,FeverState.Phase after)
    {
        nexusWaveAge=-1;if(dying)return;previousPhase=before;ambient?.Clear();
        if(after==FeverState.Phase.Entering){CaptureEntry();CaptureRetreat();restoring=false;reactions.Clear();for(int i=0;i<feverStarts.Length;i++)feverStarts[i]=float.PositiveInfinity;}
        else if(after==FeverState.Phase.Exiting){CaptureExit();restoring=true;reactions.Clear();for(int i=0;i<restoreStarts.Length;i++)restoreStarts[i]=float.PositiveInfinity;}
        else if(after==FeverState.Phase.Normal&&before!=FeverState.Phase.Exiting){restoring=false;reactions.Clear();}
        else if(after==FeverState.Phase.Stopped){
            // Death calls Abort before EmitFinal. The final-hit notification takes priority over suction/rebuild.
            reactions.Clear();
        }
    }
    private void OnHit(HitFeedback.EventInfo hit)
    {
        if(hit.kind=="nexusFinal"){
            deathSpeed=V==null?1.6f:Value(V.circuitDeathSpeed,1.6f,.4f,4);deathStagger=V==null?.018f:Value(V.circuitDeathStagger,.018f,0,.04f);
            dying=true;deathAge=0;nexusWaveAge=-1;reactions.Clear();ambient.Clear();deathFever=previousPhase==FeverState.Phase.Fever;
            for(int i=0;i<deathFronts.Length;i++)deathFronts[i]=deathFever?feverFronts[i]:normalFronts[i];return;
        }
        if(hit.kind=="circleRemove"||hit.kind=="triangleBreak")ScoreHit(hit.point);
        else if(hit.kind=="hexBlock"||hit.kind=="nexusDamage"&&!hit.confirmedDamage)Hit(hit.point);
    }
    public void Hit(Vector2 worldPoint)
    {
        if(!Active||dying||!HitsEnabled||!stage.IsPlaying||!CursorDashTuning.Toggle("fx.enabled",true))return;
        reactions.Hit(Local(worldPoint),V==null?.18f:Value(V.circuitHitRange,.18f,.02f,.25f),V==null?4:V.circuitReactionLimit);
    }
    public void ScoreHit(Vector2 worldPoint)
    {
        if(!Active||dying||!HitsEnabled||!stage.IsPlaying||!CursorDashTuning.Toggle("fx.enabled",true))return;
        reactions.HitRadial(Local(worldPoint),V==null?4:V.circuitReactionLimit,V==null?2.6f:Value(V.circuitScoreWaveSpeed,2.6f,1,5),V==null?.32f:Value(V.circuitScoreWaveWidth,.32f,.10f,.6f));
    }
    public void NexusDamaged(bool final=false)
    {
        if(!Active||!HitsEnabled||!CursorDashTuning.Toggle("fx.enabled",true))return;
        if(final){if(!dying||!stage.IsDeath)return;}else if(dying||!stage.IsPlaying)return;
        nexusWaveOrigin=Local(stage.NexusPosition);nexusWaveAge=0;
    }
    private P Local(Vector2 world)=>(new P(world.x-restCenter.x,world.y-restCenter.y))*(1/Math.Max(.0001,worldUnit));
    private Vector3 World(P p)=>new Vector3(restCenter.x+(float)p.X*worldUnit,restCenter.y+(float)p.Y*worldUnit,0);
    private float White(P p)=>fever.World==null?0:fever.World.WhiteAtViewport(board.WorldToViewportPoint(World(p)));
    private void LateUpdate()
    {
        if(normalMaterial==null)return;
        if(Mathf.Abs(layoutAspect-board.aspect)>.0001f||Mathf.Abs(layoutSize-board.orthographicSize)>.0001f)RebuildLayout();
        EnsureMask();
        bool active=Active;legacy.DynamicCircuits=active;view.enabled=active;
        if(active!=lastEnabled){reactions.Clear();ambient.Clear();nexusWaveAge=-1;lastEnabled=active;}
        if(!active)return;
        if(!HitsEnabled||!CursorDashTuning.Toggle("fx.enabled",true)){reactions.Clear();nexusWaveAge=-1;}
        if(dying){deathAge+=Time.unscaledDeltaTime;if(nexusWaveAge>=0)nexusWaveAge+=Time.unscaledDeltaTime;} // Finale uses real time; preserve the same damage wave while paths retreat.
    }
    // The existing score's LateUpdate/ForceMeshUpdate (including its vertex pulse) has finished.
    // Refresh masks with the final board camera immediately before culling/rendering, not a previous frame.
    private void BeforeBoardRender(Camera camera){if(camera!=board||normalMaterial==null||view==null)return;bool active=Active;legacy.DynamicCircuits=active;view.enabled=active;if(active)Draw();}
    private double Front(int root,bool feverLayer)
    {
        if(!feverLayer&&stage.Space!=null&&(stage.Space.IsStarting||stage.Space.IsReturning))return stage.Space.CircuitFront(graph.RootLengths[root]);
        if(dying)return Math.Min(deathFronts[root],DeathAt(root));
        if(feverLayer){
            var phase=fever.Model.Current;if(phase!=FeverState.Phase.Entering&&phase!=FeverState.Phase.Fever)return 0;
            if(float.IsPositiveInfinity(feverStarts[root])&&White(graph.Points[graph.Roots[root]])>.95f)feverStarts[root]=clock;
            double built=Math.Max(0,(clock-feverStarts[root]-buildStagger*((root*5)%graph.Roots.Length)))*buildSpeed,progress=phase==FeverState.Phase.Fever?1-CircuitPathGraph.Clamp(fever.Model.Gauge):0;
            if(built>=graph.RootLengths[root])retreat.Built(root,progress);
            return Math.Min(built,phase==FeverState.Phase.Fever&&cycleRetreat?retreat.Front(root,progress):graph.RootLengths[root]);
        }
        if(!restoring)return graph.MaxDepth;
        if(float.IsPositiveInfinity(restoreStarts[root])&&White(graph.Points[graph.Roots[root]])<.03f)restoreStarts[root]=clock;
        return Math.Max(0,(clock-restoreStarts[root]-restoreStagger*((root*3)%graph.Roots.Length)))*restoreSpeed;
    }
    private double DeathAt(int root)=>graph.DeathFront(deathAge,deathSpeed,deathStagger,root);
    private P Warp(int index,double t,bool feverLayer)
    {
        var edge=graph.Edges[index];P p=graph.Points[edge.A]+(graph.Points[edge.B]-graph.Points[edge.A])*t;float fx=distortionGain;
        if(dying)return p;
        var phase=fever.Model.Current;P center=Local(stage.NexusPosition),d=p-center;double r=Math.Max(.0001,d.Length);
        if(!feverLayer&&phase==FeverState.Phase.Entering&&!dying){
            double force=White(p)*entryPush*fx;
            return p+d*(force/r);
        }return p;
    }
    private Color Tint(bool feverLayer,double level,double tip,double quiet,double damage)
    {
        float contrast=feverLayer?(V==null?.065f:Value(V.circuitFeverContrast,.065f,.03f,.25f)):(V==null?.075f:Value(V.circuitNormalContrast,.075f,.02f,.24f));
        float accessible=flashGain;
        float red=(float)damage*accessible,priority=(float)CircuitPulseField.DamagePriority(red);
        float boost=(V==null?.44f:Value(V.circuitMaxEmphasis,.44f,0,.45f))*(float)level*accessible;
        float tipGain=V==null?.14f:Value(V.circuitTipEmphasis,.14f,0,.25f);
        Color color=feverLayer?(V==null?new Color(.18f,.19f,.20f):V.circuitFeverTint):(V==null?new Color(.40f,.47f,.52f):V.circuitNormalTint);
        if(!feverLayer)color=Color.Lerp(color,new Color(.65f,.92f,.96f),Mathf.Clamp01((float)level*.55f+(float)tip*.45f+(float)quiet*.42f));
        color=Color.Lerp(color,V==null?new Color(1,.04f,.08f):V.nexusDamageTint,priority);
        float soft=(float)quiet*(V==null?.12f:Value(V.circuitAmbientStrength,.12f,0,.16f))*accessible;
        // Quiet flow and impacts compete by max; reduced resting contrast never scales down event emphasis.
        float damageBoost=red*(V==null?.44f:Value(V.circuitNexusEmphasis,.44f,0,.6f));
        float opacity=contrast*.80f+Mathf.Max(Mathf.Max(soft,boost+(float)tip*tipGain*accessible)*(1-priority),damageBoost);
        color.a=Mathf.Min(.48f,opacity*(ContrastPreview?1.8f:1));return color;
    }
    private Color At(int index,double t,double front,bool feverLayer,double speed,double seconds,double decay)
    {
        var edge=graph.Edges[index];P point=graph.Points[edge.A]+(graph.Points[edge.B]-graph.Points[edge.A])*t;
        double damage=nexusWaveAge<0?0:CircuitPulseField.RadialLevel((point-nexusWaveOrigin).Length,nexusWaveAge,V==null?2.6:Value(V.circuitNexusSpeed,2.6f,1,5),V==null?.26:Value(V.circuitNexusWidth,.26f,.08f,.5f),V==null?.80:Value(V.circuitNexusSeconds,.80f,.3f,1.2f));
        Color color=Tint(feverLayer||dying&&deathFever,dying?0:reactions.Level(index,t,speed,seconds,decay),Tip(edge,t,front,feverLayer),dying?0:ambient.Level(index,t),damage);
        return color;
    }
    private Vector3 Side(int edgeIndex,double t,double fraction,double front,float half,bool feverLayer,int root)
    {
        var edge=graph.Edges[edgeIndex];
        Vector3 at=World(Warp(edgeIndex,t,feverLayer)),before=World(Warp(edgeIndex,Math.Max(0,t-.02),feverLayer)),after=World(Warp(edgeIndex,Math.Min(fraction,t+.02),feverLayer));
        if(t<=.000001){
            if(graph.Degree[edge.A]==2){int other=graph.Incident[edge.A][0]==edgeIndex?graph.Incident[edge.A][1]:graph.Incident[edge.A][0];var link=graph.Edges[other];before=World(Warp(other,link.A==edge.A?.02:.98,feverLayer));}
            else before=at-(after-at);
        }
        if(t>=fraction-.000001){
            if(fraction>=.999999&&front>edge.Depth+edge.Length+.000001&&graph.Degree[edge.B]==2){int other=graph.Incident[edge.B][0]==edgeIndex?graph.Incident[edge.B][1]:graph.Incident[edge.B][0];var link=graph.Edges[other];after=World(Warp(other,link.A==edge.B?.02:.98,feverLayer));}
            else after=at+(at-before);
        }
        P side=CircuitStrokeMath.Side(new P(before.x,before.y),new P(at.x,at.y),new P(after.x,after.y),half);return new Vector3((float)side.X,(float)side.Y,0);
    }
    private double Tip(CircuitPathGraph.Edge edge,double t,double front,bool feverLayer)
    {
        double tip=front<graph.RootLengths[edge.Root]||dying?Math.Max(0,1-Math.Abs(front-edge.Depth-edge.Length*t)/.035):0;
        return tip;
    }
    private void Glyph(Geometry data,Vector3 point,float radius,Color tint,int kind,Vector3 axis=default)
    {
        if(radius<.000001f)return;if(axis.sqrMagnitude<.001f)axis=Vector3.right;axis=axis.normalized*radius;
        Vector3 side=new Vector3(-axis.y,axis.x,0);data.Quad(point-axis-side,point-axis+side,point+axis+side,point+axis-side,tint,kind,board);
    }
    private void DrawLayer(Geometry data,bool feverLayer)
    {
        data.Clear();
        if(dying&&feverLayer)return;
        double speed=V==null?1.65:Value(V.circuitTransferSpeed,1.65f,.3f,3),seconds=V==null?.34:Value(V.circuitHitSeconds,.34f,.08f,.6f),decay=V==null?.40:Value(V.circuitHitDecay,.40f,.05f,.5f);
        for(int root=0;root<graph.Roots.Length;root++){
            double front=Front(root,feverLayer);if(feverLayer)feverFronts[root]=front;else if(!dying)normalFronts[root]=front;
            for(int e=0;e<graph.Edges.Length;e++){
                var edge=graph.Edges[e];if(edge.Root!=root)continue;double visible=CircuitPathGraph.VisibleLength(edge,front);if(visible<=.000001)continue;
                double fraction=visible/edge.Length;float half=HalfWidth(e);
                for(int n=0;n<16;n++){
                    double from=n/16d,to=Math.Min(fraction,(n+1)/16d);if(to<=from)break;
                    Vector3 p=World(Warp(e,from,feverLayer)),q=World(Warp(e,to,feverLayer));Vector3 direction=q-p;
                    if(direction.sqrMagnitude<1e-12f)continue;
                    Vector3 first=Side(e,from,fraction,front,half,feverLayer,root),last=Side(e,to,fraction,front,half,feverLayer,root);
                    Color c=At(e,from,front,feverLayer,speed,seconds,decay),end=At(e,to,front,feverLayer,speed,seconds,decay);
                    data.Quad(p-first,p+first,q+last,q-last,c,0,board,end);
                }
                bool complete=visible>=edge.Length-.000001;Vector3 finish=World(Warp(e,fraction,feverLayer));
                Color finishTint=At(e,fraction,front,feverLayer,speed,seconds,decay);
                if(!complete||graph.Degree[edge.B]==1||front<=edge.Depth+edge.Length+.000001)Glyph(data,finish,half,finishTint,1,finish-World(Warp(e,Math.Max(0,fraction-.02),feverLayer)));
                // A flush, stroke-sized round junction, not a separate terminal ornament. Max-mask union prevents overlap darkening.
                if(graph.Degree[edge.A]>2){float radius=half;foreach(int other in graph.Incident[edge.A])radius=Mathf.Max(radius,HalfWidth(other));Glyph(data,World(Warp(e,0,feverLayer)),radius,At(e,0,front,feverLayer,speed,seconds,decay),2);}
            }
        }
    }
    private float HalfWidth(int index)
    {
        var edge=graph.Edges[index];float width;
        if(TwoTierWidths)width=edge.Thin?(V==null?.013888889f:Value(V.circuitSecondaryWidth,.013888889f,.006f,.025f)):(V==null?.018518519f:Value(V.circuitPrimaryWidth,.018518519f,.012f,.04f));
        else width=edge.WidthPixels>40?(V==null?.043055556f:Value(V.circuitMainWidth,.043055556f,.02f,.08f))*(float)(edge.WidthPixels/60):edge.Thin?(V==null?.009259259f:Value(V.circuitBranchWidth,.009259259f,.004f,.025f))*(float)(edge.WidthPixels/10):(V==null?.025925926f:Value(V.circuitLineWidth,.025925926f,.008f,.08f))*(float)(edge.WidthPixels/28);
        return width*worldUnit*.5f;
    }
    private void Draw()
    {
        // Read user settings once for both reusable meshes, rather than once per warped vertex/segment.
        bool effects=CursorDashTuning.Toggle("fx.enabled",true);distortionGain=effects?Mathf.Clamp01(CursorDashTuning.Number("user.distortion",1)):0;
        flashGain=effects&&PlayerPrefs.GetString("OnFlash","true")=="true"?Mathf.Clamp01(CursorDashTuning.Number("user.flash",1)):0;
        DrawLayer(normal,false);DrawLayer(mono,true);normal.Upload(normalMesh);mono.Upload(feverMesh);
        if(dying){normalMaterial.SetVector("_FeverFlow",Vector4.zero);normalMaterial.SetFloat("_CircuitFever",0);}
        else fever.World.SetFlow(normalMaterial,Math.Max(1,board.pixelWidth),Math.Max(1,board.pixelHeight));
        fever.World.SetFlow(feverMaterial,Math.Max(1,board.pixelWidth),Math.Max(1,board.pixelHeight));
        RenderScoreMask();
        float hide=ScoreMaskEnabled&&scoreMaskReady?(V==null?1:Value(V.circuitScoreMaskStrength,1,0,1)):0;
        normalMaterial.SetTexture("_CircuitScoreMask",scoreMask!=null?scoreMask:Texture2D.blackTexture);normalMaterial.SetFloat("_ScoreHide",hide);
        feverMaterial.SetTexture("_CircuitScoreMask",scoreMask!=null?scoreMask:Texture2D.blackTexture);feverMaterial.SetFloat("_ScoreHide",hide);
        RenderNormal();
    }
    private void EnsureMask()
    {
        int w=Math.Max(1,board.pixelWidth),h=Math.Max(1,board.pixelHeight);float scale=Mathf.Min(1,1536f/Mathf.Max(w,h));w=Math.Max(1,Mathf.RoundToInt(w*scale));h=Math.Max(1,Mathf.RoundToInt(h*scale));
        if(normalMask!=null&&normalMask.width==w&&normalMask.height==h&&normalMask.IsCreated())return;
        if(maskFailed&&w==maskWidth&&h==maskHeight)return;maskWidth=w;maskHeight=h;
        ReleaseMask();normalMask=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Circuit union mask",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        if(!normalMask.Create()){ReleaseMask();maskFailed=true;Debug.LogWarning("CursorDash: circuit mask unavailable; using preserved PNG until viewport changes.");return;}maskFailed=false;compositeMaterial.SetTexture("_MainTex",normalMask);
    }
    private void ReleaseMask(){if(normalMask==null)return;normalMask.Release();Destroy(normalMask);normalMask=null;}
    private sealed class ScorePart
    {
        public MeshFilter filter;public MeshRenderer renderer;public readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
    }
    private void ReleaseScoreMask(){if(scoreMask==null)return;scoreMask.Release();Destroy(scoreMask);scoreMask=null;}
    private sealed class ScoreRoot
    {
        public MeshRenderer source;public TMP_Text text;public ScorePart[] parts;public int materials=-1;
        public bool Visible=>source!=null&&source.enabled&&!source.forceRenderingOff&&source.gameObject.activeInHierarchy;
    }
    private void UpdateScoreRoot(ScoreRoot cache,MeshRenderer source)
    {
        if(source!=cache.source){cache.source=source;cache.text=source==null?null:source.GetComponent<TMP_Text>();cache.parts=null;cache.materials=-1;}
        if(cache.text==null||cache.text.textInfo==null)return;
        if(cache.parts==null||cache.materials!=cache.text.textInfo.materialCount){
            var filters=source.GetComponentsInChildren<MeshFilter>(true);cache.parts=new ScorePart[filters.Length];
            for(int i=0;i<filters.Length;i++)cache.parts[i]=new ScorePart{filter=filters[i],renderer=filters[i].GetComponent<MeshRenderer>()};cache.materials=cache.text.textInfo.materialCount;
        }
    }
    private int DrawScoreRoot(ScoreRoot cache,float coverage,float padding,float softness)
    {
        if(!cache.Visible||cache.parts==null||coverage<=.0001f)return 0;int drawn=0;
        foreach(var part in cache.parts){
            if(part.filter==null||part.renderer==null||!part.renderer.enabled||part.renderer.forceRenderingOff||!part.renderer.gameObject.activeInHierarchy||(board.cullingMask&(1<<part.renderer.gameObject.layer))==0)continue;
            var mesh=part.filter.sharedMesh;var material=part.renderer.sharedMaterial;if(mesh==null||mesh.vertexCount==0||material==null||material.mainTexture==null)continue;
            float Read(string name,float fallback)=>material.HasProperty(name)?material.GetFloat(name):fallback;
            var properties=part.properties;properties.SetTexture("_MainTex",material.mainTexture);
            properties.SetVector("_FontWeight",new Vector4(Read("_WeightNormal",0),Read("_WeightBold",.5f),Read("_FaceDilate",0),Read("_ScaleRatioA",1)));
            properties.SetVector("_GlyphAdjust",new Vector4(padding,Mathf.Max(.25f,softness),Read("_VertexOffsetX",0),Read("_VertexOffsetY",0)));
            properties.SetFloat("_IsSDF",material.HasProperty("_GradientScale")?1:0);properties.SetFloat("_Coverage",Mathf.Clamp01(coverage));
            buffer.DrawMesh(mesh,part.renderer.localToWorldMatrix,scoreMaskMaterial,0,0,properties);drawn++;
        }return drawn;
    }
    private void RenderScoreMask()
    {
        scoreMaskReady=false;
        bool scoreOptics=fever.Model.Current==FeverState.Phase.Fever&&(V==null||V.feverScoreChromatic>1);
        if((!ScoreMaskEnabled&&!scoreOptics)||scoreMaskMaterial==null||normalMask==null)return;
        UpdateScoreRoot(scoreRoot,fever.BackgroundScoreRenderer);
        if(!scoreRoot.Visible)return;
        int w=normalMask.width,h=normalMask.height;
        if(scoreMask==null||scoreMask.width!=w||scoreMask.height!=h||!scoreMask.IsCreated()){
            if(glyphMaskFailed&&glyphWidth==w&&glyphHeight==h)return;glyphWidth=w;glyphHeight=h;ReleaseScoreMask();
            scoreMask=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Circuit score strokes only",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            if(!scoreMask.Create()){ReleaseScoreMask();glyphMaskFailed=true;Debug.LogWarning("CursorDash: score-stroke mask target unavailable; projected score stays unchanged.");return;}glyphMaskFailed=false;
        }
        var previous=RenderTexture.active;float ratio=w/Mathf.Max(1f,board.pixelWidth),padding=(V==null?.65f:Value(V.circuitScoreMaskPadding,.65f,0,2))*ratio,softness=(V==null?.75f:Value(V.circuitScoreMaskSoftness,.75f,.25f,2))*ratio;
        try{
            buffer.Clear();buffer.SetRenderTarget(scoreMask);buffer.SetViewport(new Rect(0,0,w,h));buffer.ClearRenderTarget(false,true,Color.clear);buffer.SetViewProjectionMatrices(board.worldToCameraMatrix,board.projectionMatrix);
            int drawn=DrawScoreRoot(scoreRoot,1,padding,softness);
            Graphics.ExecuteCommandBuffer(buffer);scoreMaskReady=drawn>0;
        }finally{RenderTexture.active=previous;}
    }
    private void RenderNormal()
    {
        if(normalMask==null)return;var previous=RenderTexture.active;
        try{
            buffer.Clear();buffer.SetRenderTarget(normalMask);buffer.SetViewport(new Rect(0,0,normalMask.width,normalMask.height));buffer.ClearRenderTarget(false,true,Color.clear);buffer.SetViewProjectionMatrices(board.worldToCameraMatrix,board.projectionMatrix);buffer.DrawMesh(normalMesh,Matrix4x4.identity,normalMaterial);
            Graphics.ExecuteCommandBuffer(buffer);
        }
        finally{RenderTexture.active=previous;}
    }
    public void RenderFever(RenderTexture target)
    {
        buffer.Clear();buffer.SetRenderTarget(target);buffer.SetViewport(new Rect(0,0,target.width,target.height));buffer.ClearRenderTarget(false,true,Color.clear);
        if(Active&&!dying){fever.World.SetFlow(feverMaterial,target.width,target.height);buffer.SetViewProjectionMatrices(board.worldToCameraMatrix,board.projectionMatrix);buffer.DrawMesh(feverMesh,Matrix4x4.identity,feverMaterial);}
        Graphics.ExecuteCommandBuffer(buffer);
    }
    public void ResetSpace()
    {
        enabledOverride=hitsOverride=retreatOverride=scoreMaskOverride=ambientOverride=twoTierOverride=null;ContrastPreview=false;
        dying=restoring=deathFever=false;deathAge=0;nexusWaveAge=-1;previousPhase=FeverState.Phase.Normal;reactions?.Clear();ambient?.Clear();
        if(graph==null)return;
        int density=Mathf.Clamp(V==null?12:V.circuitDensity,4,12);if(density!=graph.Roots.Length)RebuildLayout();
        CaptureEntry();CaptureExit();CaptureRetreat();
        for(int n=0;n<graph.Roots.Length;n++){normalFronts[n]=graph.RootLengths[n];feverFronts[n]=deathFronts[n]=0;feverStarts[n]=restoreStarts[n]=float.PositiveInfinity;}
    }
    private void OnDisable(){nexusWaveAge=-1;scoreMaskReady=false;if(legacy!=null)legacy.DynamicCircuits=false;if(view!=null)view.enabled=false;reactions?.Clear();ambient?.Clear();normalMesh?.Clear();feverMesh?.Clear();}
    private void OnDestroy()
    {
        Camera.onPreCull-=BeforeBoardRender;
        if(stage!=null&&stage.Feedback!=null)stage.Feedback.Signaled-=OnHit;if(fever?.Model!=null)fever.Model.Transitioned-=Transition;
        if(legacy!=null)legacy.DynamicCircuits=false;buffer?.Release();ReleaseMask();ReleaseScoreMask();
        if(artwork!=null)Destroy(artwork);if(normalMesh!=null)Destroy(normalMesh);if(feverMesh!=null)Destroy(feverMesh);if(displayMesh!=null)Destroy(displayMesh);if(normalMaterial!=null)Destroy(normalMaterial);if(feverMaterial!=null)Destroy(feverMaterial);if(compositeMaterial!=null)Destroy(compositeMaterial);if(scoreMaskMaterial!=null)Destroy(scoreMaskMaterial);
    }
    private sealed class Geometry
    {
        private readonly List<Vector3> vertices=new List<Vector3>(6000),uv=new List<Vector3>(6000);
        private readonly List<Vector2> screen=new List<Vector2>(6000);private readonly List<Color> colors=new List<Color>(6000);private readonly List<int> triangles=new List<int>(9000);
        public void Clear(){vertices.Clear();uv.Clear();screen.Clear();colors.Clear();triangles.Clear();}
        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color tint,int kind,Camera camera,Color? endTint=null)
        {
            int n=vertices.Count;Add(a,-1,-1,tint);Add(b,-1,1,tint);Add(c,1,1,endTint??tint);Add(d,1,-1,endTint??tint);
            void Add(Vector3 p,float x,float y,Color color){vertices.Add(p);uv.Add(new Vector3(x,y,kind));screen.Add(camera.WorldToViewportPoint(p));colors.Add(color);}
            triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);triangles.Add(n);triangles.Add(n+2);triangles.Add(n+3);
        }
        public void Upload(Mesh mesh){mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetUVs(1,screen);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();}
    }
}
