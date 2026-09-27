using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MigrationValidation
{
    private static int missingScripts;
    private static int missingReferences;

    public static void Validate()
    {
        missingScripts = 0;
        missingReferences = 0;

        string[] scenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();

        foreach (string path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
                CheckHierarchy(root, path);
        }

        foreach (string path in prefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try { CheckHierarchy(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset != null) CheckReferences(asset, path);
        }

        int missingShaders = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Material"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null && material.shader == null)
            {
                missingShaders++;
                Debug.LogError("MISSING_SHADER " + path);
            }
        }

        int missingBuildScenes = EditorBuildSettings.scenes.Count(s => s.enabled &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path) == null);

        Debug.Log($"MIGRATION_AUDIT scenes={scenePaths.Length} prefabs={prefabPaths.Length} " +
                  $"missingScripts={missingScripts} missingReferences={missingReferences} " +
                  $"missingShaders={missingShaders} missingBuildScenes={missingBuildScenes}");

        if (missingScripts + missingReferences + missingShaders + missingBuildScenes > 0)
            throw new Exception("Migration validation found broken references");
    }

    private static void CheckHierarchy(GameObject root, string path)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            GameObject gameObject = transform.gameObject;
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (count > 0)
            {
                missingScripts += count;
                Debug.LogError($"MISSING_SCRIPT {path}: {GetHierarchyPath(transform)} ({count})");
            }

            foreach (Component component in gameObject.GetComponents<Component>())
                if (component != null) CheckReferences(component, path);
        }
    }

    private static void CheckReferences(UnityEngine.Object asset, string path)
    {
        var serialized = new SerializedObject(asset);
        var property = serialized.GetIterator();
        while (property.NextVisible(true))
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue != null || property.objectReferenceInstanceIDValue == 0)
                continue;

            missingReferences++;
            Debug.LogError($"MISSING_REFERENCE {path}: {asset.name}.{property.propertyPath}");
        }
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    public static void BuildWeb()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Web",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"WEB_BUILD result={report.summary.result} errors={report.summary.totalErrors} " +
                  $"warnings={report.summary.totalWarnings} output={report.summary.outputPath}");
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Web build failed");
    }
}
