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
    private Tween damageTween;private Image damageImage;
    public Image DamageImage=>damageImage;
    private float damageUntil;
    public void InteractionFlash() => Flash(interactionColor, 0.12f, interactionFlashDuration);
    public void DamagedFlash() => Flash(damagedColor, 0.25f, damagedFlashDuration);
    public void Flash(Color color, float opacity, float duration)
    {
        if(Time.unscaledTime<damageUntil)return;
        FlashCore(color,opacity,duration);
    }
    public void ConfirmedDamage(Color tint,float opacity,float duration)
    {
        damageUntil=Time.unscaledTime+Mathf.Clamp(duration,.1f,.5f);
        // The authored FlashImage contains circuit artwork. Keep it for success; damage is a plain wash.
        tween?.Kill();if(flashImage!=null){var old=flashImage.color;old.a=0;flashImage.color=old;}
        if(flashImage==null||PlayerPrefs.GetString("OnFlash","true")!="true")return;
        if(damageImage==null){
            var go=new GameObject("Confirmed damage / plain screen flash",typeof(RectTransform)){hideFlags=HideFlags.DontSave,layer=flashImage.gameObject.layer};
            go.transform.SetParent(flashImage.transform.parent,false);go.transform.SetSiblingIndex(flashImage.transform.GetSiblingIndex()+1);
            damageImage=go.AddComponent<Image>();damageImage.raycastTarget=false;damageImage.sprite=null;
            var rect=damageImage.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
        damageTween?.Kill();tint.a=Mathf.Clamp(opacity,0,.45f);damageImage.color=tint;
        damageTween=damageImage.DOFade(0,Mathf.Clamp(duration,.1f,.5f)).SetUpdate(true).SetEase(Ease.OutQuad);
    }
    private void FlashCore(Color color,float opacity,float duration)
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
        damageUntil=0;
        damageTween?.Kill();damageTween=null;if(damageImage!=null)damageImage.color=Color.clear;
        tween?.Kill();
        tween = null;
        if (flashImage != null) flashImage.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
    }
    private void Awake() => ResetFlash();
    private void OnDisable() => ResetFlash();
    private void OnDestroy(){ResetFlash();if(damageImage!=null)Destroy(damageImage.gameObject);}
}
