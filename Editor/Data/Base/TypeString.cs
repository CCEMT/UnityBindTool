#region Using

using System;

#endregion

namespace BindTool
{
    [Serializable]
    public struct TypeString
    {
        public string typeName;
        public string typeNameSpace;
        public string assemblyName;

        public TypeString(Type type)
        {
            typeName = type.Name;
            typeNameSpace = type.Namespace;
            assemblyName = type.Assembly.GetName().Name;
        }

        #region Equals

        public override bool Equals(object obj)
        {
            TypeString typeString = (TypeString) obj;
            return Equals(typeString);
        }

        public bool Equals(TypeString other)
        {
            if (string.IsNullOrEmpty(other.typeNameSpace) && string.IsNullOrEmpty(typeNameSpace)) return typeName == other.typeName && assemblyName == other.assemblyName;
            return typeName == other.typeName && typeNameSpace == other.typeNameSpace && assemblyName == other.assemblyName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = typeName != null ? typeName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (typeNameSpace != null ? typeNameSpace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (assemblyName != null ? assemblyName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}