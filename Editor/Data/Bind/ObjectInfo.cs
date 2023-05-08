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
        public List<ComponentBindInfo> gameObjectBindInfoList=new List<ComponentBindInfo>();
        public List<DataBindInfo> dataBindInfoList=new List<DataBindInfo>();
        public List<ComponentCollectionBindInfo> componentCollectionBindInfoList=new List<ComponentCollectionBindInfo>();
        public List<DataCollectionBindInfo> dataCollectionBindInfoList=new List<DataCollectionBindInfo>();
    }
}