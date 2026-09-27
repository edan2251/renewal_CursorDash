using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ExtensionMethods {  
    public static float Distance(this Vector2 fisrtPosition, Vector2 secondPosition){
        return Mathf.Abs((fisrtPosition - secondPosition).magnitude);
    }

    public static void LookAt2D(this Transform transform, Vector2 position){
         float angle = Mathf.Atan2(
            transform.position.y - position.y, 
            transform.position.x - position.x
        ) * 180 / Mathf.PI;

        angle += 90;

        transform.localRotation = Quaternion.Euler(0,0, angle);
    }

    public static Vector2 position(this Transform transform){
        return (Vector2)transform.position;
    }
}
