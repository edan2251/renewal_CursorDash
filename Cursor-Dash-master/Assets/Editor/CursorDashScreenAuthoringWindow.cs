using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Static representative composition from real view instances. No game managers or saved sample data.</summary>
public sealed partial class CursorDashScreenAuthoringWindow : EditorWindow
{
    public enum Screen { Main, MainSettings, InGame, InGamePause, Returning, Tutorial, FeverNotice }
    [SerializeField] private StageManager source;
    [SerializeField] private Screen screen;
    [SerializeField] private bool landscape;
    [SerializeField] private string sampleScore = "0";
    [SerializeField, Range(0, 1)] private float sampleHealth = 1;
    [SerializeField, Range(0, 1)] private float sampleCharge;
    [SerializeField, Range(0, 1)] private float sampleNodeCharge = .65f;
    [SerializeField, Range(0, 1)] private float sampleHexMorph;
    [SerializeField] private bool animateBackground=true;
    [SerializeField] private float backgroundSeconds;
    private double lastAtmosphereUpdate;
    private static bool creatingSceneArtwork;
    private bool borrowsScene;
    [SerializeField] private bool editInScene=true;
    [SerializeField] private bool showComparison;
    [SerializeField] private bool showAdvanced;
    [SerializeField] private bool sampleNodes;
    [SerializeField] private bool useSavedBest=true;
    private Scene previewScene;
    private Camera previewCamera;
    private RenderTexture target;
    private readonly List<GameObject> clones = new List<GameObject>();
    private Mesh backgroundMesh;
    private Material backgroundMaterial;
    private CursorDashViewFonts fonts;
    private bool refresh = true;
    private double rebuildAfter;
    public bool HasPreview => target != null && previewScene.IsValid();
    public Texture PreviewTexture => target;

    [MenuItem("Tools/CursorDash/화면 편집")]
    public static CursorDashScreenAuthoringWindow Open()
    {
        var window=GetWindow<CursorDashScreenAuthoringWindow>("CursorDash 화면 편집");window.refresh=true;window.rebuildAfter=0;return window;
    }

