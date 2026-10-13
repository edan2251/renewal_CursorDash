using System;
using UnityEngine;

[Serializable]
public class Achieve {
    [SerializeField]
    private string achieveName;
    public string AchieveName => achieveName;
    
    [SerializeField]
    private string achieveDescription;
    public string AchieveDescription => achieveDescription;
    
    [SerializeField]
    private string require;
    public string Require => require;
    
    [SerializeField]
    private int amount;
    public int Amount => amount;

    [SerializeField]
    private string openRewardName;
    public string OpenRewardName => openRewardName;

    [SerializeField]
    private bool isUnlock;
    public bool IsUnlock {
        get => isUnlock;
        set {
            isUnlock = value;
        }
    }
}