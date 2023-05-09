#region Using

using System.Collections.Generic;
using BindTool;
using UnityEditor;
using UnityEngine;

#endregion

public partial class BindWindow
{
    [InitializeOnLoadMethod]
    static void Initilalize()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= SetShow;
        EditorApplication.hierarchyWindowItemOnGUI += SetShow;
    }

    static void SetShow(int id, Rect rect)
    {
        if (bindWindow == null) return;
        DrawBindInfo(id, rect);
        DrawBindOperate(id, rect);
        DrawExamineBind(id, rect);
    }

    static void DrawBindInfo(int id, Rect rect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
        if (go == null) return;
        if (go == bindWindow.bindObject)
        {
            Rect r = new Rect(rect);
            r.x = 34;
            r.width = 80;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            GUI.Label(r, "★", style);
            return;
        }
        ComponentBindInfo findInfo = bindWindow.editorObjectInfo.gameObjectBindInfoList.Find((info) => info.instanceObject == go || CommonTools.GetPrefabAsset(go) == info.instanceObject);
        if (findInfo == null) return;
        Rect targetRect = new Rect(rect);
        targetRect.x = 34;
        targetRect.width = 80;
        GUIStyle targetStyle = new GUIStyle();
        if (CommonTools.GetIsParent(go.transform, bindWindow.bindObject))
        {
            targetStyle.normal.textColor = Color.yellow;
            GUI.Label(targetRect, "★", targetStyle);
        }
        else
        {
            targetStyle.normal.textColor = Color.white;
            GUI.Label(targetRect, "★", targetStyle);
        }
    }

    static void DrawBindOperate(int id, Rect rect)
    {
        if (Selection.objects.Length > 1) return;
        if (! Selection.activeObject || id != Selection.activeObject.GetInstanceID()) return;
        GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
        if (go == null) return;
        float width = 30f;
        float height = 17.5f;
        rect.x += rect.width - width;
        rect.width = width;
        rect.height = height;
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.green;
        if (GUI.Button(rect, "绑定", style)) HierarchyBind(go);

    }

    static void DrawExamineBind(int id, Rect rect)
    {
        if (Selection.objects.Length > 1) return;
        if (! Selection.activeObject || id != Selection.activeObject.GetInstanceID()) return;
        GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
        if (go == null) return;
        List<ComponentBindInfo> bindList = new List<ComponentBindInfo>();
        List<int> bindIndex = new List<int>();
        ObjectInfo objectInfo = bindWindow.editorObjectInfo;
        int amount = objectInfo.gameObjectBindInfoList.Count;
        for (int i = 0; i < amount; i++)
        {
            ComponentBindInfo info = objectInfo.gameObjectBindInfoList[i];
            if (! info.GameObjectEquals(go)) continue;
            bindList.Add(info);
            bindIndex.Add(i);
        }

        if (bindList.Count <= 0) return;
        Rect lookRect = new Rect(rect);

        float lookWidth = 30f;
        float lookHeight = 17.5f;
        lookRect.x += rect.width - lookWidth - 30f;
        lookRect.width = lookWidth;
        lookRect.height = lookHeight;
        GUIStyle lookStyle = new GUIStyle();
        lookStyle.fontSize = 12;
        lookStyle.normal.textColor = Color.white;
        if (GUI.Button(lookRect, "查看", lookStyle)) HierarchyLook(bindList);

        Rect deleteRect = new Rect(rect);
        float deleteWidth = 30f;
        float deleteHeight = 17.5f;
        deleteRect.x += rect.width - deleteWidth - lookWidth - 30f;
        deleteRect.width = deleteHeight;
        deleteRect.height = deleteHeight;

        GUIStyle deleteStyle = new GUIStyle();
        deleteStyle.fontSize = 12;
        deleteStyle.normal.textColor = Color.red;
        if (GUI.Button(deleteRect, "解除", deleteStyle)) HierarchyRemove(go);
    }

    static void HierarchyBind(GameObject go)
    {
        GenericMenu menu = new GenericMenu();

        ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);
        int typeAmount = componentBindInfo.typeStrings.Length;
        for (int i = 0; i < typeAmount; i++)
        {
            menu.AddItem(new GUIContent(componentBindInfo.typeStrings[i].typeName), false, (index) => {
                componentBindInfo.index = (int) index;
                ObjectInfoHelper.BindComponent(bindWindow.editorObjectInfo, componentBindInfo, bindWindow.bindSetting.selectCompositionSetting);
                bindWindow.SearchSelectList();
                bindWindow.Repaint();
            }, i);
        }

        menu.ShowAsContext();
    }

    static void HierarchyLook(List<ComponentBindInfo> bindList)
    {
        GenericMenu menu = new GenericMenu();
        int bindAmount = bindList.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            ComponentBindInfo info = bindList[i];
            menu.AddItem(new GUIContent(info.GetTypeName()), false, BindWindowLook, info);
        }
        menu.ShowAsContext();
    }

    static void BindWindowLook(object bindInfo)
    {
        bindWindow.bindWindowState = BindWindowState.BindInfoListGUI;
        bindWindow.bindTypeIndex = BindTypeIndex.Item;
        bindWindow.selectComponentList.Clear();
        bindWindow.selectDataList.Clear();
        bindWindow.selectComponentList.Add((ComponentBindInfo) bindInfo);
        bindWindow.Repaint();
    }

    static void HierarchyRemove(GameObject go)
    {
        List<RemoveType> removeTypes = new List<RemoveType>();
        removeTypes.Add(RemoveType.This);

        Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
        if (transforms.Length > 1)
        {
            removeTypes.Add(RemoveType.Child);
            removeTypes.Add(RemoveType.ThisAndChild);
        }

        GenericMenu menu = new GenericMenu();

        int selectAmount = removeTypes.Count;
        for (int i = 0; i < selectAmount; i++)
        {
            RemoveType removeType = removeTypes[i];
            menu.AddItem(new GUIContent(LabelHelper.GetRemoveString(removeType)), false, Remove, removeType);
        }

        void Remove(object removeType)
        {
            ObjectInfoHelper.RemoveBindInfo(bindWindow.editorObjectInfo, go, (RemoveType) removeType);
            bindWindow.SearchSelectList();
            bindWindow.Repaint();
        }

        menu.ShowAsContext();
    }
}