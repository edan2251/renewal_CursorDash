using UnityEngine;

/// <summary>Slow board-space color regions only. Original circuit overlay and hit response remain separate.</summary>
public sealed class CursorDashLivingBackground : MonoBehaviour
{
    private Camera board;private StageManager stage;private GameObject quad;private Mesh mesh;private Material material;
    private SpriteRenderer original;private bool originalEnabled;private float clock,hitEnergy;private Vector2 drift;private CursorDashVisualProfile profile;
    private MeshRenderer authoredRenderer;private Material authoredMaterial;private bool ownsArtwork;
    public float Clock=>clock;
    public void Initialize(StageManager game)
    {
        stage=game;profile=game.BoardView!=null?game.BoardView.Profile:Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");board=Camera.main;var shader=Resources.Load<Shader>("CursorDash/LivingBackground");
        if(board==null||shader==null||!shader.isSupported)return;
        if(game.BoardView!=null&&game.BoardView.IsConfigured){
            authoredRenderer=game.BoardView.ColorField;authoredMaterial=authoredRenderer.sharedMaterial;
            quad=authoredRenderer.gameObject;material=new Material(authoredMaterial){hideFlags=HideFlags.DontSave};authoredRenderer.sharedMaterial=material;
        }else{
            // Compatibility for retained scenes. Cyberpunk has an explicit, saved renderer/material.
            var candidate=GameObject.Find("Cyberpunk_Background");if(candidate!=null)original=candidate.GetComponent<SpriteRenderer>();
            if(original==null)return;originalEnabled=original.enabled;original.enabled=false;ownsArtwork=true;
            quad=new GameObject("Living color field"){hideFlags=HideFlags.DontSave};
            mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.vertices=new[]{new Vector3(-1,-1),new Vector3(-1,1),new Vector3(1,1),new Vector3(1,-1)};mesh.triangles=new[]{0,1,2,0,2,3};
            material=new Material(shader){hideFlags=HideFlags.DontSave};quad.AddComponent<MeshFilter>().sharedMesh=mesh;
            var view=quad.AddComponent<MeshRenderer>();view.sharedMaterial=material;view.sortingOrder=-1000;
        }
        ApplyFrame(material,profile,0,0,Vector2.zero);PlaceField(quad.transform,board);
        if(stage.Feedback!=null)stage.Feedback.Signaled+=OnHit;
    }
    private void LateUpdate()
    {
        if(quad==null)return;
        float delta=stage.IsPlaying?Time.deltaTime:stage.AtmosphereActive?Time.unscaledDeltaTime:0;
        if(stage.NormalThreatsSuppressed)hitEnergy=0;
        float recovery=profile==null?.38f:Mathf.Clamp(profile.backgroundHitRecovery,.08f,.6f);
        if(delta>0){clock+=delta*(1+hitEnergy*(profile==null?2.6f:Mathf.Clamp(profile.backgroundHitSpeed,0,3)));hitEnergy*=Mathf.Exp(-delta/recovery);drift*=Mathf.Exp(-delta/recovery);}
        ApplyFrame(material,profile,clock,hitEnergy,drift);
        PlaceField(quad.transform,board);
    }
    private void OnHit(HitFeedback.EventInfo hit)
    {
        if(!stage.IsPlaying||stage.NormalThreatsSuppressed||!CursorDashTuning.Toggle("fx.enabled",true))return;
        if(hit.kind!="circleRemove"&&hit.kind!="triangleBreak"&&hit.kind!="nexusDamage")return;
        hitEnergy=Mathf.Max(hitEnergy,hit.kind=="nexusDamage"?1:.75f);
        Vector2 direction=hit.incoming.sqrMagnitude>.001f?hit.incoming.normalized:((Vector2)hit.point-stage.NexusPosition).normalized;
        drift=Vector2.ClampMagnitude(drift+direction*(profile==null?.075f:Mathf.Clamp(profile.backgroundHitShift,0,.12f)),.14f); // Bounded push, continuous color phase.
    }
    public static void ApplyPalette(Material target,CursorDashVisualProfile source)
    {
        if(target==null)return;
        target.SetVector("_Navy",source==null?new Color(.010f,.021f,.070f):source.backgroundNavy);
        target.SetVector("_Teal",source==null?new Color(.012f,.210f,.260f):source.backgroundTeal);
        target.SetVector("_Violet",source==null?new Color(.200f,.035f,.280f):source.backgroundViolet);
    }
    // Shared rendering inputs only. Editor callers own a sample clock; no game state runs here.
    public static void ApplyFrame(Material target,CursorDashVisualProfile source,float seconds,float hit,Vector2 shift)
    {
        ApplyPalette(target,source);
        target.SetVector("_HitDrift",new Vector4(shift.x,shift.y,0,0));
        target.SetFloat("_HitColor",hit*(source==null?.65f:Mathf.Clamp01(source.backgroundHitColor)));
        target.SetFloat("_Clock",seconds);
        target.SetFloat("_FlowSpeed",source==null?.22f:source.backgroundSpeed);
        target.SetFloat("_Mix",source==null?.84f:source.backgroundMix);
        target.SetFloat("_Bend",source==null?.30f:source.backgroundBend);
    }
    public static void PlaceField(Transform field,Camera camera)
    {
        float h=camera.orthographicSize*1.06f;
        field.position=new Vector3(camera.transform.position.x,camera.transform.position.y,0);
        field.localScale=new Vector3(h*camera.aspect,h,1);
    }
    private void OnDisable(){hitEnergy=0;drift=Vector2.zero;if(original!=null)original.enabled=originalEnabled;if(quad!=null)quad.SetActive(false);}
    private void OnEnable(){if(quad!=null){quad.SetActive(true);if(original!=null)original.enabled=false;}}
    private void OnDestroy(){if(stage!=null&&stage.Feedback!=null)stage.Feedback.Signaled-=OnHit;if(original!=null)original.enabled=originalEnabled;if(authoredRenderer!=null)authoredRenderer.sharedMaterial=authoredMaterial;if(ownsArtwork&&quad!=null)Destroy(quad);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
