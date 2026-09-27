using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectSceneManager : MonoBehaviour
{
    public static StageSelectSceneManager instance;

    [SerializeField]
    private GameObject[] sceneCanvasArray;

    [SerializeField]
    private Image[] sceneButtons;

    [SerializeField]
    private Sprite[] sceneButtonSprites;

    [SerializeField]
    private AudioClip mainSceneClip;
    
    [HideInInspector]
    public MapController mapController;
    
    [HideInInspector]
    public CharacterTextConverter characterTextConverter;

    private int currentCanvasIndex = 0;
    public int CurrentCanvasIndex => currentCanvasIndex;

    private bool isChanged;
    
    private void Awake(){
        if(instance is null){
            instance = this;
        }
    
        GameManager.instance.CurrentSceneType = GameManager.SceneType.NotInGame;
        GameManager.instance.soundManager.ChangeBGM(mainSceneClip);
        GameManager.instance.soundManager.PlayBGM();
        mapController = gameObject.GetComponent<MapController>();
        characterTextConverter = gameObject.GetComponent<CharacterTextConverter>();
    }

    private void Update() {
        if (isChanged) {
            return;
        }
        
        if (GameManager.instance.touchManager.SwipeDirection.x < -0.5f) {
            CanvasControll( currentCanvasIndex + 1);
            isChanged = true;
            CoolDown().Start(this);
        }
        else if (GameManager.instance.touchManager.SwipeDirection.x > 0.5f) {
            CanvasControll(currentCanvasIndex - 1);
            isChanged = true;
            CoolDown().Start(this);
        }
    }

    private IEnumerator CoolDown() {
        yield return YieldInstructionCache.WaitingSeconds(0.2f);
        isChanged = false;
    }

    public void CanvasControll(int index) {
        index = Mathf.Clamp(index, 0, sceneCanvasArray.Length - 1);
        
        for(int i = 0; i < sceneCanvasArray.Length; i++){
            sceneCanvasArray[i].gameObject.SetActive(false);
        }

        sceneCanvasArray[index].gameObject.SetActive(true);
        currentCanvasIndex = index;
    }
}
