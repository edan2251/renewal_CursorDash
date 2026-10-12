using UnityEngine;
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
    private void Awake()
    {
        normalNodes = nodesObject.GetComponentsInChildren<NormalNode>(true);
        if (normalNodes.Length == 0) normalNodes = nodesObject.GetComponentsInChildren<DefenceNode>(true);
        arrowNodes = nodesObject.GetComponentsInChildren<ArrowNode>(true);
        if (arrowNodes.Length == 0) arrowNodes = nodesObject.GetComponentsInChildren<LaserNode>(true);
        allNodes = nodesObject.GetComponentsInChildren<Node>(true);
        Camera camera = Camera.main;
        minimum = camera.ViewportToWorldPoint(new Vector3(0.12f, 0.12f, 0f));
        maximum = camera.ViewportToWorldPoint(new Vector3(0.88f, 0.88f, 0f));
    }
    public void CreateNode(int index)
    {
        if (generatePositions == null || generatePositions.Length == 0) return;
        CreateNode(index, Random.Range(0, generatePositions.Length));
    }
    public void CreateNode(int index, int position)
    {
        if (StageManager.instance == null || !StageManager.instance.IsPlaying) return;
        Node node = GetAvailable(index == 0 ? normalNodes : index == 1 ? arrowNodes : null);
        if (node == null || generatePositions == null || generatePositions.Length == 0) return;
        position = Mathf.Clamp(position, 0, generatePositions.Length - 1);
        if (node is LaserNode laser) laser.Index = position;
        else
        {
            Vector2 candidate = (Vector2)generatePositions[position].position + Random.insideUnitCircle * spawnJitter;
            node.transform.position = PlayerCharacterController.ClampPosition(candidate, minimum, maximum);
        }
        node.Execute();
    }
    private static Node GetAvailable(Node[] nodes)
    {
        if (nodes != null) foreach (Node node in nodes) if (!node.gameObject.activeSelf) return node;
        return null;
    }
    public void StopNodes()
    {
        if (allNodes != null) foreach (Node node in allNodes) if (node.gameObject.activeSelf) node.ResetObject();
    }
    [Button("Normal Node Generate")] public void NormalNodeGenerate() => CreateNode(0);
    [Button("Arrow Node Generate")] public void ArrowNodeGenerate() => CreateNode(1);
}
