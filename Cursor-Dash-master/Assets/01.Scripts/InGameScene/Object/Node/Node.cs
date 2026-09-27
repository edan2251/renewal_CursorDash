using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    protected Animator animator => _animator;

    private SpriteRenderer _spriteRenderer;
    protected SpriteRenderer spriteRenderer => _spriteRenderer;

    private ParticleSystem particle;
    private CircleCollider2D _circleCollider;
    protected CircleCollider2D circleCollider => _circleCollider;

    private bool _isInteraction;
    protected bool isInteraction {get => _isInteraction; set{_isInteraction = value;}}

    [SerializeField]
    private int _deducationHp;
    protected int deducationHp => _deducationHp;

    [SerializeField]
    private int distanceFromFocus;

    private float defaultDestroyTimer = 2.0f;
    private float destroyTimer;

    protected void Awake(){
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        particle = gameObject.GetComponentInChildren<ParticleSystem>(true);
        _circleCollider = gameObject.GetComponent<CircleCollider2D>();

        destroyTimer = defaultDestroyTimer;
    }

    public virtual void Interaction() {
        particle.gameObject.SetActive(true);
        particle.Play();
    }

    public virtual void Execute(){}
    public virtual void ShowEffect(){}
    public virtual void ShowEffect(Vector2 position){}

    public virtual void ResetObject(){
        gameObject.transform.localPosition = Vector2.zero;
        gameObject.SetActive(false);

        _isInteraction = false;

        if (spriteRenderer != null) _spriteRenderer.enabled = true;
        if (circleCollider != null) circleCollider.enabled = true;

        if (_animator != null) _animator.Rebind();

        if (particle != null) particle.gameObject.SetActive(false);
    }
}
