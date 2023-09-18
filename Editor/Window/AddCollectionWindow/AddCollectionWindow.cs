using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace UnityBindTool
{
    public class AddCollectionWindow : OdinEditorWindow
    {
        public static void AddToCollection(ObjectInfo objectInfo, List<BindData> bindDataList, List<TypeString> typeStrings, Action callback = null)
        {
            AddCollectionWindow window = GetWindow<AddCollectionWindow>("AddCollectionWindow");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
            window.Init(objectInfo, bindDataList, typeStrings, callback);
        }

        [NonSerialized, OdinSerialize]
        private ObjectInfo info;

        [SerializeField]
        private List<BindData> addBindDataList;

        [SerializeField]
        private List<TypeString> filterTypeStringList;

        [NonSerialized, OdinSerialize]
        private Action endCallback;

        [LabelText("选择要添加的集合"), ListDrawerSettings(Expanded = true, HideRemoveButton = true, CustomAddFunction = nameof(AddCollection))]
        public List<AddBindDataDraw> addBindDataDraws = new List<AddBindDataDraw>();

        void Init(ObjectInfo objectInfo, List<BindData> bindDataList, List<TypeString> typeStrings, Action callback)
        {
            info = objectInfo;
            addBindDataList = bindDataList;
            filterTypeStringList = typeStrings;
            endCallback = callback;
            GetCanAddCollection();
        }

        void AddCollection()
        {
            OdinHelper.InputDropDown((name) => {
                info.bindCollectionList.Add(BindCollectionFactory.CreateBindCollection(name));
                GetCanAddCollection();
            });
        }

        private void OnValidate()
        {
            if (info != null) GetCanAddCollection();
        }

        void GetCanAddCollection()
        {
            addBindDataDraws.Clear();
            int amount = info.bindCollectionList.Count;
            for (int i = 0; i < amount; i++)
            {
                BindCollection bindCollection = info.bindCollectionList[i];
                TypeString typeString = bindCollection.GetTypeString();
                if (this.filterTypeStringList.Contains(typeString)) addBindDataDraws.Add(new AddBindDataDraw(bindCollection, Select));
            }
        }

        void Select(BindCollection select)
        {
            select.AddBindData(this.addBindDataList);
            endCallback?.Invoke();
            Close();
        }
    }
}