using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneController : MonoBehaviour
{
    [SerializeField] private MapInformation map;
    [SerializeField] private AudioClip mainMusic;
    [SerializeField] private TMP_Text bestLabel;
    [SerializeField] private TMP_Text[] bestShadows;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Text bgmLabel;
    [SerializeField] private TMP_Text sfxLabel;
    private bool loading;
    private float bgmVolume;
    private float sfxVolume;
    private string bestPoints;

    private void Start()
    {
        // Compatibility entry only. The visible main and gameplay now live in Cyberpunk together.
        if(map!=null&&map.mapName=="Cyberpunk"){GameManager.instance.MapInformation=map;CursorDashSession.CurrentMode=CursorDashSession.Mode.Normal;CursorDashSession.EnterGameOnLoad=false;SceneManager.LoadScene(map.mapName);return;}
        Time.timeScale = 1f;
        GameManager manager = GameManager.instance;
        manager.CurrentSceneType = GameManager.SceneType.NotInGame;
        manager.MapInformation = map;
        if (mainMusic != null) { manager.soundManager.ChangeBGM(mainMusic); manager.soundManager.PlayBGM(); }
        bgmVolume = Mathf.Max(0.01f, manager.soundManager.BGMVolume > 0f ? manager.soundManager.BGMVolume : 1f);
        sfxVolume = Mathf.Max(0.01f, manager.soundManager.SFXVolume > 0f ? manager.soundManager.SFXVolume : 1f);
        string best = bestPoints = CursorDashPointsRecord.Read(map.mapName).ToString();
        if (bestLabel != null) bestLabel.text = best;
        if (bestShadows != null) foreach (TMP_Text shadow in bestShadows) if (shadow != null) shadow.text = best;
        foreach(var caption in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(caption.text=="BEST TIME")caption.text="BEST SCORE";
        if (settingsPanel != null) settingsPanel.SetActive(false);
        RefreshSoundLabels();
    }
    public void StartGame()
    {
        if (loading || map == null) return;
        loading = true;
        CursorDashSession.CurrentMode=CursorDashSession.FirstGuideNeeded?CursorDashSession.Mode.Tutorial:CursorDashSession.Mode.Normal;
        CursorDashTuning.EnableDev(false);
        GameManager.instance.MapInformation = map;
        GameManager.instance.stageIndex = map.index;
        Time.timeScale = 1f;
        SceneManager.LoadScene(map.mapName);
    }
    private void LateUpdate()
    {
        // Legacy menu startup/layout callbacks must not replace the integer best with a seconds placeholder.
        if(bestPoints==null)return;
        if(bestLabel!=null&&bestLabel.text!=bestPoints)bestLabel.text=bestPoints;
        if(bestShadows!=null)foreach(var shadow in bestShadows)if(shadow!=null&&shadow.text!=bestPoints)shadow.text=bestPoints;
    }
    public void OpenSettings() { CursorDashRuntimeUI.Instance.Open(false); }
    public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }
    public void ToggleBgm()
    {
        SoundManager sound = GameManager.instance.soundManager;
        sound.BGMVolume = sound.BGMVolume > 0f ? 0f : bgmVolume;
        RefreshSoundLabels();
    }
    public void ToggleSfx()
    {
        SoundManager sound = GameManager.instance.soundManager;
        sound.SFXVolume = sound.SFXVolume > 0f ? 0f : sfxVolume;
        RefreshSoundLabels();
    }
    private void RefreshSoundLabels()
    {
        if (bgmLabel != null) bgmLabel.text = "BGM " + (GameManager.instance.soundManager.BGMVolume > 0f ? "ON" : "OFF");
        if (sfxLabel != null) sfxLabel.text = "SFX " + (GameManager.instance.soundManager.SFXVolume > 0f ? "ON" : "OFF");
    }
}
