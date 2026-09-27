using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Object/Player", order = 0)]
public class PlayerData : ScriptableObject {
    [SerializeField]
    private CharacterSetting _characterSetting;

    public CharacterSetting characterSetting => _characterSetting;

    // Setting 
    
    [SerializeField]
    private AchieveRequireData achieveRequire;
    public AchieveRequireData AchieveRequire => achieveRequire;

    [SerializeField]
    private AchieveData achieveData;
    public AchieveData AchieveData => achieveData;
    
    // Unlock Map
}