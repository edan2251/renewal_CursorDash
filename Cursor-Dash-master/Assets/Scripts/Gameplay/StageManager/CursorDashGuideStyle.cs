using UnityEngine;

/// <summary>Owned per pooled actor; restores original materials/blocks. Never edits a shared asset.</summary>
[DefaultExecutionOrder(300)]
public sealed class CursorDashGuideStyle : MonoBehaviour
{
    private Renderer[] views;private Material[] originals;private MaterialPropertyBlock[] saved;
    private Color[] spriteColors;private Material ink;private MaterialPropertyBlock block;
    private CursorDashGhostDemonstration owner;
    public void Bind(CursorDashGhostDemonstration demonstration)
    {
        owner=demonstration;
        if(block==null)block=new MaterialPropertyBlock();
        if(views==null){
            views=GetComponentsInChildren<Renderer>(true);originals=new Material[views.Length];saved=new MaterialPropertyBlock[views.Length];spriteColors=new Color[views.Length];
            var shader=Resources.Load<Shader>("CursorDash/GuideInk");
            if(shader!=null&&shader.isSupported)ink=new Material(shader){hideFlags=HideFlags.DontSave};
            for(int i=0;i<views.Length;i++){
                originals[i]=views[i].sharedMaterial;saved[i]=new MaterialPropertyBlock();views[i].GetPropertyBlock(saved[i]);
                if(views[i] is SpriteRenderer sr)spriteColors[i]=sr.color;
            }
        }
        enabled=true;Apply();
    }
    private void LateUpdate(){if(owner==null){Restore();return;}Apply();}
    private void Apply()
    {
        if(views==null||owner==null)return;
        var tint=owner.InkColor;
        for(int i=0;i<views.Length;i++){
            var view=views[i];if(view==null)continue;
            var shader=originals[i]==null?null:originals[i].shader;
            // The existing geometric shaders retain topology, AA, cutouts, charge and rotating artwork.
            bool geometric=shader!=null&&(shader.name=="CursorDash/CircularArtwork"||shader.name=="CursorDash/NodeFlat"||shader.name=="CursorDash/ImpactFlat");
            if(!geometric&&ink!=null)view.sharedMaterial=ink;
            view.GetPropertyBlock(block);block.SetColor("_Color",tint);block.SetFloat("_GuideSketch",owner.SketchAmount);view.SetPropertyBlock(block);
            if(view is SpriteRenderer sr)sr.color=new Color(tint.r,tint.g,tint.b,sr.color.a);
        }
    }
    public void Restore()
    {
        owner=null;
        if(views!=null)for(int i=0;i<views.Length;i++)if(views[i]!=null){views[i].sharedMaterial=originals[i];views[i].SetPropertyBlock(saved[i]);if(views[i] is SpriteRenderer sr)sr.color=spriteColors[i];}
        enabled=false;
    }
    private void OnDisable(){if(owner!=null)Restore();}
    private void OnDestroy(){if(ink!=null)Destroy(ink);}
}
