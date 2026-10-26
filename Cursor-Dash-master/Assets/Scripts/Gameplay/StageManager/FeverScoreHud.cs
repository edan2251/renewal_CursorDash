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
    private readonly bool bound;private readonly bool scoreEnabledBefore,legacyEnabledBefore;private readonly Text legacyScore;
    public FeverScoreHud(CursorDashFever owner,TMP_Text source,Text legacy=null)
    {
        fever=owner;score=source;legacyScore=legacy;legacyEnabledBefore=legacy!=null&&legacy.enabled;if(legacy!=null)legacy.enabled=false;
        if(source==null)return;scoreEnabledBefore=source.enabled;source.enabled=false;
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
        if(legacyScore!=null)legacyScore.enabled=false;if(score!=null)score.enabled=false;
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
        float available=Mathf.Max(1,(safeMax.x-safeMin.x)*.68f),gap=0;
        float scoreWidth=0; // Hidden total reserves no width; the remaining multiplier stays centered.
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
        if(multiplier.gameObject.activeSelf&&Covers(multiplier.rectTransform))return true;
        foreach(var item in pause)if(item.graphic!=null&&Covers(item.graphic.rectTransform))return true;
        return false;
    }
    public void Restore()
    {
        if(legacyScore!=null)legacyScore.enabled=legacyEnabledBefore;if(score!=null)score.enabled=scoreEnabledBefore;
        if(!bound)return;
        if(multiplier!=null)multiplier.gameObject.SetActive(false);
        if(score!=null){rect.anchorMin=anchorMin;rect.anchorMax=anchorMax;rect.pivot=pivot;rect.anchoredPosition=position;rect.sizeDelta=size;score.fontSize=fontSize;score.color=tint;score.alignment=alignment;score.textWrappingMode=wrapping;score.font=originalFont;if(originalMaterial!=null)score.fontSharedMaterial=originalMaterial;}
        foreach(var item in pause)if(item.graphic!=null)item.graphic.color=item.tint;
    }
    public void Dispose(){Restore();if(multiplier!=null)Object.Destroy(multiplier.gameObject);if(scoreMaterial!=null)Object.Destroy(scoreMaterial);if(multiplierMaterial!=null)Object.Destroy(multiplierMaterial);}
}

