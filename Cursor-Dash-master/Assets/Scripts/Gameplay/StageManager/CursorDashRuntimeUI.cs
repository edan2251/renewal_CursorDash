using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

/// <summary>One command/metadata surface shared by the Editor overlay and touch WebGL HTML.</summary>
[Preserve]
public sealed class CursorDashRuntimeUI : MonoBehaviour
{
    public static CursorDashRuntimeUI Instance {get;private set;}
    public static bool InputBlocked=>Instance!=null&&(Instance.PanelOpen||Instance.webFocus||Instance.webCapture);
    public bool PanelOpen {get;private set;}
    public bool DeveloperPanel {get;private set;}
    public string Message {get;private set;}="";
    public string Transfer {get;private set;}="";
    public string PresetName {get;private set;}="현재 세션";
    private bool webFocus,webCapture,htmlReady;
    private float nextSnapshot,baseBgm,baseSfx,frameMs;
    private bool soundCaptured;
    private Vector2 scroll;
    private string search="",edit="",selected="circle.dash",nameInput="나의 프리셋";
    private Action deferred;
    private Font font;
    private GUISkin ownedSkin;
    private ArrowNode reflectionTrial;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]private static extern int CDPanelAttach(string objectName);
    [DllImport("__Internal")]private static extern void CDPanelReceive(string json);
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Ensure(){if(Instance==null)new GameObject("CursorDashTools").AddComponent<CursorDashRuntimeUI>();}
    private void Awake(){if(Instance!=null){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);SceneManager.sceneLoaded+=SceneLoaded;}
    private void SceneLoaded(Scene scene,LoadSceneMode mode){PanelOpen=false;webFocus=webCapture=false;nextSnapshot=0;Time.timeScale=1;}
    private void Update(){
        if(deferred!=null){var a=deferred;deferred=null;a();}
        if(reflectionTrial!=null){var stage=StageManager.instance;if(!CursorDashTuning.DevEnabled||stage==null||stage.IsDeath||!reflectionTrial.gameObject.activeSelf||reflectionTrial.BlocksThisExecution>0)reflectionTrial=null;else if(stage.IsPlaying&&reflectionTrial.Phase.ToString()=="Fired"){Vector2 origin=reflectionTrial.transform.position;Vector2 delta=stage.NexusPosition-origin;Vector2 contact=origin+delta*.5f;Vector2 side=new Vector2(-delta.y,delta.x).normalized*.6f;PlayerCharacterAfterImage.instance?.RecordMovement(contact-side,contact+side,Time.time);}}
        frameMs=Mathf.Lerp(frameMs,Time.unscaledDeltaTime*1000,.05f);
        if(GameManager.instance!=null&&GameManager.instance.soundManager!=null){var s=GameManager.instance.soundManager;if(!soundCaptured){baseBgm=s.BGMVolume;baseSfx=s.SFXVolume;soundCaptured=true;}s.ApplyRuntimeVolumes(CursorDashTuning.Number("user.bgm",baseBgm),CursorDashTuning.Number("user.sfx",baseSfx));}
#if UNITY_WEBGL && !UNITY_EDITOR
        if(!htmlReady){htmlReady=CDPanelAttach(gameObject.name)!=0;UnityEngine.WebGLInput.captureAllKeyboardInput=false;}
        if(htmlReady&&Time.unscaledTime>=nextSnapshot){nextSnapshot=Time.unscaledTime+.25f;CDPanelReceive(Snapshot().ToString(Formatting.None));}
#endif
    }
    public void Open(bool developer){if(developer)CursorDashTuning.EnableDev(true);DeveloperPanel=developer;PanelOpen=true;StageManager.instance?.SetPanelPaused(true);ResetInput();nextSnapshot=0;}
    public void Close(){PanelOpen=false;webFocus=webCapture=false;StageManager.instance?.SetPanelPaused(false);ResetInput();nextSnapshot=0;}
    private static void ResetInput(){FindFirstObjectByType<VirtualJoyStick>()?.ResetInput();PlayerCharacterController.instance?.MoveStop();}
    public static bool IsScreenPointBlocked(Vector2 screen){if(InputBlocked)return true;if(Instance==null||Instance.htmlReady)return false;float scale=Mathf.Max(1,Screen.width/900f);float height=StageManager.instance?.GetComponent<CursorDashPractice>()!=null?165:52;return new Rect(0,0,Screen.width,height*scale).Contains(new Vector2(screen.x,Screen.height-screen.y));}
    private void Load(CursorDashSession.Mode mode,bool home=false){Close();CursorDashSession.CurrentMode=mode;if(mode==CursorDashSession.Mode.Normal)CursorDashTuning.EnableDev(false);if(StageManager.instance!=null)StageManager.instance.ChangeScene(home?"01.StageSelectScene":"Cyberpunk");else SceneManager.LoadScene(home?"01.StageSelectScene":"Cyberpunk");}
    public void SafeTrial(){
        var stage=StageManager.instance;if(stage==null||!CursorDashTuning.DevEnabled){Message="개발자 전투 장면에서 실행하세요";return;}
        // Safe rebuild is explicit. Existing in-flight attacks are removed, never silently retimed.
        var originals=FindObjectsByType<Node>(FindObjectsSortMode.None).Where(n=>n.gameObject.activeSelf).Select(n=>new {kind=n is NormalNode?0:1,pos=(Vector2)n.transform.position}).ToArray();
        stage.NodeCreator.StopNodes();stage.Feedback.Clear();PlayerCharacterAfterImage.instance?.ApplyTuning();
        foreach(var item in originals)stage.NodeCreator.SpawnPreviewAt(item.kind,item.pos);
        if(originals.Length==0)stage.NodeCreator.SpawnPreviewAt(0,new Vector2(1,2));
        stage.SetDevPaused(false);Close();Message="안전 재생성 완료. 다음 라운드 항목은 재시작 필요.";
    }
    [Preserve] public void BrowserCommand(string json){
        if(json==null||json.Length>150000){Message="명령 크기 초과";return;}
        try{var q=JObject.Parse(json,new JsonLoadSettings{DuplicatePropertyNameHandling=DuplicatePropertyNameHandling.Error});string cmd=(string)q["cmd"],error="";bool ok=true;var stage=StageManager.instance;var practice=stage?.GetComponent<CursorDashPractice>();
            switch(cmd){
            case "settings":Open(false);break;case "dev":Open(true);break;case "close":Close();break;
            case "training":deferred=()=>Load(CursorDashSession.Mode.Training);break;case "tutorial":deferred=()=>Load(CursorDashSession.Mode.Tutorial);break;
            case "normal":deferred=()=>Load(CursorDashSession.Mode.Normal);break;case "home":deferred=()=>Load(CursorDashSession.Mode.Normal,true);break;
            case "skip":practice?.Skip();break;
            case "spawnCircle":if(practice!=null)practice.Spawn(0);else if(CursorDashTuning.DevEnabled)stage?.NodeCreator.SpawnPreviewAt(0,new Vector2(1,2));break;
            case "spawnHex":if(practice!=null)practice.Spawn(1);else if(CursorDashTuning.DevEnabled)stage?.NodeCreator.SpawnPreviewAt(1,new Vector2(0,2.3f));break;
            case "repeatCircle":if(practice!=null)practice.RepeatCircle=!practice.RepeatCircle;break;case "repeatHex":if(practice!=null)practice.RepeatHex=!practice.RepeatHex;break;
            case "clear":if(practice!=null)practice.Clear();else if(CursorDashTuning.DevEnabled){stage?.NodeCreator.StopNodes();stage?.Feedback.Clear();PlayerCharacterAfterImage.instance?.ClearTrail();}break;
            case "restart":if(stage!=null)deferred=()=>{Close();stage.Retry();};break;
            case "invulnerable":if(practice!=null)stage.PracticeInvulnerable=!stage.PracticeInvulnerable;break;
            case "heal":if(practice!=null||CursorDashTuning.DevEnabled){stage?.DisqualifyRecord();stage?.Heal();}break;
            case "damage":if(CursorDashTuning.DevEnabled&&stage!=null){stage.DisqualifyRecord();stage.DamageNexus(1);}break;
            case "reflectTrial":RequireDev();if(stage!=null){reflectionTrial=stage.NodeCreator.SpawnPreviewAt(1,new Vector2(0,2.3f)) as ArrowNode;Close();Message="내부 잔상 생성으로 실제 빔 방어를 재현합니다. 실제 손가락 입력 검증은 아닙니다.";}break;
            case "circleTrial":RequireDev();if(stage!=null&&PlayerCharacterController.instance!=null){stage.NodeCreator.SpawnPreviewAt(0,PlayerCharacterController.instance.transform.position);Close();Message="플레이어 위치에 원형을 생성해 실제 접촉 판정을 재현합니다.";}break;
            case "userPause":stage?.SetPaused(true);break;case "userResume":stage?.SetPaused(false);break;
            case "hp":if((practice!=null||CursorDashTuning.DevEnabled)&&stage!=null){double hp=(double)q["value"];if(double.IsNaN(hp)||double.IsInfinity(hp)||hp<0||hp>stage.scoreManager.MaxHp)throw new Exception("HP 범위 오류");stage.DisqualifyRecord();stage.scoreManager.Hp=(float)hp;}break;
            case "pause":if(CursorDashTuning.DevEnabled)stage?.SetDevPaused(true);else stage?.SetPaused(true);break;
            case "resume":if(CursorDashTuning.DevEnabled)stage?.SetDevPaused(false);else stage?.SetPaused(false);break;
            case "devOff":CursorDashTuning.EnableDev(false);DeveloperPanel=false;Message="Dev 해제. 현재 라운드의 기록 제외는 유지됩니다.";break;
            case "preview":Close();break;case "safeTrial":SafeTrial();break;
            case "feverKill":case "feverFill":case "feverStep":case "feverHit":case "feverAbort":
                RequireDev();stage?.Fever?.Review(cmd=="feverKill"?"kill":cmd=="feverFill"?"fill":cmd=="feverStep"?"step":cmd=="feverHit"?"hit":"abort");break;
            case "set":ok=CursorDashTuning.TrySet((string)q["key"],q["value"],out error,!DeveloperPanel);PresetName="미저장 변경";break;
            case "reset":RequireDev();CursorDashTuning.Reset((string)q["key"]);break;
            case "resetGroup":RequireDev();CursorDashTuning.Reset(null,(string)q["group"]);break;
            case "resetAll":RequireDev();CursorDashTuning.Reset();break;
            case "visualPreset":RequireDev();ok=CursorDashTuning.ApplyVisualPreset((string)q["name"],out error);if(ok)PresetName=(string)q["name"]=="strong"?"강렬한 노드 시안":"CP14 타격감 비교 기준";break;
            case "resetUser":CursorDashTuning.ResetUser();break;
            case "userSave":ok=CursorDashTuning.SaveUser(out error);break;
            case "savePreset":RequireDev();ok=CursorDashTuning.SavePreset((string)q["name"],out error);if(ok)PresetName=(string)q["name"];break;
            case "loadPreset":RequireDev();ok=CursorDashTuning.LoadPreset((string)q["name"],out error);if(ok)PresetName=(string)q["name"];break;
            case "duplicatePreset":RequireDev();ok=CursorDashTuning.DuplicatePreset((string)q["name"],(string)q["target"],out error);break;
            case "deletePreset":RequireDev();ok=CursorDashTuning.DeletePreset((string)q["name"],out error);break;
            case "export":Transfer=CursorDashTuning.Export(PresetName);break;case "exportChanges":Transfer=CursorDashTuning.Export(PresetName,true);break;
            case "report":Transfer=CursorDashTuning.ChangedReport();break;case "import":RequireDev();ok=CursorDashTuning.Import((string)q["text"],out error);if(ok)PresetName="가져온 설정";break;
            case "focus":webFocus=(bool?)q["value"]??false;ResetInput();break;case "capture":webCapture=(bool?)q["value"]??false;ResetInput();break;
            default:throw new Exception("지원하지 않는 명령");
            }if(!ok)Message=error;else if(cmd!="focus"&&cmd!="capture"&&cmd!="devOff"&&cmd!="safeTrial")Message="완료: "+cmd;nextSnapshot=0;
        }catch(Exception e){Message="요청 거부: "+e.Message;nextSnapshot=0;}
    }
    private static void RequireDev(){if(!CursorDashTuning.DevEnabled)throw new Exception("Dev 활성화 필요");}
    public JObject Snapshot(){
        var s=StageManager.instance;var p=s?.GetComponent<CursorDashPractice>();var trail=PlayerCharacterAfterImage.instance;
        var data=new JObject{["buildId"]=CursorDashTuning.BuildId,["defaultsVersion"]=CursorDashTuning.DefaultsVersion,["scene"]=SceneManager.GetActiveScene().name,["mode"]=CursorDashSession.CurrentMode.ToString(),["panel"]=PanelOpen?(DeveloperPanel?"dev":"user"):"closed",["dev"]=CursorDashTuning.DevEnabled,["preset"]=PresetName,["eligible"]=s!=null&&s.RecordEligible,["message"]=Message,["transfer"]=Transfer,["presets"]=new JArray(CursorDashTuning.PresetNames()),["step"]=p==null?-1:p.Step,["instruction"]=p?.Instruction??"",["tutorialStatus"]=CursorDashStorage.Get(CursorDashTuning.TutorialKey,"new"),["saveError"]=CursorDashSession.LastSaveError,
        ["hp"]=s?.scoreManager.Hp??0,["maxHp"]=s?.scoreManager.MaxHp??0,["nodes"]=s?.NodeCreator.ActiveCount??0,["trails"]=trail?.ActiveSegmentCount??0,["trailCapacity"]=trail?.Capacity??0,["timeScale"]=Time.timeScale,["devPaused"]=s!=null&&s.DevPaused,["fps"]=frameMs>0?1000/frameMs:0,["frameMs"]=frameMs,["radius"]=s?.NexusRadius??0,["distortionFrames"]=s?.Feedback.Distortion.RenderedEffectFrames??0,["repeatCircle"]=p!=null&&p.RepeatCircle,["repeatHex"]=p!=null&&p.RepeatHex,["invulnerable"]=p!=null&&s.PracticeInvulnerable};
        data["storageError"]=CursorDashStorage.LastError;data["tuningMessage"]=CursorDashTuning.LastMessage;data["liveRadius"]=s?.MeasuredBodyRadius??0;
        if(s?.Fever?.Model!=null){var f=s.Fever.Model;data["feverB"]=new JObject{["points"]=f.PointsText,["multiplier"]=f.Multiplier,["state"]=f.Current.ToString(),["gauge"]=f.Gauge,["paused"]=f.Paused,["completed"]=f.CompletedFevers,["combatConnected"]=true,["bonusCount"]=s.Fever.ActiveBonuses,["normalCombatSeconds"]=f.NormalCombatSeconds,["clearedThreats"]=s.Fever.LastClearedThreats,["worldSupported"]=s.Fever.World!=null&&s.Fever.World.Supported,["worldFrames"]=s.Fever.World==null?0:s.Fever.World.RenderedFrames};}
        data["pointsSaveError"]=CursorDashPointsRecord.LastError;
        if(PlayerCharacterController.instance!=null){data["playerX"]=PlayerCharacterController.instance.transform.position.x;data["playerY"]=PlayerCharacterController.instance.transform.position.y;}
        var events=new JArray();if(s!=null)foreach(var e in s.Feedback.Recent)events.Add(new JObject{["kind"]=e.kind,["sequence"]=e.sequence,["x"]=e.point.x,["y"]=e.point.y,["time"]=e.realTime});data["events"]=events;
        var list=new JArray();foreach(var v in CursorDashTuning.Parameters){var requested=CursorDashTuning.Requested(v.key);var effective=CursorDashTuning.Effective(v.key);list.Add(new JObject{["key"]=v.key,["label"]=v.label,["group"]=v.group,["type"]=v.type,["unit"]=v.unit,["min"]=v.min,["max"]=v.max,["step"]=v.step,["default"]=v.defaultValue.DeepClone(),["requested"]=requested.DeepClone(),["effective"]=effective.DeepClone(),["apply"]=v.apply,["scope"]=v.scope,["description"]=v.description,["changed"]=!CursorDashTuning.Equivalent(v.key,requested,v.defaultValue),["pending"]=!CursorDashTuning.HasConsumed(v.key)||!CursorDashTuning.Equivalent(v.key,requested,effective)});}data["parameters"]=list;return data;
    }
    private void Command(string command){BrowserCommand(new JObject{["cmd"]=command}.ToString());}
    private void OnGUI(){
#if UNITY_WEBGL && !UNITY_EDITOR
        return; // HTML owns Web input; do not request desktop OS fonts, even during initial attach.
#else
        if(htmlReady)return;var original=GUI.skin;if(ownedSkin==null){ownedSkin=Instantiate(original);font=CursorDashTypography.MediumFont;ownedSkin.font=font!=null?font:original.font;}GUI.skin=ownedSkin;try{DrawGUI();}finally{GUI.skin=original;}
#endif
    }
    private void DrawGUI(){
        if(htmlReady)return;
        float scale=Mathf.Max(1,Screen.width/900f);GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));float w=Screen.width/scale,h=Screen.height/scale;
        GUI.Box(new Rect(0,0,w,50),CursorDashTuning.DevEnabled?"DEV · 기록 제외 · "+PresetName:CursorDashSession.IsPractice?"훈련 · 기록 제외":"CursorDash");
        if(GUI.Button(new Rect(8,8,85,34),"설정"))Open(false);if(GUI.Button(new Rect(98,8,85,34),"훈련"))Command("training");if(GUI.Button(new Rect(188,8,85,34),"가이드"))Command("tutorial");
        var practice=StageManager.instance?.GetComponent<CursorDashPractice>();
        if(!PanelOpen&&practice!=null){GUI.Box(new Rect(8,58,w-16,104),practice.Instruction);if(GUI.Button(new Rect(16,94,95,32),"원형 생성"))Command("spawnCircle");if(GUI.Button(new Rect(116,94,95,32),"육각 생성"))Command("spawnHex");if(GUI.Button(new Rect(216,94,95,32),"건너뛰기"))Command("skip");if(GUI.Button(new Rect(316,94,95,32),"개발자"))Open(true);}
        if(!PanelOpen)return;
        GUI.Box(new Rect(8,54,w-16,h-62),DeveloperPanel?"개발자 수치 — 원본 저장 없음":"사용자 설정 — 일시정지");
        if(GUI.Button(new Rect(w-100,60,80,32),"닫기"))Close();if(GUI.Button(new Rect(20,82,110,32),"개발자 도구"))Open(true);if(GUI.Button(new Rect(136,82,100,32),"안전 재생성"))Command("safeTrial");if(GUI.Button(new Rect(242,82,90,32),"설정 저장"))Command("userSave");
        if(DeveloperPanel){if(GUI.Button(new Rect(338,82,145,32),"CP14 타격감 기준"))BrowserCommand("{\"cmd\":\"visualPreset\",\"name\":\"baseline\"}");if(GUI.Button(new Rect(490,82,145,32),"강렬한 노드 시안"))BrowserCommand("{\"cmd\":\"visualPreset\",\"name\":\"strong\"}");}
        search=GUI.TextField(new Rect(20,120,210,30),search);GUI.Label(new Rect(240,120,w-260,40),Message);
        var rows=CursorDashTuning.Parameters.Where(v=>(DeveloperPanel||v.scope=="사용자")&&(v.key.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0||v.label.Contains(search)||v.group.Contains(search))).ToArray();
        scroll=GUI.BeginScrollView(new Rect(20,156,w-40,Mathf.Max(100,h-350)),scroll,new Rect(0,0,w-70,rows.Length*72));
        for(int i=0;i<rows.Length;i++){var v=rows[i];float y=i*72;GUI.Label(new Rect(0,y,260,25),v.label+" ["+v.key+"]");GUI.Label(new Rect(0,y+26,265,30),CursorDashTuning.Requested(v.key)+" "+v.unit+" / "+v.apply);
            if(GUI.Button(new Rect(270,y,65,30),"편집")){selected=v.key;edit=CursorDashTuning.Requested(v.key).ToString();}
            if(v.type=="toggle"){if(GUI.Button(new Rect(340,y,90,30),"전환"))Set(v.key,new JValue(!(bool)CursorDashTuning.Requested(v.key)));}
            else if(v.type!="color"){float n=(float)CursorDashTuning.Requested(v.key);float value=GUI.HorizontalSlider(new Rect(340,y+13,Mathf.Max(80,w-440),24),n,(float)v.min,(float)v.max);if(Mathf.Abs(n-value)>.00001f)Set(v.key,new JValue(v.type=="integer"||v.type=="curve"?Mathf.Round(value):value));}
        }GUI.EndScrollView();
        float b=h-180;GUI.Label(new Rect(20,b,220,28),selected);edit=GUI.TextField(new Rect(240,b,140,28),edit);if(GUI.Button(new Rect(386,b,70,28),"적용")){string error;CursorDashTuning.TrySetText(selected,edit,out error,!DeveloperPanel);Message=error;}
        nameInput=GUI.TextField(new Rect(20,b+34,160,28),nameInput);if(GUI.Button(new Rect(186,b+34,80,28),"프리셋 저장"))BrowserCommand(new JObject{["cmd"]="savePreset",["name"]=nameInput}.ToString());if(GUI.Button(new Rect(272,b+34,80,28),"JSON"))Command("export");if(GUI.Button(new Rect(358,b+34,80,28),"보고서"))Command("report");if(GUI.Button(new Rect(444,b+34,80,28),"복사")){GUIUtility.systemCopyBuffer=Transfer;Message="클립보드 전송 요청. 아래 텍스트도 선택할 수 있습니다.";}
        Transfer=GUI.TextArea(new Rect(20,b+68,w-40,64),Transfer);if(GUI.Button(new Rect(20,h-38,100,28),"JSON 가져오기"))BrowserCommand(new JObject{["cmd"]="import",["text"]=Transfer}.ToString());
    }
    private void Set(string key,JToken value){BrowserCommand(new JObject{["cmd"]="set",["key"]=key,["value"]=value}.ToString());}
    private void OnDestroy(){SceneManager.sceneLoaded-=SceneLoaded;if(ownedSkin!=null){ownedSkin.font=null;Destroy(ownedSkin);}if(Instance==this)Instance=null;}
}
