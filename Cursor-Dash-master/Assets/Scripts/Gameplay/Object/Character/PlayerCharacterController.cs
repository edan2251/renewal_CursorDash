using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Range(0f, 0.2f)] private float viewportMargin = 0.08f;
    [SerializeField] private bool useCharacterSettings = true;
    [Header("Control feel (tune on devices)")]
    [SerializeField, Min(0f)] private float acceleration = 60f;
    [SerializeField, Min(0f)] private float releaseDeceleration = 18f;
    [SerializeField, Range(90f, 1080f)] private float turnDegreesPerSecond = 540f;
    private Vector2 moveVector;
    private Vector2 travelDirection;
    private float travelSpeed;
    private Rigidbody2D body;
    private Vector2 minimum;
    private Vector2 maximum;

    public Vector2 MoveVector
    {
        get => moveVector;
        set
        {
            moveVector = Vector2.ClampMagnitude(value, 1f);
        }
    }

    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        SpriteRenderer visual = GetComponent<SpriteRenderer>();
        if (useCharacterSettings)
        {
            CharacterSetting settings = GameManager.instance.playerData.characterSetting;
            if (settings.selectSprite != null) visual.sprite = settings.selectSprite.CharacterSprite;
            if (settings.selectColor != null) visual.color = settings.selectColor.ColorValue;
        }
        Camera camera = Camera.main;
        minimum = camera.ViewportToWorldPoint(new Vector3(viewportMargin, viewportMargin, 0f));
        maximum = camera.ViewportToWorldPoint(new Vector3(1f - viewportMargin, 1f - viewportMargin, 0f));
    }

    private void FixedUpdate()
    {
        if (StageManager.instance == null || !StageManager.instance.IsPlaying)
        {
            if (moveVector != Vector2.zero || travelSpeed > 0f) MoveStop();
            return;
        }
        float previousSpeed = travelSpeed;
        Vector2 displacement = AdvanceMotion(ref travelDirection, ref travelSpeed, moveVector,
            moveSpeed, acceleration, releaseDeceleration, turnDegreesPerSecond, Time.fixedDeltaTime);
        if (displacement == Vector2.zero)
        {
            if (previousSpeed > 0f && body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.MovePosition(body.position);
            }
            return;
        }
        Vector2 position = body != null ? body.position : (Vector2)transform.position;
        Vector2 next = ClampPosition(position + displacement, minimum, maximum);
        transform.LookAt2D(position + travelDirection);
        if (body != null) body.MovePosition(next);
        else transform.position = next;
    }

    public static Vector2 AdvanceMotion(ref Vector2 direction, ref float speed, Vector2 input,
        float topSpeed, float acceleration, float deceleration, float turnRate, float deltaTime)
    {
        float dt = Mathf.Max(0f, deltaTime);
        input = Vector2.ClampMagnitude(input, 1f);
        if (input.sqrMagnitude > 0.000001f)
        {
            Vector2 desired = input.normalized;
            // At a near stop, take the new direction promptly; at speed, trace a turn.
            if (speed <= Mathf.Max(0.001f, Mathf.Max(0f, topSpeed) * 0.08f) || direction.sqrMagnitude < 0.5f)
                direction = desired;
            else
            {
                float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float targetAngle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
                float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, Mathf.Max(0f, turnRate) * dt);
                direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            }
            speed = Mathf.MoveTowards(speed, Mathf.Max(0f, topSpeed) * input.magnitude,
                Mathf.Max(0f, acceleration) * dt);
        }
        else
            speed = Mathf.MoveTowards(speed, 0f, Mathf.Max(0f, deceleration) * dt);

        if (speed <= 0.001f)
        {
            speed = 0f;
            return Vector2.zero;
        }
        return direction * (speed * dt);
    }

    public static Vector2 ClampPosition(Vector2 position, Vector2 minimum, Vector2 maximum)
    {
        return new Vector2(Mathf.Clamp(position.x, minimum.x, maximum.x), Mathf.Clamp(position.y, minimum.y, maximum.y));
    }

    public void MoveStop()
    {
        moveVector = Vector2.zero;
        travelSpeed = 0f;
        travelDirection = Vector2.zero;
        if (body == null) return;
        body.linearVelocity = Vector2.zero;
        // Cancel a MovePosition queued before pause, cancellation or scene change.
        body.MovePosition(body.position);
    }

    private void OnDisable() => MoveStop();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (StageManager.instance == null || !StageManager.instance.IsPlaying) return;
        if (!other.CompareTag("Node")) return;
        NormalNode node = other.GetComponent<NormalNode>();
        if (node == null) return;
        node.Interaction();
        node.ShowEffect(transform.position);
    }
}
