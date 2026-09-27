using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

[Serializable]
public class CharacterColorInformation {
    [SerializeField]
    private bool _isUnlock;

    public bool IsUnlock {
        get => _isUnlock;
        set => _isUnlock = value;
    }
    
    [SerializeField]
    private Color _colorValue;

    public Color ColorValue {
        get => _colorValue;
        set => _colorValue = value;
    }
    
    [SerializeField]
    private string keyName;
    public string KeyName => keyName;
}

[Serializable]
public class CharacterSpriteInformation {
    [SerializeField]
    private bool _isUnlock;
    
    public bool IsUnlock {
        get => _isUnlock;
        set => _isUnlock = value;
    }

    [SerializeField]
    private Sprite _characterSprite;

    public Sprite CharacterSprite => _characterSprite;
    
    [SerializeField]
    private string keyName;
    public string KeyName => keyName;
}

[CreateAssetMenu(fileName = "CharacterSetting", menuName = "Scriptable Object/Character Setting", order = 0)]
public class CharacterSetting : ScriptableObject {
    [SerializeField]
    private CharacterColorInformation[] characterColors;
    public CharacterColorInformation[] CharacterColors => characterColors;

    [SerializeField]
    private CharacterSpriteInformation[] characterSprites;
    public CharacterSpriteInformation[] CharacterSprites => characterSprites;

    private CharacterColorInformation _selectColor;
    public CharacterColorInformation selectColor {
        get => _selectColor;
        set => _selectColor = value;
    }

    private CharacterSpriteInformation _selectSprite;
    public CharacterSpriteInformation selectSprite {
        get => _selectSprite;
        set => _selectSprite = value;
    }

    public void Unlock(string key) {
        foreach (var information in characterSprites) {
            if (information.KeyName.Equals(key)) {
                information.IsUnlock = true;
            }
        }       
        
        foreach (var information in characterColors) {
            if (information.KeyName.Equals(key)) {
                information.IsUnlock = true;
            }
        }
    }

    [Button("Reset")]
    public void AllLock() {
        foreach (var information in characterSprites) {
            information.IsUnlock = false;
        }

        foreach (var information in characterColors) {
            information.IsUnlock = false;
        }
    }
}
