using UnityEngine;

public class Node : MonoBehaviour
{
    public enum NodePhase { Inactive, Preparing, Warning, Fired, Resolving }
    [SerializeField] private Animator _animator;
    [SerializeField] private int _deducationHp = 1;
    [Header("Node entrance — presentation only; attack clocks are unchanged")]
    [SerializeField,Range(.05f,.4f)] private float entranceSeconds=.20f;
    private float entranceStarted,activeEntrance,originalColliderRadius;
    private SpriteRenderer _spriteRenderer;
    private ParticleSystem particle;
    private CircleCollider2D _circleCollider;
    private bool _isInteraction;
    protected Animator animator => _animator;
    protected SpriteRenderer spriteRenderer => _spriteRenderer;
    protected CircleCollider2D circleCollider => _circleCollider;
    protected int deducationHp => _deducationHp;
    protected bool isInteraction { get => _isInteraction; set => _isInteraction = value; }
    public NodePhase Phase { get; protected set; }
    public ulong ExecutionId {get;private set;}
    public CursorDashGhostDemonstration Demonstration {get;private set;}
    public void SetDemonstration(CursorDashGhostDemonstration owner)
    {
        Demonstration=owner;
        var style=GetComponent<CursorDashGuideStyle>();
        if(owner==null)style?.Restore();
    }
    public float EntranceSeconds=>activeEntrance;
    public float RestColliderRadius=>originalColliderRadius;
    protected float EntranceProgress=>Phase==NodePhase.Preparing?Mathf.Clamp01((Time.time-entranceStarted)/Mathf.Max(.001f,activeEntrance)):1;
    protected float EntranceBody=>(float)NodeEntranceMath.Body(EntranceProgress);
    protected float EntranceRing=>(float)NodeEntranceMath.Ring(EntranceProgress);
    protected float EntranceTurn=>(float)NodeEntranceMath.RingTurn(EntranceProgress);
    protected void ApplyEntranceCollider()
    {
        if(_circleCollider==null)return;
        _circleCollider.radius=originalColliderRadius*(Phase==NodePhase.Preparing?Mathf.Max(.001f,(float)NodeEntranceMath.Collider(EntranceProgress)):1);
        if(Phase==NodePhase.Preparing)_circleCollider.enabled=EntranceBody>.015f;
        else if(Phase==NodePhase.Warning)_circleCollider.enabled=true;
    }

    protected void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        particle = GetComponentInChildren<ParticleSystem>(true);
        _circleCollider = GetComponent<CircleCollider2D>();
        if(_circleCollider!=null)originalColliderRadius=_circleCollider.radius;
    }
    protected void BeginExecution()
    {
        ExecutionId++;
        entranceStarted=Time.time;activeEntrance=Mathf.Max(.001f,entranceSeconds);
        gameObject.SetActive(true);
        isInteraction = false;
        Phase = NodePhase.Preparing;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (animator != null) { animator.enabled = true; animator.Rebind(); }
    }
    public virtual void Interaction()
    {
        if (particle != null) { particle.gameObject.SetActive(true); particle.Play(); }
    }
    public virtual void Execute() { }
    public virtual void ShowEffect() { }
    public virtual void ShowEffect(Vector2 position) { }
    public virtual void ResetObject()
    {
        SetDemonstration(null);
        StopAllCoroutines();
        Phase = NodePhase.Inactive;
        if(_circleCollider!=null)_circleCollider.radius=originalColliderRadius;
        isInteraction = false;
        if (particle != null) { particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); particle.gameObject.SetActive(false); }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (circleCollider != null) circleCollider.enabled = true;
        if (animator != null) { animator.enabled = true; animator.Rebind(); }
        transform.localPosition = Vector2.zero;
        gameObject.SetActive(false);
    }
}
