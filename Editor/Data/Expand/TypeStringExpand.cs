using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityBindTool
{
    public static class TypeStringExpand
    {
        public static bool IsEmpty(this TypeString typeString)
        {
            return string.IsNullOrEmpty(typeString.typeName) || string.IsNullOrEmpty(typeString.assemblyName);
        }

        public static Type ToType(this TypeString typeString)
        {
            Assembly assembly = GetAssemblyByName(typeString.assemblyName);
            if (assembly == null) return null;
            List<Type> types = assembly.GetTypes().Where((type) => CheckTypeNamespace(typeString, type)).ToList();
            Type find = types.Find((type) => CheckTypeName(typeString, type));
            return find;
        }

        static bool CheckTypeNamespace(this TypeString typeString, Type type)
        {
            if (string.Equals(type.Namespace, typeString.typeNameSpace, StringComparison.Ordinal)) return true;
            return string.IsNullOrEmpty(type.Namespace) && string.IsNullOrEmpty(typeString.typeNameSpace);
        }

        static bool CheckTypeName(this TypeString typeString, Type type)
        {
            return string.Equals(type.Name, typeString.typeName, StringComparison.Ordinal);
        }

        static Assembly GetAssemblyByName(string assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);
        }

        public static string GetVisitString(this TypeString typeString)
        {
            if (string.IsNullOrEmpty(typeString.typeNameSpace)) { return typeString.typeName; }
            else { return $"{typeString.typeNameSpace}.{typeString.typeName}"; }
        }

        public static TypeString ToTypeString(this Type type)
        {
            return new TypeString(type);
        }
    }
}