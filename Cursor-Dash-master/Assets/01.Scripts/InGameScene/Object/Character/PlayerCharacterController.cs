using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
    [SerializeField]
    private float moveSpeed;
    private Vector2 moveVector = Vector2.zero;

    private float sideMin = 0.1f;
    private float sideMax = 0.9f;

    private Vector2 beforePosition;
    private SpriteRenderer spriteRenderer;

    private TrailRenderer trailRenderer;
    public Vector2 MoveVector{
        get{
            return moveVector;
        }

        set{
            moveVector = value;
            moveVector = moveVector.normalized;
        }
    }
    
    private void Awake(){
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = GameManager.instance.playerData.characterSetting.selectSprite.CharacterSprite;
        spriteRenderer.color = GameManager.instance.playerData.characterSetting.selectColor.ColorValue;
    }

    // private void Start(){
    //     Color spriteColor = gameObject.GetComponent<SpriteRenderer>().color;
    //
    //     trailRenderer.startColor = spriteColor;
    //
    //     spriteColor.a = 0.8f;
    //
    //     trailRenderer.endColor = spriteColor;
    // }

    private void FixedUpdate(){
        Move();
    }

    private void Move(){
        if(moveVector.Equals(Vector2.zero)){
            return;
        }

        Vector2 playerCharacterViewPoint; 
        Vector2 playerNewPosition; 
        
        playerNewPosition = (Vector2)gameObject.transform.position + (moveVector * moveSpeed);

        // playerNewPosition = Vector2.Lerp(gameObject.transform.position, moveVector, 0.1f);

        gameObject.transform.LookAt2D(playerNewPosition);
        gameObject.transform.position = playerNewPosition;

        playerCharacterViewPoint = Camera.main.WorldToViewportPoint(gameObject.transform.position);

        LimitSide(ref playerCharacterViewPoint.x);
        LimitSide(ref playerCharacterViewPoint.y);

        playerCharacterViewPoint = Camera.main.ViewportToWorldPoint(playerCharacterViewPoint);
        gameObject.transform.position = playerCharacterViewPoint;

        beforePosition = gameObject.transform.position;
    }

    private void LimitSide(ref float positionValue){
        if(positionValue > sideMax){
            positionValue = sideMax;
        }
        
        if(positionValue < sideMin){
            positionValue = sideMin;
        }
    }
    
    public void Dash(float dashDistance){
        var playerNewPosition = (Vector2)gameObject.transform.position + (moveVector * dashDistance);

        gameObject.transform.LookAt2D(playerNewPosition);
        gameObject.transform.position = playerNewPosition;

        Vector2 playerCharacterViewPosition = Camera.main.WorldToViewportPoint(gameObject.transform.position);

        LimitSide(ref playerCharacterViewPosition.x);
        LimitSide(ref playerCharacterViewPosition.y);

        playerCharacterViewPosition = Camera.main.ViewportToWorldPoint(playerCharacterViewPosition);
        gameObject.transform.position = playerCharacterViewPosition;

        beforePosition = gameObject.transform.position;
    }

    public void SpeedAcceleration(float speed){
        StartCoroutine(SpeedCoroutine(speed));   
    }

    public void SpeedDecelration(float speed){
        StartCoroutine(SpeedCoroutine(-speed));
    }

    public void MoveStop(){
        moveSpeed = 0;
    }

    private IEnumerator SpeedCoroutine(float speed){
        float changeSpeed = (moveSpeed / speed) * 2;
        moveSpeed += changeSpeed;
        yield return YieldInstructionCache.WaitingSeconds(1.5f);
        moveSpeed -= changeSpeed;
    }

    public void OnTriggerEnter2D(Collider2D ohter){
        if(ohter.transform.CompareTag("Node")){
            Node node = ohter.GetComponent<Node>();
            node.Interaction();
            node.ShowEffect(gameObject.transform.position);
        }
    }
}
