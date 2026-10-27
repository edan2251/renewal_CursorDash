using System.Collections.Generic;
using UnityEngine;

/// <summary>Fever-only irregular ballistic fragments and compact contact response. No shared normal VFX settings.</summary>
public sealed class FeverBonusBurst : MonoBehaviour
{
    public MeshRenderer View {get;private set;}
    private Mesh mesh;private Material material;private Vector3[] basis,points,centers,velocities;private int[] owners;
    private float[] drags,spins,shades,lifetimes;private Color[] colors;private float age,radius,flash,gain=1,life=.48f,residue=1,targetResidue=1;private Vector3 contact,forward;
    public void Initialize(Vector2 origin,Vector2 hit,Vector2 incoming,float size,float strength=2.5f,float duration=.48f)
    {
        life=Mathf.Clamp(duration,.25f,.7f);transform.position=origin;contact=hit-origin;radius=Mathf.Max(.04f,size);
        gain=Mathf.Sqrt(Mathf.Clamp(strength,.5f,4));
        forward=incoming.sqrMagnitude>.001f?(Vector3)incoming.normalized:Vector3.up;Vector3 side=new Vector3(-forward.y,forward.x);
        var vertices=new List<Vector3>();var indices=new List<int>();var ids=new List<int>();var edges=new List<Vector4>();
        void Polygon(Vector3[] shape,int owner)=>AppendPolygon(shape,owner,vertices,indices,ids,edges);
        int seed=Mathf.RoundToInt(origin.x*8191)^Mathf.RoundToInt(origin.y*131071)^Time.frameCount;
        float Sample(int i,int channel)=>(float)FeverScatter.Unit(seed,i,channel);
        const int count=18;centers=new Vector3[count];velocities=new Vector3[count];drags=new float[count];spins=new float[count];shades=new float[count];lifetimes=new float[count];
        for(int i=0;i<count;i++){
            float angle=Sample(i,0)*Mathf.PI*2,spread=Mathf.Sqrt(Sample(i,1));
            centers[i]=(forward*Mathf.Cos(angle)+side*Mathf.Sin(angle))*radius*spread*.72f;
            float lateral=Mathf.Lerp(-9.8f,9.8f,Sample(i,2));
            float drive=Mathf.Lerp(3.8f,12.2f,Sample(i,3));if(Sample(i,4)<.14f)drive=-drive*.38f;
            velocities[i]=(forward*drive+side*lateral)*gain;
            drags[i]=Mathf.Lerp(7,13,Sample(i,5));spins[i]=Mathf.Lerp(-760,760,Sample(i,6));shades[i]=Mathf.Lerp(.008f,.035f,Sample(i,7));lifetimes[i]=Mathf.Lerp(.78f,1,Sample(i,8));
            // Shape angle, size, velocity and damping use separate samples, eliminating a curved row.
            Vector3 heading=Quaternion.Euler(0,0,Sample(i,9)*360)*forward,cross=new Vector3(-heading.y,heading.x);
            float r=radius*Mathf.Lerp(.33f,.90f,Sample(i,10))*Mathf.Lerp(1,gain,.60f),skew=Mathf.Lerp(.38f,.85f,Sample(i,11));
            Polygon(Sample(i,12)<.50f?new[]{centers[i]+heading*r*1.3f,centers[i]+cross*r*skew,centers[i]-heading*r*.8f-cross*r*.4f}:
                new[]{centers[i]+heading*r,centers[i]+cross*r*skew,centers[i]-heading*r*.7f,centers[i]-cross*r*.4f},i);
        }
        // A short compressed remnant belongs to the hit node, not the nexus or the moving cursor.
        var shell=new Vector3[12];for(int j=0;j<shell.Length;j++){float a=j*Mathf.PI*2/shell.Length;shell[j]=(forward*Mathf.Cos(a)+side*Mathf.Sin(a))*radius;}Polygon(shell,-2);
        Polygon(new[]{contact-forward*radius*.24f,contact+side*radius*.34f,contact+forward*radius*.34f,contact-side*radius*.34f},-1);
        for(int i=-1;i<=1;i+=2){var c=side*i*radius*.75f;Polygon(new[]{c-forward*radius*.3f,c+forward*radius*2.5f,c+forward*radius*2.2f+side*i*radius*.09f,c+side*i*radius*.09f},-3);}
        basis=vertices.ToArray();points=(Vector3[])basis.Clone();owners=ids.ToArray();colors=new Color[points.Length];
        mesh=new Mesh{hideFlags=HideFlags.DontSave};mesh.MarkDynamic();mesh.vertices=points;mesh.triangles=indices.ToArray();mesh.SetUVs(1,edges);
        material=new Material(Resources.Load<Shader>("CursorDash/ImpactFlat")){hideFlags=HideFlags.DontSave};material.color=Color.white;
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;View=gameObject.AddComponent<MeshRenderer>();View.sharedMaterial=material;View.sortingOrder=30;
        flash=PlayerPrefs.GetString("OnFlash","true")=="true"?CursorDashTuning.EffectGain("user.flash"):0;Draw();
    }
    // ImpactFlat has four edge-distance channels. Convex shapes with more sides need a fan
    // whose triangles each carry their ONE outer edge; internal spokes must not become AA seams.
    private static void AppendPolygon(Vector3[] shape,int owner,List<Vector3> vertices,List<int> indices,List<int> ids,List<Vector4> edges)
    {
        float area=0;for(int j=0;j<shape.Length;j++){var a=shape[j];var b=shape[(j+1)%shape.Length];area+=a.x*b.y-a.y*b.x;}
        void Vertex(Vector3 point,int outerEdge=-1){
            vertices.Add(point);ids.Add(owner);var data=Vector4.one*1000;
            int count=outerEdge>=0?1:shape.Length;
            for(int k=0;k<count;k++){
                int side=outerEdge>=0?outerEdge:k;var a=shape[side];var e=shape[(side+1)%shape.Length]-a;var w=point-a;
                data[k]=(area>=0?1:-1)*(e.x*w.y-e.y*w.x)/Mathf.Max(.00001f,e.magnitude);
            }
            edges.Add(data);
        }
        if(shape.Length>4){
            Vector3 center=Vector3.zero;foreach(var point in shape)center+=point;center/=shape.Length;
            for(int j=0;j<shape.Length;j++){
                int first=vertices.Count;Vertex(center,j);Vertex(shape[j],j);Vertex(shape[(j+1)%shape.Length],j);
                indices.AddRange(new[]{first,first+1,first+2});
            }
            return;
        }
        int start=vertices.Count;foreach(var point in shape)Vertex(point);
        for(int j=1;j<shape.Length-1;j++)indices.AddRange(new[]{start,start+j,start+j+1});
    }
    public void SettleResidue()=>targetResidue=Mathf.Max(.42f,targetResidue*.82f);
    public void Advance(float delta){age+=delta;residue=Mathf.Lerp(residue,targetResidue,1-Mathf.Exp(-delta/.08f));if(age>=life){Destroy(gameObject);return;}Draw();}
    private void Draw()
    {
        float t=Mathf.Clamp01(age/life),travelAge=Mathf.Max(0,age-.014f);
        for(int k=0;k<points.Length;k++){
            int i=owners[k];float alpha=1;float shade=.012f;
            if(i==-1){points[k]=contact+(basis[k]-contact)*(1+age*gain*8);alpha=flash*Mathf.Clamp01(1-age/.035f);}
            else if(i==-2){points[k]=basis[k]-forward*Vector3.Dot(basis[k],forward)*(.28f*Mathf.Clamp01(age/.014f));alpha=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.014f,.032f,age));}
            else if(i==-3){points[k]=basis[k];alpha=Mathf.Clamp01(1-age/.075f);}
            else{
                float travel=(1-Mathf.Exp(-drags[i]*travelAge))/drags[i];points[k]=centers[i]+Quaternion.Euler(0,0,spins[i]*travelAge)*(basis[k]-centers[i])*(1-.18f*t)+velocities[i]*travel;
                if(age<.014f)points[k]-=forward*Vector3.Dot(points[k],forward)*(.20f*age/.014f);
                float end=lifetimes[i];alpha=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(end*.48f,end,t)))*Mathf.Lerp(1,residue,Mathf.Clamp01((age-.09f)/.12f));shade=shades[i];
            }
            colors[k]=new Color(shade,shade,shade,alpha);
        }
        mesh.vertices=points;mesh.colors=colors;mesh.RecalculateBounds();
    }
    private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
