using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Uses the existing stable HUD canvas. Owns only score row layout, multiplier and pause-icon contrast.</summary>
public sealed class FeverScoreHud
{
    private readonly CursorDashFever fever;private readonly TMP_Text score;private readonly TextMeshProUGUI multiplier;
    private readonly RectTransform rect,parent;private readonly Canvas canvas;private readonly float fontSize;
    private readonly Vector2 anchorMin,anchorMax,pivot,position,size;private readonly Color tint;
    private readonly TextAlignmentOptions alignment;private readonly TextWrappingModes wrapping;
    private readonly List<(Graphic graphic,Color tint)> pause=new List<(Graphic,Color)>();
    private readonly Material scoreMaterial,multiplierMaterial;private readonly Material originalMaterial;private readonly TMP_FontAsset originalFont;
    private readonly Vector3[] corners=new Vector3[4];private int lastMultiplier=1;private float pulseAt=-100;
    private readonly bool bound;
    public FeverScoreHud(CursorDashFever owner,TMP_Text source)
    {
        fever=owner;score=source;if(source==null)return;
        rect=source.rectTransform;parent=rect.parent as RectTransform;canvas=source.canvas;
        if(parent==null||canvas==null)return;
        originalFont=source.font;originalMaterial=source.fontSharedMaterial;
        fontSize=source.fontSize;tint=source.color;anchorMin=rect.anchorMin;anchorMax=rect.anchorMax;pivot=rect.pivot;position=rect.anchoredPosition;size=rect.sizeDelta;alignment=source.alignment;wrapping=source.textWrappingMode;
        var go=new GameObject("Score multiplier x2-x4",typeof(RectTransform),typeof(TextMeshProUGUI)){layer=source.gameObject.layer,hideFlags=HideFlags.DontSave};go.transform.SetParent(parent,false);
        go.transform.SetSiblingIndex(source.transform.GetSiblingIndex()+1); // Keep existing modal/pause siblings above the score row.
        multiplier=go.GetComponent<TextMeshProUGUI>();
        // Each font owns its atlas UVs. A Bold glyph cannot sample the source Medium atlas.
        source.font=CursorDashTypography.Bold??source.font;multiplier.font=source.font;multiplier.fontStyle=FontStyles.Normal;
        multiplier.alignment=TextAlignmentOptions.MidlineLeft;multiplier.textWrappingMode=TextWrappingModes.NoWrap;multiplier.raycastTarget=false;
        multiplier.enableAutoSizing=false;multiplier.characterSpacing=0;multiplier.wordSpacing=0;multiplier.margin=Vector4.zero;
        source.textWrappingMode=TextWrappingModes.NoWrap;source.alignment=TextAlignmentOptions.MidlineRight;
        Material OwnedAtlas(TMP_Text text){
            if(text.font==null||text.font.material==null)return null;
            var owned=new Material(text.font.material){hideFlags=HideFlags.DontSave};owned.EnableKeyword("OUTLINE_ON");owned.SetFloat("_OutlineWidth",.06f);text.fontSharedMaterial=owned;return owned;
        }
        scoreMaterial=OwnedAtlas(source);multiplierMaterial=OwnedAtlas(multiplier);
        foreach(var button in parent.GetComponentsInChildren<Button>(true))if(button.name=="PauseButton")foreach(var graphic in button.GetComponentsInChildren<Graphic>(true))pause.Add((graphic,graphic.color));
        bound=true;
    }
    private float White(RectTransform target)
    {
        if(fever.World==null||target==null)return 0;target.GetWorldCorners(corners);float white=0;
        Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        foreach(var point in corners){Vector2 pixel=RectTransformUtility.WorldToScreenPoint(camera,point);white=Mathf.Max(white,fever.World.WhiteAtViewport(new Vector2(pixel.x/Screen.width,pixel.y/Screen.height)));}
        return white;
    }
    public void Draw()
    {
        if(!bound||score==null||multiplier==null||parent==null)return;
        // Restore() is reversible for disable/Home. Rebind on re-enable and after the typography scene scan.
        if(score.font!=multiplier.font)score.font=multiplier.font;
        if(scoreMaterial!=null&&score.fontSharedMaterial!=scoreMaterial)score.fontSharedMaterial=scoreMaterial;
        if(multiplierMaterial!=null&&multiplier.fontSharedMaterial!=multiplierMaterial)multiplier.fontSharedMaterial=multiplierMaterial;
        var model=fever.Model;int value=model.Multiplier;
        if(value>lastMultiplier)pulseAt=fever.Clock;lastMultiplier=value;
        bool shown=value>1&&model.Current!=FeverState.Phase.Stopped;multiplier.gameObject.SetActive(shown);multiplier.text=shown?"×"+value:"";
        float factor=.60f,pulse=1+.16f*Mathf.Sin(Mathf.Clamp01((fever.Clock-pulseAt)/.20f)*Mathf.PI);
        Vector2 safeMin,safeMax;var safe=Screen.safeArea;Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,safe.min,camera,out safeMin);RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,safe.max,camera,out safeMax);
        float available=Mathf.Max(1,(safeMax.x-safeMin.x)*.68f),gap=shown?fontSize*.22f:0;
        float scoreWidth=score.GetPreferredValues(model.PointsText).x*Mathf.Max(.001f,fontSize)/Mathf.Max(.001f,score.fontSize);
        multiplier.fontSize=fontSize*factor;float multWidth=shown?multiplier.GetPreferredValues(multiplier.text).x*1.17f:0;
        float fit=(float)FeverFlowMath.ScoreFit(available,scoreWidth,multWidth,gap),total=(scoreWidth+multWidth+gap)*fit;
        float center=(safeMin.x+safeMax.x)*.5f-parent.rect.center.x,y=safeMax.y-parent.rect.center.y-Mathf.Max(fontSize*.85f,(safeMax.y-safeMin.y)*.055f);
        rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.pivot=new Vector2(1,.5f);rect.anchoredPosition=new Vector2(center-total*.5f+scoreWidth*fit,y);rect.sizeDelta=new Vector2(scoreWidth*fit+2,fontSize*1.6f);score.fontSize=fontSize*fit;
        var m=multiplier.rectTransform;m.anchorMin=m.anchorMax=Vector2.one*.5f;m.pivot=new Vector2(0,.5f);m.anchoredPosition=new Vector2(rect.anchoredPosition.x+gap*fit,y);m.sizeDelta=new Vector2(multWidth*fit+2,fontSize*1.6f);multiplier.fontSize=fontSize*factor*fit;m.localScale=Vector3.one*pulse;
        float white=Mathf.Max(White(rect),shown?White(m):0);Color ink=new Color(.008f,.010f,.014f,tint.a);score.color=multiplier.color=Color.Lerp(tint,ink,white);
        if(scoreMaterial!=null)scoreMaterial.SetColor("_OutlineColor",Color.Lerp(Color.black,Color.white,white));
        if(multiplierMaterial!=null)multiplierMaterial.SetColor("_OutlineColor",Color.Lerp(Color.black,Color.white,white));
        foreach(var item in pause)if(item.graphic!=null)item.graphic.color=Color.Lerp(item.tint,new Color(.008f,.010f,.014f,item.tint.a),White(item.graphic.rectTransform));
    }
    public string AtlasStatus
    {
        get {
            bool Matches(TMP_Text text)=>text!=null&&text.font!=null&&text.fontSharedMaterial!=null&&text.fontSharedMaterial.mainTexture==text.font.atlasTexture;
            return "점수 "+(Matches(score)?"일치":"확인 필요")+" / 배율 "+(Matches(multiplier)?"일치":"확인 필요");
        }
    }
    public bool BlocksSpawn(Vector2 world,float radius,Camera board)
    {
        if(!bound||board==null)return false;
        Vector2 pixel=board.WorldToScreenPoint(world),edge=board.WorldToScreenPoint(world+Vector2.right*radius);
        float pad=Vector2.Distance(pixel,edge)+8;
        bool Covers(RectTransform target){
            target.GetWorldCorners(corners);Camera ui=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            Vector2 lo=new Vector2(float.MaxValue,float.MaxValue),hi=new Vector2(float.MinValue,float.MinValue);
            foreach(var c in corners){var q=RectTransformUtility.WorldToScreenPoint(ui,c);lo=Vector2.Min(lo,q);hi=Vector2.Max(hi,q);}
            return Rect.MinMaxRect(lo.x-pad,lo.y-pad,hi.x+pad,hi.y+pad).Contains(pixel);
        }
        if(Covers(rect)||(multiplier.gameObject.activeSelf&&Covers(multiplier.rectTransform)))return true;
        foreach(var item in pause)if(item.graphic!=null&&Covers(item.graphic.rectTransform))return true;
        return false;
    }
    public void Restore()
    {
        if(!bound)return;
        if(multiplier!=null)multiplier.gameObject.SetActive(false);
        if(score!=null){rect.anchorMin=anchorMin;rect.anchorMax=anchorMax;rect.pivot=pivot;rect.anchoredPosition=position;rect.sizeDelta=size;score.fontSize=fontSize;score.color=tint;score.alignment=alignment;score.textWrappingMode=wrapping;score.font=originalFont;if(originalMaterial!=null)score.fontSharedMaterial=originalMaterial;}
        foreach(var item in pause)if(item.graphic!=null)item.graphic.color=item.tint;
    }
    public void Dispose(){Restore();if(multiplier!=null)Object.Destroy(multiplier.gameObject);if(scoreMaterial!=null)Object.Destroy(scoreMaterial);if(multiplierMaterial!=null)Object.Destroy(multiplierMaterial);}
}
