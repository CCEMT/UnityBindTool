using System;
using System.Collections.Generic;
using System.Reflection;
using BindTool;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ObjectInfoHelper
{
    public static SyntaxKind VisitTypeToSyntaxKind(VisitType visitType)
    {
        switch (visitType)
        {
            case VisitType.Public:
                return SyntaxKind.PublicKeyword;
            case VisitType.Private:
                return SyntaxKind.PrivateKeyword;
            case VisitType.Protected:
                return SyntaxKind.ProtectedKeyword;
            case VisitType.Internal:
                return SyntaxKind.InternalKeyword;
        }

        return default;
    }

    public static ObjectInfo GetObjectInfo(GameObject bindObject)
    {
        ObjectInfo objectInfo = null;

        BindComponents bindComponents = bindObject.GetComponent<BindComponents>();

        if (bindComponents != null)
        {
            if (bindComponents.bindRoot == null) { Object.DestroyImmediate(bindComponents); }
            else
            {
                objectInfo = new ObjectInfo();
                objectInfo.typeString = new TypeString(bindComponents.bindRoot.GetType());
                objectInfo.rootBindInfo = new ComponentBindInfo(bindObject);
                objectInfo.rootBindInfo.name = objectInfo.typeString.typeName;
                objectInfo.gameObjectBindInfoList = new List<ComponentBindInfo>();
                objectInfo.dataBindInfoList = new List<DataBindInfo>();

                Type type = bindComponents.bindRoot.GetType();
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    AutoGenerate attribute = (AutoGenerate) fieldInfo.GetCustomAttribute(typeof(AutoGenerate), true);
                    if (attribute == null || attribute.autoGenerateType != AutoGenerateType.OriginalField) continue;
                    Object bindComponent = (Object) fieldInfo.GetValue(bindComponents.bindRoot);
                    if (bindComponent is GameObject == false && bindComponent.GetType().IsSubclassOf(typeof(Component)) == false)
                    {
                        DataBindInfo dataBindInfo = new DataBindInfo(bindComponent);
                        dataBindInfo.name = fieldInfo.Name;
                        objectInfo.dataBindInfoList.Add(dataBindInfo);
                    }
                    else
                    {
                        ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindComponent);
                        componentBindInfo.name = fieldInfo.Name;
                        componentBindInfo.SetIndex(new TypeString(fieldInfo.FieldType));
                        objectInfo.gameObjectBindInfoList.Add(componentBindInfo);
                    }
                }
            }

        }

        if (objectInfo != null) return objectInfo;
        objectInfo = new ObjectInfo();
        objectInfo.gameObjectBindInfoList = new List<ComponentBindInfo>();
        objectInfo.dataBindInfoList = new List<DataBindInfo>();
        objectInfo.SetObject(bindObject);

        return objectInfo;
    }
}