using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>One guarded migration, not a runtime initializer or a scene-wide rebuild.</summary>
public static class CursorDashAuthoringSetup
{
    public const string MenuPrefab = "Assets/Prefabs/UI/MainMenuView.prefab";
    public const string SettingsPrefab = "Assets/Prefabs/UI/SettingsView.prefab";
    public const string ScorePrefab = "Assets/Prefabs/UI/ProjectedScoreView.prefab";

    public static void CreateMainComposition()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.isDirty || scene.path != "Assets/Scenes/Cyberpunk.unity") throw new InvalidOperationException("Saved, idle Cyberpunk required; no user save/discard is performed.");
        var stage = UnityEngine.Object.FindFirstObjectByType<StageManager>();
        if (stage == null || stage.MainLogoView == null || stage.MainMenuView != null || stage.SettingsView != null || stage.ProjectedScoreView != null) throw new InvalidOperationException("Expected the completed logo-only starting point.");
        foreach (var path in new[] { MenuPrefab, SettingsPrefab, ScorePrefab }) if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) throw new InvalidOperationException("Existing authoring prefab must not be overwritten: " + path);
        var medium = Font("Medium"); var bold = Font("Bold");
        var menu = Menu(medium);
        var menuData = new SerializedObject(menu);
        var pause = GameObject.Find("RemakeHUD").transform.Find("PauseButton");
        menuData.FindProperty("pauseButton").objectReferenceValue = pause.gameObject;
        menuData.ApplyModifiedPropertiesWithoutUndo();
        // Scene reference belongs to the instance, never to the reusable prefab asset.
        menuData.FindProperty("pauseButton").objectReferenceValue = null; menuData.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAssetAndConnect(menu.gameObject, MenuPrefab, InteractionMode.AutomatedAction);
        menuData.Update(); menuData.FindProperty("pauseButton").objectReferenceValue = pause.gameObject; menuData.ApplyModifiedPropertiesWithoutUndo();

        var settings = CursorDashSettingsView.CreateDefault(stage.transform, medium);
        foreach (var item in settings.GetComponentsInChildren<Transform>(true)) item.gameObject.hideFlags = HideFlags.None;
        settings.name = "SettingsView"; settings.Canvas.enabled = false;
        PrefabUtility.SaveAsPrefabAssetAndConnect(settings.gameObject, SettingsPrefab, InteractionMode.AutomatedAction);
        var score = Score(bold, Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype"));
        PrefabUtility.SaveAsPrefabAssetAndConnect(score.gameObject, ScorePrefab, InteractionMode.AutomatedAction);
        var data = new SerializedObject(stage);
        data.FindProperty("mainMenuView").objectReferenceValue = menu;
        data.FindProperty("settingsView").objectReferenceValue = settings;
        data.FindProperty("projectedScoreView").objectReferenceValue = score;
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = menu.gameObject;
    }

    private static TMP_FontAsset Font(string weight)
    {
        string path = "Assets/Resources/CursorDash/Fonts/esamanru " + weight + " View SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null) return existing;
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/CursorDash/Fonts/esamanru " + weight + ".ttf");
        if (source == null) throw new InvalidOperationException("Existing official font missing.");
        var font = TMP_FontAsset.CreateFontAsset(source, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        font.name = "esamanru " + weight + " View SDF";
        string glyphs = "0123456789×x+-./%: SCOREBESTPAUSERETRYHOME설정사운드전체음량배경효과화면VFX완화흔들림플래시왜곡색수차튜토리얼부터시작초기계속하기다메인으로클릭해서게임일정지이번점수기록제외";
        font.TryAddCharacters(glyphs, out string missing);
        if (!string.IsNullOrEmpty(missing)) throw new InvalidOperationException("Official font missing requested UI glyphs: " + missing);
        AssetDatabase.CreateAsset(font, path);
        font.material.name = font.name + " Material"; AssetDatabase.AddObjectToAsset(font.material, font);
        foreach (var atlas in font.atlasTextures) if (atlas != null) { atlas.name = font.name + " Atlas"; AssetDatabase.AddObjectToAsset(atlas, font); }
        EditorUtility.SetDirty(font); AssetDatabase.SaveAssetIfDirty(font);
        return font;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 pos)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = Vector2.one * .5f; rect.sizeDelta = size; rect.anchoredPosition = pos; return rect;
    }
    private static RectTransform Stretch(string name, Transform parent)
    {
        var rect = Rect(name, parent, Vector2.one * .5f, Vector2.zero, Vector2.zero); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; return rect;
    }
    private static void Reference(SerializedObject data, string name, UnityEngine.Object value) => data.FindProperty(name).objectReferenceValue = value;

    private static CursorDashMainMenuView Menu(TMP_FontAsset font)
    {
        var root = Rect("MainMenuView", null, Vector2.one * .5f, Vector2.zero, Vector2.zero);
        var canvas = root.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 25; canvas.enabled = false;
        var scaler = root.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
        var placement = Rect("PromptPlacement", root, new Vector2(.5f, .20f), new Vector2(1000, 90), Vector2.zero);
        var anim = Stretch("AnimationRoot", placement); var promptOpacity = anim.gameObject.AddComponent<CanvasGroup>();
        var prompt = Stretch("Start prompt", anim).gameObject.AddComponent<TextMeshProUGUI>();
        prompt.font = font; prompt.fontSize = 38; prompt.color = new Color(.85f, .92f, 1, 1); prompt.text = "클릭해서 게임 시작"; prompt.alignment = TextAlignmentOptions.Center; prompt.raycastTarget = false;
        var captionPlacement = Rect("CaptionPlacement", root, Vector2.one * .5f, new Vector2(600, 55), Vector2.zero);
        var captionAnim = Stretch("AnimationRoot", captionPlacement); var captionOpacity = captionAnim.gameObject.AddComponent<CanvasGroup>();
        var caption = Stretch("BEST caption", captionAnim).gameObject.AddComponent<TextMeshProUGUI>();
        caption.font = font; caption.fontSize = 27; caption.text = "BEST"; caption.color = Color.white; caption.alignment = TextAlignmentOptions.Center; caption.raycastTarget = false;
        var view = root.gameObject.AddComponent<CursorDashMainMenuView>(); var data = new SerializedObject(view);
        Reference(data, "canvas", canvas); Reference(data, "promptPlacement", placement); Reference(data, "promptAnimation", promptOpacity); Reference(data, "prompt", prompt);
        Reference(data, "captionPlacement", captionPlacement); Reference(data, "captionAnimation", captionAnim); Reference(data, "captionOpacity", captionOpacity); Reference(data, "caption", caption);
        data.ApplyModifiedPropertiesWithoutUndo(); return view;
    }

    private static CursorDashProjectedScoreView Score(TMP_FontAsset font, CursorDashVisualProfile profile)
    {
        var placement = Rect("ProjectedScoreView", null, Vector2.one * .5f, new Vector2(4, 4), Vector2.zero);
        var animation = Stretch("AnimationRoot", placement); var text = animation.gameObject.AddComponent<TextMeshPro>();
        text.font = font; text.fontStyle = FontStyles.Normal; text.text = "0";
        text.fontSize = 3.5f; text.fontSizeMin = 1; text.fontSizeMax = 3.5f; text.enableAutoSizing = true;
        text.isOrthographic = true; text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Overflow;
        text.margin = Vector4.zero; text.characterSpacing = 0; text.wordSpacing = 0; text.richText = false; text.raycastTarget = false;
        var renderer = text.GetComponent<MeshRenderer>(); renderer.sortingOrder = -900; renderer.enabled = false;
        var view = placement.gameObject.AddComponent<CursorDashProjectedScoreView>(); var data = new SerializedObject(view);
        Reference(data, "placementRoot", placement); Reference(data, "animationRoot", animation); Reference(data, "artwork", text);
        if (profile != null)
        {
            data.FindProperty("combatViewport").vector2Value = new Vector2(profile.backgroundScoreX, profile.backgroundScoreY);
            data.FindProperty("mainGlyphScale").floatValue = profile.spaceMainScoreScale;
            data.FindProperty("mainTint").colorValue = new Color(1, 1, 1, profile.spaceMainScoreOpacity);
            var tint = profile.backgroundScoreTint; tint.a = profile.backgroundScoreOpacity; data.FindProperty("combatTint").colorValue = tint;
        }
        data.ApplyModifiedPropertiesWithoutUndo(); return view;
    }
}
