using UnityEngine;

public class Node : MonoBehaviour
{
    public enum NodePhase { Inactive, Preparing, Warning, Fired, Resolving }
    [SerializeField] private Animator _animator;
    [SerializeField] private int _deducationHp = 1;
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

    protected void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        particle = GetComponentInChildren<ParticleSystem>(true);
        _circleCollider = GetComponent<CircleCollider2D>();
    }
    protected void BeginExecution()
    {
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
        StopAllCoroutines();
        Phase = NodePhase.Inactive;
        isInteraction = false;
        if (particle != null) { particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); particle.gameObject.SetActive(false); }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (circleCollider != null) circleCollider.enabled = true;
        if (animator != null) { animator.enabled = true; animator.Rebind(); }
        transform.localPosition = Vector2.zero;
        gameObject.SetActive(false);
    }
}
