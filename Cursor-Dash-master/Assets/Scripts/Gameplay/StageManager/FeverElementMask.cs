using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Alpha silhouettes from actual renderers/materials. No physics layers or authored assets are changed.</summary>
public sealed class FeverElementMask
{
    private readonly CommandBuffer buffer=new CommandBuffer{name="CursorDash fever element silhouettes"};
    private Renderer[] pooled;private PlayerCharacterController player;private readonly StageManager stage;
    private readonly Material trail;
    public FeverElementMask(StageManager stage){this.stage=stage;var shader=Resources.Load<Shader>("CursorDash/FeverTrailMask");if(shader!=null&&shader.isSupported)trail=new Material(shader){hideFlags=HideFlags.DontSave};}
    public void Draw(Camera camera,RenderTexture target)
    {
        if(pooled==null)pooled=stage.GetComponentsInChildren<Renderer>(true);
        buffer.Clear();buffer.SetRenderTarget(target);buffer.ClearRenderTarget(false,true,Color.clear);
        // This API takes Unity's logical camera projection and performs platform conversion itself.
        // Pre-converting for a render texture flipped the mask on D3D: head stayed correct while
        // bonuses, native trail and fragments appeared vertically mirrored away from their physics.
        buffer.SetViewProjectionMatrices(camera.worldToCameraMatrix,camera.projectionMatrix);
        void DrawOne(Renderer renderer){
            if(renderer==null||!renderer.enabled||renderer.forceRenderingOff||!renderer.gameObject.activeInHierarchy)return;
            if(renderer is SpriteRenderer sprite&&(sprite.sprite==null||sprite.name=="CursorVisual"))return;
            // Built-in additive trail materials can write zero alpha. Use the same native ribbon/vertex fade,
            // with alpha-preserving mask output; never replace its normal material or defense geometry.
            if(renderer is TrailRenderer&&trail!=null){buffer.DrawRenderer(renderer,trail);return;}
            var materials=renderer.sharedMaterials;
            for(int i=0;i<materials.Length;i++)if(materials[i]!=null)buffer.DrawRenderer(renderer,materials[i],i);
        }
        foreach(var renderer in pooled)DrawOne(renderer);
        var score=stage.Fever?.BackgroundScoreRenderer;if(score!=null&&System.Array.IndexOf(pooled,score)<0)DrawOne(score);
        if(player==null)player=PlayerCharacterController.instance;
        if(player!=null)foreach(var renderer in player.GetComponentsInChildren<Renderer>())DrawOne(renderer);
        // The field already owns these DontSave bursts. Scene searches exclude DontSave objects,
        // which previously let the white pass erase their black fragments despite successful hits.
        var bursts=stage.Fever?.BonusBursts;
        if(bursts!=null)foreach(var burst in bursts)if(burst!=null)DrawOne(burst.View);
        var recalls=stage.Fever?.RecallViews;
        if(recalls!=null)foreach(var renderer in recalls)if(System.Array.IndexOf(pooled,renderer)<0)DrawOne(renderer);
        Graphics.ExecuteCommandBuffer(buffer);
    }
    public void Dispose(){buffer.Release();if(trail!=null)Object.Destroy(trail);}
}
