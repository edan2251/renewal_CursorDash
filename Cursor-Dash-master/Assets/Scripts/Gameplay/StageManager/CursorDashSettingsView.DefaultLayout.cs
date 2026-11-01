using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CursorDashSettingsView
{
    // Used once to author the prefab and only as compatibility for scenes without a saved view.
    public static CursorDashSettingsView CreateDefault(Transform parent,TMP_FontAsset font)
    {
        return new DefaultLayout(font).Build(parent);
    }
    private sealed class DefaultLayout
    {
        private readonly TMP_FontAsset font;
        private readonly List<Row> rows=new List<Row>();
        private RectTransform root,safe,sheetRect,content,pauseCard,handle,contentPlacement,pausePlacement;
        private CanvasGroup sheetContent,pauseGroup;private Image shade;private GameObject pauseRoot,sheetRoot;
        private Button replayTutorial,resetSettings,resume,retry,home;
        private static readonly Color Ink=new Color(.92f,.96f,.98f),Muted=new Color(.57f,.66f,.73f),Accent=new Color(.24f,.88f,.81f);
        public DefaultLayout(TMP_FontAsset font){this.font=font;}
    private RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 size,Vector2 position)
    {
        var go=new GameObject(name,typeof(RectTransform)){hideFlags=HideFlags.DontSave};go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.pivot=Vector2.one*.5f;rect.sizeDelta=size;rect.anchoredPosition=position;return rect;
    }
    private Image Image(RectTransform rect,Color tint){var image=rect.gameObject.AddComponent<Image>();image.color=tint;image.raycastTarget=false;return image;}
    private TMP_Text Label(string name,Transform parent,string text,float size,Vector2 dimensions,Vector2 pos,Color tint,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
    {
        var rect=Rect(name,parent,new Vector2(.5f,1),dimensions,pos);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font=font;label.fontSize=size;label.color=tint;label.text=text;label.alignment=alignment;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.NoWrap;return label;
    }
    private Button Button(string name,Transform parent,string text,Vector2 size,Vector2 pos,float font=23)
    {
        var rect=Rect(name,parent,new Vector2(.5f,1),size,pos);var hit=Image(rect,Color.clear);hit.raycastTarget=true;
        var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=hit;
        var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,1,1,.12f);colors.pressedColor=new Color(.25f,.9f,.8f,.2f);button.colors=colors;
        var label=Label(name+" text",rect,text,font,size,new Vector2(0,-size.y*.5f),Ink,TextAlignmentOptions.Center);button.targetGraphic=label;
        colors.normalColor=Ink;colors.highlightedColor=Accent;colors.pressedColor=Color.white;button.colors=colors;
        return button;
    }
    public CursorDashSettingsView Build(Transform parent)
    {
        root=Rect("CursorDash settings and pause",parent,Vector2.one*.5f,Vector2.zero,Vector2.zero);
        var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=90;
        var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(540,960);scaler.matchWidthOrHeight=1;
        root.gameObject.AddComponent<GraphicRaycaster>();
        safe=Rect("Safe area",root,Vector2.one*.5f,Vector2.zero,Vector2.zero);
        var dim=Rect("Menu shade",root,Vector2.one*.5f,Vector2.zero,Vector2.zero);dim.anchorMin=Vector2.zero;dim.anchorMax=Vector2.one;
        shade=Image(dim,new Color(.01f,.02f,.045f,1));
        // The shade must intercept the board, never the controls inside Safe area.
        // Safe area was created first; sibling order also determines UI raycast priority.
        dim.SetAsFirstSibling();
        sheetRect=Rect("Main bottom sheet",safe,new Vector2(.5f,0),Vector2.zero,Vector2.zero);sheetRect.pivot=new Vector2(.5f,0);sheetRoot=sheetRect.gameObject;
        Image(sheetRect,new Color(.022f,.035f,.066f,.985f)).raycastTarget=true;
        handle=Rect("Sheet handle touch area",sheetRect,new Vector2(.5f,1),new Vector2(180,48),new Vector2(0,-24));
        Image(handle,Color.clear).raycastTarget=true;handle.gameObject.AddComponent<CursorDashSheetHandle>();
        Image(Rect("Handle",handle,Vector2.one*.5f,new Vector2(62,3),Vector2.zero),new Color(.7f,.83f,.86f,.75f));
        contentPlacement=Rect("Settings placement",sheetRect,new Vector2(.5f,1),new Vector2(460,790),new Vector2(0,-55));contentPlacement.pivot=new Vector2(.5f,1);
        content=Rect("Settings content",contentPlacement,Vector2.one*.5f,Vector2.zero,Vector2.zero);content.anchorMin=Vector2.zero;content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);
        sheetContent=content.gameObject.AddComponent<CanvasGroup>();
        Label("Title",content,"설정",30,new Vector2(460,46),new Vector2(0,-24),Ink);
        Label("Audio heading",content,"사운드",15,new Vector2(460,32),new Vector2(0,-85),Muted);
        Row(content,"user.master","전체 음량",-144);Row(content,"user.bgm","배경음",-223);Row(content,"user.sfx","효과음",-302);
        Label("Effects heading",content,"화면 효과",15,new Vector2(460,32),new Vector2(0,-374),Muted);
        Row(content,"user.vfxReduction","전체 VFX 완화",-433);Row(content,"user.shake","화면 흔들림",-512);Row(content,"user.flash","플래시",-591);Row(content,"user.distortion","왜곡·색수차",-670);
        replayTutorial=Button("Replay tutorial",content,"튜토리얼부터 시작",new Vector2(310,54),new Vector2(-50,-751));
        resetSettings=Button("Reset audio and VFX",content,"초기화",new Vector2(110,54),new Vector2(180,-751),17);
        pauseRoot=Rect("Pause overlay",safe,Vector2.one*.5f,Vector2.zero,Vector2.zero).gameObject;
        pauseGroup=pauseRoot.AddComponent<CanvasGroup>();
        pausePlacement=Rect("Pause placement",pauseRoot.transform,Vector2.one*.5f,new Vector2(460,530),Vector2.zero);
        pauseCard=Rect("Pause content",pausePlacement,Vector2.one*.5f,Vector2.zero,Vector2.zero);pauseCard.anchorMin=Vector2.zero;pauseCard.anchorMax=Vector2.one;
        Label("Pause title",pauseCard,"일시정지",25,new Vector2(460,55),new Vector2(0,-32),Muted,TextAlignmentOptions.Center);
        resume=Button("Continue",pauseCard,"계속하기",new Vector2(330,78),new Vector2(0,-132),35);
        Image(Rect("Continue accent",pauseCard,new Vector2(.5f,1),new Vector2(64,2),new Vector2(0,-185)),Accent);
        Row(pauseCard,"user.master","전체 음량",-264);Row(pauseCard,"user.vfxReduction","전체 VFX 완화",-367);
        retry=Button("Retry",pauseCard,"다시 시작",new Vector2(180,60),new Vector2(-115,-481),20);
        home=Button("Home",pauseCard,"메인으로",new Vector2(180,60),new Vector2(115,-481),20);
        pauseRoot.SetActive(false);
        var view=root.gameObject.AddComponent<CursorDashSettingsView>();
        view.canvas=root.GetComponent<Canvas>();view.safeArea=safe;view.shade=shade;view.sheetAnimation=sheetRect;
        view.contentPlacement=contentPlacement;view.contentAnimation=content;view.contentOpacity=sheetContent;view.handle=handle;
        view.pauseOpacity=pauseGroup;view.pausePlacement=pausePlacement;view.pauseAnimation=pauseCard;view.rows=rows.ToArray();
        view.replayTutorial=replayTutorial;view.resetSettings=resetSettings;view.resume=resume;view.retry=retry;view.home=home;
        return view;
    }
    private void Row(Transform parent,string key,string label,float y)
    {
        var rect=Rect(key,parent,new Vector2(.5f,1),new Vector2(460,74),new Vector2(0,y));
        Label(key+" label",rect,label,21,new Vector2(360,32),new Vector2(-50,-15),Ink);
        var number=Label(key+" value",rect,"",18,new Vector2(85,32),new Vector2(187,-15),Muted,TextAlignmentOptions.Right);
        var track=Rect(key+" slider",rect,new Vector2(.5f,1),new Vector2(460,48),new Vector2(0,-54));
        Image(track,Color.clear).raycastTarget=true;
        var slider=track.gameObject.AddComponent<CursorDashSettingSlider>();slider.minValue=0;slider.maxValue=1;
        Image(Rect("Track",track,Vector2.one*.5f,new Vector2(460,4),Vector2.zero),new Color(.36f,.47f,.54f,.82f));
        var fillArea=Rect("Fill area",track,Vector2.one*.5f,new Vector2(460,4),Vector2.zero);
        var fill=Rect("Fill",fillArea,Vector2.zero,Vector2.zero,Vector2.zero);Image(fill,Accent);slider.fillRect=fill;
        // Slider drives the handle's cross-axis anchors to 0..1. A zero-height
        // travel area keeps the visual handle 12x12 while the track remains a 48px hit area.
        var sliding=Rect("Handle area",track,Vector2.one*.5f,new Vector2(460,0),Vector2.zero);
        var knob=Rect("Handle",sliding,Vector2.one*.5f,new Vector2(12,12),Vector2.zero);var knobImage=Image(knob,Accent);slider.handleRect=knob;slider.targetGraphic=knobImage;
        rows.Add(new Row{key=key,slider=slider,value=number});
    }
    }
}
