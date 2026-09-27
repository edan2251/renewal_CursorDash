using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCharacterAfterImage : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField]
    private GameObject trailCollisions;

    private List<BoxCollider2D> collisionObjects = new List<BoxCollider2D>();

    private void Awake(){
        collisionObjects = trailCollisions.GetComponentsInChildren<BoxCollider2D>(true).ToList();
    }

    private void Start(){
        StartCoroutine(UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine(){
        GameObject collisionObject;

        while(true){
            collisionObject = GetAvailableCollisionObject();

            if(collisionObject != null){
                collisionObject.transform.position = gameObject.transform.position;
                collisionObject.SetActive(true);

                StartCoroutine(OffAfterImageCollision(collisionObject));
            }

            yield return YieldInstructionCache.WaitFrame;
        }
    }

    private GameObject GetAvailableCollisionObject(){
        for(int i = 0; i < collisionObjects.Count; i++){
            if(collisionObjects[i].gameObject.activeInHierarchy.Equals(false)){
                return collisionObjects[i].gameObject;
            }
        }

        return null;
    }


    // FIXME : 아무리봐도 먼가 비효율적임
    private IEnumerator OffAfterImageCollision(GameObject collisionObject){
        yield return YieldInstructionCache.WaitingSeconds(0.5f);
        collisionObject.SetActive(false);
    }

}
