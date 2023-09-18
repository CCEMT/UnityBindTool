using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace UnityBindTool
{
    public class EditorCollectionWindow : OdinEditorWindow
    {
        public static void EditorCollection(BindCollection bindCollection)
        {
            EditorCollectionWindow window = GetWindow<EditorCollectionWindow>("EditorCollectionWindow");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
            window.Init(bindCollection);
        }

        [HideInInspector]
        private BindCollection editorCollection;

        [LabelText("Item列表"), ListDrawerSettings(HideRemoveButton = true, HideAddButton = true), ExecuteAlways]
        public List<EditorCollectionItemDrawData> editorCollectionItemDrawList = new List<EditorCollectionItemDrawData>();

        void Init(BindCollection bindCollection)
        {
            this.editorCollection = bindCollection;

            editorCollectionItemDrawList.Clear();
            int amount = bindCollection.bindDataList.Count;
            for (int i = 0; i < amount; i++)
            {
                BindData bindData = bindCollection.bindDataList[i];
                EditorCollectionItemDrawData drawDataItem = new EditorCollectionItemDrawData(bindData, RemoveItem);
                editorCollectionItemDrawList.Add(drawDataItem);
            }
        }

        void RemoveItem(EditorCollectionItemDrawData item)
        {
            editorCollectionItemDrawList.Remove(item);
            this.editorCollection.bindDataList.Remove(item.drawData);
        }
    }
}