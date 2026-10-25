using UnityEngine;

/// <summary>Renders the actual cursor after the nexus board pass and before the stable HUD camera.
/// Only the collider-free visual child gets a temporary, otherwise unused layer. No asset/layer setting changes.</summary>
public sealed class CursorForeground : MonoBehaviour
{
    private Camera board,capture;
    private SpriteRenderer source;
    private Material composite;
    private int layer=-1,previousLayer;
    private bool maskContainedLayer;
    public bool Bound=>source!=null&&capture!=null&&composite!=null&&layer>=0;
    public int RenderedFrames {get;private set;}
    public string Status {get;private set;}="커서 생성 대기";
    public string SourceDescription=>source==null?"없음":source.name+" / "+source.sharedMaterial?.shader.name+" / packed="+(source.sprite!=null&&source.sprite.packed);
    public void Initialize(Camera camera)
    {
        board=camera;
        var shader=Resources.Load<Shader>("CursorDash/CursorForeground");
        if(shader==null||!shader.isSupported){Status="커서 합성 셰이더 사용 불가";return;}
        composite=new Material(shader){hideFlags=HideFlags.DontSave};
    }
    private void OnPreCull()=>Bind();
    private void Bind()
    {
        if(Bound||!isActiveAndEnabled||board==null||composite==null)return;
        var player=PlayerCharacterController.instance;
        if(player==null)return;
        var child=player.transform.Find("CursorVisual");
        if(child==null)return;
        source=child.GetComponent<SpriteRenderer>();
        if(source==null||child.GetComponent<Collider2D>()!=null||child.GetComponent<Collider>()!=null){Status="표시 전용 자식 확인 실패";source=null;return;}
        // Do not reuse any named project layer or any layer occupied by a scene object.
        var objects=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        for(int candidate=31;candidate>=8;candidate--){
            if(!string.IsNullOrEmpty(LayerMask.LayerToName(candidate)))continue;
            bool occupied=false;foreach(var item in objects)if(item.gameObject.layer==candidate){occupied=true;break;}
            if(!occupied){layer=candidate;break;}
        }
        if(layer<0){source=null;Status="비어 있는 표시 전용 레이어 없음";return;}
        previousLayer=child.gameObject.layer;maskContainedLayer=(board.cullingMask&(1<<layer))!=0;
        child.gameObject.layer=layer;board.cullingMask&=~(1<<layer);
        var go=new GameObject("CursorDash actual cursor capture"){hideFlags=HideFlags.DontSave};
        capture=go.AddComponent<Camera>();capture.enabled=false;capture.tag="Untagged";
        Status="실제 SpriteRenderer → 별도 투명 RT → 넥서스 다음 → HUD 이전";
    }
    public void Composite(RenderTexture input,RenderTexture output,Vector4[] hits,Vector4[] shapes,Vector4[] responses,float cap,bool success)
    {
        Bind();
        if(!Bound){Graphics.Blit(input,output);return;}
        var texture=RenderTexture.GetTemporary(input.width,input.height,24,RenderTextureFormat.ARGB32);
        try{
            capture.CopyFrom(board);capture.enabled=false;capture.transform.SetPositionAndRotation(board.transform.position,board.transform.rotation);
            capture.clearFlags=CameraClearFlags.SolidColor;capture.backgroundColor=Color.clear;
            capture.cullingMask=1<<layer;capture.targetTexture=texture;capture.rect=new Rect(0,0,1,1);
            // Same projection (including camera shake); never move the actual cursor or its collider.
            capture.projectionMatrix=board.projectionMatrix;capture.Render();capture.targetTexture=null;
            composite.SetTexture("_CursorTex",texture);
            composite.SetVectorArray("_Hits",hits);composite.SetVectorArray("_Shapes",shapes);composite.SetVectorArray("_Responses",responses);
            float shorter=Mathf.Min(input.width,input.height);
            composite.SetVector("_Aspect",new Vector4(input.width/shorter,input.height/shorter,0,0));
            composite.SetFloat("_Clock",Time.unscaledTime);composite.SetFloat("_OffsetCap",cap);composite.SetFloat("_SuccessActive",success?1:0);
            var fever=board.GetComponent<LocalImpactDistortion>()?.FeverPresentation;
            if(fever!=null)fever.SetFlow(composite,input.width,input.height);else composite.SetVector("_FeverFlow",Vector4.zero);
            Graphics.Blit(input,output,composite);RenderedFrames++;
        }finally{if(capture!=null)capture.targetTexture=null;RenderTexture.ReleaseTemporary(texture);}
    }
    private void Restore()
    {
        if(source!=null&&layer>=0&&source.gameObject.layer==layer)source.gameObject.layer=previousLayer;
        if(board!=null&&layer>=0&&maskContainedLayer)board.cullingMask|=1<<layer;
        source=null;layer=-1;if(capture!=null)Destroy(capture.gameObject);capture=null;
    }
    private void OnDisable()=>Restore();
    private void OnDestroy(){Restore();if(composite!=null)Destroy(composite);}
}
