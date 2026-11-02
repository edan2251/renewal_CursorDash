using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Saved layout/style and explicit controls. Does not read settings, pause, save, or start a game.</summary>
public sealed partial class CursorDashSettingsView : MonoBehaviour
{
    [Serializable] public sealed class Row
    {
        public string key; public Slider slider; public TMP_Text value;
        [NonSerialized] internal UnityAction<float> changed;
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform safeArea;
    [SerializeField] private Image shade;
    [SerializeField] private RectTransform sheetAnimation;
    [SerializeField] private RectTransform contentPlacement;
    [SerializeField] private RectTransform contentAnimation;
    [SerializeField] private CanvasGroup contentOpacity;
    [SerializeField] private RectTransform handle;
    [SerializeField] private CanvasGroup pauseOpacity;
    [SerializeField] private RectTransform pausePlacement;
    [SerializeField] private RectTransform pauseAnimation;
    [SerializeField] private Row[] rows;
    [SerializeField] private Button replayTutorial, resetSettings, resume, retry, home;
    [SerializeField, Range(.5f, 1)] private float sheetHeightFraction = 1;
    [SerializeField] private float horizontalInset = 48;
    [SerializeField] private float sheetTopInset = 65;
    [SerializeField] private float pauseVerticalInset = 90;
    private CursorDashSettingsUI owner;
    private CursorDashViewFonts fonts;
    private Color shadeTint;
    private bool capturedTint;

    public Canvas Canvas => canvas;
    public RectTransform Root => (RectTransform)canvas.transform;
    public RectTransform SafeArea => safeArea;
    public Image Shade => shade;
    public RectTransform SheetAnimation => sheetAnimation;
    public RectTransform ContentAnimation => contentAnimation;
    public CanvasGroup ContentOpacity => contentOpacity;
    public RectTransform Handle => handle;
    public CanvasGroup PauseOpacity => pauseOpacity;
    public RectTransform PauseAnimation => pauseAnimation;
    public Row[] Rows => rows;
    public bool IsConfigured
    {
        get
        {
            if(canvas==null||safeArea==null||shade==null||sheetAnimation==null||contentPlacement==null||contentAnimation==null||contentOpacity==null||handle==null||handle.GetComponent<CursorDashSheetHandle>()==null||pauseOpacity==null||pausePlacement==null||pauseAnimation==null||rows==null||rows.Length!=9||replayTutorial==null||(!HasMenuPanels&&resetSettings==null)||resume==null||retry==null||home==null)return false;
            foreach(var row in rows)if(row==null||row.slider==null||!(row.slider is CursorDashSettingSlider)||row.value==null)return false;
            return true;
        }
    }

    public void PrepareRendering()
    {
        if (!capturedTint) { shadeTint = shade.color; capturedTint = true; }
        if (fonts != null) return;
        fonts = new CursorDashViewFonts();
        foreach (var label in GetComponentsInChildren<TMP_Text>(true)) fonts.Bind(label);
    }

    public void Bind(CursorDashSettingsUI controller)
    {
        if (owner == controller) return;
        Unbind(); owner = controller;
        foreach (var row in rows)
        {
            var bound = row;
            row.changed = value => owner.SetValue(bound.key, value);
            row.slider.onValueChanged.AddListener(row.changed);
            ((CursorDashSettingSlider)row.slider).finished = owner.Save;
        }
        GetComponentInChildren<CursorDashSheetHandle>(true).owner = owner;
        replayTutorial.onClick.AddListener(owner.ReplayTutorial); if(resetSettings!=null)resetSettings.onClick.AddListener(owner.ResetSettings);
        resume.onClick.AddListener(owner.Resume); retry.onClick.AddListener(owner.Retry); home.onClick.AddListener(owner.Home);
        BindNavigation();
    }

    public void Unbind()
    {
        if (ReferenceEquals(owner,null)) return;
        foreach (var row in rows)
        {
            if (row.slider!=null&&row.changed != null) row.slider.onValueChanged.RemoveListener(row.changed);
            row.changed = null;
            var slider = row.slider as CursorDashSettingSlider;
            if (slider!=null&&slider.finished == (Action)owner.Save) slider.finished = null;
        }
        var grip=GetComponentInChildren<CursorDashSheetHandle>(true);if(grip!=null)grip.owner = null;
        if(replayTutorial!=null)replayTutorial.onClick.RemoveListener(owner.ReplayTutorial);if(resetSettings!=null)resetSettings.onClick.RemoveListener(owner.ResetSettings);
        if(resume!=null)resume.onClick.RemoveListener(owner.Resume);if(retry!=null)retry.onClick.RemoveListener(owner.Retry);if(home!=null)home.onClick.RemoveListener(owner.Home);
        UnbindNavigation();
        owner = null;
    }

    public float Layout(Rect safePixels, Vector2 pixels)
    {
        Canvas.ForceUpdateCanvases();
        safeArea.anchorMin = new Vector2(safePixels.xMin / Mathf.Max(1, pixels.x), safePixels.yMin / Mathf.Max(1, pixels.y));
        safeArea.anchorMax = new Vector2(safePixels.xMax / Mathf.Max(1, pixels.x), safePixels.yMax / Mathf.Max(1, pixels.y));
        safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        float height = safeArea.rect.height * sheetHeightFraction;
        sheetAnimation.sizeDelta = new Vector2(safeArea.rect.width, height);
        // Only the chosen half-header occupies the top strip; the body starts below it.
        if(sheetSurface!=null){sheetSurface.rectTransform.offsetMin=Vector2.zero;sheetSurface.rectTransform.offsetMax=new Vector2(0,-navigationHeight);}
        float fit = Mathf.Min(1, (safeArea.rect.width - horizontalInset) / Mathf.Max(1, contentPlacement.rect.width), (height - sheetTopInset) / Mathf.Max(1, contentPlacement.rect.height));
        contentAnimation.localScale = Vector3.one * Mathf.Max(.4f, fit);
        if(rankingAnimation!=null&&rankingPlacement!=null){float rankFit=Mathf.Min(1,(safeArea.rect.width-horizontalInset)/Mathf.Max(1,rankingPlacement.rect.width),(height-sheetTopInset)/Mathf.Max(1,rankingPlacement.rect.height));rankingAnimation.localScale=Vector3.one*Mathf.Max(.4f,rankFit);}
        float pauseFit = Mathf.Min(1, (safeArea.rect.width - horizontalInset) / Mathf.Max(1, pausePlacement.rect.width), (safeArea.rect.height - pauseVerticalInset) / Mathf.Max(1, pausePlacement.rect.height));
        pauseAnimation.localScale = Vector3.one * Mathf.Max(.4f, pauseFit);
        return height;
    }

    public void Present(bool menu, float sheet, float pauseAlpha, bool interactive)
    {
        float eased = sheet * sheet * (3 - 2 * sheet);
        PresentMenu(menu,sheet,interactive);
        pauseOpacity.gameObject.SetActive(pauseAlpha > .001f); pauseOpacity.alpha = pauseAlpha;
        pauseOpacity.interactable = pauseOpacity.blocksRaycasts = interactive && pauseAlpha > 0;
        pauseAnimation.anchoredPosition = new Vector2(0, Mathf.Lerp(-9, 0, pauseAlpha));
        if (!capturedTint) { shadeTint = shade.color; capturedTint = true; }
        var tint = shadeTint; tint.a *= Mathf.Max(menu ? eased * .32f : 0, pauseAlpha * .82f); shade.color = tint;
        shade.raycastTarget = interactive && (menu && sheet > .001f || pauseAlpha > 0);
    }

    private void OnDestroy() { Unbind(); fonts?.Dispose(); fonts = null; }
}
