using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MapController : MonoBehaviour
{
    [SerializeField]
    private MapInformation[] mapInformations;
    private MapInformation currentMapInformation;
    public MapInformation CurrentMapInformation => currentMapInformation;
    
    private int mapIndex;

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Image backgroundAfterImage;
    
    [SerializeField]
    private Text mapNameText;

    [SerializeField]
    private Image mapImage;

    [SerializeField]
    private Image mapRightUIup;

    [SerializeField]
    private Image mapRightUIdown;

    [SerializeField]
    private Sprite unlockSprite;

    [SerializeField]
    private Text scoreText;
    
    [SerializeField] private GameObject cyberpunkCore;
    [SerializeField] private GameObject flameCore;
    [SerializeField] private Text condition;
    
    private void Awake(){
        mapIndex = 0;
        currentMapInformation = mapInformations[mapIndex];
        GameManager.instance.MapInformation = currentMapInformation;
        MapInformationUpdate();
    }
    
    private void Update(){
        if(StageSelectSceneManager.instance.CurrentCanvasIndex == 0 && GameManager.instance.touchManager.IsSwipe){
            if(GameManager.instance.touchManager.SwipeDirection.y > 0){
                MapInformationUp();
            } else if (GameManager.instance.touchManager.SwipeDirection.y < 0){
                MapInformationDown();
            }
        }
    }

    private void MapInformationUp(){
        if(mapIndex + 1 < mapInformations.Length){
            currentMapInformation = mapInformations[++mapIndex];
        }
        MapInformationUpdate();
    }

    private void MapInformationDown(){
        if(mapIndex - 1 >= 0){
            currentMapInformation = mapInformations[--mapIndex];
        }
        MapInformationUpdate();
    }

    private void MapInformationUpdate()
    {
        mapNameText.text = currentMapInformation.mapName;

        if (currentMapInformation.isUnlock)
        {
            mapImage.sprite = currentMapInformation.mapSprite;
        }
        else
        {
            mapImage.sprite = unlockSprite;
        }


        backgroundImage.sprite = currentMapInformation.mapBackground;

        if (currentMapInformation.mapBackgroundAfterImage == null)
        {
            backgroundAfterImage.gameObject.SetActive(false);
            mapRightUIup.gameObject.SetActive(false);
            mapRightUIdown.gameObject.SetActive(true);
        }
        else
        {
            backgroundAfterImage.gameObject.SetActive(true);
            backgroundAfterImage.sprite = currentMapInformation.mapBackgroundAfterImage;
            mapRightUIup.gameObject.SetActive(true);
            mapRightUIdown.gameObject.SetActive(false);
        }

        scoreText.text = currentMapInformation.highScore.ToString("F2");

        var isCyberpunk = currentMapInformation.mapName == "Cyberpunk";
        flameCore.SetActive(currentMapInformation.isUnlock && !isCyberpunk);
        cyberpunkCore.SetActive(isCyberpunk);
        condition.text = !currentMapInformation.isUnlock
            ? GameManager.instance.currentLanguage.GetValue("Flame_Require").description
            : string.Empty;
    }
}


