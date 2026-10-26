using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

public class Rotator : MonoBehaviour {
    [SerializeField]
    private float angle = 360f;
    [SerializeField]
    private float duration = 1f;
    [SerializeField]
    private float interval;
    [Dropdown("directions")]
    [SerializeField]
    private int direction = 1;
    public int DirectionSign => direction;
    public float StepDegrees=>angle*direction;
    public float ConfiguredTurnSeconds=>duration;
    public float ConfiguredPauseSeconds=>Mathf.Max(0,interval);

    private DropdownList<int> directions = new DropdownList<int>() {
        { "Left", 1 },
        { "Right", -1 }
    };

    private Tween rotationTween;
    private float activeTurnSeconds,activePauseSeconds;
    public bool HasRotation=>isActiveAndEnabled&&rotationTween!=null&&rotationTween.IsActive()&&activeTurnSeconds>0&&Mathf.Abs(angle)>.0001f;
    // Read the same tween that moves this transform, including its configured rest intervals.
    public double RotationElapsedSeconds=>HasRotation?rotationTween.Elapsed(true):0;
    public double RotationTurns=>HasRotation?NodeRhythmMath.RotationTurns(RotationElapsedSeconds,activeTurnSeconds,activePauseSeconds):0;
    public float TurnSeconds=>activeTurnSeconds;
    public float PauseSeconds=>activePauseSeconds;
    private void OnEnable()=>RestartRotation();
    public void RestartRotation() {
        rotationTween?.Kill();rotationTween=null;activeTurnSeconds=activePauseSeconds=0;
        if(!isActiveAndEnabled)return;
        string key=GetComponentInParent<NormalNode>()!=null?"circle.spin":GetComponentInParent<ArrowNode>()!=null?"hex.spin":
            (gameObject.name.StartsWith("Hp")?"nexus.spin":null);
        float multiplier=key==null?1:CursorDashTuning.Number(key,1);
        if(multiplier<=0)return;
        activeTurnSeconds=duration/multiplier;
        // Hex timeline speed controls the whole beat. Other rotator users keep their existing rest behavior.
        activePauseSeconds=Mathf.Max(0,interval)/(key=="hex.spin"?multiplier:1);
        rotationTween = DOTween.Sequence().SetLoops(-1).Append(
                transform.DORotate(Vector3.forward * angle * direction, duration/multiplier)
               .SetRelative().SetEase(Ease.Linear))
               .AppendInterval(activePauseSeconds);
    }
    private void OnDisable() { rotationTween?.Kill(); rotationTween = null; }
    private void OnDestroy() { rotationTween?.Kill(); rotationTween = null; }
}
