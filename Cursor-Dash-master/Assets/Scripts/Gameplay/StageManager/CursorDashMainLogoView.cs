using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene/prefab owns placement and artwork; only the child receives runtime animation.</summary>
[DefaultExecutionOrder(70)]
public sealed class CursorDashMainLogoView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform placementRoot;
    [SerializeField] private RectTransform animationRoot;
    [SerializeField] private CanvasGroup opacity;
    [SerializeField] private Image artwork;
    [Tooltip("Existing rightward exit distance in Canvas units, relative to the authored placement.")]
    [SerializeField] private float exitDistance = 150;
    [Tooltip("Keep the existing narrow-screen width limit without rewriting the placement root.")]
    [SerializeField, Range(.1f, 1f)] private float maximumCanvasWidth = .86f;

    public RectTransform PlacementRoot => placementRoot;
    public RectTransform AnimationRoot => animationRoot;
    public Image Artwork => artwork;
    public bool IsConfigured => canvas != null && placementRoot != null && animationRoot != null && opacity != null && artwork != null;

    public void SetVisible(bool visible)
    {
        if (animationRoot != null) animationRoot.gameObject.SetActive(visible);
    }

    public void Present(float travel, float alpha)
    {
        if (!IsConfigured) return;
        animationRoot.anchoredPosition = new Vector2(Mathf.Clamp01(travel) * exitDistance, 0);
        opacity.alpha = Mathf.Clamp01(alpha);
        FitToCanvas();
    }

    private void LateUpdate() => FitToCanvas();

    private void FitToCanvas()
    {
        if (!IsConfigured) return;
        var canvasRect = (RectTransform)canvas.transform;
        float authoredWidth = Mathf.Abs(placementRoot.rect.width * placementRoot.localScale.x);
        float fit = authoredWidth > 0 ? Mathf.Min(1, canvasRect.rect.width * maximumCanvasWidth / authoredWidth) : 1;
        animationRoot.localScale = Vector3.one * fit;
    }
}
