using System.Collections.Generic;
using UnityEngine;

/// <summary>A only: irregular chunky onset and a forward/side fragment wake. No legacy sprite playback.</summary>
public sealed class CircularBreakVfx : MonoBehaviour
{
    private static uint burstSerial;
    public bool UsesOriginalSheet=>false;
    private Mesh mesh;private Material material;
    private Vector3[] basis,points,centers,velocities;private int[] owners;private Color[] colors;
    private float[] spins,drags,shades,lifetimes;
    private Vector3 center,drift,side;private Color tint;private float started,duration,contactFlash,radius;
    public Vector2 Incoming {get;private set;}
    public int PieceCount=>centers==null?0:centers.Length;
    public float ClearCorridorHalfWidth=>radius*.15f;
    public Vector3 MeanVelocity {get{Vector3 sum=Vector3.zero;if(velocities!=null)foreach(var v in velocities)sum+=v;return velocities==null?sum:sum/velocities.Length;}}
    public void Initialize(Vector3 origin,Vector3[] silhouette,Vector2 incoming,int count,float size,float speed,float life,Color color,Material source,float spread,float spin,GameObject unusedLegacyTemplate=null)
    {
        transform.position=origin;Incoming=incoming.sqrMagnitude<.001f?Vector2.up:incoming.normalized;drift=Incoming;side=new Vector3(-drift.y,drift.x);
        center=Vector3.zero;radius=.27f;
        if(silhouette!=null&&silhouette.Length>=3){foreach(var p in silhouette)center+=p;center/=silhouette.Length;center-=origin;radius=0;foreach(var p in silhouette)radius=Mathf.Max(radius,Vector3.Distance(p-origin,center));}
        radius=Mathf.Max(.03f,radius);count=Mathf.Clamp(count,4,16);float sizeRatio=Mathf.Clamp(size/.05f,.3f,1.25f);
        centers=new Vector3[count];velocities=new Vector3[count];spins=new float[count];drags=new float[count];shades=new float[count];lifetimes=new float[count];
        var verts=new List<Vector3>();var indices=new List<int>();var ids=new List<int>();var edgeData=new List<Vector4>();
        float burstSeed=Mathf.Repeat(++burstSerial*.137f+.23f,1);
        void Polygon(Vector3[] polygon,int owner){int first=verts.Count;verts.AddRange(polygon);for(int j=0;j<polygon.Length;j++)ids.Add(owner);for(int j=1;j<polygon.Length-1;j++)indices.AddRange(new[]{first,first+j,first+j+1});AddEdges(verts,first,polygon.Length,edgeData);}
        for(int i=0;i<count;i++){
            float a=Mathf.Repeat(i*.61803399f+burstSeed,1),b=Mathf.Repeat(a*4.37f+.13f,1),c=Mathf.Repeat(b*3.71f+.29f,1);
            float flank=i%2==0?1:-1;
            // Unequal chunks, not two mirrored plates. All retain forward force while opening a wide wake.
            float degrees=flank*Mathf.Lerp(18,Mathf.Clamp(spread+15,45,85),a);
            Vector3 heading=Quaternion.Euler(0,0,degrees)*drift,across=new Vector3(-heading.y,heading.x);
            float length=radius*Mathf.Lerp(.25f,.52f,b)*sizeRatio;
            Vector3 axis=Quaternion.Euler(0,0,Mathf.Lerp(-65,65,c))*heading,cross=new Vector3(-axis.y,axis.x);
            Vector3[] shape=i%3==0?new[]{axis*length*1.25f,cross*length*.70f,-axis*length*.85f-cross*length*.46f}:
                new[]{axis*length*Mathf.Lerp(.8f,1.35f,c),cross*length*Mathf.Lerp(.48f,.92f,a),-axis*length*Mathf.Lerp(.5f,1,b),-cross*length*Mathf.Lerp(.35f,.75f,c)};
            // Keep the initial silhouette clear of the travel axis without making two solid blast masses.
            float inward=0;foreach(var p in shape)inward=Mathf.Max(inward,-Vector3.Dot(p,side)*flank);
            centers[i]=center+drift*radius*Mathf.Lerp(-.95f,.85f,a)+side*flank*(ClearCorridorHalfWidth+inward+radius*Mathf.Lerp(.04f,.34f,c));
            velocities[i]=heading*Mathf.Max(0,speed)*Mathf.Lerp(2.4f,4.3f,b);
            drags[i]=Mathf.Lerp(6.5f,11.5f,c);spins[i]=spin*(c>.46f?1:-1)*Mathf.Lerp(.8f,2.7f,b);shades[i]=Mathf.Lerp(.82f,1,a);
            lifetimes[i]=Mathf.Lerp(.78f,1,c);
            for(int n=0;n<shape.Length;n++)shape[n]+=centers[i];Polygon(shape,i);
        }
        // Only the tiny contact flash uses a paired shape; there are no large paired blast plates.
        for(int flank=-1;flank<=1;flank+=2){
            Vector3 f=side*flank*radius*.24f;
            Polygon(new[]{f-drift*radius*.22f,f+side*flank*radius*.2f,f+drift*radius*.38f,f-side*flank*radius*.06f},-1);
        }
        basis=verts.ToArray();points=(Vector3[])basis.Clone();owners=ids.ToArray();colors=new Color[points.Length];tint=color;
        contactFlash=PlayerPrefs.GetString("OnFlash","true")=="true"?CursorDashTuning.Number("user.flash",1):0;
        started=Time.unscaledTime;duration=Mathf.Clamp(life,.18f,.55f);
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.MarkDynamic();mesh.vertices=points;mesh.triangles=indices.ToArray();mesh.uv=new Vector2[points.Length];mesh.SetUVs(1,edgeData);
        Shader shader=Resources.Load<Shader>("CursorDash/ImpactFlat");material=shader!=null&&shader.isSupported?new Material(shader):new Material(source);
        material.hideFlags=HideFlags.DontSave;material.mainTexture=Texture2D.whiteTexture;material.color=Color.white;
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var view=gameObject.AddComponent<MeshRenderer>();view.sharedMaterial=material;view.sortingOrder=30;Draw(0);
    }
    private static void AddEdges(List<Vector3> vertices,int start,int count,List<Vector4> data)
    {
        float area=0;for(int i=0;i<count;i++){Vector3 a=vertices[start+i],b=vertices[start+(i+1)%count];area+=a.x*b.y-a.y*b.x;}
        for(int i=0;i<count;i++){
            Vector4 distances=Vector4.one*1000;
            for(int j=0;j<count;j++){Vector3 a=vertices[start+j],e=vertices[start+(j+1)%count]-a,w=vertices[start+i]-a;distances[j]=(area>=0?1:-1)*(e.x*w.y-e.y*w.x)/Mathf.Max(.00001f,e.magnitude);}
            data.Add(distances);
        }
    }
    private void Update(){if(mesh==null)return;float age=Time.unscaledTime-started;if(age>=duration){Destroy(gameObject);return;}Draw(age);}
    private void Draw(float seconds)
    {
        for(int k=0;k<points.Length;k++){
            int i=owners[k];
            if(i==-1){points[k]=basis[k];colors[k]=Color.Lerp(tint,Color.white,.75f);colors[k].a=contactFlash*Mathf.Clamp01(1-seconds/.035f);continue;}
            float t=Mathf.Clamp01(seconds/(duration*lifetimes[i]));
            float travel=(1-Mathf.Exp(-drags[i]*seconds))/drags[i],rotation=(1-Mathf.Exp(-9*seconds))/9;
            // Hold size/opacity through the cursor crossing; fade before shrinking to a dot.
            float shrink=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.48f,1,t));
            points[k]=centers[i]+Quaternion.Euler(0,0,spins[i]*rotation)*(basis[k]-centers[i])*(1-.32f*shrink)+velocities[i]*travel;
            colors[k]=Color.Lerp(new Color(tint.r*shades[i],tint.g*shades[i],tint.b*shades[i],tint.a),Color.white,.22f*Mathf.Clamp01(1-seconds/.045f));
            colors[k].a=tint.a*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.42f,1,t)));
        }
        mesh.vertices=points;mesh.colors=colors;mesh.RecalculateBounds();
    }
    private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
