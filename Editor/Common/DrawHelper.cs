using UnityEditor;
using UnityEngine;

namespace UnityBindTool
{
    public static class DrawHelper
    {
        public static void DrawList(GUIStyle guiStyle, float operateHight = 30f)
        {
            //GUILayoutUtility.GetRect(content, style, options);
            Rect listRect = EditorGUILayout.BeginVertical(guiStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                float contentHight = listRect.height - operateHight;

            }
            EditorGUILayout.EndHorizontal();
        }
    }
}