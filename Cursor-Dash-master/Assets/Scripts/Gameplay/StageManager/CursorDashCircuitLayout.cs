using UnityEngine;

/// <summary>Editable normalized point positions on the approved connected topology. Crossings never create extra links.</summary>
[CreateAssetMenu(menuName="CursorDash/Circuit path layout")]
public sealed class CursorDashCircuitLayout : ScriptableObject
{
    [Tooltip("1080×1920 reference, centered normalized coordinates. Topology/branch ownership stays in CircuitPathGraph.")]
    public Vector2[] points;
    public void CaptureApprovedLayout()
    {
        var graph=new CircuitPathGraph(1,1);points=new Vector2[graph.Points.Length];
        for(int i=0;i<points.Length;i++)points[i]=new Vector2((float)graph.Points[i].X,(float)graph.Points[i].Y);
    }
    public CircuitPathGraph CreateGraph(double aspectX,double aspectY,int density)
    {
        if(points==null||points.Length==0)return new CircuitPathGraph(aspectX,aspectY,density);
        var mapped=new CircuitPathGraph.Point[points.Length];
        for(int i=0;i<points.Length;i++)mapped[i]=new CircuitPathGraph.Point(points[i].x*System.Math.Max(1,aspectX),points[i].y*System.Math.Max(1,aspectY));
        return new CircuitPathGraph(aspectX,aspectY,density,mapped);
    }
}
