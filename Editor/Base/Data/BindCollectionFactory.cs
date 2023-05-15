using System;
using System.Collections;
using System.Collections.Generic;
using BindTool;
using UnityEngine;
using Object = UnityEngine.Object;

public class BindCollectionFactory
{
    public static BindCollection CreateBindCollection(string name)
    {
        BindCollection bindCollection = new BindCollection();
        bindCollection.name = name;
        bindCollection.collectionType = CollectionType.Array;
        bindCollection.bindDataList = new List<BindData>();
        bindCollection.index = 0;
        bindCollection.typeStrings = new[] {new TypeString(typeof(Object))};
        return bindCollection;
    }

    public static IEnumerable GetValue(BindCollection bindCollection)
    {
        switch (bindCollection.collectionType)
        {
            case CollectionType.Array:
                return GetBindArray(bindCollection);
            case CollectionType.List:
                return GetBindList(bindCollection);
            case CollectionType.Dictionary:
                return GetBindDictionary(bindCollection);
        }
        return null;
    }

    public static Array GetBindArray(BindCollection bindCollection)
    {
        Type type = bindCollection.GetTypeString().ToType();
        int amount = bindCollection.bindDataList.Count;
        Array array = (Array) Activator.CreateInstance(type.MakeArrayType(), amount);
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            array.SetValue(bindData.GetValue(), i);
        }
        Debug.Log(array.GetType());
        return array;
    }

    public static IList GetBindList(BindCollection bindCollection)
    {
        Type elementType = bindCollection.GetTypeString().ToType(); // 指定元素类型
        Type listType = typeof(List<>).MakeGenericType(elementType);
        IList list = (IList) Activator.CreateInstance(listType);

        int amount = bindCollection.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            list.Add(bindData.GetValue());
        }

        return list;
    }

    public static IDictionary GetBindDictionary(BindCollection bindCollection)
    {
        Type keyType = typeof(string); // 指定键类型
        Type valueType = bindCollection.GetTypeString().ToType(); // 指定值类型
        Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        IDictionary dictionary = (IDictionary) Activator.CreateInstance(dictionaryType);

        int amount = bindCollection.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            dictionary.Add(bindData.name, bindData.GetValue());
        }

        return dictionary;
    }
}