using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Places only the decorative circuit wires in the world pass. HUD/text/buttons keep their own camera.</summary>
[RequireComponent(typeof(Camera))]
public sealed class WorldCircuitBackdrop : MonoBehaviour
{
    private Camera cameraView;
    private GameObject artwork;
    private Mesh mesh;
    private Material material;
    private readonly List<Image> originals=new List<Image>();
    private readonly List<bool> enabledStates=new List<bool>();
    private float aspect=-1;
    private Vector3 restPosition;
    private int pixelWidth=-1,pixelHeight=-1;
    public int SegmentCount {get;private set;}
    public MeshRenderer Renderer {get;private set;}
    private void Awake()
    {
        cameraView=GetComponent<Camera>();
        restPosition=cameraView.transform.position;
        foreach(var image in FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
            if(image.name!="Circuit"||image.transform.root.name!="RemakeHUD")continue;
            originals.Add(image);enabledStates.Add(image.enabled);image.enabled=false;
        }
        artwork=new GameObject("World circuit decoration"){hideFlags=HideFlags.DontSave};
        mesh=new Mesh{name="Circuit lines in the distortion pass",hideFlags=HideFlags.DontSave};
        material=new Material(Resources.Load<Shader>("CursorDash/ImpactFlat")){hideFlags=HideFlags.DontSave};
        artwork.AddComponent<MeshFilter>().sharedMesh=mesh;Renderer=artwork.AddComponent<MeshRenderer>();
        Renderer.sharedMaterial=material;Renderer.sortingOrder=0;
    }
    private void OnPreCull()
    {
        if(mesh==null)return;
        if(pixelWidth==cameraView.pixelWidth&&pixelHeight==cameraView.pixelHeight&&Mathf.Abs(aspect-cameraView.aspect)<.00001f)return;
        pixelWidth=cameraView.pixelWidth;pixelHeight=cameraView.pixelHeight;aspect=cameraView.aspect;
        var vertices=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();var edges=new List<Vector4>();SegmentCount=0;
        Canvas.ForceUpdateCanvases();
        if(originals.Count>0){
            foreach(var image in originals){
                if(image==null)continue;
                var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);var canvas=image.canvas;
                for(int i=0;i<4;i++){
                    Vector2 screen=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,corners[i]);
                    corners[i]=FromViewport(new Vector2(screen.x/Screen.width,screen.y/Screen.height));
                }
                Append(corners,image.color,vertices,triangles,colors,edges);
            }
        }else{
            // Same 60 reference-layout segments as the existing RemakeHUD setup, for scenes without saved Circuit children.
            // This does not run Setup, rebuild any UI, write a scene, or change the background bitmap.
            for(int mirror=0;mirror<4;mirror++)for(int i=0;i<5;i++){
                float x=52+i*67,depth=170+i*68;
                var p=new[]{new Vector2(x,1920),new Vector2(x,1920-depth),new Vector2(x+38,1882-depth),new Vector2(x+38,1650-depth)};
                for(int j=0;j<p.Length;j++){if((mirror&1)!=0)p[j].x=1080-p[j].x;if((mirror&2)!=0)p[j].y=1920-p[j].y;}
                for(int j=0;j<3;j++){
                    Vector2 normal=new Vector2(-(p[j+1]-p[j]).y,(p[j+1]-p[j]).x).normalized*2;
                    var quad=new[]{p[j]+normal,p[j+1]+normal,p[j+1]-normal,p[j]-normal};
                    var world=new Vector3[4];for(int k=0;k<4;k++)world[k]=FromViewport(new Vector2(quad[k].x/1080,quad[k].y/1920));
                    Append(world,new Color(.38f,.48f,.77f,.2f),vertices,triangles,colors,edges);
                }
            }
        }
        mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.SetUVs(1,edges);mesh.RecalculateBounds();
    }
    private Vector3 FromViewport(Vector2 p)
    {
        // Scene gameplay plane is z=0. The unparented mesh follows the world camera's shake, not the stable HUD.
        Vector3 point=cameraView.ViewportToWorldPoint(new Vector3(p.x,p.y,-cameraView.transform.position.z));
        point-=cameraView.transform.position-restPosition;
        return point;
    }
    private void Append(Vector3[] points,Color color,List<Vector3> vertices,List<int> triangles,List<Color> colors,List<Vector4> edges)
    {
        int start=vertices.Count;float area=0;for(int j=0;j<4;j++){var a=points[j];var b=points[(j+1)%4];area+=a.x*b.y-a.y*b.x;}
        float sign=area>=0?1:-1;
        for(int j=0;j<4;j++){
            vertices.Add(points[j]);colors.Add(color);Vector4 e=Vector4.zero;
            for(int k=0;k<4;k++){Vector3 a=points[k],d=points[(k+1)%4]-a,w=points[j]-a;e[k]=sign*(d.x*w.y-d.y*w.x)/Mathf.Max(.00001f,d.magnitude);}
            edges.Add(e);
        }
        triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});SegmentCount++;
    }
    private void OnDestroy()
    {
        for(int i=0;i<originals.Count;i++)if(originals[i]!=null)originals[i].enabled=enabledStates[i];
        if(artwork!=null)Destroy(artwork);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);
    }
}
