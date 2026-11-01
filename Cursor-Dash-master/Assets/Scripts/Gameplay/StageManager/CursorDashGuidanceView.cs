using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene/prefab-owned static tutorial typography. No clocks, input, completion flags or game rules.</summary>
public sealed class CursorDashGuidanceView : MonoBehaviour
{
    [SerializeField] private GameObject hintRoot, noticeRoot;
    [SerializeField] private TextMeshProUGUI hint, notice;
    [SerializeField] private Image noticeBackground;
    [SerializeField] private CanvasGroup hintOpacity, noticeOpacity;
    [SerializeField] private string circularChecklist="원형노드 파괴 {0}/2";
    [SerializeField] private string hexChecklist="육각노드로부터 수비 {0}/2";
    [SerializeField] private string completedMarker="  [완료]";
    private CursorDashViewFonts fonts;
    public GameObject HintRoot=>hintRoot;
    public GameObject NoticeRoot=>noticeRoot;
    public TextMeshProUGUI Hint=>hint;
    public TextMeshProUGUI Notice=>notice;
    public Image NoticeBackground=>noticeBackground;
    public bool IsConfigured=>hintRoot!=null&&noticeRoot!=null&&hint!=null&&notice!=null&&hintOpacity!=null&&noticeOpacity!=null&&noticeBackground!=null;
    public void PrepareRendering(){if(fonts!=null)return;fonts=new CursorDashViewFonts();fonts.Bind(hint);fonts.Bind(notice);}
    public string Checklist(int circle,int hex)=>circularChecklist.Replace("{0}",circle.ToString())+(circle==2?completedMarker:"")+"\n"+hexChecklist.Replace("{0}",hex.ToString())+(hex==2?completedMarker:"");
    public void PresentHint(string value,bool visible){hint.text=value;hint.enabled=visible;hintOpacity.alpha=visible?1:0;hintRoot.SetActive(visible);}
    public void PresentNotice(float alpha){notice.enabled=alpha>0;noticeOpacity.alpha=Mathf.Clamp01(alpha);noticeRoot.SetActive(alpha>0);}
    public void Clear(){PresentHint("",false);PresentNotice(0);}
    private void OnDestroy(){fonts?.Dispose();fonts=null;}
}
