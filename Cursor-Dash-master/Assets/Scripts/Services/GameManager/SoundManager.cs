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
    private readonly Dictionary<AudioSource, Coroutine> pitchRoutines = new Dictionary<AudioSource, Coroutine>();
    private readonly Dictionary<AudioSource,float> eventVolumes=new Dictionary<AudioSource,float>();
    private float masterVolume=1;

    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.5f;

    public AudioSource BGMSource => bgmSource;
    public AudioSource SFXSource => sfxSource;

    public float BGMVolume{
        get{
            return bgmVolume;
        }
        set{
            bgmVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("BGM", bgmVolume);
            RefreshVolumes();
        }
    }

    public float SFXVolume{
        get{
            return sfxVolume;
        }
        set{
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SFX", sfxVolume);
            RefreshVolumes();
        }
    }

    private void Awake(){
        sfxDictionary.Add(ClipTag.arrowDefence, arrowDefence);
        sfxDictionary.Add(ClipTag.coreDamaged, coreDamaged);
        sfxDictionary.Add(ClipTag.nodeExplosion, nodeExplosion);
        sfxDictionary.Add(ClipTag.laser, laser);
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
        
    }

    public void ChangeBGM(AudioClip clip){
        InitialSoundPitch(bgmSource);
        bgmSource.clip = clip;
    }

    public void PlayBGM(){
        // The current release has no background music; keep effects on their own sources.
        StopBGM();
    }

    public void StopBGM(){
        InitialSoundPitch(bgmSource);
        bgmSource.Stop();
    }

    public void ChangeSFX(AudioClip clip){
        sfxSource.clip = clip;
    }

    public void PlaySFX(){
        sfxSource.volume=sfxVolume*masterVolume;
        sfxSource.Play();
    }

    public void PlaySFX(ClipTag tag){
        if (sfxDictionary.TryGetValue(tag, out AudioSource source) && source != null){eventVolumes[source]=1;source.volume=sfxVolume*masterVolume;source.Play();}
    }
    public void PlaySFX(ClipTag tag,float volume,float pitch) {
        if(!sfxDictionary.TryGetValue(tag,out AudioSource source)||source==null)return;
        eventVolumes[source]=Mathf.Clamp01(volume);source.volume=sfxVolume*masterVolume*eventVolumes[source];source.pitch=Mathf.Clamp(pitch,.25f,3f);source.Play();
    }
    public void ApplyRuntimeVolumes(float bgm,float sfx,float master=1) {
        // v2 user settings do not overwrite the legacy BGM/SFX preference keys.
        bgm=Mathf.Clamp01(bgm);sfx=Mathf.Clamp01(sfx);master=Mathf.Clamp01(master);
        if(bgmVolume==bgm&&sfxVolume==sfx&&masterVolume==master)return;
        bgmVolume=bgm;sfxVolume=sfx;masterVolume=master;RefreshVolumes();
    }
    private void RefreshVolumes(){
        if(bgmSource!=null)bgmSource.volume=bgmVolume*masterVolume;
        if(sfxSource!=null)sfxSource.volume=sfxVolume*masterVolume;
        foreach(var source in sfxDictionary.Values)if(source!=null)source.volume=sfxVolume*masterVolume*(eventVolumes.TryGetValue(source,out float gain)?gain:1);
    }
    
    public void PullSound(AudioSource audioSource){
        StartPitch(audioSource, PullSoundCoroutine(audioSource));
    }

    public void PushSound(AudioSource audioSource){
        StartPitch(audioSource, PushSoundCoroutine(audioSource));
    }
    
    public void PullAndPushSound(AudioSource audioSource){
        StartPitch(audioSource, PullAndPushSoundCoroutine(audioSource));
    }

    public void InitialSoundPitch(AudioSource audioSource){
        if (audioSource == null) return;
        if (pitchRoutines.TryGetValue(audioSource, out Coroutine routine))
        {
            if (routine != null) StopCoroutine(routine);
            pitchRoutines.Remove(audioSource);
        }
        audioSource.pitch = 1;
    }

    private void StartPitch(AudioSource source, IEnumerator routine)
    {
        if (source == null) return;
        InitialSoundPitch(source);
        pitchRoutines[source] = StartCoroutine(routine);
    }

    private IEnumerator PullAndPushSoundCoroutine(AudioSource audioSource){
        audioSource.pitch = 1;

        yield return PullSoundCoroutine(audioSource);
        yield return PushSoundCoroutine(audioSource);
    }

    private IEnumerator PullSoundCoroutine(AudioSource audioSource){
        for(int i = 0; i < 60; i++){
            if (audioSource == null) yield break;
            audioSource.pitch = Mathf.Max(0.1f, audioSource.pitch - 0.015f);
            yield return YieldInstructionCache.WaitFrame;
        }
    }

    private IEnumerator PushSoundCoroutine(AudioSource audioSource){
        for(int i = 0; i < 60; i++){
            if (audioSource == null) yield break;
            audioSource.pitch = Mathf.Min(1f, audioSource.pitch + 0.015f);
            yield return YieldInstructionCache.WaitFrame;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (AudioSource source in pitchRoutines.Keys)
            if (source != null) source.pitch = 1f;
        pitchRoutines.Clear();
    }
}
