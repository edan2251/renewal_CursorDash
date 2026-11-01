using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// One rendering-only state for all real authored UI. Never invokes StageManager or a game controller.
public static partial class CursorDashUIAuthoringPreview
{
    private static StageManager screenStage;
    private static MeshRenderer screenField;
    private static Material fieldSample;
    private static MaterialPropertyBlock fieldProperties;
    private static bool whiteScreen;
    public static bool ScreenActive=>IsActive&&target==screenStage;
    private static void Visible(Behaviour item,bool value){if(item==null)return;Keep(item,()=>item.enabled,v=>item.enabled=v);item.enabled=value;}
    private static void Visible(Renderer item,bool value){if(item==null)return;Keep(item,()=>item.enabled,v=>item.enabled=v);item.enabled=value;}
    private static void WorldCanvas(Canvas canvas,Camera reference,int width,int height)
    {
        CanvasFrame(canvas);
        var scaler=canvas.GetComponent<CanvasScaler>();float factor=1;
        if(scaler!=null&&scaler.uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize)
            factor=Mathf.Pow(2,Mathf.Lerp(Mathf.Log(width/scaler.referenceResolution.x,2),Mathf.Log(height/scaler.referenceResolution.y,2),scaler.matchWidthOrHeight));
        var rect=(RectTransform)canvas.transform;rect.sizeDelta=new Vector2(width/factor,height/factor);
        rect.position=new Vector3(reference.transform.position.x,reference.transform.position.y,0);
        Keep(rect,()=>rect.localRotation,v=>rect.localRotation=v);rect.rotation=Quaternion.identity;
        rect.localScale=Vector3.one*(2*reference.orthographicSize/rect.rect.height);
    }
    private static string Label(Object source,string name)=>new SerializedObject(source).FindProperty(name).stringValue;
    private static void Text(TMP_Text label,string value){Keep(label,()=>label.text,v=>label.text=v);label.text=value;}
    private static float UserValue(string key)
    {
        try{var data=Newtonsoft.Json.Linq.JObject.Parse(PlayerPrefs.GetString("CursorDash.v2.user","{}"));if(data[key]!=null)return Mathf.Clamp01((float)data[key]);}catch{}
        return key=="user.vfxReduction"?0:1;
    }
    public static void ShowScreen(StageManager stage,CursorDashScreenAuthoringWindow.Screen state,int width,int height,string digits,float hp,float charge,float nodeCharge,float morph,float seconds,bool nodes,bool frame)
    {
        if(!Begin(stage,()=>ShowScreen(stage,state,width,height,digits,hp,charge,nodeCharge,morph,seconds,nodes,false)))return;
        try{BuildScreen(stage,state,width,height,digits,hp,charge,nodeCharge,morph,seconds,nodes,frame);}
        catch{Stop();throw;}
    }
    private static void BuildScreen(StageManager stage,CursorDashScreenAuthoringWindow.Screen state,int width,int height,string digits,float hp,float charge,float nodeCharge,float morph,float seconds,bool nodes,bool frame)
    {
        screenStage=stage;whiteScreen=state==CursorDashScreenAuthoringWindow.Screen.FeverNotice;
        var scene=stage.gameObject.scene;
        var original=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Camera>(true)).First(c=>c.CompareTag("MainCamera"));
        var cameraObject=new GameObject("EDITOR ONLY / layout reference camera"){hideFlags=HideFlags.HideAndDontSave};UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);owned.Add(cameraObject);
        var reference=cameraObject.AddComponent<Camera>();reference.CopyFrom(original);reference.enabled=false;reference.targetTexture=null;reference.aspect=(float)width/height;
        reference.transform.SetPositionAndRotation(original.transform.position,original.transform.rotation);
        bool main=state==CursorDashScreenAuthoringWindow.Screen.Main||state==CursorDashScreenAuthoringWindow.Screen.MainSettings;
        bool returning=state==CursorDashScreenAuthoringWindow.Screen.Returning;
        bool combat=!main&&!returning&&!whiteScreen;
        // Selectable state transitions write label tint; source design must not be tinted by a preview.
        foreach(var control in stage.SettingsView.GetComponentsInChildren<Selectable>(true)){
            Keep(control,()=>control.transition,v=>control.transition=v);control.transition=Selectable.Transition.None;
            var slider=control as Slider;if(slider!=null){if(slider.fillRect!=null)Rect(slider.fillRect);if(slider.handleRect!=null)Rect(slider.handleRect);}
        }
        var allCanvases=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Canvas>(true)).ToArray();
        foreach(var canvas in allCanvases)Visible(canvas,false);
        // Old pooled sprites are geometry references, not the approved runtime Mesh artwork.
        foreach(var root in scene.GetRootGameObjects()){
            if(root.GetComponent<PlayerCharacterController>()!=null||root==stage.GetComponent<NodeCreator>()?.AuthoredPool||root.name=="Cyberpunk_Background")
                foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))Visible(renderer,false);
        }
        var logo=stage.MainLogoView;WorldCanvas(logo.GetComponent<Canvas>(),reference,width,height);Visible(logo.GetComponent<Canvas>(),main);
        var menu=stage.MainMenuView;WorldCanvas(menu.Canvas,reference,width,height);
        var prompt=Ref<CanvasGroup>(menu,"promptAnimation");Group(prompt);prompt.alpha=main?1:0;
        var caption=Ref<CanvasGroup>(menu,"captionOpacity");Group(caption);caption.alpha=main||returning?1:0;
        Rect(Ref<RectTransform>(menu,"captionAnimation"));
        Text(menu.Caption,Label(menu,returning?"finalLabel":"bestLabel"));
        var score=stage.ProjectedScoreView;var text=score.Artwork;var rendererScore=text.GetComponent<MeshRenderer>();Visible(rendererScore,!whiteScreen&&state!=CursorDashScreenAuthoringWindow.Screen.Tutorial);
        Text(text,string.IsNullOrWhiteSpace(digits)?"0":digits);
        Rect(score.AnimationRoot);score.AnimationRoot.position=score.WorldPoint(reference,main||returning?score.MainViewport:score.CombatViewport);
        Keep(text,()=>text.color,v=>text.color=v);text.color=main||returning?score.MainTint:score.CombatTint;
        float glyphScale=main||returning?score.MainGlyphScale:1;
        Action<TMP_TextInfo> center=info=>CursorDashScoreGeometry.Center(info,text.rectTransform.rect,glyphScale);
        text.OnPreRenderText+=center;restore.Add(()=>{if(text!=null){text.OnPreRenderText-=center;text.ForceMeshUpdate();}});
        text.ForceMeshUpdate();float available=2*reference.orthographicSize*reference.aspect*.84f;
        glyphScale*=Mathf.Min(1,available/Mathf.Max(.001f,text.textBounds.size.x));text.ForceMeshUpdate();
        var bounds=rendererScore.bounds;var top=reference.WorldToViewportPoint(bounds.max);
        var menuRect=(RectTransform)menu.Canvas.transform;
        var captionPoint=new Vector2(0,(top.y-.5f)*menuRect.rect.height+menu.CaptionScoreGap);
        Ref<RectTransform>(menu,"captionAnimation").anchoredPosition=captionPoint;

        var settings=stage.SettingsView;WorldCanvas(settings.Canvas,reference,width,height);
        bool sheet=state==CursorDashScreenAuthoringWindow.Screen.MainSettings,pause=state==CursorDashScreenAuthoringWindow.Screen.InGamePause;
        Rect(settings.SafeArea);Rect(settings.SheetAnimation);Active(settings.SheetAnimation.gameObject);Rect(settings.ContentAnimation);Rect(settings.PauseAnimation);Group(settings.ContentOpacity);Group(settings.PauseOpacity);
        settings.Layout(new Rect(Vector2.zero,new Vector2(width,height)),new Vector2(width,height));
        settings.SheetAnimation.gameObject.SetActive(sheet);settings.SheetAnimation.anchoredPosition=Vector2.zero;settings.ContentOpacity.alpha=sheet?1:0;
        settings.PauseOpacity.gameObject.SetActive(pause);settings.PauseOpacity.alpha=pause?1:0;settings.PauseAnimation.anchoredPosition=Vector2.zero;
        settings.ContentOpacity.interactable=settings.PauseOpacity.interactable=true;
        Visible(settings.Canvas,sheet||pause);
        // This factor is transient and leaves the actual shade Image's design RGBA editable.
        if(sheet||pause){var shadeGroup=settings.Shade.gameObject.AddComponent<CanvasGroup>();shadeGroup.hideFlags=HideFlags.HideAndDontSave;owned.Add(shadeGroup);shadeGroup.alpha=sheet?.32f:.82f;shadeGroup.blocksRaycasts=false;}
        foreach(var row in settings.Rows){var slider=row.slider;Keep(slider,()=>slider.value,v=>slider.SetValueWithoutNotify(v));slider.SetValueWithoutNotify(UserValue(row.key));Text(row.value,Mathf.RoundToInt(slider.value*100)+"%");}

        var nexus=stage.NexusView;var body=nexus.Body;var nexusCanvas=body.GetComponentInParent<Canvas>(true);WorldCanvas(nexusCanvas,reference,width,height);Visible(nexusCanvas,combat);Visible(body,combat);Visible(nexus.Outer,false);Visible(nexus.Dotted,combat);
        var material=body.material!=null&&body.material.shader.name=="CursorDash/NexusEnergy"?new Material(body.material):new Material(Resources.Load<Shader>("CursorDash/NexusEnergy"));
        material.hideFlags=HideFlags.HideAndDontSave;owned.Add(material);material.SetFloat("_SolidFraction",stage.NexusSolidFraction);material.SetFloat("_Level",Mathf.Clamp01(hp));material.SetFloat("_ImpactPress",0);material.SetColor("_Color",nexus.EnergyTint);
        if(state==CursorDashScreenAuthoringWindow.Screen.Tutorial){
            var style=Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");var ink=style==null?new Color(.55f,.58f,.62f):style.tutorialInk;
            material.SetFloat("_TutorialPower",0);material.SetColor("_TutorialInk",ink);Keep(nexus.Dotted,()=>nexus.Dotted.color,v=>nexus.Dotted.color=v);nexus.Dotted.color=ink;
        }
        Canvas.ForceUpdateCanvases();if(combat){body.canvasRenderer.materialCount=1;body.canvasRenderer.SetMaterial(material,0);}restore.Add(()=>{if(body!=null)body.SetMaterialDirty();});
        Vector3 centerPoint=body.rectTransform.TransformPoint(body.rectTransform.rect.center);float radius=Mathf.Min(body.rectTransform.rect.width*Mathf.Abs(body.rectTransform.lossyScale.x),body.rectTransform.rect.height*Mathf.Abs(body.rectTransform.lossyScale.y))*.5f*stage.NexusSolidFraction;
        var guidance=stage.GuidanceView;var hud=guidance.GetComponentInParent<Canvas>(true);WorldCanvas(hud,reference,width,height);
        foreach(var graphic in hud.GetComponentsInChildren<Graphic>(true))Visible(graphic,false);
        var pauseButton=menu.PauseButton;if(pauseButton!=null)foreach(var graphic in pauseButton.GetComponentsInChildren<Graphic>(true))Visible(graphic,combat);
        var hint=Ref<CanvasGroup>(guidance,"hintOpacity");var notice=Ref<CanvasGroup>(guidance,"noticeOpacity");Group(hint);Group(notice);Active(guidance.HintRoot);Active(guidance.NoticeRoot);
        guidance.HintRoot.SetActive(state==CursorDashScreenAuthoringWindow.Screen.Tutorial);guidance.NoticeRoot.SetActive(whiteScreen);hint.alpha=1;notice.alpha=1;
        Visible(guidance.Hint,state==CursorDashScreenAuthoringWindow.Screen.Tutorial);Visible(guidance.Notice,whiteScreen);
        if(whiteScreen){var card=new GameObject("EDITOR ONLY / fever white",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)){hideFlags=HideFlags.HideAndDontSave};card.transform.SetParent(hud.transform,false);card.transform.SetAsFirstSibling();owned.Add(card);var rect=(RectTransform)card.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.sizeDelta=Vector2.zero;card.GetComponent<Image>().color=Color.white;}
        var player=scene.GetRootGameObjects().Select(g=>g.GetComponent<PlayerCharacterController>()).FirstOrDefault(c=>c!=null);
        if(player!=null){var cursor=Ref<SpriteRenderer>(player,"cursorVisual");Visible(cursor,combat);Keep(player.transform,()=>player.transform.position,v=>player.transform.position=v);player.transform.position=centerPoint+new Vector3(.8f,-.8f,0);}

        if(stage.BoardView!=null&&stage.BoardView.IsConfigured){screenField=stage.BoardView.ColorField;Visible(screenField,!whiteScreen);}
        else{
            var field=new GameObject("EDITOR ONLY / living background",typeof(MeshFilter),typeof(MeshRenderer)){hideFlags=HideFlags.HideAndDontSave};UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(field,scene);owned.Add(field);
            field.GetComponent<MeshFilter>().sharedMesh=Resources.Load<Mesh>("CursorDash/BoardFieldQuad");screenField=field.GetComponent<MeshRenderer>();screenField.sharedMaterial=Resources.Load<Material>("CursorDash/BoardColor");screenField.sortingOrder=-1000;screenField.enabled=!whiteScreen;
        }
        Keep(screenField.transform,()=>screenField.transform.position,v=>screenField.transform.position=v);Keep(screenField.transform,()=>screenField.transform.localScale,v=>screenField.transform.localScale=v);
        CursorDashLivingBackground.PlaceField(screenField.transform,reference);
        var displayedField=screenField;var oldBlock=new MaterialPropertyBlock();displayedField.GetPropertyBlock(oldBlock);restore.Add(()=>{if(displayedField!=null)displayedField.SetPropertyBlock(oldBlock);});
        fieldSample=new Material(screenField.sharedMaterial){hideFlags=HideFlags.HideAndDontSave};owned.Add(fieldSample);fieldProperties=new MaterialPropertyBlock();TickScreen(seconds);
        var art=CursorDashScreenAuthoringWindow.SceneArtwork(stage,state,reference,width,height,text,centerPoint,radius,charge,nodeCharge,morph,nodes);owned.Add(art);
        Canvas.ForceUpdateCanvases();if(combat){body.canvasRenderer.materialCount=1;body.canvasRenderer.SetMaterial(material,0);}
        if(frame&&SceneView.lastActiveSceneView!=null){var sv=SceneView.lastActiveSceneView;sv.in2DMode=true;sv.Frame(new Bounds(new Vector3(reference.transform.position.x,reference.transform.position.y,0),new Vector3(2*reference.orthographicSize*reference.aspect,2*reference.orthographicSize,.1f)),true);sv.Show();sv.Focus();}
        SceneView.RepaintAll();
    }
    public static void TickScreen(float seconds)
    {
        if(!ScreenActive||screenField==null||fieldSample==null||whiteScreen)return;
        var profile=screenStage.BoardView==null?Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype"):screenStage.BoardView.Profile;
        CursorDashLivingBackground.ApplyFrame(fieldSample,profile,seconds,0,Vector2.zero);
        foreach(var key in new[]{"_Navy","_Teal","_Violet","_HitDrift"})fieldProperties.SetVector(key,fieldSample.GetVector(key));
        foreach(var key in new[]{"_Clock","_FlowSpeed","_Mix","_Bend","_HitColor"})fieldProperties.SetFloat(key,fieldSample.GetFloat(key));
        screenField.SetPropertyBlock(fieldProperties);SceneView.RepaintAll();
    }
}
