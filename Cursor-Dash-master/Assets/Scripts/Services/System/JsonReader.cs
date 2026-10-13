using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

public class JsonReader : MonoBehaviour
{
    public void Save(){
        JsonData jsonData = JsonMapper.ToJson(null);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "fileName.json"), jsonData.ToString());
        
    }

    public void Load(){
        string filePath = Path.Combine(Application.persistentDataPath, "fileName.json");
        string jsonText = File.Exists(filePath)
            ? File.ReadAllText(filePath)
            : Resources.Load<TextAsset>("SaveFile/fileName")?.text;
        JsonData jsonData = JsonMapper.ToObject(jsonText);
    }
}
