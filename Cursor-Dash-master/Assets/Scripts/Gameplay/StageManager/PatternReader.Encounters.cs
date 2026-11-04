using System.Collections;
using UnityEngine;

public partial class PatternReader
{
    [Header("Normal Cyberpunk level design — x1 / x2 / x3 / x4")]
    [SerializeField] private CursorDashDifficultyProfile difficultyProfile;
    private CursorDashEncounterPacer encounterPacer;
    public bool UsesMultiplierDifficulty { get; private set; }
    public int CurrentDifficulty => encounterPacer?.Multiplier ?? 1;
    public string CurrentEncounter { get; private set; } = "";
    public int EncounterSpawns { get; private set; }
    public int SkippedEncounterSpawns { get; private set; }

    private void ResetEncounterState()
    {
        UsesMultiplierDifficulty = false; encounterPacer = null;
        CurrentEncounter = ""; EncounterSpawns = SkippedEncounterSpawns = 0;
    }
    private bool TryStartMultiplierEncounters()
    {
        // Existing explicit main-only/sub-only developer comparisons and other maps keep their old reader.
        if (selection != 0 || GameManager.instance.MapInformation?.mapName != "Cyberpunk") return false;
        if (difficultyProfile == null) difficultyProfile = Resources.Load<CursorDashDifficultyProfile>("CursorDash/MultiplierDifficulty");
        if (difficultyProfile == null || !difficultyProfile.useMultiplierEncounters) return false;
        if (!difficultyProfile.IsValid)
        {
            Debug.LogWarning("CursorDash: invalid multiplier difficulty profile; using legacy text patterns.");
            return false;
        }
        UsesMultiplierDifficulty = true; encounterPacer = new CursorDashEncounterPacer();
        StartCoroutine(GenerateEncounters()); return true;
    }

    private void ObserveEncounter()
    {
        var stage = StageManager.instance;
        var model = stage?.Fever?.Model;
        if (model == null) return;
        encounterPacer.Observe(model.Multiplier, model.CompletedFevers, model.ResetVersion, stage.NormalCombatTime,
            stage.NormalThreatsSuppressed, difficultyProfile.feverRecovery, difficultyProfile.damageRecovery);
    }
    private bool EncounterAlive => StageManager.instance != null && !StageManager.instance.IsDeath;
    private bool CanSupplyEncounter => EncounterAlive && StageManager.instance.IsPlaying &&
        !StageManager.instance.NormalThreatsSuppressed && Time.deltaTime > 0;
    private bool SameEncounter(int revision) => EncounterAlive && encounterPacer.Revision == revision;
    private double EncounterRandom() => seededRandom == null ? Random.value : seededRandom.NextDouble();

    private IEnumerator GenerateEncounters()
    {
        int previous = -1, phrase = 0, previousTurn = -1, observedRevision = -1;
        while (EncounterAlive)
        {
            ObserveEncounter();
            if (!CanSupplyEncounter || !encounterPacer.Ready(StageManager.instance.NormalCombatTime)) { yield return null; continue; }
            int revision = encounterPacer.Revision;
            if (revision != observedRevision) { phrase = 0; observedRevision = revision; }
            var tier = difficultyProfile.ForMultiplier(CurrentDifficulty);
            bool recovery = phrase % 3 == 2;
            int index = CursorDashEncounterRules.Choose(difficultyProfile.patterns, CurrentDifficulty, recovery, previous, EncounterRandom());
            if (index < 0) { Debug.LogWarning("CursorDash: no eligible encounter."); yield break; }
            var pattern = difficultyProfile.patterns[index];
            previous = index; phrase++; CurrentEncounter = pattern.name;
            int turn = Mathf.Min(3, (int)(EncounterRandom() * 4));
            if (turn == previousTurn) turn = (turn + 1) % 4;
            previousTurn = turn;
            bool mirror = EncounterRandom() < .5;
            // A phrase owns its spawn spacing. Already active nodes continue their original clocks unchanged.
            float spacing = tier.spacingScale;
            foreach (var step in pattern.steps)
            {
                yield return WaitEncounter(step.delay * spacing, revision);
                if (!SameEncounter(revision)) break;
                int lane = CursorDashEncounterRules.Lane(step.lane, turn, mirror);
                double until = StageManager.instance.NormalCombatTime + difficultyProfile.placementWait;
                bool spawned = false;
                do
                {
                    ObserveEncounter();
                    if (!SameEncounter(revision)) break;
                    if (CanSupplyEncounter && StageManager.instance.NodeCreator.TryCreateEncounterNode(step.kind, lane, tier, difficultyProfile))
                    { spawned = true; EncounterSpawns++; break; }
                    yield return WaitEncounter(.12f, revision);
                } while (SameEncounter(revision) && StageManager.instance.NormalCombatTime < until);
                if (!SameEncounter(revision)) break;
                if (!spawned) SkippedEncounterSpawns++;
            }
            if (!SameEncounter(revision)) continue;
            if (!repeat) yield break;
            float rest = (betweenPatterns + difficultyProfile.phraseRestExtra) * spacing + (recovery ? tier.recoveryExtra : 0);
            yield return WaitEncounter(rest, revision);
        }
    }
    private IEnumerator WaitEncounter(float seconds, int revision)
    {
        if (!EncounterAlive) yield break;
        double target = StageManager.instance.NormalCombatTime + Mathf.Max(0, seconds);
        while (EncounterAlive)
        {
            ObserveEncounter();
            if (!SameEncounter(revision)) yield break;
            if (CanSupplyEncounter && StageManager.instance.NormalCombatTime >= target) yield break;
            yield return null;
        }
    }
}
