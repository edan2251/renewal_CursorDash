using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CursorDashSettingsView
{
    public enum MenuPanel { Ranking, Settings }
    [Serializable] public sealed class MenuText
    {
        public TMP_Text label;
        public string english, japanese, chinese;
        [NonSerialized] internal string korean;
        [NonSerialized] internal TMP_FontAsset originalFont;
        [NonSerialized] internal Material originalMaterial;
    }
    [Header("Main menu panels — authored placement / runtime offsets")]
    [SerializeField] private RectTransform navigationPlacement;
    [SerializeField] private RectTransform rankingHeaderAnimation, settingsHeaderAnimation;
    [SerializeField] private CanvasGroup rankingHeaderOpacity, settingsHeaderOpacity, rankingOpacity;
    [SerializeField] private CanvasGroup navigationDivider;
    [SerializeField] private RectTransform rankingPlacement, rankingAnimation;
    [SerializeField] private Image sheetSurface;
    [SerializeField] private Graphic rankingDirection, settingsDirection;
    [SerializeField] private Button resetAudio, resetEffects, languageButton;
    [SerializeField] private CursorDashMenuIcon languageFlag;
    [SerializeField] private TMP_Text languageLabel, rankingBest;
    [SerializeField] private MenuText[] menuTexts;
    [SerializeField,Tooltip("새 하단 버튼·랭킹·설정 패널만 번역합니다. 한국어 원문은 자식 TMP, 나머지 언어는 Menu Texts에서 편집합니다.")]
    private bool localizeMenuCopy;
    [SerializeField] private TMP_FontAsset japaneseMenuFont, chineseMenuFont;
    [SerializeField] private float navigationHeight=72;
    [SerializeField,Min(.05f)] private float panelTravelSeconds=.24f;
    private MenuPanel selectedPanel=MenuPanel.Settings;
    public MenuPanel SelectedPanel=>selectedPanel;
    public RectTransform NavigationPlacement=>navigationPlacement;
    public RectTransform RankingPlacement=>rankingPlacement;
    public float NavigationHeight=>navigationHeight;
    public float PanelTravelSeconds=>panelTravelSeconds;
    public Image SheetSurface=>sheetSurface;
    public Graphic[] MenuDirectionGraphics=>new[]{rankingDirection,settingsDirection};
    public RectTransform[] MenuAnimationTransforms=>new[]{rankingHeaderAnimation,settingsHeaderAnimation,rankingAnimation};
    public CanvasGroup[] MenuOpacityGroups=>new[]{rankingHeaderOpacity,settingsHeaderOpacity,rankingOpacity,navigationDivider};
    public bool HasMenuPanels=>navigationPlacement!=null&&rankingHeaderAnimation!=null&&settingsHeaderAnimation!=null&&rankingOpacity!=null&&resetAudio!=null&&resetEffects!=null&&languageButton!=null&&languageFlag!=null&&rankingBest!=null;
    public void SelectPanel(MenuPanel panel){selectedPanel=panel;}
    public void SetRankingBest(string value){if(rankingBest!=null)rankingBest.text=value;}
    public bool ContainsNavigationPoint(Vector2 point)
    {
        if(navigationPlacement==null)return handle!=null&&RectTransformUtility.RectangleContainsScreenPoint(handle,point);
        return navigationPlacement.gameObject.activeInHierarchy&&
            (rankingHeaderOpacity.blocksRaycasts&&RectTransformUtility.RectangleContainsScreenPoint(rankingHeaderAnimation,point)||
             settingsHeaderOpacity.blocksRaycasts&&RectTransformUtility.RectangleContainsScreenPoint(settingsHeaderAnimation,point));
    }
    public void PresentMenu(bool menu,float progress,bool interactive)
    {
        float eased=progress*progress*(3-2*progress);
        bool settings=selectedPanel==MenuPanel.Settings;
        bool hasMenuPanels=HasMenuPanels;
        float travel=Mathf.Max(0,sheetAnimation.rect.height-(hasMenuPanels?0:navigationHeight));
        sheetAnimation.gameObject.SetActive(menu);
        sheetAnimation.anchoredPosition=new Vector2(0,-travel*(1-eased));
        contentOpacity.alpha=settings?eased:0;
        contentOpacity.interactable=contentOpacity.blocksRaycasts=menu&&settings&&interactive&&progress>.98f;
        if(!hasMenuPanels)return;
        rankingOpacity.alpha=settings?0:eased;
        rankingOpacity.interactable=rankingOpacity.blocksRaycasts=menu&&!settings&&interactive&&progress>.98f;
        navigationPlacement.gameObject.SetActive(menu);
        rankingHeaderAnimation.gameObject.SetActive(menu);
        settingsHeaderAnimation.gameObject.SetActive(menu);
        PositionMenuButtonLayer(settings?rankingHeaderAnimation:settingsHeaderAnimation,false);
        PositionMenuButtonLayer(settings?settingsHeaderAnimation:rankingHeaderAnimation,true);

        float reveal=travel*eased;
        Header(rankingHeaderOpacity,settings&&reveal>=rankingHeaderAnimation.rect.height,interactive);
        Header(settingsHeaderOpacity,!settings&&reveal>=settingsHeaderAnimation.rect.height,interactive);
        if(rankingDirection!=null)rankingDirection.enabled=menu&&!settings&&progress>.001f;
        if(settingsDirection!=null)settingsDirection.enabled=menu&&settings&&progress>.001f;
        if(navigationDivider!=null)navigationDivider.alpha=1;
    }
    private void PositionMenuButtonLayer(RectTransform button,bool aboveSheet)
    {
        int buttonIndex=button.GetSiblingIndex();
        int sheetIndex=sheetAnimation.GetSiblingIndex();
        // Removing a button before the sheet shifts the sheet index down by one.
        if(buttonIndex<sheetIndex)sheetIndex--;
        int targetIndex=aboveSheet?sheetIndex+1:sheetIndex;
        if(buttonIndex!=targetIndex)button.SetSiblingIndex(targetIndex);
    }
    private static void Header(CanvasGroup opacity,bool covered,bool interactive)
    {
        opacity.alpha=1;
        opacity.interactable=opacity.blocksRaycasts=interactive&&!covered;
    }
    public void ApplyMenuLanguage(string language)
    {
        if(fonts==null)PrepareRendering();
        if(menuTexts!=null)foreach(var entry in menuTexts)
        {
            if(entry.label==null)continue;
            if(entry.korean==null){entry.korean=entry.label.text;entry.originalFont=entry.label.font;entry.originalMaterial=entry.label.fontSharedMaterial;}
            string value=!localizeMenuCopy?entry.korean:language=="en"?entry.english:language=="ja"?entry.japanese:language=="zh-Hans"?entry.chinese:entry.korean;
            entry.label.text=string.IsNullOrEmpty(value)?entry.korean:value;
            var regional=localizeMenuCopy?(language=="ja"?japaneseMenuFont:language=="zh-Hans"?chineseMenuFont:null):null;
            if(regional!=null)fonts.Bind(entry.label,regional);
            else{entry.label.font=entry.originalFont;entry.label.fontSharedMaterial=entry.originalMaterial;}
        }
        if(languageLabel!=null)languageLabel.text=language=="en"?"EN":language=="ja"?"JP":language=="zh-Hans"?"CN":"KO";
        if(languageFlag!=null)languageFlag.Kind=language=="en"?CursorDashMenuIcon.Symbol.English:language=="ja"?CursorDashMenuIcon.Symbol.Japan:language=="zh-Hans"?CursorDashMenuIcon.Symbol.China:CursorDashMenuIcon.Symbol.Korea;
    }
    private void BindNavigation()
    {
        if(!HasMenuPanels)return;
        foreach(var grip in GetComponentsInChildren<CursorDashSheetHandle>(true))grip.owner=owner;
        resetAudio.onClick.AddListener(owner.ResetAudio);resetEffects.onClick.AddListener(owner.ResetEffects);languageButton.onClick.AddListener(owner.CycleMenuLanguage);
    }
    private void UnbindNavigation()
    {
        if(navigationPlacement!=null)foreach(var grip in GetComponentsInChildren<CursorDashSheetHandle>(true))grip.owner=null;
        if(resetAudio!=null)resetAudio.onClick.RemoveListener(owner.ResetAudio);if(resetEffects!=null)resetEffects.onClick.RemoveListener(owner.ResetEffects);if(languageButton!=null)languageButton.onClick.RemoveListener(owner.CycleMenuLanguage);
    }
}
