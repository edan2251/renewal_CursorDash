using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class CharacterViewController : MonoBehaviour {
    private CharacterColorSetter[] characterColorSetters;
    private CharacterSpriteSetter[] characterSpriteSetters;

    [SerializeField]
    private Image characterImage;
    
    private void Awake() {
        characterColorSetters = gameObject.GetComponentsInChildren<CharacterColorSetter>(true);
        characterSpriteSetters = gameObject.GetComponentsInChildren<CharacterSpriteSetter>(true);
    }
    
    private void Start() {
        var characterSetting = GameManager.instance.playerData.characterSetting;
        
        for (int i = 0; i < characterSetting.CharacterColors.Length; i++) {
            characterColorSetters[i].Setting(i, () => CharacterPreviewSetting());
        }

        for (int i = 0; i < characterSetting.CharacterSprites.Length; i++) {
            characterSpriteSetters[i].Setting(i, () => CharacterPreviewSetting());
        }
        
        CharacterPreviewSetting();
    }

    public void CharacterPreviewSetting() {
        try {
            characterImage.sprite = GameManager.instance.playerData.characterSetting.selectSprite.CharacterSprite;
        }
        catch {
            "A".Log();
        }
        characterImage.color = GameManager.instance.playerData.characterSetting.selectColor.ColorValue;
    }
    

    [Button("All Unlock")]
    public void AllUnlock() {
        for (int i = 0; i < characterColorSetters.Length; i++) {
            characterColorSetters[i].Unlock();
        }
        
        for (int i = 0; i < characterSpriteSetters.Length; i++) {
            characterSpriteSetters[i].Unlock();
        }
    }

    [Button("All Lock")]
    public void AllLock() {
        for (int i = 0; i < characterColorSetters.Length; i++) {
            characterColorSetters[i].Lock();
        }
        
        for (int i = 0; i < characterSpriteSetters.Length; i++) {
            characterSpriteSetters[i].Lock();
        }        
    }
}
