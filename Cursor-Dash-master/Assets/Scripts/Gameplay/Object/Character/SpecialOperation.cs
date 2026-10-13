using DG.Tweening;
using EventTools.Event;
using UnityEngine;

public class SpecialOperation : MonoBehaviour
{
    [SerializeField] private float doubleTouchInterval = 0.5f;
    [SerializeField] private float timeScale = 0.1f;
    [SerializeField] private float dashDistance = 1f;
    [SerializeField] private UniEvent<float> dash;
    
    private int touchCount;
    private Tween tween;

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            TouchDownNotify();
        }
        else if(Input.GetMouseButtonUp(0)) {
            TouchUpNotify();
        }
    }
    
    public void TouchUpNotify()
    {
        if (touchCount < 2 || StageManager.instance.scoreManager.Hp <= 0f || PauseHandler.IsPaused)
            return;

        touchCount = 0;
        Time.timeScale = 1f;
        dash?.Invoke(dashDistance);
    }

    public void TouchDownNotify()
    {
        if (StageManager.instance.scoreManager.Hp <= 0f || PauseHandler.IsPaused)
            return;
        
        tween?.Kill();
        touchCount++;

        if (touchCount < 2)
        {
            tween = DOVirtual.DelayedCall(doubleTouchInterval, () => touchCount = 0);
        }
        else
        {
            Time.timeScale = timeScale;
        }
    }

}