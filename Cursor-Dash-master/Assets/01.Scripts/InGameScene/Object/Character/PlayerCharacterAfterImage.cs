using UnityEngine;

/// <summary>Reuses the existing collider pool; each segment expires on the gameplay clock.</summary>
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

    public static PlayerCharacterAfterImage instance { get; private set; }
    [SerializeField] private GameObject trailCollisions;
    [Header("Initial trail settings (existing lifetime retained)")]
    [SerializeField, Min(0.05f)] private float lifetime = 0.5f;
    [SerializeField, Min(0.01f)] private float sampleInterval = 0.02f;
    [SerializeField, Min(0.01f)] private float segmentWidth = 0.135f;
    [SerializeField, Min(0.001f)] private float minimumDistance = 0.015f;
    private Segment[] segments;
    private TrailRenderer visual;
    private Vector2 previousPosition;
    private float sampleElapsed;
    private int next;

    public float Lifetime => lifetime;
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
        instance = this;
        visual = GetComponent<TrailRenderer>();
        if (trailCollisions == null) return;
        BoxCollider2D[] pool = trailCollisions.GetComponentsInChildren<BoxCollider2D>(true);
        int capacity = Mathf.Max(pool.Length, Mathf.CeilToInt(lifetime / sampleInterval) + 4);
        capacity = Mathf.Clamp(capacity, 8, 256);
        segments = new Segment[capacity];
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
            visual.time = lifetime;
            visual.minVertexDistance = minimumDistance;
            visual.startWidth = visual.endWidth = segmentWidth;
            Gradient fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.35f, 0.65f), new GradientAlphaKey(0f, 1f) });
            visual.colorGradient = fade;
            visual.Clear();
        }
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
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
        if (visual != null) visual.emitting = playing;
        if (!playing || Time.deltaTime <= 0f) return;
        sampleElapsed += Time.deltaTime;
        if (sampleElapsed < sampleInterval) return;
        sampleElapsed = 0f;
        Vector2 position = transform.position;
        if ((position - previousPosition).sqrMagnitude < minimumDistance * minimumDistance) return;
        RecordMovement(previousPosition, position, Time.time);
        previousPosition = position;
    }

    public void RecordMovement(Vector2 start, Vector2 end, float createdAt)
    {
        if (segments == null || (end - start).sqrMagnitude < 0.000001f) return;
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

    public bool TryBlockBeam(Vector2 start, Vector2 tip, float radius, out Vector2 contact)
    {
        contact = tip;
        if (segments == null) return false;
        bool blocked = false;
        float earliest = 1f;
        float width = Mathf.Max(0f, radius) + segmentWidth * 0.5f;
        foreach (Segment segment in segments)
        {
            if (segment.collider == null || !IsSegmentAlive(segment.active, segment.created, Time.time, lifetime)) continue;
            float alongBeam;
            float distance = DistanceBetweenSegments(start, tip, segment.start, segment.end, out alongBeam);
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

    public void ClearTrail()
    {
        if (segments != null)
            foreach (Segment segment in segments)
            {
                segment.active = false;
                if (segment.collider != null) segment.collider.gameObject.SetActive(false);
            }
        if (visual != null) { visual.Clear(); visual.emitting = false; }
        next = 0;
        sampleElapsed = 0f;
        previousPosition = transform.position;
    }
    private void OnDisable() => ClearTrail();
    private void OnDestroy() { if (instance == this) instance = null; }
}
