using UnityEngine;

/// <summary>Non-attacking fever reward. Only the real player body can remove it; expiry/cleanup never reward.</summary>
public sealed class FeverBonusNode : Node
{
    private FeverBonusField owner;private MeshRenderer view;private MaterialPropertyBlock properties;
    private Vector2 origin;private float age,life,radius,seed;private bool removed;
    public float WorldRadius=>Mathf.Abs(transform.lossyScale.x);
    public bool Collectible=>!removed&&Phase!=NodePhase.Inactive&&gameObject.activeInHierarchy&&circleCollider!=null&&circleCollider.enabled;
    private new void Awake(){base.Awake();}
    public void Bind(FeverBonusField field,Mesh mesh,Material material)
    {
        owner=field;
        // Node keeps its SpriteRenderer/collider on the root. Unity forbids a MeshFilter on
        // that same SpriteRenderer object, so only the procedural visual lives on a child.
        var visual=new GameObject("Fever bonus visual"){hideFlags=HideFlags.DontSave,layer=gameObject.layer};
        visual.transform.SetParent(transform,false);
        visual.AddComponent<MeshFilter>().sharedMesh=mesh;view=visual.AddComponent<MeshRenderer>();
        view.sharedMaterial=material;view.sortingOrder=22;properties=new MaterialPropertyBlock();
    }
    public void Spawn(Vector2 position,float size,float duration,float variation)
    {
        origin=position;radius=size;life=duration;seed=variation;age=0;removed=false;
        transform.position=position;BeginExecution();
        spriteRenderer.color=Color.clear;circleCollider.radius=1;circleCollider.enabled=true;
        properties.SetFloat("_Invert",variation>.5f?1:0);ApplyAppearance();
    }
    public void Advance(float delta)
    {
        if(removed||Phase==NodePhase.Inactive)return;age+=delta;
        if(age>=life){ResetObject();return;}
        ApplyAppearance();
    }
    private void ApplyAppearance()
    {
        var profile=owner?.Visuals;
        float remaining=Mathf.Min(life-age,owner==null?float.PositiveInfinity:owner.RemainingSeconds);
        float appear=profile==null?.12f:profile.feverNodeAppearSeconds,retire=profile==null?.55f:profile.feverNodeRetireSeconds;
        transform.localScale=Vector3.one*(radius*(float)NodeRhythmMath.BonusScale(age,appear,remaining,retire));
        transform.position=origin; // Renderer and real CircleCollider2D shrink together until the existing expiry.
        properties.SetFloat("_Opacity",Mathf.Clamp01(remaining/.14f));view.SetPropertyBlock(properties);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player=PlayerCharacterController.instance;
        if(player==null||other.gameObject!=player.gameObject||!Collectible)return;
        if(!owner.CanCollect)return;
        Vector2 contact=circleCollider.Distance(other).pointA;
        TryCollect(contact,player.ContactMotionDirection,false);
    }
    public bool TryCollect(Vector2 contact,Vector2 incoming,bool swept)
    {
        if(!Collectible||owner==null||!owner.CanCollect)return false;
        // Claim before any reward/feedback callback. Trigger + sweep + Stay share this single gate.
        removed=true;circleCollider.enabled=false;
        // A VFX failure must still release this claimed node, rather than leaving a visible,
        // disabled collider in the pool. Keep the exception visible; never retry the reward.
        try{return owner.Collect(this,contact,incoming,swept);}
        finally{ResetObject();}
    }
    // A bonus can appear under the body while an internal review pauses the round.
    // Resume must collect that existing overlap; removed/execution guards still allow only one reward.
    private void OnTriggerStay2D(Collider2D other)=>OnTriggerEnter2D(other);
    public override void ResetObject(){removed=true;base.ResetObject();}
    private void OnDisable(){removed=true;Phase=NodePhase.Inactive;}
}
