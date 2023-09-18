using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace UnityBindTool
{
    public class NameBindDataDrawer : OdinValueDrawer<NameBindData>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            NameBindData nameBindData = ValueEntry.SmartValue;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("匹配类型");
                        GUI.color = Color.green;
                        if (GUILayout.Button(nameBindData.typeString.typeName)) TypeStringSelectHelper.DrawTypeStringSelect((select) => { nameBindData.typeString = select; });
                        GUI.color = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();

                    nameBindData.nameCheck.name = SirenixEditorFields.TextField("检查名称", nameBindData.nameCheck.name);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    nameBindData.nameCheck.nameRule.isCaseSensitive = EditorGUILayout.Toggle("是否大小写", nameBindData.nameCheck.nameRule.isCaseSensitive);

                    OdinHelper.DrawOdinEnum("匹配规则", nameBindData.nameCheck.nameRule.nameMatchingRule, (result) => { nameBindData.nameCheck.nameRule.nameMatchingRule = (NameMatchingRule) result; });
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndHorizontal();
        }
    }
}