using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TextCore.LowLevel;

/// <summary>One explicit upgrade of the saved SettingsView, retaining its GUID and existing controls.</summary>
public static class CursorDashBottomPanelsSetup
{
    public const string SettingsPrefab="Assets/Prefabs/UI/SettingsView.prefab";
    public static void RefineSavedMenu()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(Application.dataPath.Replace('\\','/')!="D:/renewal_CursorDash/Cursor-Dash-master/Assets"||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||scene.isDirty||scene.path!="Assets/Scenes/Cyberpunk.unity"||PrefabStageUtility.GetCurrentPrefabStage()!=null)throw new InvalidOperationException("Saved idle CursorDash required");
        CursorDashUIAuthoringPreview.Stop();
        var root=PrefabUtility.LoadPrefabContents(SettingsPrefab);
        try{
            var view=root.GetComponent<CursorDashSettingsView>();if(!view.HasMenuPanels)throw new InvalidOperationException("Existing menu required");
            view.RefineBottomPanelsForAuthoring();view.Layout(new Rect(0,0,540,960),new Vector2(540,960));view.Present(true,1,0,false);
            view.Canvas.renderMode=RenderMode.ScreenSpaceOverlay;view.Canvas.worldCamera=null;
            PrefabUtility.SaveAsPrefabAsset(root,SettingsPrefab);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        // All new references live in the existing prefab. No scene/other assets are saved.
    }
    public static void UpgradeCurrentScene()
    {
        if(Application.dataPath.Replace('\\','/')!="D:/renewal_CursorDash/Cursor-Dash-master/Assets")throw new InvalidOperationException("Unexpected project");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||scene.isDirty||scene.path!="Assets/Scenes/Cyberpunk.unity"||PrefabStageUtility.GetCurrentPrefabStage()!=null)throw new InvalidOperationException("Saved, idle Cyberpunk required; user work is never saved/discarded automatically.");
        CursorDashUIAuthoringPreview.Stop();ImportMenuIcons();
        var stage=UnityEngine.Object.FindFirstObjectByType<StageManager>();
        if(stage==null||stage.SettingsView==null||PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(stage.SettingsView.gameObject)!=SettingsPrefab)throw new InvalidOperationException("Expected the explicit existing SettingsView instance");
        UpgradePrefab(SettingsPrefab);
        if(!stage.SettingsView.HasMenuPanels||!stage.SettingsView.IsConfigured)throw new InvalidOperationException("Saved prefab did not propagate its explicit references");
        // Only the approved scene is saved. SaveAssets is deliberately not used.
        stage.SettingsView.Canvas.renderMode=RenderMode.ScreenSpaceOverlay;stage.SettingsView.Canvas.worldCamera=null;
        PrefabUtility.RecordPrefabInstancePropertyModifications(stage.SettingsView.Canvas);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    public static void ImportMenuIcons()
    {
        foreach(string name in new[]{"leaderboard","settings","restart_alt","language","keyboard_arrow_down","kr","us","jp","cn"})
        {
            string path="Assets/Resources/CursorDash/MenuIcons/"+name+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new InvalidOperationException("Import the downloaded icon first: "+path);
            if(importer.textureType==TextureImporterType.Sprite&&importer.spriteImportMode==SpriteImportMode.Single&&!importer.mipmapEnabled&&importer.textureCompression==TextureImporterCompression.Uncompressed)continue;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=256;importer.SaveAndReimport();
        }
    }
    public static void InstallMenuFonts()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(Application.dataPath.Replace('\\','/')!="D:/renewal_CursorDash/Cursor-Dash-master/Assets"||EditorApplication.isPlayingOrWillChangePlaymode||scene.isDirty||scene.path!="Assets/Scenes/Cyberpunk.unity")throw new InvalidOperationException("Saved target Edit Mode required");
        CursorDashUIAuthoringPreview.Stop();
        var root=PrefabUtility.LoadPrefabContents(SettingsPrefab);
        try
        {
            var view=root.GetComponent<CursorDashSettingsView>();var data=new SerializedObject(view);
            foreach(string region in new[]{"J","SC"})
            {
                string field=region=="J"?"japanese":"chinese";var chars=new HashSet<char>();for(int n=32;n<127;n++)chars.Add((char)n);
                var entries=data.FindProperty("menuTexts");for(int n=0;n<entries.arraySize;n++)foreach(char c in entries.GetArrayElementAtIndex(n).FindPropertyRelative(field).stringValue)if(!char.IsControl(c))chars.Add(c);
                string path="Assets/Resources/CursorDash/Fonts/Sarasa UI "+region+" Menu SDF.asset";
                var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if(font==null)
                {
                    var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/SarasaUI/SarasaUi"+region+"-SemiBold.ttf");if(source==null)throw new InvalidOperationException("Chosen official font is not imported");
                    font=TMP_FontAsset.CreateFontAsset(source,48,6,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,false);font.name="Sarasa UI "+region+" Menu SDF";
                    string missing;if(!font.TryAddCharacters(new string(new List<char>(chars).ToArray()),out missing))throw new InvalidOperationException("Missing chosen menu glyphs: "+missing);
                    font.atlasPopulationMode=AtlasPopulationMode.Static;
                    var serialized=new SerializedObject(font);foreach(string key in new[]{"m_SourceFontFile","m_SourceFontFile_EditorRef"}){var p=serialized.FindProperty(key);if(p!=null)p.objectReferenceValue=null;}var guid=serialized.FindProperty("m_SourceFontFileGUID");if(guid!=null)guid.stringValue="";serialized.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.CreateAsset(font,path);font.material.name=font.name+" Material";AssetDatabase.AddObjectToAsset(font.material,font);foreach(var atlas in font.atlasTextures){atlas.name=font.name+" Atlas";AssetDatabase.AddObjectToAsset(atlas,font);}EditorUtility.SetDirty(font);AssetDatabase.SaveAssetIfDirty(font);
                }
                data.FindProperty(region=="J"?"japaneseMenuFont":"chineseMenuFont").objectReferenceValue=font;
            }
            data.FindProperty("localizeMenuCopy").boolValue=true;data.ApplyModifiedPropertiesWithoutUndo();view.Canvas.renderMode=RenderMode.ScreenSpaceOverlay;view.Canvas.worldCamera=null;PrefabUtility.SaveAsPrefabAsset(root,SettingsPrefab);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        var stage=UnityEngine.Object.FindFirstObjectByType<StageManager>();stage.SettingsView.Canvas.renderMode=RenderMode.ScreenSpaceOverlay;stage.SettingsView.Canvas.worldCamera=null;PrefabUtility.RecordPrefabInstancePropertyModifications(stage.SettingsView.Canvas);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    public static void UpgradePrefab(string path)
    {
        if(path!=SettingsPrefab&&!path.StartsWith("Assets/Tests/Editor/MenuFixture_",StringComparison.Ordinal))throw new InvalidOperationException("Unexpected target prefab");
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var view=root.GetComponent<CursorDashSettingsView>();if(view==null)throw new InvalidOperationException("Missing existing SettingsView");
            if(view.HasMenuPanels)
            {
                bool repaired=false;
                if(view.Canvas.renderMode!=RenderMode.ScreenSpaceOverlay){view.Canvas.renderMode=RenderMode.ScreenSpaceOverlay;view.Canvas.worldCamera=null;repaired=true;}
                foreach(var icon in view.GetComponentsInChildren<CursorDashMenuIcon>(true)){
                    if(icon.GetComponent<CanvasRenderer>()==null){icon.gameObject.AddComponent<CanvasRenderer>();repaired=true;}
                    if(icon.sprite==null){icon.AssignDefaultArtwork();repaired=true;}
                }
                var data=new SerializedObject(view);
                foreach(string name in new[]{"resetAudio","resetEffects","languageButton"})
                {
                    var button=(UnityEngine.UI.Button)data.FindProperty(name).objectReferenceValue;
                    if(button.targetGraphic==button.GetComponent<UnityEngine.UI.Image>())
                    {
                        button.targetGraphic=button.GetComponentInChildren<CursorDashMenuIcon>(true);var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.24f,.88f,.81f);colors.pressedColor=Color.white;colors.fadeDuration=.08f;button.colors=colors;repaired=true;
                    }
                }
                var best=(TMP_Text)data.FindProperty("rankingBest").objectReferenceValue;if(best.fontSizeMin!=14){best.fontSizeMin=14;repaired=true;}
                if(repaired)PrefabUtility.SaveAsPrefabAsset(root,path);return;
            }
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/CursorDash/Fonts/esamanru Medium View SDF.asset");
            view.AddBottomPanelsForAuthoring(font);view.Layout(new Rect(0,0,540,960),new Vector2(540,960));view.Present(true,1,0,false);
            view.Canvas.renderMode=RenderMode.ScreenSpaceOverlay;view.Canvas.worldCamera=null;view.Canvas.enabled=true;
            if(!view.IsConfigured||!view.HasMenuPanels)throw new InvalidOperationException("Incomplete controls");
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
}
