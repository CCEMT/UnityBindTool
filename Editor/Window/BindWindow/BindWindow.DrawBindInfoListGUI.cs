using System.Collections.Generic;
using BindTool;
using UnityEditor;
using UnityEngine;

public partial class BindWindow
{
    private const float ItemHight = 50;
    private const float ItemInterval = 2.25f;
    private const float ContentHight = 240;

    private string bindInputString = "";

    private SearchType searchType;
    private BindTypeIndex bindTypeIndex;

    private int componentBindAmount;
    private List<ComponentBindInfo> selectComponentList;
    private int selectComponentAmount;

    private int dataBindAmount;
    private List<DataBindInfo> selectDataList;
    private int selectDataAmount;

    private int componentCollectionBindAmount;
    private List<ComponentCollectionBindInfo> selectComponentCollectionList;
    private int selectComponentCollectionAmount;

    private int dataCollectionBindAmount;
    private List<DataCollectionBindInfo> selectDataCollectionList;
    private int selectDataCollectionAmount;

    private int showAmount;
    private int currentIndex;
    private int maxIndex;

    void BindInfoListInit()
    {
        this.bindInputString = "";
        this.searchType = SearchType.All;
        this.bindTypeIndex = BindTypeIndex.Item;

        this.selectComponentList = new List<ComponentBindInfo>();
        this.selectDataList = new List<DataBindInfo>();
        this.selectComponentCollectionList = new List<ComponentCollectionBindInfo>();
        this.selectDataCollectionList = new List<DataCollectionBindInfo>();

        GetData();
        SearchSelectList();
    }

    void DrawBindInfoListGUI()
    {
        GetData();
        DrawOperate();
        DrawBindArea();
        DrawBindInfo();
    }

    void GetData()
    {
        if (this.selectComponentList == null)
        {
            this.selectComponentList = new List<ComponentBindInfo>();
            SearchSelectList();
        }

        int tempAmount = (int) ((position.height - ContentHight) / (ItemHight + ItemInterval));
        if (tempAmount == this.showAmount) return;
        this.showAmount = tempAmount;
        SearchSelectList();
    }

    void DrawOperate()
    {
        EditorGUILayout.BeginVertical("frameBox");
        {
            DrawBuild();
            DrawBind();
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
                ObjectInfoHelper.BindAll(this.editorObjectInfo, this.bindObject);
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
            Object[] bindTargets = DragAndDrop.objectReferences;
            BindHelper.BindTarget(this.editorObjectInfo, bindTargets);
        }
        currentEvent.Use();
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
        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
        menu.AddItem(new GUIContent("重置搜索名称"), false, ResetSearchContent);
        menu.AddItem(new GUIContent("查找重复名称的Item"), false, FindRepetitionNameItem);
        menu.ShowAsContext(); //显示菜单
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
        EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - ContentHight));
        {
            for (int i = 0; i < this.showAmount; i++)
            {
                int showIndex = (this.currentIndex - 1) * this.showAmount + i;
                if (showIndex >= this.selectComponentAmount + this.selectDataAmount) break;
                if (this.selectComponentAmount + this.selectDataAmount == 0) break;

                if (showIndex < this.selectComponentAmount) { DrawComponentInfoItem(showIndex); }
                else { DrawDataInfoItem(showIndex - this.selectComponentAmount); }
            }
            GUILayout.Space(1);
        }
        EditorGUILayout.EndVertical();
    }

    void DrawComponentInfoItem(int index)
    {
        ComponentBindInfo componentBindInfo = this.selectComponentList[index];
        EditorGUILayout.BeginHorizontal("frameBox");
        {
            EditorGUILayout.BeginVertical();
            {
                componentBindInfo.index = EditorGUILayout.Popup(componentBindInfo.index, componentBindInfo.GetTypeStrings());
                EditorGUILayout.ObjectField(componentBindInfo.GetValue(), componentBindInfo.GetValue().GetType(), true);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200f));
            {
                GUILayout.Label("名称");
                string tempComponentBindInfoName = GUILayout.TextField(componentBindInfo.name);
                if (tempComponentBindInfoName != componentBindInfo.name) componentBindInfo.name = CommonTools.GetNumberAlpha(componentBindInfo.name);
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

    void DrawDataInfoItem(int index)
    {
        DataBindInfo dataInfo = this.selectDataList[index];

        EditorGUILayout.BeginHorizontal("frameBox");
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label(dataInfo.typeString.typeName);
                EditorGUILayout.ObjectField(dataInfo.bindObject, dataInfo.typeString.ToType(), true);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200f));
            {
                GUILayout.Label("名称");
                string tempComponentBindInfoName = GUILayout.TextField(dataInfo.name);
                if (tempComponentBindInfoName != dataInfo.name) dataInfo.name = CommonTools.GetNumberAlpha(dataInfo.name);
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
        EditorGUILayout.BeginVertical("box", GUILayout.Height(position.height - ContentHight));
        {
            for (int i = 0; i < this.showAmount; i++)
            {
                int showIndex = (this.currentIndex - 1) * this.showAmount + i;
                if (showIndex >= this.selectComponentCollectionAmount + this.selectDataCollectionAmount) break;
                if (this.selectComponentCollectionAmount + this.selectDataCollectionAmount == 0) break;

                if (showIndex < this.selectComponentCollectionAmount) { DrawDataCollectionList(showIndex); }
                else { DrawComponentCollectionInfoItem(showIndex - this.selectComponentCollectionAmount); }
            }
            GUILayout.Space(1);
        }
        EditorGUILayout.EndVertical();
    }

    void DrawDataCollectionList(int index)
    {
        ComponentCollectionBindInfo componentCollectionBindInfo = this.selectComponentCollectionList[index];
        EditorGUILayout.BeginHorizontal("frameBox");
        {
            EditorGUILayout.BeginVertical();
            {
                componentCollectionBindInfo.index = EditorGUILayout.Popup(componentCollectionBindInfo.index, componentCollectionBindInfo.GetTypeStrings());
                GUILayout.Label(componentCollectionBindInfo.collectionType.ToString());
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200f));
            {
                GUILayout.Label("名称");
                string tempComponentBindInfoName = GUILayout.TextField(componentCollectionBindInfo.name);
                if (tempComponentBindInfoName != componentCollectionBindInfo.name) componentCollectionBindInfo.name = CommonTools.GetNumberAlpha(componentCollectionBindInfo.name);
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

    void DrawComponentCollectionInfoItem(int index)
    {
        DataCollectionBindInfo dataCollectionBindInfo = this.selectDataCollectionList[index];
        EditorGUILayout.BeginHorizontal("frameBox");
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label(dataCollectionBindInfo.targetType.typeName);
                GUILayout.Label(dataCollectionBindInfo.collectionType.ToString());
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(200f));
            {
                GUILayout.Label("名称");
                string tempComponentBindInfoName = GUILayout.TextField(dataCollectionBindInfo.name);
                if (tempComponentBindInfoName != dataCollectionBindInfo.name) dataCollectionBindInfo.name = CommonTools.GetNumberAlpha(dataCollectionBindInfo.name);
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