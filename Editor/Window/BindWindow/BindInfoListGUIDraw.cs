using UnityEditor;
using UnityEngine;

public partial class BindWindow
{
    void BindInfoListGUIDraw()
    {
        DrawOperate();
        DrawBindArea();
        DrawBindInfo();
    }

    void DrawOperate()
    {
        EditorGUILayout.BeginVertical("frameBox");
        {
            DrawBuild();
            DrawBind();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawBuild()
    {
        if (GUILayout.Button("生成")) { }
    }

    void DrawBind()
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("绑定所有")) { }

            if (GUILayout.Button("自动绑定")) { }

            if (GUILayout.Button("解除所有")) { }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawBindArea()
    {
        Event currentEvent = Event.current;
        Rect dragArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true), GUILayout.Height(50));

        GUI.color = Color.green;
        GUI.Box(dragArea, "将组件拖拽至此进行绑定");
        GUI.color = Color.white;

        if (currentEvent.type is not (EventType.DragUpdated or EventType.DragPerform)) return;
        if (! dragArea.Contains(currentEvent.mousePosition)) return;
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (currentEvent.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            Object[] bindTargets = DragAndDrop.objectReferences;
            BindHelper.BindTarget(this.editorObjectInfo, bindTargets);
        }
        currentEvent.Use();
    }

    void DrawBindInfo()
    {
        DrawSearch();
        DrawBindList();
    }

    void DrawSearch() { }

    void DrawBindList() { }
}