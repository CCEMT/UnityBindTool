#region Using

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#endregion

namespace BindTool
{
    public partial class BindWindown
    {
        [InitializeOnLoadMethod]
        static void Initilalize()
        {
            EditorApplication.hierarchyWindowItemOnGUI = SetShow;
        }

        static void SetShow(int id, Rect rect)
        {
            if (bindWindown != null && bindWindown.commonSettingData != null && bindWindown.commonSettingData.isCustomBind && bindWindown.bindObject != null)
            {
                BindInfo(id, rect);
                BindOperate(id, rect);
                ExamineBind(id, rect);
            }
        }

        static void BindInfo(int id, Rect rect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
            if (go != null)
            {
                if (go == bindWindown.bindObject)
                {
                    var r = new Rect(rect);
                    r.x = 34;
                    r.width = 80;
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;
                    GUI.Label(r, "★", style);
                }
                else
                {
                    var findInfo = bindWindown.objectInfo.gameObjectBindInfoList.Find((bindInfo) => {
                        if (bindInfo.instanceObject == go || CommonTools.GetPrefabAsset(go) == bindInfo.instanceObject) { return true; }
                        else { return false; }
                    });
                 
                    if (findInfo != null)
                    {
                        var r = new Rect(rect);
                        r.x = 34;
                        r.width = 80;
                        GUIStyle style = new GUIStyle();
                        if (CommonTools.GetIsParent(go.transform, bindWindown.bindObject))
                        {
                            style.normal.textColor = Color.yellow;
                            GUI.Label(r, "★", style);
                        }
                        else {
                            style.normal.textColor = Color.white;
                            GUI.Label(r, "★", style);
                        }
                    }
                }
            }
        }

        static void BindOperate(int id, Rect rect)
        {
            if (Selection.activeObject && id == Selection.activeObject.GetInstanceID())
            {
                GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
                if (go != null)
                {
                    float width = 50f;
                    float height = 17.5f;
                    rect.x += rect.width - width;
                    rect.width = width;
                    rect.height = height;
                    if (GUI.Button(rect, "绑定"))
                    {
                        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                        ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);
                        int typeAmount = componentBindInfo.typeStrings.Length;
                        for (int i = 0; i < typeAmount; i++)
                        {
                            var index = i;
                            menu.AddItem(new GUIContent(componentBindInfo.typeStrings[i].typeName), false, () => {
                                bindWindown.objectInfo.Bind(componentBindInfo, index);
                                if (bindWindown.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                {
                                    componentBindInfo.name = CommonTools.GetNumberAlpha(componentBindInfo.instanceObject.name);
                                }
                            }); //向菜单中添加菜单项
                        }

                        menu.ShowAsContext(); //显示菜单
                    }
                }
            }
        }

        static void ExamineBind(int id, Rect rect)
        {
            if (Selection.activeObject && id == Selection.activeObject.GetInstanceID())
            {
                GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
                if (go != null)
                {
                    List<ComponentBindInfo> bindList = new List<ComponentBindInfo>();
                    List<int> bindIndex = new List<int>();
                    ObjectInfo objectInfo = bindWindown.objectInfo;
                    int amount = objectInfo.gameObjectBindInfoList.Count;
                    for (int i = 0; i < amount; i++)
                    {
                        ComponentBindInfo info = objectInfo.gameObjectBindInfoList[i];
                        if (info.GameObjectEquals(go))
                        {
                            bindList.Add(info);
                            bindIndex.Add(i);
                        }
                    }

                    if (bindList.Count > 0)
                    {
                        float width = 75f;
                        float height = 17.5f;
                        rect.x += rect.width - width - 50f;
                        rect.width = width;
                        rect.height = height;
                        if (GUI.Button(rect, "查看绑定"))
                        {
                            GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                            int bindAmount = bindList.Count;
                            for (int i = 0; i < bindAmount; i++)
                            {
                                ComponentBindInfo info = bindList[i];
                                int index = bindIndex[i];
                                menu.AddItem(new GUIContent(info.GetTypeName()), false, () => { bindWindown.SelectBindInfo(index); }); //向菜单中添加菜单项
                            }
                            menu.ShowAsContext(); //显示菜单
                        }
                    }
                }
            }
        }
    }
}