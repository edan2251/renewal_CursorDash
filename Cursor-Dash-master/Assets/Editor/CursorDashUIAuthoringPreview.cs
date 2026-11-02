using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Shows the real editable UI, changing only fields owned by its runtime presenter.
/// Never snapshots a whole component: editing a placement, font, image or wording must survive Stop/save.</summary>
[InitializeOnLoad]
public static partial class CursorDashUIAuthoringPreview
{
    private static readonly List<Action> restore = new List<Action>();
    private static readonly List<Object> owned = new List<Object>();
    private static Action resume;
    private static Component target;
    private static int sessionVersion;
    private static bool resumeQueued;
    public static bool IsActive => target != null;

    static CursorDashUIAuthoringPreview()
    {
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        EditorApplication.playModeStateChanged += state => { if(state==PlayModeStateChange.ExitingEditMode)Stop(); };
        PrefabStage.prefabStageOpened += Opened;
        PrefabStage.prefabStageClosing += stage => Stop();
        PrefabStage.prefabSaving += root => SuspendForSave();
        PrefabStage.prefabSaved += root => ResumeAfterSave();
        EditorSceneManager.activeSceneChangedInEditMode += (a,b) => Stop();
        EditorSceneManager.sceneSaving += (scene,path) => {if(target!=null&&target.gameObject.scene==scene)SuspendForSave();};
        EditorSceneManager.sceneSaved += scene => {if(target!=null&&target.gameObject.scene==scene)ResumeAfterSave();};
    }

