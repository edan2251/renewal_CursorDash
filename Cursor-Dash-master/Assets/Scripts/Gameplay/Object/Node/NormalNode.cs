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
    [SerializeField, Min(0.05f)] private float preparationDuration = 2.5f;
    [SerializeField, Min(0.05f)] private float warningDuration = 0.25f;
    private LineRenderer lineRenderer;
    private Tween moveTween;
    private Tween warningTween;
    private Tween releaseTween;
    private Transform outsideRing;
    private Vector3 initialScale;
    private Vector3 initialRingScale;
    private float initialLineWidth;
    private new void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        outsideRing = transform.Find("Outside");
        initialScale = transform.localScale;
        if (outsideRing != null) initialRingScale = outsideRing.localScale;
        if (lineRenderer != null) initialLineWidth = lineRenderer.widthMultiplier;
    }
    public override void Execute()
    {
        BeginExecution();
        StopVisualTweens();
        RestoreVisuals();
        if (circleCollider != null) circleCollider.enabled = true;
        lineRenderer.enabled = false;
        if (effectObject != null) effectObject.SetActive(false);
        StartCoroutine(Prepare());
    }
    private IEnumerator Prepare()
    {
        yield return new WaitForSeconds(preparationDuration);
        if (Phase != NodePhase.Preparing || !StageManager.instance.IsPlaying) yield break;
        Phase = NodePhase.Warning;
        warningTween = DOTween.Sequence()
            .Join(transform.DOScale(initialScale * 0.92f, warningDuration).SetEase(Ease.InCubic))
            .Join(outsideRing.DOScale(initialRingScale * 0.68f, warningDuration).SetEase(Ease.InCubic));
        yield return warningTween.WaitForCompletion();
        while (Phase == NodePhase.Warning && !StageManager.instance.IsPlaying) yield return null;
        if (Phase != NodePhase.Warning || !StageManager.instance.IsPlaying) yield break;
        warningTween = null;
        Phase = NodePhase.Fired;
        isInteraction = true;
        if (circleCollider != null) circleCollider.enabled = false;
        lineRenderer.widthMultiplier = initialLineWidth * 1.6f;
        releaseTween = DOTween.Sequence()
            .Join(transform.DOScale(initialScale * 1.07f, 0.09f).SetEase(Ease.OutQuad))
            .Join(outsideRing.DOScale(initialRingScale * 1.05f, 0.09f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(initialScale, 0.13f).SetEase(Ease.OutQuad))
            .Join(outsideRing.DOScale(initialRingScale, 0.13f).SetEase(Ease.OutQuad));
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
        if ((Phase != NodePhase.Preparing && Phase != NodePhase.Warning) || !StageManager.instance.IsPlaying) return;
        Phase = NodePhase.Resolving;
        isInteraction = true;
        StopAllCoroutines();
        StopVisualTweens();
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
        StopVisualTweens();
        moveTween?.Kill();
        moveTween = null;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (effectObject != null) effectObject.SetActive(false);
        RestoreVisuals();
        base.ResetObject();
    }
    private void StopVisualTweens()
    {
        warningTween?.Kill();
        warningTween = null;
        releaseTween?.Kill();
        releaseTween = null;
    }
    private void RestoreVisuals()
    {
        transform.localScale = initialScale;
        if (outsideRing != null) outsideRing.localScale = initialRingScale;
        if (lineRenderer != null) lineRenderer.widthMultiplier = initialLineWidth;
    }
}
