using System.Collections.Generic;
using System.Linq;
using BindTool;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;

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
            objectInfo = new ObjectInfo();
            if (bindComponents.targetType == null) { Object.DestroyImmediate(bindComponents); }
            else
            {
                objectInfo = new ObjectInfo();
                objectInfo.typeString = new TypeString(bindComponents.targetType);

                BindData rootData = BindDataFactory.CreateBindData(bindObject);
                objectInfo.rootData = rootData;
                objectInfo.rootData.name = objectInfo.typeString.typeName;

                int bindAmount = bindComponents.bindDataList.Count;
                for (int i = 0; i < bindAmount; i++)
                {
                    Object bindTarget = bindComponents.bindDataList[i];
                    BindData bindData = BindDataFactory.CreateBindData(bindTarget);
                    bindData.name = bindComponents.bindName[i];
                    objectInfo.bindDataList.Add(bindData);
                }
            }

        }

        if (objectInfo != null) return objectInfo;
        objectInfo = new ObjectInfo();
        BindData newRootData = BindDataFactory.CreateBindData(bindObject);
        objectInfo.rootData = newRootData;

        return objectInfo;
    }

    public static void ClearAllBind(ObjectInfo objectInfo)
    {
        objectInfo.bindDataList.Clear();
        objectInfo.bindCollectionList.Clear();
    }

    public static void BindAll(ObjectInfo objectInfo, GameObject target, CompositionSetting compositionSetting)
    {
        objectInfo.bindDataList.Clear();
        Transform[] gameObjects = target.GetComponentsInChildren<Transform>(true);
        int amount = gameObjects.Length;
        for (int i = 0; i < amount; i++)
        {
            Transform go = gameObjects[i];
            Component[] components = go.gameObject.GetComponents<Component>();

            int componentAmount = components.Length;
            for (int j = 0; j < componentAmount; j++)
            {
                Component component = components[j];
                BindData bindData = BindDataFactory.CreateBindData(component);
                bindData.SetBindInfo<BindComponent>();
                bindData.index = j;
                BindDataToObjectInfo(objectInfo, bindData, compositionSetting);
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

        bool isBindSuccess = false;
        if (nameBindSetting.isEnable)
        {
            NameBindData nameBindData = null;
            int nameBindAmont = nameBindSetting.nameBindDataList.Count;
            for (int i = 0; i < nameBindAmont; i++)
            {
                NameBindData data = nameBindSetting.nameBindDataList[i];
                if (! NameHelper.NameCheckContent(data.nameCheck, target.name, out string _)) continue;
                nameBindData = data;
                break;
            }

            if (nameBindData != null)
            {
                isBindSuccess = true;
                BindData bindData = BindDataFactory.CreateBindData(target);
                bindData.SetBindInfo<BindComponent>();
                bindData.SetIndex(nameBindData.typeString);
                BindDataToObjectInfo(objectInfo, bindData, setting);
            }
        }
        if (isBindSuccess) return;
        StreamingBind(objectInfo, target, setting);
    }

    static void StreamingBind(ObjectInfo objectInfo, Object target, CompositionSetting setting)
    {
        StreamingBindSetting streamingBindSetting = setting.autoBindSetting.streamingBindSetting;

        if (streamingBindSetting.isEnable)
        {
            BindData bindData = BindDataFactory.CreateBindData(target);
            bindData.SetBindInfo<BindComponent>();
            TypeString[] targetTypeString = bindData.bindTarget.GetTypeStrings();

            List<TypeString> elseType = new List<TypeString>();
            elseType.AddRange(targetTypeString);
            elseType.Remove(new TypeString(typeof(GameObject)));
            streamingBindSetting.streamingBindDataList = streamingBindSetting.streamingBindDataList.OrderByDescending(x => x.sequence).ToList();
            int sequenceAmount = streamingBindSetting.streamingBindDataList.Count;
            for (int i = 0; i < sequenceAmount; i++)
            {
                StreamingBindData data = streamingBindSetting.streamingBindDataList[i];
                if (targetTypeString.Contains(data.typeString)) elseType.Remove(data.typeString);
            }

            for (int i = 0; i < sequenceAmount; i++)
            {
                StreamingBindData data = streamingBindSetting.streamingBindDataList[i];

                if (data.isElse)
                {
                    if (elseType.Count <= 0) continue;
                    bindData.SetIndex(elseType.First());
                    break;
                }
                else
                {
                    if (! targetTypeString.Contains(data.typeString)) continue;
                    bindData.SetIndex(data.typeString);
                    break;
                }
            }

            BindDataToObjectInfo(objectInfo, bindData, setting);
        }
        else
        {
            if (! streamingBindSetting.isBindComponent) return;
            if (! streamingBindSetting.isBindAllComponent) return;

            BindData bindData = BindDataFactory.CreateBindData(target);
            bindData.SetBindInfo<BindComponent>();
            TypeString[] targetTypeString = bindData.bindTarget.GetTypeStrings();

            int amount = targetTypeString.Length;
            for (int i = 0; i < amount; i++)
            {
                BindData targetBindData = BindDataFactory.CreateBindData(target);
                bindData.SetBindInfo<BindComponent>();
                targetBindData.index = i;
                BindDataToObjectInfo(objectInfo, targetBindData, setting);
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

    public static void BindDataToObjectInfo(ObjectInfo objectInfo, BindData info, CompositionSetting setting)
    {
        if (setting.nameGenerateSetting.isBindAutoGenerateName) BindDataSetName(info, setting);
        if (objectInfo.bindDataList.Contains(info) == false) objectInfo.bindDataList.Add(info);
    }

    public static void BindDataSetName(BindData info, CompositionSetting setting)
    {
        info.name = NameHelper.SetVariableName(info.GetValue().name, setting.nameGenerateSetting);
        info.name = NameHelper.NameSettingByName(info, setting.scriptSetting.nameSetting);
        info.name = CommonTools.GetNumberAlpha(info.name);
    }

    public static void RemoveBindInfo(ObjectInfo objectInfo, GameObject removeObject, RemoveType removeType)
    {
        switch (removeType)
        {
            case RemoveType.This:
            {
                int bindDataAmount = objectInfo.bindDataList.Count;
                for (int i = bindDataAmount - 1; i >= 0; i--)
                {
                    BindData bindData = objectInfo.bindDataList[i];
                    GameObject targetGameObject = bindData.GetGameObject();
                    if (targetGameObject == null) { continue; }
                    if (targetGameObject == removeObject) objectInfo.bindDataList.RemoveAt(i);
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
                    int bindDataAmount = objectInfo.bindDataList.Count;
                    for (int j = bindDataAmount - 1; j >= 0; j--)
                    {
                        BindData bindData = objectInfo.bindDataList[j];
                        GameObject targetGameObject = bindData.GetGameObject();
                        if (targetGameObject == null) { continue; }
                        if (targetGameObject == transform.gameObject) objectInfo.bindDataList.RemoveAt(j);
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
                    int bindDataAmount = objectInfo.bindDataList.Count;
                    for (int j = bindDataAmount - 1; j >= 0; j--)
                    {
                        BindData bindData = objectInfo.bindDataList[j];
                        GameObject targetGameObject = bindData.GetGameObject();
                        if (targetGameObject == null) { continue; }
                        if (targetGameObject == transform.gameObject) objectInfo.bindDataList.RemoveAt(j);
                    }
                }
                break;
            }
        }
    }

    public static void BindDataAddToBindCollection(ObjectInfo objectInfo, BindData bindData)
    {
        AddCollectionWindow.AddToCollection(objectInfo, new List<BindData>() {bindData}, bindData.GetAllTypeString().ToList());
    }
}