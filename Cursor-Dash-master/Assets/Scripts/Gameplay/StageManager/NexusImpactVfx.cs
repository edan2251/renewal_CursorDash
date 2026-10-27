using System.Collections.Generic;
using UnityEngine;

/// <summary>C/D contact profiles and one exclusive whole-body finale. Never handles damage or scores.</summary>
public sealed class NexusImpactVfx : MonoBehaviour
{
    public enum Cause { Manual, Circle, Laser }
    public const float FinaleDuration=.64f;
    public const float BreakAt=.016f;
    public const float ShockDuration=.05f,SlowDuration=.16f;
    private Mesh mesh;private Material material;
    private readonly List<Vector3> basis=new List<Vector3>(),centers=new List<Vector3>(),velocities=new List<Vector3>();
    private readonly List<int> owners=new List<int>(),triangles=new List<int>();
    private readonly List<Vector4> edges=new List<Vector4>();
    private readonly List<float> spins=new List<float>(),drags=new List<float>();
    private Vector3[] points;private Color[] colors;private Color tint;private Vector3 flashPoint;
    private float age,duration,flashOpacity,contactHold;private bool final,bodyHidden;
    private NexusGameboardPresentation presentation;private System.Action hideBody;
    public Cause SourceCause {get;private set;}
    public Vector2 Incoming {get;private set;}
    public Vector2 MainDirection=>final?FinalDirection(Incoming):Rebound(Incoming);
    public bool IsFinal=>final;
    public float Age=>age;
    public int PieceCount=>centers.Count;
    public Vector3 MeanVelocity {get{Vector3 sum=Vector3.zero;foreach(var v in velocities)sum+=v;return centers.Count==0?sum:sum/centers.Count;}}

