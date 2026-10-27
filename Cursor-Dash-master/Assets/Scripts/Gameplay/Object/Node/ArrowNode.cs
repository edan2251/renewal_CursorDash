using UnityEngine;

/// <summary>Ring-driven charge, six-vertex morph and piston artwork. Existing beam collision and flight clocks remain authoritative.</summary>
public class ArrowNode : Node
{
    [SerializeField,HideInInspector] private float chargeBeatRiseSeconds=.18f; // Historic serialized value, retained for user data.
    [SerializeField,HideInInspector] private float chargeBeatSettleSeconds=.14f; // Historical seconds; stages now own beat lengths.
    [Header("Rotation timeline — tune in the existing developer tool")]
    [SerializeField,Range(0,.15f)] private float chargeBeatElasticity=.06f;
    [SerializeField,HideInInspector] private float chargeBeatHoldSeconds=.95f; // Historic time hold; no longer drives rotation charge.
    [SerializeField,HideInInspector] private float attackDuration=.06f; // Historical serialized time; developer default/preset is authoritative.
    [Tooltip("회전이 없을 때의 3박 환산 기준시간. 1박=이 값/3이며 발사 시점에 따라 총 시간은 달라집니다.") ]
    [SerializeField,Min(.05f)] private float preparationDuration=3.5f;
    [SerializeField,HideInInspector] private float warningDuration=.25f; // Old seconds retained in serialized assets only.
    [Tooltip("이전 저장 호환값. 현재 발사 전 멈춤은 테두리의 실제 정지 간격을 사용합니다.")]
    [SerializeField,Min(0)] private float preFireDuration=.025f;
    [SerializeField,Range(0,.2f)] private float preFireRecoilRatio=.08f;
    [SerializeField,Min(.02f)] private float firingRecoilDuration=.08f;
    [SerializeField,Min(0)] private float beamAfterglowDuration=.06f;
    [SerializeField,Min(.001f)] private float reflectionDuration=.06f;
    [SerializeField,Min(.05f)] private float destroyDuration=1.333f;
    [SerializeField,Min(.1f)] private float triangleRadius=.8f;
    [SerializeField] private Color triangleColor=new Color(1,.83f,.4f);
    private LineRenderer lineRenderer;
    private Transform inside,outsideOne,outsideTwo;
    private GameObject node;
    private SpriteRenderer insideRenderer,outsideOneRenderer,outsideTwoRenderer,bodyRenderer;
    private SpriteRenderer[] originalSprites;
    private bool[] originalSpriteStates;
    private Rotator oneRotator,twoRotator;
    private Vector3 initialNodeScale,initialOutsideOneScale,initialOutsideTwoScale;
    private Vector3 initialNodePosition;
    private Vector2 preFireDirection;
    private bool preFiring;
    private float activePreFire,activePreFireRatio;
    private bool beamArrived,recoiling;
    private float fireElapsed,activeRecoil,activeAfterglow,afterglowElapsed;
    public bool IsPreFiring=>Phase==NodePhase.Warning&&preFiring;
    public float PreFireSeconds=>activePreFire;
    public bool AfterglowOnly=>Phase==NodePhase.Fired&&beamArrived;
    public float WarningCompletedAt {get;private set;}=-1;
    public float MorphStartedAt {get;private set;}=-1;
    public float MorphSeconds=>morph;
    public float FiredAt {get;private set;}=-1;
    public float BlockedAt {get;private set;}=-1;
    public float NexusReachedAt {get;private set;}=-1;
    private Quaternion initialOutsideOneRotation,initialOutsideTwoRotation;
    private Quaternion warningOneRotation,warningTwoRotation;
    private float warningElapsed;
    private double rotationClock,rotationMovement,rotationPause;
    private Rotator chargeSource;
    private int timelineEvents;
    public HexRotationTimeline Timeline {get;private set;}
    public double TimelineBeat {get;private set;}
    public double BeatSeconds=>rotationMovement+rotationPause;
    public float SpawnedAt {get;private set;}
    public string TimelineReference {get;private set;}="";
    public float TimelineStepDegrees {get;private set;}
    public int TimelineEventsHandled {get;private set;}
    public float FlightSeconds=>attack;
    public float DisplayedCharge=>ChargeVisual;
    public Rotator ConfiguredReference {
        get {var first=oneRotator!=null?oneRotator:transform.Find("Node/Outside 1")?.GetComponent<Rotator>();var second=twoRotator!=null?twoRotator:transform.Find("Node/Outside 2")?.GetComponent<Rotator>();
            return first!=null&&Mathf.Abs(first.StepDegrees)>.0001f&&first.ConfiguredTurnSeconds>0?first:second;}
    }
    private int initialOutsideOneOrder,initialOutsideTwoOrder,generation;
    private float initialLineWidth,initialStartWidth,beamDefenseRadius;
    private Gradient initialGradient;
    private Vector2 tip,origin,target,blockPoint;
    private float elapsed,prepare,morph,attack,returnTime,resolve,radius,width,damage,rotation;
    private bool impacted;
    private Color bodyColor,beamColor,returnColor;
    private Mesh mesh;
    private Material material;
    private Transform triangleVisual;
    private MeshRenderer triangleRenderer;
    private NodeFlatGraphic flat;
    private HexagonalNodeRings originalRings;
    private float shortRatio,energyCurve;
    private Vector2[] hexPoints;
    public float EnergyRemaining {get;private set;}
    public float ChargeRotationTurns {get;private set;}
    public bool ChargeFollowsRotation {get;private set;}
    public int ChargeStepsCompleted {get;private set;}
    private float ChargeVisual=>Timeline==null?0:(float)Timeline.Fill(TimelineBeat,Phase==NodePhase.Preparing?chargeBeatElasticity:0,out _);
    public Vector2 BeamOutlet=>flat==null?(Vector2)transform.position:(Vector2)flat.transform.TransformPoint((hexPoints[0]+hexPoints[1])*.5f);
    private const int Segments=60;
    private readonly Vector3[] vertices=new Vector3[Segments+1];
    public Vector2 LastBlockPoint=>blockPoint;
    public float ReturnProgress {get;private set;}
    public float ReturnSeconds=>returnTime;
    public bool ReflectedReturnPending=>Phase==NodePhase.Resolving&&!impacted&&BlocksThisExecution>0;
    public float TutorialFootprint {get{float r=0;foreach(var path in HexagonalNodeOutlineA.Paths)foreach(var p in path)r=Mathf.Max(r,p.magnitude);var n=node!=null?node.transform:transform.Find("Node");var outside=n.Find("Outside 1");float child=initialNodeScale.x==0?n.localScale.x:initialNodeScale.x,ring=initialOutsideOneScale.x==0?outside.localScale.x:initialOutsideOneScale.x;return (r+.02f)*Mathf.Abs(child*ring*transform.lossyScale.x);}}
    public bool TriangleVisible=>flat!=null&&flat.Visible;
    public int ImpactsThisExecution {get;private set;}
    public int BlocksThisExecution {get;private set;}
    // Legacy property name retained for tool/test callers; now the real six-point silhouette.
    public Vector3[] TriangleWorldPoints {get{var p=new Vector3[6];for(int i=0;i<6;i++)p[i]=flat.transform.TransformPoint(hexPoints[i]);return p;}}
    private new void Awake()
    {
        base.Awake();lineRenderer=GetComponent<LineRenderer>();
        node=transform.GetChild(0).gameObject;inside=node.transform.GetChild(0);
        outsideOne=node.transform.Find("Outside 1");outsideTwo=node.transform.Find("Outside 2");
        insideRenderer=inside.GetComponent<SpriteRenderer>();bodyRenderer=node.GetComponent<SpriteRenderer>();
        originalSprites=node.GetComponentsInChildren<SpriteRenderer>(true);originalSpriteStates=new bool[originalSprites.Length];
        for(int i=0;i<originalSprites.Length;i++)originalSpriteStates[i]=originalSprites[i].enabled;
        outsideOneRenderer=outsideOne.GetComponent<SpriteRenderer>();outsideTwoRenderer=outsideTwo.GetComponent<SpriteRenderer>();
        oneRotator=outsideOne.GetComponent<Rotator>();twoRotator=outsideTwo.GetComponent<Rotator>();
        initialNodeScale=node.transform.localScale;initialOutsideOneScale=outsideOne.localScale;initialOutsideTwoScale=outsideTwo.localScale;
        initialNodePosition=node.transform.localPosition;
        initialOutsideOneRotation=outsideOne.localRotation;initialOutsideTwoRotation=outsideTwo.localRotation;
        initialOutsideOneOrder=outsideOneRenderer.sortingOrder;initialOutsideTwoOrder=outsideTwoRenderer.sortingOrder;
        initialLineWidth=lineRenderer.widthMultiplier;initialStartWidth=lineRenderer.startWidth;beamDefenseRadius=initialStartWidth*.5f;
        initialGradient=lineRenderer.colorGradient;
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.MarkDynamic();
        mesh.vertices=vertices;int[] tris=new int[Segments*3];
        for(int i=0;i<Segments;i++){tris[i*3]=0;tris[i*3+1]=i+1;tris[i*3+2]=(i+1)%Segments+1;}
        mesh.triangles=tris;mesh.uv=new Vector2[vertices.Length];
        material=new Material(insideRenderer.sharedMaterial){hideFlags=HideFlags.DontSave};material.mainTexture=Texture2D.whiteTexture;
        var visual=new GameObject("TriangleMorph"){hideFlags=HideFlags.DontSave};
        triangleVisual=visual.transform;triangleVisual.SetParent(node.transform,false);
        visual.AddComponent<MeshFilter>().sharedMesh=mesh;triangleRenderer=visual.AddComponent<MeshRenderer>();
        triangleRenderer.sharedMaterial=material;triangleRenderer.sortingLayerID=insideRenderer.sortingLayerID;triangleRenderer.sortingOrder=insideRenderer.sortingOrder+2;
        triangleRenderer.enabled=false;
        flat=NodeFlatGraphic.Create(node.transform,insideRenderer);
        originalRings=HexagonalNodeRings.Create(node.transform,outsideOneRenderer,outsideTwoRenderer);
        triangleRenderer.forceRenderingOff=true;
        foreach(var sr in originalSprites)sr.forceRenderingOff=true;
        flat.Visible=false;hexPoints=NodeFlatGraphic.Hexagon(HexagonalNodeRings.BodyRadius,1);
    }
    public override void Interaction(){}
    public override void ShowEffect(){}
    public override void Execute()
    {
        generation++;BeginExecution();RestoreVisuals();
        ChargeRotationTurns=0;ChargeStepsCompleted=1;ChargeFollowsRotation=false;
        // Restart only this hex's two owned ring components, after restoring their initial transforms.
        oneRotator?.RestartRotation();twoRotator?.RestartRotation();
        prepare=CursorDashTuning.Number("hex.prepare",preparationDuration);
        chargeSource=oneRotator!=null&&oneRotator.HasRotation?oneRotator:twoRotator!=null&&twoRotator.HasRotation?twoRotator:null;
        ChargeFollowsRotation=chargeSource!=null;
        rotationMovement=chargeSource!=null?chargeSource.TurnSeconds:Mathf.Max(.001f,prepare)/6;
        rotationPause=chargeSource!=null?chargeSource.PauseSeconds:rotationMovement;
        Timeline=CursorDashTuning.HexTimeline(true);
        if(Timeline.Warning!=""){CursorDashTuning.TimelineRejected(Timeline.Warning);ResetObject();return;}
        SpawnedAt=Time.time;TimelineReference=chargeSource==null?"회전 없음 · hex.prepare/3 대체박자":chargeSource.name;
        TimelineStepDegrees=chargeSource==null?0:chargeSource.StepDegrees;
        morph=(float)(Timeline.MorphLength*BeatSeconds);
        attack=(float)CursorDashTuning.DoubleNumber("hex.attack",HexRotationTimeline.DefaultFlightSeconds);
        returnTime=CursorDashTuning.Number("hex.return",reflectionDuration);
        resolve=CursorDashTuning.Number("hex.destroy",destroyDuration);
        radius=CursorDashTuning.Number("hex.radius",triangleRadius);
        width=CursorDashTuning.Number("hex.visualWidth",initialStartWidth*.65f);
        beamDefenseRadius=CursorDashTuning.Number("hex.hitWidth",initialStartWidth)*.5f;
        damage=CursorDashTuning.Number("hex.damage",deducationHp);
        rotation=CursorDashTuning.Number("hex.rotation",0);
        shortRatio=CursorDashTuning.Number("visual.hexShortEdge",.18f);
        energyCurve=CursorDashTuning.Number("visual.energyCurve",1);
        activePreFire=(float)System.Math.Max(0,(Timeline.FireAt-Timeline.MorphAt-Timeline.MorphLength)*BeatSeconds);activePreFireRatio=preFireRecoilRatio;preFiring=false;
        activeRecoil=Mathf.Max(.02f,firingRecoilDuration);activeAfterglow=Mathf.Max(0,beamAfterglowDuration);
        beamArrived=recoiling=false;fireElapsed=afterglowElapsed=0;
        MorphStartedAt=WarningCompletedAt=FiredAt=BlockedAt=NexusReachedAt=-1;
        bodyColor=CursorDashTuning.ColorValue("hex.color",triangleColor);
        beamColor=CursorDashTuning.ColorValue("hex.beamColor",bodyColor);
        returnColor=CursorDashTuning.ColorValue("hex.returnColor",bodyColor);
        node.SetActive(true);spriteRenderer.color=Color.clear;animator.enabled=false;circleCollider.enabled=false;
        inside.localScale=Vector3.one*(float)Timeline.Born;lineRenderer.enabled=false;elapsed=0;impacted=false;
        ReturnProgress=0;ImpactsThisExecution=BlocksThisExecution=0;
        EnergyRemaining=1;flat.transform.localRotation=Quaternion.identity;
        DrawMorph(0);
    }
    private void Update()
    {
        var stage=StageManager.instance;if(stage==null||!stage.IsPlaying||Time.deltaTime<=0)return;
        elapsed+=Time.deltaTime;
        if(recoiling)AdvanceFiringRecoil(Time.deltaTime);
        if(Phase==NodePhase.Preparing||Phase==NodePhase.Warning)return; // Sample the same ring tween after its normal Update.
        else if(Phase==NodePhase.Fired)
        {
            if(beamArrived){
                // Visual tail only. Never re-query defense, damage or the return path after contact.
                afterglowElapsed+=Time.deltaTime;
                float fade=activeAfterglow<=0?0:1-Mathf.Clamp01(afterglowElapsed/activeAfterglow);
                var color=beamColor;color.a*=fade;lineRenderer.startColor=lineRenderer.endColor=color;
                DrawBeam(target,false);if(fade<=0)ResetObject();return;
            }
            tip=Vector2.Lerp(origin,target,Mathf.Clamp01(elapsed/attack));DrawBeam(tip,false);
            EnergyRemaining=Mathf.Pow(1-Mathf.Clamp01(elapsed/attack),energyCurve);
            Vector2 contact;var trail=Demonstration==null?PlayerCharacterAfterImage.instance:Demonstration.Trail;
            if(trail!=null&&trail.TryBlockBeam(origin,tip,beamDefenseRadius,out contact)){Block(contact);return;}
            if(elapsed<attack)return;
            // Commit BEFORE DamageNexus: lethal damage synchronously resets all pooled nodes.
            beamArrived=true;afterglowElapsed=0;NexusReachedAt=Time.time;
            ImpactsThisExecution++;
            if(Demonstration!=null){Demonstration.Missed(this);return;}
            NormalNode.DefenseStreak=0;stage.DamageNexusFromLaser(damage,target,(target-origin).normalized,beamColor);
            if(Phase==NodePhase.Fired&&activeAfterglow<=0)ResetObject();
        }
        else if(Phase==NodePhase.Resolving)
        {
            if(!impacted)
            {
                ReturnProgress=Mathf.Clamp01(elapsed/Mathf.Max(.001f,returnTime));
                // Preserve the visible original segment at the turn. Its head retreats to the node.
                tip=Vector2.Lerp(blockPoint,origin,ReturnProgress);DrawBeam(Vector2.Lerp(blockPoint,BeamOutlet,ReturnProgress),true);
                if(ReturnProgress>=1)Impact();
            }
            else if(elapsed>=resolve)ResetObject();
        }
    }
    private void AdvanceRotationCharge(StageManager stage)
    {
        // Snapshot values and the clock owner never change midway through an execution.
        if(Timeline==null)return;
        if(chargeSource!=null&&!chargeSource.HasRotation){ResetObject();return;}
        rotationClock=chargeSource!=null?chargeSource.RotationElapsedSeconds:elapsed;
        TimelineBeat=System.Math.Max(TimelineBeat,HexRotationTimeline.Beat(rotationClock,rotationMovement,rotationPause));
        int crossed=Timeline.DueEvents(TimelineBeat)&~timelineEvents;timelineEvents|=crossed;
        for(int bit=1;bit<=16;bit<<=1)if((crossed&bit)!=0)TimelineEventsHandled++;
        ChargeRotationTurns=(float)NodeRhythmMath.RotationTurns(rotationClock,rotationMovement,rotationPause);
        if(Phase==NodePhase.Preparing){
            ChargeStepsCompleted=Timeline.CompletedCharges(TimelineBeat);
            inside.localScale=Vector3.one*ChargeVisual;
            if((timelineEvents&4)==0)return;
            inside.localScale=Vector3.one;Phase=NodePhase.Warning;MorphStartedAt=Time.time;
            // Keep the native clock alive until fire; the absorbed artwork is drawn explicitly below.
            warningOneRotation=outsideOne.localRotation;warningTwoRotation=outsideTwo.localRotation;
            Vector2 toward=stage.NexusPosition-(Vector2)transform.position;
            triangleVisual.rotation=Quaternion.Euler(0,0,Mathf.Atan2(toward.y,toward.x)*Mathf.Rad2Deg+rotation);
            if(bodyRenderer!=null)bodyRenderer.enabled=false;insideRenderer.enabled=false;triangleRenderer.enabled=true;DrawMorph(0);
            foreach(var sr in originalSprites)if(sr!=outsideOneRenderer&&sr!=outsideTwoRenderer)sr.enabled=false;
        }
        warningElapsed=(float)System.Math.Max(0,(TimelineBeat-Timeline.MorphAt)*BeatSeconds);
        AdvanceWarning(stage);
    }
    private void AdvanceWarning(StageManager stage)
    {
        if(preFiring){AdvancePreFire(stage);return;}
        float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(warningElapsed/morph));
        node.transform.localScale=initialNodeScale*Mathf.Lerp(1,.92f,t);
        outsideOne.localScale=initialOutsideOneScale*(1-t);outsideTwo.localScale=initialOutsideTwoScale*(1-t);
        outsideOne.localRotation=warningOneRotation*Quaternion.Euler(0,0,150*t);
        outsideTwo.localRotation=warningTwoRotation*Quaternion.Euler(0,0,-150*t);
        DrawMorph(t);if(warningElapsed<morph-.000001f)return;
        outsideOne.localScale=outsideTwo.localScale=Vector3.zero;
        outsideOneRenderer.enabled=outsideTwoRenderer.enabled=false;DrawMorph(1);
        foreach(var sr in originalSprites)sr.enabled=false;
        preFiring=true;WarningCompletedAt=Time.time;preFireDirection=(stage.NexusPosition-(Vector2)transform.position).normalized;
        AdvancePreFire(stage);
    }
    private void AdvancePreFire(StageManager stage)
    {
        // The final aiming pose is held briefly; the main recoil belongs to the actual shot.
        if((timelineEvents&16)!=0)BeginFire(stage);
    }
    private void AdvanceFiringRecoil(float delta)
    {
        fireElapsed+=delta;float t=Mathf.Clamp01(fireElapsed/activeRecoil);
        float push=t<.15f?Mathf.Sin(t/.15f*Mathf.PI*.5f):Mathf.Pow(1-(t-.15f)/.85f,2);
        float distance=radius*Mathf.Abs(node.transform.lossyScale.x)*activePreFireRatio;
        node.transform.localPosition=initialNodePosition-transform.InverseTransformVector((Vector3)preFireDirection*distance*push);
        if(t>=1){recoiling=false;node.transform.localPosition=initialNodePosition;}
    }
    private void BeginFire(StageManager stage)
    {
        // No scheduled callback: disabling/resetting this execution can never emit a delayed beam.
        if(oneRotator!=null)oneRotator.enabled=false;if(twoRotator!=null)twoRotator.enabled=false;
        preFiring=false;node.transform.localPosition=initialNodePosition;
        Phase=NodePhase.Fired;elapsed=0;origin=transform.position;tip=origin;
        FiredAt=Time.time;beamArrived=false;fireElapsed=0;recoiling=true;
        AdvanceFiringRecoil(Mathf.Min(Time.deltaTime,.006f));
        target=stage.NexusPosition+(origin-stage.NexusPosition).normalized*stage.NexusRadius;
        stage.Practice?.TraceBeam(this,Demonstration==null?"practice fire":"ghost fire",origin);
        lineRenderer.widthMultiplier=width/Mathf.Max(.0001f,initialStartWidth);
        lineRenderer.startColor=lineRenderer.endColor=beamColor;lineRenderer.enabled=true;
        DrawBeam(origin,false);
    }
    private void Block(Vector2 contact)
    {
        if(Phase!=NodePhase.Fired)return;
        Phase=NodePhase.Resolving;elapsed=0;blockPoint=tip=contact;BlocksThisExecution++;BlockedAt=Time.time;
        DrawBeam(contact,true);
        lineRenderer.startColor=lineRenderer.endColor=returnColor;
        if(Demonstration!=null)Demonstration.Reflected(this,contact);
        else {StageManager.instance.Practice?.TraceBeam(this,"player trail reflected",contact);NormalNode.DefenseStreak++;StageManager.instance.RegisterHexBlocked(contact);}
    }
    private void Impact()
    {
        if(impacted||Phase!=NodePhase.Resolving)return;
        impacted=true;ImpactsThisExecution++;elapsed=0;lineRenderer.enabled=false;
        Vector3[] silhouette=TriangleWorldPoints;
        StageManager.instance.Practice?.TraceBeam(this,"returned impact",BeamOutlet);
        if(Demonstration!=null)Demonstration.Killed(this,1,BeamOutlet,(origin-blockPoint).normalized,silhouette);
        else StageManager.instance.RegisterHexKill(this,BeamOutlet,silhouette,bodyColor,(origin-blockPoint).normalized);
        flat.Visible=false;
        triangleRenderer.enabled=false;node.SetActive(false);
    }
    private void DrawMorph(float t)
    {
        if(flat==null)return;
        hexPoints=NodeFlatGraphic.Hexagon(Mathf.Lerp(HexagonalNodeRings.BodyRadius,radius,t),Mathf.Lerp(1,shortRatio,t));
        flat.transform.rotation=Quaternion.Slerp(node.transform.rotation,triangleVisual.rotation,t);
    }
    private void DrawBeam(Vector2 head,bool returning)
    {
        Vector2 outlet=BeamOutlet;
        if(!returning&&Vector2.Dot(head-outlet,(target-origin).normalized)<0)head=outlet;
        lineRenderer.SetPosition(0,outlet);lineRenderer.SetPosition(1,head);
    }
    private void LateUpdate()
    {
        if(flat==null)return;
        var stage=StageManager.instance;
        if((Phase==NodePhase.Preparing||Phase==NodePhase.Warning)&&stage!=null&&stage.IsPlaying&&Time.deltaTime>0)AdvanceRotationCharge(stage);
        if(Phase==NodePhase.Inactive||impacted){flat.Visible=false;if(originalRings!=null)originalRings.Visible=false;return;}
        float t=Phase==NodePhase.Preparing?0:Phase==NodePhase.Warning&&!preFiring?Mathf.SmoothStep(0,1,Mathf.Clamp01(warningElapsed/morph)):1;
        float charge=ChargeVisual;
        float ring=Phase==NodePhase.Preparing?1:Phase==NodePhase.Warning?1-t:0;
        flat.DrawHexagonA(Mathf.Lerp(HexagonalNodeRings.BodyRadius,radius,t),Mathf.Lerp(1,shortRatio,t),charge,EnergyRemaining,
            Phase==NodePhase.Fired||Phase==NodePhase.Resolving,bodyColor,t);
        double pulse=0;Timeline?.Fill(TimelineBeat,chargeBeatElasticity,out pulse);
        flat.SetChargePulse(Phase==NodePhase.Preparing?(float)pulse:0);
        flat.transform.localScale=Vector3.one*EntranceBody;
        if(originalRings!=null)originalRings.Draw(bodyColor,ring);
        if(originalRings!=null)originalRings.ApplyEntrance(EntranceRing,EntranceTurn);
    }
    private void RestoreVisuals()
    {
        if(node==null)return;
        ChargeRotationTurns=0;ChargeStepsCompleted=0;ChargeFollowsRotation=false;
        chargeSource=null;Timeline=null;TimelineBeat=0;timelineEvents=TimelineEventsHandled=0;TimelineReference="";TimelineStepDegrees=0;
        preFiring=beamArrived=recoiling=false;rotationClock=rotationMovement=rotationPause=0;elapsed=fireElapsed=afterglowElapsed=warningElapsed=0;node.transform.localPosition=initialNodePosition;
        if(flat!=null){flat.Visible=false;flat.transform.localRotation=Quaternion.identity;}
        if(originalRings!=null)originalRings.Visible=false;
        node.SetActive(true);node.transform.localScale=initialNodeScale;
        outsideOne.localScale=initialOutsideOneScale;outsideTwo.localScale=initialOutsideTwoScale;
        outsideOne.localRotation=initialOutsideOneRotation;outsideTwo.localRotation=initialOutsideTwoRotation;
        outsideOneRenderer.sortingOrder=initialOutsideOneOrder;outsideTwoRenderer.sortingOrder=initialOutsideTwoOrder;
        outsideOneRenderer.enabled=outsideTwoRenderer.enabled=insideRenderer.enabled=true;
        if(bodyRenderer!=null)bodyRenderer.enabled=true;
        for(int i=0;i<originalSprites.Length;i++)originalSprites[i].enabled=originalSpriteStates[i];
        if(oneRotator!=null)oneRotator.enabled=true;if(twoRotator!=null)twoRotator.enabled=true;
        if(triangleRenderer!=null)triangleRenderer.enabled=false;
        inside.localScale=Vector3.zero;lineRenderer.widthMultiplier=initialLineWidth;lineRenderer.colorGradient=initialGradient;lineRenderer.enabled=false;
    }
    public override void ResetObject()
    {
        generation++;RestoreVisuals();base.ResetObject();
        spriteRenderer.color=Color.clear;animator.enabled=false;circleCollider.enabled=false;
    }
    private void OnDisable()
    {
        generation++;if(node!=null){RestoreVisuals();Phase=NodePhase.Inactive;transform.localPosition=Vector3.zero;}
    }
    private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
