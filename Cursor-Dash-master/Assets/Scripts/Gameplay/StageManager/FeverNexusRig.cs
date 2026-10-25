using UnityEngine;
using UnityEngine.UI;

// Owns only the dotted ring: bounded spin impulses, staggered absorption, reversible temporary transforms.
public sealed class FeverNexusRig
{
    private Transform dotted;private Rotator rotator;private Vector3 scale;private Quaternion rotation;
    private bool bound,enabledBefore;private float direction=1,angle;private readonly FeverRingSpin spin=new FeverRingSpin();
    public bool Bound=>bound;
    public double Speed=>spin.Speed;
    public void Bind(NexusGameboardPresentation view)
    {
        if(bound||view?.SourceImages==null)return;
        foreach(Image image in view.SourceImages)if(image.name=="HpCentral"){
            dotted=image.transform;scale=dotted.localScale;rotation=dotted.localRotation;angle=dotted.localEulerAngles.z;
            rotator=image.GetComponent<Rotator>();if(rotator!=null){enabledBefore=rotator.enabled;direction=rotator.DirectionSign;rotator.enabled=false;}bound=true;break;
        }
    }
    public void Pulse(bool damage)
    {
        spin.Pulse(damage);
    }
    public void Begin()=>spin.Begin();
    public void Advance(float delta,FeverState.Phase phase,float progress)
    {
        if(!bound||dotted==null)return;
        if(delta>0){
            angle=Mathf.Repeat(angle+direction*(float)spin.Advance(delta,phase,progress)*Mathf.Max(0,CursorDashTuning.Number("nexus.spin",1)),360);
        }
        dotted.localRotation=Quaternion.Euler(0,0,angle);Draw(phase,progress);
    }
    public void Draw(FeverState.Phase phase,float progress)
    {
        if(!bound||dotted==null)return;
        float s=phase==FeverState.Phase.Entering?(float)FeverFlowMath.DotScale(false,progress):phase==FeverState.Phase.Fever?0:phase==FeverState.Phase.Exiting?(float)FeverFlowMath.DotScale(true,progress):1;
        dotted.localScale=scale*s;
    }
    public void Restore(bool completed){if(bound&&dotted!=null)dotted.localScale=scale;}
    public void Release()
    {
        if(!bound)return;if(dotted!=null){dotted.localScale=scale;dotted.localRotation=rotation;}if(rotator!=null)rotator.enabled=enabledBefore;bound=false;spin.Clear();
    }
}
