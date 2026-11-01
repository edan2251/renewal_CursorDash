using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Clarifies existing defaults without deleting fields, changing presets, or introducing a second settings system.</summary>
[CustomEditor(typeof(CursorDashVisualProfile))]
public sealed class CursorDashVisualProfileEditor : Editor
{
    private bool showLegacy;
    private static readonly HashSet<string> ViewFallbacks=new HashSet<string>{
        "mainLogo","spaceLogoWidth","spaceLogoY","spaceMainScoreScale","spaceMainScoreOpacity","spaceMainScoreLift",
        "backgroundScoreX","backgroundScoreY","backgroundScoreTint","backgroundScoreOpacity"
    };
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Cyberpunk 배치/글꼴/기본 색은 실제 View가 정식 원본입니다. 이 자산은 배경·회로·전환·VFX 반응을 담당합니다. 공격/점수 규칙이나 사용자 저장값을 변경하는 창이 아닙니다.",MessageType.Info);
        showLegacy=EditorGUILayout.Toggle("미연결 씬의 호환 기본값 표시",showLegacy);
        serializedObject.Update();var property=serializedObject.GetIterator();bool enter=true;
        while(property.NextVisible(enter))
        {
            enter=false;if(!showLegacy&&ViewFallbacks.Contains(property.name))continue;
            using(new EditorGUI.DisabledScope(property.name=="m_Script"))EditorGUILayout.PropertyField(property,true);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
