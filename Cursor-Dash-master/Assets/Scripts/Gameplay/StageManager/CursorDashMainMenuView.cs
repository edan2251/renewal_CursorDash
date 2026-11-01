using TMPro;
using UnityEngine;

/// <summary>Authored prompt/caption. SpaceFlow supplies only relative presentation values.</summary>
public sealed class CursorDashMainMenuView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform promptPlacement;
    [SerializeField] private CanvasGroup promptAnimation;
    [SerializeField] private TMP_Text prompt;
    [SerializeField] private RectTransform captionPlacement;
    [SerializeField] private RectTransform captionAnimation;
    [SerializeField] private CanvasGroup captionOpacity;
    [SerializeField] private TMP_Text caption;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private string bestLabel = "BEST";
    [SerializeField] private string newBestLabel = "NEW BEST";
    [SerializeField] private string finalLabel = "이번 점수";
    [SerializeField] private string excludedFinalLabel = "이번 점수 · 기록 제외";
    [SerializeField] private float captionScoreGap = 22;
    private CursorDashViewFonts fonts;

    public bool IsConfigured => canvas != null && promptPlacement != null && promptAnimation != null && prompt != null && captionPlacement != null && captionAnimation != null && captionOpacity != null && caption != null;
    public Canvas Canvas => canvas;
    public TMP_Text Prompt => prompt;
    public TMP_Text Caption => caption;
    public RectTransform PromptPlacement => promptPlacement;
    public RectTransform CaptionPlacement => captionPlacement;
    public GameObject PauseButton => pauseButton;
    public float CaptionScoreGap => captionScoreGap;

    public void PrepareRendering()
    {
        if (fonts != null) return;
        fonts = new CursorDashViewFonts(); fonts.Bind(prompt); fonts.Bind(caption);
    }

    public void SetVisible(bool visible) => canvas.enabled = visible;
    public void PresentPrompt(float alpha) => promptAnimation.alpha = Mathf.Clamp01(alpha);

    public void PresentCaption(Vector2 canvasPoint, bool final, bool eligible, bool record, float pulse, float alpha)
    {
        captionAnimation.anchoredPosition = canvasPoint;
        captionAnimation.localScale = Vector3.one * (1 + .10f * pulse);
        captionOpacity.alpha = Mathf.Clamp01(alpha);
        caption.text = final ? (eligible ? finalLabel : excludedFinalLabel) : record ? newBestLabel : bestLabel;
    }

    private void OnDestroy() { fonts?.Dispose(); fonts = null; }
}
