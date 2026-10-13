using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CameraFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float defaultFlashDuration;
    [SerializeField] private float interactionFlashDuration = 0.15f;
    [SerializeField] private float damagedFlashDuration = 0.25f;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color interactionColor = Color.white;
    [SerializeField] private Color damagedColor = Color.red;
    private Tween tween;
    public void InteractionFlash() => Flash(interactionColor, 0.12f, interactionFlashDuration);
    public void DamagedFlash() => Flash(damagedColor, 0.25f, damagedFlashDuration);
    public void Flash(Color color, float opacity, float duration)
    {
        if (flashImage == null || PlayerPrefs.GetString("OnFlash", "true") != "true") return;
        tween?.Kill();
        color.a = Mathf.Clamp01(opacity);
        flashImage.raycastTarget = false;
        flashImage.color = color;
        tween = flashImage.DOFade(0f, Mathf.Max(0.01f, duration)).SetUpdate(true).SetEase(Ease.OutQuad);
    }
    public void ResetFlash()
    {
        tween?.Kill();
        tween = null;
        if (flashImage != null) flashImage.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
    }
    private void Awake() => ResetFlash();
    private void OnDisable() => ResetFlash();
}
