using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 defaultPosition;
    private Coroutine routine;
    private float strength;
    private float until;
    private float started,decayExponent=1.8f;
    private bool directed;private Vector2 direction;private float frequency,rebound;
    private void Awake()
    {
        if (Camera.main == null) return;
        cameraTransform = Camera.main.transform;
        defaultPosition = cameraTransform.position;
    }
    public void Viberate(float amount, float duration,float decay=1.8f)
    {
        directed=false;Begin(amount,duration,decay);
    }
    public void Kick(Vector2 incoming,float amount,float duration,float decay,float hz,float bounce)
    {
        directed=true;direction=incoming.normalized;frequency=Mathf.Clamp(hz,5,45);rebound=Mathf.Clamp01(bounce);
        Begin(amount,duration,decay);
    }
    private void Begin(float amount,float duration,float decay)
    {
        if (cameraTransform == null || PlayerPrefs.GetString("OnVibe", "true") != "true") return;
        if(routine==null)started=Time.unscaledTime;
        decayExponent=decay;
        strength = Mathf.Min(CursorDashTuning.Number("fx.shakeCap",.2f),Mathf.Max(strength, Mathf.Max(0f, amount)));
        until = Mathf.Min(started+CursorDashTuning.Number("fx.shakeMaxDuration",.35f),Mathf.Max(until, Time.unscaledTime + duration));
        if (routine == null) routine = StartCoroutine(Shake());
    }
    private IEnumerator Shake()
    {
        while (Time.unscaledTime < until)
        {
            float fade=Mathf.Pow(1-Mathf.Clamp01((Time.unscaledTime-started)/Mathf.Max(.01f,until-started)),decayExponent);
            float age=Time.unscaledTime-started;
            float wave=Mathf.Cos(age*frequency*Mathf.PI*2);
            Vector2 offset=directed?(direction*(wave>=0?wave:wave*rebound)+new Vector2(-direction.y,direction.x)*Mathf.Sin(age*frequency*8.1f)*.22f)*strength*fade:
                Random.insideUnitCircle*strength*fade;
            cameraTransform.position = defaultPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }
        routine = null;
        Reset();
    }
    public void Reset()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        strength = 0f;
        directed=false;
        until = 0f;
        if (cameraTransform != null) cameraTransform.position = defaultPosition;
    }
    private void OnDisable() => Reset();
}
