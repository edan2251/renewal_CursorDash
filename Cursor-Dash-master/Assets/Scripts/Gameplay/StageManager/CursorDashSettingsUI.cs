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
    private sealed class SettingRow {public string key;public Slider slider;public TMP_Text value;}

    public void Initialize(StageManager owner)
    {
        stage=owner;Instance=this;CursorDashRuntimeUI.Instance?.RefreshAudio();Build();CursorDashTuning.Changed+=Sync;Sync();Layout();
    }
    private CursorDashSettingsView view;
    private void Build()
    {
        view=stage.SettingsView!=null?stage.SettingsView:CursorDashSettingsView.CreateDefault(transform,CursorDashTypography.Medium);
        view.PrepareRendering();view.Present(stage.Space!=null&&stage.Space.IsMenu,0,0,false);view.Canvas.enabled=true;
        root=view.Root;safe=view.SafeArea;sheetRect=view.SheetAnimation;content=view.ContentAnimation;
        shade=view.Shade;sheetContent=view.ContentOpacity;handle=view.Handle;pauseGroup=view.PauseOpacity;pauseCard=view.PauseAnimation;
        sheetRoot=sheetRect.gameObject;pauseRoot=pauseGroup.gameObject;
        foreach(var row in view.Rows)rows.Add(new SettingRow{key=row.key,slider=row.slider,value=row.value});
        view.Bind(this);
    }
    private void Layout()
    {
        var area=Screen.safeArea;sheetHeight=view.Layout(area,new Vector2(Screen.width,Screen.height));
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
        if(!dragging)sheet=Mathf.MoveTowards(sheet,open?1:0,Time.unscaledDeltaTime/Mathf.Max(.01f,view.PanelTravelSeconds));
        pauseAlpha=Mathf.MoveTowards(pauseAlpha,paused?1:0,Time.unscaledDeltaTime/.16f);
        view.Present(menu,sheet,pauseAlpha,true);
        // Interaction follows the existing state, even while the exit fade remains visible.
        pauseGroup.interactable=paused;pauseGroup.blocksRaycasts=paused;
        shade.raycastTarget=menu&&sheet>.001f||paused;
        if(launchTutorial&&menu&&sheet<=0&&!releaseInput&&!PointerHeld()){
            launchTutorial=false;CursorDashSession.ReplayTutorial=true;CursorDashSession.CurrentMode=CursorDashSession.Mode.Tutorial;stage.Space.LaunchSelectedMode();
        }
    }
    private static bool PointerHeld()=>Input.GetMouseButton(0)||Input.touchCount>0;
    private static bool SteeringHeld()=>Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.D);
    public static bool HitsHandle(Vector2 point)=>Instance!=null&&Instance.sheetRoot.activeInHierarchy&&Instance.view.ContainsNavigationPoint(point);
    public void SetSheet(bool value){if(stage?.Space?.IsMenu!=true)return;open=value;releaseInput=true;stage.ResetPresentationInput();if(!value)Save();}
    public void OpenPanel(CursorDashSettingsView.MenuPanel panel){if(stage?.Space?.IsMenu!=true)return;SelectPanel(panel);SetSheet(true);}
    private void SelectPanel(CursorDashSettingsView.MenuPanel panel)
    {
        view.SelectPanel(panel);
        if(panel==CursorDashSettingsView.MenuPanel.Ranking){var map=GameManager.instance?.MapInformation;view.SetRankingBest(map==null?"0":CursorDashPointsRecord.Read(map.mapName).ToString(System.Globalization.CultureInfo.InvariantCulture));}
    }
    public void BeginHandle(Vector2 screen){BeginHandle(screen,CursorDashSettingsView.MenuPanel.Settings);}
    public void BeginHandle(Vector2 screen,CursorDashSettingsView.MenuPanel panel)
    {
        if(stage?.Space?.IsMenu!=true)return;SelectPanel(panel);dragging=true;releaseInput=true;dragOrigin=dragY=screen.y;dragTime=Time.unscaledTime;dragValue=sheet;velocity=0;
    }
    public void DragHandle(Vector2 screen)
    {
        if(!dragging)return;float pixels=Mathf.Max(1,(sheetHeight-view.NavigationHeight)*root.GetComponent<Canvas>().scaleFactor);
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
        view.ApplyMenuLanguage(CursorDashTuning.MenuLanguage);
    }
    public void Save(){if(!dirty)return;if(CursorDashTuning.SaveUser(out string error)){dirty=false;SaveError="";}else {SaveError=error;Debug.LogWarning("CursorDash settings: "+error);}}
    public void ResetSettings(){CursorDashTuning.ResetAudioVisual();dirty=true;Save();}
    public void ResetAudio(){CursorDashTuning.ResetAudio();dirty=true;Save();}
    public void ResetEffects(){CursorDashTuning.ResetVisual();dirty=true;Save();}
    public void CycleMenuLanguage(){CursorDashTuning.CycleMenuLanguage();dirty=true;Save();}
    public void ReplayTutorial(){if(launchTutorial||stage?.Space?.IsMenu!=true)return;launchTutorial=true;SetSheet(false);}
    public void Resume(){if(stage==null)return;Save();releaseInput=true;stage.SetPaused(false);stage.ResetPresentationInput();EventSystem.current?.SetSelectedGameObject(null);}
    public void Retry(){Save();releaseInput=true;stage.Retry();}
    public void Home(){Save();releaseInput=true;stage.ReturnHome();}
    private void OnApplicationPause(bool paused){if(paused)Save();}
    private void OnDestroy(){view?.Unbind();Save();CursorDashTuning.Changed-=Sync;if(Instance==this)Instance=null;}
}
