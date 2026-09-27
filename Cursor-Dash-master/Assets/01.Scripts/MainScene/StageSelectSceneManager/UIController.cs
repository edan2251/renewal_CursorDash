using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour, ITouchUpDownObserver
{
    private void Awake() {
        GameManager.instance.touchManager.AddObserver(this);
    }

    public void TouchDownNotify() {
        RaycastSettingUI()?.Execute();
    }

    public void TouchUpNotify() {
        
    }

    private SettingUI RaycastSettingUI() {
        Ray ray = new Ray();
        
        ray.origin = GameManager.instance.touchManager.TouchPosition;
        ray.direction = Vector2.zero;

        var rayHit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("SettingUI"));

        if (rayHit.collider == null) {
            return null;
        }
        
        return rayHit.collider.GetComponent<SettingUI>();
    }
}
