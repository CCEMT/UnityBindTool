using BindTool;
using UnityEngine;

public static class BindComponentsHelper
{
    public static BindComponents AddBindComponent(GenerateData generateData)
    {
        GameObject root = generateData.objectInfo.rootData.GetBindInfo<BindComponent>().bindGameObject;
        BindComponents bindComponents = root.GetComponent<BindComponents>();
        if (bindComponents == null)
        {
            bindComponents.bindComponentList.Clear();
            bindComponents.bindCollectionList.Clear();
        }
        bindComponents = root.AddComponent<BindComponents>();

        int bindAmount = generateData.objectInfo.bindDataList.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindData bindData = generateData.objectInfo.bindDataList[i];
            bindComponents.bindComponentList.Add(bindData.GetValue());
        }

        int collectionAmount = generateData.objectInfo.bindCollectionList.Count;
        for (int i = 0; i < collectionAmount; i++)
        {
            BindCollection bindCollection = generateData.objectInfo.bindCollectionList[i];
            bindComponents.bindCollectionList.Add(bindCollection.GetValue());
        }
        return bindComponents;
    }
}