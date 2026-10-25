using UnityEngine;

/// <summary>Owned runtime vector artwork; never edits a sprite, importer, collider or node clock.</summary>
public sealed class NodeFlatGraphic : MonoBehaviour
{
    private Mesh mesh; private Material material; private MeshRenderer view;
    private readonly Vector4[] polygon=new Vector4[6];
    private readonly Vector3[] quad=new Vector3[4];
    public float QuadExtent {get;private set;}
    public bool Visible {get=>view!=null&&view.enabled;set{if(view!=null)view.enabled=value;}}
    public float Energy {get;private set;}
    public float CoreRadius {get;private set;}
    public static NodeFlatGraphic Create(Transform parent,SpriteRenderer reference)
    {
        var go=new GameObject("Flat node artwork"){hideFlags=HideFlags.DontSave,layer=parent.gameObject.layer};
        go.transform.SetParent(parent,false);
        var art=go.AddComponent<NodeFlatGraphic>();
        art.mesh=new Mesh{name="Editable procedural node quad",hideFlags=HideFlags.DontSave};
        art.mesh.vertices=new[]{new Vector3(-2,-2),new Vector3(2,-2),new Vector3(2,2),new Vector3(-2,2)};
        art.mesh.triangles=new[]{0,1,2,0,2,3};
        go.AddComponent<MeshFilter>().sharedMesh=art.mesh;
        art.view=go.AddComponent<MeshRenderer>();
        art.material=new Material(Resources.Load<Shader>("CursorDash/NodeFlat")){hideFlags=HideFlags.DontSave};
        art.view.sharedMaterial=art.material;art.view.sortingLayerID=reference.sortingLayerID;art.view.sortingOrder=reference.sortingOrder+3;
        return art;
    }
    // Alternating cut / original triangle edges, all six nonzero. Radius remains circumradius (legacy hex.radius).
    public static Vector2[] Hexagon(float radius,float shortToLong)
    {
        float t=Mathf.Clamp(shortToLong,.03f,1)/(1+2*Mathf.Clamp(shortToLong,.03f,1));
        Vector2 a=Vector2.right,b=new Vector2(-.5f,.866025404f),c=new Vector2(-.5f,-.866025404f);
        float scale=radius/Mathf.Sqrt(1-3*t+3*t*t);
        return new[]{Vector2.Lerp(a,c,t)*scale,Vector2.Lerp(a,b,t)*scale,Vector2.Lerp(b,a,t)*scale,
            Vector2.Lerp(b,c,t)*scale,Vector2.Lerp(c,b,t)*scale,Vector2.Lerp(c,a,t)*scale};
    }
    public void Draw(bool hex,float radius,float ratio,float charge,float energy,bool piston,float ring,float angle,Color color,bool body=true)
    {
        if(material==null)return;
        // Fit the artwork, not a fixed +/-2 quad: the existing hex.radius slider allows radius 3.
        // A tight transparent margin also avoids shading a large empty square for every small mobile node.
        float extent=(hex?radius*(1+.35f*ring):Mathf.Max(radius,(radius+.19f)*ring))+.20f;
        if(Mathf.Abs(extent-QuadExtent)>.0001f){
            QuadExtent=extent;quad[0]=new Vector3(-extent,-extent);quad[1]=new Vector3(extent,-extent);
            quad[2]=new Vector3(extent,extent);quad[3]=new Vector3(-extent,extent);
            mesh.vertices=quad;mesh.RecalculateBounds();
        }
        Vector2[] points=Hexagon(radius,ratio);for(int i=0;i<6;i++)polygon[i]=points[i];
        material.SetVectorArray("_Polygon",polygon);material.SetFloat("_Hex",hex?1:0);material.SetFloat("_Radius",radius);
        material.SetFloat("_Charge",Mathf.Clamp01(charge));material.SetFloat("_Energy",Energy=Mathf.Clamp01(energy));
        material.SetFloat("_FillScale",1);material.SetFloat("_ChargePulse",0);
        material.SetFloat("_Piston",piston?1:0);material.SetFloat("_Body",body?1:0);
        material.SetFloat("_Ring",ring);material.SetFloat("_Angle",angle*Mathf.Deg2Rad);
        material.SetFloat("_Stroke",CursorDashTuning.Number("visual.outline",.035f));
        CoreRadius=Mathf.Max(0,CursorDashTuning.Number("visual.core",0));material.SetFloat("_Core",CoreRadius);
        material.SetFloat("_Glow",CursorDashTuning.Number("visual.glow",0));
        material.SetColor("_Color",color);Visible=true;
    }
    public void DrawHexagonA(float radius,float ratio,float charge,float energy,bool piston,Color color,float morph)
    {
        // The separately owned original rings follow their own Rotators, not this aiming body's rotation.
        Draw(true,radius,ratio,charge,energy,piston,0,0,color);
        float t=Mathf.Clamp01(morph),stroke=CursorDashTuning.Number("visual.outline",.035f);
        material.SetFloat("_Stroke",Mathf.Lerp(HexagonalNodeRings.BodyStroke,.035f,t)*stroke/.035f);
        material.SetFloat("_FillScale",Mathf.Lerp(HexagonalNodeRings.FillRatio,1,t));
    }
    public void SetChargePulse(float pulse){if(material!=null)material.SetFloat("_ChargePulse",Mathf.Clamp(pulse,0,1.35f));}
    private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
}
