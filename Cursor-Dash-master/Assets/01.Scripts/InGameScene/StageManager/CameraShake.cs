using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 defaultPosition;
    private float strength;
    
    private void Awake(){
        mainCamera = Camera.main;
        defaultPosition = mainCamera.gameObject.transform.position;
    }
    
    public void Viberate(float amount, float spendTime){
        if (bool.Parse(PlayerPrefs.GetString("OnVibe", "true"))) {
            strength = amount;
            StartCoroutine(ViberateCoroutine(spendTime));
        }
    }
    
    private IEnumerator ViberateCoroutine(float spendTime){
        while(spendTime > 0 && Time.timeScale != 0){
            mainCamera.gameObject.transform.position = Random.insideUnitSphere * strength + defaultPosition; 
            spendTime -= Time.deltaTime;
            yield return YieldInstructionCache.WaitFrame;
        }

        Reset();
    }


    public void Reset(){
        mainCamera.gameObject.transform.position = defaultPosition;
    }

}
