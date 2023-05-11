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
        BindData findData = bindWindow.editorObjectInfo.bindDataList.Find((info) => info.GetGameObject() == go || CommonTools.GetPrefabAsset(go) == info.GetGameObject());
        if (findData == null) return;
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
        List<BindData> bindList = new List<BindData>();
        ObjectInfo objectInfo = bindWindow.editorObjectInfo;
        int amount = objectInfo.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = objectInfo.bindDataList[i];
            if (! bindData.GameObjectEquals(go)) continue;
            bindList.Add(bindData);
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

        BindData bindData = BindDataHelper.CreateBindData(go);
        bindData.SetBindInfo<BindComponent>();
        TypeString[] typeStrings = bindData.bindTarget.GetTypeStrings();

        int typeAmount = typeStrings.Length;
        for (int i = 0; i < typeAmount; i++)
        {
            TypeString typeString = typeStrings[i];
            menu.AddItem(new GUIContent(typeString.typeName), false, (index) => {
                bindData.index = (int) index;
                ObjectInfoHelper.BindDataToObjectInfo(bindWindow.editorObjectInfo, bindData, bindWindow.bindSetting.selectCompositionSetting);
                bindWindow.SearchSelectList();
                bindWindow.Repaint();
            }, i);
        }

        menu.ShowAsContext();
    }

    static void HierarchyLook(List<BindData> bindList)
    {
        GenericMenu menu = new GenericMenu();
        int bindAmount = bindList.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindData info = bindList[i];
            menu.AddItem(new GUIContent(info.GetTypeName()), false, BindWindowLook, info);
        }
        menu.ShowAsContext();
    }

    static void BindWindowLook(object bindInfo)
    {
        bindWindow.bindWindowState = BindWindowState.BindInfoListGUI;
        bindWindow.bindTypeIndex = BindTypeIndex.Item;
        bindWindow.selectBindDataList.Add((BindData) bindInfo);
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