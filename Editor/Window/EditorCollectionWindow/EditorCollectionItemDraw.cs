using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class EditorCollectionItemDraw : OdinValueDrawer<EditorCollectionItemDrawData>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        EditorCollectionItemDrawData data = ValueEntry.SmartValue;

        EditorGUILayout.BeginHorizontal();
        {
            SirenixEditorFields.UnityObjectField(data.drawData.GetValue(), data.drawData.GetTypeString().ToType(), true);
            if (GUILayout.Button("移除")) { data.removeCallback?.Invoke(data); }
        }
        EditorGUILayout.EndHorizontal();
    }
}