    // Integral of a single presentation clock. Every final shard/spin/fade uses this clock, not Time.timeScale.
    public static float FinaleMotionAge(float realAge)
    {
        float t=Mathf.Max(0,realAge-BreakAt);
        // The initial separation must read BEFORE the slow-motion tail, including unscaled VFX.
        float shock=Mathf.Min(t,ShockDuration)*2.2f;
        float tail=Mathf.Max(0,t-ShockDuration);
        return shock+Mathf.Min(tail,SlowDuration)*.25f+Mathf.Max(0,tail-SlowDuration)*1.4f;
    }
    public static Vector2 FinalDirection(Vector2 incoming)=>-Rebound(incoming);
    public static Vector2 Rebound(Vector2 incoming)=>incoming.sqrMagnitude>.00001f?-incoming.normalized:Vector2.up;
    public void InitializeContact(Cause cause,Vector2 contact,Vector2 incoming,Color color,float circleLife=.4f)
    {
        SourceCause=cause;Incoming=incoming.sqrMagnitude>.001f?incoming.normalized:Vector2.down;transform.position=contact;tint=color;
        // C keeps the .4s footprint and strong white onset; D is an independent contact burst, not a muzzle effect.
        contactHold=cause==Cause.Circle?Mathf.Clamp(CursorDashTuning.Number("fx.nexusDamage.stop",.08f),0,.1f):.035f;
        duration=(cause==Cause.Circle?Mathf.Max(.4f,circleLife):.38f)+contactHold;
        AddChips(cause==Cause.Circle?22:18,Vector3.zero,Rebound(Incoming),cause==Cause.Circle?.095f:.080f,cause==Cause.Circle?5.2f:4.8f);
        flashPoint=Vector3.zero;AddFlash(.17f);FinishMesh();Draw(0);
    }
    public void InitializeFinal(Vector2 center,float radius,Vector2 contact,Vector2 incoming,Color color,NexusGameboardPresentation view,System.Action hide,Cause cause=Cause.Manual)
    {
        final=true;SourceCause=cause;duration=FinaleDuration;tint=color;Incoming=incoming.sqrMagnitude>.001f?incoming.normalized:Vector2.down;
        transform.position=center;presentation=view;hideBody=hide;flashPoint=(Vector3)(contact-center);
        // D-026 user correction: reverse the CURRENT final emission, not ordinary C/D contacts.
        AddBodyCells(Mathf.Max(.05f,radius),MainDirection);
        AddChips(24,flashPoint,MainDirection,radius*.13f,5.0f);
        AddFlash(Mathf.Max(.18f,radius*.55f));FinishMesh();Draw(0);
    }
    private static float Seed(int i,float offset)=>Mathf.Repeat(i*.61803399f+offset,1);
    private void AddChips(int count,Vector3 origin,Vector2 direction,float size,float speed)
    {
        Vector3 forward=direction,side=new Vector3(-direction.y,direction.x);
        for(int i=0;i<count;i++){
            float a=Seed(i,.17f),b=Seed(i*3,.31f);bool main=i<count*3/4;
            Vector3 heading=Quaternion.Euler(0,0,(a*2-1)*(main?66:135))*forward;
            var center=origin+side*(a-.5f)*.09f;
            int owner=centers.Count;centers.Add(center);velocities.Add(heading*speed*Mathf.Lerp(.55f,1.5f,b));
            spins.Add((i%2==0?1:-1)*Mathf.Lerp(140,680,a));drags.Add(Mathf.Lerp(7,12,b));
            float length=size*Mathf.Lerp(.55f,1.55f,b);Vector3 cross=new Vector3(-heading.y,heading.x);
            AddPolygon(new[]{center+heading*length,center+cross*length*.35f,center-heading*length*.65f,center-cross*length*Mathf.Lerp(.2f,.55f,a)},owner);
        }
    }
    private void AddBodyCells(float radius,Vector2 direction)
    {
        // Convex Voronoi cells tile the WHOLE body once; no stacked explosion copies or HUD geometry.
        var sites=new List<Vector2>{new Vector2(-.13f,.07f)*radius};
        for(int i=0;i<11;i++){float a=i*Mathf.PI*2/11+.14f;sites.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*Mathf.Lerp(.42f,.79f,Seed(i,.33f)));}
        for(int i=0;i<sites.Count;i++){
            var polygon=new List<Vector2>();for(int n=0;n<48;n++){float a=n*Mathf.PI*2/48;polygon.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
            for(int j=0;j<sites.Count;j++)if(i!=j){Vector2 normal=sites[j]-sites[i];float offset=(sites[j].sqrMagnitude-sites[i].sqrMagnitude)*.5f;polygon=Clip(polygon,normal,offset);}
            if(polygon.Count<3)continue;
            Vector3 center=Vector3.zero;foreach(var v in polygon)center+=(Vector3)v;center/=polygon.Count;
            int owner=centers.Count;centers.Add(center);
            Vector3 heading=((Vector2)center.normalized*.78f+direction*.72f).normalized;
            velocities.Add(heading*Mathf.Lerp(2.4f,4.1f,Seed(i,.49f)));spins.Add((i%2==0?1:-1)*Mathf.Lerp(95,310,Seed(i,.62f)));drags.Add(4.5f);
            var verts=new Vector3[polygon.Count];for(int n=0;n<verts.Length;n++)verts[n]=center+((Vector3)polygon[n]-center)*.975f;
            AddPolygon(verts,owner);
        }
    }
    private static List<Vector2> Clip(List<Vector2> input,Vector2 normal,float offset)
    {
        var output=new List<Vector2>();if(input.Count==0)return output;
        Vector2 previous=input[input.Count-1];float pd=Vector2.Dot(previous,normal)-offset;
        foreach(var current in input){float cd=Vector2.Dot(current,normal)-offset;
            if((cd<=0)!=(pd<=0))output.Add(Vector2.Lerp(previous,current,pd/(pd-cd)));
            if(cd<=0)output.Add(current);previous=current;pd=cd;
        }return output;
    }
    private void AddFlash(float size)
    {
        Vector3 forward=MainDirection,side=new Vector3(-forward.y,forward.x);
        AddPolygon(new[]{flashPoint-forward*size*.45f,flashPoint+side*size*.65f,flashPoint+forward*size*1.2f,flashPoint-side*size*.65f},-1);
    }
    private void AddPolygon(Vector3[] polygon,int owner)
    {
        int first=basis.Count;float winding=0;
        for(int j=0;j<polygon.Length;j++){Vector3 a=polygon[j],b=polygon[(j+1)%polygon.Length];winding+=a.x*b.y-a.y*b.x;}
        for(int j=0;j<polygon.Length;j++){
            basis.Add(polygon[j]);owners.Add(owner);
            // ImpactFlat has four edge channels. For larger body cells, leave conservative coverage;
            // their fan is interior and triangle rasterization supplies the exterior edge.
            Vector4 data=Vector4.one*1000;
            if(polygon.Length<=4)for(int k=0;k<polygon.Length;k++){Vector3 a=polygon[k],edge=polygon[(k+1)%polygon.Length]-a,w=polygon[j]-a;data[k]=(winding>=0?1:-1)*(edge.x*w.y-edge.y*w.x)/Mathf.Max(.00001f,edge.magnitude);}
            edges.Add(data);
        }
        for(int n=1;n<polygon.Length-1;n++)triangles.AddRange(new[]{first,first+n,first+n+1});
    }
    private void FinishMesh()
    {
        var shader=Resources.Load<Shader>("CursorDash/ImpactFlat");if(shader==null||!shader.isSupported){Destroy(gameObject);return;}
        flashOpacity=PlayerPrefs.GetString("OnFlash","true")=="true"?CursorDashTuning.EffectGain("user.flash"):0;
        material=new Material(shader){hideFlags=HideFlags.DontSave};material.color=Color.white;
        points=basis.ToArray();colors=new Color[points.Length];mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.MarkDynamic();
        mesh.vertices=points;mesh.triangles=triangles.ToArray();mesh.SetUVs(1,edges);
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=35;
    }
    private void Update()
    {
        age+=Time.unscaledDeltaTime;
        if(final){
            if(presentation!=null)presentation.FinaleScale=1-.13f*Mathf.Clamp01(age/BreakAt);
            if(age>=BreakAt&&!bodyHidden){bodyHidden=true;if(presentation!=null)presentation.FinaleAlpha=0;hideBody?.Invoke();}
        }
        if(age>=duration){Destroy(gameObject);return;}Draw(age);
    }
    private void Draw(float realAge)
    {
        if(mesh==null)return;float t=final?FinaleMotionAge(realAge):Mathf.Max(0,realAge-contactHold);
        float total=final?FinaleMotionAge(duration):duration-contactHold,progress=Mathf.Clamp01(t/total);
        for(int n=0;n<points.Length;n++){
            int i=owners[n];
            if(i<0){points[n]=flashPoint+(basis[n]-flashPoint)*(1+realAge*2);colors[n]=Color.white;colors[n].a=flashOpacity*Mathf.Clamp01(1-(final?realAge:t)/(final?.075f:.055f));continue;}
            float travel=(1-Mathf.Exp(-drags[i]*t))/drags[i];
            points[n]=centers[i]+Quaternion.Euler(0,0,spins[i]*t)*(basis[n]-centers[i])*(1-progress*.35f)+velocities[i]*travel;
            float fade=Mathf.Clamp01((1-progress)*2);colors[n]=Color.Lerp(tint,Color.white,Mathf.Clamp01(1-t/.055f)*.95f);
            colors[n].a*=fade*(final?(realAge>=BreakAt?1:0):1);
        }
        mesh.vertices=points;mesh.colors=colors;mesh.RecalculateBounds();
    }
    private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
