using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ArrowNode : Node
{
    [SerializeField, Min(0.05f)] private float attackDuration = 0.6f;
    [SerializeField, Min(0.05f)] private float preparationDuration = 3.5f;
    [SerializeField, Min(0.05f)] private float warningDuration = 0.25f;
    [SerializeField, Min(0.05f)] private float reflectionDuration = 0.3f;
    [SerializeField, Min(0.05f)] private float destroyDuration = 1.333f;
    private LineRenderer lineRenderer;
    private Transform inside;
    private Transform outsideOne;
    private Transform outsideTwo;
    private GameObject node;
    private Tween preparationTween;
    private Tween warningTween;
    private Tween moveTween;
    private Tween returnTween;
    private Vector3 initialNodeScale;
    private Vector3 initialOutsideOneScale;
    private Vector3 initialOutsideTwoScale;
    private Quaternion initialOutsideOneRotation;
    private Quaternion initialOutsideTwoRotation;
    private SpriteRenderer insideRenderer;
    private SpriteRenderer outsideOneRenderer;
    private SpriteRenderer outsideTwoRenderer;
    private int initialOutsideOneOrder;
    private int initialOutsideTwoOrder;
    private int generation;
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
        insideRenderer = inside.GetComponent<SpriteRenderer>();
        outsideOneRenderer = outsideOne.GetComponent<SpriteRenderer>();
        outsideTwoRenderer = outsideTwo.GetComponent<SpriteRenderer>();
        initialNodeScale = node.transform.localScale;
        if (outsideOne != null) initialOutsideOneScale = outsideOne.localScale;
        if (outsideTwo != null) initialOutsideTwoScale = outsideTwo.localScale;
        initialOutsideOneRotation = outsideOne.localRotation;
        initialOutsideTwoRotation = outsideTwo.localRotation;
        initialOutsideOneOrder = outsideOneRenderer.sortingOrder;
        initialOutsideTwoOrder = outsideTwoRenderer.sortingOrder;
        initialLineWidth = lineRenderer.widthMultiplier;
        beamDefenseRadius = lineRenderer.startWidth * 0.5f;
    }
    // The hexagonal body cannot be destroyed by touching it.
    public override void Interaction() { }
    public override void ShowEffect() { }
    public override void Execute()
    {
        generation++;
        BeginExecution();
        preparationTween?.Kill();
        warningTween?.Kill();
        moveTween?.Kill();
        returnTween?.Kill();
        preparationTween = moveTween = returnTween = null;
        warningTween = null;
        RestoreVisuals();
        node.SetActive(true);
        if (spriteRenderer != null) spriteRenderer.color = Color.clear;
        if (animator != null) animator.enabled = false;
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
        outsideOneRenderer.sortingOrder = insideRenderer.sortingOrder - 1;
        outsideTwoRenderer.sortingOrder = insideRenderer.sortingOrder - 1;
        Sequence warning = DOTween.Sequence()
            .Join(node.transform.DOScale(initialNodeScale * 0.92f, warningDuration).SetEase(Ease.InCubic));
        if (outsideOne != null)
        {
            warning.Join(outsideOne.DOScale(initialOutsideOneScale * 0.08f, warningDuration).SetEase(Ease.InCubic));
            warning.Join(outsideOne.DOLocalRotate(new Vector3(0f, 0f, 150f), warningDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
        }
        if (outsideTwo != null)
        {
            warning.Join(outsideTwo.DOScale(initialOutsideTwoScale * 0.08f, warningDuration).SetEase(Ease.InCubic));
            warning.Join(outsideTwo.DOLocalRotate(new Vector3(0f, 0f, -150f), warningDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
        }
        warningTween = warning;
        yield return warningTween.WaitForCompletion();
        while (Phase == NodePhase.Warning && !StageManager.instance.IsPlaying) yield return null;
        if (Phase != NodePhase.Warning || !StageManager.instance.IsPlaying) yield break;
        warningTween = null;
        Phase = NodePhase.Fired;
        int firedGeneration = generation;
        Vector2 origin = transform.position;
        tip = origin;
        lineRenderer.widthMultiplier = initialLineWidth * 0.65f;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, origin);
        moveTween = DOTween.To(() => tip, value => tip = value, StageManager.instance.NexusPosition,
            Mathf.Max(0.05f, attackDuration)).SetEase(Ease.Linear);
        moveTween.OnUpdate(() =>
        {
            if (firedGeneration != generation || Phase != NodePhase.Fired) return;
            lineRenderer.SetPosition(1, tip);
            if (!StageManager.instance.IsPlaying) return;
            PlayerCharacterAfterImage trail = PlayerCharacterAfterImage.instance;
            Vector2 contact;
            // Only the visible, fired beam is tested. Preparation never reserves a defense.
            if (trail != null && trail.TryBlockBeam(origin, tip, beamDefenseRadius, out contact))
                Block(contact);
        });
        moveTween.OnComplete(() =>
        {
            if (firedGeneration != generation || Phase != NodePhase.Fired) return;
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
        moveTween = null;
        int blockedGeneration = generation;
        Vector2 origin = transform.position;
        Vector2 reflectedTip = contact;
        lineRenderer.SetPosition(0, contact);
        lineRenderer.SetPosition(1, contact);
        NormalNode.DefenseStreak++;
        GameManager.instance.soundManager.PlaySFX(ClipTag.arrowDefence);
        StageManager.instance.RegisterHexBlocked(contact);
        returnTween = DOTween.To(() => reflectedTip, value => reflectedTip = value, origin,
            reflectionDuration).SetEase(Ease.Linear);
        returnTween.OnUpdate(() =>
        {
            if (blockedGeneration == generation && Phase == NodePhase.Resolving)
                lineRenderer.SetPosition(1, reflectedTip);
        });
        returnTween.OnComplete(() => Impact(blockedGeneration));
    }
    private void Impact(int blockedGeneration)
    {
        if (blockedGeneration != generation || Phase != NodePhase.Resolving || !gameObject.activeInHierarchy) return;
        returnTween = null;
        lineRenderer.enabled = false;
        node.SetActive(false);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("AttackNode_Destroy", 0, 0f);
        }
        base.Interaction();
        GameManager.instance.soundManager.PlaySFX(ClipTag.nodeExplosion);
        StartCoroutine(Resolve(blockedGeneration));
    }
    private static void DefenseStreakFailed() => NormalNode.DefenseStreak = 0;
    private IEnumerator Resolve(int blockedGeneration)
    {
        yield return new WaitForSeconds(destroyDuration);
        if (blockedGeneration == generation && Phase == NodePhase.Resolving) ResetObject();
    }
    public override void ResetObject()
    {
        generation++;
        preparationTween?.Kill();
        warningTween?.Kill();
        moveTween?.Kill();
        returnTween?.Kill();
        preparationTween = warningTween = moveTween = returnTween = null;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (inside != null) inside.localScale = Vector3.zero;
        if (node != null) node.SetActive(true);
        RestoreVisuals();
        base.ResetObject();
        if (spriteRenderer != null) spriteRenderer.color = Color.clear;
        if (animator != null) animator.enabled = false;
        if (circleCollider != null) circleCollider.enabled = false;
    }
    private void RestoreVisuals()
    {
        if (node != null) node.transform.localScale = initialNodeScale;
        if (outsideOne != null) outsideOne.localScale = initialOutsideOneScale;
        if (outsideTwo != null) outsideTwo.localScale = initialOutsideTwoScale;
        outsideOne.localRotation = initialOutsideOneRotation;
        outsideTwo.localRotation = initialOutsideTwoRotation;
        outsideOneRenderer.sortingOrder = initialOutsideOneOrder;
        outsideTwoRenderer.sortingOrder = initialOutsideTwoOrder;
        if (lineRenderer != null) lineRenderer.widthMultiplier = initialLineWidth;
    }
    private void OnDisable()
    {
        generation++;
        preparationTween?.Kill();
        warningTween?.Kill();
        moveTween?.Kill();
        returnTween?.Kill();
    }
}
