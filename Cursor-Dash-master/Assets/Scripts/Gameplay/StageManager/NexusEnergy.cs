using UnityEngine;
using UnityEngine.UI;

public class NexusEnergy : MonoBehaviour
{
    private Image image;
    private Material material,original;
    private float displayed=1,target=1,impactStarted=float.NegativeInfinity;
    public const float ImpactSeconds=.12f;
    public float DisplayedLevel=>displayed;
    public float TargetLevel=>target;
    public float SurfacePress {get;private set;}
    public void Initialize(Image source,float fraction)
    {
        image=source;if(image==null)return;
        var shader=Resources.Load<Shader>("CursorDash/NexusEnergy");
        if(shader==null||!shader.isSupported)return;
        original=image.material;material=new Material(shader){hideFlags=HideFlags.DontSave};
        material.SetFloat("_SolidFraction",fraction);
        image.material=material;image.type=Image.Type.Simple;image.fillAmount=1;
        displayed=target=1;
        impactStarted=float.NegativeInfinity;SurfacePress=0;ApplyMaterial();
    }
    public void SetTarget(float level)
    {
        float next=Mathf.Clamp01(level);
        // Authoritative health is displayed immediately. A new hit replaces the brief surface response, never queues it.
        if(next<target&&next>0)impactStarted=Time.unscaledTime;
        else if(next>target||next<=0||next>=1)impactStarted=float.NegativeInfinity;
        displayed=target=next;
        if(image!=null)image.fillAmount=1;
        ApplyMaterial();
    }
    private void Update()
    {
        if(StageManager.instance!=null&&!StageManager.instance.IsPlaying)impactStarted=float.NegativeInfinity;
        ApplyMaterial();
    }
    private void ApplyMaterial()
    {
        float fade=1-Mathf.Clamp01((Time.unscaledTime-impactStarted)/ImpactSeconds);
        SurfacePress=Mathf.Clamp(CursorDashTuning.Number("nexus.waveAmplitude",.015f),0,.025f)*fade*fade;
        if(material==null)return;
        material.SetFloat("_Level",displayed);
        material.SetFloat("_ImpactPress",SurfacePress);
        material.SetColor("_Color",CursorDashTuning.ColorValue("nexus.color",new Color(.04f,.7f,.9f)));
    }
    private void OnDisable(){impactStarted=float.NegativeInfinity;SurfacePress=0;if(material!=null)material.SetFloat("_ImpactPress",0);}
    private void OnDestroy(){if(image!=null)image.material=original;if(material!=null)Destroy(material);}
}
