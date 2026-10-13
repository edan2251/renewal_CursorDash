using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Range(0f, 0.2f)] private float viewportMargin = 0.08f;
    [SerializeField] private bool useCharacterSettings = true;
    private Vector2 moveVector;
    private Rigidbody2D body;
    private Vector2 minimum;
    private Vector2 maximum;

    public Vector2 MoveVector
    {
        get => moveVector;
        set
        {
            moveVector = Vector2.ClampMagnitude(value, 1f);
            if (moveVector == Vector2.zero && body != null)
            {
                body.linearVelocity = Vector2.zero;
                // Replace a movement queued before release for the next physics step.
                body.MovePosition(body.position);
            }
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
        if (StageManager.instance == null || !StageManager.instance.IsPlaying || moveVector == Vector2.zero)
            return;
        Vector2 position = body != null ? body.position : (Vector2)transform.position;
        Vector2 next = ClampPosition(position + moveVector * (moveSpeed * Time.fixedDeltaTime), minimum, maximum);
        transform.LookAt2D(position + moveVector);
        if (body != null) body.MovePosition(next);
        else transform.position = next;
    }

    public static Vector2 ClampPosition(Vector2 position, Vector2 minimum, Vector2 maximum)
    {
        return new Vector2(Mathf.Clamp(position.x, minimum.x, maximum.x), Mathf.Clamp(position.y, minimum.y, maximum.y));
    }

    public void MoveStop() => MoveVector = Vector2.zero;

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
