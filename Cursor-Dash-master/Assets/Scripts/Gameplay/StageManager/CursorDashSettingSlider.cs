using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CursorDashSettingSlider : Slider
{
    public Action finished;
    public override void OnPointerUp(PointerEventData e){base.OnPointerUp(e);finished?.Invoke();}
    public override void OnDeselect(BaseEventData e){base.OnDeselect(e);finished?.Invoke();}
    public override void OnMove(AxisEventData e){base.OnMove(e);finished?.Invoke();}
}
