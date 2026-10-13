using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSpriteSetter : SettingUI {
    private Image characterImage;
    private Image frameImage;

    private CharacterSpriteInformation characterSetting;
    private bool isUnlock;
    private int index;

   [SerializeField]
    private string key;

    [Header("Resources")]
    [SerializeField]
    private Sprite unlockSprite;
     [SerializeField]
    private Sprite lockSprite;

    private Action executeEvent;
    
    [SerializeField]
    private EventTools.Event.UniEvent<string> textConvertEvent;

    private void Awake() {
        characterImage = gameObject.transform.GetChild(0).GetComponent<Image>();
        frameImage = gameObject.GetComponent<Image>();
    }

    public void Setting() {
        isUnlock = characterSetting.IsUnlock;
    
        characterImage.sprite = characterSetting.CharacterSprite;
        frameImage.sprite = isUnlock ? unlockSprite : lockSprite;
    }
    
    public void Setting(int index) {
        characterSetting = GameManager.instance.playerData.characterSetting.CharacterSprites[index];
        Setting();
    }

    public void Setting(int index, Action action) {
        Setting(index);
        executeEvent = action;
    }
    
    public override void Execute() {
        if (isUnlock) {
            GameManager.instance.playerData.characterSetting.selectSprite = characterSetting;
            executeEvent?.Invoke();
        }
        else {
            textConvertEvent?.Invoke(key);
        }
    }
    
    public void Unlock() {
        characterSetting.IsUnlock = true;
        Setting();
        executeEvent?.Invoke();
    }

    public void Lock() {
        characterSetting.IsUnlock = false;
        Setting();
        executeEvent?.Invoke();
    }
}
