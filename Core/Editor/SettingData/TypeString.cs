#region Using

using System;
using System.Linq;
using System.Reflection;

#endregion

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

    public bool IsEmpty()
    {
        if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(assemblyName)) return true;
        return false;
    }

    public static bool IsExist(string typeName, string typeNameSpace, string assemblyName)
    {
        TypeString typeString = new TypeString();
        typeString.typeName = typeName;
        typeString.typeNameSpace = typeNameSpace;
        typeString.assemblyName = assemblyName;
        var type = typeString.ToType();
        return type != null;
    }

    public Type ToType()
    {
        var assembly = GetAssemblyByName(assemblyName);
        if (assembly == null) return null;
        var types = assembly.GetTypes().Where(CheckTypeNamespace).ToList();
        var find = types.Find(CheckTypeName);
        return find;
    }

    bool CheckTypeNamespace(Type type)
    {
        if (string.Equals(type.Namespace, typeNameSpace, StringComparison.Ordinal)) return true;
        if (string.IsNullOrEmpty(type.Namespace) && string.IsNullOrEmpty(typeNameSpace)) return true;
        return false;
    }

    bool CheckTypeName(Type type)
    {
        if (string.Equals(type.Name, typeName, StringComparison.Ordinal)) return true;
        return false;
    }

    Assembly GetAssemblyByName(string assemblyName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);
    }

    public string GetVisitString()
    {
        if (string.IsNullOrEmpty(typeNameSpace)) { return typeName; }
        else { return $"{typeNameSpace}.{typeName}"; }
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
            var hashCode = typeName != null ? typeName.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (typeNameSpace != null ? typeNameSpace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (assemblyName != null ? assemblyName.GetHashCode() : 0);
            return hashCode;
        }
    }

    #endregion
}