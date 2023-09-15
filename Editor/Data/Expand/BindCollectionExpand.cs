using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BindTool;

public static class BindCollectionExpand
{
    public static IEnumerable GetValue(this BindCollection bindCollection)
    {
        return BindCollectionFactory.GetValue(bindCollection);
    }

    public static TypeString GetTypeString(this BindCollection bindCollection)
    {
        return bindCollection.typeStrings[bindCollection.index];
    }

    public static string[] GetTypeStrings(this BindCollection bindCollection)
    {
        return bindCollection.typeStrings.Select((t) => t.typeName).ToArray();
    }

    public static string GetTypeName(this BindCollection bindCollection)
    {
        return bindCollection.typeStrings[bindCollection.index].typeName;
    }

    public static string GetTypeFullNmae(this BindCollection bindCollection)
    {
        return bindCollection.GetTypeString().GetVisitString();
    }

    public static void AddBindData(this BindCollection bindCollection, List<BindData> addBindDataList)
    {
        TypeString selectTypeString = bindCollection.GetTypeString();

        List<TypeString> typeStringList = new List<TypeString>();

        bindCollection.bindDataList.AddRange(addBindDataList);
        
        int bindAmount = bindCollection.bindDataList.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            TypeString[] typeStrings = bindData.GetAllTypeString();
            if (typeStringList.Count == 0) { typeStringList.AddRange(typeStrings); }
            else { typeStringList = typeStringList.Intersect(typeStrings).ToList(); }
        }

        bindCollection.typeStrings = typeStringList.ToArray();

        int amount = bindCollection.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            bindData.SetIndexByAll(selectTypeString);
            bindData.name = bindData.GetValue().name;
        }

        bindCollection.SetIndex(selectTypeString);
    }

    public static void SetIndex(this BindCollection bindCollection, TypeString selectType)
    {
        int amount = bindCollection.typeStrings.Length;
        for (int i = 0; i < amount; i++)
        {
            TypeString typeString = bindCollection.typeStrings[i];
            if (! typeString.Equals(selectType)) continue;
            bindCollection.index = i;
            break;
        }
    }

    public static void SetIndex(this BindCollection bindCollection, int index)
    {
        bindCollection.index = index;
        TypeString typeString = bindCollection.GetTypeString();
        int amount = bindCollection.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            bindData.SetIndex(typeString);
        }
    }
}