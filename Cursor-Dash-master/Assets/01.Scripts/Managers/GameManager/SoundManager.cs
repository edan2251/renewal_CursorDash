using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{  
    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioSource arrowDefence;

    [SerializeField]
    private AudioSource nodeExplosion;
    
    [SerializeField]
    private AudioSource coreDamaged;

    [SerializeField] 
    private AudioSource laser;
    
    private Dictionary<ClipTag, AudioSource> sfxDictionary = new Dictionary<ClipTag, AudioSource>();

    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.5f;

    public AudioSource BGMSource => bgmSource;
    public AudioSource SFXSource => sfxSource;

    public float BGMVolume{
        get{
            return bgmVolume;
        }
        set{
            bgmVolume = value;
            PlayerPrefs.SetFloat("BGM", bgmVolume);
            bgmSource.volume = bgmVolume;
        }
    }

    public float SFXVolume{
        get{
            return sfxVolume;
        }
        set{
            sfxVolume = value;
            PlayerPrefs.SetFloat("SFX", sfxVolume);
            sfxSource.volume = sfxVolume; 
        }
    }

    private void Awake(){
        if(PlayerPrefs.HasKey("BGM")){
            BGMVolume = PlayerPrefs.GetFloat("BGM");
        }
        else{
            BGMVolume = 0.5f;
        }

        if(PlayerPrefs.HasKey("SFX")){
            SFXVolume = PlayerPrefs.GetFloat("SFX");
        }
        else{
            SFXVolume = 0.5f;
        }
        
        sfxDictionary.Add(ClipTag.arrowDefence, arrowDefence);
        sfxDictionary.Add(ClipTag.coreDamaged, coreDamaged);
        sfxDictionary.Add(ClipTag.nodeExplosion, nodeExplosion);
        sfxDictionary.Add(ClipTag.laser, laser);
    }

    public void ChangeBGM(AudioClip clip){
        bgmSource.clip = clip;
    }

    public void PlayBGM(){
        PullAndPushSound(bgmSource);
        bgmSource.Play();
    }

    public void StopBGM(){
        bgmSource.Stop();
    }

    public void ChangeSFX(AudioClip clip){
        sfxSource.clip = clip;
    }

    public void PlaySFX(){
        sfxSource.Play();
    }

    public void PlaySFX(ClipTag tag){
        sfxDictionary[tag].Play();
    }
    
    public void PullSound(AudioSource audioSource){
        audioSource.pitch = 1;

        StartCoroutine(PullSoundCoroutine(audioSource));        
    }

    public void PushSound(AudioSource audioSource){
        audioSource.pitch = 1;

        StartCoroutine(PushSoundCoroutine(audioSource));        
    }
    
    public void PullAndPushSound(AudioSource audioSource){
        audioSource.pitch = 1;

        StartCoroutine(PullAndPushSoundCoroutine(audioSource));
    }

    public void InitialSoundPitch(AudioSource audioSource){
        audioSource.pitch = 1;
    }

    private IEnumerator PullAndPushSoundCoroutine(AudioSource audioSource){
        audioSource.pitch = 1;

        yield return StartCoroutine(PullSoundCoroutine(audioSource));
        yield return StartCoroutine(PushSoundCoroutine(audioSource));
    }

    private IEnumerator PullSoundCoroutine(AudioSource audioSource){
        for(int i = 0; i < 60; i++){
            audioSource.pitch -= 0.015f;
            yield return YieldInstructionCache.WaitFrame;
        }
    }

    private IEnumerator PushSoundCoroutine(AudioSource audioSource){
        for(int i = 0; i < 60; i++){
            audioSource.pitch += 0.015f;
            yield return YieldInstructionCache.WaitFrame;
        }
    }
}
