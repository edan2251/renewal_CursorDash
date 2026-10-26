using UnityEngine;

/// <summary>Slow board-space color regions only. Original circuit overlay and hit response remain separate.</summary>
public sealed class CursorDashLivingBackground : MonoBehaviour
{
    private Camera board;private StageManager stage;private GameObject quad;private Mesh mesh;private Material material;
    private SpriteRenderer original;private bool originalEnabled;private float clock,hitEnergy;private Vector2 drift;private CursorDashVisualProfile profile;
    public float Clock=>clock;
    public void Initialize(StageManager game)
    {
        stage=game;profile=Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");board=Camera.main;var shader=Resources.Load<Shader>("CursorDash/LivingBackground");
        if(board==null||shader==null||!shader.isSupported)return;
        var candidate=GameObject.Find("Cyberpunk_Background");if(candidate!=null)original=candidate.GetComponent<SpriteRenderer>();
        if(original==null)return;originalEnabled=original.enabled;
        quad=new GameObject("Living color field"){hideFlags=HideFlags.DontSave};
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.vertices=new[]{new Vector3(-1,-1),new Vector3(-1,1),new Vector3(1,1),new Vector3(1,-1)};mesh.triangles=new[]{0,1,2,0,2,3};
        material=new Material(shader){hideFlags=HideFlags.DontSave};quad.AddComponent<MeshFilter>().sharedMesh=mesh;
        var view=quad.AddComponent<MeshRenderer>();view.sharedMaterial=material;view.sortingOrder=-1000;original.enabled=false;
        if(stage.Feedback!=null)stage.Feedback.Signaled+=OnHit;
    }
    private void LateUpdate()
    {
        if(quad==null)return;
        float delta=stage.IsPlaying?Time.deltaTime:stage.AtmosphereActive?Time.unscaledDeltaTime:0;
        if(stage.NormalThreatsSuppressed)hitEnergy=0;
        float recovery=profile==null?.38f:Mathf.Clamp(profile.backgroundHitRecovery,.08f,.6f);
        if(delta>0){clock+=delta*(1+hitEnergy*(profile==null?2.6f:Mathf.Clamp(profile.backgroundHitSpeed,0,3)));hitEnergy*=Mathf.Exp(-delta/recovery);drift*=Mathf.Exp(-delta/recovery);}
        material.SetVector("_HitDrift",new Vector4(drift.x,drift.y,0,0)); // Continues through fever; pause holds it, exit never resets it.
        material.SetFloat("_HitColor",hitEnergy*(profile==null?.65f:Mathf.Clamp01(profile.backgroundHitColor)));
        float h=board.orthographicSize*1.06f;quad.transform.position=new Vector3(board.transform.position.x,board.transform.position.y,0);
        quad.transform.localScale=new Vector3(h*board.aspect,h,1);material.SetFloat("_Clock",clock);
        material.SetFloat("_FlowSpeed",profile==null ? .22f : profile.backgroundSpeed);material.SetFloat("_Mix",profile==null ? .84f : profile.backgroundMix);material.SetFloat("_Bend",profile==null ? .30f : profile.backgroundBend);
    }
    private void OnHit(HitFeedback.EventInfo hit)
    {
        if(!stage.IsPlaying||stage.NormalThreatsSuppressed||!CursorDashTuning.Toggle("fx.enabled",true))return;
        if(hit.kind!="circleRemove"&&hit.kind!="triangleBreak"&&hit.kind!="nexusDamage")return;
        hitEnergy=Mathf.Max(hitEnergy,hit.kind=="nexusDamage"?1:.75f);
        Vector2 direction=hit.incoming.sqrMagnitude>.001f?hit.incoming.normalized:((Vector2)hit.point-stage.NexusPosition).normalized;
        drift=Vector2.ClampMagnitude(drift+direction*(profile==null?.075f:Mathf.Clamp(profile.backgroundHitShift,0,.12f)),.14f); // Bounded push, continuous color phase.
    }
    private void OnDisable(){hitEnergy=0;drift=Vector2.zero;if(original!=null)original.enabled=originalEnabled;if(quad!=null)quad.SetActive(false);}
    private void OnEnable(){if(quad!=null){quad.SetActive(true);if(original!=null)original.enabled=false;}}
    private void OnDestroy(){if(stage!=null&&stage.Feedback!=null)stage.Feedback.Signaled-=OnHit;if(original!=null)original.enabled=originalEnabled;if(quad!=null)Destroy(quad);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
