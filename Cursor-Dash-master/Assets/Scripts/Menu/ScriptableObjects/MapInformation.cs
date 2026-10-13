using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapInformation", menuName = "Assets/MapInformation", order = 0)]
public class MapInformation : ScriptableObject {
    public string mapName;
    public Sprite mapSprite;
    public Sprite mapBackground;
    public Sprite mapBackgroundAfterImage;
    public TextAsset mainPatternFile;
    public TextAsset subPatternFile;
    public int index;
    public float highScore;
    public bool isUnlock;
    public AudioClip audioClip;
    
    public void Unlock(string key) {
        if (key.Equals(mapName)) {
            isUnlock = true;
        }
    }
}