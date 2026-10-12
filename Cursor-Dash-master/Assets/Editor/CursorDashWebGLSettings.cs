using UnityEditor;
using UnityEngine;

public static class CursorDashWebGLSettings
{
    [MenuItem("Tools/CursorDash Remake/Finalize WebGL Settings")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/00.Scenes/01.StageSelectScene.unity", true),
            new EditorBuildSettingsScene("Assets/00.Scenes/Cyberpunk.unity", true)
        };
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        PlayerSettings.defaultWebScreenWidth = 540;
        PlayerSettings.defaultWebScreenHeight = 960;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.WebGL.template = "PROJECT:CursorDash";
        AssetDatabase.SaveAssets();
        Debug.Log("CURSORDASH_WEBGL_SETTINGS: main + Cyberpunk, 540x960, portrait template.");
    }
}
