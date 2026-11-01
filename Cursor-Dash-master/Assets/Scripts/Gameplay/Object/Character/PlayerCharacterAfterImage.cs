using UnityEngine;

/// <summary>Draws and defends the same retained path; legacy pooled samples still expire on the gameplay clock.</summary>
public class PlayerCharacterAfterImage : MonoBehaviour
{
    private sealed class Segment
    {
        public BoxCollider2D collider;
        public Vector2 start;
        public Vector2 end;
        public float created;
        public bool active;
    }
    private struct VisualSegment {public Vector2 start,end;public float created;}

    public static PlayerCharacterAfterImage instance { get; private set; }
    [SerializeField] private GameObject trailCollisions;
    [Header("Trail settings")]
    // 5.75 * lifetime = 1.3 * (5 * 0.6), so speed does not double-count trail length.
    [SerializeField, Min(0.05f)] private float lifetime = 0.6782609f;
    [SerializeField, Min(0.01f)] private float sampleInterval = 0.02f;
    [SerializeField, Min(0.01f)] private float segmentWidth = 0.0675f;
    [SerializeField, Min(0.001f)] private float minimumDistance = 0.015f;
    private Segment[] segments;
    private TrailRenderer visual;
    private LineRenderer ribbon;private VisualSegment[] visualSegments;private int visualNext;
    private Vector2 previousPosition;
    private float sampleElapsed;
    private int next;
    private float sourceLifetime,sourceSample,sourceWidth,sourceDistance;
    private float lastVisualWidth=-1,lastAlpha=-1,lastTail=-1;
    private int lastFade=-1;
    private Color lastColor;
    private Vector3 entryLast;private bool entryHasPoint;
    private Vector3[] visualPath,lastVisualPath;private int recordedCount,visualCount,cachedVisualCount;private float visualLength;
    public int Capacity=>segments==null?0:segments.Length;
    public int VisualCapacity=>visualSegments==null?0:visualSegments.Length;
    public LineRenderer DisplayRenderer=>ribbon;
    public float RestVisualLength=>Mathf.Max(0,CursorDashTuning.Number("player.speed",5.75f))*lifetime*
        (Demonstration!=null?Mathf.Clamp01(StageManager.instance?.Fever?.Visuals==null?.50f:StageManager.instance.Fever.Visuals.tutorialGhostTailFraction):
        Mathf.Clamp(StageManager.instance?.Fever?.Visuals==null?.20f:StageManager.instance.Fever.Visuals.cursorRestTailFraction,0,.4f));
    public float DisplayedTrailLength=>visualLength;

    public float Lifetime => lifetime;
    public CursorDashGhostDemonstration Demonstration {get;private set;}
    public void ConfigureDemonstration(CursorDashGhostDemonstration owner,GameObject collisionPool){Demonstration=owner;trailCollisions=collisionPool;}
    // Timed collider telemetry only; retained beam defense uses the rendered path below.
    public int ActiveSegmentCount
    {
        get
        {
            int count = 0;
            if (segments != null)
                foreach (Segment segment in segments)
                    if (IsSegmentAlive(segment.active, segment.created, Time.time, lifetime)) count++;
            return count;
        }
    }

