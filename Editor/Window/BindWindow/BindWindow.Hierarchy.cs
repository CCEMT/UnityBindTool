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
        BindInfo(id, rect);
        BindOperate(id, rect);
        ExamineBind(id, rect);
    }

    static void BindInfo(int id, Rect rect)
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
        }
        else
        {
            ComponentBindInfo findInfo = bindWindow.editorObjectInfo.gameObjectBindInfoList.Find((bindInfo) => {
                if (bindInfo.instanceObject == go || CommonTools.GetPrefabAsset(go) == bindInfo.instanceObject) { return true; }
                else { return false; }
            });

            if (findInfo == null) return;
            Rect r = new Rect(rect);
            r.x = 34;
            r.width = 80;
            GUIStyle style = new GUIStyle();
            if (CommonTools.GetIsParent(go.transform, bindWindow.bindObject))
            {
                style.normal.textColor = Color.yellow;
                GUI.Label(r, "★", style);
            }
            else
            {
                style.normal.textColor = Color.white;
                GUI.Label(r, "★", style);
            }
        }
    }

    static void BindOperate(int id, Rect rect)
    {
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
        if (! GUI.Button(rect, "绑定", style)) return;
        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

        ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);
        int typeAmount = componentBindInfo.typeStrings.Length;
        for (int i = 0; i < typeAmount; i++)
        {
            menu.AddItem(new GUIContent(componentBindInfo.typeStrings[i].typeName), false, (index) => {
                componentBindInfo.index = (int) index;
                ObjectInfoHelper.BindComponent(bindWindow.editorObjectInfo, componentBindInfo, bindWindow.bindSetting.selectCompositionSetting);
                bindWindow.Repaint();
            }, i);
        }

        menu.ShowAsContext(); //显示菜单
    }

    static void ExamineBind(int id, Rect rect)
    {
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
        {
            Rect lookRect = new Rect(rect);

            float lookWidth = 30f;
            float lookHeight = 17.5f;
            lookRect.x += rect.width - lookWidth - 30f;
            lookRect.width = lookWidth;
            lookRect.height = lookHeight;
            GUIStyle lookStyle = new GUIStyle();
            lookStyle.fontSize = 12;
            lookStyle.normal.textColor = Color.white;
            if (GUI.Button(lookRect, "查看", lookStyle))
            {
                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                int bindAmount = bindList.Count;
                for (int i = 0; i < bindAmount; i++)
                {
                    ComponentBindInfo info = bindList[i];
                    menu.AddItem(new GUIContent(info.GetTypeName()), false, () => {
                        bindWindow.bindWindowState = BindWindowState.BindInfoListGUI;
                        bindWindow.bindTypeIndex = BindTypeIndex.Item;
                        bindWindow.selectComponentList.Clear();
                        bindWindow.selectDataList.Clear();
                        bindWindow.selectComponentList.Add(info);
                    }); //向菜单中添加菜单项
                }
                menu.ShowAsContext(); //显示菜单
            }

            Rect deleteRect = new Rect(rect);
            float deleteWidth = 30f;
            float deleteHeight = 17.5f;
            deleteRect.x += rect.width - deleteWidth - lookWidth - 30f;
            deleteRect.width = deleteHeight;
            deleteRect.height = deleteHeight;

            GUIStyle deleteStyle = new GUIStyle();
            deleteStyle.fontSize = 12;
            deleteStyle.normal.textColor = Color.red;
            if (! GUI.Button(deleteRect, "解除", deleteStyle)) return;
            {
                List<RemoveType> removeTypes = new List<RemoveType>();
                removeTypes.Add(RemoveType.This);

                Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
                if (transforms.Length > 1)
                {
                    removeTypes.Add(RemoveType.Child);
                    removeTypes.Add(RemoveType.ThisAndChild);
                }

                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                int selectAmount = removeTypes.Count;
                for (int i = 0; i < selectAmount; i++)
                {
                    RemoveType removeType = removeTypes[i];
                    menu.AddItem(new GUIContent(LabelHelper.GetRemoveString(removeType)), false, () => { ObjectInfoHelper.RemoveBindInfo(bindWindow.editorObjectInfo, go, removeType); });
                }

                menu.ShowAsContext(); //显示菜单
            }
        }
    }
}