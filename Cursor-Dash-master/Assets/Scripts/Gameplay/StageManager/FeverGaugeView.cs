using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Five stationary charged-only arcs. Multiplier lives exclusively in the stable score HUD.
public sealed class FeverGaugeView : MonoBehaviour
{
    private StageManager stage;private CursorDashFever fever;private Image original;private Color originalTint;
    private GameObject root;private Mesh mesh;private Material material;private Camera board;
    private readonly Transform[] segments=new Transform[5];
    private bool frameReplaced;private float displayCharge,velocity,closingStart,previousGauge,chargePulse;
    private uint seenReset;private FeverState.Phase lastPhase=FeverState.Phase.Stopped;
    public float GapDegrees=>fever==null?4:Mathf.Clamp((float)FeverSettings.Safe(fever.Settings.gaugeGapDegrees,4,1),1,8);
    public float HeadTurn {get;private set;}
    public float Radius {get;private set;}
    public Vector2 Center=>root==null?Vector2.zero:(Vector2)root.transform.position;
    public bool Bound=>root!=null;
    public MeshRenderer[] SegmentRenderers {get;private set;}
    public void Initialize(StageManager owner,CursorDashFever controller,Image outer,TMP_Text fontSource)
    {
        stage=owner;fever=controller;original=outer;board=Camera.main;if(outer!=null)originalTint=outer.color;
        var shader=Resources.Load<Shader>("CursorDash/FeverGauge");
        if(outer==null||board==null||shader==null||!shader.isSupported){Debug.LogWarning("CursorDash: five-arc gauge unavailable; original frame retained.");return;}
        root=new GameObject("Nexus five arc gauge"){hideFlags=HideFlags.DontSave};root.transform.SetParent(transform,false);
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.vertices=new[]{new Vector3(-1.3f,-1.3f),new Vector3(-1.3f,1.3f),new Vector3(1.3f,1.3f),new Vector3(1.3f,-1.3f)};mesh.triangles=new[]{0,1,2,0,2,3};
        material=new Material(shader){hideFlags=HideFlags.DontSave};material.SetColor("_Color",originalTint);SegmentRenderers=new MeshRenderer[5];
        for(int i=0;i<5;i++){
            var piece=new GameObject("Gauge arc "+(i+1)){hideFlags=HideFlags.DontSave};piece.transform.SetParent(root.transform,false);segments[i]=piece.transform;
            piece.AddComponent<MeshFilter>().sharedMesh=mesh;var view=piece.AddComponent<MeshRenderer>();view.sharedMaterial=material;view.sortingOrder=34;
            var properties=new MaterialPropertyBlock();properties.SetFloat("_Segment",i);view.SetPropertyBlock(properties);SegmentRenderers[i]=view;
        }
        ReplaceFrame();
    }
    private void LateUpdate()
    {
        if(root==null||fever.Model==null)return;var m=fever.Model;bool visible=!stage.IsDeath&&m.Current!=FeverState.Phase.Stopped;
        root.SetActive(visible);if(!visible)return;
        Radius=stage.NexusRadius*1.86f;root.transform.SetPositionAndRotation(stage.NexusPosition,Quaternion.identity);root.transform.localScale=Vector3.one*Radius;
        if(seenReset!=m.ResetVersion){seenReset=m.ResetVersion;displayCharge=(float)m.Gauge;velocity=0;HeadTurn=0;}
        if(lastPhase!=m.Current){if(m.Current==FeverState.Phase.Closing)closingStart=HeadTurn;if(m.Current==FeverState.Phase.Normal){displayCharge=(float)m.Gauge;velocity=0;}lastPhase=m.Current;}
        if(m.Current==FeverState.Phase.Normal&&Time.deltaTime>0)displayCharge=Mathf.SmoothDamp(displayCharge,(float)m.Gauge,ref velocity,(float)FeverSettings.Safe(fever.Settings.gaugeSmoothSeconds,.06,.015),Mathf.Infinity,Time.deltaTime);
        if(m.Current==FeverState.Phase.Normal&&m.Gauge>previousGauge+.0001)chargePulse=1;
        previousGauge=(float)m.Gauge;chargePulse=Mathf.Max(0,chargePulse-Time.deltaTime/ .14f);
        float head=displayCharge,tail=0,arrow=displayCharge>.001f?1:0;
        if(m.Current==FeverState.Phase.Closing){head=Mathf.Lerp(closingStart,1,Mathf.SmoothStep(0,1,(float)m.Progress));arrow=1;}
        if(m.Current==FeverState.Phase.Entering){head=1;arrow=0;}
        if(m.Current==FeverState.Phase.Fever){head=1;tail=1-(float)m.Gauge;arrow=m.Gauge>0?1:0;}
        if(m.Current==FeverState.Phase.Exiting){head=tail=arrow=0;}
        HeadTurn=m.Current==FeverState.Phase.Fever?tail:head;
        material.SetFloat("_Head",Mathf.Clamp01(head));material.SetFloat("_Tail",tail);material.SetFloat("_Drain",m.Current==FeverState.Phase.Fever?1:0);material.SetFloat("_Arrow",arrow);
        material.SetFloat("_Start",7f/12);material.SetFloat("_Thickness",Mathf.Clamp((float)FeverSettings.Safe(fever.Settings.gaugeThickness,.14,.08),.08f,.21f));
        material.SetFloat("_GapDegrees",GapDegrees);
        material.SetFloat("_ChargePulse",m.Current==FeverState.Phase.Normal?chargePulse:0);
        material.SetVector("_NexusWorld",new Vector4(stage.NexusPosition.x,stage.NexusPosition.y,stage.NexusRadius,0));
        for(int i=0;i<5;i++){
            float scale=1,spin=0;Vector3 offset=Vector3.zero;float p=(float)m.Progress;
            if(m.Current==FeverState.Phase.Entering){float t=Mathf.Clamp01((p-i*.018f)/.50f);scale=(float)FeverFlowMath.OuterScale(i,false,p);spin=-(450+i*11)*(1-Mathf.Pow(1-t,3));offset=Quaternion.Euler(0,0,spin-i*72)*Vector3.up*(.11f*Mathf.Sin(t*Mathf.PI)*scale);}
            else if(m.Current==FeverState.Phase.Fever){float t=Mathf.Clamp01((float)m.Elapsed/.20f);scale=Mathf.SmoothStep(0,1,t);spin=120*(1-scale);}
            else if(m.Current==FeverState.Phase.Exiting){scale=(float)FeverFlowMath.OuterScale(i,true,p);spin=180*(1-scale);}
            segments[i].localPosition=offset;segments[i].localScale=Vector3.one*scale;segments[i].localRotation=Quaternion.Euler(0,0,spin);
        }
    }
    private void ReplaceFrame(){if(original==null||frameReplaced)return;var tint=originalTint;tint.a=0;original.color=tint;frameReplaced=true;}
    private void RestoreFrame(){if(original!=null&&frameReplaced)original.color=originalTint;frameReplaced=false;}
    private void OnEnable(){if(root!=null)ReplaceFrame();}
    private void OnDisable(){if(root!=null)root.SetActive(false);RestoreFrame();chargePulse=velocity=0;}
    private void OnDestroy(){RestoreFrame();if(root!=null)Destroy(root);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
