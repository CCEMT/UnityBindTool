#region Using

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace BindTool
{
    [Serializable]
    public class ObjectInfo
    {
        public TypeString typeString;
        public ComponentBindInfo rootBindInfo;
        public List<ComponentBindInfo> gameObjectBindInfoList;
        public List<DataBindInfo> dataBindInfoList;
        public List<ComponentCollectionBindInfo> componentCollectionBindInfoList;
        public List<DataCollectionBindInfo> dataCollectionBindInfoList;
    }
}