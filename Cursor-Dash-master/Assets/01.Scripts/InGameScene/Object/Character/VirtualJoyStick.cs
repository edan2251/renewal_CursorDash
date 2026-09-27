using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualJoyStick : MonoBehaviour
{
    private bool isTouch;
    
    private Vector2 pivotPosition;
    private Vector2 direction;
    
    private GameObject childObject;

    private SpriteRenderer frameSpriteRenderer;
    private SpriteRenderer childSpriteRenderer;
    
    private void Awake() {
        childObject = gameObject.transform.GetChild(0).gameObject;
        
        frameSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        childSpriteRenderer = childObject.GetComponent<SpriteRenderer>();

        DisableSpriteRenderer();
    }

    private void Update(){
        JoyStickMove();
        KeyBoardMove();

    }

    private void JoyStickMove() {
        var currentTouchPosition = Vector2.zero;
        if (Input.GetMouseButtonDown(0)) {
            isTouch = true;
            pivotPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gameObject.transform.position = pivotPosition;
            
            EnableSpriteRenderer();
        }

        if (isTouch == false) {
            return;
        }
        
        if (Input.GetMouseButton(0)) {
            currentTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0)){
            currentTouchPosition = Vector2.zero;
            PlayerCharacterController.instance.MoveVector = Vector2.zero;
            childObject.transform.position = gameObject.transform.position;
            
            isTouch = false;
            
            DisableSpriteRenderer();
            return;
        }
        
        direction = (currentTouchPosition - (Vector2)gameObject.transform.position).normalized;
        childObject.transform.position = ((Vector2)gameObject.transform.position + direction) ;
        PlayerCharacterController.instance.MoveVector = direction;
        
        if(GameManager.instance.touchManager.SwipeDirection != Vector2.zero){
            // ANCHOR : PlayerInput[direction:Vector2]&time:float
            StageManager.instance?.AddReplayData($"PlayerInput[{PlayerCharacterController.instance.MoveVector}]&{StageManager.instance?.scoreManager.TimeScore}");
        }
    }

    private void KeyBoardMove(){
        if(Input.anyKey){
            if(Input.GetKey(KeyCode.W)){
                PlayerCharacterController.instance.MoveVector += Vector2.up;
            }
            else if(Input.GetKey(KeyCode.S)){
                PlayerCharacterController.instance.MoveVector += Vector2.down;
            }

            if(Input.GetKey(KeyCode.A)){
                PlayerCharacterController.instance.MoveVector += Vector2.left;
            }
            else if(Input.GetKey(KeyCode.D)){
                PlayerCharacterController.instance.MoveVector += Vector2.right;
            }

            // ANCHOR : PlayerInput:direction:Vector2&time:float
            StageManager.instance?.AddReplayData($"PlayerInput:{PlayerCharacterController.instance.MoveVector}&{StageManager.instance?.scoreManager.TimeScore}");
        } 
    }

    private void EnableSpriteRenderer() {
        frameSpriteRenderer.enabled = true;
        childSpriteRenderer.enabled = true;
    }

    private void DisableSpriteRenderer() {
        frameSpriteRenderer.enabled = false;
        childSpriteRenderer.enabled = false;
    }
}