    private void OnEnable()
    {
        if(creatingSceneArtwork)return;
        AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
        EditorApplication.playModeStateChanged += OnPlayState;
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
        EditorApplication.projectChanged += Invalidate;
        Undo.undoRedoPerformed += Invalidate;
        Undo.postprocessModifications += OnModification;
        source = UnityEngine.Object.FindFirstObjectByType<StageManager>();
        refresh = true;
    }
    private void OnDisable()
    {
        Cleanup(); if(!borrowsScene)CursorDashUIAuthoringPreview.Stop(); AssemblyReloadEvents.beforeAssemblyReload -= Cleanup;
        EditorApplication.playModeStateChanged -= OnPlayState; EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
        EditorApplication.projectChanged -= Invalidate; Undo.undoRedoPerformed -= Invalidate; Undo.postprocessModifications -= OnModification;
    }
    private void OnPlayState(PlayModeStateChange state) { Cleanup(); refresh = state == PlayModeStateChange.EnteredEditMode; }
    private void OnSceneChanged(Scene before, Scene after) { Cleanup(); source = UnityEngine.Object.FindFirstObjectByType<StageManager>(); refresh = true; }
    private UndoPropertyModification[] OnModification(UndoPropertyModification[] values) { Invalidate(); return values; }
    private void Invalidate() { refresh = true; rebuildAfter = EditorApplication.timeSinceStartup + .3; Repaint(); }
    private void OnInspectorUpdate()
    {
        double now=EditorApplication.timeSinceStartup;
        if (refresh && !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && now>=rebuildAfter) { Rebuild();if(editInScene&&Ready())ApplyToScene(false);Repaint(); }
        if(HasPreview&&animateBackground&&!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if(lastAtmosphereUpdate>0)backgroundSeconds+=(float)Math.Min(.25,now-lastAtmosphereUpdate);
            UpdateBackground();previewCamera.Render();Repaint();
            CursorDashUIAuthoringPreview.TickScreen(backgroundSeconds);
        }
        lastAtmosphereUpdate=now;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("화면 선택 → Scene에서 직접 편집", EditorStyles.boldLabel);
        DrawScreenButtons();
        if(EditorApplication.isPlayingOrWillChangePlaymode){EditorGUILayout.HelpBox("Play 중에는 편집 표시를 종료합니다.",MessageType.Info);return;}
        if(!Ready()){EditorGUILayout.HelpBox("저장된 Cyberpunk 씬에서 사용하세요. Prefab Mode는 기존 Inspector 표시 버튼을 사용합니다.",MessageType.Info);return;}
        DrawSimpleSourceButtons();
        if(GUILayout.Button("편집 표시 종료")){editInScene=false;CursorDashUIAuthoringPreview.Stop();Cleanup();refresh=false;}
        showAdvanced=EditorGUILayout.Foldout(showAdvanced,"샘플·화면비·표시 시간",true);
        if(showAdvanced)DrawAdvancedControls();
        showComparison=EditorGUILayout.Foldout(showComparison,"조합 렌더 비교 (보조 화면)",true);
        if(showComparison&&target!=null){var area=GUILayoutUtility.GetRect(80,10000,120,600,GUILayout.ExpandWidth(true));EditorGUI.DrawPreviewTexture(area,target,null,ScaleMode.ScaleToFit);}
        EditorGUILayout.HelpBox("버튼을 누르면 실제 Scene의 UI를 함께 표시합니다. Scene에서 요소를 선택해 수정하세요. 저장/Play 전에는 표시 상태만 복원하며 편집한 배치·외형은 유지합니다. 전투·파괴·후처리 애니메이션은 Play 검수 대상입니다.",MessageType.None);
    }
    private void DrawAdvancedControls()
    {
        using (var change = new EditorGUI.ChangeCheckScope())
        {
            source = (StageManager)EditorGUILayout.ObjectField("원본 Scene의 StageManager", source, typeof(StageManager), true);
            landscape = EditorGUILayout.Toggle("1920×1080 (끄면1080×1920)", landscape);
            sampleScore = EditorGUILayout.TextField("샘플 점수 · 저장값 아님", sampleScore);
            useSavedBest=EditorGUILayout.Toggle("메인은 저장된 BEST 표시 (읽기만)",useSavedBest);
            sampleHealth = EditorGUILayout.Slider("샘플 체력", sampleHealth, 0, 1);
            sampleCharge = EditorGUILayout.Slider("샘플 충전량", sampleCharge, 0, 1);
            sampleNodeCharge = EditorGUILayout.Slider("노드 충전 대표 상태", sampleNodeCharge, 0, 1);
            sampleHexMorph = EditorGUILayout.Slider("육각 형태 대표 상태", sampleHexMorph, 0, 1);
            sampleNodes=EditorGUILayout.Toggle("원형·육각 샘플 표시",sampleNodes);
            animateBackground=EditorGUILayout.Toggle("실제 배경 흐름 미리보기",animateBackground);
            backgroundSeconds=Mathf.Max(0,EditorGUILayout.FloatField("배경 표시 시간 (초)",backgroundSeconds));
            if (change.changed) { editInScene=true;refresh = true; rebuildAfter = 0; }
        }
    }
    private void DrawScreenButtons()
    {
        using(new EditorGUILayout.HorizontalScope()){ScreenButton("메인",Screen.Main);ScreenButton("메인 옵션",Screen.MainSettings);ScreenButton("인게임",Screen.InGame);}
        using(new EditorGUILayout.HorizontalScope()){ScreenButton("일시정지",Screen.InGamePause);ScreenButton("게임 종료 · 점수 대기",Screen.Returning);}
        using(new EditorGUILayout.HorizontalScope()){ScreenButton("튜토리얼",Screen.Tutorial);ScreenButton("첫 피버 안내",Screen.FeverNotice);}
    }
    private void ScreenButton(string label,Screen value)
    {
        var old=GUI.backgroundColor;if(editInScene&&screen==value)GUI.backgroundColor=new Color(.35f,.85f,.8f);
        if(GUILayout.Button(label,GUILayout.Height(32))&&Ready()){screen=value;editInScene=true;Rebuild();ApplyToScene(true);}
        GUI.backgroundColor=old;
    }
    private void DrawSimpleSourceButtons()
    {
        using(new EditorGUILayout.HorizontalScope()){
            if(GUILayout.Button("점수"))Select(source.ProjectedScoreView.gameObject);
            if(screen==Screen.Main||screen==Screen.MainSettings){if(GUILayout.Button("로고"))Select(source.MainLogoView.PlacementRoot.gameObject);if(GUILayout.Button("시작 문구"))Select(source.MainMenuView.Prompt.gameObject);if(GUILayout.Button("BEST 문구"))Select(source.MainMenuView.Caption.gameObject);}
            else{if(GUILayout.Button("넥서스"))Select(source.NexusView.gameObject);if(screen==Screen.Tutorial||screen==Screen.FeverNotice)if(GUILayout.Button("안내 문구"))Select((screen==Screen.Tutorial?source.GuidanceView.Hint:source.GuidanceView.Notice).gameObject);}
            if(screen==Screen.MainSettings||screen==Screen.InGamePause)if(GUILayout.Button("설정/Pause 배치")){var data=new SerializedObject(source.SettingsView);Select(((RectTransform)data.FindProperty(screen==Screen.InGamePause?"pausePlacement":"contentPlacement").objectReferenceValue).gameObject);}
            if(GUILayout.Button("배경·회로 설정"))Selection.activeObject=source.BoardView!=null?(UnityEngine.Object)source.BoardView:Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");
        }
    }
    private void ApplyToScene(bool frame)
    {
        string digits=useSavedBest&&(screen==Screen.Main||screen==Screen.MainSettings)?CursorDashPointsRecord.Read("Cyberpunk").ToString():sampleScore;
        CursorDashUIAuthoringPreview.ShowScreen(source,screen,landscape?1920:1080,landscape?1080:1920,digits,sampleHealth,sampleCharge,sampleNodeCharge,sampleHexMorph,backgroundSeconds,sampleNodes,frame);
    }

