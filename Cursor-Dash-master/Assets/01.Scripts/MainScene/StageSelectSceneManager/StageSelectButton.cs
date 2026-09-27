using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StageSelectButton : SettingUI
{
    public override void Execute() {
        GameManager.instance.MapInformation = StageSelectSceneManager.instance.mapController.CurrentMapInformation;
        if (GameManager.instance.MapInformation.isUnlock) {
            SceneManager.LoadScene(StageSelectSceneManager.instance.mapController.CurrentMapInformation.mapName);
        }
    }
}
