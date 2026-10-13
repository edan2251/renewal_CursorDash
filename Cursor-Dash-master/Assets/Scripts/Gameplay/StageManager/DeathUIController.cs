using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathUIController : MonoBehaviour
{
    [SerializeField] private Image[] uiImages;
    [SerializeField] private Text scoreText;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text summaryLabel;
    private Tween tween;
    public void UICanvasOn()
    {
        gameObject.SetActive(true);
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = group.blocksRaycasts = true;
            tween?.Kill();
            tween = group.DOFade(1f, 0.2f).SetUpdate(true);
        }
        else
        {
            if (uiImages != null) foreach (Image image in uiImages) if (image != null) image.DOFade(1f, 0.2f).SetUpdate(true);
            if (scoreText != null) scoreText.DOFade(1f, 0.2f).SetUpdate(true);
        }
    }
    public void UICanvasOff()
    {
        tween?.Kill();
        gameObject.SetActive(false);
    }
    public void SetScoreText(string value)
    {
        if (scoreLabel != null) scoreLabel.text = value;
        if (scoreText != null) scoreText.text = value;
    }
    public void SetSummary(int circles, int beams)
    {
        if (summaryLabel != null) summaryLabel.text = $"원형 파괴 {circles}  ·  육각 차단 {beams}";
    }
    private void OnDisable() { tween?.Kill(); tween = null; }
}
