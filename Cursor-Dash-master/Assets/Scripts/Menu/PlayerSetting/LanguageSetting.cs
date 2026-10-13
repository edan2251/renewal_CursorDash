using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LanguageSettingDictionaryItem {
    [SerializeField]
    private string _key;
    public string key => _key;
    
    [SerializeField]
    private string _id;
    public string id => _id;
        
    [SerializeField]
    private string _description;
    public string description => _description;
}

[CreateAssetMenu(fileName = "Language File", menuName = "Scriptable Object/Language Setting", order = 0)]
public class LanguageSetting : ScriptableObject {
    [SerializeField]
    private string _fileKey;

    public string fileKey => _fileKey;
    
    [SerializeField]
    private LanguageSettingDictionaryItem[] items;

    public LanguageSettingDictionaryItem[] Items => items;

    private Dictionary<string, LanguageSettingDictionaryItem> languageDictionary = new Dictionary<string, LanguageSettingDictionaryItem>();
    
    public void Initialize() {
        foreach (var item in items) {
            if (languageDictionary.ContainsKey(item.key) == false) {
                languageDictionary.Add(item.key, item);
            }
        }
    }

    public LanguageSettingDictionaryItem GetValue(string key) {
        return languageDictionary[key];
    }

    //stageselectSceneManager 코드 응용
    /*
    public void CanvasControll(int index){
        for(int i = 0; i < sceneCanavsArray.Length; i++){
            sceneCanavsArray[i].gameObject.SetActive(false);
        }

        sceneCanavsArray[index].gameObject.SetActive(true);
    }*/
}
