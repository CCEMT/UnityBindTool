using System;
using System.Collections.Generic;
using BindTool;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BindHelper
{
    public static void BindTarget(ObjectInfo objectInfo, Object[] targets)
    {
        
    }

    public static TypeString[] GetTypeStringByObject(Object target)
    {
        if (target == null) return null;
        if (target is GameObject gameObject) { return GetTypeStringByGameObject(gameObject); }
        if (target is Component component) { return GetTypeStringByGameObject(component.gameObject); }
        return new[] {new TypeString(target.GetType())};
    }

    public static TypeString[] GetTypeStringByGameObject(GameObject gameObject)
    {
        List<TypeString> typeStringList = new List<TypeString>();

        Type gameObjectType = typeof(GameObject);
        TypeString gameObjectTypeString = new TypeString(gameObjectType);
        typeStringList.Add(gameObjectTypeString);

        Component[] cs = gameObject.GetComponents(typeof(Component));
        foreach (Component t in cs)
        {
            if (t == null) continue;
            Type type = t.GetType();
            TypeString typeString = new TypeString(type);
            typeStringList.Add(typeString);
        }

        return typeStringList.ToArray();
    }
}