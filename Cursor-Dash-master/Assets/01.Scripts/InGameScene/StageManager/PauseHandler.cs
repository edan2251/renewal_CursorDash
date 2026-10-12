using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseHandler : MonoBehaviour
{
    [Header("BGM")] [SerializeField] private Image bgm;
    [SerializeField] private Sprite bgmOn;
    [SerializeField] private Sprite bgmOff;
    [Header("SFX")] [SerializeField] private Image sfx;
    [SerializeField] private Sprite sfxOn;
    [SerializeField] private Sprite sfxOff;
    [Header("Vibration")] [SerializeField] private Image vibration;
    [SerializeField] private Sprite vibrationOn;
    [SerializeField] private Sprite vibrationOff;
    [SerializeField] private TMP_Text bgmLabel;
    [SerializeField] private TMP_Text sfxLabel;
    private float bgmVolume;
    private float sfxVolume;
    private bool isBgmOn;
    private bool isSfxOn;
    private bool isVibrationOn;

    public static bool IsPaused => StageManager.instance != null && StageManager.instance.State == StageManager.RoundState.Paused;
    private void Awake()
    {
        SoundManager sound = GameManager.instance.soundManager;
        isBgmOn = sound.BGMVolume > 0f;
        isSfxOn = sound.SFXVolume > 0f;
        bgmVolume = isBgmOn ? sound.BGMVolume : 1f;
        sfxVolume = isSfxOn ? sound.SFXVolume : 1f;
        isVibrationOn = PlayerPrefs.GetString("OnVibe", "true") == "true";
        RefreshLabels();
    }
    public void Pause() => StageManager.instance?.SetPaused(true);
    public void Unpause() => StageManager.instance?.SetPaused(false);
    public void LoadScene(string sceneName) => StageManager.instance?.ChangeScene(sceneName);
    public void Retry() => StageManager.instance?.Retry();
    public void ReturnHome() => StageManager.instance?.ReturnHome();
    public void ToggleBgm()
    {
        isBgmOn = !isBgmOn;
        GameManager.instance.soundManager.BGMVolume = isBgmOn ? bgmVolume : 0f;
        RefreshLabels();
    }
    public void ToggleSfx()
    {
        isSfxOn = !isSfxOn;
        GameManager.instance.soundManager.SFXVolume = isSfxOn ? sfxVolume : 0f;
        RefreshLabels();
    }
    public void ToggleVibration()
    {
        isVibrationOn = !isVibrationOn;
        PlayerPrefs.SetString("OnVibe", isVibrationOn.ToString().ToLowerInvariant());
        RefreshLabels();
    }
    private void RefreshLabels()
    {
        if (bgm != null) bgm.sprite = isBgmOn ? bgmOn : bgmOff;
        if (sfx != null) sfx.sprite = isSfxOn ? sfxOn : sfxOff;
        if (vibration != null) vibration.sprite = isVibrationOn ? vibrationOn : vibrationOff;
        if (bgmLabel != null) bgmLabel.text = "BGM " + (isBgmOn ? "ON" : "OFF");
        if (sfxLabel != null) sfxLabel.text = "SFX " + (isSfxOn ? "ON" : "OFF");
    }
}
