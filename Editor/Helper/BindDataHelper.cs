using System;
using System.Collections.Generic;
using System.Linq;
using BindTool;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public static class BindDataHelper
{
    private class TypeSelectData
    {
        public Type type;
        public TypeString typeString;
    }

    public static void DropDownBinDataTypeSwitch(BindData bindData)
    {
        Dictionary<string, TypeSelectData> dataForDraw = new Dictionary<string, TypeSelectData>();

        int bindAmount = bindData.bindInfos.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindInfo bindInfo = bindData.bindInfos[i];
            Type bindInfoType = bindInfo.GetType();
            TypeString[] typeStrings = bindInfo.GetTypeStrings();
            int typeStringAmount = typeStrings.Length;
            for (int j = 0; j < typeStringAmount; j++)
            {
                TypeString typeString = typeStrings[j];

                TypeSelectData typeSelectData = new TypeSelectData();
                typeSelectData.type = bindInfoType;
                typeSelectData.typeString = typeString;

                string path = $"{bindInfoType.Name}/{typeString.typeName}";
                dataForDraw.Add(path, typeSelectData);
            }
        }

        IEnumerable<GenericSelectorItem<TypeSelectData>> customCollection = dataForDraw.Keys.Select(itemName =>
            new GenericSelectorItem<TypeSelectData>($"{itemName}", dataForDraw[itemName]));

        GenericSelector<TypeSelectData> CustomGenericSelector = new("切换类型", false, customCollection);

        CustomGenericSelector.EnableSingleClickToSelect();
        CustomGenericSelector.SelectionChanged += ints => {
            TypeSelectData result = ints.FirstOrDefault();
            if (result == null) return;
            bindData.SetBindInfo(result.type);
            bindData.SetIndex(result.typeString);
        };

        CustomGenericSelector.ShowInPopup();
    }

    public static void BindDataList(ObjectInfo objectInfo, List<BindData> bindDataList, Action endCallback)
    {
       
    }
}