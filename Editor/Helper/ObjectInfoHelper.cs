using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BindTool;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ObjectInfoHelper
{
    public static SyntaxKind VisitTypeToSyntaxKind(VisitType visitType)
    {
        switch (visitType)
        {
            case VisitType.Public:
                return SyntaxKind.PublicKeyword;
            case VisitType.Private:
                return SyntaxKind.PrivateKeyword;
            case VisitType.Protected:
                return SyntaxKind.ProtectedKeyword;
            case VisitType.Internal:
                return SyntaxKind.InternalKeyword;
        }

        return default;
    }

    public static ObjectInfo GetObjectInfo(GameObject bindObject)
    {
        ObjectInfo objectInfo = null;

        BindComponents bindComponents = bindObject.GetComponent<BindComponents>();

        if (bindComponents != null)
        {
            if (bindComponents.bindRoot == null) { Object.DestroyImmediate(bindComponents); }
            else
            {
                objectInfo = new ObjectInfo();
                objectInfo.typeString = new TypeString(bindComponents.bindRoot.GetType());
                objectInfo.rootBindInfo = new ComponentBindInfo(bindObject);
                objectInfo.rootBindInfo.name = objectInfo.typeString.typeName;

                Type type = bindComponents.bindRoot.GetType();
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    AutoGenerate attribute = (AutoGenerate) fieldInfo.GetCustomAttribute(typeof(AutoGenerate), true);
                    if (attribute == null || attribute.autoGenerateType != AutoGenerateType.OriginalField) continue;
                    Object bindComponent = (Object) fieldInfo.GetValue(bindComponents.bindRoot);
                    if (bindComponent is GameObject == false && bindComponent.GetType().IsSubclassOf(typeof(Component)) == false)
                    {
                        DataBindInfo dataBindInfo = new DataBindInfo(bindComponent);
                        dataBindInfo.name = fieldInfo.Name;
                        objectInfo.dataBindInfoList.Add(dataBindInfo);
                    }
                    else
                    {
                        ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindComponent);
                        componentBindInfo.name = fieldInfo.Name;
                        componentBindInfo.SetIndex(new TypeString(fieldInfo.FieldType));
                        objectInfo.gameObjectBindInfoList.Add(componentBindInfo);
                    }
                }
            }

        }

        if (objectInfo != null) return objectInfo;
        objectInfo = new ObjectInfo();
        //objectInfo.SetRootObject(bindObject);

        return objectInfo;
    }

    public static void ClearAllBind(ObjectInfo objectInfo)
    {
        objectInfo.gameObjectBindInfoList.Clear();
        objectInfo.dataBindInfoList.Clear();
        objectInfo.componentCollectionBindInfoList.Clear();
        objectInfo.dataCollectionBindInfoList.Clear();
    }

    public static void BindAll(ObjectInfo objectInfo, GameObject target, CompositionSetting compositionSetting)
    {
        objectInfo.gameObjectBindInfoList.Clear();
        Transform[] gameObjects = target.GetComponentsInChildren<Transform>(true);
        int amount = gameObjects.Length;
        for (int i = 0; i < amount; i++)
        {
            Transform go = gameObjects[i];
            int componentAmount = new ComponentBindInfo(go.gameObject).typeStrings.Length;
            for (int j = 0; j < componentAmount; j++)
            {
                ComponentBindInfo info = new ComponentBindInfo(go.gameObject);
                info.index = j;
                BindComponent(objectInfo, info, compositionSetting);
            }
        }
    }

    public static void ObjectInfoAutoBind(ObjectInfo objectInfo, GameObject bindObject, CompositionSetting compositionSetting)
    {
        Transform[] gameObjects = bindObject.GetComponentsInChildren<Transform>(true);
        int amount = gameObjects.Length;
        for (int i = 0; i < amount; i++)
        {
            Transform go = gameObjects[i];
            StartAutoBind(objectInfo, go, compositionSetting);
        }
    }

    // public void AutoBind(ComponentBindInfo bindInfo, AutoBindSetting autoBindSetting)
    // {
    //     BindByAutoSetting(bindInfo, autoBindSetting);
    // }
    //
    static void StartAutoBind(ObjectInfo objectInfo, Object target, CompositionSetting autoBindSetting)
    {
        NameLgnoreSetting lgnoreSetting = autoBindSetting.autoBindSetting.nameLgnoreSetting;

        if (lgnoreSetting.isEnable)
        {
            if (target == null) return;
            int lgnoreAmount = lgnoreSetting.nameLgnoreDataList.Count;
            for (int i = 0; i < lgnoreAmount; i++)
            {
                NameCheck data = lgnoreSetting.nameLgnoreDataList[i];
                if (NameHelper.NameCheckContent(data, target.name, out string _)) return;
            }
        }
        StartNameBind(objectInfo, target, autoBindSetting);
    }

    public static void StartNameBind(ObjectInfo objectInfo, Object target, CompositionSetting setting)
    {
        NameBindSetting nameBindSetting = setting.autoBindSetting.nameBindSetting;

        if (nameBindSetting.isEnable)
        {
            List<NameBindData> canNameData = new List<NameBindData>();

            int nameBindAmont = nameBindSetting.nameBindDataList.Count;
            for (int i = 0; i < nameBindAmont; i++)
            {
                NameBindData data = nameBindSetting.nameBindDataList[i];

                if (! NameHelper.NameCheckContent(data.nameCheck, target.name, out string _)) continue;
                canNameData.Add(data);
            }

            int canNameAmount = canNameData.Count;
            for (int i = 0; i < canNameAmount; i++)
            {
                NameBindData data = canNameData[i];
                ComponentBindInfo componentBindInfo = new ComponentBindInfo(target);
                int index = componentBindInfo.SetIndex(data.typeString);
                if (index == -1) continue;
                BindComponent(objectInfo, componentBindInfo, setting);
            }

        }
        StreamingBind(objectInfo, target, setting);
    }

    static void StreamingBind(ObjectInfo objectInfo, Object target, CompositionSetting setting)
    {
        StreamingBindSetting streamingBindSetting = setting.autoBindSetting.streamingBindSetting;

        if (streamingBindSetting.isEnable)
        {
            ComponentBindInfo bindInfo = new ComponentBindInfo(target);

            List<TypeString> tempTypeList = new List<TypeString>();
            tempTypeList.AddRange(bindInfo.typeStrings);

            List<TypeString> elseType = new List<TypeString>();
            elseType.AddRange(tempTypeList);
            elseType.Remove(new TypeString(typeof(GameObject)));
            streamingBindSetting.streamingBindDataList = streamingBindSetting.streamingBindDataList.OrderByDescending(x => x.sequence).ToList();
            int sequenceAmount = streamingBindSetting.streamingBindDataList.Count;
            for (int i = 0; i < sequenceAmount; i++)
            {
                StreamingBindData data = streamingBindSetting.streamingBindDataList[i];
                if (tempTypeList.Contains(data.typeString)) elseType.Remove(data.typeString);
            }

            for (int i = 0; i < sequenceAmount; i++)
            {
                StreamingBindData data = streamingBindSetting.streamingBindDataList[i];
                if (data.isElse)
                {
                    if (elseType.Count <= 0) continue;
                    bindInfo.SetIndex(elseType.First());
                    break;
                }
                else
                {
                    if (! tempTypeList.Contains(data.typeString)) continue;
                    bindInfo.SetIndex(data.typeString);
                    break;
                }
            }

            BindComponent(objectInfo, bindInfo, setting);
        }
        else
        {
            if (! streamingBindSetting.isBindComponent) return;
            if (! streamingBindSetting.isBindAllComponent) return;

            ComponentBindInfo bindInfo = new ComponentBindInfo(target);

            int amount = bindInfo.typeStrings.Length;
            for (int i = 0; i < amount; i++)
            {
                ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindInfo.instanceObject);
                componentBindInfo.index = i;
                BindComponent(objectInfo, componentBindInfo, setting);
            }
        }
    }

    // public void Bind(ComponentBindInfo bindInfo, int index)
    // {
    //     bindInfo.index = index;
    //     gameObjectBindInfoList.Add(bindInfo);
    // }

    // public void SetRootObject(GameObject instanceObject)
    // {
    //     rootBindInfo = new ComponentBindInfo(instanceObject);
    // }

    // public void AgainGet()
    // {
    //     if (rootBindInfo.AgainGet() == false)
    //     {
    //         if (rootBindInfo.autoBindSetting != null) ComponentBind(rootBindInfo, rootBindInfo.autoBindSetting);
    //     }
    //
    //     int amount = gameObjectBindInfoList.Count;
    //     for (int i = 0; i < amount; i++)
    //     {
    //         ComponentBindInfo info = gameObjectBindInfoList[i];
    //         if (info.AgainGet() != false) continue;
    //         if (info.autoBindSetting != null) ComponentBind(info, info.autoBindSetting);
    //     }
    // }

    // public ComponentBindInfo AddObject(GameObject instanceObject)
    // {
    //     ComponentBindInfo componentBindInfo = new ComponentBindInfo(instanceObject);
    //     gameObjectBindInfoList.Add(componentBindInfo);
    //     return componentBindInfo;
    // }
    //
    // public bool GameObjectEquals(object obj)
    // {
    //     if (obj == null) return false;
    //     GameObject targetObject = obj as GameObject;
    //     if (targetObject == null) return false;
    //     if (targetObject == this.rootBindInfo.instanceObject) { return true; }
    //     else if (targetObject == this.rootBindInfo.prefabObject) return true;
    //
    //     GameObject targetPrefab = CommonTools.GetPrefabAsset(targetObject);
    //     GameObject currentPrefab = CommonTools.GetPrefabAsset(this.rootBindInfo.GetObject());
    //     if (targetPrefab == currentPrefab) return true;
    //     return false;
    // }

    public static void BindComponent(ObjectInfo objectInfo, ComponentBindInfo info, CompositionSetting setting)
    {
        if (setting.nameGenerateSetting.isBindAutoGenerateName) info.name = NameHelper.SetVariableName(info.instanceObject.name, setting.nameGenerateSetting);
        info.name = NameHelper.NameSettingByName(info, setting.scriptSetting.nameSetting);
        info.name = CommonTools.GetNumberAlpha(info.name);
        if (objectInfo.gameObjectBindInfoList.Contains(info) == false) objectInfo.gameObjectBindInfoList.Add(info);
    }

    public static void BindData(ObjectInfo objectInfo, DataBindInfo info, CompositionSetting setting)
    {
        if (setting.nameGenerateSetting.isBindAutoGenerateName) info.name = NameHelper.SetVariableName(info.bindObject.name, setting.nameGenerateSetting);
        info.name = NameHelper.NameSettingByName(info, setting.scriptSetting.nameSetting);
        info.name = CommonTools.GetNumberAlpha(info.name);
        if (objectInfo.dataBindInfoList.Contains(info) == false) objectInfo.dataBindInfoList.Add(info);
    }

    public static void RemoveBindInfo(ObjectInfo objectInfo, GameObject removeObject, RemoveType removeType)
    {
        switch (removeType)
        {
            case RemoveType.This:
            {
                int thisBindInfoAmount = objectInfo.gameObjectBindInfoList.Count;
                for (int i = thisBindInfoAmount - 1; i >= 0; i--)
                {
                    ComponentBindInfo componentBindInfo = objectInfo.gameObjectBindInfoList[i];
                    if (componentBindInfo.instanceObject == removeObject) objectInfo.gameObjectBindInfoList.RemoveAt(i);
                }
                break;
            }
            case RemoveType.Child:
            {
                Transform[] transforms = removeObject.GetComponentsInChildren<Transform>(true);
                int amount = transforms.Length;
                for (int i = 0; i < amount; i++)
                {
                    Transform transform = transforms[i];
                    if (transform.gameObject == removeObject) continue;
                    int childBindInfoAmount = objectInfo.gameObjectBindInfoList.Count;
                    for (int j = childBindInfoAmount - 1; j >= 0; j--)
                    {
                        ComponentBindInfo componentBindInfo = objectInfo.gameObjectBindInfoList[j];
                        if (componentBindInfo.instanceObject == transform.gameObject) objectInfo.gameObjectBindInfoList.RemoveAt(j);
                    }
                }
                break;
            }
            case RemoveType.ThisAndChild:
            {
                Transform[] transforms = removeObject.GetComponentsInChildren<Transform>(true);
                int amount = transforms.Length;
                for (int i = 0; i < amount; i++)
                {
                    Transform transform = transforms[i];
                    int childBindInfoAmount = objectInfo.gameObjectBindInfoList.Count;
                    for (int j = childBindInfoAmount - 1; j >= 0; j--)
                    {
                        ComponentBindInfo componentBindInfo = objectInfo.gameObjectBindInfoList[j];
                        if (componentBindInfo.instanceObject == transform.gameObject) objectInfo.gameObjectBindInfoList.RemoveAt(j);
                    }
                }
                break;
            }
        }
    }
}