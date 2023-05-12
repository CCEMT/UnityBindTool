using System;
using System.Collections.Generic;
using BindTool;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BindDataExpand
{
    public static T GetBindInfo<T>(this BindData bindData) where T : BindInfo
    {
        int amount = bindData.bindInfos.Count;
        for (int i = 0; i < amount; i++)
        {
            BindInfo bindInfo = bindData.bindInfos[i];
            if (bindInfo == null) continue;
            if (bindInfo.GetType() == typeof(T)) return (T) bindInfo;
        }
        return default;
    }

    public static Object GetValue(this BindData bindData)
    {
        return bindData.bindTarget.GetValue(bindData.index);
    }

    public static TypeString GetTypeString(this BindData bindData)
    {
        return bindData.bindTarget.GetTypeStrings()[bindData.index];
    }

    public static string GetTypeName(this BindData bindData)
    {
        return bindData.GetTypeString().typeName;
    }

    public static GameObject GetGameObject(this BindData bindData)
    {
        BindComponent bindComponent = bindData.GetBindInfo<BindComponent>();
        if (bindComponent != null) { return bindComponent.bindGameObject; }
        return null;
    }

    public static bool GameObjectEquals(this BindData bindData, GameObject target)
    {
        if (target == null) return false;
        GameObject bindGameObject = bindData.GetGameObject();
        if (target == bindGameObject) return true;
        GameObject targetPrefab = CommonTools.GetPrefabAsset(target);
        if (targetPrefab == bindGameObject) return true;
        return false;
    }

    public static void SetBindInfo<T>(this BindData bindData) where T : BindInfo
    {
        SetBindInfo(bindData, typeof(T));
    }

    public static void SetBindInfo(this BindData bindData, Type targetType)
    {
        int amount = bindData.bindInfos.Count;
        for (int i = 0; i < amount; i++)
        {
            BindInfo bindInfo = bindData.bindInfos[i];
            if (bindInfo == null) continue;
            if (bindInfo.GetType() != targetType) continue;
            bindData.bindTarget = bindInfo;
            return;
        }
    }

    public static void SetIndex(this BindData bindData, TypeString typeString)
    {
        TypeString[] typeStrings = bindData.bindTarget.GetTypeStrings();
        int componentAmount = typeStrings.Length;
        for (int i = 0; i < componentAmount; i++)
        {
            TypeString type = typeStrings[i];
            if (! type.Equals(typeString)) continue;
            bindData.index = i;
            return;
        }
    }

    public static void SetIndexByAll(this BindData bindData, TypeString targetType)
    {
        int bindAmount = bindData.bindInfos.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindInfo bindInfo = bindData.bindInfos[i];
            TypeString[] typeStrings = bindInfo.GetTypeStrings();
            int typeStringAmount = typeStrings.Length;
            for (int j = 0; j < typeStringAmount; j++)
            {
                TypeString typeString = typeStrings[j];
                if (! typeString.Equals(targetType)) continue;
                bindData.bindTarget = bindInfo;
                bindData.index = j;
            }
        }
    }

    public static TypeString[] GetAllTypeString(this BindData bindData)
    {
        List<TypeString> typeStringList = new List<TypeString>();

        int bindAmount = bindData.bindInfos.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindInfo bindInfo = bindData.bindInfos[i];
            typeStringList.AddRange(bindInfo.GetTypeStrings());
        }

        return typeStringList.ToArray();
    }
}