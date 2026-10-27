using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Preserved PNG comparison/fallback compositor. The optional path layer suppresses only this decoration.</summary>
[RequireComponent(typeof(Camera))]
public sealed class OriginalBackgroundFlash : MonoBehaviour
{
    private struct Flash { public Vector3 point; public float started; }
    private readonly List<Flash> flashes = new List<Flash>();
    private readonly Vector4[] flashData = new Vector4[8];
    private readonly Vector3[] corners = new Vector3[4];
    private Camera cameraView;
    private Material material;
    private Image source;
    private bool previousEnabled;
    public Image SourceImage => source;
    public bool DynamicCircuits {get;set;}
    public bool Ready {get {Bind();return material!=null&&source!=null&&source.sprite!=null&&source.gameObject.activeInHierarchy;}}
    public int ActiveFlashCount => flashes.Count;
    public const float FlashSeconds = .25f;

    private void Awake()
    {
        cameraView = GetComponent<Camera>();
        Shader shader = Resources.Load<Shader>("CursorDash/OriginalBackgroundFlash");
        if (shader != null && shader.isSupported) material = new Material(shader) { hideFlags = HideFlags.DontSave };
        else Debug.LogWarning("CursorDash: original background compositor unavailable; preserving its UI image.");
    }
    private void LateUpdate() { Bind(); }
    private void Bind()
    {
        if (source != null || material == null) return;
        foreach (var image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (image.name != "Background_Flash" || image.transform.root.name != "RemakeHUD" || image.gameObject.scene != gameObject.scene ||
                !image.enabled || image.sprite == null || image.type != Image.Type.Simple || image.preserveAspect || !image.gameObject.activeInHierarchy) continue;
            // This scene's source is a standalone, unrotated sprite. Leave unsupported atlas layouts intact.
            if (image.sprite.packed) continue;
            source = image; previousEnabled = image.enabled; image.enabled = false;
            // The original color/alpha (87/255), RectTransform, sprite and importer are never written.
            Canvas.ForceUpdateCanvases();
            break;
        }
    }
    public bool CanComposite
    {
        get { return !DynamicCircuits&&Ready; }
    }
    public void Hit(Vector3 point)
    {
        flashes.RemoveAll(f => Time.unscaledTime - f.started >= FlashSeconds);
        int cap = Mathf.Clamp(CursorDashTuning.Integer("fx.distortionSlots", 4), 1, 8);
        if (flashes.Count >= cap) flashes.RemoveAt(0);
        flashes.Add(new Flash { point = point, started = Time.unscaledTime });
    }
    public void Clear() { flashes.Clear(); }

    public void Composite(RenderTexture input, RenderTexture destination, Vector4[] lensHits, Vector4[] lensShapes, float offsetCap)
    {
        if (!CanComposite) { Graphics.Blit(input, destination); return; }
        source.rectTransform.GetWorldCorners(corners);
        Canvas canvas = source.canvas;
        Camera canvasCamera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[2]);
        // Overlay corners are screen coordinates even when a diagnostic camera renders into a smaller RT.
        Rect cameraRect = cameraView.rect;
        Rect viewport = new Rect(cameraRect.x * Screen.width, cameraRect.y * Screen.height,
            cameraRect.width * Screen.width, cameraRect.height * Screen.height);
        if (b.x <= a.x || b.y <= a.y || viewport.width <= 0 || viewport.height <= 0)
        { Graphics.Blit(input, destination); return; }
        material.SetVector("_ImageRect", new Vector4((a.x - viewport.x) / viewport.width, (a.y - viewport.y) / viewport.height,
            (b.x - a.x) / viewport.width, (b.y - a.y) / viewport.height));
        Sprite sprite = source.sprite; Rect uv = sprite.textureRect; Texture texture = sprite.texture;
        material.SetTexture("_FlashTex", texture);
        material.SetVector("_SpriteRect", new Vector4(uv.x / texture.width, uv.y / texture.height, uv.width / texture.width, uv.height / texture.height));
        material.SetColor("_ImageColor", source.color);
        var visual=StageManager.instance?.Fever?.Visuals;
        material.SetFloat("_RestContrast",visual==null?1:Mathf.Clamp(visual.circuitRestContrast,.2f,1));
        float shorter = Mathf.Min(input.width, input.height);
        material.SetVector("_Aspect", new Vector4(input.width / shorter, input.height / shorter, 0, 0));
        bool enabled = CursorDashTuning.Toggle("fx.enabled", true);
        flashes.RemoveAll(f => Time.unscaledTime - f.started >= FlashSeconds);
        for (int i = 0; i < flashData.Length; i++)
        {
            flashData[i] = Vector4.zero;
            if (!enabled || i >= flashes.Count) continue;
            var flash = flashes[i]; Vector3 center = cameraView.WorldToViewportPoint(flash.point);
            flashData[i] = new Vector4(center.x, center.y, Mathf.Clamp01((Time.unscaledTime - flash.started) / FlashSeconds), 1);
        }
        material.SetVectorArray("_Flashes", flashData);
        material.SetVectorArray("_LensHits", lensHits); material.SetVectorArray("_LensShapes", lensShapes);
        material.SetFloat("_OffsetCap", offsetCap);
        float gain = PlayerPrefs.GetString("OnFlash", "true") == "true" ? CursorDashTuning.EffectGain("user.flash") : 0;
        material.SetFloat("_FlashGain", CursorDashTuning.Number("visual.impactExposure", .65f) * gain);
        Graphics.Blit(input, destination, material);
    }
    private void Restore()
    {
        Clear();
        if (source != null) source.enabled = previousEnabled;
        source = null;
    }
    private void OnDisable() { Restore(); }
    private void OnDestroy() { Restore(); if (material != null) Destroy(material); }
}
