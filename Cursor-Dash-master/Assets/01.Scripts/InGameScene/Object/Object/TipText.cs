using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class TipText : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private string key;
    [SerializeField] private bool key2Exists;

    [ShowIf("key2Exists")] [SerializeField]
    private string key2;

    private void OnEnable() {
        text.text = key2Exists
            ? $"{GameManager.instance.currentLanguage.GetValue(key).description}\n{GameManager.instance.currentLanguage.GetValue(key2).description}"
            : GameManager.instance.currentLanguage.GetValue(key).description;
    }
}