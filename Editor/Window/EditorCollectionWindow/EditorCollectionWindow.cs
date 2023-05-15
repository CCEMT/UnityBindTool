using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class EditorCollectionWindow : OdinEditorWindow
{
    [Serializable]
    public class EditorCollectionItemDraw
    {
        [HideInInspector]
        public BindData drawData;

        public EditorCollectionItemDraw(BindData bindData)
        {
            drawData = bindData;
        }
    }

    public static void EditorCollection(BindCollection bindCollection)
    {
        EditorCollectionWindow window = GetWindow<EditorCollectionWindow>("EditorCollectionWindow");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
        window.Init(bindCollection);
    }

    [HideInInspector]
    private BindCollection editorCollection;

    [LabelText("Item列表"), ListDrawerSettings(HideRemoveButton = true, HideAddButton = true), ExecuteAlways]
    public List<EditorCollectionItemDraw> editorCollectionItemDrawList = new List<EditorCollectionItemDraw>();

    void Init(BindCollection bindCollection)
    {
        this.editorCollection = bindCollection;

        editorCollectionItemDrawList.Clear();
        int amount = bindCollection.bindDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            BindData bindData=bindCollection.bindDataList[i];
            EditorCollectionItemDraw drawItem = new EditorCollectionItemDraw(bindData);
            editorCollectionItemDrawList.Add(drawItem);
        }
    }
}