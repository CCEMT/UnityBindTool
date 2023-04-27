#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BindTool
{
    public partial class BindWindow
    {
        enum SearchType
        {
            All,
            TypeName,
            GameObjectName,
            VariableName
        }

        private string bindInputString = "";
        private SearchType searchType;

        private int componentBindAmount;
        private List<ComponentBindInfo> selectComponentList;
        private int selectComponentAmount;

        private int dataBindAmount;
        private List<DataBindInfo> selectDataList;
        private int selectDataAmount;

        private const int BindListShowAmount = 5;
        public int bindListMaxIndex;
        private int bindListIndex = 1;

        void DrawBindGUI()
        {
            bool tempIsCustomBind = GUILayout.Toggle(this.commonSettingData.isCustomBind, "是否自定义绑定（不勾选则全部绑定）");
            if (this.commonSettingData.isCustomBind != tempIsCustomBind)
            {
                this.commonSettingData.isCustomBind = tempIsCustomBind;
                this.isSavaSetting = true;
            }

            EditorGUI.BeginDisabledGroup(this.commonSettingData.isCustomBind == false);
            {
                BindInfo();
                BindArea();

                if (this.componentBindAmount != this.objectInfo.gameObjectBindInfoList.Count || this.dataBindAmount != this.objectInfo.dataBindInfoList.Count)
                {
                    this.componentBindAmount = this.objectInfo.gameObjectBindInfoList.Count;
                    this.dataBindAmount = this.objectInfo.dataBindInfoList.Count;
                    GetBindSelectList();
                }
                BindList();
            }
            EditorGUI.EndDisabledGroup();
        }

        void BindInfo()
        {
            string addErrorInfo = "错误：附加模式下选择的类型必须继承MonoBehaviour！";

            if (this.commonSettingData.selectScriptSetting.isGenerateNew == false)
            {
                EditorGUILayout.BeginHorizontal();
                {

                    GUILayout.Label("绑定的组件");

                    if (this.objectInfo.rootBindInfo.GetTypeName() == nameof(GameObject))
                    {
                        EditorGUILayout.ObjectField(this.objectInfo.rootBindInfo.instanceObject, this.objectInfo.rootBindInfo.GetValue().GetType(), true);
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(this.objectInfo.rootBindInfo.instanceObject.GetComponent(this.objectInfo.rootBindInfo.GetTypeName()),
                            this.objectInfo.rootBindInfo.GetTypeString().ToType(), true);
                    }

                    int tempRootBindInfoIndex = EditorGUILayout.Popup(this.objectInfo.rootBindInfo.index, this.objectInfo.rootBindInfo.GetTypeStrings());
                    if (tempRootBindInfoIndex != this.objectInfo.rootBindInfo.index)
                    {
                        this.objectInfo.rootBindInfo.index = tempRootBindInfoIndex;
                        this.isSavaSetting = true;
                    }

                    if (GUILayout.Button("生成", GUILayout.Width(50)))
                    {
                        this.commonSettingData.selectScriptSetting.isGenerateNew = true;
                        this.isSavaSetting = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                Type monoType = typeof(MonoBehaviour);
                Type type = this.objectInfo.rootBindInfo.GetTypeString().ToType();

                if (monoType.IsAssignableFrom(type) == false)
                {
                    GUI.color = Color.red;
                    if (this.errorList.Contains(addErrorInfo) == false) this.errorList.Add(addErrorInfo);
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(addErrorInfo);
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }
                else
                {
                    if (this.errorList.Contains(addErrorInfo)) this.errorList.Remove(addErrorInfo);
                }
                this.commonSettingData.mergeTypeString = this.objectInfo.rootBindInfo.GetTypeString();
            }
            else
            {
                if (this.errorList.Contains(addErrorInfo)) this.errorList.Remove(addErrorInfo);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("生成脚本的名称：");
                    string content = GUILayout.TextField(this.commonSettingData.newScriptName);
                    this.commonSettingData.newScriptName = CommonTools.GetNumberAlpha(content);
                    if (GUILayout.Button("设置为物体名")) this.commonSettingData.newScriptName = CommonTools.GetNumberAlpha(this.bindObject.name);
                    if (GUILayout.Button("附加", GUILayout.Width(50))) this.commonSettingData.selectScriptSetting.isGenerateNew = false;
                }
                EditorGUILayout.EndHorizontal();

                string directoryName = "";
                if (this.commonSettingData.isCreateScriptFolder) directoryName = this.commonSettingData.newScriptName + "/";
                string path = $"{Application.dataPath}/{this.commonSettingData.createScriptPath}/{directoryName}{this.commonSettingData.newScriptName}.cs";
                if (File.Exists(path))
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("提示：目标路径已存在对应脚本，生成时将会覆盖目标路径的脚本");
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUI.color = Color.green;
                if (GUILayout.Button("全部自动绑定", GUILayout.Width(250)))
                {
                    Transform[] gameObjects = this.bindObject.GetComponentsInChildren<Transform>(true);
                    int amount = gameObjects.Length;
                    for (int i = 0; i < amount; i++)
                    {
                        Transform go = gameObjects[i];
                        ComponentBindInfo info = this.objectInfo.AutoBind(go, this.commonSettingData.selectAutoBindSetting);
                        if (info != null)
                        {
                            if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                info.name = CommonTools.SetVariableName(info.instanceObject.name, this.commonSettingData.selectCreateNameSetting);
                            info.name = CommonTools.NameSettingByName(info, this.commonSettingData.selectScriptSetting.nameSetting);
                        }
                    }
                    this.isSavaSetting = true;
                }

                GUI.color = Color.red;

                if (GUILayout.Button("解除所有绑定"))
                {
                    GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
                    menu.AddItem(new GUIContent("取消解除"), false, () => { });
                    menu.AddItem(new GUIContent("确认解除"), false, () => {
                        this.objectInfo.gameObjectBindInfoList.Clear();
                        this.isSavaSetting = true;
                    }); //向菜单中添加菜单项

                    menu.ShowAsContext(); //显示菜单
                }

                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        void BindArea()
        {
            Event e = Event.current;
            GUI.color = Color.green;
            //绘制一个监听区域
            Rect dragArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            GUIContent guiContent = new GUIContent("将组件拖拽至此进行绑定");
            GUI.Box(dragArea, guiContent);

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dragArea.Contains(e.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            int amount = DragAndDrop.objectReferences.Length;
                            if (amount == 1)
                            {
                                Object selectObject = DragAndDrop.objectReferences[0];
                                GameObject go = selectObject as GameObject;
                                if (go != null)
                                {
                                    ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);

                                    GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                                    int typeAmount = componentBindInfo.typeStrings.Length;
                                    for (int i = 0; i < typeAmount; i++)
                                    {
                                        int infoIndex = i;
                                        menu.AddItem(new GUIContent(componentBindInfo.typeStrings[i].typeName), false, () => {
                                            this.objectInfo.Bind(componentBindInfo, infoIndex);
                                            if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                            {
                                                componentBindInfo.name = CommonTools.SetVariableName(componentBindInfo.instanceObject.name, this.commonSettingData.selectCreateNameSetting);
                                            }
                                            componentBindInfo.name = CommonTools.NameSettingByName(componentBindInfo, this.commonSettingData.selectScriptSetting.nameSetting);
                                            this.isSavaSetting = true;
                                        });
                                    }
                                    menu.ShowAsContext(); //显示菜单
                                }
                                else
                                {
                                    DataBindInfo dataBindInfo = new DataBindInfo(selectObject);
                                    if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                    {
                                        dataBindInfo.name = CommonTools.SetVariableName(dataBindInfo.bindObject.name, this.commonSettingData.selectCreateNameSetting);
                                    }
                                    dataBindInfo.name = CommonTools.NameSettingByName(dataBindInfo, this.commonSettingData.selectScriptSetting.nameSetting);
                                    if (dataBindInfo.typeString.assemblyName.Equals("Assembly-CSharp-Editor") == false) { this.objectInfo.dataBindInfoList.Add(dataBindInfo); }
                                    else { Debug.Log("禁止绑定编辑器中的数据"); }
                                }
                            }
                            else if (amount > 1)
                            {
                                List<ComponentBindInfo> bindList = new List<ComponentBindInfo>();
                                List<TypeString> bindTypeList = new List<TypeString>();
                                for (int i = 0; i < amount; i++)
                                {
                                    Object selectObject = DragAndDrop.objectReferences[i];
                                    GameObject go = selectObject as GameObject;
                                    if (go != null)
                                    {
                                        ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);
                                        bindList.Add(componentBindInfo);

                                        if (bindTypeList.Count > 0) { bindTypeList = bindTypeList.Intersect(componentBindInfo.typeStrings).ToList(); }
                                        else { bindTypeList.AddRange(componentBindInfo.typeStrings); }
                                    }
                                    else
                                    {
                                        DataBindInfo dataBindInfo = new DataBindInfo(selectObject);
                                        if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                        {
                                            dataBindInfo.name = CommonTools.SetVariableName(dataBindInfo.bindObject.name, this.commonSettingData.selectCreateNameSetting);
                                        }
                                        dataBindInfo.name = CommonTools.NameSettingByName(dataBindInfo, this.commonSettingData.selectScriptSetting.nameSetting);
                                        if (dataBindInfo.typeString.assemblyName.Equals("Assembly-CSharp-Editor") == false) { this.objectInfo.dataBindInfoList.Add(dataBindInfo); }
                                        else { Debug.Log("禁止绑定编辑器中的数据"); }
                                    }
                                }

                                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                                menu.AddItem(new GUIContent("自动获取类型"), false, () => {
                                    int bindComponentAmount = bindList.Count;
                                    for (int j = 0; j < bindComponentAmount; j++)
                                    {
                                        ComponentBindInfo info = bindList[j];
                                        this.objectInfo.AutoBind(info, this.commonSettingData.selectAutoBindSetting);
                                        if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                        {
                                            info.name = CommonTools.SetVariableName(info.instanceObject.name, this.commonSettingData.selectCreateNameSetting);
                                        }
                                        info.name = CommonTools.NameSettingByName(info, this.commonSettingData.selectScriptSetting.nameSetting);
                                    }
                                    this.isSavaSetting = true;
                                });

                                int typeAmount = bindTypeList.Count;
                                for (int i = 0; i < typeAmount; i++)
                                {
                                    TypeString typeString = bindTypeList[i];
                                    menu.AddItem(new GUIContent(typeString.typeName), false, () => {
                                        int bindComponentAmount = bindList.Count;
                                        for (int j = 0; j < bindComponentAmount; j++)
                                        {
                                            ComponentBindInfo info = bindList[j];
                                            int infoTypeAmount = info.typeStrings.Length;
                                            for (int k = 0; k < infoTypeAmount; k++)
                                            {
                                                if (info.typeStrings[k].Equals(typeString))
                                                {
                                                    this.objectInfo.Bind(info, k);
                                                    if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                                    {
                                                        info.name = CommonTools.SetVariableName(info.instanceObject.name, this.commonSettingData.selectCreateNameSetting);
                                                    }
                                                    info.name = CommonTools.NameSettingByName(info, this.commonSettingData.selectScriptSetting.nameSetting);
                                                }
                                            }
                                        }
                                        this.isSavaSetting = true;
                                    });
                                }
                                menu.ShowAsContext(); //显示菜单
                            }
                        }
                        e.Use();
                    }
                    break;
                case EventType.DragExited:
                    break;
            }

            GUI.color = Color.white;
        }

        void BindList()
        {
            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Search Item：");
                    this.searchType = (SearchType) EditorGUILayout.EnumPopup(this.searchType, GUILayout.Width(150));

                    if (GUILayout.Button("Reset", GUILayout.Width(50)))
                    {
                        this.bindInputString = "";
                        GetBindSelectList();
                    }
                }
                EditorGUILayout.EndHorizontal();

                string tempString = GUILayout.TextField(this.bindInputString, "SearchTextField");
                if (tempString.Equals(this.bindInputString) == false)
                {
                    this.bindInputString = tempString;
                    GetBindSelectList();
                }

            }
            GUILayout.EndVertical();

            if (this.selectComponentAmount + this.selectDataAmount > 0)
            {
                GUILayout.BeginVertical("box", GUILayout.Height(272.5f));
                {
                    for (int i = 0; i < BindListShowAmount; i++)
                    {
                        int showIndex = (this.bindListIndex - 1) * BindListShowAmount + i;

                        if (showIndex >= this.selectComponentAmount + this.selectDataAmount) { break; }

                        if (showIndex < this.selectComponentAmount)
                        {
                            //显示选择的组件
                            EditorGUILayout.BeginHorizontal("frameBox");
                            {
                                EditorGUILayout.BeginVertical();
                                ComponentBindInfo componentBindInfo = this.selectComponentList[showIndex];
                                int itemIndex = this.objectInfo.gameObjectBindInfoList.IndexOf(componentBindInfo);

                                if (itemIndex == -1) GetBindSelectList();

                                int tempComponentBindInfoIndex = EditorGUILayout.Popup(componentBindInfo.index, componentBindInfo.GetTypeStrings());
                                if (tempComponentBindInfoIndex != componentBindInfo.index)
                                {
                                    this.objectInfo.gameObjectBindInfoList[itemIndex].index = tempComponentBindInfoIndex;
                                    this.isSavaSetting = true;
                                }
                                if (componentBindInfo.GetTypeName() == nameof(GameObject))
                                {
                                    EditorGUILayout.ObjectField(componentBindInfo.instanceObject, componentBindInfo.GetValue().GetType(), true);
                                }
                                else { EditorGUILayout.ObjectField(componentBindInfo.GetValue(), componentBindInfo.GetValue().GetType(), true); }

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical();

                                GUILayout.Label("变量名");
                                string tempComponentBindInfoName = CommonTools.GetNumberAlpha(GUILayout.TextField(componentBindInfo.name));
                                if (tempComponentBindInfoName != componentBindInfo.name)
                                {
                                    this.objectInfo.gameObjectBindInfoList[itemIndex].name = tempComponentBindInfoName;
                                    this.isSavaSetting = true;
                                }

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical();

                                if (GUILayout.Button("操作", GUILayout.Width(100)))
                                {
                                    GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
                                    menu.AddItem(new GUIContent("设置默认名"), false, () => {

                                        componentBindInfo.name = componentBindInfo.instanceObject.name;
                                        if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                        {
                                            componentBindInfo.name = CommonTools.SetVariableName(componentBindInfo.instanceObject.name, this.commonSettingData.selectCreateNameSetting);
                                        }
                                        componentBindInfo.name = CommonTools.NameSettingByName(componentBindInfo, this.commonSettingData.selectScriptSetting.nameSetting);

                                        this.isSavaSetting = true;
                                    });

                                    menu.ShowAsContext(); //显示菜单
                                }

                                GUI.color = Color.red;
                                if (GUILayout.Button("删除", GUILayout.Width(100)))
                                {
                                    this.objectInfo.gameObjectBindInfoList.RemoveAt(itemIndex);
                                    this.isSavaSetting = true;
                                    break;
                                }
                                GUI.color = Color.white;

                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            //显示选择的数据
                            showIndex -= this.selectComponentAmount;
                            DataBindInfo dataInfo = this.selectDataList[showIndex];
                            int itemIndex = this.objectInfo.dataBindInfoList.IndexOf(dataInfo);

                            if (itemIndex == -1) GetBindSelectList();

                            EditorGUILayout.BeginHorizontal("frameBox");
                            {
                                EditorGUILayout.BeginVertical();
                                {
                                    GUILayout.Label(dataInfo.typeString.typeName);

                                    EditorGUILayout.ObjectField(dataInfo.bindObject, dataInfo.typeString.ToType(), true);

                                }
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical();
                                {
                                    GUILayout.Label("变量名");
                                    string tempComponentBindInfoName = CommonTools.GetNumberAlpha(GUILayout.TextField(dataInfo.name));
                                    if (tempComponentBindInfoName != dataInfo.name)
                                    {
                                        this.objectInfo.dataBindInfoList[itemIndex].name = tempComponentBindInfoName;
                                        this.isSavaSetting = true;
                                    }
                                }
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical();
                                {

                                    if (GUILayout.Button("操作", GUILayout.Width(100)))
                                    {
                                        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
                                        menu.AddItem(new GUIContent("设置默认名"), false, () => {
                                            dataInfo.name = dataInfo.bindObject.name;
                                            if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
                                            {
                                                dataInfo.name = CommonTools.SetVariableName(dataInfo.bindObject.name, this.commonSettingData.selectCreateNameSetting);
                                            }
                                            dataInfo.name = CommonTools.NameSettingByName(dataInfo, this.commonSettingData.selectScriptSetting.nameSetting);

                                            this.isSavaSetting = true;
                                        });

                                        menu.ShowAsContext(); //显示菜单
                                    }

                                    GUI.color = Color.red;
                                    if (GUILayout.Button("删除", GUILayout.Width(100)))
                                    {
                                        this.objectInfo.dataBindInfoList.RemoveAt(itemIndex);
                                        this.isSavaSetting = true;
                                        break;
                                    }
                                    GUI.color = Color.white;
                                }
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                {

                    GUILayout.Label($"当前页数：{this.bindListIndex}  /  总页数：{this.bindListMaxIndex}");

                    if (GUILayout.Button("上一页", GUILayout.Width(100)))
                    {
                        this.bindListIndex--;
                        this.bindListIndex = Mathf.Clamp(this.bindListIndex, 1, this.bindListMaxIndex);

                    }

                    if (GUILayout.Button("下一页", GUILayout.Width(100)))
                    {
                        this.bindListIndex++;
                        this.bindListIndex = Mathf.Clamp(this.bindListIndex, 1, this.bindListMaxIndex);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        void GetBindSelectList()
        {
            this.selectComponentList = new List<ComponentBindInfo>();
            int bindComponentAmount = this.objectInfo.gameObjectBindInfoList.Count;
            this.selectDataList = new List<DataBindInfo>();
            int bindDataAmount = this.objectInfo.dataBindInfoList.Count;
            switch (this.searchType)
            {
                case SearchType.All:
                    for (int i = 0; i < bindComponentAmount; i++)
                    {
                        ComponentBindInfo info = this.objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.GetTypeName(), this.bindInputString)
                            || CommonTools.Search(info.GetObject().name, this.bindInputString)
                            || CommonTools.Search(info.name, this.bindInputString)) { this.selectComponentList.Add(info); }
                    }
                    for (int i = 0; i < bindDataAmount; i++)
                    {
                        DataBindInfo info = this.objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.typeString.typeName, this.bindInputString)
                            || CommonTools.Search(info.bindObject.name, this.bindInputString)
                            || CommonTools.Search(info.name, this.bindInputString)) { this.selectDataList.Add(info); }
                    }
                    break;
                case SearchType.TypeName:
                    for (int i = 0; i < bindComponentAmount; i++)
                    {
                        ComponentBindInfo info = this.objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.GetTypeName(), this.bindInputString)) this.selectComponentList.Add(info);
                    }
                    for (int i = 0; i < bindDataAmount; i++)
                    {
                        DataBindInfo info = this.objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.typeString.typeName, this.bindInputString)) this.selectDataList.Add(info);
                    }
                    break;
                case SearchType.GameObjectName:
                    for (int i = 0; i < bindComponentAmount; i++)
                    {
                        ComponentBindInfo info = this.objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.GetObject().name, this.bindInputString)) this.selectComponentList.Add(info);
                    }
                    for (int i = 0; i < bindDataAmount; i++)
                    {
                        DataBindInfo info = this.objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.bindObject.name, this.bindInputString)) this.selectDataList.Add(info);
                    }
                    break;
                case SearchType.VariableName:
                    for (int i = 0; i < bindComponentAmount; i++)
                    {
                        ComponentBindInfo info = this.objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.name, this.bindInputString)) this.selectComponentList.Add(info);
                    }
                    for (int i = 0; i < bindDataAmount; i++)
                    {
                        DataBindInfo info = this.objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.name, this.bindInputString)) this.selectDataList.Add(info);
                    }
                    break;
            }

            this.selectComponentAmount = this.selectComponentList.Count;
            this.selectDataAmount = this.objectInfo.dataBindInfoList.Count;

            this.bindListMaxIndex = (int) Math.Ceiling((this.selectComponentAmount + this.selectDataAmount) / (double) BindListShowAmount);
            this.bindListIndex = Mathf.Clamp(this.bindListIndex, 1, this.bindListMaxIndex);
        }

        void BindComponent(GameObject bindObject, int index)
        {
            ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindObject);
            this.objectInfo.Bind(componentBindInfo, index);
            if (_bindWindow.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) componentBindInfo.name = CommonTools.GetNumberAlpha(componentBindInfo.instanceObject.name);
            if (this.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName)
            {
                componentBindInfo.name = CommonTools.SetVariableName(componentBindInfo.instanceObject.name, this.commonSettingData.selectCreateNameSetting);
            }
            componentBindInfo.name = CommonTools.NameSettingByName(componentBindInfo, this.commonSettingData.selectScriptSetting.nameSetting);
            _bindWindow.isSavaSetting = true;
        }

        void SelectBindInfo(int index)
        {
            this.selectComponentList = new List<ComponentBindInfo>();
            this.selectComponentList.Add(this.objectInfo.gameObjectBindInfoList[index]);
            this.selectComponentAmount = 1;

            this.bindListMaxIndex = 1;
            this.bindListIndex = 1;
        }

        void RemoveBindInfo(GameObject removeObject, RemoveType removeType)
        {
            switch (removeType)
            {
                case RemoveType.This:
                    int thisBindInfoAmount = this.objectInfo.gameObjectBindInfoList.Count;
                    for (int i = thisBindInfoAmount - 1; i >= 0; i--)
                    {
                        ComponentBindInfo componentBindInfo = this.objectInfo.gameObjectBindInfoList[i];
                        if (componentBindInfo.instanceObject == removeObject) this.objectInfo.gameObjectBindInfoList.RemoveAt(i);
                    }
                    break;
                case RemoveType.Child:
                {
                    Transform[] transforms = removeObject.GetComponentsInChildren<Transform>(true);
                    int amount = transforms.Length;
                    for (int i = 0; i < amount; i++)
                    {
                        Transform transform = transforms[i];
                        if (transform.gameObject != removeObject)
                        {
                            int childBindInfoAmount = this.objectInfo.gameObjectBindInfoList.Count;
                            for (int j = childBindInfoAmount - 1; j >= 0; j--)
                            {
                                ComponentBindInfo componentBindInfo = this.objectInfo.gameObjectBindInfoList[j];
                                if (componentBindInfo.instanceObject == transform.gameObject) this.objectInfo.gameObjectBindInfoList.RemoveAt(j);
                            }
                        }
                    }
                    break;
                }
                case RemoveType.ThisAndChild:
                {
                    Transform[] transforms = removeObject.GetComponentsInChildren<Transform>(true);
                    int amount = transforms.Length;
                    for (int i = 0; i < amount; i++)
                    {
                        Transform transform = transforms[i];
                        int childBindInfoAmount = this.objectInfo.gameObjectBindInfoList.Count;
                        for (int j = childBindInfoAmount - 1; j >= 0; j--)
                        {
                            ComponentBindInfo componentBindInfo = this.objectInfo.gameObjectBindInfoList[j];
                            if (componentBindInfo.instanceObject == transform.gameObject) this.objectInfo.gameObjectBindInfoList.RemoveAt(j);
                        }
                    }
                    break;
                }
            }

            this.isSavaSetting = true;
        }
    }
}