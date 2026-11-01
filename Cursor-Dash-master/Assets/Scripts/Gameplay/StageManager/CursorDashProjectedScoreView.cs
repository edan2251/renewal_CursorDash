using TMPro;
using UnityEngine;

/// <summary>One authored numeric renderer and two endpoint poses. No points, records, or countdown logic.</summary>
public sealed class CursorDashProjectedScoreView : MonoBehaviour
{
    [SerializeField] private RectTransform placementRoot;
    [SerializeField] private RectTransform animationRoot;
    [SerializeField] private TextMeshPro artwork;
    [Header("Main endpoint")]
    [SerializeField] private Vector2 mainViewport = new Vector2(.5f, .5f);
    [SerializeField, Range(.5f, .95f)] private float mainGlyphScale = .68f;
    [SerializeField] private Color mainTint = new Color(1, 1, 1, .85f);
    [Header("Combat endpoint")]
    [SerializeField] private Vector2 combatViewport = new Vector2(.5f, .5f);
    [SerializeField] private Color combatTint = new Color(.36f, .76f, .84f, .105f);
    private CursorDashViewFonts fonts;

    public RectTransform PlacementRoot => placementRoot;
    public RectTransform AnimationRoot => animationRoot;
    public TextMeshPro Artwork => artwork;
    public Vector2 MainViewport => mainViewport;
    public Vector2 CombatViewport => combatViewport;
    public float MainGlyphScale => mainGlyphScale;
    public Color MainTint => mainTint;
    public Color CombatTint => combatTint;
    public bool IsConfigured => placementRoot != null && animationRoot != null && artwork != null && artwork.font != null;

    public Vector2 WorldPoint(Camera board, Vector2 viewport) => (Vector2)board.ViewportToWorldPoint(viewport) + (Vector2)placementRoot.position;
    public void PrepareRendering()
    {
        if (fonts != null) return;
        fonts = new CursorDashViewFonts(); fonts.Bind(artwork);
    }
    private void OnDestroy() { fonts?.Dispose(); fonts = null; }
}

/// <summary>The same glyph-centering operation is used by live projection and static authoring renders.</summary>
public static class CursorDashScoreGeometry
{
    public static void Center(TMP_TextInfo info, Rect rect, float scale)
    {
        if (info.meshInfo == null) return;
        Vector3 low = new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0), high = new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0);
        bool any = false;
        foreach (var part in info.meshInfo)
        {
            if (part.vertices == null) continue;
            for (int n = 0; n < part.vertexCount && n < part.vertices.Length; n++) { low = Vector3.Min(low, part.vertices[n]); high = Vector3.Max(high, part.vertices[n]); any = true; }
        }
        if (!any) return;
        Vector3 center = (low + high) * .5f, anchor = rect.center;
        foreach (var part in info.meshInfo)
            if (part.vertices != null) for (int n = 0; n < part.vertexCount && n < part.vertices.Length; n++) part.vertices[n] = anchor + (part.vertices[n] - center) * scale;
    }
}
