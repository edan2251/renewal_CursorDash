using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LocalImpactDistortion : MonoBehaviour
{
    private struct Pulse { public Vector3 point; public float started,duration,radius,strength,recovery;public bool success,circle;public Vector4 response; }
    private readonly List<Pulse> pulses=new List<Pulse>();
    private readonly Vector4[] hits=new Vector4[8];
    private readonly Vector4[] shapes=new Vector4[8];
    private readonly Vector4[] responses=new Vector4[8];
    private Camera worldCamera,uiCamera;
    private Material material;
    private CursorForeground cursorForeground;
    public CursorForeground CursorLayer=>cursorForeground;
    private OriginalBackgroundFlash originalBackground;
    private bool ownsBackground;
    private NexusGameboardPresentation nexusPresentation;
    public FeverWorldTransition FeverPresentation {get;set;}
    public void SetNexusPresentation(NexusGameboardPresentation value){nexusPresentation=value;nexusPresentation?.SetDriverActive(isActiveAndEnabled);}
    public int ActiveCount => pulses.Count;
    public int RenderedEffectFrames { get; private set; }
    public Vector2 LastViewportCenter { get; private set; }
    public Camera UiCamera => uiCamera;
    public OriginalBackgroundFlash OriginalBackground => originalBackground;
    private void Awake()
    {
        worldCamera=GetComponent<Camera>();
        cursorForeground=gameObject.AddComponent<CursorForeground>();cursorForeground.Initialize(worldCamera);
        Shader shader=Resources.Load<Shader>("CursorDash/LocalImpact");
        if(shader!=null&&shader.isSupported)material=new Material(shader){hideFlags=HideFlags.DontSave};
        // Built-in pipeline: render world effect first, then a separate untouched UI camera.
        var item=new GameObject("CursorDash UI camera"){hideFlags=HideFlags.DontSave};
        uiCamera=item.AddComponent<Camera>();uiCamera.CopyFrom(worldCamera);
        uiCamera.transform.position=worldCamera.transform.position;uiCamera.transform.rotation=worldCamera.transform.rotation;
        uiCamera.clearFlags=CameraClearFlags.Depth;uiCamera.depth=worldCamera.depth+1;
        uiCamera.cullingMask=1<<5;uiCamera.tag="Untagged";worldCamera.cullingMask &= ~(1<<5);
        foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if(canvas.isRootCanvas&&canvas.renderMode==RenderMode.ScreenSpaceCamera&&canvas.worldCamera==worldCamera)canvas.worldCamera=uiCamera;
        // Use the existing background image. Do not synthesize an extra circuit mesh.
        originalBackground=GetComponent<OriginalBackgroundFlash>();
        if(originalBackground==null){originalBackground=gameObject.AddComponent<OriginalBackgroundFlash>();ownsBackground=true;}
    }
    public void Add(Vector3 point,float strength,float radius,float duration)
    {AddPulse(point,strength,radius,duration,2,false);}
    public void AddSuccess(Vector3 point,float strength,float radius,float duration,float recovery)
    {AddPulse(point,strength,radius,duration,recovery,true);}
    public void AddCircleSuccess(Vector3 point,float strength,float radius,float duration,float recovery)
    { originalBackground?.Hit(point);AddPulse(point,strength,radius,duration,recovery,true,true); }
    private void AddPulse(Vector3 point,float strength,float radius,float duration,float recovery,bool success,bool circle=false)
    {
        if(material==null||strength<=0)return;
        int cap=Mathf.Clamp(CursorDashTuning.Integer("fx.distortionSlots",4),1,8);
        pulses.RemoveAll(p=>Time.unscaledTime-p.started>=p.duration);
        // Merge only nearby impacts, keeping their own contact point. Never move events to the core.
        int close=pulses.FindIndex(p=>(p.point-point).sqrMagnitude<0.0025f);
        var pulse=new Pulse{point=point,started=Time.unscaledTime,duration=Mathf.Max(.01f,duration),radius=radius,
            strength=Mathf.Min(strength,success?CursorDashTuning.Number("visual.successDistortionCap",.06f):CursorDashTuning.Number("fx.distortionCap",.025f)),success=success,circle=circle,recovery=recovery,
            response=success&&!circle?new Vector4(CursorDashTuning.Number("visual.rippleStrength",.8f),CursorDashTuning.Number("visual.rippleSpeed",6),
                CursorDashTuning.Number("visual.rippleWidth",.35f),CursorDashTuning.Number("visual.impactExposure",.65f)):Vector4.zero};
        if(close>=0)pulses[close]=pulse;
        else {if(pulses.Count>=cap)pulses.RemoveAt(0);pulses.Add(pulse);}
    }
    public void Clear() { pulses.Clear();originalBackground?.Clear();FeverPresentation?.ClearDamage(); }
    private void OnRenderImage(RenderTexture source,RenderTexture destination)
    {
        pulses.RemoveAll(p=>Time.unscaledTime-p.started>=p.duration);
        bool enabled=CursorDashTuning.Toggle("fx.enabled",true);
        bool renderEffect=material!=null&&pulses.Count>0&&enabled;
        float user=CursorDashTuning.EffectGain("user.distortion");
        bool success=false;
        for(int i=0;i<hits.Length;i++)
        {
            hits[i]=shapes[i]=responses[i]=Vector4.zero;if(!renderEffect||i>=pulses.Count)continue;
            var p=pulses[i];Vector3 uv=worldCamera.WorldToViewportPoint(p.point);
            LastViewportCenter=uv;
            float fade=Mathf.Pow(1-Mathf.Clamp01((Time.unscaledTime-p.started)/p.duration),2);
            hits[i]=new Vector4(uv.x,uv.y,p.radius,p.strength*(p.success?1:fade)*user);
            shapes[i]=new Vector4(p.circle?2:p.success?1:0,Mathf.Clamp01((Time.unscaledTime-p.started)/p.duration),p.recovery,p.duration);
            responses[i]=p.response;responses[i].w*=PlayerPrefs.GetString("OnFlash","true")=="true"?CursorDashTuning.EffectGain("user.flash"):0;success|=p.success;
        }
        float cap=success?CursorDashTuning.Number("visual.successDistortionCap",.06f):CursorDashTuning.Number("fx.distortionCap",.025f);
        bool composite=originalBackground!=null&&originalBackground.CanComposite;
        RenderTexture board=nexusPresentation!=null&&nexusPresentation.CanComposite?RenderTexture.GetTemporary(source.width,source.height,0,source.format):null;
        RenderTexture cursorBoard=cursorForeground!=null?RenderTexture.GetTemporary(source.width,source.height,0,source.format):null;
        RenderTexture opticalBoard=FeverPresentation!=null&&FeverPresentation.ShouldRenderOptics?RenderTexture.GetTemporary(source.width,source.height,0,source.format):null;
        RenderTexture finalOutput=opticalBoard!=null?opticalBoard:destination;
        RenderTexture afterNexus=cursorBoard!=null?cursorBoard:finalOutput;
        RenderTexture final=board!=null?board:afterNexus;
        RenderTexture feverBoard=FeverPresentation!=null&&FeverPresentation.isActiveAndEnabled?RenderTexture.GetTemporary(source.width,source.height,0,source.format):null;
        try{
        if(!renderEffect)
        {
            if(composite)originalBackground.Composite(source,final,hits,shapes,cap);else Graphics.Blit(source,final);
        }
        else{
        material.SetVectorArray("_Hits",hits);
        material.SetVectorArray("_Shapes",shapes);material.SetVectorArray("_Responses",responses);material.SetFloat("_SuccessActive",success?1:0);
        float shorter=Mathf.Min(source.width,source.height);
        material.SetVector("_Aspect",new Vector4(source.width/shorter,source.height/shorter,0,0));
        material.SetFloat("_Clock",Time.unscaledTime);material.SetFloat("_OffsetCap",cap);
        RenderTexture world=null,small=null;
        try{
            if(composite)world=RenderTexture.GetTemporary(source.width,source.height,0,source.format);
            RenderTexture output=composite?world:final;
            float scale=CursorDashTuning.Number("fx.renderScale",1);
            if(scale<.99f){
                small=RenderTexture.GetTemporary(Mathf.Max(64,Mathf.RoundToInt(source.width*scale)),Mathf.Max(64,Mathf.RoundToInt(source.height*scale)),0,source.format);
                Graphics.Blit(source,small);Graphics.Blit(small,output,material);
            }else Graphics.Blit(source,output,material);
            if(composite)originalBackground.Composite(world,final,hits,shapes,cap);
        }finally{if(small!=null)RenderTexture.ReleaseTemporary(small);if(world!=null)RenderTexture.ReleaseTemporary(world);}
        RenderedEffectFrames++;
        }
        // Nexus sits above the original background, in the same board displacement/exposure, before the stable HUD.
        if(board!=null)nexusPresentation.Composite(board,afterNexus,hits,shapes,responses,cap,success);
        if(feverBoard!=null)FeverPresentation.Composite(afterNexus,feverBoard,hits,shapes,responses,cap,success);
        if(cursorBoard!=null)cursorForeground.Composite(feverBoard!=null?feverBoard:cursorBoard,finalOutput,hits,shapes,responses,cap,success);
        else if(feverBoard!=null)Graphics.Blit(feverBoard,finalOutput);
        if(opticalBoard!=null)FeverPresentation.FinishBoard(opticalBoard,destination);
        }finally{if(board!=null)RenderTexture.ReleaseTemporary(board);if(cursorBoard!=null)RenderTexture.ReleaseTemporary(cursorBoard);if(feverBoard!=null)RenderTexture.ReleaseTemporary(feverBoard);if(opticalBoard!=null)RenderTexture.ReleaseTemporary(opticalBoard);}
    }
    private void OnEnable(){if(cursorForeground!=null)cursorForeground.enabled=true;if(ownsBackground&&originalBackground!=null)originalBackground.enabled=true;nexusPresentation?.SetDriverActive(true);}
    private void OnDisable(){Clear();if(cursorForeground!=null)cursorForeground.enabled=false;if(ownsBackground&&originalBackground!=null)originalBackground.enabled=false;nexusPresentation?.SetDriverActive(false);}
    private void OnDestroy(){if(material!=null)Destroy(material);if(uiCamera!=null)Destroy(uiCamera.gameObject);if(cursorForeground!=null)Destroy(cursorForeground);if(ownsBackground&&originalBackground!=null)Destroy(originalBackground);}
}
