using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "AchieveData", menuName = "Scriptable Object/Achieve Data", order = 0)]
public class AchieveData : ScriptableObject {
    [SerializeField]
    private Achieve[] achieves;
    public Achieve[] Achieves => achieves;

    public void Unlock(string achieveName, int amount) {
        var findAchieves = new List<Achieve>();
        
        foreach (var achieve in achieves) {
            if (achieve.Require.Equals(achieveName)) {
                findAchieves.Add(achieve);
            }
        }

        foreach (var findAchieve in findAchieves) {
            if (findAchieve.IsUnlock == false) {
                if (findAchieve.Amount == -1 ||  amount < findAchieve.Amount) {
                    return;
                }
                
                findAchieve.IsUnlock = true;
                GameManager.instance.playerData.characterSetting.Unlock(findAchieve.OpenRewardName);
                if (GameManager.instance.Maps != null && GameManager.instance.Maps.Length > 0) {
                    for (int i = 0; i < GameManager.instance.Maps.Length; i++) {
                        GameManager.instance.Maps[i].Unlock(findAchieve.OpenRewardName);
                    }
                }
            }
        }
    }

    public bool GetAchieve(string findAchieve) {
        foreach (var achieve in achieves) {
            if (achieve.AchieveName.Equals(findAchieve)) {
                return achieve.IsUnlock;
            }
        }

        throw new KeyNotFoundException();
    }

    [Button("Reset")]
    public void DataReset() {
        foreach (var achieve in achieves) {
            achieve.IsUnlock = false;
        }
    }

    [Button("Unlock")]
    public void Unlock() {
        foreach (var achieve in achieves) {
            achieve.IsUnlock = true;
            GameManager.instance.playerData.characterSetting.Unlock(achieve.OpenRewardName);

        }
    }
}