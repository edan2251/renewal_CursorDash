using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Tempus.CoroutineTools;

public class DefenceNode : Node
{
    [SerializeField] private GameObject node;

    private LineRenderer lineRenderer;
    private Tween executeTween;
    private Vector2 lineRendererVector;

    private static int defenseStreak;

    public static int DefenseStreak
    {
        get => defenseStreak;
        set {
            defenseStreak = value;
            if (defenseStreak == 15)
                GameManager.instance.playerData.AchieveRequire.SetValueToRequire("MaxDefence_Flame", 15);
        }
    }

    private new void Awake(){
        base.Awake();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    public override void Execute(){
        gameObject.SetActive(true);
        circleCollider.enabled = false;
        spriteRenderer.color = Color.clear;
        node.SetActive(true);
    }

    public void Attack(){
        lineRendererVector = Vector2.zero;
        lineRenderer.SetPosition(0, lineRendererVector);

        Vector2 coreOffset = transform.position / -0.6f;
        circleCollider.enabled = true;
        circleCollider.offset = coreOffset;

        executeTween = DOTween.To(() => lineRendererVector, x => lineRendererVector = x, (Vector2)gameObject.transform.position, 1.25f);
        executeTween.OnUpdate(() => {
            lineRenderer.SetPosition(1, lineRendererVector);
            circleCollider.offset = coreOffset + lineRendererVector / 0.6f;

            if(Vector2.Distance(gameObject.transform.position, lineRendererVector) < 0.5f){
                executeTween.Complete();
            }
        });

        executeTween.OnComplete(() => {
            isInteraction = true;
            executeTween.Kill();
            GameManager.instance.soundManager.PlaySFX(ClipTag.coreDamaged);
            StageManager.instance.scoreManager.Hp -= deducationHp;
            Interaction();
            DOVirtual.DelayedCall(0.5f, ResetObject);
        });
    }

    public override void Interaction()
    {
        base.Interaction();
        node.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("AfterImage") && !isInteraction){
            isInteraction = true;
            executeTween.Kill();
            GameManager.instance.soundManager.PlaySFX(ClipTag.arrowDefence);
            Interaction();
            DOVirtual.DelayedCall(0.5f, ResetObject);
        }
    }

    public override void ResetObject(){
        base.ResetObject();
        spriteRenderer.color = Color.white;
        circleCollider.enabled = false;
        lineRenderer.SetPosition(0, Vector2.zero);
        lineRenderer.SetPosition(1, Vector2.zero);
    }
}
