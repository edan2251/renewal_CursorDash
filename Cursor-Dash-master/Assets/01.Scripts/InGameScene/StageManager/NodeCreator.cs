using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;

public class NodeCreator : MonoBehaviour
{   
    [Header("Objects")]
    [SerializeField]
    private GameObject nodesObject;

    [SerializeField]
    private Transform[] generatePositions;

    private List<Node> normalNodes = new List<Node>();
    private List<Node> arrowNodes = new List<Node>();

    private void Awake(){
        Node[] tempNodes;
    
        tempNodes = nodesObject.GetComponentsInChildren<NormalNode>(true);
        if(tempNodes.Length <= 0){
            tempNodes = nodesObject.GetComponentsInChildren<DefenceNode>(true);
        }
        normalNodes = tempNodes.ToList();
        
        tempNodes = nodesObject.GetComponentsInChildren<ArrowNode>(true);
        if(tempNodes.Length <= 0){
            tempNodes = nodesObject.GetComponentsInChildren<LaserNode>(true);
        }
        arrowNodes = tempNodes.ToList();
    }

    private void Start(){
        if(GameManager.instance.CurrentSceneType.Equals(GameManager.SceneType.NotInGame)){
            this.enabled = false;
        }
    }

    #if UNITY_EDITOR
    private void Update(){
        switch(Input.anyKeyDown){
            case var k when Input.GetKeyDown(KeyCode.K):
            NormalNodeGenerate();
            break; 

            case var k when Input.GetKeyDown(KeyCode.L):
            ArrowNodeGenerate();
            break; 
        }
    }
    #endif


    public void CreateNode(int index){
        Node node = null;
        switch(index){
            case 0:
                node = GetAvailableNode(normalNodes);
            break;

            case 1:
                node = GetAvailableNode(arrowNodes);
            break;
        }

        if(node is null){
            return;
        }

        node.transform.position = GetRandomPosition(Random.Range(0,4));
        node.Execute();

        // NodeCreate:index:int#position:Vector3]&time:float
        StageManager.instance.AddReplayData("NodeCreate:" + 
        index.ToString() + "#" + node.transform.position().ToString() + 
        "&" + StageManager.instance.scoreManager.TimeScore);
    }

    public void CreateNode(int index, int position) {
        Node node = null;
        switch(index){
            case 0:
                node = GetAvailableNode(normalNodes);
                break;

            case 1:
                node = GetAvailableNode(arrowNodes);
                break;
        }

        if(node is null){
            return;
        }


        if (node is LaserNode laserNode) {
             laserNode.Index = position;
        }
        else {
            node.transform.position = GetRandomPosition(Mathf.Clamp(position, 0, 3));
        }
        
        node.Execute();

        // NodeCreate:index:int#position:Vector3]&time:float
        StageManager.instance.AddReplayData("NodeCreate:" + 
                                            index.ToString() + "#" + node.transform.position().ToString() + 
                                            "&" + StageManager.instance.scoreManager.TimeScore);
    }

    [Button("Normal Node Generate")]
    public void NormalNodeGenerate(){
        CreateNode(0);
    }

    [Button("Arrow Node Generate")]
    public void ArrowNodeGenerate(){
        CreateNode(1);
    }

    private Vector2 GetRandomPosition(int index)
    {
        Debug.Log(index);
        return (Vector2)(generatePositions[index].position + Random.onUnitSphere);
    }

    private Node GetAvailableNode(List<Node> nodes){
        for(int i = 0; i < nodes.Count; i++){
            if(nodes[i].gameObject.activeInHierarchy.Equals(false)){
                return nodes[i];
            }
        }
        
        return null;
    }
}
