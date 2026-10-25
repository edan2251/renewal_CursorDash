using UnityEngine;

/// <summary>Small, persistent visual prototype settings; no combat rules or global runtime manager.</summary>
[CreateAssetMenu(menuName="CursorDash/Visual prototype profile")]
public sealed class CursorDashVisualProfile : ScriptableObject
{
    [Header("Normal background — slow regions, independent of hit flashes")]
    [Range(.03f,.4f)] public float backgroundSpeed=.16f;
    [Range(.2f,.9f)] public float backgroundMix=.66f;
    [Range(0,.5f)] public float backgroundBend=.22f;
    [Range(0,3)] public float backgroundHitSpeed=1.8f;
    [Range(.08f,.6f)] public float backgroundHitRecovery=.26f;
    [Range(.2f,1)] public float circuitRestContrast=.72f;
    [Header("Fever only — grouped feedback still has hard time/strength caps")]
    [Range(.5f,4f)] public float feverBreakStrength=2.5f;
    [Range(.06f,.5f)] public float feverSpawnInterval=.12f;
    [Range(2,36)] public int feverConcurrentLimit=14;
    [Range(.05f,.8f)] public float feverNodeGap=.22f;
    [Range(.1f,.7f)] public float feverCursorClearance=.28f;
    [Range(.05f,.25f)] public float feverSideMargin=.12f;
    [Range(.1f,.3f)] public float feverTopMargin=.18f;
    [Range(.1f,.3f)] public float feverBottomMargin=.16f;
    [Range(.25f,.7f)] public float feverFragmentLife=.48f;
    [Range(.04f,.16f)] public float feverGatherCompression=.115f;
    [Header("Node appearance/departure — existing lifetime and combat clocks stay unchanged")]
    [Range(.05f,.2f)] public float feverNodeAppearSeconds=.12f;
    [Range(.2f,.9f)] public float feverNodeRetireSeconds=.55f;
    [Range(.1f,.3f)] public float feverThreatRetireSeconds=.18f;
    [Header("Fever optics — final board pass, stable HUD excluded")]
    [Range(.1f,.6f)] public float feverOpticsSeconds=.30f;
    [Range(0,.025f)] public float feverLensStrength=.0175f;
    [Range(.2f,.9f)] public float feverLensRadius=.58f;
    [Range(0,.006f)] public float feverChromaticHit=.0042f;
    [Range(0,.0015f)] public float feverChromaticAmbient=.0008f;
    [Range(0,1)] public float feverShakeScale=.45f;
}
