using System;
using UnityEngine;

[Serializable]
public class EditorCollectionItemDrawData
{
    [HideInInspector]
    public BindData drawData;

    [HideInInspector]
    public Action<EditorCollectionItemDrawData> removeCallback;

    public EditorCollectionItemDrawData(BindData bindData, Action<EditorCollectionItemDrawData> remove)
    {
        drawData = bindData;
        this.removeCallback = remove;
    }
}