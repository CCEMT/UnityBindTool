using BindTool;
using UnityEngine;

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

    public static string[] GetTypeStrings(this BindData bindData)
    {
        return default;
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

    public static void SetBindInfo<T>(this BindData bindData)
    {
        int amount = bindData.bindInfos.Count;
        for (int i = 0; i < amount; i++)
        {
            BindInfo bindInfo = bindData.bindInfos[i];
            if (bindInfo == null) continue;
            if (bindInfo.GetType() != typeof(T)) continue;
            bindData.bindTarget = bindInfo;
            return;
        }
    }

    public static void SetBindComponentIndex(this BindData bindData, TypeString typeString)
    {
        BindComponent bindComponent = bindData.GetBindInfo<BindComponent>();
        if (bindComponent == null) { return; }
        bindData.bindTarget = bindComponent;
        int componentAmount = bindComponent.componentTypeStrings.Length;
        for (int i = 0; i < componentAmount; i++)
        {
            TypeString componentType = bindComponent.componentTypeStrings[i];
            if (! componentType.Equals(typeString)) continue;
            bindData.index = i;
            return;
        }
    }
}