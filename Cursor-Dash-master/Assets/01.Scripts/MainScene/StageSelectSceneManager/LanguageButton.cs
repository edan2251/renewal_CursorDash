using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : SettingUI {
    [SerializeField]
    private GameObject[] languageItems;

    private bool isOpen = false;
    private bool IsOpen => isOpen;

    public override void Execute() {
        isOpen = !isOpen;
        for (int i = 0; i < languageItems.Length; i++) {
            languageItems[i].gameObject.SetActive(isOpen);
        }
    }

    public void ChangeLanguage(string key) {
        GameManager.instance.currentLanguage = GameManager.instance.LanguageSettingDictionary[key];
    }
}
