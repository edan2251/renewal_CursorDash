using System.Collections.Generic;
using UnityEngine;

/// <summary>Reusable stroke vertices only. Shared by live circuits and static editor rendering.</summary>
public sealed class CircuitStrokeBuffer
{
    private readonly List<Vector3> vertices=new List<Vector3>(6000),uv=new List<Vector3>(6000);
    private readonly List<Vector2> screen=new List<Vector2>(6000);private readonly List<Color> colors=new List<Color>(6000);private readonly List<int> triangles=new List<int>(9000);
    public void Clear(){vertices.Clear();uv.Clear();screen.Clear();colors.Clear();triangles.Clear();}
    public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color tint,int kind,Camera camera,Color? endTint=null)
    {
        int n=vertices.Count;Add(a,-1,-1,tint);Add(b,-1,1,tint);Add(c,1,1,endTint??tint);Add(d,1,-1,endTint??tint);
        void Add(Vector3 p,float x,float y,Color color){vertices.Add(p);uv.Add(new Vector3(x,y,kind));screen.Add(camera.WorldToViewportPoint(p));colors.Add(color);}
        triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);triangles.Add(n);triangles.Add(n+2);triangles.Add(n+3);
    }
    public void Upload(Mesh mesh){mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetUVs(1,screen);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();}
}
