using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AddBindDataDraw
{
    [HideInInspector]
    public BindCollection drawCollection;

    [HideInInspector]
    public Action<BindCollection> selectCallback;

    public AddBindDataDraw(BindCollection collection, Action<BindCollection> callback)
    {
        this.drawCollection = collection;
        this.selectCallback = callback;
    }

    [Button("@" + nameof(drawCollection) + "." + nameof(BindCollection.name), ButtonSizes.Medium)]
    void Select()
    {
        selectCallback?.Invoke(this.drawCollection);
    }
}