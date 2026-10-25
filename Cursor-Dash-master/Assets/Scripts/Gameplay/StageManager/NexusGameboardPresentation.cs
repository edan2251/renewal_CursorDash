using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Projects only the existing nexus images into the board effect pass. Their layout/boundary sources stay untouched.</summary>
public sealed class NexusGameboardPresentation : MonoBehaviour
{
    private Image[] sources;
    private bool[] previousEnabled;
    private Image body;
    private Camera boardCamera;
    private StageManager stage;
    private Material material;
    private float solidFraction;
    private int energyIndex;
    private bool driverActive,hidden;
    public bool CursorForegroundBound=>stage!=null&&stage.Feedback?.Distortion?.CursorLayer!=null&&stage.Feedback.Distortion.CursorLayer.Bound;
    public string BindingStatus {get;private set;}="초기화 대기";
    public float FinaleScale {get;set;}=1;
    public float FinaleAlpha {get;set;}=1;
    public Camera GameboardCamera=>boardCamera;
    public int RenderedFrames {get;private set;}
    private static readonly string[] LayerTextures={"_Layer0Tex","_Layer1Tex","_Layer2Tex"};
    private readonly Vector3[] corners=new Vector3[4];
    private readonly Vector4[] rectU=new Vector4[3],rectV=new Vector4[3],tints=new Vector4[3],textureRects=new Vector4[3];
    public bool CanComposite=>material!=null&&sources!=null&&driverActive&&isActiveAndEnabled;
    public Image[] SourceImages=>sources;
    private Vector3 RawCenter=>body==null?Vector3.zero:body.rectTransform.TransformPoint(body.rectTransform.rect.center);
    private float RawRadius=>body==null?0:Mathf.Min(Mathf.Abs(body.rectTransform.rect.width*body.rectTransform.lossyScale.x),Mathf.Abs(body.rectTransform.rect.height*body.rectTransform.lossyScale.y))*.5f*solidFraction;
    public float LayoutScale=>stage!=null&&stage.NexusRadius>0&&RawRadius>.000001f?stage.NexusRadius/RawRadius:1;
    public Vector2 WorldCenter=>(Vector2)MapToBoundary(RawCenter);
    public float BodyRadius=>RawRadius*LayoutScale;
    public Vector3 MapVisualPoint(Vector3 point)=>MapToBoundary(point);
    private Vector3 MapToBoundary(Vector3 point)
    {
        if(stage==null||stage.NexusRadius<=0)return point;
        Vector3 center=RawCenter;Vector2 anchor=stage.NexusPosition;float scale=LayoutScale;
        return new Vector3(anchor.x+(point.x-center.x)*scale*FinaleScale,anchor.y+(point.y-center.y)*scale*FinaleScale,point.z);
    }