    private bool Ready() => PrefabStageUtility.GetCurrentPrefabStage()==null && source != null && source.gameObject.scene.path == "Assets/Scenes/Cyberpunk.unity" && source.MainLogoView != null && source.MainMenuView != null && source.MainMenuView.IsConfigured && source.SettingsView != null && source.SettingsView.IsConfigured && source.ProjectedScoreView != null && source.ProjectedScoreView.IsConfigured;
    private static void Select(GameObject item) { Selection.activeGameObject = item; EditorGUIUtility.PingObject(item); }

    // Also used by focused EditMode tests: never starts Play, a round, or a settings callback.
    public void Show(StageManager stage, Screen selected, bool wide, string digits)
    {
        editInScene=false;CursorDashUIAuthoringPreview.Stop();source = stage; screen = selected; landscape = wide; sampleScore = digits; Rebuild();
    }
    // Exactly the state-button path, exposed for focused editor/save validation.
    public void SelectScreen(StageManager stage,Screen selected,bool wide,string digits,bool frame=false)
    {
        source=stage;screen=selected;landscape=wide;sampleScore=digits;editInScene=true;Rebuild();if(Ready())CursorDashUIAuthoringPreview.ShowScreen(source,screen,wide?1920:1080,wide?1080:1920,digits,sampleHealth,sampleCharge,sampleNodeCharge,sampleHexMorph,backgroundSeconds,sampleNodes,frame);
    }
    public void Rebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !Ready()) { Cleanup(); refresh=false; return; }
        if(CursorDashUIAuthoringPreview.ScreenActive)CursorDashUIAuthoringPreview.Stop();
        int width = landscape ? 1920 : 1080, height = landscape ? 1080 : 1920;
        if (target == null || target.width != width || target.height != height) { Cleanup(); CreatePreview(width, height); }
        DestroyComposition();
        try
        {
            previewCamera.backgroundColor=screen==Screen.FeverNotice?Color.white:Color.black;
            if(screen!=Screen.FeverNotice)AddBackground();
            bool main = screen == Screen.Main || screen == Screen.MainSettings;
            bool returning = screen == Screen.Returning;
            var logo = Clone(source.MainLogoView); logo.SetVisible(main); logo.Present(0, 1);
            var menu = Clone(source.MainMenuView); BindFonts(menu.gameObject); menu.SetVisible(true); menu.PresentPrompt(main ? .70f : 0);
            var score = Clone(source.ProjectedScoreView); BindFonts(score.gameObject);
            string digits = string.IsNullOrWhiteSpace(sampleScore) ? "0" : sampleScore;
            foreach (char value in digits) if (value < '0' || value > '9') throw new InvalidOperationException("샘플 점수에는 숫자만 입력하세요.");
            var text = score.Artwork; float glyphScale = 1;
            text.OnPreRenderText += data => CursorDashScoreGeometry.Center(data, text.rectTransform.rect, glyphScale);
            text.text = digits; if(text.enableAutoSizing)text.fontSize = text.fontSizeMax;
            score.AnimationRoot.position = score.WorldPoint(previewCamera, main || returning ? score.MainViewport : score.CombatViewport);
            text.color = main || returning ? score.MainTint : score.CombatTint; text.GetComponent<MeshRenderer>().enabled = screen!=Screen.Tutorial&&screen!=Screen.FeverNotice; text.ForceMeshUpdate();
            float available = 2 * previewCamera.orthographicSize * previewCamera.aspect * .84f;
            glyphScale = (main || returning ? score.MainGlyphScale : 1) * Mathf.Min(1, available / Mathf.Max(.001f, text.textBounds.size.x)); text.ForceMeshUpdate();
            if(screen!=Screen.FeverNotice)AddCircuitComposition(text);
            if (!main && !returning && screen!=Screen.FeverNotice) AddBoardComposition();
            if (!main && !returning && screen!=Screen.FeverNotice) AddNodeAndCursorSamples();
            AddGuidance();
            var settings = Clone(source.SettingsView); BindFonts(settings.gameObject); settings.Canvas.enabled = true;
            foreach (var row in settings.Rows)
            {
                float value = row.key == "user.vfxReduction" ? 0 : 1; row.slider.SetValueWithoutNotify(value); row.value.text = Mathf.RoundToInt(value * 100) + "%";
            }
            settings.Layout(new Rect(0, 0, width, height), new Vector2(width, height));
            settings.Present(main, screen == Screen.MainSettings ? 1 : 0, screen == Screen.InGamePause ? 1 : 0, screen == Screen.InGamePause);
            // Keep the live enabled appearance; absence of a raycaster/EventSystem blocks preview input.
            settings.ContentOpacity.interactable = true; settings.ContentOpacity.blocksRaycasts = false;
            foreach (var raycaster in settings.GetComponentsInChildren<GraphicRaycaster>(true)) raycaster.enabled = false;
            Canvas.ForceUpdateCanvases();
            var bounds = text.GetComponent<MeshRenderer>().bounds; Vector2 point = previewCamera.WorldToScreenPoint(bounds.max); point.x = width * .5f; point.y += menu.CaptionScoreGap;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)menu.Canvas.transform, point, previewCamera, out Vector2 local);
            menu.PresentCaption(local, returning, true, false, 0, main || returning ? .75f : 0);
            previewCamera.Render(); refresh = false;
        }
        catch { Cleanup(); refresh = false; throw; }
    }

    private void CreatePreview(int width, int height)
    {
        previewScene = EditorSceneManager.NewPreviewScene();
        var cameraObject = new GameObject("CursorDash authoring preview camera") { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(cameraObject, previewScene); previewCamera = cameraObject.AddComponent<Camera>();
        var board = source.gameObject.scene.GetRootGameObjects(); Camera original = null;
        foreach (var root in board) { original = root.GetComponent<Camera>(); if (original != null) break; }
        if (original == null) throw new InvalidOperationException("Original board camera missing.");
        previewCamera.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);
        previewCamera.orthographic = original.orthographic; previewCamera.orthographicSize = original.orthographicSize;
        previewCamera.clearFlags = CameraClearFlags.SolidColor; previewCamera.backgroundColor = Color.black;
        previewCamera.nearClipPlane = original.nearClipPlane; previewCamera.farClipPlane = original.farClipPlane;
        previewCamera.scene = previewScene; previewCamera.enabled = false;
        target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { hideFlags = HideFlags.HideAndDontSave, name = "CursorDash authoring composite" };
        target.Create(); previewCamera.targetTexture = target; previewCamera.aspect = (float)width / height;
        var lightObject = new GameObject("Preview unlit reference light") { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(lightObject, previewScene); var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 0;
        fonts = new CursorDashViewFonts();
    }

    private T Clone<T>(T sourceView) where T : Component
    {
        var copy = UnityEngine.Object.Instantiate(sourceView.gameObject);
        copy.name = "PREVIEW ONLY - " + sourceView.name; copy.hideFlags = HideFlags.HideAndDontSave;
        SceneManager.MoveGameObjectToScene(copy, previewScene); clones.Add(copy);
        foreach (var canvas in copy.GetComponentsInChildren<Canvas>(true))
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = previewCamera; canvas.planeDistance = 1;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // Native ScaleWithScreenSize equation, evaluated for this private target rather than the live Game View.
                float scale = Mathf.Pow(2, Mathf.Lerp(Mathf.Log(target.width / scaler.referenceResolution.x, 2), Mathf.Log(target.height / scaler.referenceResolution.y, 2), scaler.matchWidthOrHeight));
                scaler.enabled = false; canvas.scaleFactor = scale;
            }
        }
        return copy.GetComponent<T>();
    }
    private void BindFonts(GameObject root) { foreach (var text in root.GetComponentsInChildren<TMP_Text>(true)) fonts.Bind(text); }

    private void AddBackground()
    {
        var shader = Resources.Load<Shader>("CursorDash/LivingBackground");
        if (shader == null || !shader.isSupported) throw new InvalidOperationException("Existing living background shader unavailable.");
        var profile = Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");
        backgroundMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        CursorDashLivingBackground.ApplyFrame(backgroundMaterial,profile,backgroundSeconds,0,Vector2.zero);
        backgroundMesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
        backgroundMesh.vertices = new[] { new Vector3(-1, -1), new Vector3(-1, 1), new Vector3(1, 1), new Vector3(1, -1) }; backgroundMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        var root = new GameObject("PREVIEW ONLY - Living color field") { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(root, previewScene); clones.Add(root);
        root.AddComponent<MeshFilter>().sharedMesh = backgroundMesh; var view = root.AddComponent<MeshRenderer>(); view.sharedMaterial = backgroundMaterial; view.sortingOrder = -1000;
        CursorDashLivingBackground.PlaceField(root.transform,previewCamera);
    }
    private void UpdateBackground()
    {
        if(backgroundMaterial!=null)CursorDashLivingBackground.ApplyFrame(backgroundMaterial,Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype"),backgroundSeconds,0,Vector2.zero);
    }

    public void Capture(string path)
    {
        if (!HasPreview) Rebuild(); if (target == null) throw new InvalidOperationException("No preview.");
        var previous = RenderTexture.active; var texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
        try { RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply(); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, texture.EncodeToPNG()); }
        finally { RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(texture); }
    }
    private void DestroyComposition()
    {
        foreach (var clone in clones) if (clone != null) UnityEngine.Object.DestroyImmediate(clone); clones.Clear();
        ReleaseBoardResources();
        if (backgroundMesh != null) UnityEngine.Object.DestroyImmediate(backgroundMesh); backgroundMesh = null;
        if (backgroundMaterial != null) UnityEngine.Object.DestroyImmediate(backgroundMaterial); backgroundMaterial = null;
    }
    public void Cleanup()
    {
        DestroyComposition(); fonts?.Dispose(); fonts = null;
        if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); target = null; }
        if (!borrowsScene&&previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
        previewScene = default(Scene); previewCamera = null;
    }
}
