using System.Collections;
using System.Collections.Generic;
using EventTools.Event;
using UnityEngine;
using UnityEngine.UI;

public class ValueSetter : MonoBehaviour {
    private int maxValue = 5;
    private int currentValue;
    
    public int CurrentValue {
        get => currentValue;

        set {
            if (value < maxValue && value >= 0) {
                currentValue = value;
                PlayerPrefs.SetInt(settingName + "Setting", value);
                
                var settingValue = (float) currentValue / (float) maxValue;
                valueSettingEvent.Invoke(settingValue);
                
                ValueChange();
            }
        }
    }    
    
    [Header("Objects")]
    [SerializeField]
    private Image[] valueBlocks;

    [SerializeField]
    private Image iconImage;
    
    [Space(10)]
    [Header("Events")]
    [SerializeField]
    private UniEvent<float> valueSettingEvent;
    
    [Space(10)]
    [Header("Resources")]
    [SerializeField]
    private string settingName;

    [SerializeField]
    private Sprite[] iconSetting;
    
    private void Awake() {
        currentValue = PlayerPrefs.GetInt(settingName + "Setting", 2);
        ValueChange();
    }
    
    private void ValueChange() {
        if (currentValue == 0) {
            iconImage.sprite = iconSetting[0];
        }
        else {
            iconImage.sprite = iconSetting[1];
        }

        int index = 0;
        for (index = 0; index < currentValue; index++) {
            valueBlocks[index].gameObject.SetActive(true);
        }
        
        for (int j = index; j < maxValue - 1; j++) {
            valueBlocks[j].gameObject.SetActive(false);
        }
    }
}
