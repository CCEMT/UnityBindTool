using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace UnityBindTool
{
    public class NameCheckDrawer : OdinValueDrawer<NameCheck>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            NameCheck nameCheck = ValueEntry.SmartValue;

            EditorGUILayout.BeginHorizontal();
            {
                nameCheck.name = SirenixEditorFields.TextField("检查名称", nameCheck.name);

                nameCheck.nameRule.isCaseSensitive = EditorGUILayout.Toggle("是否大小写", nameCheck.nameRule.isCaseSensitive);

                OdinHelper.DrawOdinEnum("匹配规则", nameCheck.nameRule.nameMatchingRule, (result) => { nameCheck.nameRule.nameMatchingRule = (NameMatchingRule) result; });
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}