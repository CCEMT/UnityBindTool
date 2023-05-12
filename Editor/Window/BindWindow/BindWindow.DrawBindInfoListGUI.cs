using System;
using System.Collections.Generic;
using System.Linq;
using BindTool;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class BindWindow
{
    private const float ItemHight = 50;
    private const float ItemInterval = 2.25f;
    private const float ContentHight = 240;

    private string bindInputString = "";

    private SearchType searchType;
    private BindTypeIndex bindTypeIndex;

    [NonSerialized, OdinSerialize]
    private List<BindData> selectBindDataList;

    [NonSerialized, OdinSerialize]
    private List<BindCollection> selectbindCollectionList;

    private int bindAmount;
    private int selectBindAmount;

    private int bindCollectionAmount;
    private int selectbindCollectionAmount;

    private int showAmount;
    private int currentIndex;
    private int maxIndex;

    void BindInfoListInit()
    {
        this.bindInputString = "";
        this.searchType = SearchType.All;
        this.bindTypeIndex = BindTypeIndex.Item;

        selectBindDataList = new List<BindData>();
        selectbindCollectionList = new List<BindCollection>();

        GetData();
        SearchSelectList();
    }

    void DrawBindInfoListGUI()
    {
        Chack();
        GetData();
        DrawOperate();
        DrawBindArea();
        DrawBindInfo();
    }

    void GetData()
    {
        int tempAmount = (int) ((position.height - ContentHight) / (ItemHight + ItemInterval));
        if (tempAmount == this.showAmount) return;
        this.showAmount = tempAmount;
        SearchSelectList();
    }

    void Chack()
    {
        if (this.bindTypeIndex == BindTypeIndex.Item)
        {
            if (this.selectBindDataList != null) return;
            this.selectBindDataList = new List<BindData>();
            SearchSelectList();
        }
        else if (this.bindTypeIndex == BindTypeIndex.Collection)
        {
            if (this.selectbindCollectionList != null) return;
            this.selectbindCollectionList = new List<BindCollection>();
            SearchSelectList();
        }
    }

    void DrawOperate()
    {
        EditorGUILayout.BeginVertical("frameBox");
        {
            DrawSetting();
            DrawBuild();
            DrawBind();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawSetting()
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            string content = this.bindSetting.selectCompositionSetting != null ? this.bindSetting.selectCompositionSetting.compositionName : "选择设置为空，请先选择设置";
            GUILayout.Label(content);

            if (GUILayout.Button("编辑", GUILayout.Width(50f))) { BindSettingWindow.OpenWindow(); }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawBuild()
    {
        if (GUILayout.Button("生成")) { this.bindWindowState = BindWindowState.BuildGUI; }
    }

    void DrawBind()
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("绑定所有"))
            {
                ObjectInfoHelper.BindAll(this.editorObjectInfo, this.bindObject, this.bindSetting.selectCompositionSetting);
                SearchSelectList();
            }

            if (GUILayout.Button("自动绑定"))
            {
                ObjectInfoHelper.ObjectInfoAutoBind(this.editorObjectInfo, this.bindObject, this.bindSetting.selectCompositionSetting);
                SearchSelectList();
            }

            if (GUILayout.Button("解除所有"))
            {
                if (EditorUtility.DisplayDialog("确认窗口", "是否解除所有绑定", "确认", "取消"))
                {
                    ObjectInfoHelper.ClearAllBind(this.editorObjectInfo);
                    SearchSelectList();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawBindArea()
    {
        Event currentEvent = Event.current;
        Rect dragArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true), GUILayout.Height(50));

        GUI.color = Color.green;
        GUI.Box(dragArea, "将组件拖拽至此进行绑定");
        GUI.color = Color.white;

        if (currentEvent.type is not (EventType.DragUpdated or EventType.DragPerform)) return;
        if (! dragArea.Contains(currentEvent.mousePosition)) return;
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (currentEvent.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            BindSelectTarget(DragAndDrop.objectReferences);
        }
        currentEvent.Use();
    }

    void BindSelectTarget(Object[] bindTargets)
    {
        if (bindTargets.Length == 0) return;

        List<TypeString> typeStringList = new List<TypeString>();

        List<BindData> bindDataList = new List<BindData>();
        int amount = bindTargets.Length;
        for (int i = 0; i < amount; i++)
        {
            Object bindTarget = bindTargets[i];
            BindData bindData = BindDataFactory.CreateBindData(bindTarget);
            TypeString[] typeStrings = bindData.GetAllTypeString();
            if (typeStringList.Count == 0) { typeStringList.AddRange(typeStrings); }
            else { typeStringList = typeStringList.Intersect(typeStrings).ToList(); }
            bindDataList.Add(bindData);
        }

        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("自动获取类型"), false, () => { AutoBindSelectTarget(bindTargets); });
        menu.AddItem(new GUIContent("添加到集合"), false, () => { AddCollectionWindow.AddToCollection(this.editorObjectInfo, bindDataList, typeStringList, SearchSelectList); });

        int typeStringAmount = typeStringList.Count;
        for (int i = 0; i < typeStringAmount; i++)
        {
            TypeString typeString = typeStringList[i];
            menu.AddItem(new GUIContent(typeString.typeName), false, SetIndex, typeString);
        }

        menu.ShowAsContext();

        void SetIndex(object targetType)
        {
            int bindDataAmount = bindDataList.Count;
            for (int i = 0; i < bindDataAmount; i++)
            {
                BindData bindData = bindDataList[i];
                bindData.SetIndexByAll((TypeString) targetType);
                ObjectInfoHelper.BindDataToObjectInfo(this.editorObjectInfo, bindData, this.bindSetting.selectCompositionSetting);
            }
            SearchSelectList();
        }
    }

    void AutoBindSelectTarget(Object[] bindTargets)
    {
        int amount = bindTargets.Length;
        for (int i = 0; i < amount; i++)
        {
            Object bindTarget = bindTargets[i];
            ObjectInfoHelper.StartNameBind(this.editorObjectInfo, bindTarget, this.bindSetting.selectCompositionSetting);
        }
        SearchSelectList();
    }

    void DrawBindInfo()
    {
        DrawSearch();
        DrawBindList();
    }

    void DrawSearch()
    {
        GUILayout.BeginVertical("box");
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Search Item：");
                OdinHelper.DrawOdinEnum(string.Empty, this.searchType, (value) => { this.searchType = (SearchType) value; });
                if (GUILayout.Button("操作", GUILayout.Width(50))) ListOperate();
            }
            EditorGUILayout.EndHorizontal();

            string tempString = GUILayout.TextField(this.bindInputString, "SearchTextField");
            if (tempString.Equals(this.bindInputString) == false)
            {
                this.bindInputString = tempString;
                SearchSelectList();
            }

        }
        GUILayout.EndVertical();
    }

    void ListOperate()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("重置搜索名称"), false, ResetSearchContent);
        menu.AddItem(new GUIContent("查找重复名称的Item"), false, FindRepetitionNameItem);
        menu.ShowAsContext();
    }

    void ResetSearchContent()
    {
        this.bindInputString = "";
        SearchSelectList();
    }

    void FindRepetitionNameItem() { }

    void DrawBindList()
    {
        int tempIndex = GUILayout.Toolbar((int) this.bindTypeIndex, new string[] {"Item", "Collection"});

        if (tempIndex != (int) this.bindTypeIndex)
        {
            this.bindTypeIndex = (BindTypeIndex) tempIndex;
            this.currentIndex = 0;
            SearchSelectList();
            GUI.FocusControl(null);
        }

        switch (this.bindTypeIndex)
        {
            case BindTypeIndex.Item:
                DrawBindItem();
                break;
            case BindTypeIndex.Collection:
                DrawBindCollection();
                break;
        }

        GUILayout.BeginHorizontal("box");
        {
            GUILayout.Label($"{this.currentIndex}/{this.maxIndex}");

            if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationLeft", GUILayout.Width(30), GUILayout.Height(20)))
            {
                this.currentIndex--;
                this.currentIndex = Mathf.Clamp(this.currentIndex, 1, this.maxIndex);
            }
            if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationRight", GUILayout.Width(30), GUILayout.Height(20)))
            {
                this.currentIndex++;
                this.currentIndex = Mathf.Clamp(this.currentIndex, 1, this.maxIndex);
            }
        }
        GUILayout.EndHorizontal();
    }

    void DrawBindItem()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - ContentHight), GUILayout.ExpandWidth(true));
        {
            for (int i = 0; i < this.showAmount; i++)
            {
                int showIndex = (this.currentIndex - 1) * this.showAmount + i;
                if (showIndex >= selectBindAmount) break;
                if (selectBindAmount == 0) break;

                DrawBindInfoItem(showIndex);
            }
            GUILayout.Space(1);
        }
        EditorGUILayout.EndVertical();
    }

    void DrawBindInfoItem(int index)
    {
        BindData bindData = this.selectBindDataList[index];
        EditorGUILayout.BeginHorizontal("frameBox");
        {
            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button(bindData.GetTypeName(), "MiniPopup")) BindDataHelper.DropDownBinDataTypeSwitch(bindData);
                EditorGUILayout.ObjectField(bindData.GetValue(), bindData.GetValue().GetType(), true);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200f));
            {
                GUILayout.Label("名称");
                string tempComponentBindInfoName = GUILayout.TextField(bindData.name);
                if (tempComponentBindInfoName != bindData.name) bindData.name = CommonTools.GetNumberAlpha(bindData.name);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("操作", GUILayout.Width(100))) { }
                if (GUILayout.Button("删除", GUILayout.Width(100))) { }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawBindCollection()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - ContentHight), GUILayout.ExpandWidth(true));
        {
            for (int i = 0; i < this.showAmount; i++)
            {
                int showIndex = (this.currentIndex - 1) * this.showAmount + i;
                if (showIndex >= selectbindCollectionAmount) break;
                if (selectbindCollectionAmount == 0) break;

                DrawBindCollection(showIndex);
            }
            GUILayout.Space(1);
        }
        EditorGUILayout.EndVertical();
    }

    void DrawBindCollection(int index)
    {
        BindCollection bindCollection = this.selectbindCollectionList[index];
        EditorGUILayout.BeginHorizontal("frameBox");
        {
            EditorGUILayout.BeginVertical();
            {
                bindCollection.index = EditorGUILayout.Popup(bindCollection.index, bindCollection.GetTypeStrings());
                bindCollection.collectionType = (CollectionType) EditorGUILayout.EnumPopup(bindCollection.collectionType);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200f));
            {
                GUILayout.Label("名称");
                string tempComponentBindInfoName = GUILayout.TextField(bindCollection.name);
                if (tempComponentBindInfoName != bindCollection.name) bindCollection.name = CommonTools.GetNumberAlpha(bindCollection.name);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("编辑", GUILayout.Width(100))) { }
                if (GUILayout.Button("删除", GUILayout.Width(100))) { }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
}