using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTextConverter : MonoBehaviour
{    
    [SerializeField]
    private Text UnlockText;
    
    //Unlock image가 존재 한다면 UnlockText는 Off가 되어야한다.
    [SerializeField]
    private Text UnlockCondition; 

    //Resources에서 UnLock, Lock 변경

    private void OnEnable() {
        UnlockText.text = GameManager.instance.currentLanguage.GetValue("Unlock").description;
    }

    //Lock 상태일때
    public void TextConvert(string key){
        UnlockText.text = GameManager.instance.currentLanguage.GetValue("Unlock").description;
        UnlockCondition.text = GameManager.instance.currentLanguage.GetValue(key).description;
    }

    public void OpenTextConvert(string key) {
        UnlockText.text = GameManager.instance.currentLanguage.GetValue(key).id;
        UnlockCondition.text = "";
    }
    
    //Unlock 상태일때

}
