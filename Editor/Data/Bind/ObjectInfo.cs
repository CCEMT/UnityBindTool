using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace UnityBindTool
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