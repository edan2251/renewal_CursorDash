using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CursorDashSettingsView
{
    /// <summary>Explicit one-time authoring upgrade. Existing sliders and Pause geometry are retained.</summary>
    public void AddBottomPanelsForAuthoring(TMP_FontAsset font)
    {
        if(navigationPlacement!=null)return;
        new MenuLayout(this,font).Build();
    }
    /// <summary>Explicit, idempotent authoring revision; never reapplies authored positions per frame.</summary>
    public void RefineBottomPanelsForAuthoring(){new MenuLayout(this,null).Refine();}
    private sealed class MenuLayout
    {
        private readonly CursorDashSettingsView view;
        private readonly TMP_FontAsset font;
        private readonly List<MenuText> texts=new List<MenuText>();
        private static readonly Color Ink=new Color(.92f,.96f,.98f),Muted=new Color(.57f,.66f,.73f),Accent=new Color(.24f,.88f,.81f),Panel=new Color(.022f,.035f,.066f,.985f);
        public MenuLayout(CursorDashSettingsView view,TMP_FontAsset font){this.view=view;this.font=font;}
        private RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 pos,Vector2? anchor=null)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=anchor??new Vector2(.5f,1);r.sizeDelta=size;r.anchoredPosition=pos;return r;
        }
        private Image Image(RectTransform rect,Color tint,bool hit=false){var image=rect.gameObject.AddComponent<Image>();image.color=tint;image.raycastTarget=hit;return image;}
        private CursorDashMenuIcon Icon(string name,Transform parent,CursorDashMenuIcon.Symbol kind,Vector2 size,Vector2 pos,Color tint)
        {
            var r=Rect(name,parent,size,pos,Vector2.one*.5f);var graphic=r.gameObject.AddComponent<CursorDashMenuIcon>();graphic.Kind=kind;graphic.color=tint;graphic.raycastTarget=false;return graphic;
        }
        private TMP_Text Label(string name,Transform parent,string ko,string en,string ja,string zh,float size,Vector2 dimensions,Vector2 pos,Color tint,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var r=Rect(name,parent,dimensions,pos);var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.fontSize=size;label.fontSizeMin=size*.7f;label.fontSizeMax=size;label.enableAutoSizing=true;
            label.color=tint;label.text=ko;label.alignment=align;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.NoWrap;
            texts.Add(new MenuText{label=label,english=en,japanese=ja,chinese=zh});return label;
        }
        private Button HitButton(string name,Transform parent,Vector2 size,Vector2 pos)
        {
            var r=Rect(name,parent,size,pos);var image=Image(r,Color.clear,true);var button=r.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var colors=button.colors;colors.normalColor=Color.clear;colors.highlightedColor=new Color(.24f,.88f,.81f,.08f);colors.pressedColor=new Color(.24f,.88f,.81f,.14f);button.colors=colors;return button;
        }
        private Button Reset(string name,Transform parent,float y)
        {
            var button=HitButton(name,parent,new Vector2(52,48),new Vector2(208,y));var icon=Icon("Reset icon",button.transform,CursorDashMenuIcon.Symbol.Reset,new Vector2(23,23),Vector2.zero,Muted);IconFeedback(button,icon);return button;
        }
        private static void IconFeedback(Button button,Graphic icon){button.targetGraphic=icon;var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Accent;colors.pressedColor=Color.white;colors.fadeDuration=.08f;button.colors=colors;}
        private RectTransform Tab(string name,bool left,MenuPanel panel,string ko,string en,string ja,string zh,CursorDashMenuIcon.Symbol icon,out CanvasGroup opacity)
        {
            var r=Rect(name,view.navigationPlacement,new Vector2(0,view.navigationHeight),Vector2.zero,Vector2.zero);r.anchorMin=new Vector2(left?0:.5f,0);r.anchorMax=new Vector2(left?.5f:1,0);r.pivot=new Vector2(.5f,0);
            Image(r,Panel,true);opacity=r.gameObject.AddComponent<CanvasGroup>();var grip=r.gameObject.AddComponent<CursorDashSheetHandle>();grip.panel=panel;
            Icon("Menu icon",r,icon,new Vector2(25,25),new Vector2(-64,0),Ink);
            Label("Menu title",r,ko,en,ja,zh,22,new Vector2(142,48),new Vector2(16,-view.navigationHeight*.5f),Ink,TextAlignmentOptions.Center);
            Icon("Close direction",r,CursorDashMenuIcon.Symbol.Down,new Vector2(15,15),new Vector2(111,0),Muted);
            return r;
        }
        private static void Remove(GameObject obj){if(obj==null)return;obj.SetActive(false);if(Application.isPlaying)Object.Destroy(obj);else Object.DestroyImmediate(obj);}
        private void Existing(string name,string en,string ja,string zh,float y)
        {
            var item=view.contentAnimation.Find(name);if(item==null)return;var label=item.GetComponent<TMP_Text>();if(label!=null)texts.Add(new MenuText{label=label,english=en,japanese=ja,chinese=zh});((RectTransform)item).anchoredPosition=new Vector2(((RectTransform)item).anchoredPosition.x,y);
        }
        public void Build()
        {
            Remove(view.handle.gameObject);var oldTitle=view.contentAnimation.Find("Title");if(oldTitle!=null)Remove(oldTitle.gameObject);if(view.resetSettings!=null)Remove(view.resetSettings.gameObject);view.resetSettings=null;
            view.navigationPlacement=Rect("Main navigation placement",view.safeArea,new Vector2(0,view.navigationHeight),Vector2.zero,Vector2.zero);
            view.navigationPlacement.anchorMin=Vector2.zero;view.navigationPlacement.anchorMax=new Vector2(1,0);view.navigationPlacement.pivot=new Vector2(.5f,0);
            view.rankingHeaderAnimation=Tab("Ranking header animation",true,MenuPanel.Ranking,"랭킹","Ranking","ランキング","排行榜",CursorDashMenuIcon.Symbol.Trophy,out view.rankingHeaderOpacity);
            view.settingsHeaderAnimation=Tab("Settings header animation",false,MenuPanel.Settings,"설정","Settings","設定","设置",CursorDashMenuIcon.Symbol.Settings,out view.settingsHeaderOpacity);view.handle=view.settingsHeaderAnimation;
            var divider=Rect("Menu divider",view.navigationPlacement,new Vector2(1,28),new Vector2(0,view.navigationHeight*.5f),new Vector2(.5f,0));Image(divider,new Color(.34f,.43f,.5f,.5f));view.navigationDivider=divider.gameObject.AddComponent<CanvasGroup>();
            view.contentPlacement.anchoredPosition=new Vector2(view.contentPlacement.anchoredPosition.x,-95);
            Existing("Audio heading","SOUND","サウンド","声音",-24);Existing("Effects heading","VISUAL EFFECTS","画面エフェクト","画面效果",-320);
            var labels=new[]{new[]{"Master volume","全体音量","总音量"},new[]{"Music","BGM","背景音乐"},new[]{"Sound effects","効果音","音效"},new[]{"Reduce VFX","エフェクト軽減","减弱特效"},new[]{"Screen shake","画面の揺れ","屏幕震动"},new[]{"Flash","フラッシュ","闪光"},new[]{"Distortion / RGB","歪み・色収差","扭曲・色差"}};
            float[] positions={-84,-163,-242,-380,-459,-538,-617};int n=0;
            foreach(var row in view.rows){if(!row.slider.transform.IsChildOf(view.contentAnimation))continue;var rowRoot=(RectTransform)row.slider.transform.parent;rowRoot.anchoredPosition=new Vector2(rowRoot.anchoredPosition.x,positions[n]);var label=rowRoot.Find(row.key+" label").GetComponent<TMP_Text>();texts.Add(new MenuText{label=label,english=labels[n][0],japanese=labels[n][1],chinese=labels[n][2]});n++;}
            view.resetAudio=Reset("Reset sound section",view.contentAnimation,-24);view.resetEffects=Reset("Reset effects section",view.contentAnimation,-320);
            var tutorial=(RectTransform)view.replayTutorial.transform;tutorial.sizeDelta=new Vector2(280,60);tutorial.anchoredPosition=new Vector2(-90,-742);
            var tutorialLabel=view.replayTutorial.GetComponentInChildren<TMP_Text>();tutorialLabel.text="튜토리얼 플레이";tutorialLabel.fontSize=20;tutorialLabel.enableAutoSizing=true;tutorialLabel.fontSizeMin=14;tutorialLabel.fontSizeMax=20;
            texts.Add(new MenuText{label=tutorialLabel,english="Play tutorial",japanese="チュートリアル",chinese="游玩教程"});
            view.languageButton=HitButton("Cycle menu language",view.contentAnimation,new Vector2(156,60),new Vector2(155,-742));
            var globe=Icon("Language globe",view.languageButton.transform,CursorDashMenuIcon.Symbol.Globe,new Vector2(22,22),new Vector2(-49,0),Muted);IconFeedback(view.languageButton,globe);
            view.languageFlag=Icon("Selected language flag",view.languageButton.transform,CursorDashMenuIcon.Symbol.Korea,new Vector2(35,35),new Vector2(-8,0),Color.white);
            view.languageLabel=Label("Language code",view.languageButton.transform,"KO","KO","KO","KO",16,new Vector2(40,40),new Vector2(43,-30),Ink,TextAlignmentOptions.Center);
            // This dynamic value is not translated together with the fixed menu copy.
            texts.RemoveAt(texts.Count-1);
            view.rankingPlacement=Rect("Ranking placement",view.sheetAnimation,new Vector2(460,790),new Vector2(0,-95));view.rankingPlacement.pivot=new Vector2(.5f,1);
            view.rankingAnimation=Rect("Ranking content",view.rankingPlacement,Vector2.zero,Vector2.zero,Vector2.zero);view.rankingAnimation.anchorMin=Vector2.zero;view.rankingAnimation.anchorMax=Vector2.one;view.rankingAnimation.pivot=new Vector2(.5f,1);
            view.rankingOpacity=view.rankingAnimation.gameObject.AddComponent<CanvasGroup>();view.rankingOpacity.alpha=0;view.rankingOpacity.blocksRaycasts=false;
            var rank=view.rankingAnimation;
            Label("Best score caption",rank,"내 최고 점수","PERSONAL BEST","自己ベスト","我的最高分",16,new Vector2(460,32),new Vector2(0,-42),Muted,TextAlignmentOptions.Center);
            view.rankingBest=Label("Local best score",rank,"0","0","0","0",76,new Vector2(440,100),new Vector2(0,-123),Ink,TextAlignmentOptions.Center);texts.RemoveAt(texts.Count-1);
            var bold=Resources.Load<TMP_FontAsset>("CursorDash/Fonts/esamanru Bold View SDF");if(bold!=null)view.rankingBest.font=bold;
            view.rankingBest.fontSizeMin=14;
            Image(Rect("Best score rule",rank,new Vector2(460,1),new Vector2(0,-214)),new Color(.34f,.43f,.5f,.35f));
            Label("Rank column",rank,"순위","RANK","順位","排名",13,new Vector2(80,36),new Vector2(-190,-253),Muted);
            Label("Player column",rank,"플레이어","PLAYER","プレイヤー","玩家",13,new Vector2(245,36),new Vector2(-26,-253),Muted);
            Label("Score column",rank,"점수","SCORE","スコア","分数",13,new Vector2(100,36),new Vector2(180,-253),Muted,TextAlignmentOptions.Right);
            Icon("Ranking empty state",rank,CursorDashMenuIcon.Symbol.Trophy,new Vector2(52,52),new Vector2(0,-16),new Color(.45f,.57f,.63f,.7f));
            Label("Ranking status",rank,"온라인 랭킹 준비 중","Online rankings coming soon","オンラインランキング準備中","在线排行榜即将开放",20,new Vector2(450,46),new Vector2(0,-485),Ink,TextAlignmentOptions.Center);
            Label("Ranking service",rank,"Google Play Games","Google Play Games","Google Play Games","Google Play Games",15,new Vector2(450,36),new Vector2(0,-530),Muted,TextAlignmentOptions.Center);
            Label("Ranking placeholder note",rank,"연동 후 순위가 표시됩니다","Rankings will appear here","連携後に順位を表示します","连接后将在此显示排名",15,new Vector2(450,40),new Vector2(0,-720),Muted,TextAlignmentOptions.Center);
            view.menuTexts=texts.ToArray();view.sheetTopInset=95;
            view.japaneseMenuFont=Resources.Load<TMP_FontAsset>("CursorDash/Fonts/Sarasa UI J Menu SDF");view.chineseMenuFont=Resources.Load<TMP_FontAsset>("CursorDash/Fonts/Sarasa UI SC Menu SDF");view.localizeMenuCopy=view.japaneseMenuFont!=null&&view.chineseMenuFont!=null;
            Refine();
        }
        public void Refine()
        {
            view.sheetHeightFraction=1;
            // Both tabs stay bottom-anchored. Only the selected tab draws above the sheet.
            view.navigationPlacement.SetAsFirstSibling();
            view.rankingHeaderAnimation.SetParent(view.safeArea,false);
            view.settingsHeaderAnimation.SetParent(view.safeArea,false);
            view.rankingHeaderAnimation.anchoredPosition=view.settingsHeaderAnimation.anchoredPosition=Vector2.zero;
            view.PositionMenuButtonLayer(view.rankingHeaderAnimation,false);
            view.PositionMenuButtonLayer(view.settingsHeaderAnimation,true);
            var oldSurface=view.sheetAnimation.GetComponent<Image>();
            if(view.sheetSurface==null){
                var surface=Rect("Panel body surface",view.sheetAnimation,Vector2.zero,Vector2.zero,Vector2.zero);
                surface.anchorMax=Vector2.one;surface.pivot=Vector2.one*.5f;surface.SetAsFirstSibling();
                view.sheetSurface=Image(surface,oldSurface!=null?oldSurface.color:Panel,true);
                if(oldSurface!=null){view.sheetSurface.sprite=oldSurface.sprite;view.sheetSurface.material=oldSurface.material;view.sheetSurface.type=oldSurface.type;}
            }
            view.sheetSurface.rectTransform.offsetMin=view.sheetSurface.rectTransform.offsetMax=Vector2.zero;
            var tint=view.sheetSurface.color;tint.a=1;view.sheetSurface.color=tint;
            if(oldSurface!=null){oldSurface.enabled=false;oldSurface.raycastTarget=false;}
            view.rankingDirection=view.rankingHeaderAnimation.Find("Close direction").GetComponent<Graphic>();
            view.settingsDirection=view.settingsHeaderAnimation.Find("Close direction").GetComponent<Graphic>();

            // Lower the settings block 16 units from the preceding revision; retain its row spacing.
            SetY((RectTransform)view.contentAnimation.Find("Audio heading"),-22);
            SetY((RectTransform)view.contentAnimation.Find("Effects heading"),-318);
            float[] positions={-82,-161,-240,-378,-457,-536,-615};int n=0;
            foreach(var row in view.rows)if(row.slider.transform.IsChildOf(view.contentAnimation))SetY((RectTransform)row.slider.transform.parent,positions[n++]);
            SetY((RectTransform)view.resetAudio.transform,-22);SetY((RectTransform)view.resetEffects.transform,-318);

            var tutorial=(RectTransform)view.replayTutorial.transform;var language=(RectTransform)view.languageButton.transform;
            tutorial.sizeDelta=language.sizeDelta=new Vector2(view.contentPlacement.rect.width*.5f,60);
            // Fit the footer above the fixed settings button, including its touch targets.
            view.contentPlacement.sizeDelta=new Vector2(view.contentPlacement.sizeDelta.x,790);
            tutorial.anchoredPosition=new Vector2(-view.contentPlacement.rect.width*.25f,-742);language.anchoredPosition=new Vector2(view.contentPlacement.rect.width*.25f,-742);
            var tutorialText=view.replayTutorial.GetComponentInChildren<TMP_Text>();var textRect=tutorialText.rectTransform;
            textRect.anchorMin=Vector2.zero;textRect.anchorMax=Vector2.one;textRect.pivot=Vector2.one*.5f;textRect.anchoredPosition=Vector2.zero;textRect.sizeDelta=new Vector2(-16,0);
            tutorialText.fontSize=18;tutorialText.fontSizeMin=14;tutorialText.fontSizeMax=18;tutorialText.enableAutoSizing=true;tutorialText.alignment=TextAlignmentOptions.Center;
            var globe=(RectTransform)view.languageButton.transform.Find("Language globe");globe.sizeDelta=new Vector2(18,18);globe.anchoredPosition=new Vector2(-36,0);
            var flag=view.languageFlag.rectTransform;flag.sizeDelta=new Vector2(24,18);flag.anchoredPosition=new Vector2(-5,0);
            var code=view.languageLabel.rectTransform;code.anchorMin=code.anchorMax=code.pivot=Vector2.one*.5f;code.sizeDelta=new Vector2(32,24);code.anchoredPosition=new Vector2(29,0);
            view.languageLabel.fontSize=view.languageLabel.fontSizeMin=view.languageLabel.fontSizeMax=18;view.languageLabel.enableAutoSizing=false;
        }
        private static void SetY(RectTransform rect,float y){rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,y);}
    }
}
