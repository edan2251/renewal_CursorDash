using UnityEngine;
using UnityEngine.SceneManagement;
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

    private float bgmVolume;
    private float sfxVolume;
    private const string vibrationKey = "OnVibe";

    private bool isBgmOn = true;
    private bool isSfxOn = true;
    private bool isVibrationOn = true;

    public static bool IsPaused { get; private set; }
    private Sprite BgmSprite => isBgmOn ? bgmOn : bgmOff;
    private Sprite SfxSprite => isSfxOn ? sfxOn : sfxOff;
    private Sprite VibrationSprite => isVibrationOn ? vibrationOn : vibrationOff;
    private float BgmVolume => isBgmOn ? bgmVolume : 0f;
    private float SfxVolume => isSfxOn ? sfxVolume : 0f;

    public void Pause()
    {
        IsPaused = true;
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        IsPaused = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ToggleBgm()
    {
        isBgmOn = !isBgmOn;
        bgm.sprite = BgmSprite;
        GameManager.instance.soundManager.BGMVolume = BgmVolume;
    }

    public void ToggleSfx()
    {
        isSfxOn = !isSfxOn;
        sfx.sprite = SfxSprite;
        GameManager.instance.soundManager.SFXVolume = SfxVolume;
    }

    public void ToggleVibration()
    {
        isVibrationOn = !isVibrationOn;
        vibration.sprite = VibrationSprite;
        PlayerPrefs.SetString(vibrationKey, isVibrationOn.ToString());
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void Awake()
    {
        bgmVolume = GameManager.instance.soundManager.BGMVolume;
        if (bgmVolume == 0f)
        {
            bgmVolume = 2f;
            isBgmOn = false;
        }

        bgm.sprite = BgmSprite;

        sfxVolume = GameManager.instance.soundManager.SFXVolume;
        if (sfxVolume == 0f)
        {
            sfxVolume = 2f;
            isSfxOn = false;
        }

        sfx.sprite = SfxSprite;

        isVibrationOn = PlayerPrefs.GetString(vibrationKey) == "true";
        vibration.sprite = VibrationSprite;
    }
}