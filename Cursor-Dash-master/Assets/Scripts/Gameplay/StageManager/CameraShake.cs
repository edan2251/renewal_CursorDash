using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 defaultPosition;
    private Coroutine routine;
    private float strength;
    private float until;
    private void Awake()
    {
        if (Camera.main == null) return;
        cameraTransform = Camera.main.transform;
        defaultPosition = cameraTransform.position;
    }
    public void Viberate(float amount, float duration)
    {
        if (cameraTransform == null || PlayerPrefs.GetString("OnVibe", "true") != "true") return;
        strength = Mathf.Max(strength, Mathf.Max(0f, amount));
        until = Mathf.Max(until, Time.unscaledTime + duration);
        if (routine == null) routine = StartCoroutine(Shake());
    }
    private IEnumerator Shake()
    {
        while (Time.unscaledTime < until)
        {
            Vector2 offset = Random.insideUnitCircle * strength;
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
        until = 0f;
        if (cameraTransform != null) cameraTransform.position = defaultPosition;
    }
    private void OnDisable() => Reset();
}