    private void Awake()
    {
        if(Demonstration==null)instance = this;
        visual = GetComponent<TrailRenderer>();
        sourceLifetime=lifetime;sourceSample=sampleInterval;sourceWidth=segmentWidth;sourceDistance=minimumDistance;
        ApplyTuning();
    }
    public void ApplyTuning()
    {
        ClearTrail();
        lifetime=CursorDashTuning.Number("trail.lifetime",sourceLifetime);
        sampleInterval=CursorDashTuning.Number("trail.sample",sourceSample);
        segmentWidth=CursorDashTuning.Number("trail.hitWidth",sourceWidth);
        minimumDistance=CursorDashTuning.Number("trail.distance",sourceDistance);
        if (trailCollisions == null) return;
        BoxCollider2D[] pool = trailCollisions.GetComponentsInChildren<BoxCollider2D>(true);
        int capacity = Mathf.Max(pool.Length, Mathf.CeilToInt(lifetime / sampleInterval) + 4);
        capacity = Mathf.Clamp(capacity, 8, 2048);
        segments = new Segment[capacity];
        // Retained path history is independent of the legacy timed collider pool.
        // Beam defense queries this same path without allocating extra colliders.
        int historyCapacity=Mathf.Clamp(Mathf.Max(capacity,Mathf.CeilToInt(CursorDashTuning.Number("player.speed",5.75f)*lifetime/Mathf.Max(.001f,minimumDistance))+4),8,4096);
        visualSegments=new VisualSegment[historyCapacity];visualNext=0;
        visualPath=new Vector3[historyCapacity*2+2];lastVisualPath=new Vector3[visualPath.Length];recordedCount=cachedVisualCount=0;
        for (int i = 0; i < capacity; i++)
        {
            BoxCollider2D collider;
            if (i < pool.Length) collider = pool[i];
            else
            {
                GameObject item = new GameObject("Trail Segment " + i);
                item.transform.SetParent(trailCollisions.transform, false);
                item.tag = "AfterImage";
                collider = item.AddComponent<BoxCollider2D>();
            }
            collider.isTrigger = true;
            collider.gameObject.SetActive(false);
            segments[i] = new Segment { collider = collider };
        }
        if (visual != null)
        {
            // Keep the authored component/material for compatibility, but do not rebuild native
            // time-stamped TrailRenderer points at one timestamp. The display is distance based.
            visual.time=lifetime;visual.emitting=false;visual.Clear();visual.enabled=false;
            InitializeRibbon();
            visual.minVertexDistance = minimumDistance;
            visual.startWidth = visual.endWidth = segmentWidth;
            Gradient fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.35f, 0.65f), new GradientAlphaKey(0f, 1f) });
            visual.colorGradient = fade;
            visual.Clear();
        }
        previousPosition = transform.position;
        lastVisualWidth=-1;RefreshVisual();
    }
    private void InitializeRibbon()
    {
        if(ribbon==null){
            var existing=transform.Find("Cursor movement ribbon");
            var go=existing==null?new GameObject("Cursor movement ribbon"){hideFlags=HideFlags.DontSave}:existing.gameObject;
            if(existing==null)go.transform.SetParent(transform,false);go.layer=gameObject.layer;
            ribbon=go.GetComponent<LineRenderer>();if(ribbon==null)ribbon=go.AddComponent<LineRenderer>();
        }
        ribbon.sharedMaterials=visual.sharedMaterials;ribbon.sortingLayerID=visual.sortingLayerID;ribbon.sortingOrder=visual.sortingOrder;
        ribbon.useWorldSpace=true;ribbon.loop=false;ribbon.alignment=LineAlignment.View;ribbon.textureMode=LineTextureMode.Stretch;
        ribbon.numCapVertices=visual.numCapVertices;ribbon.numCornerVertices=visual.numCornerVertices;
        ribbon.widthMultiplier=1;ribbon.positionCount=0;ribbon.enabled=false;
    }
    private void RefreshVisual()
    {
        if(visual==null)return;
        float width=CursorDashTuning.Number("trail.visualWidth",sourceWidth),alpha=CursorDashTuning.Number("trail.alpha",1),tail=CursorDashTuning.Number("trail.tailAlpha",.35f);
        int fade=CursorDashTuning.Integer("trail.fade",1);
        Color color=CursorDashTuning.ColorValue("user.trailColor",Color.white);
        if(width==lastVisualWidth&&alpha==lastAlpha&&tail==lastTail&&fade==lastFade&&color==lastColor)return;
        lastVisualWidth=width;lastAlpha=alpha;lastTail=tail;lastFade=fade;lastColor=color;
        visual.startWidth=visual.endWidth=width;
        if(ribbon!=null)ribbon.startWidth=ribbon.endWidth=width;
        var gradient=new Gradient();
        gradient.SetKeys(new[]{new GradientColorKey(color,0),new GradientColorKey(color,1)},
            new[]{new GradientAlphaKey(alpha*color.a,0),new GradientAlphaKey(tail*color.a,fade==0?.5f:fade==1?.65f:.3f),new GradientAlphaKey(0,1)});
        visual.colorGradient=gradient;
        if(ribbon!=null)ribbon.colorGradient=gradient;
    }

    private void LateUpdate()
    {
        RefreshVisual();
        if (segments == null) return;
        foreach (Segment segment in segments)
        {
            if (segment.active && !IsSegmentAlive(true, segment.created, Time.time, lifetime))
            {
                segment.active = false;
                if (segment.collider != null) segment.collider.gameObject.SetActive(false);
            }
        }
        bool playing = StageManager.instance != null && StageManager.instance.IsPlaying;
        if (visual != null) visual.emitting = false;
        if (!playing || Time.deltaTime <= 0f) return;
        sampleElapsed += Time.deltaTime;
        Vector2 position = transform.position;
        if(sampleElapsed>=sampleInterval&&(position-previousPosition).sqrMagnitude>=minimumDistance*minimumDistance){
            sampleElapsed=0;RecordMovement(previousPosition,position,Time.time);previousPosition=position;
        }
        DrawMovementPath(position,Time.time);
    }

    public void RecordMovement(Vector2 start, Vector2 end, float createdAt)
        =>RememberMovement(start,end,createdAt,true);

    private void RememberMovement(Vector2 start,Vector2 end,float createdAt,bool defensive)
    {
        if (segments == null || (end - start).sqrMagnitude < 0.000001f) return;
        RememberVisual(start,end,createdAt);
        if(!defensive)return;
        Segment segment = segments[next];
        next = (next + 1) % segments.Length;
        if (segment.collider == null) return;
        segment.start = start;
        segment.end = end;
        segment.created = createdAt;
        segment.active = true;
        Vector2 direction = end - start;
        Transform target = segment.collider.transform;
        target.position = (start + end) * 0.5f;
        target.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        target.localScale = Vector3.one;
        segment.collider.offset = Vector2.zero;
        segment.collider.size = new Vector2(direction.magnitude, segmentWidth);
        segment.collider.gameObject.SetActive(true);
    }
    private void RememberVisual(Vector2 start,Vector2 end,float createdAt)
    {
        if(visualSegments==null)return;
        visualSegments[visualNext]=new VisualSegment{start=start,end=end,created=createdAt};
        visualNext=(visualNext+1)%visualSegments.Length;recordedCount=Mathf.Min(recordedCount+1,visualSegments.Length);
    }
    // Seed the ghost's visible path without generating a contact or collection event.
    // Once displayed, it follows the same beam-defense rule as every retained tail.
    public void SeedVisualTail(Vector2 forward)
    {
        if(Demonstration==null||forward.sqrMagnitude<.001f)return;
        Vector2 head=transform.position;RememberVisual(head-forward.normalized*RestVisualLength,head,Time.time-lifetime-1);
        DrawMovementPath(head,Time.time);
    }

    private void DrawMovementPath(Vector2 head,float now)
    {
        if(ribbon==null||visualPath==null)return;
        visualCount=1;visualLength=0;visualPath[0]=head;float floor=RestVisualLength;
        for(int i=0;i<recordedCount;i++){
            VisualSegment segment=visualSegments[(visualNext-1-i+visualSegments.Length)%visualSegments.Length];
            bool live=IsSegmentAlive(true,segment.created,now,lifetime);
            float remaining=live&&Demonstration==null?float.PositiveInfinity:Mathf.Max(0,floor-visualLength);
            if(!AppendVisualPoint(segment.end,remaining))break;
            remaining=live&&Demonstration==null?float.PositiveInfinity:Mathf.Max(0,floor-visualLength);
            if(!AppendVisualPoint(segment.start,remaining))break;
        }
        // LineRenderer starts at the head: full alpha at zero, then the original fading tail.
        bool changed=cachedVisualCount!=visualCount||ribbon.positionCount!=visualCount;
        for(int i=0;!changed&&i<visualCount;i++)changed=(lastVisualPath[i]-visualPath[i]).sqrMagnitude>.0000000001f;
        if(!changed)return;
        ribbon.positionCount=visualCount;
        for(int i=0;i<visualCount;i++)ribbon.SetPosition(i,visualPath[i]);
        ribbon.enabled=visualCount>1;
        for(int i=0;i<visualCount;i++)lastVisualPath[i]=visualPath[i];cachedVisualCount=visualCount;
    }
    private bool AppendVisualPoint(Vector2 point,float allowed)
    {
        Vector2 from=visualPath[visualCount-1];float length=Vector2.Distance(from,point);
        if(length<.00001f)return true;
        if(allowed<=.00001f||visualCount>=visualPath.Length)return false;
        float use=Mathf.Min(length,allowed);visualPath[visualCount++]=Vector2.Lerp(from,point,use/length);visualLength+=use;
        return use>=length-.00001f;
    }

    public bool TryBlockBeam(Vector2 start, Vector2 tip, float radius, out Vector2 contact)
    {
        contact = tip;
        if (visualPath == null || recordedCount == 0) return false;
        // Arrow.Update can run before our LateUpdate. Rebuild the shared path using the
        // current head/clock so rendering and defense cannot use different retained tails.
        DrawMovementPath(transform.position,Time.time);
        bool blocked = false;
        float earliest = 1f;
        float width = Mathf.Max(0f, radius) + segmentWidth * 0.5f;
        for (int i=1;i<visualCount;i++)
        {
            float alongBeam;
            float distance = DistanceBetweenSegments(start, tip, visualPath[i-1], visualPath[i], out alongBeam);
            if (distance > width || (blocked && alongBeam > earliest)) continue;
            earliest = alongBeam;
            contact = Vector2.Lerp(start, tip, alongBeam);
            blocked = true;
        }
        return blocked;
    }

    public static bool IsSegmentAlive(bool active, float created, float now, float duration)
        => active && duration > 0f && now >= created && now - created < duration;

    public static float DistanceBetweenSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out float alongFirst)
    {
        const float epsilon = 0.000001f;
        Vector2 u = b - a, v = d - c, w = a - c;
        float lengthA = Vector2.Dot(u, u), lengthB = Vector2.Dot(v, v), f = Vector2.Dot(v, w);
        float s, t;
        if (lengthA <= epsilon && lengthB <= epsilon) { alongFirst = 0f; return Vector2.Distance(a, c); }
        if (lengthA <= epsilon) { s = 0f; t = Mathf.Clamp01(f / lengthB); }
        else
        {
            float e = Vector2.Dot(u, w);
            if (lengthB <= epsilon) { t = 0f; s = Mathf.Clamp01(-e / lengthA); }
            else
            {
                float cross = Vector2.Dot(u, v), denominator = lengthA * lengthB - cross * cross;
                s = denominator > epsilon ? Mathf.Clamp01((cross * f - e * lengthB) / denominator) : 0f;
                t = (cross * s + f) / lengthB;
                if (t < 0f) { t = 0f; s = Mathf.Clamp01(-e / lengthA); }
                else if (t > 1f) { t = 1f; s = Mathf.Clamp01((cross - e) / lengthA); }
            }
        }
        alongFirst = s;
        return (w + u * s - v * t).magnitude;
    }

    public void DrawEntryPoint(){if(visual!=null&&(!entryHasPoint||(entryLast-transform.position).sqrMagnitude>.000001f)){
        if(entryHasPoint)RememberMovement(entryLast,transform.position,Time.time,false);
        entryLast=transform.position;entryHasPoint=true;DrawMovementPath(transform.position,Time.time);
    }}
    public void FinishEntry(){previousPosition=transform.position;sampleElapsed=0;entryHasPoint=false;}
    public void ClearTrail()
    {
        entryHasPoint=false;
        if (segments != null)
            foreach (Segment segment in segments)
            {
                segment.active = false;
                if (segment.collider != null) segment.collider.gameObject.SetActive(false);
            }
        if (visual != null) { visual.Clear(); visual.emitting = false; }
        if(ribbon!=null){ribbon.positionCount=0;ribbon.enabled=false;}
        next = 0;
        visualNext=0;
        recordedCount=visualCount=cachedVisualCount=0;visualLength=0;
        sampleElapsed = 0f;
        previousPosition = transform.position;
    }
    private void OnDisable() => ClearTrail();
    private void OnDestroy() { if (instance == this) instance = null; }
}
