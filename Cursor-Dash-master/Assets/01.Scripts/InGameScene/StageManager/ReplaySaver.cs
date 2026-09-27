using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ReplaySaver : MonoBehaviour
{
    private List<string> replayInformation = new List<string>();

    public void AddReplayInformation(string value){
        replayInformation.Add(value);
    }

    private void OnDestroy() {
        string resourcesPath = Application.dataPath + "/Resources";
        string filePath = resourcesPath + "/Highlight_" + (GameManager.instance?.stageIndex.ToString("D2") ?? "00") + ".txt";

        if(File.Exists(filePath)){
            File.Delete(filePath);
        }

        using(StreamWriter file = File.CreateText(filePath)){
            replayInformation.ForEach((value) => {
                file.WriteLine(value);
            });
            file.Close();
        }
    }
}
