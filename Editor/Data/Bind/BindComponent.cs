using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityBindTool
{
    [Serializable]
    public class BindComponent : IBindInfo
    {
        public GameObject bindGameObject;
        public TypeString[] componentTypeStrings;

        public TypeString[] GetTypeStrings()
        {
            return this.componentTypeStrings;
        }

        public Object GetValue(int index)
        {
            Type type = componentTypeStrings[index].ToType();
            Type gameObjecType = typeof(GameObject);
            if (type == gameObjecType) return bindGameObject;
            return bindGameObject.GetComponent(type);
        }
    }
}