using UnityEngine;
public class ImpactFragments : MonoBehaviour
{
    private Mesh mesh;private Material material;private Vector3[] basis,points,centers,velocities;private Color[] colors;
    private float started,duration;private Color tint;
    private bool directional;private int[] owners;private float[] spins;private int flashStart;private float contactFlash;
    public Vector2 Incoming {get;private set;}
    public int PieceCount=>centers==null?0:centers.Length;
    public Vector3 MeanVelocity {get{Vector3 v=Vector3.zero;if(velocities!=null)foreach(var item in velocities)v+=item;return velocities==null?Vector3.zero:v/velocities.Length;}}
    public void InitializeDirectional(Vector3 origin,Vector3[] silhouette,Vector2 incoming,int count,float size,float speed,float life,
        Color color,Material source,float spread,float spin,bool circle)
    {
        directional=true;Incoming=incoming.normalized;transform.position=origin;count=Mathf.Clamp(count,1,24);
        var verts=new System.Collections.Generic.List<Vector3>();var indices=new System.Collections.Generic.List<int>();var ids=new System.Collections.Generic.List<int>();
        var edgeData=new System.Collections.Generic.List<Vector4>();
        centers=new Vector3[count];velocities=new Vector3[count];spins=new float[count];
        Vector3 center=origin;float radius=.27f;
        if(silhouette!=null&&silhouette.Length>=3){center=Vector3.zero;foreach(var p in silhouette)center+=p;center/=silhouette.Length;radius=0;foreach(var p in silhouette)radius=Mathf.Max(radius,Vector3.Distance(p,center));}
        // A legacy full export can contain the old erroneous size=1 fallback. Preserve its stored
        // value but never turn one circular shard into an object larger than the shattered body.
        if(circle)size=Mathf.Min(Mathf.Max(.005f,size),radius*.24f);
        Vector3 drift=Incoming,side=new Vector3(-drift.y,drift.x);
        for(int i=0;i<count;i++)
        {
            float seed=Mathf.Repeat(i*.61803399f+.17f,1),a=seed*Mathf.PI*2;
            Vector3 radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a));
            Vector3 c=center-origin+radial*radius*(i%3==0?.78f:.32f);
            centers[i]=c;spins[i]=spin*(i%2==0?1:-1)*Mathf.Lerp(.45f,1.2f,seed);
            float theta=(seed*2-1)*spread;
            velocities[i]=Quaternion.Euler(0,0,theta)*drift*speed*Mathf.Lerp(.6f,1.25f,Mathf.Repeat(seed*3.7f,1));
            int start=verts.Count;int corners;
            if(i%3==0&&silhouette!=null&&silhouette.Length>=3){
                // A narrow piece of the real border; gaps/unequal lengths prevent a radial pizza fan.
                int edge=Mathf.FloorToInt(seed*silhouette.Length)%silhouette.Length;
                Vector3 p=Vector3.Lerp(silhouette[edge],silhouette[(edge+1)%silhouette.Length],.12f)-origin;
                Vector3 q=Vector3.Lerp(silhouette[edge],silhouette[(edge+1)%silhouette.Length],.78f)-origin;
                float thickness=circle?Mathf.Min(size*.45f,radius*.08f):radius*.15f*size;
                Vector3 inset=-(p+q-2*(center-origin)).normalized*thickness;
                verts.Add(p);verts.Add(q);verts.Add(q+inset);verts.Add(p+inset);corners=4;
                centers[i]=(p+q+inset)*.5f;
            }else{
                float s=circle?size:radius*.35f*size;corners=i%2==0?3:4;
                for(int j=0;j<corners;j++){
                    float angle=(j/(float)corners+.11f*seed)*Mathf.PI*2;
                    Vector3 local=drift*Mathf.Cos(angle)*s*(j==0?1.3f:.7f)+side*Mathf.Sin(angle)*s*.65f;
                    verts.Add(c+local);
                }
            }
            for(int j=0;j<corners;j++)ids.Add(i);
            for(int j=1;j<corners-1;j++){indices.Add(start);indices.Add(start+j);indices.Add(start+j+1);}
            AddEdges(verts,start,corners,edgeData);
        }
        flashStart=verts.Count;
        float flashSize=Mathf.Clamp(radius*.45f,.045f,.16f);
        verts.Add(-drift*flashSize);verts.Add(side*flashSize*.55f);verts.Add(drift*flashSize*1.5f);verts.Add(-side*flashSize*.55f);
        for(int j=0;j<4;j++)ids.Add(-1);
        indices.AddRange(new[]{flashStart,flashStart+1,flashStart+2,flashStart,flashStart+2,flashStart+3});
        AddEdges(verts,flashStart,4,edgeData);
        contactFlash=PlayerPrefs.GetString("OnFlash","true")=="true"?CursorDashTuning.Number("user.flash",1):0;
        basis=verts.ToArray();points=(Vector3[])basis.Clone();owners=ids.ToArray();colors=new Color[points.Length];
        for(int i=0;i<colors.Length;i++)colors[i]=i>=flashStart?new Color(1,1,1,contactFlash):color;
        tint=color;started=Time.unscaledTime;duration=Mathf.Max(.05f,life);
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.MarkDynamic();mesh.vertices=points;mesh.triangles=indices.ToArray();mesh.colors=colors;mesh.uv=new Vector2[points.Length];mesh.SetUVs(1,edgeData);
        var flatShader=Resources.Load<Shader>("CursorDash/ImpactFlat");
        material=flatShader!=null&&flatShader.isSupported?new Material(flatShader):new Material(source);
        material.hideFlags=HideFlags.DontSave;material.mainTexture=Texture2D.whiteTexture;material.color=Color.white;
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=30;
    }
    private static void AddEdges(System.Collections.Generic.List<Vector3> vertices,int start,int count,System.Collections.Generic.List<Vector4> data)
    {
        float area=0;for(int j=0;j<count;j++){Vector3 a=vertices[start+j],b=vertices[start+(j+1)%count];area+=a.x*b.y-a.y*b.x;}
        float sign=area>=0?1:-1;
        for(int j=0;j<count;j++){
            Vector4 distances=Vector4.one*1000;Vector3 p=vertices[start+j];
            for(int k=0;k<count;k++){
                Vector3 a=vertices[start+k],edge=vertices[start+(k+1)%count]-a,w=p-a;
                distances[k]=sign*(edge.x*w.y-edge.y*w.x)/Mathf.Max(.00001f,edge.magnitude);
            }
            data.Add(distances);
        }
    }
    public void Initialize(Vector3 origin,Vector3[] silhouette,int count,float size,float speed,float life,Color color,Material source)
    {
        transform.position=origin;count=Mathf.Clamp(count,1,24);
        basis=new Vector3[count*3];points=new Vector3[basis.Length];centers=new Vector3[count];velocities=new Vector3[count];colors=new Color[basis.Length];
        int[] indices=new int[basis.Length];
        for(int i=0;i<count;i++)
        {
            float a=i*Mathf.PI*2/count;Vector3 direction=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
            int k=i*3;
            if(silhouette!=null&&silhouette.Length==3) {
                basis[k]=Vector3.zero;
                basis[k+1]=(Perimeter(silhouette,(float)i/count)-origin)*size;
                basis[k+2]=(Perimeter(silhouette,(float)(i+1)/count)-origin)*size;
            } else {
                basis[k]=direction*size*.2f;
                basis[k+1]=direction*size+new Vector3(-direction.y,direction.x,0)*size*.3f;
                basis[k+2]=direction*size-new Vector3(-direction.y,direction.x,0)*size*.3f;
            }
            centers[i]=(basis[k]+basis[k+1]+basis[k+2])/3;
            velocities[i]=(centers[i].sqrMagnitude>.00001f?centers[i].normalized:direction)*speed;
            for(int j=0;j<3;j++){indices[k+j]=k+j;points[k+j]=basis[k+j];colors[k+j]=color;}
        }
        tint=color;started=Time.unscaledTime;duration=Mathf.Max(.02f,life);
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.vertices=points;mesh.triangles=indices;mesh.colors=colors;mesh.uv=new Vector2[points.Length];
        material=new Material(source){hideFlags=HideFlags.DontSave};material.mainTexture=Texture2D.whiteTexture;material.color=Color.white;
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        var r=gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.sortingOrder=30;
    }
    private static Vector3 Perimeter(Vector3[] p,float t) {
        float f=Mathf.Repeat(t,1)*3;int edge=Mathf.FloorToInt(f);return Vector3.Lerp(p[edge],p[(edge+1)%3],f-edge);
    }
    private void Update()
    {
        if(mesh==null)return;float t=(Time.unscaledTime-started)/duration;if(t>=1){Destroy(gameObject);return;}
        float seconds=t*duration;
        if(directional){
            for(int k=0;k<points.Length;k++){
                int i=owners[k];
                if(i<0){points[k]=basis[k]*(1+seconds*3);colors[k]=new Color(1,1,1,contactFlash*Mathf.Clamp01(1-seconds/.035f));continue;}
                float travel=Mathf.Max(0,seconds-.018f),compression=1-.22f*Mathf.Sin(Mathf.Clamp01(seconds/.036f)*Mathf.PI);
                float shrink=1-.55f*Mathf.SmoothStep(0,1,t);
                points[k]=centers[i]*compression+Quaternion.Euler(0,0,travel*spins[i])*(basis[k]-centers[i])*shrink+velocities[i]*(travel*(1-.30f*t));
                colors[k]=new Color(tint.r,tint.g,tint.b,tint.a*Mathf.Clamp01((1-t)*1.5f));
            }
            mesh.vertices=points;mesh.colors=colors;mesh.RecalculateBounds();return;
        }
        for(int i=0;i<centers.Length;i++) {
            Quaternion turn=Quaternion.Euler(0,0,seconds*(i%2==0?120:-120));
            for(int j=0;j<3;j++){int k=i*3+j;points[k]=centers[i]+turn*(basis[k]-centers[i])+velocities[i]*seconds;colors[k]=new Color(tint.r,tint.g,tint.b,tint.a*(1-t));}
        }
        mesh.vertices=points;mesh.colors=colors;mesh.RecalculateBounds();
    }
    private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
