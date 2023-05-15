using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class StreamingBindDataDrawer : OdinValueDrawer<StreamingBindData>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        StreamingBindData streamingBindData = ValueEntry.SmartValue;

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("匹配类型");
                GUI.color = Color.green;
                if (GUILayout.Button(streamingBindData.typeString.typeName)) TypeStringSelectHelper.DrawTypeStringSelect((select) => { streamingBindData.typeString = select; });
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("顺序");
            streamingBindData.sequence = SirenixEditorFields.IntField(streamingBindData.sequence, GUILayout.Width(100));

            GUILayout.Label("是否为else");
            streamingBindData.isElse = EditorGUILayout.Toggle(streamingBindData.isElse);
        }
        EditorGUILayout.EndHorizontal();
    }
}