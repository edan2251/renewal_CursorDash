using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PatternData {
    private List<PatternStep> patternSteps = new List<PatternStep>();
    public List<PatternStep> PatternSteps => patternSteps;
}

[Serializable]
public class PatternStep {
    private Action currentStepAction;
    public Action CurrentStepAction {
        get => currentStepAction;
        set => currentStepAction = value;
    }

    private float waitingTime;
    public float WaitingTime {
        get => waitingTime;
        set => waitingTime = value;
    }

    public IEnumerator Execute() {
        yield return YieldInstructionCache.WaitingSeconds(waitingTime);
        currentStepAction?.Invoke();
    }
}


public class PatternReader : MonoBehaviour {
    private List<PatternData> mainPatterns = new List<PatternData>();
    private List<PatternData> subPatterns = new List<PatternData>();


    private void Awake() {
        Execute();
    }

    private void Start() {
        GenerateNode().Start(this);
    }
    
    private IEnumerator GenerateNode() {
        while (StageManager.instance.IsDeath == false) {
            var randomPatterns = mainPatterns[Random.Range(0, mainPatterns.Count)];
            foreach (var step in randomPatterns.PatternSteps) {
                yield return StartCoroutine(step.Execute());
            }
            
            randomPatterns = subPatterns[Random.Range(0, subPatterns.Count)];
            foreach (var step in randomPatterns.PatternSteps) {
                yield return StartCoroutine(step.Execute());
            }

            yield return YieldInstructionCache.WaitingSeconds(1.4f);
        }
    }
    
    public (List<PatternData>, List<PatternData>) Execute() {
        FileRead(GameManager.instance.MapInformation.mainPatternFile, mainPatterns);
        FileRead(GameManager.instance.MapInformation.mainPatternFile, subPatterns);
        
        return (mainPatterns, subPatterns);
    }
    
    private void FileRead(TextAsset mapFile, List<PatternData> patternList) {
        var patternFile = mapFile;
        var patternTexts = patternFile.text.Split(',');
        
        for (int i = 0; i < patternTexts.Length; i++) {
            MakePattern(patternList, patternTexts[i]);
        }
    }

    private void MakePattern(List<PatternData> patternList, string data) {
        if (data.Equals("")) {
            return;
        }
        
        var newPatternData = new PatternData();
        data = data.Replace("\r\n", "\n");
        var stepInformations = data.Split('\n');

        foreach (var information in stepInformations) {
            var newPatternStep = new PatternStep();

            if (information.StartsWith("//") || information.Equals("")) {
                continue;
            }
            
            if (information.StartsWith("#")) {
                float waitingValue = float.Parse(information.Split('#')[1]);
                newPatternStep.WaitingTime = waitingValue;
                newPatternData.PatternSteps.Add(newPatternStep);
                continue;
            }

            var detailedInformation = information.Split('/');
            try {
                newPatternStep.CurrentStepAction = PatternAction(int.Parse(detailedInformation[0]),
                    int.Parse(detailedInformation[1]));
            }
            catch {
                "a".Log();
            }
            newPatternData.PatternSteps.Add(newPatternStep);
        }
        
        patternList.Add(newPatternData);
    }
    
    private Action PatternAction(int position, int nodeStyle) {
        return () => {
            StageManager.instance.NodeCreator.CreateNode(nodeStyle, position);
        };
    }
}
