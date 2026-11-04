using System;

// Encounter data and scheduling rules only. Attack clocks, rewards and physics remain on the real nodes.
[Serializable]
public sealed class CursorDashEncounterStep
{
    public float delay;
    public int kind; // 0 circle / 1 hexagon
    public int lane; // Clockwise: upper-left, upper-right, lower-right, lower-left, before rotation.
}

[Serializable]
public sealed class CursorDashEncounterPattern
{
    public string name;
    public bool recovery;
    public int[] weights = { 1, 1, 1, 1 };
    public CursorDashEncounterStep[] steps;
}

[Serializable]
public sealed class CursorDashEncounterTier
{
    public string name;
    public float spacingScale = 1;
    public float recoveryExtra = .5f;
    public float angularSpread = 8;
    public float radialSpread = .18f;
    public int activeLimit = 4;
    public int hexLimit = 2;
}

public static class CursorDashEncounterRules
{
    public static int Tier(int multiplier) => Math.Max(1, Math.Min(4, multiplier));
    public static int Lane(int lane, int quarterTurn, bool mirror)
    {
        int clockwise = ((mirror ? -lane : lane) + quarterTurn + 8) % 4;
        return clockwise < 2 ? clockwise : 5 - clockwise; // Existing scene order is UL, UR, LL, LR.
    }
    public static bool Separated(double ax, double ay, double ar, double bx, double by, double br, double gap)
    {
        double x = ax - bx, y = ay - by, radius = ar + br + gap;
        return x * x + y * y >= radius * radius;
    }
    public static int Weight(CursorDashEncounterPattern pattern, int multiplier)
    {
        int index = Tier(multiplier) - 1;
        return pattern?.weights != null && pattern.weights.Length > index ? Math.Max(0, pattern.weights[index]) : 0;
    }
    // One slower phrase after two pressure phrases; never repeats the same eligible phrase immediately.
    public static int Choose(CursorDashEncounterPattern[] patterns, int multiplier, bool recovery, int previous, double roll)
    {
        int total = 0, alternatives = 0;
        for (int i = 0; i < patterns.Length; i++)
            if (patterns[i].recovery == recovery && Weight(patterns[i], multiplier) > 0 && i != previous) alternatives++;
        for (int i = 0; i < patterns.Length; i++)
            if (patterns[i].recovery == recovery && (alternatives == 0 || i != previous)) total += Weight(patterns[i], multiplier);
        if (total <= 0) return -1;
        double ticket = Math.Max(0, Math.Min(.999999999, roll)) * total;
        for (int i = 0; i < patterns.Length; i++)
        {
            if (patterns[i].recovery != recovery || (alternatives > 0 && i == previous)) continue;
            ticket -= Weight(patterns[i], multiplier);
            if (ticket < 0) return i;
        }
        return -1;
    }
    public static bool Valid(CursorDashEncounterTier[] tiers, CursorDashEncounterPattern[] patterns)
    {
        if (tiers == null || tiers.Length != 4 || patterns == null || patterns.Length == 0) return false;
        foreach (var tier in tiers)
            if (tier == null || !Finite(tier.spacingScale) || tier.spacingScale < .5f || tier.spacingScale > 1.5f ||
                !Finite(tier.recoveryExtra) || tier.recoveryExtra < 0 || !Finite(tier.angularSpread) || tier.angularSpread < 0 || tier.angularSpread > 40 ||
                !Finite(tier.radialSpread) || tier.radialSpread < 0 || tier.radialSpread > .6f ||
                tier.activeLimit < 1 || tier.activeLimit > 10 || tier.hexLimit < 1 || tier.hexLimit > tier.activeLimit) return false;
        foreach (var pattern in patterns)
        {
            if (pattern == null || pattern.weights == null || pattern.weights.Length != 4 || pattern.steps == null || pattern.steps.Length == 0) return false;
            foreach (var weight in pattern.weights) if (weight < 0 || weight > 100) return false;
            foreach (var step in pattern.steps)
                if (step == null || !Finite(step.delay) || step.delay < 0 || step.kind < 0 || step.kind > 1 || step.lane < 0 || step.lane > 3) return false;
        }
        for (int multiplier = 1; multiplier <= 4; multiplier++)
            if (Choose(patterns, multiplier, false, -1, 0) < 0 || Choose(patterns, multiplier, true, -1, 0) < 0) return false;
        return true;
    }
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}

// Observes the existing model's NORMAL-combat clock. No wall clock, stored progression or extra fever rules.
public sealed class CursorDashEncounterPacer
{
    private bool initialized, suppressed;
    private uint reset;
    private int completed;
    public int Multiplier { get; private set; } = 1;
    public int Revision { get; private set; }
    public double ReadyAt { get; private set; }
    public void Observe(int multiplier, int completedFevers, uint resetVersion, double combatTime, bool threatsSuppressed,
        double feverRecovery, double damageRecovery)
    {
        int tier = CursorDashEncounterRules.Tier(multiplier);
        if (!initialized)
        {
            initialized = true; Multiplier = tier; completed = completedFevers; reset = resetVersion; suppressed = threatsSuppressed;
            ReadyAt = combatTime; return;
        }
        bool hitOrReset = resetVersion != reset;
        bool returned = suppressed && !threatsSuppressed;
        if (hitOrReset || completedFevers != completed || tier != Multiplier || threatsSuppressed != suppressed)
        {
            Revision++;
            // Discard unspawned events from the old phrase. Existing nodes are not edited or removed here.
            if (hitOrReset) ReadyAt = combatTime + Math.Max(0, damageRecovery);
            else if (returned || completedFevers != completed) ReadyAt = combatTime + Math.Max(0, feverRecovery);
        }
        Multiplier = tier; completed = completedFevers; reset = resetVersion; suppressed = threatsSuppressed;
    }
    public bool Ready(double combatTime) => initialized && !suppressed && combatTime >= ReadyAt;
}
