using UnityEngine;
using UnityEngine.UI;

// Owns only the dotted ring: bounded spin impulses, staggered absorption, reversible temporary transforms.
public sealed class FeverNexusRig
{
    private Transform dotted;private Rotator rotator;private Vector3 scale;private Quaternion rotation;
    private bool bound,enabledBefore;private float direction=1,angle;private readonly FeverRingSpin spin=new FeverRingSpin();
    private FeverExitMotion recall;private Image dottedImage;private float initialAlpha;private NexusGameboardPresentation presentation;
    private double actualSpeed=24;
    public bool Bound=>bound;
    public double Speed=>actualSpeed;
    public float RestRadius {
        get {var rect=dotted as RectTransform;if(rect==null||presentation==null||dotted.parent==null)return 0;
            Vector3 a=Vector3.Scale(rect.rect.center,scale),b=Vector3.Scale((Vector3)rect.rect.center+Vector3.right*rect.rect.width*.5f,scale);
            Vector3 p=dotted.parent.TransformPoint(dotted.localPosition+rotation*a),q=dotted.parent.TransformPoint(dotted.localPosition+rotation*b);
            return Vector2.Distance(presentation.MapVisualPoint(p),presentation.MapVisualPoint(q))*.94f;}
    }
    public void Bind(NexusGameboardPresentation view)
    {
        if(bound||view?.SourceImages==null)return;
        foreach(Image image in view.SourceImages)if(image.name=="HpCentral"){
            presentation=view;dottedImage=image;initialAlpha=image.color.a;
            dotted=image.transform;scale=dotted.localScale;rotation=dotted.localRotation;angle=dotted.localEulerAngles.z;
            rotator=image.GetComponent<Rotator>();if(rotator!=null){enabledBefore=rotator.enabled;direction=rotator.DirectionSign;rotator.enabled=false;}bound=true;break;
        }
    }
    public void Pulse(bool damage)
    {
        spin.Pulse(damage);
    }
    public void EntryPulse(float strength,float seconds)=>spin.EntryPulse(strength,seconds);
    public void Begin(){recall=null;SetOpacity(1);spin.Begin();}
    public void BeginExit(FeverExitMotion motion){recall=motion;spin.Clear();Draw(FeverState.Phase.Exiting,0,0);}
    private void SetOpacity(float opacity){if(dottedImage==null)return;var color=dottedImage.color;color.a=initialAlpha*opacity;dottedImage.color=color;}
    public void Advance(float delta,FeverState.Phase phase,float progress,double elapsed=0)
    {
        if(!bound||dotted==null)return;
        if(delta>0){
            float multiplier=Mathf.Max(0,CursorDashTuning.Number("nexus.spin",1));
            if(phase==FeverState.Phase.Exiting&&recall!=null){spin.AdvanceExit(delta,elapsed,recall);actualSpeed=System.Math.Min(recall.MaxSpin,spin.Speed*multiplier);}
            else{spin.Advance(delta,phase,progress);actualSpeed=spin.Speed*multiplier;}
            angle=Mathf.Repeat(angle+direction*(float)(actualSpeed*delta),360);
        }
        dotted.localRotation=Quaternion.Euler(0,0,angle);Draw(phase,progress,elapsed);
    }
    public void Draw(FeverState.Phase phase,float progress,double elapsed=0)
    {
        if(!bound||dotted==null)return;
        float s=phase==FeverState.Phase.Entering?(float)FeverFlowMath.DotScale(false,progress):phase==FeverState.Phase.Fever?0:phase==FeverState.Phase.Exiting&&recall!=null?(float)recall.RingScale(elapsed):1;
        SetOpacity(phase==FeverState.Phase.Exiting&&recall!=null?(float)recall.RingAlpha(elapsed):1);
        dotted.localScale=scale*s;
    }
    public void Restore(bool completed){if(bound&&dotted!=null)dotted.localScale=scale;SetOpacity(1);spin.Clear();actualSpeed=24;recall=null;}
    public void GuideReset(float progress,float delta)
    {
        if(!bound||dotted==null)return;
        Advance(delta,FeverState.Phase.Normal,0);float amount=(float)CursorDashGuideFlow.ResetRing(progress);
        dotted.localScale=scale*amount;SetOpacity(Mathf.Clamp01(amount*2));
    }
    public void Suspend()
    {
        if(!bound)return;if(dotted!=null){dotted.localScale=scale;dotted.localRotation=rotation;angle=rotation.eulerAngles.z;}SetOpacity(1);recall=null;spin.Clear();actualSpeed=24;
        // Retain ownership while disabled: enabling Rotator here can recreate DOTween during scene teardown.
    }
    public void Release(bool resumeLegacy=true)
    {
        if(!bound)return;Suspend();if(resumeLegacy&&rotator!=null)rotator.enabled=enabledBefore;bound=false;
    }
}
