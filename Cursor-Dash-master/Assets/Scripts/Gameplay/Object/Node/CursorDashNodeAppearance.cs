using UnityEngine;

/// <summary>Mesh artwork defaults only. Geometry defining collision and attack clocks stays in the existing nodes.</summary>
[CreateAssetMenu(menuName = "CursorDash/Node appearance")]
public sealed class CursorDashNodeAppearance : ScriptableObject
{
    [Range(.01f, .12f)] public float outline = .035f;
    [Range(0, .2f)] public float core;
    [Range(0, .15f)] public float glow;
    public Color circularFill = new Color32(203, 73, 255, 255);
    public Color circularBorder = new Color32(232, 124, 255, 255);
    [Range(.4f, 1)] public float secondHexRingBrightness = .82f;
    [Range(.02f, .12f)] public float hexBodyStroke = .071768f;
    [Range(.8f, 1.2f)] public float hexFillRatio = 1.014927f;

    private static CursorDashNodeAppearance cached;
    public static CursorDashNodeAppearance Default => cached != null ? cached : (cached = Resources.Load<CursorDashNodeAppearance>("CursorDash/NodeAppearance"));
    public static float Outline(bool overrides = true)
    {
        float value = Default == null ? .035f : Default.outline;
        return overrides ? CursorDashTuning.FeedbackNumber("visual.outline", value) : value;
    }
}
