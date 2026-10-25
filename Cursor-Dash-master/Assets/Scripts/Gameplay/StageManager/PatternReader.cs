using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class PatternData
{
    private readonly List<PatternStep> patternSteps = new List<PatternStep>();
    public List<PatternStep> PatternSteps => patternSteps;
}

[Serializable]
public class PatternStep
{
    public Action CurrentStepAction { get; set; }
    public float WaitingTime { get; set; }
    public int Position { get; set; } = -1;
    public int NodeStyle { get; set; } = -1;
    public IEnumerator Execute()
    {
        if (WaitingTime > 0f) yield return PatternReader.WaitNormalCombat(WaitingTime);
        while (StageManager.instance != null && !StageManager.instance.IsDeath &&
            (!StageManager.instance.IsPlaying||StageManager.instance.NormalThreatsSuppressed))yield return null;
        if (StageManager.instance != null && StageManager.instance.IsPlaying&&!StageManager.instance.NormalThreatsSuppressed) CurrentStepAction?.Invoke();
    }
}

public class PatternReader : MonoBehaviour
{
    private readonly List<PatternData> mainPatterns = new List<PatternData>();
    private readonly List<PatternData> subPatterns = new List<PatternData>();
    [SerializeField, Min(0f)] private float betweenPatterns = 1.4f;
    private int selection;
    private bool repeat;
    private System.Random seededRandom;
    private void Start()
    {
        if(CursorDashSession.IsPractice){enabled=false;return;}
        betweenPatterns=CursorDashTuning.Number("spawn.interval",betweenPatterns);
        selection=CursorDashTuning.Integer("spawn.pattern",0);repeat=CursorDashTuning.Toggle("spawn.repeat",true);
        if(CursorDashTuning.Toggle("spawn.seedEnabled",false))seededRandom=new System.Random(CursorDashTuning.Integer("spawn.seed",1234)+1);
        Execute();
        if (mainPatterns.Count == 0 && subPatterns.Count == 0)
        {
            Debug.LogError("CursorDash: both pattern lists are empty.");
            enabled = false;
            return;
        }
        StartCoroutine(Generate());
    }
    public (List<PatternData>, List<PatternData>) Execute()
    {
        mainPatterns.Clear();
        subPatterns.Clear();
        MapInformation map = GameManager.instance.MapInformation;
        if (map != null)
        {
            if (map.mainPatternFile != null) mainPatterns.AddRange(ParsePatterns(map.mainPatternFile.text));
            if (map.subPatternFile != null) subPatterns.AddRange(ParsePatterns(map.subPatternFile.text));
        }
        return (mainPatterns, subPatterns);
    }
    private IEnumerator Generate()
    {
        while (StageManager.instance != null && !StageManager.instance.IsDeath)
        {
            if(selection!=2)yield return PlayRandom(mainPatterns);
            if(selection!=1)yield return PlayRandom(subPatterns);
            if(!repeat)yield break;
            if (betweenPatterns > 0f) yield return WaitNormalCombat(betweenPatterns);
            else yield return null;
        }
    }
    public static IEnumerator WaitNormalCombat(float seconds)
    {
        // Absolute normal-combat clock: held waits resume at the same remainder, without replay/catch-up.
        var stage=StageManager.instance;if(stage==null)yield break;
        double target=stage.NormalCombatTime+Mathf.Max(0,seconds);
        while(StageManager.instance==stage&&!stage.IsDeath&&stage.NormalCombatTime<target)yield return null;
    }
    private IEnumerator PlayRandom(List<PatternData> patterns)
    {
        if (patterns.Count == 0) yield break;
        foreach (PatternStep step in patterns[seededRandom==null?UnityEngine.Random.Range(0,patterns.Count):seededRandom.Next(patterns.Count)].PatternSteps)
        {
            if (StageManager.instance == null || StageManager.instance.IsDeath) yield break;
            yield return step.Execute();
        }
    }
    public static List<PatternData> ParsePatterns(string text)
    {
        List<PatternData> result = new List<PatternData>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        foreach (string block in text.Replace("\r", "").Split(','))
        {
            PatternData pattern = new PatternData();
            foreach (string raw in block.Split('\n'))
            {
                string line = raw.Trim().TrimStart('\uFEFF');
                if (line.Length == 0 || line.StartsWith("//")) continue;
                if (line.StartsWith("#"))
                {
                    float delay;
                    if (float.TryParse(line.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out delay)
                        && !float.IsNaN(delay) && !float.IsInfinity(delay) && delay >= 0f)
                        pattern.PatternSteps.Add(new PatternStep { WaitingTime = delay });
                    else Debug.LogWarning("CursorDash: invalid pattern delay: " + line);
                    continue;
                }
                string[] fields = line.Split('/');
                int position, style;
                if (fields.Length != 2 || !int.TryParse(fields[0], out position) || !int.TryParse(fields[1], out style)
                    || position < 0 || position > 3 || style < 0 || style > 1)
                {
                    Debug.LogWarning("CursorDash: invalid pattern node: " + line);
                    continue;
                }
                int spawnPosition = position, nodeStyle = style;
                pattern.PatternSteps.Add(new PatternStep
                {
                    Position = position,
                    NodeStyle = style,
                    CurrentStepAction = () => StageManager.instance?.NodeCreator.CreateNode(nodeStyle, spawnPosition)
                });
            }
            if (pattern.PatternSteps.Count > 0) result.Add(pattern);
        }
        return result;
    }
    private void OnDisable() => StopAllCoroutines();
}
