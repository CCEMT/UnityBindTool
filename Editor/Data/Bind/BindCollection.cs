using System;
using System.Collections.Generic;

namespace UnityBindTool
{
    [Serializable]
    public class BindCollection
    {
        public string name;
        public List<BindData> bindDataList = new List<BindData>();
        public CollectionType collectionType;

        public int index;
        public TypeString[] typeStrings;
    }
}