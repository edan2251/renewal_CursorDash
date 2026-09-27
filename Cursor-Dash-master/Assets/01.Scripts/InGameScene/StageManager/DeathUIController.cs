using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DeathUIController : MonoBehaviour
{

    [Header("Objects")]    
    [SerializeField]
    private Image[] uiImages;

    [SerializeField]
    private Text scoreText;

    public void UICanvasOn(){
        gameObject.SetActive(true);
        
        for(int i = 0; i < uiImages.Length; i++){
            uiImages[i].DOFade(1.0f, 0.25f);
        }

        scoreText.DOFade(1.0f, 0.25f);
    }    

    public void UICanvasOff(){
        gameObject.SetActive(false);
        
        Color initialColor = Color.white;
        initialColor.a = 0;

        for(int i = 0; i < uiImages.Length; i++){
            uiImages[i].color = initialColor; 
        }

        scoreText.color = initialColor;
    }

    public void SetScoreText(string value){
        scoreText.text = value;
    }

}
