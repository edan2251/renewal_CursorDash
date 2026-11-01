using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CursorDashCircuitLayout))]
public sealed class CursorDashCircuitLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("원점은 화면 중앙입니다. X/Y는 기준 화면의 정규화 좌표이며 Y+는 위입니다. 점 개수/순서와 분기 연결은 보존하고 좌표만 수정하세요. Tools → CursorDash → 화면 편집에서 실제 경로를 확인합니다.",MessageType.Info);
        DrawDefaultInspector();
        try
        {
            var graph=((CursorDashCircuitLayout)target).CreateGraph(1,1,12);
            EditorGUILayout.LabelField("연결 구조",graph.Roots.Length+" roots / "+graph.Edges.Length+" edges / "+graph.Points.Length+" points");
            for(int i=0;i<graph.Roots.Length;i++)EditorGUILayout.LabelField("시작점 "+graph.Roots[i],graph.Names[i]);
        }
        catch(Exception error){EditorGUILayout.HelpBox(error.Message+" 정상 좌표로 수정하거나 해당 편집만 Undo 하세요.",MessageType.Error);}
    }
}
