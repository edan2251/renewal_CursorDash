using UnityEngine;
using UnityEngine.UI;

/// <summary>Authored sources and visual defaults; animation and the measured HP boundary remain owned by their existing drivers.</summary>
public sealed class CursorDashNexusView : MonoBehaviour
{
    [SerializeField] private Image body, outer, dotted;
    [SerializeField] private RectTransform placementRoot;
    [SerializeField] private Color energyTint = new Color(.04f,.7f,.9f);
    [SerializeField, Range(.08f,.21f)] private float gaugeThickness=.14f;
    [SerializeField, Range(1,8)] private float gaugeGapDegrees=4;
    [SerializeField, Range(1.2f,2.5f)] private float gaugeRadiusRatio=1.86f;
    public Image Body => body;
    public Image Outer => outer;
    public Image Dotted => dotted;
    public RectTransform PlacementRoot => placementRoot;
    public Color EnergyTint => energyTint;
    public float GaugeThickness => gaugeThickness;
    public float GaugeGapDegrees => gaugeGapDegrees;
    public float GaugeRadiusRatio => gaugeRadiusRatio;
    public bool IsConfigured => body!=null&&outer!=null&&dotted!=null;
}