    private static void Opened(PrefabStage stage)
    {
        var root=stage.prefabContentsRoot;
        var settings=root.GetComponent<CursorDashSettingsView>();
        var guidance=root.GetComponent<CursorDashGuidanceView>();
        var menu=root.GetComponent<CursorDashMainMenuView>();
        var score=root.GetComponent<CursorDashProjectedScoreView>();
        if(settings!=null)Show(settings,false);
        else if(guidance!=null)Show(guidance,false);
        else if(menu!=null)Show(menu);
        else if(score!=null)Show(score,true);
    }
    private static bool Begin(Component view,Action repeat)
    {
        Stop();
        if(view==null||EditorUtility.IsPersistent(view)||EditorApplication.isPlayingOrWillChangePlaymode)return false;
        target=view;resume=repeat;return true;
    }
    private static void Keep<T>(Object owner,Func<T> get,Action<T> set)
    {
        T saved=get();restore.Add(()=>{if(owner!=null)set(saved);});
    }
    private static T Ref<T>(Object view,string name) where T:Object => (T)new SerializedObject(view).FindProperty(name).objectReferenceValue;
    private static void Active(GameObject obj) => Keep(obj,()=>obj.activeSelf,v=>obj.SetActive(v));
    private static void Rect(RectTransform r)
    {
        Keep(r,()=>r.anchorMin,v=>r.anchorMin=v);Keep(r,()=>r.anchorMax,v=>r.anchorMax=v);
        Keep(r,()=>r.pivot,v=>r.pivot=v);
        Keep(r,()=>r.sizeDelta,v=>r.sizeDelta=v);Keep(r,()=>r.anchoredPosition3D,v=>r.anchoredPosition3D=v);
        Keep(r,()=>r.localScale,v=>r.localScale=v);
    }
    private static void Group(CanvasGroup g)
    {
        Active(g.gameObject);Keep(g,()=>g.alpha,v=>g.alpha=v);
        Keep(g,()=>g.interactable,v=>g.interactable=v);Keep(g,()=>g.blocksRaycasts,v=>g.blocksRaycasts=v);
    }
    private static void CanvasFrame(Canvas canvas)
    {
        var rect=(RectTransform)canvas.transform;Rect(rect);
        Keep(canvas,()=>canvas.enabled,v=>canvas.enabled=v);Keep(canvas,()=>canvas.renderMode,v=>canvas.renderMode=v);
        var scaler=canvas.GetComponent<CanvasScaler>();
        if(scaler!=null){Keep(scaler,()=>scaler.enabled,v=>scaler.enabled=v);scaler.enabled=false;}
        // A Prefab Stage has no game display to drive an Overlay root. Use the same
        // pixel layout as a flat Scene-view canvas; this mode is restored before save/Play.
        canvas.renderMode=RenderMode.WorldSpace;canvas.enabled=true;
        rect.localScale=Vector3.one;rect.pivot=Vector2.one*.5f;
        rect.sizeDelta=scaler==null?new Vector2(1080,1920):scaler.referenceResolution;
        Canvas.ForceUpdateCanvases();
    }
    private static void HideOtherCanvases(Component view)
    {
        var scene=view.gameObject.scene;
        StageManager stage=null;
        foreach(var root in scene.GetRootGameObjects()){stage=root.GetComponentInChildren<StageManager>(true);if(stage!=null)break;}
        if(stage==null)return; // A standalone prefab has no game bootstrap or sibling UI.
        var candidates=new[]{stage.SettingsView?.Canvas,stage.MainMenuView?.Canvas,stage.MainLogoView==null?null:stage.MainLogoView.GetComponent<Canvas>(),stage.NexusBodySource==null?null:stage.NexusBodySource.canvas,stage.GuidanceView==null?null:stage.GuidanceView.Hint.GetComponentInParent<Canvas>()};
        var seen=new HashSet<Canvas>();
        foreach(var canvas in candidates)
        {
            if(canvas==null||!seen.Add(canvas)||view.transform.IsChildOf(canvas.transform)||canvas.transform.IsChildOf(view.transform))continue;
            Keep(canvas,()=>canvas.enabled,v=>canvas.enabled=v);canvas.enabled=false;
        }
    }
    public static void Show(CursorDashSettingsView view,bool pause)
    {
        ShowSettings(view,pause,CursorDashSettingsView.MenuPanel.Settings);
    }
    public static void ShowMenu(CursorDashSettingsView view,CursorDashSettingsView.MenuPanel panel)
    {
        ShowSettings(view,false,panel);
    }
    private static void KeepMenuPresentation(CursorDashSettingsView view)
    {
        Keep(view,()=>view.SelectedPanel,value=>view.SelectPanel(value));
        if(view.NavigationPlacement!=null)Active(view.NavigationPlacement.gameObject);
        foreach(var rect in view.MenuAnimationTransforms)if(rect!=null)Rect(rect);
        foreach(var group in view.MenuOpacityGroups)if(group!=null)Group(group);
        foreach(var graphic in view.MenuDirectionGraphics)if(graphic!=null)Keep(graphic,()=>graphic.enabled,value=>graphic.enabled=value);
    }
    private static void ShowSettings(CursorDashSettingsView view,bool pause,CursorDashSettingsView.MenuPanel panel)
    {
        if(!Begin(view,()=>ShowSettings(view,pause,panel)))return;
        HideOtherCanvases(view);
        CanvasFrame(view.Canvas);Rect(view.SafeArea);Rect(view.SheetAnimation);Active(view.SheetAnimation.gameObject);
        Rect(view.ContentAnimation);Rect(view.PauseAnimation);Group(view.ContentOpacity);Group(view.PauseOpacity);
        KeepMenuPresentation(view);
        var scaler=view.Canvas.GetComponent<CanvasScaler>();var size=scaler==null?new Vector2(540,960):scaler.referenceResolution;
        view.Layout(new UnityEngine.Rect(Vector2.zero,size),size);
        // Only presenter-owned values. The placement roots and label styles are not overwritten.
        // Show finished endpoint geometry, without Present's mutation of the authored
        // shade Image.color. Editing an image tint must never be reverted by Stop.
        view.SelectPanel(panel);view.PresentMenu(!pause,1,false);
        view.PauseOpacity.gameObject.SetActive(pause);view.PauseOpacity.alpha=1;view.PauseOpacity.blocksRaycasts=false;
        view.PauseAnimation.anchoredPosition=Vector2.zero;
        view.ContentOpacity.interactable=true;view.PauseOpacity.interactable=true;
        foreach(var row in view.Rows)
        {
            var slider=row.slider;Keep(slider,()=>slider.value,v=>slider.SetValueWithoutNotify(v));
            slider.SetValueWithoutNotify(row.key=="user.vfxReduction"?0:.75f);
            var label=row.value;Keep(label,()=>label.text,v=>label.text=v);
            label.text=Mathf.RoundToInt(row.slider.value*100)+"%";
        }
        Frame(pause?Ref<RectTransform>(view,"pausePlacement"):panel==CursorDashSettingsView.MenuPanel.Ranking?view.RankingPlacement:Ref<RectTransform>(view,"contentPlacement"));
    }
    public static void Show(CursorDashMainMenuView view)
    {
        if(!Begin(view,()=>Show(view)))return;
        HideOtherCanvases(view);
        CanvasFrame(view.Canvas);Frame(view.PromptPlacement);
    }
    public static void Show(CursorDashMainLogoView view)
    {
        if(!Begin(view,()=>Show(view)))return;
        HideOtherCanvases(view);CanvasFrame(view.GetComponent<Canvas>());Frame(view.PlacementRoot);
    }
    public static void Show(CursorDashProjectedScoreView view,bool main)
    {
        if(!Begin(view,()=>Show(view,main)))return;
        HideOtherCanvases(view);
        var renderer=view.Artwork.GetComponent<MeshRenderer>();Keep(renderer,()=>renderer.enabled,v=>renderer.enabled=v);renderer.enabled=true;
        Keep(view.Artwork,()=>view.Artwork.color,v=>view.Artwork.color=v);view.Artwork.color=main?view.MainTint:view.CombatTint;
        Frame(view.PlacementRoot);
    }
    public static void Show(CursorDashGuidanceView view,bool notice)
    {
        if(!Begin(view,()=>Show(view,notice)))return;
        HideOtherCanvases(view);
        var canvas=view.GetComponentInParent<Canvas>();
        if(canvas==null)
        {
            Rect((RectTransform)view.transform);
            canvas=view.gameObject.AddComponent<Canvas>();canvas.hideFlags=HideFlags.HideAndDontSave;owned.Add(canvas);
        }
        CanvasFrame(canvas);
        var hint=Ref<CanvasGroup>(view,"hintOpacity");var message=Ref<CanvasGroup>(view,"noticeOpacity");Group(hint);Group(message);
        Keep(view.Hint,()=>view.Hint.enabled,v=>view.Hint.enabled=v);Keep(view.Notice,()=>view.Notice.enabled,v=>view.Notice.enabled=v);
        Active(view.HintRoot);Active(view.NoticeRoot);
        view.HintRoot.SetActive(!notice);view.NoticeRoot.SetActive(notice);
        view.Hint.enabled=!notice;view.Notice.enabled=notice;hint.alpha=notice?0:1;message.alpha=notice?1:0;
        // Editor-only contrast card. The live notice background remains unchanged/transparent.
        if(notice)
        {
            var card=new GameObject("Authoring only white contrast",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            card.hideFlags=HideFlags.HideAndDontSave;owned.Add(card);card.transform.SetParent(view.NoticeRoot.transform,false);card.transform.SetAsFirstSibling();
            var rect=(RectTransform)card.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.sizeDelta=Vector2.zero;
            var image=card.GetComponent<Image>();image.color=Color.white;image.raycastTarget=false;
        }
        Frame(notice?view.Notice.rectTransform:view.Hint.rectTransform);
    }
    private static void Frame(RectTransform rect)
    {
        Canvas.ForceUpdateCanvases();SceneView.RepaintAll();
        if(SceneView.lastActiveSceneView==null)return;
        var corners=new Vector3[4];rect.GetWorldCorners(corners);var bounds=new Bounds(corners[0],Vector3.zero);
        foreach(var corner in corners)bounds.Encapsulate(corner);bounds.Expand(Mathf.Max(1,bounds.size.y*.12f));
        SceneView.lastActiveSceneView.in2DMode=true;SceneView.lastActiveSceneView.Frame(bounds,true);
    }
    private static void RestoreOnly()
    {
        // Remove temporary components first, before restoring their driven RectTransform.
        foreach(var item in owned)if(item!=null)Object.DestroyImmediate(item);owned.Clear();
        for(int i=restore.Count-1;i>=0;i--)restore[i]();restore.Clear();SceneView.RepaintAll();
    }
    public static void Stop(){sessionVersion++;resume=null;target=null;RestoreOnly();}
    public static void SuspendForSave(){RestoreOnly();}
    public static void ResumeAfterSave()
    {
        if(resume==null||resumeQueued)return;
        int version=sessionVersion;resumeQueued=true;
        EditorApplication.CallbackFunction run=null;run=()=>{EditorApplication.update-=run;resumeQueued=false;var apply=resume;if(version==sessionVersion&&target!=null&&!EditorApplication.isPlayingOrWillChangePlaymode)apply?.Invoke();};EditorApplication.update+=run;
    }
}

internal sealed class CursorDashAuthoringSaveGuard:AssetModificationProcessor
{
    private static string[] OnWillSaveAssets(string[] paths)
    {
        if(CursorDashUIAuthoringPreview.IsActive){CursorDashUIAuthoringPreview.SuspendForSave();CursorDashUIAuthoringPreview.ResumeAfterSave();}
        return paths;
    }
}

[CustomEditor(typeof(CursorDashSettingsView))]
internal sealed class CursorDashSettingsViewEditor:Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("이 컴포넌트는 표시/컨트롤 연결입니다. 배치는 Settings placement / Pause placement, 글꼴·색·이미지는 그 아래 TMP/Image에서 편집합니다. 표시 버튼은 샘플75%를 쓰며 저장값·게임 상태를 실행하지 않습니다.",MessageType.Info);
        using(new EditorGUILayout.HorizontalScope()){
            if(GUILayout.Button("랭킹 UI 표시"))CursorDashUIAuthoringPreview.ShowMenu((CursorDashSettingsView)target,CursorDashSettingsView.MenuPanel.Ranking);
            if(GUILayout.Button("설정 UI 표시"))CursorDashUIAuthoringPreview.Show((CursorDashSettingsView)target,false);
            if(GUILayout.Button("일시정지 UI 표시"))CursorDashUIAuthoringPreview.Show((CursorDashSettingsView)target,true);
            if(GUILayout.Button("표시 종료"))CursorDashUIAuthoringPreview.Stop();
        }
        DrawDefaultInspector();
    }
}
[CustomEditor(typeof(CursorDashGuidanceView))]
internal sealed class CursorDashGuidanceViewEditor:Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("체크리스트 문구는 아래 View의 템플릿, 첫 피버 문구는 First fever wording의 TMP입니다. 글꼴·색·배치는 각 TMP/RectTransform에서 편집합니다. 표시용 Canvas/흰 대비판은 저장 전에 제거됩니다.",MessageType.Info);
        using(new EditorGUILayout.HorizontalScope()){
            if(GUILayout.Button("체크리스트 표시"))CursorDashUIAuthoringPreview.Show((CursorDashGuidanceView)target,false);
            if(GUILayout.Button("첫 피버 안내 표시"))CursorDashUIAuthoringPreview.Show((CursorDashGuidanceView)target,true);
            if(GUILayout.Button("표시 종료"))CursorDashUIAuthoringPreview.Stop();
        }
        DrawDefaultInspector();
    }
}
[CustomEditor(typeof(CursorDashProjectedScoreView))]
internal sealed class CursorDashProjectedScoreViewEditor:Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("위치/상태별 색은 이 View의 Main/Combat 기준, 글꼴·Auto Size는 AnimationRoot의 TMP입니다. 실제 점수나 기록은 여기서 계산하지 않습니다.",MessageType.Info);
        using(new EditorGUILayout.HorizontalScope()){
            if(GUILayout.Button("메인 색으로 표시"))CursorDashUIAuthoringPreview.Show((CursorDashProjectedScoreView)target,true);
            if(GUILayout.Button("인게임 색으로 표시"))CursorDashUIAuthoringPreview.Show((CursorDashProjectedScoreView)target,false);
            if(GUILayout.Button("표시 종료"))CursorDashUIAuthoringPreview.Stop();
        }
        DrawDefaultInspector();
    }
}
