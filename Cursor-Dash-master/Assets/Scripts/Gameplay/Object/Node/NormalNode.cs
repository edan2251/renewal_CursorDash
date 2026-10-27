using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>First visual draft: a destructible charge morphs into the node's own unreflectable dash.</summary>
public class NormalNode : Node
{
    private static int defenseStreak;
    public static int DefenseStreak
    {
        get => defenseStreak;
        set
        {
            defenseStreak = value;
            if ((value == 15 || value == 20) && StageManager.instance!=null && StageManager.instance.RecordEligible)
                GameManager.instance.playerData.AchieveRequire.SetValueToRequire("MaxDefence_Cyberpunk", value);
        }
    }

    [SerializeField] private GameObject effectObject;
    [SerializeField] private Animator childAniamtor;
    [FormerlySerializedAs("lineDuration")]
    [SerializeField, Min(0.01f)] private float dashDuration = 0.20f;
    [SerializeField, Min(0.05f)] private float preparationDuration = 2.5f;
    [SerializeField, Min(0.01f)] private float warningDuration = 0.15f;
    [SerializeField, Range(0.1f, 2f)] private float cursorScale = 0.60f;
    [SerializeField, Min(0f)] private float recoilDuration = 0.05f;
    [SerializeField, Range(0f, 0.5f)] private float recoilDistanceRatio = 0.08f;
    [SerializeField, Range(0, 2)] private int recoilCurve = 1;
    [SerializeField, Range(0, 2)] private int morphCurve = 1;
    [Header("Circular cursor draft - awaiting visual review")]
    [SerializeField, Min(0.2f)] private float cursorLength = 1.6f;
    [SerializeField, Min(0.1f)] private float cursorWidth = 1.05f;
    [SerializeField, Min(0.05f)] private float impactDuration = 0.4f;
    [SerializeField] private Color cursorColor = new Color(0.68f, 0.12f, 1f, 1f);

    private LineRenderer lineRenderer; // Legacy component retained for prefab compatibility, always off.
    private Transform outsideRing, insideRing;
    private SpriteRenderer outsideRenderer, insideRenderer, breakVisual;
    private Rotator ringRotator;
    private bool initialRotationEnabled;
    private Vector3 initialScale, initialRingScale;
    private Quaternion initialRotation, initialRingRotation;
    private Color initialRingColor;
    private int initialOutsideOrder;
    private Tween breakTween, impactTween;
    private Transform cursorVisual, impactVisual;
    private Mesh cursorMesh, impactMesh;
    private MeshRenderer cursorRenderer, impactRenderer;
    private Material cursorMaterial, impactMaterial;
    private CircularNodeArtwork flat;
    private const int Segments = 64;
    private readonly Vector3[] vertices = new Vector3[Segments + 1];
    private readonly Color[] colors = new Color[Segments + 1];
    private float elapsed, morph;
    private float activePreparation, activeMorph, activeDash, activeRecoil, activeRatio, activeLength=1.6f, activeWidth=1.05f, activeScale=0.6f, activeImpact, activeDamage;
    private int activeRecoilCurve, activeMorphCurve;
    private Color activeColor;
    private Vector3 recoilOrigin;
    private bool recoilStarted;
    public bool IsRecoiling => Phase == NodePhase.Warning && recoilStarted;
    public float RecoilWorldDistance => activeLength * activeScale * Mathf.Abs(transform.lossyScale.y) * activeRatio;
    public float CursorWorldLength => activeLength * activeScale * Mathf.Abs(transform.lossyScale.y);
    public float DashSeconds => activeDash;
    private Vector2 dashStart, dashEnd, dashDirection, previousTip;
    private bool damageCommitted;
    public Vector2 LeadingTip => cursorVisual == null ? (Vector2)transform.position :
        (Vector2)cursorVisual.TransformPoint(new Vector3(0f, activeLength * activeScale * 0.68f));
    public Vector2 LastImpactPoint { get; private set; }
    public int ImpactsThisExecution { get; private set; }
    public float MorphProgress => morph;
    public float PreparationProgress=>Phase==NodePhase.Warning||Phase==NodePhase.Fired?1:activePreparation>0?Mathf.Clamp01(elapsed/activePreparation):0;
    public float RestContactRadius=>circleCollider==null?.2f:RestColliderRadius*Mathf.Abs(initialScale.x*transform.parent.lossyScale.x);
    public float PreparationSeconds=>activePreparation;
    public float LastRemovalCharge {get;private set;}
    public float TutorialFootprint {get{float r=0;foreach(var path in CircularNodeOutlineA.Paths)foreach(var p in path)r=Mathf.Max(r,p.magnitude);return (r+.02f)*Mathf.Abs(initialScale.x==0?transform.lossyScale.x:initialScale.x*transform.parent.lossyScale.x);}}

