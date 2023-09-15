#region Using

using System;
using System.Collections.Generic;
using Sirenix.Serialization;

#endregion

namespace BindTool
{
    [Serializable]
    public class ObjectInfo
    {
        public TypeString typeString;

        [NonSerialized, OdinSerialize]
        public BindData rootData;

        [NonSerialized, OdinSerialize]
        public List<BindData> bindDataList = new List<BindData>();

        [NonSerialized, OdinSerialize]
        public List<BindCollection> bindCollectionList = new List<BindCollection>();
    }
}