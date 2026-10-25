using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit task-local diagnostics, no SaveAssets/SaveScene/configuration/pref writes.
public static class CursorDashGraphicsWorkbench
{
    private static string Run=>Path.GetFullPath(SessionState.GetString("NodeGraphics.OutputRoot",Path.Combine(Application.dataPath,"../Logs/node-graphics-20261024-192424")));
    [MenuItem("Tools/CursorDash Graphics/A Feedback Revision State")]
    public static void RevisionState(){SessionState.SetString("NodeGraphics.OutputRoot",Path.GetFullPath("Logs/hitfeel2-20261024-231337"));Snapshot();}
    [MenuItem("Tools/CursorDash Graphics/5 Readonly State")]
    public static void Snapshot()
    {
        var scenes=Enumerable.Range(0,SceneManager.sceneCount).Select(i=>SceneManager.GetSceneAt(i)).Select(s=>new{s.path,s.isDirty}).ToArray();
        var dirty=Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Where(o=>EditorUtility.IsDirty(o)&&!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)))
            .Select(o=>new{name=o.name,path=AssetDatabase.GetAssetPath(o),type=o.GetType().Name}).ToArray();
        string[] keys={"CursorDash_Best_Cyberpunk","BGM","SFX","OnVibe","CursorDash.v2.user","CursorDash.v2.presets","CursorDash.v2.tutorial"};
        var prefs=keys.Select(k=>new{key=k,exists=PlayerPrefs.HasKey(k),text=PlayerPrefs.GetString(k,""),number=PlayerPrefs.GetFloat(k)}).ToArray();
        var graphics=Resources.FindObjectsOfTypeAll<UnityEngine.UI.Image>().Where(i=>i.gameObject.scene.IsValid())
            .Select(i=>new{i.name,root=i.transform.root.name,scene=i.gameObject.scene.name,active=i.gameObject.activeInHierarchy,i.enabled,sprite=i.sprite==null?"":i.sprite.name,spritePath=i.sprite==null?"":AssetDatabase.GetAssetPath(i.sprite)}).ToArray();
        string file=Path.Combine(Run,"editor-state-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".json");Directory.CreateDirectory(Run);
        File.WriteAllText(file,JsonConvert.SerializeObject(new{utc=DateTime.UtcNow,project=Application.dataPath,EditorApplication.isPlaying,EditorApplication.isCompiling,EditorApplication.isPaused,timeScale=Time.timeScale,scenes,dirty,prefs,graphics,
            shaderSupported=Resources.Load<Shader>("CursorDash/NodeFlat")?.isSupported},Formatting.Indented));
        Debug.Log("NODE_GRAPHICS_READONLY_STATE: "+file);
    }
    [MenuItem("Tools/CursorDash Graphics/6 Build Local WebGL")]
    public static void Build()
    {
        Snapshot();
        string identity=CursorDashTuning.BuildId;
        if(!identity.StartsWith("nodefx-")||identity.IndexOfAny(Path.GetInvalidFileNameChars())>=0)throw new InvalidOperationException("New nodefx build identity required");
        CursorDashLocalPreviewBuild.Begin(Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/LocalDevPreview",identity)));
    }
}
