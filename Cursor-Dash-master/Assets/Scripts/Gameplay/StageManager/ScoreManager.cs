using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField, Min(1f)] private float defaultHp = 3f;
    private float hp;
    private float timeScore;
    private Action<float> hpEditFunction;
    private Action deathFunction;
    private Action<float> timeScoreFunction;
    public float MaxHp=>defaultHp;
    public event Action Damaged;

    public float Hp
    {
        get => hp;
        set
        {
            float previous = hp;
            hp = Mathf.Clamp(value, 0f, defaultHp);
            hpEditFunction?.Invoke(hp / defaultHp);
            if (hp < previous) { HoldAchivement.IsMaxHp = false;Damaged?.Invoke(); }
            if (previous > 0f && hp <= 0f) deathFunction?.Invoke();
        }
    }
    public float TimeScore
    {
        get => timeScore;
        set { timeScore = Mathf.Max(0f, value); timeScoreFunction?.Invoke(timeScore); }
    }
    public void Initialize()
    {
        defaultHp=CursorDashTuning.Number("nexus.maxHp",defaultHp);
        hp = defaultHp;
        timeScore = 0f;
        HoldAchivement.IsMaxHp = true;
        hpEditFunction?.Invoke(1f);
        timeScoreFunction?.Invoke(0f);
    }
    public void SetHpEditAction(Action<float> action) => hpEditFunction = action;
    public void SetDeathAction(Action action) => deathFunction = action;
    public void SetScoreAction(Action<float> action) => timeScoreFunction = action;
}