/// <summary>Exact total projected into the existing world render/mask path. No Canvas, collider, reward or new manager.</summary>
public sealed class FeverBackgroundScore
{
    private readonly StageManager stage;
    private readonly CursorDashFever fever;private readonly Camera board;
    private readonly TextMeshPro text;private readonly Material material;private readonly GameObject root;
    private readonly ScoreProjectionPulse pulse=new ScoreProjectionPulse();private bool? visibleOverride;private float glyphScale=1,presentationScale=1,mainFontSize=3.5f,mainFit=1;
    private string preparedBest;private int preparedWidth,preparedHeight;private bool wasBest;
    public MeshRenderer View {get;}
    public bool Visible {get=>visibleOverride??(fever.Visuals==null||fever.Visuals.backgroundScoreEnabled);set{visibleOverride=value;if(!value)Clear();}}
    public string Text=>text==null?"":text.text;
    public double Pulse=>pulse.Value;
    public bool AtlasMatches=>text!=null&&text.font!=null&&material!=null&&material.mainTexture==text.font.atlasTexture;
    public FeverBackgroundScore(CursorDashFever owner,StageManager game,TMP_Text source)
    {
        stage=game;fever=owner;board=Camera.main;
        var font=CursorDashTypography.Bold??source?.font;if(board==null||font==null||font.material==null){Debug.LogWarning("CursorDash: projected score camera/font unavailable; stable HUD retained.");return;}
        root=new GameObject("Projected total score"){hideFlags=HideFlags.DontSave,layer=game.gameObject.layer};root.transform.SetParent(game.transform,false);
        text=root.AddComponent<TextMeshPro>();text.font=font;text.fontStyle=FontStyles.Normal;
        root.transform.localScale=Vector3.one;
        text.rectTransform.sizeDelta=new Vector2(4,4);text.rectTransform.pivot=Vector2.one*.5f;
        text.fontSize=3.5f;text.fontSizeMin=1;text.fontSizeMax=3.5f;text.enableAutoSizing=true;text.isOrthographic=board.orthographic;text.alignment=TextAlignmentOptions.Center;text.textWrappingMode=TextWrappingModes.NoWrap;text.overflowMode=TextOverflowModes.Overflow;
        text.margin=Vector4.zero;text.characterSpacing=0;text.wordSpacing=0;text.richText=false;text.raycastTarget=false;
        material=new Material(font.material){hideFlags=HideFlags.DontSave};material.mainTexture=font.atlasTexture;
        if(material.HasProperty("_OutlineWidth"))material.SetFloat("_OutlineWidth",0);
        if(material.HasProperty("_GlowPower"))material.SetFloat("_GlowPower",0);
        material.DisableKeyword("GLOW_ON");text.fontSharedMaterial=material;text.OnPreRenderText+=PulseGlyphs;
        View=text.GetComponent<MeshRenderer>();View.sortingOrder=-900; // Existing living color field=-1000; gameplay bodies and Nexus are later.
        text.text=owner.Model.PointsText;owner.Model.PointsAwarded+=OnAward;Draw();
    }
    private void OnAward(System.Numerics.BigInteger delta,System.Numerics.BigInteger total)
    {
        if(text==null)return;
        text.text=total.ToString(System.Globalization.CultureInfo.InvariantCulture); // Exact and immediate, independent of pulse grouping.
        if(Visible&&CursorDashTuning.Toggle("fx.enabled",true))pulse.Award(delta);
    }
    public void Advance(float delta)
    {
        if(!Visible||fever.Model.Current==FeverState.Phase.Stopped||!CursorDashTuning.Toggle("fx.enabled",true)){pulse.Clear();return;}
        pulse.Advance(delta,fever.Visuals==null?.32:fever.Visuals.backgroundScoreResponse);
    }
    private static float Value(float n,float fallback,float low,float high)=>float.IsNaN(n)||float.IsInfinity(n)?fallback:Mathf.Clamp(n,low,high);
    public void Draw()
    {
        if(text==null||View==null)return;
        var space=stage.Space;bool best=space!=null&&space.UsesBestProjection;
        bool show=(best||Visible)&&fever.isActiveAndEnabled&&fever.Model.Current!=FeverState.Phase.Stopped;View.enabled=show;if(!show){pulse.Clear();return;}
        if(text.fontSharedMaterial!=material)text.fontSharedMaterial=material;
        var v=fever.Visuals;float x=Value(v==null?.5f:v.backgroundScoreX,.5f,.15f,.85f),y=Value(v==null?.5f:v.backgroundScoreY,.5f,.15f,.85f);
        Vector2 destination=board.ViewportToWorldPoint(new Vector3(x,y,0));
        if(best){
            pulse.Clear();glyphScale=1;
            if(preparedBest!=space.BestText||preparedWidth!=Screen.width||preparedHeight!=Screen.height){
                preparedBest=space.BestText;preparedWidth=Screen.width;preparedHeight=Screen.height;
                presentationScale=1;text.enableAutoSizing=true;text.fontSize=3.5f;text.text=preparedBest;text.ForceMeshUpdate();mainFontSize=text.fontSize;
                var safe=Screen.safeArea;float left=board.ScreenToWorldPoint(new Vector3(safe.xMin+safe.width*.08f,safe.center.y,0)).x,right=board.ScreenToWorldPoint(new Vector3(safe.xMax-safe.width*.08f,safe.center.y,0)).x;
                float available=Mathf.Max(.1f,2*Mathf.Min(stage.NexusPosition.x-left,right-stage.NexusPosition.x));mainFit=Mathf.Min(1,available/Mathf.Max(.001f,text.textBounds.size.x));
            }
            // Freeze autosizing during digit changes. Grow the font smoothly only after display count reaches zero.
            float release=space.IsMenu?0:(float)CursorDashStartTimeline.Smooth((space.StartProgress-.80)/.18);
            text.enableAutoSizing=space.IsMenu;text.fontSize=Mathf.Lerp(mainFontSize,3.5f,release);text.text=space.DisplayedPoints;
            float travel=space.ScoreTravel;
            presentationScale=Mathf.Lerp(Value(v==null?.82f:v.spaceMainScoreScale,.82f,.65f,.95f),1,travel)*Mathf.Lerp(mainFit,1,release);
            root.transform.position=Vector2.Lerp(stage.NexusPosition+Vector2.up*(v==null?.28f:v.spaceMainScoreLift),destination,travel);
        }else{
            if(wasBest){text.enableAutoSizing=true;text.fontSize=3.5f;}
            if(text.text!=fever.Model.PointsText){text.text=fever.Model.PointsText;pulse.Clear();}
            presentationScale=1;glyphScale=(float)pulse.Scale(Value(v==null?.22f:v.backgroundScoreReaction,.22f,0,.4f),Value(v==null?1.22f:v.backgroundScoreScaleCap,1.22f,1,1.4f));root.transform.position=destination;
        }
        wasBest=best;root.transform.rotation=Quaternion.identity;
        root.transform.localScale=Vector3.one; // World-unit Rect/TMP controls are authoritative; no screen-fit transform.
        Color color=v==null?new Color(.36f,.76f,.84f):v.backgroundScoreTint;
        float rest=Value(v==null?.105f:v.backgroundScoreOpacity,.105f,0,.22f),boost=Value(v==null?.14f:v.backgroundScoreOpacityBoost,.14f,0,.3f),maximum=Value(v==null?.24f:v.backgroundScoreOpacityCap,.24f,.05f,.35f);
        boost*=PlayerPrefs.GetString("OnFlash","true")=="true"?Mathf.Clamp01(CursorDashTuning.Number("user.flash",1)):0;
        // The existing white-world alpha mask turns this low-alpha text into low-contrast grey per pixel.
        color.a=(float)pulse.Opacity(rest,boost,maximum);
        if(best)color=Color.Lerp(new Color(1,1,1,Value(v==null?.85f:v.spaceMainScoreOpacity,.85f,.4f,1)),color,space.ScoreTravel);
        text.color=color;
        text.ForceMeshUpdate(); // Fresh base geometry each frame prevents vertex-pulse accumulation.
    }
    private void PulseGlyphs(TMP_TextInfo info)
    {
        float scale=glyphScale*presentationScale;if(Mathf.Abs(scale-1)<.00001f||info.meshInfo==null)return;
        Vector3 center=text.rectTransform.rect.center;
        foreach(var part in info.meshInfo){var vertices=part.vertices;if(vertices==null)continue;
            for(int i=0;i<part.vertexCount&&i<vertices.Length;i++)vertices[i]=center+(vertices[i]-center)*scale;}
    }
    public void Clear(){pulse.Clear();glyphScale=1;if(View!=null)View.enabled=false;}
    public void ResetRound(){visibleOverride=null;preparedBest=null;wasBest=false;presentationScale=1;Clear();}
    public void Dispose(){fever.Model.PointsAwarded-=OnAward;if(text!=null)text.OnPreRenderText-=PulseGlyphs;Clear();if(root!=null)Object.Destroy(root);if(material!=null)Object.Destroy(material);}
}
