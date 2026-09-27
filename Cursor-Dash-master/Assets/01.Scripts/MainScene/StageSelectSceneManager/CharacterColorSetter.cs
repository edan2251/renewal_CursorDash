using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSetter : SettingUI {
    private Image colorImage;
    private Image frameImage;

    [Header("Resources")]
    [SerializeField]
    private Sprite unlockSprite;
    
    [SerializeField]
    private Sprite lockSprite;

    private bool isUnlock;
    private int index;

    [SerializeField]
    private string key;


    private CharacterColorInformation characterSetting;

    private Action executeEvent;

    [SerializeField]
    private EventTools.Event.UniEvent<string> textConvertEvent;
    
    [SerializeField]
    private EventTools.Event.UniEvent<string> openTextConvertEvent;
    
    private void Awake() {
        colorImage = gameObject.GetComponent<Image>();
        frameImage = gameObject.transform.GetChild(0).GetComponent<Image>();
    }

    public void Setting() {
        colorImage.color = characterSetting.ColorValue;
        isUnlock = characterSetting.IsUnlock;
        
        frameImage.sprite = isUnlock ? unlockSprite : lockSprite;
    }
    
    public void Setting(int index) {
        characterSetting = GameManager.instance.playerData.characterSetting.CharacterColors[index];
        Setting();
    }
    
    public void Setting(int index, Action action) {
        Setting(index);
        executeEvent = action;
    }
    
    public override void Execute() {
        if (isUnlock) {
            GameManager.instance.playerData.characterSetting.selectColor = characterSetting;
            executeEvent?.Invoke();
            openTextConvertEvent?.Invoke(key);
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
