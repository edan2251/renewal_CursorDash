using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Two views of the existing user settings. Stage owns pause/time; Tuning owns data/storage.</summary>
[DefaultExecutionOrder(-100)]
public sealed class CursorDashSettingsUI : MonoBehaviour
{
    public static CursorDashSettingsUI Instance {get;private set;}
    public static bool BlocksInput=>Instance!=null&&(Instance.sheet>0.001f||Instance.open||Instance.releaseInput||Instance.paused);
    public bool SheetOpen=>open;
    public float SheetProgress=>sheet;
    public bool PauseVisible=>paused;
    public string SaveError {get;private set;}="";
    private StageManager stage;
    private RectTransform root,safe,sheetRect,content,pauseCard,handle;
    private CanvasGroup sheetContent,pauseGroup;
    private Image shade;
    private GameObject pauseRoot,sheetRoot;
    private readonly List<SettingRow> rows=new List<SettingRow>();
    private bool open,paused,releaseInput,dirty,dragging,launchTutorial;
    private float sheet,pauseAlpha,sheetHeight,dragOrigin,dragValue,dragTime,dragY,velocity;
    private int lastWidth,lastHeight;private Rect lastSafe;
    private static readonly Color Ink=new Color(.92f,.96f,.98f),Muted=new Color(.57f,.66f,.73f),Accent=new Color(.24f,.88f,.81f);
    private sealed class SettingRow {public string key;public Slider slider;public TMP_Text value;}

