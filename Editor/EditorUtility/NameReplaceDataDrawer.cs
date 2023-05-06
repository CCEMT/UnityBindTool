using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class NameReplaceDataDrawer : OdinValueDrawer<NameReplaceData>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {

        NameReplaceData nameReplaceData = ValueEntry.SmartValue;

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            {
                SirenixEditorFields.TextField("替换名称", nameReplaceData.targetName);
                SirenixEditorFields.TextField("检查名称", nameReplaceData.nameCheck.name);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.Toggle("是否大小写", nameReplaceData.nameCheck.nameRule.isCaseSensitive);

                OdinHelper.DrawOdinEnum("匹配规则", nameReplaceData.nameCheck.nameRule.nameMatchingRule, (result) => { nameReplaceData.nameCheck.nameRule.nameMatchingRule = (NameMatchingRule) result; });
            }
            EditorGUILayout.EndVertical();

        }
        EditorGUILayout.EndHorizontal();
    }
}