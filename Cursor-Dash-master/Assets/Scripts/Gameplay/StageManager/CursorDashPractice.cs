using UnityEngine;

/// <summary>Guidance only; all movement, collision, pooling and damage remain in the real game.</summary>
public sealed partial class CursorDashPractice : MonoBehaviour
{
    public int Step {get;private set;}
    public Node LastSpawned {get;private set;}
    public bool RepeatCircle,RepeatHex;
    public bool WaitingForAction {get;private set;}
    private StageManager stage;
    private Vector2 movementOrigin;
    private int circles,triangles;
    private float nextRepeat;
    public string Instruction=>Guided?GuideInstruction:Step==0?"화면을 눌러 커서를 이동하세요":Step==1?"원형 생성 버튼을 누른 뒤 충전 중 원형에 닿으세요":Step==2?"육각 생성 버튼을 누른 뒤 이동 잔상으로 레이저를 막으세요":"자유 훈련 — 실제 전투 규칙과 수치를 사용합니다";
    private void Start(){if(!guideInitialized)Initialize(GetComponent<StageManager>());}
    private void Update(){
        if(Guided){UpdateGuide();return;}
        if(stage==null||!stage.IsPlaying)return;
        if(Step==0&&Vector2.Distance(movementOrigin,PlayerCharacterController.instance.transform.position)>.4f)Step=1;
        if(Step==1&&WaitingForAction&&stage.DestroyedNodes>circles){Step=2;WaitingForAction=false;}
        if(Step==2&&WaitingForAction&&stage.HexDestroyed>triangles){Step=3;WaitingForAction=false;CursorDashSession.SetTutorialStatus("completed");CursorDashSession.CurrentMode=CursorDashSession.Mode.Training;}
        if(Step==3&&Time.time>=nextRepeat){nextRepeat=Time.time+CursorDashTuning.Number("practice.repeatInterval",2);if(RepeatCircle)Spawn(0);if(RepeatHex)Spawn(1);}
    }
    public Node Spawn(int kind){
        if(Guided)return SpawnGuide(kind);
        if(stage==null||stage.IsDeath||Step==0||(Step==1&&kind!=0)||(Step==2&&kind!=1))return null;
        Vector2 p=kind==0?(Vector2)PlayerCharacterController.instance.transform.position+Vector2.right*.9f:new Vector2(0,2.3f);
        LastSpawned=stage.NodeCreator.SpawnPreviewAt(kind,p);
        if(LastSpawned!=null){WaitingForAction=true;circles=stage.DestroyedNodes;triangles=stage.HexDestroyed;}
        return LastSpawned;
    }
    public void Skip(){if(!CursorDashTuning.DevEnabled)return;Cancel();Step=3;WaitingForAction=false;CursorDashSession.CurrentMode=CursorDashSession.Mode.Training;stage.Retry();}
    public void Clear(){stage.NodeCreator.StopNodes();PlayerCharacterAfterImage.instance?.ClearTrail();stage.Feedback.Clear();WaitingForAction=false;RepeatCircle=RepeatHex=false;}
}
