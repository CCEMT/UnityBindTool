using System.Collections.Generic;
using BindTool;

public class BindCollection
{
    public string name;
    public List<BindData> BindDataList = new List<BindData>();
    public CollectionType collectionType;

    public int index;
    public TypeString[] typeStrings;
}