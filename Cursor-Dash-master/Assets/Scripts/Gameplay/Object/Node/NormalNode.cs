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
    private Tween preparationTween;
    private Tween moveTween;
    private Tween warningTween;
    private Tween releaseTween;
    private Transform outsideRing;
    private Transform insideRing;
    private SpriteRenderer outsideRenderer;
    private SpriteRenderer insideRenderer;
    private SpriteRenderer breakVisual;
    private Tween breakTween;
    private Vector3 initialScale;
    private Vector3 initialRingScale;
    private Quaternion initialRingRotation;
    private int initialOutsideOrder;
    private float initialLineWidth;
    private new void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        outsideRing = transform.Find("Outside");
        insideRing = transform.Find("Inside");
        outsideRenderer = outsideRing.GetComponent<SpriteRenderer>();
        insideRenderer = insideRing.GetComponent<SpriteRenderer>();
        breakVisual = transform.Find("BreakShell").GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        initialRingScale = outsideRing.localScale;
        initialRingRotation = outsideRing.localRotation;
        initialOutsideOrder = outsideRenderer.sortingOrder;
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
        insideRing.localScale = new Vector3(0f, 0f, 1f);
        StartCoroutine(Prepare());
    }
    private IEnumerator Prepare()
    {
        preparationTween = insideRing.DOScale(Vector3.one, preparationDuration).SetEase(Ease.Linear);
        yield return preparationTween.WaitForCompletion();
        if (Phase != NodePhase.Preparing || !StageManager.instance.IsPlaying) yield break;
        Phase = NodePhase.Warning;
        outsideRenderer.sortingOrder = insideRenderer.sortingOrder - 1;
        warningTween = DOTween.Sequence()
            .Join(transform.DOScale(initialScale * 0.92f, warningDuration).SetEase(Ease.InCubic))
            .Join(outsideRing.DOScale(initialRingScale * 0.08f, warningDuration).SetEase(Ease.InCubic))
            .Join(outsideRing.DOLocalRotate(new Vector3(0f, 0f, 180f), warningDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
        yield return warningTween.WaitForCompletion();
        while (Phase == NodePhase.Warning && !StageManager.instance.IsPlaying) yield return null;
        if (Phase != NodePhase.Warning || !StageManager.instance.IsPlaying) yield break;
        warningTween = null;
        Phase = NodePhase.Fired;
        isInteraction = true;
        if (circleCollider != null) circleCollider.enabled = false;
        lineRenderer.widthMultiplier = initialLineWidth * 0.65f;
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
        insideRenderer.enabled = false;
        outsideRenderer.enabled = false;
        LaunchBreak(Vector2.up);
        if (animator != null) animator.enabled = false;
        if (circleCollider != null) circleCollider.enabled = false;
        if (childAniamtor != null) childAniamtor.updateMode = AnimatorUpdateMode.UnscaledTime;
        GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion);
        StageManager.instance.RegisterCircleDestroyed(transform.position);
        StartCoroutine(Resolve());
    }
    private IEnumerator Resolve() { yield return new WaitForSecondsRealtime(0.4f); ResetObject(); }
    public override void ShowEffect(Vector2 position)
    {
        if (Phase != NodePhase.Resolving || effectObject == null) return;
        Vector2 direction = position - (Vector2)effectObject.transform.position;
        effectObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f);
        effectObject.SetActive(true);
        LaunchBreak(direction.sqrMagnitude > 0.0001f ? -direction.normalized : Vector2.up);
    }
    private void LaunchBreak(Vector2 direction)
    {
        breakTween?.Kill();
        breakVisual.gameObject.SetActive(true);
        breakVisual.color = Color.white;
        breakVisual.transform.localPosition = Vector3.zero;
        breakVisual.transform.localScale = Vector3.one;
        breakVisual.transform.localRotation = Quaternion.identity;
        breakTween = DOTween.Sequence()
            .Join(breakVisual.transform.DOLocalMove((Vector3)(direction * 0.55f), 0.38f).SetEase(Ease.OutCubic))
            .Join(breakVisual.transform.DOScale(1.28f, 0.38f).SetEase(Ease.OutQuad))
            .Join(breakVisual.transform.DOLocalRotate(new Vector3(0f, 0f, 48f), 0.38f))
            .Join(breakVisual.DOFade(0f, 0.38f));
    }
    public override void ResetObject()
    {
        StopVisualTweens();
        moveTween?.Kill();
        moveTween = null;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (effectObject != null) effectObject.SetActive(false);
        breakVisual.gameObject.SetActive(false);
        RestoreVisuals();
        base.ResetObject();
    }
    private void StopVisualTweens()
    {
        preparationTween?.Kill();
        preparationTween = null;
        warningTween?.Kill();
        warningTween = null;
        releaseTween?.Kill();
        releaseTween = null;
        breakTween?.Kill();
        breakTween = null;
    }
    private void RestoreVisuals()
    {
        transform.localScale = initialScale;
        if (outsideRing != null) outsideRing.localScale = initialRingScale;
        outsideRing.localRotation = initialRingRotation;
        outsideRenderer.sortingOrder = initialOutsideOrder;
        outsideRenderer.enabled = true;
        insideRenderer.enabled = true;
        insideRing.localScale = new Vector3(0f, 0f, 1f);
        if (lineRenderer != null) lineRenderer.widthMultiplier = initialLineWidth;
    }
    private void OnDisable()
    {
        StopVisualTweens();
        moveTween?.Kill();
        moveTween = null;
    }
}
