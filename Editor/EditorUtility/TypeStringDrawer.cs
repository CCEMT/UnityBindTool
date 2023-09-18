using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace UnityBindTool
{
    public class TypeStringDrawer : OdinValueDrawer<TypeString>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            TypeString typeString = ValueEntry.SmartValue;

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(label);
                GUI.color = Color.green;
                if (GUILayout.Button(typeString.typeName)) TypeStringSelectHelper.DrawTypeStringSelect((select) => { ValueEntry.SmartValue = select; });
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}