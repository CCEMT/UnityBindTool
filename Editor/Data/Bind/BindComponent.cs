using System;
using BindTool;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class BindComponent : BindInfo
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