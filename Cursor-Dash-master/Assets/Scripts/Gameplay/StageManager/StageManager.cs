using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class StageManager : MonoBehaviour
{
    public enum RoundState { Playing, Paused, Ended }
    public static StageManager instance;
    public ScoreManager scoreManager;

    [SerializeField] private Image hpBarImage;
    [SerializeField] private Image hpBarFrame;
    [SerializeField] private Text timeScoreText;
    [SerializeField] private TMP_Text timeScoreLabel;
    [SerializeField] private DeathUIController deathUIController;
    [SerializeField] private PauseHandler pauseHandler;
    [SerializeField] private ParticleSystem coreDeathAnimation;

    private NodeCreator nodeCreator;
    private CameraFlash cameraFlash;
    private CameraShake cameraShake;
    private VirtualJoyStick stick;
    private Coroutine hitStopRoutine;
    private float hitStopUntil;
    private float stopBurstStart,stopCooldownUntil;
    private bool panelPaused,devPaused;
    private bool boundaryReady;
    private float activeNexusFraction;
    private Vector2 nexusPosition;
    private float nexusRadius;
    private HitFeedback feedback;
    private NexusEnergy energy;
    private NexusGameboardPresentation nexusPresentation;
    private NexusImpactVfx finale;
    public CursorDashFever Fever {get;private set;}
    private Coroutine resultRoutine;
    private bool pendingLethal;
    private Vector2 lastContact,lastIncoming;
    private NexusImpactVfx.Cause lastCause;
    public NexusImpactVfx Finale=>finale;
    public NexusGameboardPresentation NexusPresentation=>nexusPresentation;
    public HitFeedback Feedback=>feedback;
    public NexusEnergy Energy=>energy;
    public bool RecordEligible {get;private set;}
    public bool PracticeInvulnerable {get;set;}=true;
    public bool DevPaused=>devPaused;
    public bool NormalThreatsSuppressed=>Fever!=null&&Fever.Model!=null&&Fever.Model.SuppressesThreats;
    public double NormalCombatTime=>Fever==null?scoreManager.TimeScore:Fever.Model.NormalCombatSeconds;
    private RoundState state = RoundState.Playing;

    public NodeCreator NodeCreator => nodeCreator;
    public RoundState State => state==RoundState.Playing&&!IsPlaying?RoundState.Paused:state;
    public bool IsDeath => state == RoundState.Ended;
    public bool IsPlaying => boundaryReady && state == RoundState.Playing && !panelPaused && !devPaused;
    public int DestroyedNodes { get; private set; }
    public int BlockedBeams { get; private set; }
    public int HexDestroyed {get;private set;}
    public Vector2 NexusPosition => nexusPosition;
    public float NexusRadius => nexusRadius;
    public float MeasuredBodyRadius {get{if(hpBarImage==null)return nexusRadius;var rect=hpBarImage.rectTransform;return Mathf.Min(Mathf.Abs(rect.rect.width*rect.lossyScale.x),Mathf.Abs(rect.rect.height*rect.lossyScale.y))*.5f*activeNexusFraction;}}

    [Header("Circular dash draft boundary (solid body, excluding glow)")]
    [SerializeField, Range(0.1f, 1f)] private float normalNexusSolidFraction = 125f / 211f;
    // Compatibility aliases; both attacks now share one stable, measured body boundary.
    public Vector2 NormalDashNexusPosition => NexusPosition;
    public float NormalDashNexusRadius => NexusRadius;

    private void Awake()
    {
        instance = this;
        scoreManager = GetComponent<ScoreManager>();
        nodeCreator = GetComponent<NodeCreator>();
        cameraFlash = GetComponent<CameraFlash>();
        cameraShake = GetComponent<CameraShake>();
        stick = FindFirstObjectByType<VirtualJoyStick>();
        RecordEligible=!CursorDashSession.IsPractice&&!CursorDashTuning.DevEnabled;
        CursorDashTuning.RuleSessionChanged+=DisqualifyRecord;
        CursorDashTuning.Changed+=RefreshRuntimeClock;
        feedback=gameObject.AddComponent<HitFeedback>();
        Canvas.ForceUpdateCanvases();
        float fraction=CursorDashTuning.Number("nexus.solidFraction",normalNexusSolidFraction);
        activeNexusFraction=fraction;
        if(hpBarImage!=null){
            var rect=hpBarImage.rectTransform;
            nexusPosition=rect.TransformPoint(rect.rect.center);
            nexusRadius=Mathf.Min(Mathf.Abs(rect.rect.width*rect.lossyScale.x),Mathf.Abs(rect.rect.height*rect.lossyScale.y))*.5f*fraction;
        }
        energy=gameObject.AddComponent<NexusEnergy>();energy.Initialize(hpBarImage,fraction);
        nexusPresentation=gameObject.AddComponent<NexusGameboardPresentation>();nexusPresentation.Initialize(hpBarImage,hpBarFrame,fraction,feedback.Distortion);
        if(hpBarFrame!=null)hpBarFrame.gameObject.AddComponent<NexusRingRhythm>().Initialize(hpBarFrame.GetComponent<Rotator>());
        Fever=gameObject.AddComponent<CursorDashFever>();Fever.Initialize(this,hpBarFrame,timeScoreLabel,timeScoreText);
        scoreManager.Damaged+=OnNexusHealthLost;
        scoreManager.SetHpEditAction(SetHpImage);
        scoreManager.SetDeathAction(Death);
        scoreManager.SetScoreAction(SetTimeScore);
        GameManager.instance.CurrentSceneType = GameManager.SceneType.InGame;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        // Directly opening the gameplay scene in the Editor follows the same map setup.
        GameManager manager = GameManager.instance;
        string sceneName = SceneManager.GetActiveScene().name;
        if (manager.MapInformation == null || manager.MapInformation.mapName != sceneName)
        {
            foreach (MapInformation map in manager.Maps)
                if (map.mapName == sceneName) { manager.MapInformation = map; break; }
        }
        if (manager.MapInformation == null)
        {
            Debug.LogError("CursorDash: the active scene has no MapInformation asset.");
            enabled = false;
            return;
        }
        state = RoundState.Playing;
        DestroyedNodes = BlockedBeams = HexDestroyed = 0;
        NormalNode.DefenseStreak = 0;
        scoreManager.Initialize();
        deathUIController?.UICanvasOff();
        if (pauseHandler != null) pauseHandler.gameObject.SetActive(false);
        manager.soundManager.InitialSoundPitch(manager.soundManager.BGMSource);
        manager.soundManager.ChangeBGM(manager.MapInformation.audioClip);
        manager.soundManager.PlayBGM();
        if(CursorDashSession.IsPractice)gameObject.AddComponent<CursorDashPractice>();
        gameObject.AddComponent<CursorDashBoundaryView>();
        StartCoroutine(CaptureSettledBoundary());
    }
    private IEnumerator CaptureSettledBoundary(){
        // CanvasScaler.OnEnable/Update must run on Web before measuring. No attacks advance during this one-frame gate.
        yield return null;Canvas.ForceUpdateCanvases();
        if(hpBarImage!=null){var rect=hpBarImage.rectTransform;nexusPosition=rect.TransformPoint(rect.rect.center);nexusRadius=MeasuredBodyRadius;}
        boundaryReady=true;ApplyTimeScale();
    }

    private void Update()
    {
        if (IsPlaying) scoreManager.TimeScore += Time.deltaTime;
        Fever?.Advance(Time.deltaTime,!IsPlaying||Time.deltaTime<=0);
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Escape) && !IsDeath) TogglePause();
#endif
    }

    public void TogglePause() => SetPaused(state != RoundState.Paused);

    public void SetPaused(bool paused)
    {
        if (IsDeath) return;
        state = paused ? RoundState.Paused : RoundState.Playing;
        stick?.ResetInput();
        PlayerCharacterController.instance?.MoveStop();
        if (pauseHandler != null) pauseHandler.gameObject.SetActive(paused);
        if (paused) { feedback?.Clear();cameraShake?.Reset(); cameraFlash?.ResetFlash(); }
        if (paused) GameManager.instance.soundManager.BGMSource.Pause();
        else GameManager.instance.soundManager.BGMSource.UnPause();
        ApplyTimeScale();
    }

    public void RegisterCircleDestroyed(Vector2 position)
    {
        if (!IsPlaying) return;
        DestroyedNodes++;
        feedback.Emit("circleRemove",position);
    }

    public void RegisterCircleContact(Vector2 point,Vector2 incoming,Vector3[] silhouette)
    {
        if(!IsPlaying)return;DestroyedNodes++;
        feedback.Emit("circleRemove",point,silhouette,null,incoming);
    }
    public void RegisterCircleSheetContact(Vector2 point,Vector2 incoming,Vector3[] silhouette,GameObject sheet)
    {
        if(!IsPlaying)return;DestroyedNodes++;feedback.EmitCircle(point,silhouette,incoming,sheet);
    }

    public void RegisterHexBlocked(Vector2 position)
    {
        if (!IsPlaying) return;
        BlockedBeams++;
        feedback.Emit("hexBlock",position);
    }
    public void RegisterTriangleDestroyed(Vector2 point,Vector3[] triangle,Color tint)
    {
        if(!IsPlaying)return;HexDestroyed++;feedback.Emit("triangleBreak",point,triangle,tint);
    }
    public void RegisterHexReturn(Vector2 point,Vector3[] silhouette,Color tint,Vector2 incoming)
    {
        if(!IsPlaying)return;HexDestroyed++;feedback.Emit("triangleBreak",point,silhouette,tint,incoming);
    }
    // Only confirmed kills carry a source/execution token. Reflection/contact/cleanup never award twice.
    public void RegisterCircleKill(Node source,Vector2 point,Vector2 incoming,Vector3[] silhouette,GameObject sheet)
    {
        if(!IsPlaying||NormalThreatsSuppressed||Fever==null||!Fever.Award(source))return;
        RegisterCircleSheetContact(point,incoming,silhouette,sheet);SetTimeScore(scoreManager.TimeScore);
    }
    public void RegisterHexKill(Node source,Vector2 point,Vector3[] silhouette,Color tint,Vector2 incoming)
    {
        if(!IsPlaying||NormalThreatsSuppressed||Fever==null||!Fever.Award(source))return;
        RegisterHexReturn(point,silhouette,tint,incoming);SetTimeScore(scoreManager.TimeScore);
    }
    private void OnNexusHealthLost()=>Fever?.NexusHit();

    public void DamageNexus(float amount)=>DamageNexusAt(amount,NexusPosition);
    public void DamageNexusAt(float amount,Vector2 contact)
    { DamageNexusContact(amount,contact,(NexusPosition-contact).normalized,NexusImpactVfx.Cause.Manual,new Color(.1f,.8f,1),.4f); }
    public void DamageNexusFromCircle(float amount,Vector2 contact,Vector2 incoming,Color tint,float life)
    { DamageNexusContact(amount,contact,incoming,NexusImpactVfx.Cause.Circle,tint,life); }
    public void DamageNexusFromLaser(float amount,Vector2 contact,Vector2 incoming,Color tint)
    { DamageNexusContact(amount,contact,incoming,NexusImpactVfx.Cause.Laser,tint,.38f); }
    private void DamageNexusContact(float amount,Vector2 contact,Vector2 incoming,NexusImpactVfx.Cause cause,Color tint,float life)
    {
        if(!IsPlaying||NormalThreatsSuppressed||amount<=0)return;
        bool protectedHp=CursorDashSession.IsPractice&&PracticeInvulnerable;
        bool lethal=!protectedHp&&scoreManager.Hp>0&&amount>=scoreManager.Hp;
        lastCause=cause;lastContact=contact;lastIncoming=incoming.sqrMagnitude>.00001f?incoming.normalized:Vector2.down;
        // HP setter synchronously calls Death. Never emit the normal final hit before that callback.
        if(!lethal)feedback.EmitNexus(cause,contact,lastIncoming,tint,life);
        if(protectedHp){Fever?.NexusHit();return;}
        pendingLethal=lethal;scoreManager.Hp-=amount;pendingLethal=false;
    }
    public void DisqualifyRecord()=>RecordEligible=false;
    private void RefreshRuntimeClock()=>ApplyTimeScale();
    public void SetPanelPaused(bool value){panelPaused=value;ResetTransientInput(value);ApplyTimeScale();}
    public void SetDevPaused(bool value){devPaused=value;ResetTransientInput(value);ApplyTimeScale();}
    private void ResetTransientInput(bool paused){stick?.ResetInput();PlayerCharacterController.instance?.MoveStop();if(paused)feedback?.Clear();}
    public void Heal()=>scoreManager.Hp=scoreManager.MaxHp;
    public void RequestFeedbackStop(float duration)=>RequestHitStop(duration);

    private void RequestHitStop(float duration)
    {
        if (!IsPlaying || duration <= 0f) return;
        if(hitStopRoutine==null){if(Time.unscaledTime<stopCooldownUntil)return;stopBurstStart=Time.unscaledTime;}
        hitStopUntil = Mathf.Min(stopBurstStart+CursorDashTuning.Number("fx.stopCap",.1f),Mathf.Max(hitStopUntil, Time.unscaledTime + duration));
        if (hitStopRoutine == null) hitStopRoutine = StartCoroutine(HitStop());
    }

    private IEnumerator HitStop()
    {
        ApplyTimeScale();
        while (Time.unscaledTime < hitStopUntil && !IsDeath) yield return null;
        hitStopRoutine = null;
        stopCooldownUntil=Time.unscaledTime+CursorDashTuning.Number("fx.stopGap",.04f);
        ApplyTimeScale();
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = state == RoundState.Paused || panelPaused || devPaused || (IsPlaying && Time.unscaledTime < hitStopUntil) ? 0f :
            (IsPlaying&&CursorDashTuning.DevEnabled?CursorDashTuning.Number("debug.timeScale",1):1f);
    }

    public void SetHpImage(float percent) { if (energy != null)energy.SetTarget(percent);else if(hpBarImage!=null)hpBarImage.fillAmount=percent; }
    public void SetTimeScore(float value)
    {
        // TimeScore remains the survival clock; this existing HUD location now displays integer points.
        string text = Fever==null?"0":Fever.Model.PointsText;
        if (timeScoreText != null&&timeScoreText.text!=text) timeScoreText.text = text;
        if (timeScoreLabel != null&&timeScoreLabel.text!=text) timeScoreLabel.text = text;
    }

    public void Death()
    {
        if (IsDeath) return;
        state = RoundState.Ended;
        StopGameplay();
        GameManager manager = GameManager.instance;
        MapInformation map = manager.MapInformation;
        if(RecordEligible){
            map.highScore = Mathf.Max(map.highScore, scoreManager.TimeScore);
            string key = "CursorDash_Best_" + map.mapName;
            PlayerPrefs.SetFloat(key, Mathf.Max(PlayerPrefs.GetFloat(key), map.highScore));
        }
        CursorDashPointsRecord.Save(map.mapName,Fever.Model.Points,RecordEligible&&!CursorDashSession.IsPractice&&!CursorDashTuning.DevEnabled);
        manager.soundManager.PullSound(manager.soundManager.BGMSource);
        Vector2 point=pendingLethal?lastContact:NexusPosition;
        Vector2 incoming=pendingLethal?lastIncoming:Vector2.down;
        feedback?.EmitFinal(point,incoming);
        // Replace the old whole-core particle system, rather than stacking it on the final impact.
        if(coreDeathAnimation!=null)coreDeathAnimation.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if(CursorDashTuning.Toggle("fx.enabled",true)&&CursorDashTuning.Toggle("fx.particles",true)){
            finale=new GameObject("Nexus exclusive final break"){hideFlags=HideFlags.DontSave}.AddComponent<NexusImpactVfx>();
            finale.InitializeFinal(NexusPosition,NexusRadius,point,incoming,CursorDashTuning.ColorValue("nexus.color",new Color(.04f,.7f,.9f)),nexusPresentation,HideNexusVisuals,pendingLethal?lastCause:NexusImpactVfx.Cause.Manual);
        }
        if(resultRoutine!=null)StopCoroutine(resultRoutine);
        resultRoutine=StartCoroutine(ShowResult());
    }
    private void HideNexusVisuals()
    {
        if(nexusPresentation!=null)nexusPresentation.FinaleAlpha=0;
        if(hpBarFrame!=null)hpBarFrame.gameObject.SetActive(false);
        if(hpBarImage!=null){var central=hpBarImage.transform.parent.Find("HpCentral");if(central!=null)central.gameObject.SetActive(false);hpBarImage.gameObject.SetActive(false);}
    }
    private IEnumerator ShowResult()
    {
        yield return new WaitForSecondsRealtime(NexusImpactVfx.FinaleDuration);
        HideNexusVisuals();CancelFinale();cameraShake?.Reset();cameraFlash?.ResetFlash();Time.timeScale=1;
        resultRoutine=null;
        if (deathUIController == null) yield break;
        deathUIController.SetScoreText(Fever.Model.PointsText);
        deathUIController.SetRunSummary(DestroyedNodes,HexDestroyed,BlockedBeams,scoreManager.TimeScore);
        deathUIController.UICanvasOn();
    }
    private void CancelFinale(){if(finale!=null)Destroy(finale.gameObject);finale=null;}

    private void StopGameplay()
    {
        Fever?.Abort();
        if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
        hitStopRoutine = null;
        hitStopUntil = 0f;
        panelPaused=devPaused=false;
        feedback?.Clear();
        Time.timeScale = 1f;
        stick?.ResetInput();
        PlayerCharacterController.instance?.MoveStop();
        PlayerCharacterAfterImage.instance?.ClearTrail();
        nodeCreator?.StopNodes();
        PatternReader patterns = GetComponent<PatternReader>();
        if (patterns != null) patterns.enabled = false;
        if (pauseHandler != null) pauseHandler.gameObject.SetActive(false);
    }

    public void Retry() => ChangeScene(SceneManager.GetActiveScene().name);
    public void ReturnHome() => ChangeScene("01.StageSelectScene");
    public void ChangeScene(string sceneName)
    {
        if(resultRoutine!=null)StopCoroutine(resultRoutine);resultRoutine=null;CancelFinale();
        state = RoundState.Ended;
        StopGameplay();
        cameraShake?.Reset();
        cameraFlash?.ResetFlash();
        SceneManager.LoadScene(sceneName);
    }

    // Existing callbacks are retained while feedback ownership moves into this manager.
    public void CameraInteractionFlash() => cameraFlash?.InteractionFlash();
    public void CameraDamagedFlash() => cameraFlash?.DamagedFlash();
    public void CameraShake(float amount, float spendTime) => cameraShake?.Viberate(amount, spendTime);
    private void OnApplicationPause(bool paused) { if (paused && IsPlaying) SetPaused(true); }
    private void OnApplicationFocus(bool focused)
    {
#if !UNITY_EDITOR
        if (!focused && IsPlaying) SetPaused(true);
#endif
    }
    private void OnDestroy()
    {
        if(scoreManager!=null)scoreManager.Damaged-=OnNexusHealthLost;
        Fever?.Abort();
        CancelFinale();
        CursorDashTuning.RuleSessionChanged-=DisqualifyRecord;
        CursorDashTuning.Changed-=RefreshRuntimeClock;
        if (instance != this) return;
        instance = null;
        Time.timeScale = 1f;
    }
}
