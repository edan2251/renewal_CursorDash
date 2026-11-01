using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Only the handle owns sheet drag. Sliders never bubble a sheet gesture.</summary>
public sealed class CursorDashSheetHandle : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    public CursorDashSettingsUI owner;private int finger=int.MinValue;private Vector2 start;private bool moved;
    public void OnPointerDown(PointerEventData e){if(finger!=int.MinValue)return;finger=e.pointerId;start=e.position;moved=false;owner?.BeginHandle(e.position);e.Use();}
    public void OnDrag(PointerEventData e){if(e.pointerId!=finger)return;moved|=(e.position-start).sqrMagnitude>64;owner?.DragHandle(e.position);e.Use();}
    public void OnPointerUp(PointerEventData e){if(e.pointerId!=finger)return;finger=int.MinValue;owner?.EndHandle(moved);e.Use();}
    private void OnDisable(){if(finger==int.MinValue)return;finger=int.MinValue;owner?.EndHandle(true);}
}
