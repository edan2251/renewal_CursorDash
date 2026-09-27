using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

public class JsonReader : MonoBehaviour
{
    public void Save(){
        JsonData jsonData = JsonMapper.ToJson(null);
        File.WriteAllText(Application.dataPath + "/Resources/SaveFile/fileName.json", jsonData.ToString());
        
    }

    public void Load(){
        string jsonText = Resources.Load<TextAsset>("SaveFile/fileName.json").ToString();
        JsonData jsonData = JsonMapper.ToObject(jsonText);
    }
}
