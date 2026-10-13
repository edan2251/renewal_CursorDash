using System.Collections;
using DG.Tweening;
using UnityEngine;

public class LaserNode : Node
{
    [SerializeField] private GameObject node;
    [SerializeField] private LaserArea[] areas;
    [SerializeField] private float duration = 2.6f;
    [SerializeField] private Transform inside;
    private Tweener tweener;

    public int Index { get; set; } = -1;
    private LaserArea Area => Index == -1 ? null : areas[Index];
    
    public override void Execute()
    {
        tweener?.Complete();
        gameObject.SetActive(true);
        node.SetActive(true);
        
        transform.position = Vector3.zero;
        node.transform.localPosition = Area.transform.localPosition;
        Area.gameObject.SetActive(true);
        Area.Renderer.color = LaserArea.HalfColor;
        ExecuteCoroutine().Start(this);
    }

    private IEnumerator ExecuteCoroutine()
    {
        yield return inside.DOScale(1f, duration).SetEase(Ease.Linear).WaitForCompletion();
        if (Area.IsPlayerWithin)
        {
            StageManager.instance.scoreManager.Hp = 0;
            GameManager.instance.playerData.AchieveRequire.AddValueToRequire("Razor_Death", 1);
        }
        
        GameManager.instance.soundManager.PlaySFX(ClipTag.laser);
        ResetObject();
    }

    public override void ResetObject()
    {
        base.ResetObject();
        gameObject.SetActive(true);
        node.SetActive(false);
        inside.transform.localScale = Vector3.zero;
        
        Area.Renderer.color = LaserArea.FullColor;
        tweener = Area.Renderer.DOFade(0f, 0.5f).SetEase(Ease.Linear);
        tweener.OnComplete(() => { Area.gameObject.SetActive(false); gameObject.SetActive(false); });
    }
}