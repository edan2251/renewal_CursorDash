using UnityEngine;
using UnityEngine.UI;

/// <summary>Saved board artwork sources. No game clock, input, scoring or fever state.</summary>
public sealed class CursorDashBoardView : MonoBehaviour
{
    [SerializeField] private MeshRenderer colorField;
    [SerializeField] private CursorDashVisualProfile profile;
    [SerializeField] private Image decorationAnchor;
    [SerializeField,Tooltip("옛 PNG 비교·비지원 환경 복구 전용. 기본 게임은 Color Field와 동적 회로를 사용합니다.")] private Sprite comparisonDecoration;
    public MeshRenderer ColorField=>colorField;
    public CursorDashVisualProfile Profile=>profile;
    public Image DecorationAnchor=>decorationAnchor;
    public Sprite ComparisonDecoration=>comparisonDecoration;
    public bool IsConfigured=>colorField!=null&&colorField.sharedMaterial!=null&&colorField.GetComponent<MeshFilter>()?.sharedMesh!=null&&profile!=null;
}
