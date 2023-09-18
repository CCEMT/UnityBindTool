using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace UnityBindTool
{
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

        public static BindCollection CreateBindCollection(string name, IEnumerable enumerable)
        {
            BindCollection bindCollection = new BindCollection();
            bindCollection.name = name;
            bindCollection.collectionType = GetCollectionType(enumerable);
            List<BindData> bindDataList = GetCollectionBindDataList(enumerable, bindCollection.collectionType);
            bindCollection.bindDataList = bindDataList;

            bindCollection.typeStrings = GetBindDataListIntersectTypeString(bindDataList);
            bindCollection.index = GetCollectionIndex(enumerable, bindCollection.collectionType, bindCollection.typeStrings);
            return bindCollection;
        }

        public static CollectionType GetCollectionType(IEnumerable enumerable)
        {
            switch (enumerable)
            {
                case Array:
                    return CollectionType.Array;
                case IList:
                    return CollectionType.List;
            }
            return default;
        }

        public static List<BindData> GetCollectionBindDataList(IEnumerable enumerable, CollectionType collectionType)
        {
            List<BindData> bindDataList = new List<BindData>();
            switch (collectionType)
            {
                case CollectionType.Array:
                case CollectionType.List:
                    foreach (object value in enumerable) bindDataList.Add(BindDataFactory.CreateBindData((Object) value));
                    break;
            }
            return bindDataList;
        }

        public static TypeString[] GetBindDataListIntersectTypeString(List<BindData> bindDataList)
        {
            List<TypeString> typeStringList = new List<TypeString>();
            int amount = bindDataList.Count;
            for (int i = 0; i < amount; i++)
            {
                BindData bindData = bindDataList[i];
                TypeString[] allTypeString = bindData.GetAllTypeString();
                if (i == 0) typeStringList.AddRange(allTypeString);
                else typeStringList = typeStringList.Intersect(allTypeString).ToList();
            }
            return typeStringList.ToArray();
        }

        public static int GetCollectionIndex(IEnumerable enumerable, CollectionType collectionType, TypeString[] typeStrings)
        {
            Type type = null;
            switch (collectionType)
            {
                case CollectionType.Array:
                    type = enumerable.GetType().GetElementType();
                    break;
                case CollectionType.List:
                    type = enumerable.GetType().GetGenericArguments()[0];
                    break;
            }
            TypeString targetTypeString = new TypeString(type);
            for (int i = 0; i < typeStrings.Length; i++)
            {
                TypeString typeString = typeStrings[i];
                if (typeString.Equals(targetTypeString)) return i;
            }

            return -1;
        }

        public static IEnumerable GetValue(BindCollection bindCollection)
        {
            switch (bindCollection.collectionType)
            {
                case CollectionType.Array:
                    return GetBindArray(bindCollection);
                case CollectionType.List:
                    return GetBindList(bindCollection);
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
    }
}