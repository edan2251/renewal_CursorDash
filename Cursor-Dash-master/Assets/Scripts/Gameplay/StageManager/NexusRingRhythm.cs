using UnityEngine;

/// <summary>Legacy identity retained. D-025 holds the outer ring still, without replacement motion.</summary>
[DisallowMultipleComponent]
public sealed class NexusRingRhythm : MonoBehaviour
{
    private Rotator legacy;
    private bool initialized,wasEnabled;
    public bool IsResting=>initialized;
    public int Segment=>0;
    public void Initialize(Rotator source){legacy=source;if(legacy==null)return;wasEnabled=legacy.enabled;initialized=true;legacy.enabled=false;}
    private void OnEnable(){if(initialized&&legacy!=null)legacy.enabled=false;}
    private void OnDisable(){if(initialized&&legacy!=null)legacy.enabled=wasEnabled;}
}
