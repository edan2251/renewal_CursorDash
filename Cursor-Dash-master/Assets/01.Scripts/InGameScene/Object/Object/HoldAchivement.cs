using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class HoldAchivement : MonoBehaviour
{
    [Dropdown("stages")] [SerializeField]
    private string stage;
    private string[] stages = new[] {"Flame", "Cyberpunk"};

    public static bool IsMaxHp { get; set; }

    private IEnumerator Start()
    {
        IsMaxHp = true;
        int minutes = 0;
        
        while (true)
        {
            minutes++;
            yield return YieldInstructionCache.WaitingSeconds(60f);
            GameManager.instance.playerData.AchieveRequire.SetValueToRequire($"Hold_{stage}", minutes);
            if (IsMaxHp)
                GameManager.instance.playerData.AchieveRequire.SetValueToRequire($"Hold_MaxHp_{stage}", minutes);
        }
    }
}