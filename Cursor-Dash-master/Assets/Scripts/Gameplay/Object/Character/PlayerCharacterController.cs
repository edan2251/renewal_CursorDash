using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
    [SerializeField, Min(0f)] private float moveSpeed = 5.75f;
    [SerializeField, Range(0f, 0.2f)] private float viewportMargin = 0.08f;
    [SerializeField] private bool useCharacterSettings = true;
    [Header("Control feel (tune on devices)")]
    [SerializeField, Min(0f)] private float acceleration = 60f;
    [SerializeField, Min(0f)] private float releaseDeceleration = 18f;
    // Developer tool and gameplay share one default; the old scene's serialized 540 is left untouched.
    public const float DefaultTurnDegreesPerSecond = 1800f;
    private Vector2 moveVector;
    private Vector2 travelDirection;
    private float travelSpeed;
    private Vector2 lastMotionDirection;
    private float lastMotionTime=float.NegativeInfinity;
    public Vector2 ContactMotionDirection=>Time.time-lastMotionTime<.20f?lastMotionDirection:Vector2.zero;
    private Rigidbody2D body;
    private Vector2 minimum;
    private Vector2 maximum;
    private SpriteRenderer cursorVisual;
    private Color initialVisualColor;
    private float sourceVisualScale;private Vector2 sourceColliderSize;

    public Vector2 MoveVector
    {
        get => moveVector;
        set
        {
            moveVector = Vector2.ClampMagnitude(value, 1f);
        }
    }

    public float MoveSpeed => CursorDashTuning.Number("player.speed",moveSpeed);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        // The visible cursor lives on a child so resizing it does not shrink the body collider.
        SpriteRenderer visual = transform.Find("CursorVisual").GetComponent<SpriteRenderer>();
        cursorVisual=visual;
        sourceVisualScale=visual.transform.localScale.x;
        var box=GetComponent<BoxCollider2D>();
        if(box!=null)sourceColliderSize=box.size;ApplyRoundTuning();
        if (useCharacterSettings)
        {
            CharacterSetting settings = GameManager.instance.playerData.characterSetting;
            if (settings.selectSprite != null) visual.sprite = settings.selectSprite.CharacterSprite;
            if (settings.selectColor != null) visual.color = settings.selectColor.ColorValue;
        }
        Camera camera = Camera.main;
        initialVisualColor=visual.color;
        minimum = camera.ViewportToWorldPoint(new Vector3(viewportMargin, viewportMargin, 0f));
        maximum = camera.ViewportToWorldPoint(new Vector3(1f - viewportMargin, 1f - viewportMargin, 0f));
    }

    private void FixedUpdate()
    {
        if (StageManager.instance == null || !StageManager.instance.IsPlaying || CursorDashRuntimeUI.InputBlocked)
        {
            if (moveVector != Vector2.zero || travelSpeed > 0f) MoveStop();
            return;
        }
        float previousSpeed = travelSpeed;
        Camera boundsCamera=StageManager.instance.Feedback?.Distortion?.UiCamera;
        if(boundsCamera==null)boundsCamera=Camera.main;
        float margin=CursorDashTuning.Number("player.margin",viewportMargin);
        minimum=boundsCamera.ViewportToWorldPoint(new Vector3(margin,margin,0));
        maximum=boundsCamera.ViewportToWorldPoint(new Vector3(1-margin,1-margin,0));
        Vector2 displacement = AdvanceMotion(ref travelDirection, ref travelSpeed, moveVector,
            MoveSpeed, CursorDashTuning.Number("player.acceleration",acceleration),CursorDashTuning.Number("player.release",releaseDeceleration),
            CursorDashTuning.Number("player.turn",DefaultTurnDegreesPerSecond), Time.fixedDeltaTime);
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
        Vector2 actual=next-position;
        if(actual.sqrMagnitude>.0000001f){lastMotionDirection=actual.normalized;lastMotionTime=Time.time;}
        transform.LookAt2D(position + travelDirection);
        if (body != null) body.MovePosition(next);
        else transform.position = next;
        // Fever samples the rendered head in LateUpdate, without changing this movement or normal defense.
    }
    private void Update(){if(cursorVisual!=null)cursorVisual.color=CursorDashTuning.ColorValue("user.cursorColor",initialVisualColor);}

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

    // Only the start presentation writes this pose. Normal movement/rotation/collision math is unchanged.
    public void PresentEntry(bool visible,Vector2 position,Vector2 direction)
    {
        moveVector=travelDirection=lastMotionDirection=Vector2.zero;travelSpeed=0;lastMotionTime=float.NegativeInfinity;
        if(cursorVisual!=null)cursorVisual.enabled=visible;
        if(body!=null){body.linearVelocity=Vector2.zero;body.position=position;body.MovePosition(position);}transform.position=position;
        if(direction.sqrMagnitude>.001f)transform.LookAt2D(position+direction);
    }
    public void ApplyRoundTuning()
    {
        if(cursorVisual!=null)cursorVisual.transform.localScale=Vector3.one*CursorDashTuning.Number("player.visualScale",sourceVisualScale);
        var box=GetComponent<BoxCollider2D>();if(box!=null)box.size=new Vector2(CursorDashTuning.Number("player.colliderX",sourceColliderSize.x),CursorDashTuning.Number("player.colliderY",sourceColliderSize.y));
    }

    private void OnDisable(){MoveStop();lastMotionDirection=Vector2.zero;lastMotionTime=float.NegativeInfinity;}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (StageManager.instance == null || !StageManager.instance.IsPlaying) return;
        if (!other.CompareTag("Node")) return;
        NormalNode node = other.GetComponent<NormalNode>();
        if (node == null) return;
        Collider2D own=GetComponent<Collider2D>();
        Vector2 contact=own==null?(Vector2)transform.position:other.Distance(own).pointA;
        node.ContactAt(contact,ContactMotionDirection);
        node.ShowEffect(transform.position);
    }
}
