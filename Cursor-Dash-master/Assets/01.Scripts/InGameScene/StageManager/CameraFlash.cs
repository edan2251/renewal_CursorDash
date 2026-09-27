using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CameraFlash : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField]
    private Image flashImage;

    [Header("Values")]
    [SerializeField]
    private float defaultFlashDuration;

    [SerializeField]
    private float interactionFlashDuration;

    [SerializeField]
    private float damagedFlashDuration;

    private IEnumerator flashCoroutine;

    private Tween flashTween;
    
    [Space(10)]
    [SerializeField]
    private Color defaultColor;

    [SerializeField]
    private Color interactionColor;

    [SerializeField]
    private Color damagedColor;

    public void InteractionFlash() {
        if (bool.Parse(PlayerPrefs.GetString("OnFlash", "true"))) {
            flashCoroutine?.Stop(this);
            flashCoroutine = FlashCoroutine(interactionColor, interactionFlashDuration).Start(this);
        }

    }

    public void DamagedFlash(){
        if (bool.Parse(PlayerPrefs.GetString("OnFlash", "true"))) {
            flashCoroutine?.Stop(this);
            flashCoroutine = FlashCoroutine(damagedColor, damagedFlashDuration).Start(this);
        }
    }

    private IEnumerator FlashCoroutine(Color color, float duration){
        flashImage.DOColor(color, 0.2f);

        flashTween = flashImage.DOFade(1.0f, duration);
        yield return flashTween.WaitForCompletion();
        
        flashTween = flashImage.DOFade(0.5f, 0.25f);
        yield return flashTween.WaitForCompletion();

        flashImage.DOColor(defaultColor, 0.2f);
    }
}
