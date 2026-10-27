using UnityEngine;
using UnityEngine.UI;

// Explicit gameboard pass: after Nexus, before actual cursor and stable HUD. No independent OnRenderImage.
[RequireComponent(typeof(Camera))]
public sealed class FeverWorldTransition : MonoBehaviour
{
    private Material material,optics;private Camera board;private StageManager stage;private CursorDashFever fever;private FeverElementMask elements;
    private readonly Vector3[] points=new Vector3[6];private readonly float[] started=new float[6];private int next,count;
    private readonly Vector4[] pulses=new Vector4[6];private readonly Vector3[] corners=new Vector3[4];
    private Vector4 centerAspect,flow;
    public int RenderedFrames {get;private set;}
    public bool Supported=>material!=null;
    public int OpticsFrames {get;private set;}
    public bool OpticsSupported=>optics!=null;
    private readonly Vector4[] opticalPulses=new Vector4[6];
    private float damageAt=float.NegativeInfinity,damageSeconds;private Vector4 damageField;private float damageChroma;
    private Vector4 damageShape;private Vector2 damageWorldPoint;private bool damageHasContact;
    public int DamageExecutions {get;private set;}
    public float DamageChromatic=>damageChroma;
    public bool DamageUsesSpatialWave=>damageHasContact;
    public bool DamageActive=>damageSeconds>0&&Time.unscaledTime-damageAt<damageSeconds;
    public Vector2 DamageViewportCenter {get;private set;}
    public void NexusDamaged(float seconds,bool wave=true,Vector2? contact=null)
    {
        if(!CursorDashTuning.Toggle("fx.enabled",true))return;var v=fever.Visuals;
        DamageExecutions++;damageWorldPoint=contact??stage.NexusPosition;damageHasContact=wave;
        damageAt=Time.unscaledTime;damageSeconds=Mathf.Clamp(seconds,.1f,.5f);damageChroma=wave?Mathf.Clamp(v==null?.018f:v.nexusDamageChromatic,0,.024f):.0055f;
        damageField=new Vector4(wave&&(v==null||v.nexusDamageWaveEnabled)?Mathf.Clamp(v==null?.03f:v.nexusDamageWaveStrength,0,.035f):0,Mathf.Clamp(v==null?.24f:v.nexusDamageWaveWidth,.03f,.35f),Mathf.Clamp(v==null?4.2f:v.nexusDamageWaveSpeed,.4f,6),Mathf.Clamp(v==null?1.25f:v.nexusDamageWaveRange,.2f,1.6f));
        damageShape=new Vector4(Mathf.Clamp(v==null?.85f:v.nexusDamageCompression,0,1.2f),Mathf.Clamp(v==null?1.4f:v.nexusDamageReleasePower,.6f,2.4f),wave?Mathf.Clamp(v==null?1.15f:v.nexusDamageSpatialLight,0,1.8f):0,0);
        // Keep the false-wave API for legacy diagnostics; every confirmed HP loss now uses wave=true.
        if(!wave){damageField=new Vector4(0,.12f,2.6f,.72f);damageShape=new Vector4(.85f,1.4f,0,0);}
    }
    public void ClearDamage(){damageAt=float.NegativeInfinity;damageSeconds=0;if(optics!=null)optics.SetVector("_NexusDamage",Vector4.zero);}
    public bool ShouldRenderOptics
    {
        get {
            if(optics==null||fever?.Model==null||fever.ContactDiagnostics||!CursorDashTuning.Toggle("fx.enabled",true))return false;
            if(DamageActive&&(damageField.x>0||damageChroma>0)&&CursorDashTuning.EffectGain("user.distortion")>0)return true;
            if(fever.Model.Current==FeverState.Phase.Exiting&&fever.Exit!=null)return fever.Exit.Pressure(fever.Model.Elapsed)>0&&((CursorDashTuning.EffectGain("user.distortion")>0&&(fever.Exit.Chromatic>0||fever.Exit.Lens>0))||(CursorDashTuning.EffectGain("user.shake")>0&&PlayerPrefs.GetString("OnVibe","true")=="true"&&fever.Exit.Shake>0));
            if(fever.Model.Current!=FeverState.Phase.Fever||CursorDashTuning.EffectGain("user.distortion")<=0)return false;
            var v=fever.Visuals;if((v==null?.0018f:v.feverChromaticAmbient)>0)return true;
            float seconds=v==null?.30f:Mathf.Clamp(v.feverOpticsSeconds,.1f,.6f);
            for(int i=0;i<count;i++)if(fever.Clock-started[i]<seconds)return true;
            return false;
        }
    }
    public float WhiteAtViewport(Vector2 uv)
    {
        if(board==null||fever?.Model==null||stage==null)return 0;
        var m=fever.Model;if(!m.SuppressesThreats)return 0;if(m.Current==FeverState.Phase.Fever)return 1;
        var exit=fever.Exit;double u=exit==null?0:exit.Suction(m.Elapsed);
        float t=(float)(m.Current==FeverState.Phase.Entering?FeverFlowMath.EntrySpread(m.Progress):exit==null?0:1-FeverExitMotion.Travel(u,exit.Power));
        if(t<=0)return 0;if(t>=1)return 1;Vector2 center=board.WorldToViewportPoint(stage.NexusPosition);
        float ax=Mathf.Max(1,board.aspect),ay=Mathf.Max(1,1/board.aspect);
        double x=(uv.x-center.x)*ax,y=(uv.y-center.y)*ay,extent=Extent(center,ax,ay);
        double d=m.Current==FeverState.Phase.Exiting?FeverFlowMath.RecallField(x,y,u,extent,exit.Power,exit.Twist):FeverFlowMath.SignedField(x,y,t,extent);
        return 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.006f,.006f,(float)d));
    }
    public void Initialize(CursorDashFever owner,StageManager game)
    {
        fever=owner;stage=game;board=GetComponent<Camera>();var shader=Resources.Load<Shader>("CursorDash/FeverWorld");
        if(shader==null||!shader.isSupported){Debug.LogWarning("CursorDash fever: world shader unavailable.");return;}
        material=new Material(shader){hideFlags=HideFlags.DontSave};elements=new FeverElementMask(game);
        var opticalShader=Resources.Load<Shader>("CursorDash/FeverImpact");
        if(opticalShader!=null&&opticalShader.isSupported)optics=new Material(opticalShader){hideFlags=HideFlags.DontSave};
        else Debug.LogWarning("CursorDash fever: final optical shader unavailable; board presentation retained.");
        var driver=GetComponent<LocalImpactDistortion>();if(driver!=null)driver.FeverPresentation=this;
    }
    public void Hit(Vector2 point){points[next]=point;started[next]=fever.Clock;next=(next+1)%points.Length;count=Mathf.Min(count+1,points.Length);}
    public void Clear(){count=next=0;ClearDamage();if(optics!=null)optics.SetVector("_ExitOptics",Vector4.zero);}
    public static float Extent(Vector2 center,float ax,float ay){float x=Mathf.Max(center.x,1-center.x)*ax,y=Mathf.Max(center.y,1-center.y)*ay;return Mathf.Sqrt(x*x+y*y);}
    public void SetFlow(Material target,int width,int height)
    {
        if(stage==null||fever?.Model==null){target.SetVector("_FeverFlow",Vector4.zero);return;}
        var m=fever.Model;Vector2 center=board.WorldToViewportPoint(stage.NexusPosition);float shorter=Mathf.Min(width,height),ax=width/shorter,ay=height/shorter;
        float mode=m.Current==FeverState.Phase.Entering?1:m.Current==FeverState.Phase.Fever?2:m.Current==FeverState.Phase.Exiting?3:0;
        var exit=fever.Exit;double u=exit==null?0:exit.Suction(m.Elapsed);
        float spread=mode==1?(float)FeverFlowMath.EntrySpread(m.Progress):mode==2?1:mode==3&&exit!=null?(float)(1-FeverExitMotion.Travel(u,exit.Power)):0;
        centerAspect=new Vector4(center.x,center.y,ax,ay);flow=new Vector4(mode,spread,Extent(center,ax,ay),mode==3?(float)u:(float)m.Progress);
        target.SetVector("_FeverCenterAspect",centerAspect);target.SetVector("_FeverFlow",flow);
        target.SetVector("_RecallSettings",new Vector4((float)(exit?.Power??3.2),(float)(exit?.Twist??.90),(float)(exit?.SpacePull??.65),0));
        Vector2 rim=board.WorldToViewportPoint(stage.NexusPosition+Vector2.right*stage.NexusRadius);
        target.SetFloat("_FeverCoreRadius",Mathf.Abs(rim.x-center.x)*ax);
        for(int i=0;i<6;i++){
            pulses[i]=Vector4.zero;if(i>=count)continue;float age=(fever.Clock-started[i])/.24f;if(age>=1)continue;
            Vector2 uv=board.WorldToViewportPoint(points[i]);pulses[i]=new Vector4(uv.x,uv.y,Mathf.Clamp01(age),1);
        }
        target.SetVectorArray("_ContactPulses",pulses);
        target.SetFloat("_ContactWarp",0); // Replaced, not stacked: the final board pass now owns fever optics.
    }
    private void SetDotted()
    {
        material.SetFloat("_DotVisible",0);var presentation=stage.NexusPresentation;if(presentation?.SourceImages==null)return;
        foreach(var image in presentation.SourceImages)if(image.name=="HpCentral"&&image.sprite!=null&&image.gameObject.activeInHierarchy){
            image.rectTransform.GetWorldCorners(corners);Vector2 a=board.WorldToViewportPoint(presentation.MapVisualPoint(corners[0]));
            Vector2 u=(Vector2)board.WorldToViewportPoint(presentation.MapVisualPoint(corners[3]))-a,v=(Vector2)board.WorldToViewportPoint(presentation.MapVisualPoint(corners[1]))-a;
            if(Mathf.Abs(u.x*v.y-u.y*v.x)<1e-10f)return;
            var sprite=image.sprite;var rect=sprite.textureRect;var tex=sprite.texture;
            material.SetTexture("_DotTex",tex);material.SetVector("_DotRect",new Vector4(rect.x/tex.width,rect.y/tex.height,rect.width/tex.width,rect.height/tex.height));
            material.SetVector("_DotU",new Vector4(a.x,a.y,u.x,u.y));material.SetVector("_DotV",v);material.SetFloat("_DotVisible",image.color.a*presentation.EntryRingAlpha);return;
        }
    }
    private void SetCircuit()
    {
        material.SetFloat("_CircuitVisible",0);var source=GetComponent<LocalImpactDistortion>()?.OriginalBackground?.SourceImage;
        if(stage.Fever.Circuits!=null&&stage.Fever.Circuits.Active)return;
        if(source==null||source.sprite==null)return;
        source.rectTransform.GetWorldCorners(corners);var canvas=source.canvas;var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        Vector2 a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]),b=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
        var r=board.pixelRect;if(b.x<=a.x||b.y<=a.y||r.width<=0||r.height<=0)return;
        material.SetVector("_CircuitImage",new Vector4((a.x-r.x)/r.width,(a.y-r.y)/r.height,(b.x-a.x)/r.width,(b.y-a.y)/r.height));
        var sprite=source.sprite;var uv=sprite.textureRect;var tex=sprite.texture;material.SetTexture("_CircuitTex",tex);
        material.SetVector("_CircuitSprite",new Vector4(uv.x/tex.width,uv.y/tex.height,uv.width/tex.width,uv.height/tex.height));material.SetFloat("_CircuitVisible",1);
    }
    public void Composite(RenderTexture source,RenderTexture destination,Vector4[] hits,Vector4[] shapes,Vector4[] responses,float cap,bool success)
    {
        if(material==null||stage==null||fever?.Model==null){Graphics.Blit(source,destination);return;}
        SetFlow(material,source.width,source.height);bool active=flow.x>.5f&&flow.y>.0001f;RenderTexture mask=null,circuits=null;
        try{
            bool dynamic=fever.Circuits!=null&&fever.Circuits.Active;material.SetFloat("_DynamicCircuitVisible",dynamic?1:0);
            if(active&&dynamic){circuits=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);fever.Circuits.RenderFever(circuits);material.SetTexture("_DynamicCircuits",circuits);}
            else material.SetTexture("_DynamicCircuits",Texture2D.blackTexture);
            if(active){mask=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);elements.Draw(board,mask);material.SetTexture("_ElementsTex",mask);}
            else material.SetTexture("_ElementsTex",Texture2D.blackTexture);
            material.SetFloat("_HasElements",active?1:0);
            Vector2 edge=board.WorldToViewportPoint(stage.NexusPosition+Vector2.right*stage.NexusRadius);
            material.SetFloat("_CoreRadius",Mathf.Max(.001f,Mathf.Abs(edge.x-centerAspect.x)*centerAspect.z));material.SetFloat("_CoreVisible",stage.IsDeath?0:stage.NexusPresentation==null?1:stage.NexusPresentation.FinaleAlpha*stage.NexusPresentation.EntryBodyAlpha);
            material.SetFloat("_VisualBodyScale",(float)FeverFlowMath.BodyScale(fever.Model.Current,fever.Model.Progress,fever.Visuals==null?.115:fever.Visuals.feverGatherCompression)*fever.MagneticBody*(stage.NexusPresentation==null?1:stage.NexusPresentation.FinaleScale*stage.NexusPresentation.EntryBodyScale));
            material.SetFloat("_Level",stage.Energy.DisplayedLevel);material.SetFloat("_Press",stage.Energy.SurfacePress);
            material.SetColor("_CoreTint",Color.Lerp(fever.TutorialInk,CursorDashTuning.ColorValue("nexus.color",new Color(.04f,.7f,.9f)),fever.TutorialPower));
            bool fx=CursorDashTuning.Toggle("fx.enabled",true);material.SetFloat("_Light",fx&&PlayerPrefs.GetString("OnFlash","true")=="true"?CursorDashTuning.EffectGain("user.flash"):0);material.SetFloat("_Warp",fx?CursorDashTuning.EffectGain("user.distortion"):0);
            material.SetFloat("_FlowClock",fever.Clock);material.SetFloat("_Clock",Time.unscaledTime);material.SetVector("_Aspect",new Vector4(centerAspect.z,centerAspect.w,0,0));SetDotted();SetCircuit();
            material.SetVectorArray("_Hits",hits);material.SetVectorArray("_Shapes",shapes);material.SetVectorArray("_Responses",responses);
            material.SetFloat("_SuccessActive",success?1:0);material.SetFloat("_OffsetCap",cap);
            material.SetFloat("_BonusGain",fever.BonusImpactGain);
            material.SetVectorArray("_Pulses",pulses);Graphics.Blit(source,destination,material);RenderedFrames++;
        }finally{if(mask!=null)RenderTexture.ReleaseTemporary(mask);if(circuits!=null)RenderTexture.ReleaseTemporary(circuits);}
    }
    // After actual foreground cursor, before the independent HUD camera. Colors survive the white/black pass.
    public void FinishBoard(RenderTexture source,RenderTexture destination)
    {
        if(!ShouldRenderOptics){Graphics.Blit(source,destination);return;}
        Vector2 coreCenter=board.WorldToViewportPoint(stage.NexusPosition),damageCenter=board.WorldToViewportPoint(damageWorldPoint),damageEdge=board.WorldToViewportPoint(stage.NexusPosition+Vector2.right*stage.NexusRadius);
        if(DamageActive)DamageViewportCenter=damageCenter;
        float damageUser=Mathf.Clamp01(CursorDashTuning.EffectGain("user.distortion"));
        optics.SetVector("_NexusDamage",DamageActive?new Vector4(damageCenter.x,damageCenter.y,Mathf.Clamp01((Time.unscaledTime-damageAt)/damageSeconds),damageUser):Vector4.zero);
        optics.SetVector("_DamageField",damageField);optics.SetVector("_DamageCore",new Vector4(Mathf.Abs(damageEdge.x-coreCenter.x)*source.width/Mathf.Max(1,Mathf.Min(source.width,source.height)),damageChroma,damageSeconds,0));
        optics.SetVector("_DamageShape",damageShape);
        optics.SetColor("_DamageTint",fever.Visuals==null?new Color(1,.04f,.08f):fever.Visuals.nexusDamageTint);
        optics.SetVector("_DamageNexusCenter",new Vector4(coreCenter.x,coreCenter.y,damageHasContact?1:0,0));
        optics.SetFloat("_DamageLightUser",PlayerPrefs.GetString("OnFlash","true")=="true"?Mathf.Clamp01(CursorDashTuning.EffectGain("user.flash")):0);optics.SetTexture("_DamageTextMask",fever.Circuits==null?Texture2D.blackTexture:fever.Circuits.ScoreGlyphMask);
        optics.SetFloat("_ScoreChromaticGain",fever.Model.Current==FeverState.Phase.Fever?Mathf.Clamp(fever.Visuals==null?3:fever.Visuals.feverScoreChromatic,1,4):1);
        if(fever.Model.Current==FeverState.Phase.Exiting&&fever.Exit!=null){
            var exit=fever.Exit;float pressure=(float)exit.Pressure(fever.Model.Elapsed),distort=Mathf.Clamp01(CursorDashTuning.EffectGain("user.distortion"));
            float exitShort=Mathf.Max(1,Mathf.Min(source.width,source.height));Vector2 center=board.WorldToViewportPoint(stage.NexusPosition);
            float worldShort=2*board.orthographicSize*Mathf.Min(1,board.aspect),cap=Mathf.Clamp(CursorDashTuning.Number("fx.distortionCap",.025f),0,.018f);
            float shake=PlayerPrefs.GetString("OnVibe","true")=="true"?Mathf.Min((float)exit.Shake,CursorDashTuning.Number("fx.shakeCap",.2f)/Mathf.Max(.001f,worldShort))*Mathf.Clamp01(CursorDashTuning.EffectGain("user.shake")):0;
            optics.SetVector("_Optics",Vector4.zero);optics.SetVector("_LensAspect",new Vector4(source.width/exitShort,source.height/exitShort,0,0));
            optics.SetVector("_ExitOptics",new Vector4((float)exit.Chromatic*distort,shake,(float)exit.Lens*distort,pressure));
            optics.SetVector("_ExitCenter",new Vector4(center.x,center.y,(float)(fever.Model.Elapsed-exit.Impact),cap));Graphics.Blit(source,destination,optics);OpticsFrames++;return;
        }
        optics.SetVector("_ExitOptics",Vector4.zero);
        var v=fever.Visuals;float seconds=v==null?.30f:Mathf.Clamp(v.feverOpticsSeconds,.1f,.6f);
        for(int i=0;i<6;i++){
            opticalPulses[i]=Vector4.zero;if(i>=count)continue;
            float age=(fever.Clock-started[i])/seconds;if(age>=1)continue;
            Vector2 uv=board.WorldToViewportPoint(points[i]);opticalPulses[i]=new Vector4(uv.x,uv.y,Mathf.Clamp01(age),1);
        }
        float shorter=Mathf.Max(1,Mathf.Min(source.width,source.height));
        optics.SetVector("_LensAspect",new Vector4(source.width/shorter,source.height/shorter,0,0));
        float weight=fever.Model.Current==FeverState.Phase.Fever?(float)FeverFlowMath.OpticsWeight(fever.Model.Elapsed,fever.Model.PhaseSeconds-fever.Model.Elapsed):0;
        float user=Mathf.Clamp(CursorDashTuning.EffectGain("user.distortion"),0,2);
        // This completed-board pass includes the projected score; only the later stable HUD is excluded.
        optics.SetVector("_Optics",new Vector4(Mathf.Clamp(v==null?.0175f:v.feverLensStrength,0,.025f),Mathf.Clamp(v==null?.008f:v.feverChromaticHit,0,.012f),Mathf.Clamp(v==null?.0018f:v.feverChromaticAmbient,0,.003f),weight*user));
        optics.SetFloat("_LensRadius",v==null?.58f:Mathf.Clamp(v.feverLensRadius,.2f,.9f));
        optics.SetVectorArray("_OpticalPulses",opticalPulses);Graphics.Blit(source,destination,optics);OpticsFrames++;
    }
    private void OnDisable()=>Clear();
    private void OnDestroy(){var driver=GetComponent<LocalImpactDistortion>();if(driver!=null&&driver.FeverPresentation==this)driver.FeverPresentation=null;elements?.Dispose();if(material!=null)Destroy(material);if(optics!=null)Destroy(optics);}
}
