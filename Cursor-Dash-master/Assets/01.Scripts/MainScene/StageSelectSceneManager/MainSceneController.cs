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

    private void Start()
    {
        Time.timeScale = 1f;
        GameManager manager = GameManager.instance;
        manager.CurrentSceneType = GameManager.SceneType.NotInGame;
        manager.MapInformation = map;
        if (mainMusic != null) { manager.soundManager.ChangeBGM(mainMusic); manager.soundManager.PlayBGM(); }
        bgmVolume = Mathf.Max(0.01f, manager.soundManager.BGMVolume > 0f ? manager.soundManager.BGMVolume : 1f);
        sfxVolume = Mathf.Max(0.01f, manager.soundManager.SFXVolume > 0f ? manager.soundManager.SFXVolume : 1f);
        string best = Mathf.Max(map.highScore, PlayerPrefs.GetFloat("CursorDash_Best_" + map.mapName)).ToString("F2");
        if (bestLabel != null) bestLabel.text = best;
        if (bestShadows != null) foreach (TMP_Text shadow in bestShadows) if (shadow != null) shadow.text = best;
        if (settingsPanel != null) settingsPanel.SetActive(false);
        RefreshSoundLabels();
    }
    public void StartGame()
    {
        if (loading || map == null) return;
        loading = true;
        GameManager.instance.MapInformation = map;
        GameManager.instance.stageIndex = map.index;
        Time.timeScale = 1f;
        SceneManager.LoadScene(map.mapName);
    }
    public void OpenSettings() { if (settingsPanel != null) settingsPanel.SetActive(true); }
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
