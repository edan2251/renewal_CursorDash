using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ArrowNode : Node
{
    [SerializeField, Min(0.05f)] private float attackDuration = 0.6f;
    [SerializeField, Min(0.05f)] private float preparationDuration = 3.5f;
    [SerializeField, Min(0.05f)] private float warningDuration = 0.25f;
    private LineRenderer lineRenderer;
    private Transform inside;
    private Transform outsideOne;
    private Transform outsideTwo;
    private GameObject node;
    private Tween preparationTween;
    private Tween warningTween;
    private Tween moveTween;
    private Vector3 initialNodeScale;
    private Vector3 initialOutsideOneScale;
    private Vector3 initialOutsideTwoScale;
    private float initialLineWidth;
    private float beamDefenseRadius;
    private Vector2 tip;
    private new void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        node = transform.GetChild(0).gameObject;
        inside = node.transform.GetChild(0);
        outsideOne = node.transform.Find("Outside 1");
        outsideTwo = node.transform.Find("Outside 2");
        initialNodeScale = node.transform.localScale;
        if (outsideOne != null) initialOutsideOneScale = outsideOne.localScale;
        if (outsideTwo != null) initialOutsideTwoScale = outsideTwo.localScale;
        initialLineWidth = lineRenderer.widthMultiplier;
        beamDefenseRadius = lineRenderer.startWidth * 0.5f;
    }
    // The hexagonal body cannot be destroyed by touching it.
    public override void Interaction() { }
    public override void ShowEffect() { }
    public override void Execute()
    {
        BeginExecution();
        warningTween?.Kill();
        warningTween = null;
        RestoreVisuals();
        node.SetActive(true);
        if (spriteRenderer != null) spriteRenderer.color = Color.clear;
        if (circleCollider != null) circleCollider.enabled = false;
        inside.localScale = Vector3.zero;
        lineRenderer.enabled = false;
        StartCoroutine(Prepare());
    }
    private IEnumerator Prepare()
    {
        preparationTween = inside.DOScale(1f, preparationDuration).SetEase(Ease.Linear);
        yield return preparationTween.WaitForCompletion();
        while (Phase == NodePhase.Preparing && !StageManager.instance.IsPlaying) yield return null;
        if (Phase != NodePhase.Preparing || !StageManager.instance.IsPlaying) yield break;
        Phase = NodePhase.Warning;
        Sequence warning = DOTween.Sequence()
            .Join(node.transform.DOScale(initialNodeScale * 0.92f, warningDuration).SetEase(Ease.InCubic));
        if (outsideOne != null)
            warning.Join(outsideOne.DOScale(initialOutsideOneScale * 0.68f, warningDuration).SetEase(Ease.InCubic));
        if (outsideTwo != null)
            warning.Join(outsideTwo.DOScale(initialOutsideTwoScale * 0.68f, warningDuration).SetEase(Ease.InCubic));
        warningTween = warning;
        yield return warningTween.WaitForCompletion();
        while (Phase == NodePhase.Warning && !StageManager.instance.IsPlaying) yield return null;
        if (Phase != NodePhase.Warning || !StageManager.instance.IsPlaying) yield break;
        warningTween = null;
        Phase = NodePhase.Fired;
        Vector2 origin = transform.position;
        tip = origin;
        node.SetActive(false);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        if (animator != null) animator.SetTrigger("destroy");
        lineRenderer.widthMultiplier = initialLineWidth * 1.6f;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, origin);
        moveTween = DOTween.To(() => tip, value => tip = value, StageManager.instance.NexusPosition,
            Mathf.Max(0.05f, attackDuration)).SetEase(Ease.Linear);
        moveTween.OnUpdate(() =>
        {
            lineRenderer.SetPosition(1, tip);
            if (Phase != NodePhase.Fired || !StageManager.instance.IsPlaying) return;
            PlayerCharacterAfterImage trail = PlayerCharacterAfterImage.instance;
            Vector2 contact;
            // Only the visible, fired beam is tested. Preparation never reserves a defense.
            if (trail != null && trail.TryBlockBeam(origin, tip, beamDefenseRadius, out contact))
                Block(contact);
        });
        moveTween.OnComplete(() =>
        {
            if (Phase != NodePhase.Fired) return;
            DefenseStreakFailed();
            StageManager.instance.DamageNexus(deducationHp);
            ResetObject();
        });
    }
    private void Block(Vector2 contact)
    {
        if (Phase != NodePhase.Fired) return;
        Phase = NodePhase.Resolving;
        isInteraction = true;
        moveTween?.Kill();
        lineRenderer.SetPosition(1, contact);
        NormalNode.DefenseStreak++;
        GameManager.instance.soundManager.PlaySFX(ClipTag.arrowDefence);
        StageManager.instance.RegisterHexBlocked(contact);
        base.Interaction();
        StartCoroutine(Resolve());
    }
    private static void DefenseStreakFailed() => NormalNode.DefenseStreak = 0;
    private IEnumerator Resolve() { yield return new WaitForSecondsRealtime(0.25f); ResetObject(); }
    public override void ResetObject()
    {
        preparationTween?.Kill();
        warningTween?.Kill();
        moveTween?.Kill();
        preparationTween = warningTween = moveTween = null;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (inside != null) inside.localScale = Vector3.zero;
        RestoreVisuals();
        base.ResetObject();
        if (circleCollider != null) circleCollider.enabled = false;
    }
    private void RestoreVisuals()
    {
        if (node != null) node.transform.localScale = initialNodeScale;
        if (outsideOne != null) outsideOne.localScale = initialOutsideOneScale;
        if (outsideTwo != null) outsideTwo.localScale = initialOutsideTwoScale;
        if (lineRenderer != null) lineRenderer.widthMultiplier = initialLineWidth;
    }
}
