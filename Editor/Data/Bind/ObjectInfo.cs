#region Using

using System;
using System.Collections.Generic;

#endregion

namespace BindTool
{
    [Serializable]
    public class ObjectInfo
    {
        public TypeString typeString;
        public BindData rootData;
        public List<BindData> bindDataList = new List<BindData>();
        public List<BindCollection> bindCollectionList = new List<BindCollection>();
    }
}