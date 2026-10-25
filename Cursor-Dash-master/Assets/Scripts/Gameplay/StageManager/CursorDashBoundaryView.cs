using UnityEngine;

/// <summary>Developer-only view of the actual stable gameplay circle; never adds a collider.</summary>
public sealed class CursorDashBoundaryView : MonoBehaviour
{
    private StageManager stage;
    private LineRenderer line;
    private Material material;
    private void Awake(){stage=GetComponent<StageManager>();var item=new GameObject("Measured nexus boundary"){hideFlags=HideFlags.DontSave,layer=0};item.transform.SetParent(transform,false);line=item.AddComponent<LineRenderer>();material=new Material(Shader.Find("Sprites/Default")){hideFlags=HideFlags.DontSave};line.sharedMaterial=material;line.useWorldSpace=true;line.loop=false;line.positionCount=65;line.widthMultiplier=.006f;line.startColor=line.endColor=new Color(1,.9f,.25f,.9f);line.sortingOrder=1000;line.enabled=false;}
    private void LateUpdate(){line.enabled=CursorDashTuning.DevEnabled&&CursorDashTuning.Toggle("debug.boundary",false);if(!line.enabled)return;for(int i=0;i<65;i++){float a=i*Mathf.PI*2/64;line.SetPosition(i,(Vector3)stage.NexusPosition+new Vector3(Mathf.Cos(a),Mathf.Sin(a))*stage.NexusRadius);}}
    private void OnDestroy(){if(material!=null)Destroy(material);}
}
