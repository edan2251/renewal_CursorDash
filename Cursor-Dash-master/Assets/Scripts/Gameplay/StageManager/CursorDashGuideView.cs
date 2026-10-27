using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Non-interactive views under the existing HUD. No buttons, raycaster or gameplay state.</summary>
public sealed class CursorDashGuideView
{
    private readonly GameObject root,hintRoot,noticeRoot;private readonly TextMeshProUGUI hint,notice;private readonly Image white;
    private readonly bool bound;
    public bool Bound=>bound&&root!=null;
    public CursorDashGuideView(Transform parent)
    {
        root=new GameObject("In-world guidance",typeof(RectTransform)){hideFlags=HideFlags.DontSave};
        var rect=root.GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        bound=parent!=null&&parent.GetComponentInParent<Canvas>()!=null;
        var pause=parent==null?null:parent.Find("PauseButton");
        if(pause!=null)root.transform.SetSiblingIndex(pause.GetSiblingIndex());else root.transform.SetAsLastSibling();
        // Background artwork is below; existing Pause/Retry/Home remain above the opaque notice.
        noticeRoot=new GameObject("First fever white notice",typeof(RectTransform));noticeRoot.transform.SetParent(root.transform,false);
        var r=noticeRoot.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
        white=noticeRoot.AddComponent<Image>();white.color=Color.white;white.raycastTarget=false;
        notice=Label("First fever exact wording",noticeRoot.transform,60,new Vector2(.5f,.5f),new Vector2(960,300));
        notice.text="- FEVER TIME -\n검은 노드를 마구 파괴하세요!";notice.color=Color.black;
        hintRoot=new GameObject("Practice action hint",typeof(RectTransform));hintRoot.transform.SetParent(root.transform,false);
        hint=hintRoot.AddComponent<TextMeshProUGUI>();Configure(hint,34);
        var h=hint.rectTransform;h.anchorMin=h.anchorMax=new Vector2(.5f,.13f);h.sizeDelta=new Vector2(960,90);hint.color=Color.white;
        ShowNotice(0);ShowHint("",false);
    }
    private static TextMeshProUGUI Label(string name,Transform parent,float size,Vector2 anchor,Vector2 dimensions)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var text=go.AddComponent<TextMeshProUGUI>();Configure(text,size);
        var r=text.rectTransform;r.anchorMin=r.anchorMax=anchor;r.sizeDelta=dimensions;return text;
    }
    private static void Configure(TextMeshProUGUI text,float size){text.font=CursorDashTypography.Medium;text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=size*.65f;text.fontSizeMax=size;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;}
    public void ShowHint(string value,bool visible){if(hintRoot==null||hint==null)return;hintRoot.SetActive(visible&&Bound);if(hint.text!=value)hint.text=value;}
    public bool BlocksSpawn(Vector2 point,float radius,Camera board)
    {
        if(hintRoot==null||!hintRoot.activeInHierarchy||board==null)return false;
        var canvas=hint.canvas;var ui=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        Vector2 pixel=board.WorldToScreenPoint(point);
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(hint.rectTransform,pixel,ui,out var local))return false;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(hint.rectTransform,board.WorldToScreenPoint(point+Vector2.right*radius),ui,out var edge);
        float padding=Vector2.Distance(local,edge);
        return new Rect(hint.rectTransform.rect.position-Vector2.one*padding,hint.rectTransform.rect.size+Vector2.one*padding*2).Contains(local);
    }
    public void ShowNotice(float alpha)
    {
        if(noticeRoot==null||white==null||notice==null)return;
        // The completed fever world supplies white. Fade only wording so pre-supplied nodes remain visible.
        noticeRoot.SetActive(alpha>0&&Bound);white.color=Color.clear;notice.color=new Color(0,0,0,Mathf.Clamp01(alpha));
    }
    public void Clear(){ShowHint("",false);ShowNotice(0);}
    public void Dispose(){if(root!=null)Object.Destroy(root);}
}
