using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class NodeCreator : MonoBehaviour
{
    [SerializeField] private GameObject nodesObject;
    [SerializeField] private Transform[] generatePositions;
    [SerializeField, Min(0f)] private float spawnJitter = 0.35f;
    private Node[] normalNodes;
    private Node[] arrowNodes;
    private Node[] allNodes;
    private Vector2 minimum;
    private Vector2 maximum;
    private int activeLimit;
    private System.Random seededRandom;
    private sealed class Departure
    {
        public GameObject root;public Vector3 scale;public float age,seconds;
        public readonly List<Mesh> meshes=new List<Mesh>();public readonly List<Material> materials=new List<Material>();
    }
    private readonly List<Departure> departures=new List<Departure>();
    public int DepartingArtworkCount=>departures.Count;
    public int ActiveCount {get{int count=0;if(allNodes!=null)foreach(var n in allNodes)if(n.gameObject.activeSelf)count++;return count;}}
    private void Awake()
    {
        normalNodes = nodesObject.GetComponentsInChildren<NormalNode>(true);
        if (normalNodes.Length == 0) normalNodes = nodesObject.GetComponentsInChildren<DefenceNode>(true);
        arrowNodes = nodesObject.GetComponentsInChildren<ArrowNode>(true);
        if (arrowNodes.Length == 0) arrowNodes = nodesObject.GetComponentsInChildren<LaserNode>(true);
        allNodes = nodesObject.GetComponentsInChildren<Node>(true);
        spawnJitter=CursorDashTuning.Number("spawn.jitter",spawnJitter);
        activeLimit=CursorDashTuning.Integer("spawn.limit",128);
        if(CursorDashTuning.Toggle("spawn.seedEnabled",false))seededRandom=new System.Random(CursorDashTuning.Integer("spawn.seed",1234));
        Camera camera = Camera.main;
        minimum = camera.ViewportToWorldPoint(new Vector3(0.12f, 0.12f, 0f));
        maximum = camera.ViewportToWorldPoint(new Vector3(0.88f, 0.88f, 0f));
    }
    public void CreateNode(int index)
    {
        if (generatePositions == null || generatePositions.Length == 0) return;
        CreateNode(index, seededRandom==null?Random.Range(0,generatePositions.Length):seededRandom.Next(generatePositions.Length));
    }
    public void CreateNode(int index, int position)
    {
        if (StageManager.instance == null || !StageManager.instance.IsPlaying||StageManager.instance.NormalThreatsSuppressed) return;
        if(ActiveCount>=activeLimit)return;
        Node node = GetAvailable(index == 0 ? normalNodes : index == 1 ? arrowNodes : null);
        if (node == null || generatePositions == null || generatePositions.Length == 0) return;
        position = Mathf.Clamp(position, 0, generatePositions.Length - 1);
        if (node is LaserNode laser) laser.Index = position;
        else
        {
            Vector2 jitter=seededRandom==null?Random.insideUnitCircle:new Vector2((float)seededRandom.NextDouble()*2-1,(float)seededRandom.NextDouble()*2-1).normalized*Mathf.Sqrt((float)seededRandom.NextDouble());
            Vector2 candidate = (Vector2)generatePositions[position].position + jitter * spawnJitter;
            node.transform.position = PlayerCharacterController.ClampPosition(candidate, minimum, maximum);
        }
        node.Execute();
    }
    public Node SpawnPreviewAt(int index,Vector2 position)
    {
        if(StageManager.instance==null||StageManager.instance.IsDeath||StageManager.instance.NormalThreatsSuppressed||(!CursorDashSession.IsPractice&&!CursorDashTuning.DevEnabled)||ActiveCount>=activeLimit)return null;
        Node node=GetAvailable(index==0?normalNodes:index==1?arrowNodes:null);
        if(node==null)return null;
        node.transform.position=PlayerCharacterController.ClampPosition(position,minimum,maximum);node.Execute();return node;
    }
    private static Node GetAvailable(Node[] nodes)
    {
        if (nodes != null) foreach (Node node in nodes) if (!node.gameObject.activeSelf) return node;
        return null;
    }
    public void StopNodes(){ClearDepartures();ResetNodes();}
    public void ClearFeverDepartures()=>ClearDepartures();
    private void ResetNodes(){if(allNodes!=null)foreach(var node in allNodes)if(node.gameObject.activeSelf)node.ResetObject();}
    public void RetireForFever(float seconds)
    {
        ClearDepartures();
        try{
            if(allNodes!=null)foreach(var node in allNodes)if(node.gameObject.activeSelf)CaptureDeparture(node,Mathf.Max(.001f,seconds));
        }finally{
            // Threats and beams stop now, even if a visual copy fails. No kill/damage/reward callback.
            ResetNodes();
        }
    }
    private void CaptureDeparture(Node node,float seconds)
    {
        var item=new Departure{root=new GameObject("Fever departing node artwork"){hideFlags=HideFlags.DontSave},seconds=seconds};
        item.root.transform.SetParent(transform,true);item.root.transform.SetPositionAndRotation(node.transform.position,Quaternion.identity);item.scale=item.root.transform.localScale;departures.Add(item);
        foreach(var source in node.GetComponentsInChildren<Renderer>(true)){
            if(!source.enabled||source.forceRenderingOff||!source.gameObject.activeInHierarchy)continue;
            // Only body artwork; never clone a collider, behaviour, Animator, particle system or live laser.
            var filter=source as MeshRenderer;var sprite=source as SpriteRenderer;
            if(filter==null&&(sprite==null||sprite.sprite==null||sprite.color.a<=0))continue;
            var meshFilter=filter==null?null:filter.GetComponent<MeshFilter>();if(filter!=null&&(meshFilter==null||meshFilter.sharedMesh==null))continue;
            var piece=new GameObject(source.name+" departing"){hideFlags=HideFlags.DontSave,layer=source.gameObject.layer};piece.transform.SetParent(item.root.transform,false);
            piece.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);piece.transform.localScale=source.transform.lossyScale;
            Renderer copy;
            if(filter!=null){
                var mesh=Object.Instantiate(meshFilter.sharedMesh);mesh.hideFlags=HideFlags.DontSave;item.meshes.Add(mesh);
                piece.AddComponent<MeshFilter>().sharedMesh=mesh;copy=piece.AddComponent<MeshRenderer>();
            }else{
                var view=piece.AddComponent<SpriteRenderer>();view.sprite=sprite.sprite;view.color=sprite.color;view.flipX=sprite.flipX;view.flipY=sprite.flipY;view.drawMode=sprite.drawMode;view.size=sprite.size;copy=view;
            }
            var original=source.sharedMaterials;var owned=new Material[original.Length];
            for(int i=0;i<owned.Length;i++)if(original[i]!=null){owned[i]=new Material(original[i]){hideFlags=HideFlags.DontSave};item.materials.Add(owned[i]);}
            copy.sharedMaterials=owned;copy.sortingLayerID=source.sortingLayerID;copy.sortingOrder=source.sortingOrder;
            var properties=new MaterialPropertyBlock();source.GetPropertyBlock(properties);copy.SetPropertyBlock(properties);
        }
    }
    private void Update()
    {
        var stage=StageManager.instance;if(stage==null||!stage.IsPlaying||Time.deltaTime<=0)return;
        for(int i=departures.Count-1;i>=0;i--){var item=departures[i];item.age+=Time.deltaTime;
            if(item.age>=item.seconds){Release(item);departures.RemoveAt(i);continue;}
            item.root.transform.localScale=item.scale*(float)NodeRhythmMath.Departure(item.age,item.seconds);
        }
    }
    private static void Release(Departure item)
    {
        if(item.root!=null)Object.Destroy(item.root);foreach(var mesh in item.meshes)if(mesh!=null)Object.Destroy(mesh);foreach(var material in item.materials)if(material!=null)Object.Destroy(material);
    }
    private void ClearDepartures(){foreach(var item in departures)Release(item);departures.Clear();}
    private void OnDisable()=>ClearDepartures();
    private void OnDestroy()=>ClearDepartures();
    [Button("Normal Node Generate")] public void NormalNodeGenerate() => CreateNode(0);
    [Button("Arrow Node Generate")] public void ArrowNodeGenerate() => CreateNode(1);
}
