using UnityEngine;

/// <summary>Small, persistent visual prototype settings; no combat rules or global runtime manager.</summary>
[CreateAssetMenu(menuName="CursorDash/Visual prototype profile")]
public sealed class CursorDashVisualProfile : ScriptableObject
{
    [Header("Normal background — slow regions, independent of hit flashes")]
    [Range(.03f,.4f)] public float backgroundSpeed=.22f;
    [Range(.2f,.9f)] public float backgroundMix=.84f;
    [Range(0,.5f)] public float backgroundBend=.30f;
    [Range(0,3)] public float backgroundHitSpeed=2.6f;
    [Range(.08f,.6f)] public float backgroundHitRecovery=.38f;
    [Range(0,1)] public float backgroundHitColor=.65f;
    [Range(0,.12f)] public float backgroundHitShift=.075f;
    [Range(.2f,1)] public float circuitRestContrast=.62f;
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
