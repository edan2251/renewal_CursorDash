using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CursorDashBoardView))]
public sealed class CursorDashBoardViewEditor:Editor
{
    private bool connections;
    public override void OnInspectorGUI()
    {
        var view=(CursorDashBoardView)target;
        EditorGUILayout.HelpBox("실제 게임과 Scene 표시가 같은 VisualPrototype의 배경색·흐름과 CircuitLayout을 사용합니다. Color field의 재질은 저장된 시작 화면이며 실행용 복사본만 움직입니다.",MessageType.Info);
        if(GUILayout.Button("배경 색·흐름·회로 대비 편집"))Selection.activeObject=view.Profile;
        if(view.Profile!=null&&GUILayout.Button("회로 경로 편집"))Selection.activeObject=view.Profile.circuitLayout;
        connections=EditorGUILayout.Foldout(connections,"표시 연결·옛 PNG 비교 전용",true);
        if(connections){EditorGUILayout.HelpBox("Comparison Decoration은 비교/미지원 복구 전용입니다. 기본 동적 회로와 겹쳐 표시하지 않습니다.",MessageType.None);DrawDefaultInspector();}
    }
}

[CustomEditor(typeof(CursorDashNexusView))]
public sealed class CursorDashNexusViewEditor:Editor
{
    public override void OnInspectorGUI()
    {
        var view=(CursorDashNexusView)target;
        EditorGUILayout.HelpBox("위치는 NexusPlacement, 기본 본체 색은 Energy Tint입니다. HpBar의 Image/TMP가 아닌 이 색을 실행 중에도 사용합니다. 체력·회전·피버는 Play 연출입니다.",MessageType.Info);
        using(new EditorGUILayout.HorizontalScope()){
            if(GUILayout.Button("위치·크기"))Selection.activeGameObject=view.PlacementRoot.gameObject;
            if(GUILayout.Button("본체 Image"))Selection.activeGameObject=view.Body.gameObject;
            if(GUILayout.Button("점선 Image"))Selection.activeGameObject=view.Dotted.gameObject;
        }
        DrawDefaultInspector();
    }
}
