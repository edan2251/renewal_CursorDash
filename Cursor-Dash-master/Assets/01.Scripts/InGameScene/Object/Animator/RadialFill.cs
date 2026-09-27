using DG.Tweening;
using UnityEngine;
using EventTools.Event;

public class RadialFill : MonoBehaviour
{
    [SerializeField] private Renderer target;
    [SerializeField] private float duration = 1f;
    [SerializeField] private UniEvent onComplete;
    private static readonly int Arc1 = Shader.PropertyToID("_Arc1");

    private void OnEnable()
    {
        target.material.SetFloat(Arc1, 0f);
        target.material.DOFloat(360f, Arc1, duration).SetEase(Ease.Linear).OnComplete(() => onComplete?.Invoke());
    }
}