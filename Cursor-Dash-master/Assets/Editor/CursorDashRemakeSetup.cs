using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>One reproducible, asset-backed setup for the existing two-scene loop.</summary>
public static class CursorDashRemakeSetup
{
    private const string MainPath = "Assets/00.Scenes/01.StageSelectScene.unity";
    private const string PlayPath = "Assets/00.Scenes/Cyberpunk.unity";
    private const string ArtRoot = "Assets/02.Sprites/New_Resources/";
    private static TMP_FontAsset medium;
    private static TMP_FontAsset bold;
    private static Sprite ring;
    private static readonly Color Teal = new Color(0.08f, 0.94f, 0.86f);
    private static readonly Color Pink = new Color(1f, 0.2f, 0.7f);
    private static readonly Color Muted = new Color(0.7f, 0.76f, 0.88f);

    [MenuItem("Tools/CursorDash/Apply Minimal Loop Setup")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before setup.");
        if (SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before applying the setup; no modal dialog will be opened.");
        medium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/Paperlogy-5Medium SDF.asset");
        bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/Paperlogy-7Bold SDF.asset");
        if (medium == null || bold == null) throw new InvalidOperationException("The two installed Paperlogy SDF fonts are required.");
        SetupPlay();
        SetupMain();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainPath, true), new EditorBuildSettingsScene(PlayPath, true) };
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.WebGL.template = "PROJECT:CursorDash";
        Debug.Log("CURSORDASH_SCENE_SETUP_PASS: existing main + Cyberpunk loop, floating stick, TMP HUD, pause and result callbacks.");
    }

    private static void SetupPlay()
    {
        Scene scene = EditorSceneManager.OpenScene(PlayPath, OpenSceneMode.Single);
        StageManager stage = UnityEngine.Object.FindFirstObjectByType<StageManager>();
        GameObject previousHud = FindRoot(scene, "RemakeHUD");
        bool alreadySetUp = previousHud != null;
        SerializedObject data = new SerializedObject(stage);
        Image frame = data.FindProperty("hpBarFrame").objectReferenceValue as Image;
        ring = frame != null ? frame.sprite : null;
        if (frame != null) frame.color = Teal;
        Image hp = data.FindProperty("hpBarImage").objectReferenceValue as Image;
        if (hp != null) hp.color = Teal;
        if (previousHud != null) UnityEngine.Object.DestroyImmediate(previousHud);
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == "Pause UI" || root.name == "DeathUI" || root.name == "Hold Achivement") root.SetActive(false);
        GameObject oldHud = GameObject.Find("UI");
        if (oldHud != null)
        {
            foreach (Text label in oldHud.GetComponentsInChildren<Text>(true)) label.gameObject.SetActive(false);
            foreach (Graphic graphic in oldHud.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        }
        foreach (SpecialOperation special in UnityEngine.Object.FindObjectsByType<SpecialOperation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(special);

        PlayerCharacterController player = UnityEngine.Object.FindFirstObjectByType<PlayerCharacterController>();
        SerializedObject movement = new SerializedObject(player);
        // The original 0.1-per-physics-step speed becomes the same 5 world units/second.
        if (!alreadySetUp) movement.FindProperty("moveSpeed").floatValue = movement.FindProperty("moveSpeed").floatValue / Time.fixedDeltaTime;
        movement.FindProperty("useCharacterSettings").boolValue = false;
        movement.ApplyModifiedPropertiesWithoutUndo();
        SpriteRenderer cursor = player.GetComponent<SpriteRenderer>();
        float cursorHeight = Mathf.Max(0.2f, cursor.bounds.size.y);
        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        Vector2 collisionSize = playerCollider != null ? (Vector2)playerCollider.bounds.size : Vector2.one * 0.25f;
        cursor.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/02.Sprites/Old_Resources/Character/cursor.png");
        cursor.color = Color.white;
        if (cursor.sprite != null) player.transform.localScale = Vector3.one * (cursorHeight / cursor.sprite.bounds.size.y);
        if (playerCollider != null)
        {
            playerCollider.size = new Vector2(collisionSize.x / Mathf.Abs(player.transform.lossyScale.x), collisionSize.y / Mathf.Abs(player.transform.lossyScale.y));
            playerCollider.isTrigger = true;
        }
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null) { body.gravityScale = 0f; body.interpolation = RigidbodyInterpolation2D.Interpolate; }
        CameraFlash flash = stage.GetComponent<CameraFlash>();
        SerializedObject flashData = new SerializedObject(flash);
        Image flashImage = flashData.FindProperty("flashImage").objectReferenceValue as Image;
        if (flashImage != null) { flashImage.raycastTarget = false; flashImage.color = Color.clear; }

        RectTransform hud = CanvasRoot("RemakeHUD", 25);
        AddCircuits(hud);
        TMP_Text time = Label("Time", hud, "0.00", new Vector2(0.5f, 0.91f), new Vector2(620f, 140f), 84f, Color.white, true);
        Label("Time caption", hud, "생존 시간", new Vector2(0.5f, 0.86f), new Vector2(500f, 65f), 29f, Muted);
        Button pauseButton = ButtonAt("PauseButton", hud, "", new Vector2(0.9f, 0.95f), new Vector2(130f, 140f), Color.clear, stage.TogglePause);
        RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
        ImageAt("Pause bar left", pauseRect, null, new Vector2(0.35f, 0.5f), new Vector2(14f, 62f), Color.white);
        ImageAt("Pause bar right", pauseRect, null, new Vector2(0.65f, 0.5f), new Vector2(14f, 62f), Color.white);
        Label("Rules", hud, "원형은 직접 닿기  ·  육각 빔은 잔상으로 차단", new Vector2(0.5f, 0.045f), new Vector2(1020f, 80f), 28f, Muted);

        RectTransform pausePanel = Panel("PausePanel", hud);
        PauseHandler pause = pausePanel.gameObject.AddComponent<PauseHandler>();
        Label("Heading", pausePanel, "PAUSED", new Vector2(0.5f, 0.73f), new Vector2(960f, 150f), 94f, Teal, true);
        Label("Subtitle", pausePanel, "잠깐 멈춤", new Vector2(0.5f, 0.66f), new Vector2(800f, 90f), 36f, Muted);
        ButtonAt("Resume", pausePanel, "계속하기", new Vector2(0.5f, 0.5f), new Vector2(750f, 148f), Teal, pause.Unpause);
        ButtonAt("Retry", pausePanel, "다시 시작", new Vector2(0.5f, 0.39f), new Vector2(750f, 148f), new Color(0.13f, 0.18f, 0.3f), pause.Retry);
        ButtonAt("Home", pausePanel, "메인으로", new Vector2(0.5f, 0.28f), new Vector2(750f, 148f), new Color(0.13f, 0.18f, 0.3f), pause.ReturnHome);
        Button bgm = ButtonAt("BGM", pausePanel, "BGM ON", new Vector2(0.34f, 0.16f), new Vector2(330f, 100f), new Color(0.1f, 0.14f, 0.23f), pause.ToggleBgm);
        Button sfx = ButtonAt("SFX", pausePanel, "SFX ON", new Vector2(0.66f, 0.16f), new Vector2(330f, 100f), new Color(0.1f, 0.14f, 0.23f), pause.ToggleSfx);
        SerializedObject pauseData = new SerializedObject(pause);
        Ref(pauseData, "bgmLabel", bgm.GetComponentInChildren<TMP_Text>());
        Ref(pauseData, "sfxLabel", sfx.GetComponentInChildren<TMP_Text>());
        pauseData.ApplyModifiedPropertiesWithoutUndo();
        pausePanel.gameObject.SetActive(false);

        RectTransform resultPanel = Panel("ResultPanel", hud);
        resultPanel.gameObject.AddComponent<CanvasGroup>();
        DeathUIController result = resultPanel.gameObject.AddComponent<DeathUIController>();
        Label("Heading", resultPanel, "ROUND OVER", new Vector2(0.5f, 0.76f), new Vector2(1000f, 130f), 83f, Pink, true);
        Label("Time caption", resultPanel, "생존 시간", new Vector2(0.5f, 0.65f), new Vector2(800f, 75f), 35f, Muted);
        TMP_Text score = Label("Score", resultPanel, "0.00", new Vector2(0.5f, 0.58f), new Vector2(960f, 210f), 144f, Color.white, true);
        TMP_Text summary = Label("Summary", resultPanel, "원형 파괴 0  ·  육각 차단 0", new Vector2(0.5f, 0.46f), new Vector2(950f, 90f), 35f, Muted);
        ButtonAt("Retry", resultPanel, "다시 도전", new Vector2(0.5f, 0.33f), new Vector2(750f, 148f), Teal, stage.Retry);
        ButtonAt("Home", resultPanel, "메인으로", new Vector2(0.5f, 0.22f), new Vector2(750f, 148f), new Color(0.13f, 0.18f, 0.3f), stage.ReturnHome);
        SerializedObject resultData = new SerializedObject(result);
        Ref(resultData, "scoreLabel", score);
        Ref(resultData, "summaryLabel", summary);
        resultData.ApplyModifiedPropertiesWithoutUndo();
        resultPanel.gameObject.SetActive(false);
        Ref(data, "timeScoreLabel", time);
        Ref(data, "pauseHandler", pause);
        Ref(data, "deathUIController", result);
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupMain()
    {
        Scene scene = EditorSceneManager.OpenScene(MainPath, OpenSceneMode.Single);
        AudioClip mainMusic = null;
        GameObject oldManager = FindRoot(scene, "StageSelectManager");
        if (oldManager != null)
        {
            SerializedObject old = new SerializedObject(oldManager.GetComponent<StageSelectSceneManager>());
            mainMusic = old.FindProperty("mainSceneClip").objectReferenceValue as AudioClip;
            oldManager.SetActive(false);
        }
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.GetComponent<Canvas>() != null) root.SetActive(false);
        GameObject previous = FindRoot(scene, "RemakeMenu");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
        RectTransform menu = CanvasRoot("RemakeMenu", 20);
        MainSceneController controller = menu.gameObject.AddComponent<MainSceneController>();
        Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "Stage/Cyberpunk/Background/Cyberpunk_Background.png");
        RectTransform bg = Stretch("Background", menu);
        Image backgroundImage = bg.gameObject.AddComponent<Image>();
        backgroundImage.sprite = background;
        backgroundImage.raycastTarget = false;
        AddCircuits(menu);
        GlitchLabel("CURSOR", menu, new Vector2(0.48f, 0.69f), new Vector2(1000f, 170f), 118f, true);
        GlitchLabel("DASH", menu, new Vector2(0.64f, 0.63f), new Vector2(620f, 140f), 86f, true);
        ImageAt("Outer ring", menu, ring, new Vector2(0.5f, 0.465f), new Vector2(410f, 410f), new Color(0.4f, 0.56f, 1f, 0.25f));
        ImageAt("Inner ring", menu, ring, new Vector2(0.5f, 0.465f), new Vector2(330f, 330f), new Color(0.4f, 0.56f, 1f, 0.45f));
        TMP_Text[] best = GlitchLabel("0.00", menu, new Vector2(0.5f, 0.465f), new Vector2(900f, 180f), 144f, false);
        Label("Best caption", menu, "BEST TIME", new Vector2(0.5f, 0.39f), new Vector2(750f, 60f), 28f, Muted);
        ButtonAt("Start", menu, "클릭해서 게임 시작", new Vector2(0.5f, 0.285f), new Vector2(980f, 170f), Color.clear, controller.StartGame);
        Label("Controls", menu, "터치한 곳에서 드래그해 이동", new Vector2(0.5f, 0.215f), new Vector2(960f, 85f), 32f, Muted);
        Sprite curve = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "Settings/Ui_Down/Ui_Down_Design.png");
        ImageAt("Menu curve", menu, curve, new Vector2(0.5f, 0.065f), new Vector2(1080f, 66f), Color.white);
        RectTransform cursorBadge = Rect("Cursor badge", menu, new Vector2(0.5f, 0.065f), new Vector2(190f, 190f));
        for (int i = 0; i < 48; i++)
        {
            float angle = i * Mathf.PI * 2f / 48f;
            RectTransform edge = Rect("Ring " + i, cursorBadge, Vector2.one * 0.5f, new Vector2(2.4f, 12.4f));
            edge.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 92f;
            edge.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            Image stroke = edge.gameObject.AddComponent<Image>();
            stroke.color = Color.white;
            stroke.raycastTarget = false;
        }
        Sprite mainIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/02.Sprites/Old_Resources/Character/cursor.png");
        ImageAt("Cursor menu icon", menu, mainIcon, new Vector2(0.5f, 0.065f), new Vector2(105f, 105f), Color.white);
        Button gear = ButtonAt("Settings", menu, "", new Vector2(0.88f, 0.065f), new Vector2(130f, 130f), Color.clear, controller.OpenSettings);
        gear.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "Settings/Ui_Down/Ui_Down_Settings.png");
        gear.GetComponent<Image>().preserveAspect = true;
        gear.GetComponent<Image>().color = Color.white;

        RectTransform settings = Panel("SettingsPanel", menu);
        Label("Heading", settings, "SETTINGS", new Vector2(0.5f, 0.73f), new Vector2(1000f, 150f), 88f, Teal, true);
        Button bgm = ButtonAt("BGM", settings, "BGM ON", new Vector2(0.5f, 0.53f), new Vector2(750f, 148f), new Color(0.13f, 0.18f, 0.3f), controller.ToggleBgm);
        Button sfx = ButtonAt("SFX", settings, "SFX ON", new Vector2(0.5f, 0.42f), new Vector2(750f, 148f), new Color(0.13f, 0.18f, 0.3f), controller.ToggleSfx);
        ButtonAt("Close", settings, "돌아가기", new Vector2(0.5f, 0.28f), new Vector2(750f, 148f), Teal, controller.CloseSettings);
        SerializedObject data = new SerializedObject(controller);
        Ref(data, "map", AssetDatabase.LoadAssetAtPath<MapInformation>("Assets/Resources/Maps/Cyberpunk.asset"));
        Ref(data, "mainMusic", mainMusic);
        Ref(data, "bestLabel", best[2]);
        SerializedProperty shadows = data.FindProperty("bestShadows");
        shadows.arraySize = 2;
        shadows.GetArrayElementAtIndex(0).objectReferenceValue = best[0];
        shadows.GetArrayElementAtIndex(1).objectReferenceValue = best[1];
        Ref(data, "settingsPanel", settings.gameObject);
        Ref(data, "bgmLabel", bgm.GetComponentInChildren<TMP_Text>());
        Ref(data, "sfxLabel", sfx.GetComponentInChildren<TMP_Text>());
        data.ApplyModifiedPropertiesWithoutUndo();
        settings.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects()) if (root.name == name) return root;
        return null;
    }
    private static void Ref(SerializedObject data, string name, UnityEngine.Object value) => data.FindProperty(name).objectReferenceValue = value;
    private static RectTransform CanvasRoot(string name, int order)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        item.layer = 5;
        Canvas canvas = item.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        CanvasScaler scaler = item.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        return item.GetComponent<RectTransform>();
    }
    private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.layer = 5;
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = Vector2.one * 0.5f;
        rect.sizeDelta = size;
        return rect;
    }
    private static RectTransform Stretch(string name, Transform parent)
    {
        RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.zero);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }
    private static RectTransform Panel(string name, Transform parent)
    {
        RectTransform rect = Stretch(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.025f, 0.035f, 0.08f, 0.96f);
        return rect;
    }
    private static TMP_Text Label(string name, Transform parent, string text, Vector2 anchor, Vector2 size, float fontSize, Color color, bool strong = false)
    {
        RectTransform rect = Rect(name, parent, anchor, size);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = strong ? bold : medium;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        return label;
    }
    private static TMP_Text[] GlitchLabel(string text, Transform parent, Vector2 anchor, Vector2 size, float fontSize, bool italic)
    {
        TMP_Text cyan = Label(text + " cyan", parent, text, anchor, size, fontSize, Teal, true);
        TMP_Text pink = Label(text + " pink", parent, text, anchor, size, fontSize, Pink, true);
        TMP_Text white = Label(text + " white", parent, text, anchor, size, fontSize, Color.white, true);
        cyan.rectTransform.anchoredPosition = new Vector2(-6f, -2f);
        pink.rectTransform.anchoredPosition = new Vector2(6f, 2f);
        if (italic) cyan.fontStyle = pink.fontStyle = white.fontStyle = FontStyles.Italic;
        return new[] { cyan, pink, white };
    }
    private static Image ImageAt(string name, Transform parent, Sprite sprite, Vector2 anchor, Vector2 size, Color color)
    {
        RectTransform rect = Rect(name, parent, anchor, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = sprite != null;
        image.raycastTarget = false;
        return image;
    }
    private static Button ButtonAt(string name, Transform parent, string text, Vector2 anchor, Vector2 size, Color color, UnityAction action)
    {
        Image image = ImageAt(name, parent, null, anchor, size, color);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.82f, 0.96f, 1f);
        colors.pressedColor = new Color(0.5f, 0.7f, 0.8f);
        button.colors = colors;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        if (!string.IsNullOrEmpty(text))
            Label("Label", image.transform, text, Vector2.one * 0.5f, size - new Vector2(30f, 10f), 48f,
                color == Teal ? new Color(0.02f, 0.1f, 0.13f) : Color.white, true);
        return button;
    }
    private static void AddCircuits(Transform parent)
    {
        Color wire = new Color(0.38f, 0.48f, 0.77f, 0.2f);
        for (int mirror = 0; mirror < 4; mirror++)
        {
            bool right = (mirror & 1) != 0, bottom = (mirror & 2) != 0;
            for (int i = 0; i < 5; i++)
            {
                float x = 52f + i * 67f, depth = 170f + i * 68f;
                Vector2[] points = { new Vector2(x, 1920f), new Vector2(x, 1920f - depth),
                    new Vector2(x + 38f, 1882f - depth), new Vector2(x + 38f, 1650f - depth) };
                for (int j = 0; j < points.Length; j++)
                {
                    if (right) points[j].x = 1080f - points[j].x;
                    if (bottom) points[j].y = 1920f - points[j].y;
                }
                for (int j = 0; j < points.Length - 1; j++)
                {
                    Vector2 delta = points[j + 1] - points[j];
                    Image line = ImageAt("Circuit", parent, null, Vector2.zero, new Vector2(delta.magnitude, 4f), wire);
                    line.rectTransform.anchoredPosition = (points[j + 1] + points[j]) * 0.5f;
                    line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                }
            }
        }
    }
}