    public void Initialize(StageManager owner)
    {
        stage=owner;Instance=this;CursorDashRuntimeUI.Instance?.RefreshAudio();Build();CursorDashTuning.Changed+=Sync;Sync();Layout();
    }
    private RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 size,Vector2 position)
    {
        var go=new GameObject(name,typeof(RectTransform)){hideFlags=HideFlags.DontSave};go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.pivot=Vector2.one*.5f;rect.sizeDelta=size;rect.anchoredPosition=position;return rect;
    }
    private Image Image(RectTransform rect,Color tint){var image=rect.gameObject.AddComponent<Image>();image.color=tint;image.raycastTarget=false;return image;}
    private TMP_Text Label(string name,Transform parent,string text,float size,Vector2 dimensions,Vector2 pos,Color tint,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
    {
        var rect=Rect(name,parent,new Vector2(.5f,1),dimensions,pos);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font=CursorDashTypography.Medium;label.fontSize=size;label.color=tint;label.text=text;label.alignment=alignment;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.NoWrap;return label;
    }
    private Button Button(string name,Transform parent,string text,Vector2 size,Vector2 pos,float font=23)
    {
        var rect=Rect(name,parent,new Vector2(.5f,1),size,pos);var hit=Image(rect,Color.clear);hit.raycastTarget=true;
        var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=hit;
        var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,1,1,.12f);colors.pressedColor=new Color(.25f,.9f,.8f,.2f);button.colors=colors;
        var label=Label(name+" text",rect,text,font,size,new Vector2(0,-size.y*.5f),Ink,TextAlignmentOptions.Center);button.targetGraphic=label;
        colors.normalColor=Ink;colors.highlightedColor=Accent;colors.pressedColor=Color.white;button.colors=colors;
        return button;
    }
    private void Build()
    {
        root=Rect("CursorDash settings and pause",transform,Vector2.one*.5f,Vector2.zero,Vector2.zero);
        var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=90;
        var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(540,960);scaler.matchWidthOrHeight=1;
        root.gameObject.AddComponent<GraphicRaycaster>();
        safe=Rect("Safe area",root,Vector2.one*.5f,Vector2.zero,Vector2.zero);
        var dim=Rect("Menu shade",root,Vector2.one*.5f,Vector2.zero,Vector2.zero);dim.anchorMin=Vector2.zero;dim.anchorMax=Vector2.one;
        shade=Image(dim,new Color(.01f,.02f,.045f,0));
        // The shade must intercept the board, never the controls inside Safe area.
        // Safe area was created first; sibling order also determines UI raycast priority.
        dim.SetAsFirstSibling();
        sheetRect=Rect("Main bottom sheet",safe,new Vector2(.5f,0),Vector2.zero,Vector2.zero);sheetRect.pivot=new Vector2(.5f,0);sheetRoot=sheetRect.gameObject;
        Image(sheetRect,new Color(.022f,.035f,.066f,.985f)).raycastTarget=true;
        handle=Rect("Sheet handle touch area",sheetRect,new Vector2(.5f,1),new Vector2(180,48),new Vector2(0,-24));
        Image(handle,Color.clear).raycastTarget=true;handle.gameObject.AddComponent<CursorDashSheetHandle>().owner=this;
        Image(Rect("Handle",handle,Vector2.one*.5f,new Vector2(62,3),Vector2.zero),new Color(.7f,.83f,.86f,.75f));
        content=Rect("Settings content",sheetRect,new Vector2(.5f,1),new Vector2(460,790),new Vector2(0,-55));content.pivot=new Vector2(.5f,1);
        sheetContent=content.gameObject.AddComponent<CanvasGroup>();
        Label("Title",content,"설정",30,new Vector2(460,46),new Vector2(0,-24),Ink);
        Label("Audio heading",content,"사운드",15,new Vector2(460,32),new Vector2(0,-85),Muted);
        Row(content,"user.master","전체 음량",-144);Row(content,"user.bgm","배경음",-223);Row(content,"user.sfx","효과음",-302);
        Label("Effects heading",content,"화면 효과",15,new Vector2(460,32),new Vector2(0,-374),Muted);
        Row(content,"user.vfxReduction","전체 VFX 완화",-433);Row(content,"user.shake","화면 흔들림",-512);Row(content,"user.flash","플래시",-591);Row(content,"user.distortion","왜곡·색수차",-670);
        Button("Replay tutorial",content,"튜토리얼부터 시작",new Vector2(310,54),new Vector2(-50,-751)).onClick.AddListener(ReplayTutorial);
        Button("Reset audio and VFX",content,"초기화",new Vector2(110,54),new Vector2(180,-751),17).onClick.AddListener(ResetSettings);
        pauseRoot=Rect("Pause overlay",safe,Vector2.one*.5f,Vector2.zero,Vector2.zero).gameObject;
        pauseGroup=pauseRoot.AddComponent<CanvasGroup>();
        pauseCard=Rect("Pause content",pauseRoot.transform,Vector2.one*.5f,new Vector2(460,530),Vector2.zero);
        Label("Pause title",pauseCard,"일시정지",25,new Vector2(460,55),new Vector2(0,-32),Muted,TextAlignmentOptions.Center);
        Button("Continue",pauseCard,"계속하기",new Vector2(330,78),new Vector2(0,-132),35).onClick.AddListener(Resume);
        Image(Rect("Continue accent",pauseCard,new Vector2(.5f,1),new Vector2(64,2),new Vector2(0,-185)),Accent);
        Row(pauseCard,"user.master","전체 음량",-264);Row(pauseCard,"user.vfxReduction","전체 VFX 완화",-367);
        Button("Retry",pauseCard,"다시 시작",new Vector2(180,60),new Vector2(-115,-481),20).onClick.AddListener(Retry);
        Button("Home",pauseCard,"메인으로",new Vector2(180,60),new Vector2(115,-481),20).onClick.AddListener(Home);
        pauseRoot.SetActive(false);
    }
    private void Row(Transform parent,string key,string label,float y)
    {
        var rect=Rect(key,parent,new Vector2(.5f,1),new Vector2(460,74),new Vector2(0,y));
        Label(key+" label",rect,label,21,new Vector2(360,32),new Vector2(-50,-15),Ink);
        var number=Label(key+" value",rect,"",18,new Vector2(85,32),new Vector2(187,-15),Muted,TextAlignmentOptions.Right);
        var track=Rect(key+" slider",rect,new Vector2(.5f,1),new Vector2(460,48),new Vector2(0,-54));
        Image(track,Color.clear).raycastTarget=true;
        var slider=track.gameObject.AddComponent<CursorDashSettingSlider>();slider.minValue=0;slider.maxValue=1;
        Image(Rect("Track",track,Vector2.one*.5f,new Vector2(460,4),Vector2.zero),new Color(.36f,.47f,.54f,.82f));
        var fillArea=Rect("Fill area",track,Vector2.one*.5f,new Vector2(460,4),Vector2.zero);
        var fill=Rect("Fill",fillArea,Vector2.zero,Vector2.zero,Vector2.zero);Image(fill,Accent);slider.fillRect=fill;
        // Slider drives the handle's cross-axis anchors to 0..1. A zero-height
        // travel area keeps the visual handle 12x12 while the track remains a 48px hit area.
        var sliding=Rect("Handle area",track,Vector2.one*.5f,new Vector2(460,0),Vector2.zero);
        var knob=Rect("Handle",sliding,Vector2.one*.5f,new Vector2(12,12),Vector2.zero);var knobImage=Image(knob,Accent);slider.handleRect=knob;slider.targetGraphic=knobImage;
        slider.onValueChanged.AddListener(value=>SetValue(key,value));slider.finished=Save;
        rows.Add(new SettingRow{key=key,slider=slider,value=number});
    }
    private void Layout()
    {
        Canvas.ForceUpdateCanvases();var area=Screen.safeArea;
        safe.anchorMin=new Vector2(area.xMin/Mathf.Max(1,Screen.width),area.yMin/Mathf.Max(1,Screen.height));safe.anchorMax=new Vector2(area.xMax/Mathf.Max(1,Screen.width),area.yMax/Mathf.Max(1,Screen.height));safe.offsetMin=safe.offsetMax=Vector2.zero;
        Canvas.ForceUpdateCanvases();sheetHeight=safe.rect.height*.95f;
        sheetRect.sizeDelta=new Vector2(safe.rect.width,sheetHeight);
        float fit=Mathf.Min(1,(safe.rect.width-48)/460,(sheetHeight-65)/790);
        content.localScale=Vector3.one*Mathf.Max(.4f,fit);
        float pauseFit=Mathf.Min(1,(safe.rect.width-48)/460,(safe.rect.height-90)/530);pauseCard.localScale=Vector3.one*Mathf.Max(.4f,pauseFit);
        lastWidth=Screen.width;lastHeight=Screen.height;lastSafe=area;
    }
    private void Update()
    {
        if(stage==null)return;
        if(Screen.width!=lastWidth||Screen.height!=lastHeight||Screen.safeArea!=lastSafe)Layout();
        bool external=CursorDashRuntimeUI.Instance!=null&&CursorDashRuntimeUI.Instance.PanelOpen;
        bool menu=stage.Space!=null&&stage.Space.IsMenu&&!external;
        if(!menu){open=false;sheet=0;dragging=false;}
        paused=stage.State==StageManager.RoundState.Paused&&!external;
        if(Input.GetKeyDown(KeyCode.Escape)&&!external){
            if(menu){if(open||sheet>.001f)SetSheet(false);}
            else if(paused)Resume();else if(!stage.IsDeath&&stage.State!=StageManager.RoundState.Starting)stage.SetPaused(true);
        }
        if(releaseInput&&!PointerHeld()&&!Input.GetKey(KeyCode.Escape)&&!SteeringHeld())releaseInput=false;
        if(!dragging)sheet=Mathf.MoveTowards(sheet,open?1:0,Time.unscaledDeltaTime/Mathf.Max(.01f,.24f));
        float eased=sheet*sheet*(3-2*sheet);
        sheetRect.anchoredPosition=new Vector2(0,-(sheetHeight-48)*(1-eased));sheetRoot.SetActive(menu);
        sheetContent.alpha=eased;sheetContent.interactable=sheet>.98f;sheetContent.blocksRaycasts=sheet>.98f;
        pauseAlpha=Mathf.MoveTowards(pauseAlpha,paused?1:0,Time.unscaledDeltaTime/.16f);
        pauseRoot.SetActive(pauseAlpha>.001f);pauseGroup.alpha=pauseAlpha;pauseGroup.interactable=paused;pauseGroup.blocksRaycasts=paused;
        pauseCard.anchoredPosition=new Vector2(0,Mathf.Lerp(-9,0,pauseAlpha));
        shade.color=new Color(.01f,.02f,.045f,Mathf.Max(menu?eased*.32f:0,pauseAlpha*.82f));shade.raycastTarget=menu&&sheet>.001f||paused;
        if(launchTutorial&&menu&&sheet<=0&&!releaseInput&&!PointerHeld()){
            launchTutorial=false;CursorDashSession.ReplayTutorial=true;CursorDashSession.CurrentMode=CursorDashSession.Mode.Tutorial;stage.Space.LaunchSelectedMode();
        }
    }
    private static bool PointerHeld()=>Input.GetMouseButton(0)||Input.touchCount>0;
    private static bool SteeringHeld()=>Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.D);
    public static bool HitsHandle(Vector2 point)=>Instance!=null&&Instance.sheetRoot.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(Instance.handle,point);
    public void SetSheet(bool value){if(stage?.Space?.IsMenu!=true)return;open=value;releaseInput=true;stage.ResetPresentationInput();if(!value)Save();}
    public void BeginHandle(Vector2 screen)
    {
        if(stage?.Space?.IsMenu!=true)return;dragging=true;releaseInput=true;dragOrigin=dragY=screen.y;dragTime=Time.unscaledTime;dragValue=sheet;velocity=0;
    }
    public void DragHandle(Vector2 screen)
    {
        if(!dragging)return;float pixels=Mathf.Max(1,sheetHeight*root.GetComponent<Canvas>().scaleFactor-48);
        float delta=screen.y-dragY,dt=Mathf.Max(.008f,Time.unscaledTime-dragTime);velocity=delta/dt/pixels;
        sheet=Mathf.Clamp01(dragValue+(screen.y-dragOrigin)/pixels);dragY=screen.y;dragTime=Time.unscaledTime;
    }
    public void EndHandle(bool moved)
    {
        if(!dragging)return;dragging=false;SetSheet(moved?(Mathf.Abs(velocity)>.45f?velocity>0:sheet>.45f):!open);
    }
    public void SetValue(string key,float value)
    {
        if(!CursorDashTuning.TrySet(key,new JValue(value),out string error,true)){SaveError=error;return;}dirty=true;
    }
    public void Sync()
    {
        foreach(var row in rows){float value=(float)CursorDashTuning.Requested(row.key);row.slider.SetValueWithoutNotify(value);row.value.text=Mathf.RoundToInt(value*100)+"%";}
    }
    public void Save(){if(!dirty)return;if(CursorDashTuning.SaveUser(out string error)){dirty=false;SaveError="";}else {SaveError=error;Debug.LogWarning("CursorDash settings: "+error);}}
    public void ResetSettings(){CursorDashTuning.ResetAudioVisual();dirty=true;Save();}
    public void ReplayTutorial(){if(launchTutorial||stage?.Space?.IsMenu!=true)return;launchTutorial=true;SetSheet(false);}
    public void Resume(){if(stage==null)return;Save();releaseInput=true;stage.SetPaused(false);stage.ResetPresentationInput();EventSystem.current?.SetSelectedGameObject(null);}
    public void Retry(){Save();releaseInput=true;stage.Retry();}
    public void Home(){Save();releaseInput=true;stage.ReturnHome();}
    private void OnApplicationPause(bool paused){if(paused)Save();}
    private void OnDestroy(){Save();CursorDashTuning.Changed-=Sync;if(Instance==this)Instance=null;}
}

