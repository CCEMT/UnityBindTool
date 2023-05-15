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
        int amount = bindCollection.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            bindData.SetIndexByAll(bindCollection.GetTypeString());
            bindData.name = bindData.GetValue().name;
        }

        bindCollection.bindDataList.AddRange(addBindDataList);

        List<TypeString> typeStringList = new List<TypeString>();

        int bindAmount = bindCollection.bindDataList.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindData bindData = bindCollection.bindDataList[i];
            TypeString[] typeStrings = bindData.GetAllTypeString();
            if (typeStringList.Count == 0) { typeStringList.AddRange(typeStrings); }
            else { typeStringList = typeStringList.Intersect(typeStrings).ToList(); }
        }

        bindCollection.typeStrings = typeStringList.ToArray();
    }
}