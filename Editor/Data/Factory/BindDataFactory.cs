using System;
using System.Collections.Generic;
using BindTool;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BindDataFactory
{
    public static BindData CreateBindData(Object bindTarget)
    {
        BindData bindData = new BindData();

        BindBase bindBase = CreateBindBase(bindTarget);
        bindData.bindInfos.Add(bindBase);
        bindData.bindTarget = bindBase;

        switch (bindTarget)
        {
            case GameObject gameObject:
            {
                BindComponent bindComponent = CreateBindComponent(gameObject);
                bindData.bindInfos.Add(bindComponent);
                break;
            }
            case Component component:
            {
                BindComponent bindComponent = CreateBindComponent(component.gameObject);
                bindData.bindInfos.Add(bindComponent);
                break;
            }
        }

        return bindData;
    }

    public static BindBase CreateBindBase(Object target)
    {
        BindBase bindBase = new BindBase();
        bindBase.target = target;

        List<TypeString> addTypeString = new List<TypeString>();
        Type disposeType = target.GetType();
        Type objectType = typeof(Object);
        while (true)
        {
            addTypeString.Add(new TypeString(disposeType));
            if (disposeType == objectType) break;
            disposeType = disposeType.BaseType;
        }
        bindBase.typeStrings = addTypeString.ToArray();

        return bindBase;
    }

    public static BindComponent CreateBindComponent(GameObject bindGameObject)
    {
        BindComponent bindComponent = new BindComponent();
        bindComponent.bindGameObject = bindGameObject;

        List<TypeString> typeStringList = new List<TypeString>();

        Type gameObjectType = typeof(GameObject);
        TypeString gameObjectTypeString = new TypeString(gameObjectType);
        typeStringList.Add(gameObjectTypeString);

        Component[] components = bindGameObject.GetComponents<Component>();
        int componentAmount = components.Length;
        for (int i = 0; i < componentAmount; i++)
        {
            Component component = components[i];
            if (component == null) continue;
            Type type = component.GetType();
            TypeString typeString = new TypeString(type);
            typeStringList.Add(typeString);
        }

        bindComponent.componentTypeStrings = typeStringList.ToArray();
        return bindComponent;
    }
}