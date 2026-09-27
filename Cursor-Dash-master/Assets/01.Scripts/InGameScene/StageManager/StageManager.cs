using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public ScoreManager scoreManager;
    
    private NodeCreator nodeCreator;
    public NodeCreator NodeCreator => nodeCreator;
    
    [SerializeField]
    private Image hpBarImage;

    [SerializeField]
    private Image hpBarFrame;

    [SerializeField]
    private Text timeScoreText;

    [SerializeField]
    private DeathUIController deathUIController;
    
    [SerializeField]
    private ParticleSystem coreDeathAnimation;
    
    private CameraFlash cameraFlash;
    private CameraShake cameraShake;

    private ReplaySaver replaySaver;

    private bool isDeath = false;
    public bool IsDeath => isDeath;

    public void Awake(){
        if(instance is null){
            instance = this;
        }
        cameraFlash = gameObject.GetComponent<CameraFlash>();
        cameraShake = gameObject.GetComponent<CameraShake>();

        nodeCreator = gameObject.GetComponent<NodeCreator>();
        scoreManager = gameObject.GetComponent<ScoreManager>();
        
        replaySaver = gameObject.GetComponent<ReplaySaver>();

        scoreManager.SetHpEditAction(SetHpImage);
        scoreManager.SetDeathAction(Death);
        scoreManager.SetScoreAction(SetTimeScore);

        GameManager.instance.CurrentSceneType = GameManager.SceneType.InGame;
    }

    private void Start(){
        GameManager.instance.soundManager.InitialSoundPitch(GameManager.instance.soundManager.BGMSource);
        GameManager.instance.soundManager.ChangeBGM(GameManager.instance.MapInformation.audioClip);
        GameManager.instance.soundManager.PlayBGM();

        // StartCoroutine(NodeCreate());
        StartCoroutine(ScoreTimer());
    }

    public void CameraInteractionFlash(){
        cameraFlash.InteractionFlash();
    }

    public void CameraDamagedFlash(){
        cameraFlash.DamagedFlash();
    }

    private IEnumerator NodeCreate(){
        while(!isDeath){
            yield return YieldInstructionCache.WaitingSeconds(1.5f);
            nodeCreator.CreateNode(Random.Range(0,2));
        }
    }

    private IEnumerator ScoreTimer(){
        while(!isDeath){
            yield return YieldInstructionCache.WaitFrame;
            scoreManager.TimeScore += Time.deltaTime;
        }
    }
    
    public void CameraShake(float amount, float spendTime){
        cameraShake.Viberate(amount, spendTime);
    }

    public void SetHpImage(float percent){
        this.hpBarImage.fillAmount = percent;
    }

        
    public void SetTimeScore(float timeScore){
        timeScoreText.text = timeScore.ToString("F2");
    }

    public void Death(){
        if(isDeath){
            return;
        }

        Time.timeScale = 1.0f;
        
        isDeath = true;
        PlayerCharacterController.instance.MoveStop();
        GameManager.instance.soundManager.PullSound(GameManager.instance.soundManager.BGMSource);
        
        GameManager.instance.MapInformation.highScore = Mathf.Max(GameManager.instance.MapInformation.highScore, scoreManager.TimeScore);
        
        hpBarFrame.gameObject.SetActive(false);
        hpBarImage.gameObject.SetActive(false);
        
        DeathCoroutine().Start(this);
    }
    
    public void AddReplayData(string value){
        replaySaver.AddReplayInformation(value);
    }

    private IEnumerator DeathCoroutine(){
        coreDeathAnimation.gameObject.SetActive(true);
        coreDeathAnimation.Play();
        yield return YieldInstructionCache.WaitingSeconds(1.25f);
        deathUIController.SetScoreText(scoreManager.TimeScore.ToString("F2"));
        deathUIController.UICanvasOn();
    }

    public void Retry(){
        instance = null;
        // TODO : MAP DATA 받아와서 재시작하게 만들기
        SceneManager.LoadScene(GameManager.instance.MapInformation.mapName);
    }

    public void ReturnHome(){
        SceneManager.LoadScene("01.StageSelectScene");
    }

    private void OnDestroy() {
        instance = null;
    }
}
