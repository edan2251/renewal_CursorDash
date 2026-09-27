using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchManager : MonoBehaviour
{
    private bool isSwipe;
    private bool isTouch;
    private Vector2 swipeDirection = Vector2.zero;
    private Vector2 touchPosition;
    private Vector2 touchUpPosition;

    private Vector2 touchDownPositionNotScreen;

    private Touch tempTouch;

    private float minSwipeDistance;

    private List<ITouchUpDownObserver> touchObservers = new List<ITouchUpDownObserver>();

    private Camera mainCamera;
    public Vector2 SwipeDirection => swipeDirection;
    
    public bool IsTouch => isTouch;
    public bool IsSwipe => isSwipe;
    public Vector2 TouchPosition => touchPosition;
    public Vector2 TouchUpPosition => touchUpPosition;

    private void Awake() {
        mainCamera = Camera.main;
        SceneManager.sceneLoaded += (scene, loadMode) => {
            mainCamera = Camera.main;
        };
    }
    
    private void Start(){
        minSwipeDistance = Screen.width / 8;
    }

    private void Update(){
        ProcessTouch();
        
        #if UNITY_EDITOR
        ProcessMouse();
        #endif
    }
    
    private void ProcessTouch(){
        if(Input.touchCount > 0){
            tempTouch = Input.touches[0];
            if(tempTouch.phase.Equals(TouchPhase.Began)){
                isTouch = true;
                touchDownPositionNotScreen = tempTouch.position;
                touchPosition = mainCamera.ScreenToWorldPoint(touchDownPositionNotScreen);
                TouchDownNotify();
            }
            else if(tempTouch.phase.Equals(TouchPhase.Moved)){
                Vector2 currentPosition = tempTouch.position;
                if((currentPosition - touchDownPositionNotScreen).magnitude > minSwipeDistance){
                    isSwipe = true;
                    swipeDirection = (currentPosition - touchDownPositionNotScreen).normalized;
                }
            }
            else if(tempTouch.phase.Equals(TouchPhase.Ended)){
                isTouch = false;
                isSwipe = false;
                touchUpPosition = mainCamera.ScreenToWorldPoint(tempTouch.position);
                swipeDirection = Vector2.zero;
                TouchUpNotify();
            }
        }
    }

    public void ProcessMouse(){
        if(Input.GetMouseButtonDown(0)){
            isTouch = true;
            touchDownPositionNotScreen = Input.mousePosition;
            touchPosition = mainCamera.ScreenToWorldPoint(touchDownPositionNotScreen);
            TouchDownNotify();
        } else if(Input.GetMouseButton(0)){
            Vector2 currentPosition = Input.mousePosition;
            if((currentPosition - touchDownPositionNotScreen).magnitude > minSwipeDistance){
                isSwipe = true;
                swipeDirection = (currentPosition - touchDownPositionNotScreen).normalized;
            }
        } else if(Input.GetMouseButtonUp(0)){
            isTouch = false;
            isSwipe = false;
            touchUpPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            swipeDirection = Vector2.zero;
            TouchUpNotify();
        }
    }
    
    public void AddObserver(ITouchUpDownObserver observer){
        touchObservers.Add(observer);
    }

    public void DeleteObserver(ITouchUpDownObserver observer){
        touchObservers.Remove(observer);
    }

    private void TouchUpNotify(){
        touchObservers.ForEach(delegate(ITouchUpDownObserver observer){
            observer.TouchUpNotify();
        });
    }

    private void TouchDownNotify(){
        touchObservers.ForEach(delegate(ITouchUpDownObserver observer){
            observer.TouchDownNotify();
        });
    }
}
