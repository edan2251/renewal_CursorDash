using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Explicit local preview only. Does not configure, save scenes/assets, install or deploy.</summary>
public static class CursorDashLocalPreviewBuild
{
    private static string ValidateOutput(string output)
    {
        string allowed=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/LocalDevPreview"))+Path.DirectorySeparatorChar;
        output=Path.GetFullPath(output);
        if(!output.StartsWith(allowed,StringComparison.OrdinalIgnoreCase)||Directory.Exists(output))throw new InvalidOperationException("A new LocalDevPreview child directory is required.");
        if(EditorApplication.isPlaying||EditorApplication.isCompiling||BuildPipeline.isBuildingPlayer)throw new InvalidOperationException("Editor must be idle.");
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene: "+UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path);
        if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL)throw new InvalidOperationException("Existing WebGL target required; no automatic switch.");
        return output;
    }
    public static void Begin(string output)
    {
        string destination=ValidateOutput(output);
        EditorApplication.delayCall+=()=>Build(destination);
    }
    // D-026: build-only entry for an idle project with no running Editor. Never enters Play.
    public static void BuildCommandLine()
    {
        try{
            string[] args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-cursorDashOutput");
            if(i<0||i+1>=args.Length)throw new InvalidOperationException("Explicit dedicated output is required.");
            string output=ValidateOutput(args[i+1]);
            WriteFeedbackProof();
            Build(output);
            var state=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(Application.dataPath,"../Logs/CP14-local-build.json")));
            EditorApplication.Exit((string)state["state"]=="Succeeded"?0:1);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    [MenuItem("Tools/CursorDash Graphics/9 Build Current Release (No Play)")]
    public static void BuildCurrentRelease()
    {
        if(Path.GetFullPath(Application.dataPath).Replace('\\','/')!="D:/renewal_CursorDash/Cursor-Dash-master/Assets")throw new InvalidOperationException("Unexpected project");
        string id=CursorDashTuning.BuildId;
        if(!id.StartsWith("cursordash-20261027-feedback-")||id.IndexOfAny(Path.GetInvalidFileNameChars())>=0)throw new InvalidOperationException("Explicit current release identity required");
        Begin(Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/LocalDevPreview",id)));
    }
    [MenuItem("Tools/CursorDash Graphics/7 Build Circle Punch Release (No Play)")]
    public static void BuildCirclePunchRelease()
    {
        if(Path.GetFullPath(Application.dataPath).Replace('\\','/')!="D:/renewal_CursorDash/Cursor-Dash-master/Assets")throw new InvalidOperationException("Unexpected project");
        string id=CursorDashTuning.BuildId;
        if(!id.StartsWith("circlefx-20261025-")||id.IndexOfAny(Path.GetInvalidFileNameChars())>=0)throw new InvalidOperationException("Explicit release identity required");
        string destination=ValidateOutput(Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/LocalDevPreview",id)));
        WriteFeedbackProof();Begin(destination);
    }
    [MenuItem("Tools/CursorDash Graphics/8 Read Circle Hex Feedback (No Play)")]
    public static void WriteFeedbackProof()
    {
    if(EditorApplication.isPlaying)throw new InvalidOperationException("Read-only build proof requires Edit Mode.");
    var profiles=new Newtonsoft.Json.Linq.JObject();
    foreach(string kind in new[]{"circleRemove","triangleBreak"}){
        var values=new Newtonsoft.Json.Linq.JObject();
        foreach(string field in new[]{"stop","shake","shakeDuration","shakeDecay","frequency","rebound","distortion","radius","duration","recovery","flash"})
            values[field]=CursorDashTuning.Number("fx."+kind+"."+field,(float)CursorDashTuning.Find("fx."+kind+"."+field).defaultValue);
        profiles[kind]=values;
    }
    for(int n=0;n<12;n++){
        var direction=new Vector2(Mathf.Cos(n*Mathf.PI/6),Mathf.Sin(n*Mathf.PI/6));
        if(Vector2.Dot(NexusImpactVfx.FinalDirection(direction),direction)<.999f)throw new InvalidOperationException("Final direction invariant failed.");
    }
    string proof=Path.Combine(Application.dataPath,"../Logs/circle-punch-release-20261025/editor-build-proof.json");
    File.WriteAllText(proof,Newtonsoft.Json.JsonConvert.SerializeObject(new{isPlaying=EditorApplication.isPlaying,profiles,finalDirectionsChecked=12,buildId=CursorDashTuning.BuildId,project=Application.dataPath},Newtonsoft.Json.Formatting.Indented));
    }
    private static void Build(string destination)
    {
        var statePath=Path.Combine(Application.dataPath,"../Logs/CP14-local-build.json");
        Directory.CreateDirectory(Path.GetDirectoryName(statePath));
        File.WriteAllText(statePath,"{\"state\":\"building\",\"output\":"+Newtonsoft.Json.JsonConvert.SerializeObject(destination)+"}");
        try{
            // Use the approved registered order, including the shared-space Cyberpunk entry scene.
            var scenes=EditorBuildSettings.scenes.Where(scene=>scene.enabled).Select(scene=>scene.path).ToArray();
            if(scenes.Length==0||scenes.Any(scene=>!File.Exists(scene)))throw new InvalidOperationException("Registered build scenes must exist.");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=destination,target=BuildTarget.WebGL,options=BuildOptions.None});
            File.WriteAllText(statePath,Newtonsoft.Json.JsonConvert.SerializeObject(new{state=report.summary.result.ToString(),output=destination,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,buildId=CursorDashTuning.BuildId,finishedUtc=DateTime.UtcNow.ToString("o")},Newtonsoft.Json.Formatting.Indented));
            Debug.Log("CP14_LOCAL_BUILD_FINISHED: "+report.summary.result+" "+destination);
        }catch(Exception e){File.WriteAllText(statePath,Newtonsoft.Json.JsonConvert.SerializeObject(new{state="Exception",error=e.ToString(),output=destination}));Debug.LogException(e);}
    }
}
