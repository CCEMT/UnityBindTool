using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BindTool;
using Sirenix.OdinInspector.Editor;
using UnityEditorInternal;
using UnityEngine;

public static class TypeStringSelectHelper
{
    public static void DrawTypeStringSelect(Action<TypeString> endCallback)
    {
        BindSetting bindSetting = BindSetting.Get();
        Dictionary<string, Type> typeForDraw = new Dictionary<string, Type>();

        Dictionary<string, Assembly> assemblieDictionary = AppDomain.CurrentDomain.GetAssemblies().ToDictionary((a) => a.GetName().Name, (a) => a);

        List<Assembly> addAssembly = new List<Assembly>();

        int nameAmount = bindSetting.baseSetting.scanAssemblyList.Count;
        for (int i = 0; i < nameAmount; i++)
        {
            string assemblyName = bindSetting.baseSetting.scanAssemblyList[i];
            if (! assemblieDictionary.ContainsKey(assemblyName)) continue;
            Assembly assembly = assemblieDictionary[assemblyName];
            addAssembly.Add(assembly);

        }

        int assetAmount = bindSetting.baseSetting.scanAssemblyAssetList.Count;
        for (int i = 0; i < assetAmount; i++)
        {
            AssemblyDefinitionAsset assemblyDefinitionAsset = bindSetting.baseSetting.scanAssemblyAssetList[i];
            AssemblyDefinitionData assemblyDefinitionData = JsonUtility.FromJson<AssemblyDefinitionData>(assemblyDefinitionAsset.text);
            if (! assemblieDictionary.ContainsKey(assemblyDefinitionData.name)) continue;
            Assembly assembly = assemblieDictionary[assemblyDefinitionData.name];
            addAssembly.Add(assembly);
        }

        int assemblyAmount = addAssembly.Count;
        for (int i = 0; i < assemblyAmount; i++)
        {
            Assembly assembly = addAssembly[i];
            Type[] types = assembly.GetTypes();
            int typeAmount = types.Length;
            for (int j = 0; j < typeAmount; j++)
            {
                Type type = types[j];
                typeForDraw.Add(type.FullName, type);
            }
        }

        IEnumerable<GenericSelectorItem<Type>> customCollection = typeForDraw.Keys.Select(itemName =>
            new GenericSelectorItem<Type>($"{itemName}", typeForDraw[itemName]));

        GenericSelector<Type> CustomGenericSelector = new("选择类型", false, customCollection);
        CustomGenericSelector.EnableSingleClickToSelect();
        CustomGenericSelector.SelectionChanged += ints => {
            Type result = ints.FirstOrDefault();
            if (result != null) { endCallback?.Invoke(result.ToTypeString()); }
        };

        CustomGenericSelector.ShowInPopup(500);
    }
}