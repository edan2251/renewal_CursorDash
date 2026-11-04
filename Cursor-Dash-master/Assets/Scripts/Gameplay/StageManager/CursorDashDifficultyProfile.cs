using UnityEngine;

/// <summary>One authored profile for normal Cyberpunk encounters; reused by the existing reader and node pool.</summary>
[CreateAssetMenu(menuName = "CursorDash/Multiplier difficulty profile")]
public sealed class CursorDashDifficultyProfile : ScriptableObject
{
    [Tooltip("OFF: use the unchanged legacy main/sub text patterns for comparison.")]
    public bool useMultiplierEncounters = true;
    [Tooltip("Added to the existing spawn.interval. Tier spacing scales both this pause and phrase steps.")]
    [Range(0, 2)] public float phraseRestExtra = .65f;
    [Tooltip("Normal-combat seconds after fever has fully finished; never taken from the 8-second fever.")]
    [Range(0, 2)] public float feverRecovery = .8f;
    [Range(0, 2)] public float damageRecovery = .8f;
    [Tooltip("A blocked spawn waits briefly, then is skipped; it is never released later as a backlog.")]
    [Range(.1f, 1)] public float placementWait = .6f;
    [Range(0, .5f)] public float nodeGap = .18f;
    [Range(.25f, 1.5f)] public float nexusClearance = .65f;
    [Range(.1f, .5f)] public float cursorClearance = .2f;
    [Tooltip("Exactly four rows, for x1 / x2 / x3 / x4. Only spawn timing changes, never node attack clocks.")]
    public CursorDashEncounterTier[] tiers;
    [Tooltip("Delays are relative to the preceding spawn. Weights are x1,x2,x3,x4; zero excludes that phrase.")]
    public CursorDashEncounterPattern[] patterns;
    public bool IsValid => CursorDashEncounterRules.Valid(tiers, patterns) &&
        InRange(phraseRestExtra, 0, 2) && InRange(feverRecovery, 0, 2) && InRange(damageRecovery, 0, 2) &&
        InRange(placementWait, .1f, 1) && InRange(nodeGap, 0, .5f) && InRange(nexusClearance, .25f, 1.5f) && InRange(cursorClearance, .1f, .5f);
    private static bool InRange(float value, float min, float max) => !float.IsNaN(value) && value >= min && value <= max;
    public CursorDashEncounterTier ForMultiplier(int multiplier) => tiers[CursorDashEncounterRules.Tier(multiplier) - 1];
}
