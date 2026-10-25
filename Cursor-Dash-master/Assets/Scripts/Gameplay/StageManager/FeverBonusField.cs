using System.Collections.Generic;
using UnityEngine;

/// <summary>Fever-only timed individual rewards, reusing Node execution/reset tokens; no attack or nexus-damage path.</summary>
public sealed class FeverBonusField
{
    private readonly List<FeverBonusNode> nodes=new List<FeverBonusNode>();
    private readonly List<FeverBonusBurst> bursts=new List<FeverBonusBurst>();
    public IReadOnlyList<FeverBonusBurst> Bursts=>bursts;
    public CursorDashVisualProfile Visuals=>fever.Visuals;
    public float RemainingSeconds=>fever.Model.Current==FeverState.Phase.Fever?Mathf.Max(0,(float)(fever.Model.PhaseSeconds-fever.Model.Elapsed)):0;
    private readonly CursorDashFever fever;private readonly StageManager stage;
    private Mesh quad;private Material material;private GameObject root;private int spawnSerial;private readonly FeverSupplyClock supply=new FeverSupplyClock();
    private bool sampledHead;private Vector2 previousHead;
    public int Spawned {get;private set;}
    public bool Diagnostics {get;set;}
    private Vector2 lastContact,lastOrigin;private float diagnosticUntil;
    private readonly List<(FeverBonusNode node,float along,Vector2 point)> contacts=new List<(FeverBonusNode,float,Vector2)>();
    public int Collected {get;private set;}
    public int SweptCollections {get;private set;}
    public bool CanCollect=>stage.IsPlaying&&fever.Model.Current==FeverState.Phase.Fever;
    public int ActiveCount {get{int n=0;foreach(var item in nodes)if(item!=null&&item.gameObject.activeSelf)n++;return n;}}
    public FeverBonusField(CursorDashFever controller,StageManager game)
    {
        fever=controller;stage=game;var shader=Resources.Load<Shader>("CursorDash/FeverBonus");
        if(shader==null||!shader.isSupported){Debug.LogWarning("CursorDash fever C: bonus shader unavailable.");return;}
        // Real physics roots must remain in their PhysicsScene2D. Only owned visual resources use DontSave.
        // This pool is created in Play and explicitly destroyed by its stage owner; no scene save is performed.
        root=new GameObject("Fever reward pool");root.transform.SetParent(stage.transform,false);
        quad=new Mesh{hideFlags=HideFlags.DontSave};quad.vertices=new[]{new Vector3(-1,-1),new Vector3(-1,1),new Vector3(1,1),new Vector3(1,-1)};quad.triangles=new[]{0,1,2,0,2,3};
        material=new Material(shader){hideFlags=HideFlags.DontSave};
        int capacity=Mathf.Clamp(fever.Settings.bonusCapacity,8,96);
        for(int i=0;i<capacity;i++){
            var go=new GameObject("Fever bonus "+i);go.SetActive(false);go.transform.SetParent(root.transform,false);go.tag="Node";
            go.AddComponent<SpriteRenderer>();var collider=go.AddComponent<CircleCollider2D>();collider.isTrigger=true;
            var node=go.AddComponent<FeverBonusNode>();node.Bind(this,quad,material);nodes.Add(node);
        }
    }
    public void Begin(){ClearNodes();spawnSerial=0;Spawned=Collected=SweptCollections=0;supply.Reset();}
    public void Tick(float delta)
    {
        if(delta<=0)return;
        for(int i=bursts.Count-1;i>=0;i--){if(bursts[i]==null)bursts.RemoveAt(i);else bursts[i].Advance(delta);}
        if(!CanCollect)return;
        foreach(var item in nodes)if(item.gameObject.activeSelf)item.Advance(delta);
        var profile=fever.Visuals;
        double interval=profile==null?.12:profile.feverSpawnInterval;
        int limit=Mathf.Clamp(profile==null?14:profile.feverConcurrentLimit,1,nodes.Count);
        // At most one per opportunity. Full capacity or no safe position only waits; no live reward is evicted.
        if(supply.Advance(delta,interval,ActiveCount,limit))SpawnOne();
    }
    private void SpawnOne()
    {
        if(root==null||Camera.main==null)return;
        var camera=Camera.main;var profile=fever.Visuals;
        float margin=profile==null?.12f:Mathf.Clamp(profile.feverSideMargin,.05f,.25f);
        float top=profile==null?.18f:Mathf.Clamp(profile.feverTopMargin,.1f,.3f),bottom=profile==null?.16f:Mathf.Clamp(profile.feverBottomMargin,.1f,.3f);
        Vector2 low=camera.ViewportToWorldPoint(new Vector3(margin,bottom)),high=camera.ViewportToWorldPoint(new Vector3(1-margin,1-top));
        float size=(float)FeverSettings.Safe(fever.Settings.bonusRadius,.205,.06),life=(float)FeverSettings.Safe(fever.Settings.bonusLife,2.8,.4);
        float gap=profile==null?.22f:Mathf.Max(.05f,profile.feverNodeGap),clearance=profile==null?.28f:Mathf.Max(.1f,profile.feverCursorClearance);
        low+=Vector2.one*size;high-=Vector2.one*size;if(high.x<=low.x||high.y<=low.y)return;
        var node=nodes.Find(n=>!n.gameObject.activeSelf);if(node==null)return;
        var player=PlayerCharacterController.instance;Vector2 head=player==null?Vector2.zero:(Vector2)player.transform.position;
        float headRadius=0;var box=player==null?null:player.GetComponent<BoxCollider2D>();
        if(box!=null){head=player.transform.TransformPoint(box.offset);headRadius=box.bounds.extents.magnitude;}
        int serial=++spawnSerial;
        for(int attempt=0;attempt<96;attempt++){
            // Independent samples, not a line/circle/group template. Reject every unsafe candidate.
            Vector2 point=new Vector2(Mathf.Lerp(low.x,high.x,(float)FeverScatter.Unit(serial,attempt,0)),Mathf.Lerp(low.y,high.y,(float)FeverScatter.Unit(serial,attempt,1)));
            if(Vector2.Distance(point,stage.NexusPosition)<stage.NexusRadius+size+.35f)continue;
            if(player!=null&&Vector2.Distance(point,head)<headRadius+size+clearance)continue;
            if(fever.OverlapsHud(point,size,camera))continue;
            bool free=true;foreach(var other in nodes)if(other.gameObject.activeSelf&&Vector2.Distance(point,other.transform.position)<size+other.WorldRadius+gap){free=false;break;}
            if(!free)continue;
            node.Spawn(point,size,life,0);Spawned++;return;
        }
    }
    // Sample the actual rendered head after movement, not a queued future Rigidbody position or an old tail.
    public void SweepHead()
    {
        var player=PlayerCharacterController.instance;
        if(!CanCollect||player==null){sampledHead=false;return;}
        Vector2 current=player.transform.position;
        Sweep(player,sampledHead?previousHead:current,current);previousHead=current;sampledHead=true;
    }
    public void Sweep(PlayerCharacterController player,Vector2 start,Vector2 end)
    {
        if(!CanCollect||player==null)return;var box=player.GetComponent<BoxCollider2D>();if(box==null||!box.enabled)return;
        contacts.Clear();Quaternion inverse=Quaternion.Inverse(player.transform.rotation);
        Vector2 offset=player.transform.TransformVector(box.offset),scale=player.transform.lossyScale;
        Vector2 half=new Vector2(Mathf.Abs(box.size.x*scale.x),Mathf.Abs(box.size.y*scale.y))*.5f;
        foreach(var node in nodes){
            if(node==null||!node.Collectible)continue;
            Vector2 center=node.transform.position,a=inverse*(Vector3)(center-start-offset),b=inverse*(Vector3)(center-end-offset);
            if(!FeverContactMath.Sweep(a.x,a.y,b.x,b.y,half.x,half.y,node.WorldRadius,out double along))continue;
            Vector2 local=Vector2.Lerp(a,b,(float)along),closest=new Vector2(Mathf.Clamp(local.x,-half.x,half.x),Mathf.Clamp(local.y,-half.y,half.y));
            Vector2 toward=player.transform.rotation*(Vector3)(closest-local).normalized;
            if(toward.sqrMagnitude<.001f)toward=(start-center).sqrMagnitude>.00001f?(start-center).normalized:-player.ContactMotionDirection;
            contacts.Add((node,(float)along,center+toward*node.WorldRadius));
        }
        contacts.Sort((a,b)=>a.along.CompareTo(b.along));Vector2 direction=end-start;
        foreach(var hit in contacts)hit.node.TryCollect(hit.point,direction.sqrMagnitude>.000001f?direction.normalized:player.ContactMotionDirection,true);
        contacts.Clear();
    }
    public bool Collect(FeverBonusNode node,Vector2 contact,Vector2 incoming,bool swept=false)
    {
        if(!CanCollect||!fever.AwardBonus(node))return false;
        Collected++;if(swept)SweptCollections++;
        Vector2 origin=node.transform.position;float radius=node.WorldRadius;
        lastContact=contact;lastOrigin=origin;diagnosticUntil=fever.Clock+.45f;
        if(CursorDashTuning.Toggle("fx.enabled",true)&&CursorDashTuning.Toggle("fx.particles",true)){
            bursts.RemoveAll(b=>b==null);
            // A bounded stream has fewer than 16 live .48 s bursts. Keep prior chunks through their decay.
            foreach(var previous in bursts)previous.SettleResidue();
            var burst=new GameObject("Fever bonus directional break"){hideFlags=HideFlags.DontSave}.AddComponent<FeverBonusBurst>();
            try{
                burst.Initialize(origin,contact,incoming,radius,fever.BonusImpactGain,fever.Visuals==null?.48f:fever.Visuals.feverFragmentLife);bursts.Add(burst);
            }catch{
                Object.Destroy(burst.gameObject); // Initialization failed before the owned list could register it.
                throw;
            }
        }
        fever.BonusFeel(contact,incoming);
        return true;
    }
    public void ClearNodes(){sampledHead=false;supply.Reset();foreach(var node in nodes)if(node!=null&&node.gameObject.activeSelf)node.ResetObject();}
    public void DrawDiagnostics()
    {
        if(!Diagnostics||fever.Model.Current!=FeverState.Phase.Fever||Camera.main==null)return;
        var camera=Camera.main;Color old=GUI.color;Matrix4x4 matrix=GUI.matrix;GUI.matrix=Matrix4x4.identity;
        void Line(Vector2 a,Vector2 b,Color color){Vector2 d=b-a;var save=GUI.matrix;GUI.color=color;GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg,a);GUI.DrawTexture(new Rect(a.x,a.y,d.magnitude,2),Texture2D.whiteTexture);GUI.matrix=save;}
        Vector2 Pixel(Vector2 world){Vector2 p=camera.WorldToScreenPoint(world);return new Vector2(p.x,Screen.height-p.y);}
        void Cross(Vector2 world,Color color){var p=Pixel(world);Line(p-Vector2.right*7,p+Vector2.right*7,color);Line(p-Vector2.up*7,p+Vector2.up*7,color);}
        var player=PlayerCharacterController.instance;var box=player==null?null:player.GetComponent<BoxCollider2D>();
        if(box!=null){Vector2 h=box.size*.5f,o=box.offset;Vector2[] q={o+new Vector2(-h.x,-h.y),o+new Vector2(h.x,-h.y),o+h,o+new Vector2(-h.x,h.y)};
            for(int i=0;i<4;i++)Line(Pixel(player.transform.TransformPoint(q[i])),Pixel(player.transform.TransformPoint(q[(i+1)%4])),Color.green);}
        foreach(var node in nodes)if(node.Collectible){Vector2 center=node.transform.position,last=center+Vector2.right*node.WorldRadius;
            for(int i=1;i<=24;i++){float a=i*Mathf.PI*2/24;Vector2 next=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*node.WorldRadius;Line(Pixel(last),Pixel(next),Color.magenta);last=next;}}
        if(fever.Clock<diagnosticUntil){Cross(lastContact,Color.red);Cross(lastOrigin,Color.cyan);}
        GUI.matrix=matrix;GUI.color=old;
    }
    public void ClearEffects(){foreach(var burst in bursts)if(burst!=null)Object.Destroy(burst.gameObject);bursts.Clear();}
    public void Dispose(){ClearNodes();ClearEffects();if(root!=null)Object.Destroy(root);if(quad!=null)Object.Destroy(quad);if(material!=null)Object.Destroy(material);}
}
