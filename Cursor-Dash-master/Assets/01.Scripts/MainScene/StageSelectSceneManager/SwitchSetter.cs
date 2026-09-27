using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchSetter : SettingUI {
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Sprite[] iconSprite;

    [SerializeField]
    private Image buttonImage;

    [SerializeField]
    private Sprite[] buttonSprite;
    
    [SerializeField]
    private string valueName;

    [SerializeField]
    private bool value;
    
    private void Update() {
        if (bool.Parse(PlayerPrefs.GetString(valueName, "true"))) {
            iconImage.sprite = iconSprite[0];
        }
        else {
            iconImage.sprite = iconSprite[1];
        }

        if (bool.Parse(PlayerPrefs.GetString(valueName, "true")) == value) {
            buttonImage.sprite = buttonSprite[0];
        }
        else {
            buttonImage.sprite = buttonSprite[1];
        }
    }
    
    public override void Execute() {
        PlayerPrefs.SetString(valueName, value.ToString());
    }
    
}
