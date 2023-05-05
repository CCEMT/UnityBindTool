using System;
using Object = UnityEngine.Object;

namespace BindTool
{
    [Serializable]
    public class DataBindInfo
    {
        public string name;
        public TypeString typeString;
        public Object bindObject;

        public DataBindInfo(Object bindObject)
        {
            this.bindObject = bindObject;
            typeString = new TypeString(bindObject.GetType());
        }
    }
}