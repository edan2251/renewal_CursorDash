using System.Collections;
using UnityEngine;
using DG.Tweening;

public class NormalNode : Node
{
    private static int defenseStreak;

    public static int DefenseStreak
    {
        get => defenseStreak;
        set
        {
            defenseStreak = value;
            if (defenseStreak == 15 || defenseStreak == 20)
                GameManager.instance.playerData.AchieveRequire.SetValueToRequire("MaxDefence_Cyberpunk", value);
        }
    }
    private IEnumerator executeCoroutine;

    [Header("Objects")]
    [SerializeField]
    private GameObject effectObject;

    [SerializeField]
    private Animator childAniamtor;

    [Header("Values")]
    [SerializeField]
    private float lineDuration;

    private LineRenderer lineRenderer;
    private Vector2 lineVector;

    private Tween moveTween;

    private new void Awake(){
        base.Awake();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    public override void Execute(){
        gameObject.SetActive(true);
        executeCoroutine = ExecuteCoroutine().Start(this);
    }

    public override void Interaction(){
        if(isInteraction){
            return;
        }
        
        base.Interaction();

        StageManager.instance.CameraShake(1.5f, 0.1f);
        StageManager.instance.CameraInteractionFlash();

        executeCoroutine.Stop(this);
        DefenseStreak++;

        spriteRenderer.enabled = false;
        animator.enabled = false;

        GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion);

        StageManager.instance.CameraShake(0.5f, 0.5f);
        StageManager.instance.CameraInteractionFlash();

        InteractionCoroutine().Start(this);
    }

    private IEnumerator InteractionCoroutine(){
        Time.timeScale = 0.1f;

        do{
            if(childAniamtor.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.1f){
                Time.timeScale = 1.0f;
            }

            yield return YieldInstructionCache.WaitFrame;

        }while(childAniamtor.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.96f);
        ResetObject();
    }

    private IEnumerator ExecuteCoroutine(){
        do{
            yield return YieldInstructionCache.WaitFrame;
        }while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.99f);

        isInteraction = true;
        FailedInteraction();
    }

    private void FailedInteraction(){
        lineRenderer.enabled = true;
        lineVector = gameObject.transform.position;

        lineRenderer.SetPosition(0, gameObject.transform.position);
        lineRenderer.SetPosition(1, gameObject.transform.position);

        moveTween = DOTween.To(() => lineVector, x => lineVector = x, Vector2.zero, lineDuration);

        moveTween.OnUpdate(() => {
            lineRenderer.SetPosition(1, lineVector);
        });

        moveTween.OnComplete(() => {
            StageManager.instance.scoreManager.Hp -= deducationHp;
            DefenseStreak = 0;
            ResetObject();
        });

    }

    public override void ResetObject(){
        base.ResetObject();

        animator.enabled = true;
        lineRenderer.enabled = false;

        effectObject.SetActive(false);
    }

    public override void ShowEffect(Vector2 position){
        if(isInteraction){
            return;
        }

        isInteraction = true;

        float angle = Mathf.Atan2(
            position.y - effectObject.transform.position.y,
            position.x - effectObject.transform.position.x
        ) * 180 / Mathf.PI;

        angle += 90;

        effectObject.transform.localRotation = Quaternion.Euler(0,0, angle);
        effectObject.SetActive(true);
    }
}
