using UnityEngine;

/// <summary>Small, persistent visual prototype settings; no combat rules or global runtime manager.</summary>
[CreateAssetMenu(menuName="CursorDash/Visual prototype profile")]
public sealed class CursorDashVisualProfile : ScriptableObject
{
    [Header("Shared main / gameplay space — captured when Start is accepted")]
    public Sprite mainLogo;
    [Range(.65f,3)] public float spaceStartSeconds=1.35f;
    [Range(.12f,.4f)] public float spaceRetreatEnd=.28f;
    [Range(.55f,.95f)] public float spaceRebuildEnd=.82f;
    [Range(.25f,.65f)] public float spaceNexusAt=.42f;
    [Range(.45f,.82f)] public float spaceCursorAt=.64f;
    [Range(.2f,1.5f)] public float spaceCursorDistance=.95f;
    [HideInInspector] public float spaceBestScale=1.5f; // D-052 two-text crossfade, retained for rollback only.
    [HideInInspector] public float spaceBestOpacity=.52f;
    [Range(.03f,.18f)] public float spaceRingLead=.10f;
    [Range(0,.08f)] public float spaceBodyOvershoot=.045f;
    [Range(.08f,.4f)] public float spaceCursorAcceleration=.18f;
    [Range(180,950)] public float spaceRingBoost=780;
    [Range(.08f,.5f)] public float spaceRingDecay=.30f;
    [Range(.65f,.95f)] public float spaceMainScoreScale=.82f;
    [Range(.4f,1)] public float spaceMainScoreOpacity=.85f;
    [Range(0,.6f)] public float spaceMainScoreLift=.28f;
    [Range(780,940)] public float spaceLogoWidth=840;
    [Range(.75f,.84f)] public float spaceLogoY=.79f;
    [Header("Confirmed Nexus health loss — board optics, stable HUD")]
    [Range(.08f,.4f)] public float nexusGaugeRewindSeconds=.16f;
    [Range(.1f,.5f)] public float nexusDamageSeconds=.24f;
    [Range(0,.45f)] public float nexusDamageFlash=.26f;
    [ColorUsage(false)] public Color nexusDamageTint=new Color(1,.04f,.08f,1);
    [Range(0,.008f)] public float nexusDamageChromatic=.0055f;
    public bool nexusDamageWaveEnabled=true;
    [Range(0,.018f)] public float nexusDamageWaveStrength=.008f;
    [Range(.03f,.18f)] public float nexusDamageWaveWidth=.12f;
    [Range(.4f,4)] public float nexusDamageWaveSpeed=2.6f;
    [Range(.2f,1.2f)] public float nexusDamageWaveRange=.72f;
    [Range(0,1.2f)] public float nexusDamageCompression=.85f;
    [Range(.6f,2.4f)] public float nexusDamageReleasePower=1.4f;
    [Header("Normal background — slow regions, independent of hit flashes")]
    [Range(.03f,.4f)] public float backgroundSpeed=.22f;
    [Range(.2f,.9f)] public float backgroundMix=.84f;
    [Range(0,.5f)] public float backgroundBend=.30f;
    [Range(0,3)] public float backgroundHitSpeed=2.6f;
    [Range(.08f,.6f)] public float backgroundHitRecovery=.38f;
    [Range(0,1)] public float backgroundHitColor=.65f;
    [Range(0,.12f)] public float backgroundHitShift=.075f;
    [Range(.2f,1)] public float circuitRestContrast=.62f;
    [Header("Dynamic circuits — background only; density applies on next round or viewport resize")]
    public bool dynamicCircuitsEnabled=true;
    public bool circuitHitsEnabled=true;
    [Range(4,12)] public int circuitDensity=12;
    public bool circuitTwoTierWidthsEnabled=true;
    [Tooltip("현재 주회로의 절대 폭. 1080px 기준20px. 대각 경로·둥근 끝과 조화시키는 시안값입니다.")]
    [Range(.012f,.04f)] public float circuitPrimaryWidth=.018518519f;
    [Tooltip("보조 회로의 절대 폭. 1080px 기준15px. 주회로와 독립입니다.")]
    [Range(.006f,.025f)] public float circuitSecondaryWidth=.013888889f;
    // D-048 comparison widths; not multiplied again. Used only when the two-tier comparison is disabled.
    [HideInInspector] public float circuitMainWidth=.043055556f;
    [HideInInspector] public float circuitLineWidth=.025925926f;
    [HideInInspector] public float circuitBranchWidth=.009259259f;
    // Retain old serialized values for comparison/rollback. Target-ring ornaments are no longer drawn.
    [HideInInspector] public float circuitContactRadius=.02962963f;
    [HideInInspector] public float circuitContactStroke=.006481481f;
    [Range(0,.25f)] public float circuitTipEmphasis=.14f;
    [ColorUsage(false)] public Color circuitNormalTint=new Color(.40f,.47f,.52f,1);
    [ColorUsage(false)] public Color circuitFeverTint=new Color(.18f,.19f,.20f,1);
    [Range(.02f,.24f)] public float circuitNormalContrast=.075f;
    [Range(.03f,.25f)] public float circuitFeverContrast=.065f;
    public bool circuitAmbientEnabled=true;
    [Range(.08f,.6f)] public float circuitAmbientSpeed=.24f;
    [Range(1.5f,12)] public float circuitAmbientInterval=2.6f;
    [Range(0,.16f)] public float circuitAmbientStrength=.10f;
    [Range(.06f,.35f)] public float circuitAmbientTail=.20f;
    [Range(.008f,.06f)] public float circuitAmbientHead=.025f;
    [Range(0,3)] public int circuitAmbientLimit=2;
    [Range(.02f,.25f)] public float circuitHitRange=.13f;
    [Range(.3f,3)] public float circuitTransferSpeed=1.65f;
    [Range(.05f,.5f)] public float circuitHitDecay=.26f;
    [Range(.08f,.6f)] public float circuitHitSeconds=.24f;
    [Range(0,.3f)] public float circuitMaxEmphasis=.18f;
    [Range(1,8)] public int circuitReactionLimit=4;
    [Range(.4f,4)] public float circuitBuildSpeed=1.8f;
    [Range(.4f,4)] public float circuitRestoreSpeed=2.1f;
    [Range(0,.05f)] public float circuitPathStagger=.018f;
    [Range(0,.2f)] public float circuitEntryPush=.075f;
    // Legacy central-intake values are retained in serialization only. Circuits no longer use that exit path.
    [HideInInspector] public float circuitRecallBend=.65f;
    [HideInInspector] public float circuitRecallPower=2.6f;
    [HideInInspector] public float circuitRecallStagger=.12f;
    [Header("Circuit fever retreat — next fever capture; gauge progress units, not real seconds")]
    public bool circuitFeverRetreatEnabled=true;
    [Range(.08f,.4f)] public float circuitRetreatStart=.18f;
    [Range(0,.45f)] public float circuitRetreatSpread=.30f;
    [Range(.55f,2.4f)] public float circuitRetreatPower=1.35f;
    [Range(.02f,.15f)] public float circuitRetreatReadHold=.06f;
    [Header("Circuit-only score glyph occlusion — display opacity stays unchanged")]
    public bool circuitScoreMaskEnabled=true;
    [Range(0,1)] public float circuitScoreMaskStrength=1;
    [Range(0,2)] public float circuitScoreMaskPadding=.65f;
    [Range(.25f,2)] public float circuitScoreMaskSoftness=.75f;
    [Header("Circuit game-over retreat — existing outward motion")]
    [Range(.4f,4)] public float circuitDeathSpeed=1.6f;
    [Range(0,.04f)] public float circuitDeathStagger=.018f;
    public void ResetCircuitDefaults()
    {
        dynamicCircuitsEnabled=circuitHitsEnabled=true;circuitDensity=12;circuitMainWidth=.043055556f;circuitLineWidth=.025925926f;circuitBranchWidth=.009259259f;circuitContactRadius=.02962963f;circuitContactStroke=.006481481f;circuitTipEmphasis=.14f;circuitNormalContrast=.075f;circuitFeverContrast=.065f;
        circuitTwoTierWidthsEnabled=true;circuitPrimaryWidth=.018518519f;circuitSecondaryWidth=.013888889f;circuitAmbientEnabled=true;
        circuitNormalTint=new Color(.40f,.47f,.52f,1);circuitFeverTint=new Color(.18f,.19f,.20f,1);circuitAmbientSpeed=.24f;circuitAmbientInterval=2.6f;circuitAmbientStrength=.10f;circuitAmbientTail=.20f;circuitAmbientHead=.025f;circuitAmbientLimit=2;circuitRecallPower=2.6f;circuitRecallStagger=.12f;
        circuitFeverRetreatEnabled=true;circuitRetreatStart=.18f;circuitRetreatSpread=.30f;circuitRetreatPower=1.35f;circuitRetreatReadHold=.06f;
        circuitScoreMaskEnabled=true;circuitScoreMaskStrength=1;circuitScoreMaskPadding=.65f;circuitScoreMaskSoftness=.75f;
        circuitHitRange=.13f;circuitTransferSpeed=1.65f;circuitHitDecay=.26f;circuitHitSeconds=.24f;circuitMaxEmphasis=.18f;circuitReactionLimit=4;
        circuitBuildSpeed=1.8f;circuitRestoreSpeed=2.1f;circuitPathStagger=.018f;circuitEntryPush=.075f;circuitRecallBend=.65f;circuitDeathSpeed=1.6f;circuitDeathStagger=.018f;
    }
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
    [Header("Fever exit — one ring-led clock; no extra fever play time")]
    [Range(.08f,.6f)] public float feverExitAppear=.20f;
    [Range(.25f,1.2f)] public float feverExitAbsorb=.51f;
    [Range(.1f,.6f)] public float feverExitSettle=.24f;
    [Range(180,1800)] public float feverExitSpin=1260;
    [Range(.04f,.4f)] public float feverExitSpinDecay=.10f;
    [Range(0,.2f)] public float feverExitSqueeze=.12f;
    [Range(0,.5f)] public float feverExitBend=.28f;
    [Range(1.2f,4)] public float feverExitPower=3.2f;
    [Range(0,1.8f)] public float feverExitTwist=.90f;
    [Range(0,.006f)] public float feverExitChromatic=.006f;
    [Range(0,.006f)] public float feverExitShake=.0035f;
    [Range(0,.018f)] public float feverExitLens=.012f;
    [Tooltip("등장 중 일정한 회전속도. 이 단계에는 흡입/공간 왜곡이 없습니다.")]
    [Range(24,480)] public float feverExitAppearSpin=180;
    [Range(0,1)] public float feverExitNodeStretch=.65f;
    [Tooltip("출발점에서 링까지 거리 대비 남은 반경 거리의 비율. 시간 진행률이 아니라 실제 거리로 축소를 시작합니다.")]
    [Range(.05f,.4f)] public float feverExitNodeShrinkStart=.22f;
    [Range(0,.85f)] public float feverExitSpacePull=.65f;
    [Tooltip("마지막 흡수 뒤 수축 크기를 잠깐 유지하는 초. 이후 원래 크기로 복귀합니다.")]
    [Range(0,.12f)] public float feverExitRecoilHold=.06f;
    [Header("Projected total score — world layer, independent of the stable top HUD")]
    public bool backgroundScoreEnabled=true;
    [Range(.15f,.85f)] public float backgroundScoreX=.5f;
    [Range(.15f,.85f)] public float backgroundScoreY=.5f;
    [Tooltip("과거 화면 맞춤 호환값. 현재는 World Rect 4×4와 TMP Auto Size 1~3.5를 사용합니다.")]
    [HideInInspector] public float backgroundScoreSize=.42f;
    [HideInInspector] public float backgroundScoreWidth=.80f;
    public Color backgroundScoreTint=new Color(.36f,.76f,.84f,1);
    [Range(0,.22f)] public float backgroundScoreOpacity=.105f;
    [Range(0,.3f)] public float backgroundScoreOpacityBoost=.14f;
    [Range(.05f,.35f)] public float backgroundScoreOpacityCap=.24f;
    [Range(0,.4f)] public float backgroundScoreReaction=.22f;
    [Range(1,1.4f)] public float backgroundScoreScaleCap=1.22f;
    [Range(.15f,.65f)] public float backgroundScoreResponse=.32f;
    public FeverExitMotion CaptureExit()=>new FeverExitMotion(feverExitAppear,feverExitAbsorb,feverExitSettle,feverExitPower,feverExitSpin,feverExitSpinDecay,feverExitSqueeze,feverExitBend,feverExitTwist,feverExitChromatic,feverExitShake,feverExitLens,feverExitAppearSpin,feverExitNodeStretch,feverExitNodeShrinkStart,feverExitSpacePull,feverExitRecoilHold);
}