/// <summary>Only the handle owns sheet drag. Sliders never bubble a sheet gesture.</summary>
public sealed class CursorDashSheetHandle : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    public CursorDashSettingsUI owner;private int finger=int.MinValue;private Vector2 start;private bool moved;
    public void OnPointerDown(PointerEventData e){if(finger!=int.MinValue)return;finger=e.pointerId;start=e.position;moved=false;owner.BeginHandle(e.position);e.Use();}
    public void OnDrag(PointerEventData e){if(e.pointerId!=finger)return;moved|=(e.position-start).sqrMagnitude>64;owner.DragHandle(e.position);e.Use();}
    public void OnPointerUp(PointerEventData e){if(e.pointerId!=finger)return;finger=int.MinValue;owner.EndHandle(moved);e.Use();}
    private void OnDisable(){if(finger==int.MinValue)return;finger=int.MinValue;owner.EndHandle(true);}
}

public sealed class CursorDashSettingSlider : Slider
{
    public Action finished;
    public override void OnPointerUp(PointerEventData e){base.OnPointerUp(e);finished?.Invoke();}
    public override void OnDeselect(BaseEventData e){base.OnDeselect(e);finished?.Invoke();}
    public override void OnMove(AxisEventData e){base.OnMove(e);finished?.Invoke();}
}