    public void Initialize(Image energy,Image frame,float fraction,LocalImpactDistortion driver)
    {
        var center=energy==null||energy.transform.parent==null?null:energy.transform.parent.Find("HpCentral");
        Image central=center==null?null:center.GetComponent<Image>();
        Shader shader=Resources.Load<Shader>("CursorDash/NexusGameboard");
        if(energy==null||frame==null||central==null||driver==null||shader==null||!shader.isSupported){BindingStatus="필수3이미지/카메라/셰이더 누락 또는 미지원";Debug.LogWarning("CursorDash nexus: "+BindingStatus);return;}
        Image[] candidates={energy,frame,central};
        foreach(var image in candidates)
            if(image.sprite==null||image.sprite.packed||image.preserveAspect||
                (image.type!=Image.Type.Simple&&(image.type!=Image.Type.Filled||image.fillAmount<1))){BindingStatus="지원하지 않는 원본 이미지: "+image.name;Debug.LogWarning("CursorDash nexus: "+BindingStatus);return;}
        if(energy.material==null||energy.material.shader.name!="CursorDash/NexusEnergy"){BindingStatus="본체 NexusEnergy 셰이더 확인 필요";return;}
        if(frame.material.shader.name!="UI/Default"||central.material.shader.name!="UI/Default"){BindingStatus="장식 원본 UI 셰이더 확인 필요";return;}
        Array.Sort(candidates,(a,b)=>a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        body=energy;solidFraction=fraction;sources=candidates;energyIndex=Array.IndexOf(sources,energy);
        previousEnabled=new bool[sources.Length];for(int i=0;i<sources.Length;i++)previousEnabled[i]=sources[i].enabled;
        stage=GetComponent<StageManager>();boardCamera=driver.GetComponent<Camera>();material=new Material(shader){hideFlags=HideFlags.DontSave};
        BindingStatus="게임판 합성 등록 완료";driver.SetNexusPresentation(this);
    }
    public void SetDriverActive(bool value)
    {
        driverActive=value;
        if(CanComposite){if(!hidden){for(int i=0;i<sources.Length;i++)sources[i].enabled=false;hidden=true;}}
        else RestoreImages();
    }
    public void Composite(RenderTexture input,RenderTexture output,Vector4[] hits,Vector4[] shapes,Vector4[] responses,float cap,bool success)
    {
        if(!CanComposite){Graphics.Blit(input,output);return;}
        Vector2 low=new Vector2(float.PositiveInfinity,float.PositiveInfinity),high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
        for(int i=0;i<sources.Length;i++)
        {
            Image source=sources[i];source.rectTransform.GetWorldCorners(corners);
            // Follow the existing stable gameplay circle even if CanvasScaler relayouts after a Game View resize.
            Vector2 a=boardCamera.WorldToViewportPoint(MapToBoundary(corners[0]));
            Vector2 u=(Vector2)boardCamera.WorldToViewportPoint(MapToBoundary(corners[3]))-a;
            Vector2 v=(Vector2)boardCamera.WorldToViewportPoint(MapToBoundary(corners[1]))-a;
            foreach(Vector2 corner in new[]{a,a+u,a+v,a+u+v}){low=Vector2.Min(low,corner);high=Vector2.Max(high,corner);}
            rectU[i]=new Vector4(a.x,a.y,u.x,u.y);rectV[i]=new Vector4(v.x,v.y,0,0);
            Color tint=source.color;tint.a*=FinaleAlpha;if(!source.gameObject.activeInHierarchy||!previousEnabled[i])tint.a=0;tints[i]=tint;
            Sprite sprite=source.sprite;Rect uv=sprite.textureRect;Texture texture=sprite.texture;
            textureRects[i]=new Vector4(uv.x/texture.width,uv.y/texture.height,uv.width/texture.width,uv.height/texture.height);
            material.SetTexture(LayerTextures[i],texture);
        }
        material.SetVectorArray("_RectsU",rectU);material.SetVectorArray("_RectsV",rectV);
        material.SetVectorArray("_Tints",tints);material.SetVectorArray("_TextureRects",textureRects);
        material.SetInt("_EnergyIndex",energyIndex);
        Material energyMaterial=body.material;
        material.SetFloat("_Level",energyMaterial.GetFloat("_Level"));material.SetFloat("_SolidFraction",solidFraction);
        material.SetFloat("_ImpactPress",energyMaterial.GetFloat("_ImpactPress"));material.SetColor("_EnergyTint",energyMaterial.GetColor("_Color"));
        float shorter=Mathf.Min(input.width,input.height);
        material.SetVector("_Aspect",new Vector4(input.width/shorter,input.height/shorter,0,0));
        material.SetVectorArray("_Hits",hits);material.SetVectorArray("_Shapes",shapes);material.SetVectorArray("_Responses",responses);
        material.SetFloat("_Clock",Time.unscaledTime);material.SetFloat("_OffsetCap",cap);material.SetFloat("_SuccessActive",success?1:0);
        var fever=stage.Fever;var phase=fever==null?FeverState.Phase.Normal:fever.Model.Current;double progress=fever==null?0:fever.Model.Progress;
        Vector2 center=boardCamera.WorldToViewportPoint(stage.NexusPosition),edge=boardCamera.WorldToViewportPoint(stage.NexusPosition+Vector2.right*stage.NexusRadius);
        material.SetVector("_CoreCircle",new Vector4(center.x,center.y,Mathf.Abs(edge.x-center.x)*input.width/shorter,0));
        material.SetFloat("_VisualBodyScale",(float)FeverFlowMath.BodyScale(phase,progress,fever?.Visuals==null?.115:fever.Visuals.feverGatherCompression));material.SetFloat("_GatherLight",(float)FeverFlowMath.GatherLight(phase,progress));
        material.SetFloat("_Absorbing",phase==FeverState.Phase.Entering||phase==FeverState.Phase.Exiting?1:0);
        material.SetVector("_Bounds",new Vector4(low.x-cap,low.y-cap,high.x+cap,high.y+cap));
        Graphics.Blit(input,output,material);
        RenderedFrames++;
    }
    private void RestoreImages()
    {
        if(!hidden||sources==null)return;
        for(int i=0;i<sources.Length;i++)if(sources[i]!=null)sources[i].enabled=previousEnabled[i];
        hidden=false;
    }
    private void OnEnable(){if(driverActive)SetDriverActive(true);}
    private void OnDisable(){RestoreImages();}
    private void OnDestroy(){RestoreImages();if(material!=null)Destroy(material);}
}
