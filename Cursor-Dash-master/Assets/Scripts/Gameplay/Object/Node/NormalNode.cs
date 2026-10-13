using System.Collections;
using DG.Tweening;
using UnityEngine;

public class NormalNode : Node
{
    private static int defenseStreak;
    public static int DefenseStreak
    {
        get => defenseStreak;
        set
        {
            defenseStreak = value;
            if (value == 15 || value == 20)
                GameManager.instance.playerData.AchieveRequire.SetValueToRequire("MaxDefence_Cyberpunk", value);
        }
    }
    [SerializeField] private GameObject effectObject;
    [SerializeField] private Animator childAniamtor;
    [SerializeField, Min(0.05f)] private float lineDuration = 0.6f;
    [SerializeField, Min(0.05f)] private float preparationDuration = 2f;
    private LineRenderer lineRenderer;
    private Tween moveTween;
    private new void Awake() { base.Awake(); lineRenderer = GetComponent<LineRenderer>(); }
    public override void Execute()
    {
        BeginExecution();
        if (circleCollider != null) circleCollider.enabled = true;
        lineRenderer.enabled = false;
        if (effectObject != null) effectObject.SetActive(false);
        StartCoroutine(Prepare());
    }
    private IEnumerator Prepare()
    {
        yield return new WaitForSeconds(preparationDuration);
        if (Phase != NodePhase.Preparing || StageManager.instance.IsDeath) yield break;
        Phase = NodePhase.Fired;
        isInteraction = true;
        if (circleCollider != null) circleCollider.enabled = false;
        Vector2 origin = transform.position;
        Vector2 tip = origin;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, origin);
        // Circular-node beams never ask the trail for defense.
        moveTween = DOTween.To(() => tip, value => tip = value, StageManager.instance.NexusPosition,
            Mathf.Max(0.05f, lineDuration)).SetEase(Ease.Linear);
        moveTween.OnUpdate(() => lineRenderer.SetPosition(1, tip));
        moveTween.OnComplete(() =>
        {
            if (Phase != NodePhase.Fired) return;
            DefenseStreak = 0;
            StageManager.instance.DamageNexus(deducationHp);
            ResetObject();
        });
    }
    public override void Interaction()
    {
        if (Phase != NodePhase.Preparing || !StageManager.instance.IsPlaying) return;
        Phase = NodePhase.Resolving;
        isInteraction = true;
        StopAllCoroutines();
        base.Interaction();
        DefenseStreak++;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (animator != null) animator.enabled = false;
        if (circleCollider != null) circleCollider.enabled = false;
        if (childAniamtor != null) childAniamtor.updateMode = AnimatorUpdateMode.UnscaledTime;
        GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion);
        StageManager.instance.RegisterCircleDestroyed(transform.position);
        StartCoroutine(Resolve());
    }
    private IEnumerator Resolve() { yield return new WaitForSecondsRealtime(0.3f); ResetObject(); }
    public override void ShowEffect(Vector2 position)
    {
        if (Phase != NodePhase.Resolving || effectObject == null) return;
        Vector2 direction = position - (Vector2)effectObject.transform.position;
        effectObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f);
        effectObject.SetActive(true);
    }
    public override void ResetObject()
    {
        moveTween?.Kill();
        moveTween = null;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (effectObject != null) effectObject.SetActive(false);
        base.ResetObject();
    }
}
