using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Presentation and input handoff inside the existing Cyberpunk scene. No second game manager/camera.</summary>
[DefaultExecutionOrder(60)]
public sealed class CursorDashSpaceFlow : MonoBehaviour
{
    private StageManager stage;private CursorDashVisualProfile profile;private CursorDashStartTimeline timeline;
    private GameObject canvasRoot,pauseButton;private RectTransform logoRect;private Image logo;private TextMeshProUGUI prompt,bestCaption;
    private Vector2 direction=Vector2.up,endpoint;private float age,distance;
    private System.Numerics.BigInteger bestNumber;
    private bool quick,ringStarted,cursorStarted,pauseWasActive;private string bestValue="0";
    private readonly List<RaycastResult> uiHits=new List<RaycastResult>();private EventSystem pointerSystem;private PointerEventData pointerData;
    public MeshRenderer BestScoreRenderer=>stage.Fever.BackgroundScoreRenderer; // Same numeric text, never a second renderer.
    public float BestMaskWeight=>0;
    public float CurrentScoreAlpha=>1;
    public float LastStartSeconds {get;private set;}
    public bool IsMenu=>stage!=null&&stage.State==StageManager.RoundState.Main;
    public bool IsStarting=>stage!=null&&stage.State==StageManager.RoundState.Starting;
    public bool AtmosphereActive=>IsMenu||IsStarting;
    public string BestText=>bestValue;
    public bool UsesBestProjection=>IsMenu||IsStarting&&!quick;
    public float ScoreTravel=>IsMenu?0:IsStarting&&!quick?(float)timeline.ScoreTravel:1;
    public string DisplayedPoints=>IsMenu?bestValue:IsStarting&&!quick?CursorDashScoreCountdown.At(bestNumber,timeline.ScoreCountProgress).ToString(System.Globalization.CultureInfo.InvariantCulture):stage.Fever.Model.PointsText;
    public float StartProgress=>quick?Mathf.Clamp01(age/.24f):(float)(timeline?.Progress??0);
    public void Initialize(StageManager owner)
    {
        stage=owner;profile=stage.Fever.Visuals;CreateMenu();
        var hud=GameObject.Find("RemakeHUD");pauseButton=hud==null?null:hud.transform.Find("PauseButton")?.gameObject;
        if(pauseButton!=null)pauseWasActive=pauseButton.activeSelf;
        bool launch=CursorDashSession.IsPractice||CursorDashSession.EnterGameOnLoad;CursorDashSession.EnterGameOnLoad=false;
        if(launch)Begin(true,Vector2.up);else Home();
    }
    private void CreateMenu()
    {
        canvasRoot=new GameObject("Shared-space main UI",typeof(RectTransform)){hideFlags=HideFlags.DontSave};
        var canvas=canvasRoot.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=25;
        var scaler=canvasRoot.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
        var image=new GameObject("Existing title logo",typeof(RectTransform)){hideFlags=HideFlags.DontSave};image.transform.SetParent(canvasRoot.transform,false);
        logo=image.AddComponent<Image>();logo.sprite=profile==null?null:profile.mainLogo;logo.preserveAspect=true;logo.raycastTarget=false;
        logoRect=image.GetComponent<RectTransform>();logoRect.anchorMin=logoRect.anchorMax=new Vector2(.5f,.79f);logoRect.pivot=Vector2.one*.5f;
        float height=logo.sprite==null?240:780*logo.sprite.rect.height/Mathf.Max(1,logo.sprite.rect.width);logoRect.sizeDelta=new Vector2(780,height);
        var hint=new GameObject("Start prompt",typeof(RectTransform)){hideFlags=HideFlags.DontSave};hint.transform.SetParent(canvasRoot.transform,false);
        prompt=hint.AddComponent<TextMeshProUGUI>();prompt.font=CursorDashTypography.Medium;prompt.fontSize=38;prompt.text="클릭해서 게임 시작";prompt.alignment=TextAlignmentOptions.Center;prompt.raycastTarget=false;
        prompt.rectTransform.anchorMin=prompt.rectTransform.anchorMax=new Vector2(.5f,.20f);prompt.rectTransform.sizeDelta=new Vector2(1000,90);
        var caption=new GameObject("BEST caption",typeof(RectTransform)){hideFlags=HideFlags.DontSave};caption.transform.SetParent(canvasRoot.transform,false);
        bestCaption=caption.AddComponent<TextMeshProUGUI>();bestCaption.font=CursorDashTypography.Medium;bestCaption.fontSize=27;bestCaption.text="BEST";bestCaption.alignment=TextAlignmentOptions.Center;bestCaption.raycastTarget=false;
        bestCaption.rectTransform.sizeDelta=new Vector2(600,55);
    }
    public void Home()
    {
        timeline=null;age=0;quick=ringStarted=false;CursorDashSession.EnterGameOnLoad=false;CursorDashSession.CurrentMode=CursorDashSession.Mode.Normal;
        stage.PrepareSpace();stage.SetSpaceState(StageManager.RoundState.Main);stage.Fever.Circuits?.ResetSpace();
        bestNumber=CursorDashPointsRecord.Read(GameManager.instance.MapInformation.mapName);bestValue=bestNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
        canvasRoot.SetActive(true);
        SetHud(false);SetNexus(0,0);SetCursor(false,stage.NexusPosition);GameManager.instance.CurrentSceneType=GameManager.SceneType.NotInGame;
    }
    public bool Begin(bool fast,Vector2 launchDirection)
    {
        if(IsStarting)return false;
        CursorDashSession.EnterGameOnLoad=false;stage.PrepareSpace();stage.SetSpaceState(StageManager.RoundState.Starting);quick=fast;age=0;ringStarted=cursorStarted=false;
        timeline=new CursorDashStartTimeline(profile==null?1.35:profile.spaceStartSeconds,profile==null?.28:profile.spaceRetreatEnd,profile==null?.82:profile.spaceRebuildEnd,profile==null?.42:profile.spaceNexusAt,profile==null?.64:profile.spaceCursorAt,profile==null?.10:profile.spaceRingLead,profile==null?.045:profile.spaceBodyOvershoot,profile==null?.18:profile.spaceCursorAcceleration);
        direction=launchDirection.sqrMagnitude>.001f?launchDirection.normalized:Vector2.up;
        distance=profile==null?.95f:Mathf.Clamp(profile.spaceCursorDistance,.2f,1.5f);
        var camera=Camera.main;float margin=CursorDashTuning.Number("player.margin",.08f);
        Vector2 lo=camera.ViewportToWorldPoint(new Vector3(margin,margin,0)),hi=camera.ViewportToWorldPoint(new Vector3(1-margin,1-margin,0));
        endpoint=PlayerCharacterController.ClampPosition(stage.NexusPosition+direction*distance,lo,hi);
        canvasRoot.SetActive(!fast);SetHud(false);SetNexus(0,0);SetCursor(false,stage.NexusPosition);
        GameManager.instance.CurrentSceneType=GameManager.SceneType.NotInGame;return true;
    }
    private void Update()
    {
        if(!IsMenu||CursorDashRuntimeUI.InputBlocked)return;
        bool pressed=Input.GetMouseButtonDown(0);Vector2 screen=Input.mousePosition;
        if(Input.touchCount>0){var touch=Input.GetTouch(0);pressed=touch.phase==TouchPhase.Began;screen=touch.position;}
        if(!pressed||CursorDashRuntimeUI.IsScreenPointBlocked(screen))return;
        RequestStart(screen);
    }
    private bool OverControl(Vector2 screen)
    {
        var current=EventSystem.current;if(current==null)return false;
        if(pointerData==null||pointerSystem!=current){pointerSystem=current;pointerData=new PointerEventData(current);}
        pointerData.position=screen;uiHits.Clear();current.RaycastAll(pointerData,uiHits);
        // Match the existing stick's ownership rule: background Images are not interactive controls.
        foreach(var hit in uiHits)if(hit.gameObject.GetComponentInParent<Selectable>()!=null)return true;
        return false;
    }
    public bool RequestStart(Vector2 screen)
    {
        if(!IsMenu||CursorDashRuntimeUI.InputBlocked||CursorDashRuntimeUI.IsScreenPointBlocked(screen)||OverControl(screen))return false;
        CursorDashSession.CurrentMode=CursorDashSession.FirstGuideNeeded?CursorDashSession.Mode.Tutorial:CursorDashSession.Mode.Normal;CursorDashTuning.EnableDev(false);
        Vector2 aim=(Vector2)Camera.main.ScreenToWorldPoint(screen)-stage.NexusPosition;return Begin(false,aim);
    }
    public void Restart()
    {
        if(IsMenu){RequestStart(new Vector2(Screen.width*.5f,Screen.height*.5f));return;}
        if(IsStarting&&quick)return;
        LaunchSelectedMode();
    }
    // Explicit existing tool commands retain their selected mode, including from Main before first guide.
    public void LaunchSelectedMode(){timeline=null;stage.SetSpaceState(StageManager.RoundState.Ended);Begin(true,Vector2.up);}
    public void Advance(float delta)
    {
        if(!IsStarting)return;
        if(CursorDashRuntimeUI.InputBlocked){Home();return;}
        age+=Mathf.Max(0,delta);timeline.Advance(delta);float progress=StartProgress;
        float ring=quick?Mathf.SmoothStep(0,1,progress/.20f):(float)timeline.Ring;
        float nexus=quick?(float)CursorDashStartTimeline.Body((progress-.12f)/.58f,.045):(float)timeline.Nexus;
        float cursor=quick?(float)CursorDashStartTimeline.Launch((progress-.24f)/.76f):(float)timeline.Cursor;
        SetNexus(nexus,ring);
        if(ring>0&&!ringStarted){ringStarted=true;stage.Fever.StartRingPulse(profile==null?780:profile.spaceRingBoost,profile==null?.30f:profile.spaceRingDecay);}
        if(cursor>0){
            if(!cursorStarted){cursorStarted=true;SetCursor(true,stage.NexusPosition);PlayerCharacterAfterImage.instance?.DrawEntryPoint();}
            SetCursor(true,Vector2.LerpUnclamped(stage.NexusPosition,endpoint,cursor));PlayerCharacterAfterImage.instance?.DrawEntryPoint();
        }
        if(!quick){
            logo.color=new Color(1,1,1,(float)timeline.LogoAlpha);logoRect.anchoredPosition=new Vector2((float)timeline.LogoTravel*150,0);
            prompt.color=new Color(.85f,.92f,1,(float)timeline.PromptAlpha);
        }
        bool released=!Input.GetMouseButton(0)&&Input.touchCount==0;
        if(progress>=1&&released){
            LastStartSeconds=age;timeline=null;canvasRoot.SetActive(false);
            SetNexus(1,1);SetCursor(true,endpoint);PlayerCharacterAfterImage.instance?.FinishEntry();SetHud(true);stage.BeginSpaceGameplay();
        }
    }
    public double CircuitFront(double length)
    {
        if(!IsStarting||quick||timeline==null)return length;
        return timeline.Progress<=timeline.RetreatEnd?length*(1-timeline.Retreat):length*timeline.Rebuild;
    }
    private void LateUpdate()
    {
        if(logo==null)return;
        if(IsMenu){logo.color=Color.white;logoRect.anchoredPosition=Vector2.zero;prompt.color=new Color(.85f,.92f,1,.35f+.50f*(.5f+.5f*Mathf.Sin(Time.unscaledTime*2.7f)));}
        if(IsMenu||IsStarting&&!quick){
            var canvas=canvasRoot.GetComponent<RectTransform>();float width=Mathf.Min(profile==null?840:profile.spaceLogoWidth,canvas.rect.width*.86f);
            logoRect.sizeDelta=new Vector2(width,logo.sprite==null?240:width*logo.sprite.rect.height/Mathf.Max(1,logo.sprite.rect.width));
            logoRect.anchorMin=logoRect.anchorMax=new Vector2(.5f,profile==null?.79f:profile.spaceLogoY);
            Vector2 point=Camera.main.WorldToScreenPoint(stage.NexusPosition+Vector2.up*(profile==null?.28f:profile.spaceMainScoreLift));
            if(BestScoreRenderer!=null)point.y=Mathf.Max(point.y,Camera.main.WorldToScreenPoint(BestScoreRenderer.bounds.max).y+22);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,point,null,out Vector2 local);bestCaption.rectTransform.anchoredPosition=local;
            bestCaption.color=new Color(1,1,1,IsMenu? .75f: .75f*(1-(float)CursorDashStartTimeline.Smooth(timeline.Progress/.30)));
        }
    }
    private void SetHud(bool value){if(pauseButton!=null)pauseButton.SetActive(value&&pauseWasActive);}
    private void SetNexus(float body,float ring){var view=stage.NexusPresentation;if(view==null)return;view.FinaleScale=1;view.FinaleAlpha=1;view.EntryBodyScale=Mathf.Max(.0001f,body);view.EntryBodyAlpha=body>0?1:0;view.EntryRingScale=.85f+.15f*ring;view.EntryRingAlpha=ring;}
    private void SetCursor(bool visible,Vector2 point)=>PlayerCharacterController.instance?.PresentEntry(visible,point,direction);
    private void OnDestroy(){if(pauseButton!=null)pauseButton.SetActive(pauseWasActive);if(canvasRoot!=null)Destroy(canvasRoot);}
}
