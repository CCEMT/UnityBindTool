using BindTool;
using UnityEngine;

public static class BindComponentsHelper
{
    public static void AddBindComponent(GenerateData generateData)
    {
        GameObject root = generateData.objectInfo.rootBindInfo.GetObject();
        BindComponents bindComponents = root.GetComponent<BindComponents>();
        if (bindComponents != null) return;
        bindComponents = root.AddComponent<BindComponents>();
        int componentAmount = generateData.objectInfo.gameObjectBindInfoList.Count;
        for (int i = 0; i < componentAmount; i++)
        {
            ComponentBindInfo componentBindInfo = generateData.objectInfo.gameObjectBindInfoList[i];
            bindComponents.bindComponentList.Add(componentBindInfo.GetValue());
        }

        int dataAmount = generateData.objectInfo.dataBindInfoList.Count;
        for (int i = 0; i < dataAmount; i++)
        {
            DataBindInfo dataBindInfo = generateData.objectInfo.dataBindInfoList[i];
            bindComponents.bindComponentList.Add(dataBindInfo.bindObject);
        }
    }
}