    private new void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        outsideRing = transform.Find("Outside");
        insideRing = transform.Find("Inside");
        outsideRenderer = outsideRing.GetComponent<SpriteRenderer>();
        insideRenderer = insideRing.GetComponent<SpriteRenderer>();
        breakVisual = transform.Find("BreakShell").GetComponent<SpriteRenderer>();
        ringRotator = outsideRing.GetComponent<Rotator>();
        initialRotationEnabled = ringRotator != null && ringRotator.enabled;
        initialScale = transform.localScale;
        initialRotation = transform.localRotation;
        initialRingScale = outsideRing.localScale;
        initialRingRotation = outsideRing.localRotation;
        initialRingColor = outsideRenderer.color;
        initialOutsideOrder = outsideRenderer.sortingOrder;
        CreateVisuals();
        flat=CircularNodeArtwork.Create(transform,spriteRenderer);
        spriteRenderer.forceRenderingOff=insideRenderer.forceRenderingOff=outsideRenderer.forceRenderingOff=true;
    }

    private void CreateVisuals()
    {
        cursorMaterial = new Material(spriteRenderer.sharedMaterial) { name = "Circular cursor (runtime)", hideFlags = HideFlags.DontSave };
        cursorMaterial.mainTexture = Texture2D.whiteTexture;
        impactMaterial = new Material(cursorMaterial) { name = "Circular impact (runtime)", hideFlags = HideFlags.DontSave };
        cursorMesh = new Mesh { name = "Circular morph (runtime)", hideFlags = HideFlags.DontSave };
        cursorMesh.MarkDynamic();
        var indices = new int[Segments * 3];
        for (int i = 0; i < Segments; i++)
        {
            indices[i * 3] = 0;
            indices[i * 3 + 1] = i + 1;
            indices[i * 3 + 2] = (i + 1) % Segments + 1;
        }
        cursorMesh.vertices = vertices;
        cursorMesh.triangles = indices;
        cursorMesh.uv = new Vector2[vertices.Length];
        cursorVisual = NewVisual("CursorMorph", cursorMesh, cursorMaterial, transform, out cursorRenderer);
        // One owned, short-lived visual can finish even if lethal damage immediately returns the node to its pool.
        impactMesh = new Mesh { name = "Circular impact shards (runtime)", hideFlags = HideFlags.DontSave };
        var points = new Vector3[24];
        var tris = new int[36];
        for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI / 3f;
            Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Vector2 side = new Vector2(-d.y, d.x);
            points[i * 4] = d * 0.12f;
            points[i * 4 + 1] = d * 0.45f + side * 0.055f;
            points[i * 4 + 2] = d * 0.7f;
            points[i * 4 + 3] = d * 0.45f - side * 0.055f;
            int b = i * 4, t = i * 6;
            tris[t] = b; tris[t + 1] = b + 1; tris[t + 2] = b + 2;
            tris[t + 3] = b; tris[t + 4] = b + 2; tris[t + 5] = b + 3;
        }
        impactMesh.vertices = points; impactMesh.triangles = tris;
        impactMesh.uv = new Vector2[points.Length];
        impactVisual = NewVisual("CircleDashImpact", impactMesh, impactMaterial, null, out impactRenderer);
        impactVisual.gameObject.SetActive(false);
        cursorRenderer.enabled = false;
    }

    private Transform NewVisual(string label, Mesh mesh, Material material, Transform parent, out MeshRenderer renderer)
    {
        var item = new GameObject(label) { layer = gameObject.layer, hideFlags = HideFlags.DontSave };
        item.transform.SetParent(parent, false);
        item.AddComponent<MeshFilter>().sharedMesh = mesh;
        renderer = item.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingLayerID = spriteRenderer.sortingLayerID;
        renderer.sortingOrder = spriteRenderer.sortingOrder + 2;
        return item.transform;
    }

    public override void Execute()
    {
        StopOwnedEffects();
        // Snapshot at generation: editing the panel never changes an in-flight attack.
        activePreparation=CursorDashTuning.Number("circle.prepare",preparationDuration);
        activeMorph=CursorDashTuning.Number("circle.morph",warningDuration);
        activeDash=CursorDashTuning.Number("circle.dash",dashDuration);
        activeRecoil=CursorDashTuning.Number("circle.recoil",recoilDuration);
        activeRatio=CursorDashTuning.Number("circle.recoilRatio",recoilDistanceRatio);
        activeLength=CursorDashTuning.Number("circle.length",cursorLength);
        activeWidth=CursorDashTuning.Number("circle.width",cursorWidth);
        activeScale=CursorDashTuning.Number("circle.scale",cursorScale);
        activeImpact=CursorDashTuning.Number("circle.impact",impactDuration);
        activeColor=CursorDashTuning.ColorValue("circle.color",cursorColor);
        activeDamage=CursorDashTuning.Number("circle.damage",deducationHp);
        activeRecoilCurve=CursorDashTuning.Integer("circle.recoilCurve",recoilCurve);
        activeMorphCurve=CursorDashTuning.Integer("circle.morphCurve",morphCurve);
        recoilStarted=false;
        BeginExecution();
        RestoreVisuals();
        if (animator != null) animator.enabled = false;
        elapsed = morph = 0f;
        damageCommitted = false;
        ImpactsThisExecution = 0;
        LastImpactPoint = Vector2.zero;
        if (circleCollider != null) circleCollider.enabled = true;
        ApplyEntranceCollider();
    }

    private void Update()
    {
        var stage = StageManager.instance;
        if (stage == null || !stage.IsPlaying || Time.deltaTime <= 0f) return;
        elapsed += Time.deltaTime;
        if (Phase == NodePhase.Preparing)
        {
            insideRing.localScale = new Vector3(Mathf.Clamp01(elapsed / activePreparation),
                Mathf.Clamp01(elapsed / activePreparation), 1f);
            if (elapsed < activePreparation) return;
            insideRing.localScale = Vector3.one;
            Phase = NodePhase.Warning;
            elapsed = 0f;
            if (ringRotator != null) ringRotator.enabled = false;
            dashDirection = (stage.NormalDashNexusPosition - (Vector2)transform.position).normalized;
            if (dashDirection == Vector2.zero) dashDirection = Vector2.down;
            cursorVisual.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg - 90f);
            spriteRenderer.enabled = insideRenderer.enabled = false;
            cursorRenderer.enabled = true;
            DrawMorph(0f);
        }
        else if (Phase == NodePhase.Warning)
        {
            if (recoilStarted)
            {
                float progress=activeRecoil<=0f?1f:Mathf.Clamp01(elapsed/activeRecoil);
                transform.position=recoilOrigin-(Vector3)dashDirection*(RecoilWorldDistance*CursorDashTuning.Curve(progress,activeRecoilCurve));
                if(progress<1f)return;
                BeginDash(stage);
                return;
            }
            morph = CursorDashTuning.Curve(elapsed / activeMorph, activeMorphCurve);
            transform.localScale = initialScale * Mathf.Lerp(1f, 0.92f, morph);
            outsideRing.localScale = initialRingScale * (1f - morph);
            outsideRing.localRotation = initialRingRotation * Quaternion.Euler(0f, 0f, 180f * morph);
            outsideRenderer.color = new Color(initialRingColor.r, initialRingColor.g, initialRingColor.b, 1f - morph);
            DrawMorph(morph);
            if (elapsed < activeMorph) return;
            outsideRing.localScale = Vector3.zero;
            outsideRenderer.enabled = false;
            morph = 1f;
            DrawMorph(1f);
            recoilStarted=true;
            recoilOrigin=transform.position;
            elapsed=0f;
        }
        else if (Phase == NodePhase.Fired)
        {
            Vector2 next = Vector2.Lerp(dashStart, dashEnd, Mathf.Clamp01(elapsed / Mathf.Max(0.001f, activeDash)));
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            Vector2 tip = LeadingTip;
            Vector2 hit;
            if (TryFirstSegmentCircleHit(previousTip, tip, stage.NormalDashNexusPosition, stage.NormalDashNexusRadius, out hit))
            {
                transform.position += (Vector3)(hit - tip);
                Impact(hit);
            }
            else if (elapsed >= activeDash)
            {
                Vector2 edge = stage.NormalDashNexusPosition - dashDirection * stage.NormalDashNexusRadius;
                transform.position += (Vector3)(edge - tip);
                Impact(edge);
            }
            previousTip = tip;
        }
        else if (Phase == NodePhase.Resolving && elapsed >= activeImpact)
            ResetObject();
    }

    private void BeginDash(StageManager stage)
    {
            Phase = NodePhase.Fired;
            isInteraction = true;
            if (circleCollider != null) circleCollider.enabled = false;
            elapsed = 0f;
            dashStart = transform.position;
            Vector2 center = stage.NormalDashNexusPosition;
            float nose = Vector2.Distance(transform.position, LeadingTip);
            dashEnd = center - dashDirection * (stage.NormalDashNexusRadius + nose);
            // An unusually close spawn is already touching; never dash away from the core.
            if (Vector2.Dot(dashEnd - dashStart, dashDirection) < 0f) dashEnd = dashStart;
            previousTip = LeadingTip;
    }

    private void Impact(Vector2 hit)
    {
        if (damageCommitted || Phase != NodePhase.Fired) return;
        damageCommitted = true;
        ImpactsThisExecution++;
        LastImpactPoint = hit;
        Phase = NodePhase.Resolving;
        elapsed = 0f;
        cursorRenderer.enabled = false;
        // The contact profile now belongs to Stage feedback, independent of pooled node reset.
        impactTween?.Kill();impactTween=null;impactVisual.gameObject.SetActive(false);
        if(Demonstration!=null){Demonstration.Missed(this);return;}
        DefenseStreak = 0;
        // Commit before DamageNexus: lethal damage synchronously calls StopNodes/ResetObject.
        StageManager.instance.DamageNexusFromCircle(activeDamage,hit,dashDirection,activeColor,activeImpact);
    }

    public static bool TryFirstSegmentCircleHit(Vector2 from, Vector2 to, Vector2 center, float radius, out Vector2 hit)
    {
        Vector2 offset = from - center, delta = to - from;
        float r = Mathf.Max(0f, radius);
        if (offset.sqrMagnitude <= r * r) { hit = from; return true; }
        float a = delta.sqrMagnitude;
        if (a > 0.00000001f)
        {
            float b = Vector2.Dot(offset, delta);
            float disc = b * b - a * (offset.sqrMagnitude - r * r);
            if (disc >= -0.000001f)
            {
                float t = (-b - Mathf.Sqrt(Mathf.Max(0f, disc))) / a;
                if (t >= 0f && t <= 1.00001f) { hit = from + delta * Mathf.Clamp01(t); return true; }
            }
        }
        hit = to;
        return false;
    }

    private void DrawMorph(float progress)
    {
        // Shared polar topology: circle -> rounded lance -> concave cursor. No sprite swap at the fire boundary.
        vertices[0] = Vector3.zero;
        Color morphColor = Color.Lerp(CircularNodeArtwork.FillColor, activeColor, progress);
        colors[0] = morphColor;
        Vector2[] polygon = { new Vector2(0f, activeLength * 0.68f), new Vector2(activeWidth * 0.5f, -activeLength * 0.32f),
            new Vector2(0f, -activeLength * 0.06f), new Vector2(-activeWidth * 0.5f, -activeLength * 0.32f) };
        for (int i = 0; i < Segments; i++)
        {
            float a = i * Mathf.PI * 2f / Segments;
            Vector2 ray = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            float distance = float.MaxValue;
            for (int j = 0; j < polygon.Length; j++)
            {
                Vector2 p = polygon[j], edge = polygon[(j + 1) % polygon.Length] - p;
                float cross = ray.x * edge.y - ray.y * edge.x;
                if (Mathf.Abs(cross) < 0.000001f) continue;
                float t = (p.x * edge.y - p.y * edge.x) / cross;
                float u = (p.x * ray.y - p.y * ray.x) / cross;
                if (t >= 0f && u >= -0.00001f && u <= 1.00001f) distance = Mathf.Min(distance, t);
            }
            if (distance == float.MaxValue) distance = 0.54f;
            Vector2 round = ray * CircularNodeArtwork.BodyRadius;
            Vector2 middle = new Vector2(ray.x * 0.45f, ray.y * (ray.y > 0f ? 0.85f : 0.44f)) * Mathf.Lerp(1f,activeScale,0.5f);
            Vector2 point = progress < 0.5f ? Vector2.Lerp(round, middle, progress * 2f) :
                Vector2.Lerp(middle, ray * distance * activeScale, (progress - 0.5f) * 2f);
            vertices[i + 1] = point;
            colors[i + 1] = morphColor;
        }
        cursorMesh.vertices = vertices; cursorMesh.colors = colors; cursorMesh.RecalculateBounds();
    }

    private Vector2? touchPoint;
    private Vector2 incomingMotion;
    public void InteractionAt(Vector2 point){touchPoint=point;Interaction();touchPoint=null;}
    public void ContactAt(Vector2 point,Vector2 motion){incomingMotion=motion;InteractionAt(point);incomingMotion=Vector2.zero;}
    public override void Interaction()
    {
        if ((Phase != NodePhase.Preparing && Phase != NodePhase.Warning) || !StageManager.instance.IsPlaying) return;
        LastRemovalCharge=PreparationProgress;
        bool wasMorphing=Phase==NodePhase.Warning;
        Phase = NodePhase.Resolving;
        elapsed = 0f;
        isInteraction = true;
        cursorRenderer.enabled = false;
        if (ringRotator != null) ringRotator.enabled = false;
        if(Demonstration==null)DefenseStreak++;
        spriteRenderer.enabled = insideRenderer.enabled = outsideRenderer.enabled = false;
        if (circleCollider != null) circleCollider.enabled = false;
        if(flat!=null)flat.Visible=false;
        Vector2 point=touchPoint??(Vector2)transform.position;
        Vector2 direction=incomingMotion.sqrMagnitude>.00001f?incomingMotion.normalized:((Vector2)transform.position-point).normalized;
        if(direction.sqrMagnitude<.1f)direction=Vector2.up;
        var shape=new Vector3[12];
        for(int i=0;i<shape.Length;i++){float a=i*Mathf.PI*2/shape.Length;shape[i]=wasMorphing?cursorVisual.TransformPoint(vertices[i*Segments/shape.Length+1]):transform.TransformPoint(new Vector3(Mathf.Cos(a),Mathf.Sin(a))*CircularNodeArtwork.BodyRadius);}
        if(Demonstration!=null)Demonstration.Killed(this,0,point,direction,shape);
        else StageManager.instance.RegisterCircleKill(this,point,direction,shape,effectObject);
    }

    public override void ShowEffect(Vector2 position)
    {
        // Contact feedback is emitted exactly once by Interaction. Keep the legacy API for callers.
    }

    private void LaunchBreak(Vector2 direction)
    {
        breakTween?.Kill();
        breakVisual.gameObject.SetActive(true);
        breakVisual.color = Color.white;
        breakVisual.transform.localPosition = Vector3.zero;
        breakVisual.transform.localScale = Vector3.one;
        breakVisual.transform.localRotation = Quaternion.identity;
        breakTween = DOTween.Sequence()
            .Join(breakVisual.transform.DOLocalMove((Vector3)(direction * 0.55f), 0.38f).SetEase(Ease.OutCubic))
            .Join(breakVisual.transform.DOScale(1.28f, 0.38f).SetEase(Ease.OutQuad))
            .Join(breakVisual.transform.DOLocalRotate(new Vector3(0f, 0f, 48f), 0.38f))
            .Join(breakVisual.DOFade(0f, 0.38f));
    }

    private void StopOwnedEffects()
    {
        breakTween?.Kill(); breakTween = null;
        impactTween?.Kill(); impactTween = null;
        if (impactVisual != null) impactVisual.gameObject.SetActive(false);
    }

    private void RestoreVisuals()
    {
        if(flat!=null)flat.Visible=false;
        transform.localScale = initialScale;
        transform.localRotation = initialRotation;
        outsideRing.localScale = initialRingScale;
        outsideRing.localRotation = initialRingRotation;
        outsideRenderer.color = initialRingColor;
        outsideRenderer.sortingOrder = initialOutsideOrder;
        spriteRenderer.enabled = outsideRenderer.enabled = insideRenderer.enabled = true;
        insideRing.localScale = new Vector3(0f, 0f, 1f);
        cursorVisual.localRotation = Quaternion.identity;
        cursorRenderer.enabled = false;
        if (ringRotator != null) ringRotator.enabled = initialRotationEnabled;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (circleCollider != null) circleCollider.enabled = true;
        if (effectObject != null) effectObject.SetActive(false);
        breakVisual.gameObject.SetActive(false);
    }

    public override void ResetObject()
    {
        breakTween?.Kill(); breakTween = null;
        RestoreVisuals();
        recoilStarted=false;
        elapsed = morph = 0f;
        base.ResetObject();
    }
    private void OnDisable()
    {
        breakTween?.Kill(); breakTween = null;
        if (cursorRenderer != null)
        {
            RestoreVisuals();
            recoilStarted=false;
            transform.localPosition = Vector3.zero;
            elapsed = morph = 0f;
            Phase = NodePhase.Inactive;
        }
    }
    private void OnDestroy()
    {
        StopOwnedEffects();
        if (impactVisual != null) Destroy(impactVisual.gameObject);
        if (cursorMesh != null) Destroy(cursorMesh);
        if (impactMesh != null) Destroy(impactMesh);
        if (cursorMaterial != null) Destroy(cursorMaterial);
        if (impactMaterial != null) Destroy(impactMaterial);
    }

    private void LateUpdate()
    {
        ApplyEntranceCollider();
        if(flat==null)return;
        if(Phase!=NodePhase.Preparing&&Phase!=NodePhase.Warning){flat.Visible=false;return;}
        flat.Draw(Phase==NodePhase.Preparing?Mathf.Clamp01(elapsed/activePreparation):1,
            Phase==NodePhase.Preparing?1:Mathf.Clamp01(outsideRing.localScale.x/Mathf.Max(.0001f,initialRingScale.x)),
            outsideRing.localEulerAngles.z,Phase==NodePhase.Preparing);
        flat.ApplyEntrance(EntranceBody,EntranceRing,EntranceTurn*(ringRotator==null?1:ringRotator.DirectionSign));
    }
}
