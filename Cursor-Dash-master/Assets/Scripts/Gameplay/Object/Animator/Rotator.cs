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

    private DropdownList<int> directions = new DropdownList<int>() {
        { "Left", 1 },
        { "Right", -1 }
    };

    private void Awake() {
        DOTween.Sequence().SetLoops(-1).Append(
                transform.DORotate(Vector3.forward * angle * direction, duration)
               .SetRelative().SetEase(Ease.Linear))
               .AppendInterval(interval);
    }
}
