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

        #region Equals

        public override bool Equals(object obj)
        {
            DataBindInfo dataBindInfo = (DataBindInfo) obj;
            if (dataBindInfo != null) return Equals(dataBindInfo);
            return false;
        }

        protected bool Equals(DataBindInfo other)
        {
            return name == other.name && typeString.Equals(other.typeString) && Equals(bindObject, other.bindObject);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = name != null ? name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ typeString.GetHashCode();
                hashCode = (hashCode * 397) ^ (bindObject != null ? bindObject.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}