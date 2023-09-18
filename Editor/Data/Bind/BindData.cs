using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace UnityBindTool
{
    [Serializable]
    public class BindData
    {
        public string name;
        public int index;

        [NonSerialized, OdinSerialize]
        public IBindInfo bindTarget;

        [NonSerialized, OdinSerialize]
        public List<IBindInfo> bindInfos = new List<IBindInfo>();
    }
}