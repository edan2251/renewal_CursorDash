using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>One-time explicit bindings. No gameplay simulation, mass SaveAssets or migration on editor startup.</summary>
public static class CursorDashWorldAuthoringSetup
{
    public const string GuidancePrefab="Assets/Prefabs/UI/GuidanceView.prefab";
    public static void CompleteNexusPlacement()
    {
        var scene=SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.isDirty||scene.path!="Assets/Scenes/Cyberpunk.unity")throw new InvalidOperationException("Clean idle Cyberpunk required.");
        var stage=UnityEngine.Object.FindFirstObjectByType<StageManager>();var view=stage.NexusView;
        if(view==null||!view.IsConfigured||view.PlacementRoot!=null)throw new InvalidOperationException("Expected the existing Nexus sources without a placement root.");
        Canvas.ForceUpdateCanvases();var images=new[]{view.Body,view.Outer,view.Dotted};var before=new Vector3[images.Length];
        for(int i=0;i<images.Length;i++)before[i]=images[i].transform.position;
        var parent=view.Body.transform.parent;
        var placement=Rect("NexusPlacement",parent);placement.anchorMin=placement.anchorMax=Vector2.one*.5f;placement.pivot=Vector2.one*.5f;placement.sizeDelta=new Vector2(211,211);placement.anchoredPosition=Vector2.zero;
        placement.SetSiblingIndex(images[0].transform.GetSiblingIndex());
        foreach(var image in images)image.transform.SetParent(placement,false);
        Canvas.ForceUpdateCanvases();
        for(int i=0;i<images.Length;i++)if(Vector3.Distance(before[i],images[i].transform.position)>.00001f)throw new InvalidOperationException("Nexus placement changed its original world position; do not save.");
        var data=new SerializedObject(view);Reference(data,"placementRoot",placement);data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    public static void Apply()
    {
        var scene=SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.isDirty||scene.path!="Assets/Scenes/Cyberpunk.unity")throw new InvalidOperationException("Clean idle Cyberpunk required.");
        var stage=UnityEngine.Object.FindFirstObjectByType<StageManager>();
        if(stage==null||stage.NexusView!=null||stage.GuidanceView!=null)throw new InvalidOperationException("Migration already applied or source missing.");
        string appearancePath="Assets/Resources/CursorDash/NodeAppearance.asset",layoutPath="Assets/Resources/CursorDash/CircuitLayout.asset";
        foreach(var path in new[]{GuidancePrefab,appearancePath,layoutPath})if(AssetDatabase.LoadMainAssetAtPath(path)!=null)throw new InvalidOperationException("Do not overwrite existing authored data: "+path);
        var appearance=ScriptableObject.CreateInstance<CursorDashNodeAppearance>();AssetDatabase.CreateAsset(appearance,appearancePath);
        var layout=ScriptableObject.CreateInstance<CursorDashCircuitLayout>();layout.CaptureApprovedLayout();AssetDatabase.CreateAsset(layout,layoutPath);
        var profile=Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");profile.circuitLayout=layout;EditorUtility.SetDirty(profile);AssetDatabase.SaveAssetIfDirty(profile);

        var nexus=stage.NexusBodySource.transform.parent.gameObject.AddComponent<CursorDashNexusView>();
        var data=new SerializedObject(nexus);Reference(data,"body",stage.NexusBodySource);Reference(data,"outer",stage.NexusFrameSource);Reference(data,"dotted",stage.NexusBodySource.transform.parent.Find("HpCentral").GetComponent<Image>());data.ApplyModifiedPropertiesWithoutUndo();
        var stageData=new SerializedObject(stage);var fontSource=(TMP_Text)stageData.FindProperty("timeScoreLabel").objectReferenceValue;
        if(fontSource==null||fontSource.canvas==null)throw new InvalidOperationException("Existing HUD is required.");
        var guide=CreateGuidance(Resources.Load<TMP_FontAsset>("CursorDash/Fonts/esamanru Medium View SDF"));
        PrefabUtility.SaveAsPrefabAssetAndConnect(guide.gameObject,GuidancePrefab,InteractionMode.AutomatedAction);
        guide.transform.SetParent(fontSource.canvas.transform,false);Stretch((RectTransform)guide.transform);
        var pause=stage.MainMenuView.PauseButton;if(pause!=null)guide.transform.SetSiblingIndex(pause.transform.GetSiblingIndex());
        Reference(stageData,"nexusView",nexus);Reference(stageData,"guidanceView",guide);stageData.ApplyModifiedPropertiesWithoutUndo();
        var player=UnityEngine.Object.FindFirstObjectByType<PlayerCharacterController>();
        var playerData=new SerializedObject(player);Reference(playerData,"cursorVisual",player.transform.Find("CursorVisual").GetComponent<SpriteRenderer>());playerData.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }

    private static void Reference(SerializedObject data,string key,UnityEngine.Object value)=>data.FindProperty(key).objectReferenceValue=value;
    private static RectTransform Rect(string name,Transform parent)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);return rect;
    }
    private static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    private static TextMeshProUGUI Label(string name,Transform parent,TMP_FontAsset font,float size,Vector2 anchor,Vector2 dimensions)
    {
        var rect=Rect(name,parent);rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=dimensions;
        var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.enabled=false;text.font=font;text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=size*.65f;text.fontSizeMax=size;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return text;
    }
    private static CanvasGroup Opacity(GameObject item){var value=item.AddComponent<CanvasGroup>();value.alpha=0;value.interactable=value.blocksRaycasts=false;return value;}
    private static CursorDashGuidanceView CreateGuidance(TMP_FontAsset font)
    {
        var root=Rect("GuidanceView",null);Stretch(root);
        var noticeRoot=Rect("First fever notice",root);Stretch(noticeRoot);var noticeOpacity=Opacity(noticeRoot.gameObject);
        var white=noticeRoot.gameObject.AddComponent<Image>();white.color=Color.clear;white.raycastTarget=false;
        var notice=Label("First fever wording",noticeRoot,font,60,Vector2.one*.5f,new Vector2(960,300));notice.text="- FEVER TIME -\n검은 노드를 마구 파괴하세요!";notice.color=Color.black;
        var hintRoot=Rect("Checklist placement",root);Stretch(hintRoot);var hintOpacity=Opacity(hintRoot.gameObject);
        var hint=Label("Checklist",hintRoot,font,34,new Vector2(.5f,.13f),new Vector2(960,90));hint.text="원형노드 파괴 0/2\n육각노드로부터 수비 0/2";hint.color=Color.white;
        var view=root.gameObject.AddComponent<CursorDashGuidanceView>();var data=new SerializedObject(view);
        Reference(data,"hintRoot",hintRoot.gameObject);Reference(data,"noticeRoot",noticeRoot.gameObject);Reference(data,"hint",hint);Reference(data,"notice",notice);Reference(data,"noticeBackground",white);Reference(data,"hintOpacity",hintOpacity);Reference(data,"noticeOpacity",noticeOpacity);data.ApplyModifiedPropertiesWithoutUndo();return view;
    }
}
