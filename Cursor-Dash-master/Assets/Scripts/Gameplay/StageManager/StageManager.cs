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

    [Header("Initial feedback settings (tune on devices)")]
    [SerializeField, Range(0f, 0.15f)] private float circleHitStop = 0.035f;
    [SerializeField, Range(0f, 0.15f)] private float hexHitStop = 0.055f;
    [SerializeField, Range(0f, 0.2f)] private float damageHitStop = 0.08f;
    [SerializeField, Range(0f, 0.3f)] private float circleShake = 0.035f;
    [SerializeField, Range(0f, 0.3f)] private float hexShake = 0.065f;
    [SerializeField, Range(0f, 0.3f)] private float damageShake = 0.14f;

    private NodeCreator nodeCreator;
    private CameraFlash cameraFlash;
    private CameraShake cameraShake;
    private VirtualJoyStick stick;
    private Coroutine hitStopRoutine;
    private float hitStopUntil;
    private RoundState state = RoundState.Playing;

    public NodeCreator NodeCreator => nodeCreator;
    public RoundState State => state;
    public bool IsDeath => state == RoundState.Ended;
    public bool IsPlaying => state == RoundState.Playing;
    public int DestroyedNodes { get; private set; }
    public int BlockedBeams { get; private set; }
    public Vector2 NexusPosition => Vector2.zero;

    private void Awake()
    {
        instance = this;
        scoreManager = GetComponent<ScoreManager>();
        nodeCreator = GetComponent<NodeCreator>();
        cameraFlash = GetComponent<CameraFlash>();
        cameraShake = GetComponent<CameraShake>();
        stick = FindFirstObjectByType<VirtualJoyStick>();
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
        DestroyedNodes = BlockedBeams = 0;
        NormalNode.DefenseStreak = 0;
        scoreManager.Initialize();
        deathUIController?.UICanvasOff();
        if (pauseHandler != null) pauseHandler.gameObject.SetActive(false);
        manager.soundManager.InitialSoundPitch(manager.soundManager.BGMSource);
        manager.soundManager.ChangeBGM(manager.MapInformation.audioClip);
        manager.soundManager.PlayBGM();
    }

    private void Update()
    {
        if (IsPlaying) scoreManager.TimeScore += Time.deltaTime;
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
        if (paused) { cameraShake?.Reset(); cameraFlash?.ResetFlash(); }
        if (paused) GameManager.instance.soundManager.BGMSource.Pause();
        else GameManager.instance.soundManager.BGMSource.UnPause();
        ApplyTimeScale();
    }

    public void RegisterCircleDestroyed(Vector2 position)
    {
        if (!IsPlaying) return;
        DestroyedNodes++;
        RequestHitStop(circleHitStop);
        cameraShake?.Viberate(circleShake, 0.12f);
        cameraFlash?.Flash(new Color(0.68f, 0.35f, 1f), 0.12f, 0.15f);
    }

    public void RegisterHexBlocked(Vector2 position)
    {
        if (!IsPlaying) return;
        BlockedBeams++;
        RequestHitStop(hexHitStop);
        cameraShake?.Viberate(hexShake, 0.16f);
        cameraFlash?.Flash(new Color(1f, 0.82f, 0.18f), 0.16f, 0.18f);
    }

    public void DamageNexus(float amount)
    {
        if (!IsPlaying || amount <= 0f) return;
        RequestHitStop(damageHitStop);
        cameraShake?.Viberate(damageShake, 0.22f);
        cameraFlash?.Flash(new Color(1f, 0.2f, 0.38f), 0.25f, 0.25f);
        GameManager.instance.soundManager.PlaySFX(ClipTag.coreDamaged);
        scoreManager.Hp -= amount;
    }

    private void RequestHitStop(float duration)
    {
        if (!IsPlaying || duration <= 0f) return;
        hitStopUntil = Mathf.Max(hitStopUntil, Time.unscaledTime + duration);
        if (hitStopRoutine == null) hitStopRoutine = StartCoroutine(HitStop());
    }

    private IEnumerator HitStop()
    {
        ApplyTimeScale();
        while (Time.unscaledTime < hitStopUntil && !IsDeath) yield return null;
        hitStopRoutine = null;
        ApplyTimeScale();
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = state == RoundState.Paused || (IsPlaying && Time.unscaledTime < hitStopUntil) ? 0f : 1f;
    }

    public void SetHpImage(float percent) { if (hpBarImage != null) hpBarImage.fillAmount = percent; }
    public void SetTimeScore(float value)
    {
        string text = value.ToString("F2");
        if (timeScoreText != null) timeScoreText.text = text;
        if (timeScoreLabel != null) timeScoreLabel.text = text;
    }

    public void Death()
    {
        if (IsDeath) return;
        state = RoundState.Ended;
        StopGameplay();
        GameManager manager = GameManager.instance;
        MapInformation map = manager.MapInformation;
        map.highScore = Mathf.Max(map.highScore, scoreManager.TimeScore);
        string key = "CursorDash_Best_" + map.mapName;
        PlayerPrefs.SetFloat(key, Mathf.Max(PlayerPrefs.GetFloat(key), map.highScore));
        manager.soundManager.PullSound(manager.soundManager.BGMSource);
        if (hpBarFrame != null) hpBarFrame.gameObject.SetActive(false);
        if (hpBarImage != null) hpBarImage.gameObject.SetActive(false);
        if (coreDeathAnimation != null) { coreDeathAnimation.gameObject.SetActive(true); coreDeathAnimation.Play(); }
        cameraShake?.Viberate(damageShake, 0.25f);
        cameraFlash?.Flash(new Color(1f, 0.2f, 0.38f), 0.3f, 0.4f);
        StartCoroutine(ShowResult());
    }

    private IEnumerator ShowResult()
    {
        yield return new WaitForSecondsRealtime(0.45f);
        if (deathUIController == null) yield break;
        deathUIController.SetScoreText(scoreManager.TimeScore.ToString("F2"));
        deathUIController.SetSummary(DestroyedNodes, BlockedBeams);
        deathUIController.UICanvasOn();
    }

    private void StopGameplay()
    {
        if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
        hitStopRoutine = null;
        hitStopUntil = 0f;
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
        if (instance != this) return;
        instance = null;
        Time.timeScale = 1f;
    }
}
