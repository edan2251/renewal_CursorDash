using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSettingButton : SettingUI {
    [SerializeField]
    private string key;
    private LanguageButton button;

    private void Awake() {
        button = gameObject.GetComponentInParent<LanguageButton>();
    }

    public override void Execute() {
        button.ChangeLanguage(key);
    }
}
