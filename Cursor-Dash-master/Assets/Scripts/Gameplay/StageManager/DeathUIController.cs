using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathUIController : MonoBehaviour
{
    [SerializeField] private Image[] uiImages;
    [SerializeField] private Text scoreText;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text summaryLabel;
    private Tween tween;
    public void UICanvasOn()
    {
        gameObject.SetActive(true);
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = group.blocksRaycasts = true;
            tween?.Kill();
            tween = group.DOFade(1f, 0.2f).SetUpdate(true);
        }
        else
        {
            if (uiImages != null) foreach (Image image in uiImages) if (image != null) image.DOFade(1f, 0.2f).SetUpdate(true);
            if (scoreText != null) scoreText.DOFade(1f, 0.2f).SetUpdate(true);
        }
    }
    public void UICanvasOff()
    {
        tween?.Kill();
        gameObject.SetActive(false);
    }
    public void SetScoreText(string value)
    {
        foreach(var caption in GetComponentsInChildren<TMP_Text>(true))if(caption.text=="생존 시간"||caption.text=="획득 점수")caption.text="이번 점수";
        if (scoreLabel != null) scoreLabel.text = value;
        if (scoreText != null) scoreText.text = value;
    }
    public void SetSummary(int circles, int beams)
    {
        if (summaryLabel != null) summaryLabel.text = $"원형 파괴 {circles}  ·  육각 차단 {beams}";
    }
    public void SetRunSummary(int circles,int hexes,int blocks,float survival)
    {
        if(summaryLabel!=null){
            string map=GameManager.instance.MapInformation.mapName;
            summaryLabel.text=$"최고 점수 {CursorDashPointsRecord.Read(map)}점\n생존 {survival:F2}초  ·  처치 {circles+hexes}  ·  방어 {blocks}";
            var stage=StageManager.instance;
            if(stage!=null&&!stage.RecordEligible)summaryLabel.text+="\n기록 제외: "+stage.RecordExclusionReason;
            else if(!string.IsNullOrEmpty(CursorDashPointsRecord.LastError))summaryLabel.text+="\n"+CursorDashPointsRecord.LastError;
            var size=summaryLabel.rectTransform.sizeDelta;size.y=Mathf.Max(size.y,summaryLabel.fontSize*2.6f);summaryLabel.rectTransform.sizeDelta=size;
            if(stage!=null&&!stage.RecordEligible){size.y=Mathf.Max(size.y,summaryLabel.fontSize*3.7f);summaryLabel.rectTransform.sizeDelta=size;}
        }
    }
    private void OnDisable() { tween?.Kill(); tween = null; }
}
