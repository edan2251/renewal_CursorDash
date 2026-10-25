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

    private DropdownList<int> directions = new DropdownList<int>() {
        { "Left", 1 },
        { "Right", -1 }
    };

    private Tween rotationTween;
    private float activeTurnSeconds,activePauseSeconds;
    public bool HasRotation=>isActiveAndEnabled&&rotationTween!=null&&rotationTween.IsActive()&&activeTurnSeconds>0&&Mathf.Abs(angle)>.0001f;
    // Read the same tween that moves this transform, including its configured rest intervals.
    public double RotationTurns=>HasRotation?NodeRhythmMath.RotationTurns(rotationTween.Elapsed(true),activeTurnSeconds,activePauseSeconds):0;
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
        activeTurnSeconds=duration/multiplier;activePauseSeconds=Mathf.Max(0,interval);
        rotationTween = DOTween.Sequence().SetLoops(-1).Append(
                transform.DORotate(Vector3.forward * angle * direction, duration/multiplier)
               .SetRelative().SetEase(Ease.Linear))
               .AppendInterval(interval);
    }
    private void OnDisable() { rotationTween?.Kill(); rotationTween = null; }
    private void OnDestroy() { rotationTween?.Kill(); rotationTween = null; }
}
