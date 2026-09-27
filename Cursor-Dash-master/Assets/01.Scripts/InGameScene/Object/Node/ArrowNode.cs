using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowNode : Node
{
    [Header("Values")]
    [SerializeField]
    private float attackDuration;

    private Vector3 arrowPosition;

    private LineRenderer lineRenderer;

    private Tween moveTween;
    private Vector2 attackLineVector;

    private const float ScalingTime = 3.5f;
    private SpriteRenderer spriteRenderer;
    private Transform inside;
    private GameObject node;

    private new void Awake(){
        base.Awake();

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        node = transform.GetChild(0).gameObject;
        inside = node.transform.GetChild(0);

        attackLineVector = gameObject.transform.position;
        
        lineRenderer.SetPosition(0, gameObject.transform.position);
        lineRenderer.SetPosition(1, gameObject.transform.position);
    }

    public override void Interaction(){ }
    public override void ShowEffect(){ }

    public override void Execute(){
        attackLineVector = gameObject.transform.position;

        gameObject.SetActive(true);
        node.SetActive(true);
        spriteRenderer.color = Color.clear;
        circleCollider.enabled = false;
        inside.localScale = Vector3.zero;

        lineRenderer.SetPosition(0, gameObject.transform.position);
        lineRenderer.SetPosition(1, gameObject.transform.position);

        ExecuteCoroutine().Start(this);
    }

    private void PlayDestroyAnimation() {
        node.SetActive(false);
        spriteRenderer.color = Color.white;
        animator.SetTrigger("destroy");
    }

    private IEnumerator ExecuteCoroutine(){
        yield return inside.DOScale(1f, ScalingTime).SetEase(Ease.Linear).WaitForCompletion();
        PlayDestroyAnimation();
        
        circleCollider.enabled = true;
        circleCollider.offset = Vector2.zero;

        moveTween = DOTween.To(() => attackLineVector, x => attackLineVector = x, Vector2.zero, attackDuration);
        
        moveTween.OnUpdate(() => {
            lineRenderer.SetPosition(1, attackLineVector);
            circleCollider.offset = (Vector3)attackLineVector - gameObject.transform.position;
        });
    }

    private IEnumerator ResetCoroutine(){
        PlayDestroyAnimation();
        GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion);

        do{
            yield return null;
        }while((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.9f);
        
        ResetObject();
    }

    public override void ResetObject(){
        base.ResetObject();
    
        circleCollider.enabled = false;

        moveTween.Kill();        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("AfterImage") && !isInteraction){
            isInteraction = true;
            moveTween.Kill();
            
            GameManager.instance.soundManager.PlaySFX(ClipTag.arrowDefence);
            NormalNode.DefenseStreak++;

            StageManager.instance.CameraShake(0.5f, 0.5f);
            StageManager.instance.CameraInteractionFlash();
        
            ResetCoroutine().Start(this);
        } else if (other.CompareTag("Core") && !isInteraction){
            isInteraction = true;
            
            moveTween.Kill();

            StageManager.instance.scoreManager.Hp -= deducationHp;
            NormalNode.DefenseStreak = 0;
            // lineRenderer.SetPosition(1, gameObject.transform.position);            

            ResetCoroutine().Start(this);
        }
        
    }
}
