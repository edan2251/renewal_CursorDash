using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GameManager : DontDestroySingleton<GameManager>
{
    [HideInInspector]
    public TouchManager touchManager;
    
    [HideInInspector] 
    public FadeManager fadeManager;
    
    [HideInInspector]
    public SoundManager soundManager;

    [HideInInspector]
    public int stageIndex;

    public enum SceneType {NotInGame, InGame}

    [SerializeField]
    private SceneType currentSceneType;

    public SceneType CurrentSceneType{ get => currentSceneType; set => currentSceneType = value;}

    private PlayerData _playerData;
    public PlayerData playerData => _playerData;
    
    private Dictionary<string, LanguageSetting> languageSettingDictionary = new Dictionary<string, LanguageSetting>();
    public Dictionary<string, LanguageSetting> LanguageSettingDictionary => languageSettingDictionary;

    private MapInformation[] maps;
    public MapInformation[] Maps => maps;
    
    private LanguageSetting _currentLanguage;
    public LanguageSetting currentLanguage {
        get => _currentLanguage;
        set => _currentLanguage = value;
    }

    private MapInformation mapInformation;
    public MapInformation MapInformation {
        get => mapInformation;
        set => mapInformation = value;
    }
    
    private void Awake(){
        if (instance != null && instance != this) {
            Destroy(gameObject);
        }
        
        touchManager = gameObject.GetComponent<TouchManager>();
        fadeManager = gameObject.GetComponent<FadeManager>();
        soundManager = gameObject.GetComponent<SoundManager>();
        
        var languageSettings = new List<LanguageSetting>(Resources.LoadAll<LanguageSetting>("PlayerSetting/Language"));

        languageSettings.ForEach((item) => {
            item.Initialize();
            languageSettingDictionary.Add(item.fileKey, item);
        });

        maps = Resources.LoadAll<MapInformation>("Maps");
        _playerData = Resources.Load<PlayerData>("PlayerSetting/PlayerData");
        _currentLanguage = languageSettingDictionary["Korean"];

        if (_playerData.characterSetting.selectSprite == null) {
            _playerData.characterSetting.selectSprite = _playerData.characterSetting.CharacterSprites[0];
        }

        if (_playerData.characterSetting.selectColor == null) {
            _playerData.characterSetting.selectColor = _playerData.characterSetting.CharacterColors[_playerData.characterSetting.CharacterColors.Length - 1];
        }
    }

    private void Start() {
        playerData.AchieveRequire.Initialize();
        maps[1].isUnlock = playerData.AchieveData.GetAchieve("Flame");
    }

    [Button("Reset")]
    public void ResetSaveData() {
        PlayerPrefs.DeleteAll();
    }

}
