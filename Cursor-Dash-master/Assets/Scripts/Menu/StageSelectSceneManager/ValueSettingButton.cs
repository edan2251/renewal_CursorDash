using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ValueSettingButton : SettingUI {
    private ValueSetter valueSetter;
    
    private Action buttonAction;
    
    [Dropdown("ButtonTypes")]
    [SerializeField]
    private int buttonType;
    
        
    private DropdownList<int> ButtonTypes() {
        return new DropdownList<int>() {
            {"Minus", 0},
            {"Plus", 1}
        };
    }
    
    private void Awake() {
        valueSetter = gameObject.transform.parent.GetComponent<ValueSetter>();

        switch (buttonType) {
            case 0:
                buttonAction = () => {
                    valueSetter.CurrentValue--;
                };
                break;
            case 1:
                buttonAction = () => {
                    valueSetter.CurrentValue++;
                };
                break;
        }
    }

    public override void Execute() {
        buttonAction?.Invoke();
    }
}
