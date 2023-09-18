using System;
using Object = UnityEngine.Object;

namespace UnityBindTool
{
    [Serializable]
    public class BindBase : IBindInfo
    {
        public Object target;
        public TypeString[] typeStrings;

        public TypeString[] GetTypeStrings()
        {
            return typeStrings;
        }

        public Object GetValue(int index)
        {
            return this.target;
        }
    }
}