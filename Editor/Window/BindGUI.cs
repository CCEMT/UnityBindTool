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

        private Vector2 bindScrollPosition;

        private string bindInputString = "";
        private SearchType searchType;

        private int componentBindAmount;
        private List<ComponentBindInfo> selectComponentList;
        private int selectComponentAmount;

        private int dataBindAmount;
        private List<DataBindInfo> selectDataList;
        private int selectDataAmount;

        void DrawBindGUI()
        {
            bool tempIsCustomBind = GUILayout.Toggle(commonSettingData.isCustomBind, "是否自定义绑定（不勾选则全部绑定）");
            if (commonSettingData.isCustomBind != tempIsCustomBind) {
                commonSettingData.isCustomBind = tempIsCustomBind;
                isSavaSetting = true;
            }

            EditorGUI.BeginDisabledGroup(commonSettingData.isCustomBind == false);
            BindInfo();
            BindArea();

            if (componentBindAmount != objectInfo.gameObjectBindInfoList.Count || dataBindAmount != objectInfo.dataBindInfoList.Count) {
                componentBindAmount = objectInfo.gameObjectBindInfoList.Count;
                dataBindAmount = objectInfo.dataBindInfoList.Count;
                GetBindSelectList();
            }
            BindList();


            EditorGUI.EndDisabledGroup();
        }

        void BindInfo()
        {
            string addErrorInfo = "错误：附加模式下选择的类型必须继承MonoBehaviour！";

            if (commonSettingData.selectScriptSetting.isGenerateNew == false) {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("绑定的组件");

                if (objectInfo.rootBindInfo.GetTypeName() == nameof(GameObject)) {
                    EditorGUILayout.ObjectField(objectInfo.rootBindInfo.instanceObject, objectInfo.rootBindInfo.GetValue().GetType(), true);
                }
                else {
                    EditorGUILayout.ObjectField(objectInfo.rootBindInfo.instanceObject.GetComponent(objectInfo.rootBindInfo.GetTypeName()), objectInfo.rootBindInfo.GetTypeString().ToType(), true);
                }

                int tempRootBindInfoIndex = EditorGUILayout.Popup(objectInfo.rootBindInfo.index, objectInfo.rootBindInfo.GetTypeStrings());
                if (tempRootBindInfoIndex != objectInfo.rootBindInfo.index) {
                    objectInfo.rootBindInfo.index = tempRootBindInfoIndex;
                    isSavaSetting = true;
                }

                if (GUILayout.Button("生成", GUILayout.Width(50))) {
                    commonSettingData.selectScriptSetting.isGenerateNew = true;
                    isSavaSetting = true;
                }

                EditorGUILayout.EndHorizontal();

                Type monoType = typeof(MonoBehaviour);
                Type type = objectInfo.rootBindInfo.GetTypeString().ToType();

                if (monoType.IsAssignableFrom(type) == false) {
                    GUI.color = Color.red;
                    if (errorList.Contains(addErrorInfo) == false) errorList.Add(addErrorInfo);
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(addErrorInfo);
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }
                else {
                    if (errorList.Contains(addErrorInfo)) errorList.Remove(addErrorInfo);
                }
                commonSettingData.addTypeString = objectInfo.rootBindInfo.GetTypeString();
            }
            else {
                if (errorList.Contains(addErrorInfo)) errorList.Remove(addErrorInfo);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("生成脚本的名称：");
                string content = GUILayout.TextField(commonSettingData.newScriptName);
                commonSettingData.newScriptName = CommonTools.GetNumberAlpha(content);
                if (GUILayout.Button("设置为物体名")) commonSettingData.newScriptName = CommonTools.GetNumberAlpha(bindObject.name);
                if (GUILayout.Button("附加", GUILayout.Width(50))) commonSettingData.selectScriptSetting.isGenerateNew = false;
                EditorGUILayout.EndHorizontal();

                string directoryName = "";
                if (commonSettingData.isCreateScriptFolder) directoryName = commonSettingData.newScriptName + "/";
                string path = $"{Application.dataPath}/{commonSettingData.createScriptPath}/{directoryName}{commonSettingData.newScriptName}.cs";
                if (File.Exists(path)) {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("提示：目标路径已存在对应脚本，生成时将会覆盖目标路径的脚本");
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();

            GUI.color = Color.green;
            if (GUILayout.Button("全部自动绑定", GUILayout.Width(250))) {
                Transform[] gameObjects = bindObject.GetComponentsInChildren<Transform>(true);
                int amount = gameObjects.Length;
                for (int i = 0; i < amount; i++) {
                    Transform go = gameObjects[i];
                    ComponentBindInfo info = objectInfo.AutoBind(go, commonSettingData.selectAutoBindSetting);
                    if (info != null) {
                        if (commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) info.name = CommonTools.SetName(info.instanceObject.name, commonSettingData.selectCreateNameSetting);
                    }
                }
                isSavaSetting = true;
            }

            GUI.color = Color.red;

            if (GUILayout.Button("解除所有绑定")) {
                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
                menu.AddItem(new GUIContent("取消解除"), false, () => { });
                menu.AddItem(new GUIContent("确认解除"), false, () => {
                    objectInfo.gameObjectBindInfoList.Clear();
                    isSavaSetting = true;
                }); //向菜单中添加菜单项

                menu.ShowAsContext(); //显示菜单
            }

            GUI.color = Color.white;

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

            switch (e.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dragArea.Contains(e.mousePosition)) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (e.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag();

                            int amount = DragAndDrop.objectReferences.Length;
                            if (amount == 1) {
                                Object selectObject = DragAndDrop.objectReferences[0];
                                GameObject go = selectObject as GameObject;
                                if (go != null) {
                                    ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);

                                    GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                                    int typeAmount = componentBindInfo.typeStrings.Length;
                                    for (int i = 0; i < typeAmount; i++) {
                                        int infoIndex = i;
                                        menu.AddItem(new GUIContent(componentBindInfo.typeStrings[i].typeName), false, () => {
                                            objectInfo.Bind(componentBindInfo, infoIndex);
                                            if (commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) {
                                                componentBindInfo.name = CommonTools.SetName(componentBindInfo.instanceObject.name, commonSettingData.selectCreateNameSetting);
                                            }
                                            isSavaSetting = true;
                                        });
                                    }
                                    menu.ShowAsContext(); //显示菜单
                                }
                                else {
                                    DataBindInfo dataBindInfo = new DataBindInfo(selectObject);
                                    dataBindInfo.name = CommonTools.SetName(dataBindInfo.bindObject.name, commonSettingData.selectCreateNameSetting);
                                    if (dataBindInfo.typeString.assemblyName.Equals("Assembly-CSharp-Editor") == false) { objectInfo.dataBindInfoList.Add(dataBindInfo); }
                                    else { Debug.Log("禁止绑定编辑器中的数据"); }
                                }
                            }
                            else if (amount > 1) {
                                List<ComponentBindInfo> bindList = new List<ComponentBindInfo>();
                                List<TypeString> bindTypeList = new List<TypeString>();
                                for (int i = 0; i < amount; i++) {
                                    Object selectObject = DragAndDrop.objectReferences[i];
                                    GameObject go = selectObject as GameObject;
                                    if (go != null) {
                                        ComponentBindInfo componentBindInfo = new ComponentBindInfo(go);
                                        bindList.Add(componentBindInfo);

                                        if (bindTypeList.Count > 0) { bindTypeList = bindTypeList.Intersect(componentBindInfo.typeStrings).ToList(); }
                                        else { bindTypeList.AddRange(componentBindInfo.typeStrings); }
                                    }
                                    else {
                                        DataBindInfo dataBindInfo = new DataBindInfo(selectObject);
                                        dataBindInfo.name = CommonTools.SetName(dataBindInfo.bindObject.name, commonSettingData.selectCreateNameSetting);
                                        if (dataBindInfo.typeString.assemblyName.Equals("Assembly-CSharp-Editor") == false) { objectInfo.dataBindInfoList.Add(dataBindInfo); }
                                        else { Debug.Log("禁止绑定编辑器中的数据"); }
                                    }
                                }

                                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                                menu.AddItem(new GUIContent("自动获取类型"), false, () => {
                                    int bindComponentAmount = bindList.Count;
                                    for (int j = 0; j < bindComponentAmount; j++) {
                                        ComponentBindInfo info = bindList[j];
                                        objectInfo.AutoBind(info, commonSettingData.selectAutoBindSetting);
                                        if (commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) {
                                            info.name = CommonTools.SetName(info.instanceObject.name, commonSettingData.selectCreateNameSetting);
                                        }
                                    }
                                    isSavaSetting = true;
                                });

                                int typeAmount = bindTypeList.Count;
                                for (int i = 0; i < typeAmount; i++) {
                                    TypeString typeString = bindTypeList[i];
                                    menu.AddItem(new GUIContent(typeString.typeName), false, () => {
                                        int bindComponentAmount = bindList.Count;
                                        for (int j = 0; j < bindComponentAmount; j++) {
                                            ComponentBindInfo info = bindList[j];
                                            int infoTypeAmount = info.typeStrings.Length;
                                            for (int k = 0; k < infoTypeAmount; k++) {
                                                if (info.typeStrings[k].Equals(typeString)) {
                                                    objectInfo.Bind(info, k);
                                                    if (commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) {
                                                        info.name = CommonTools.SetName(info.instanceObject.name, commonSettingData.selectCreateNameSetting);
                                                    }
                                                }
                                            }
                                        }
                                        isSavaSetting = true;
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
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search Item：");
            searchType = (SearchType) EditorGUILayout.EnumPopup(searchType, GUILayout.Width(150));

            if (GUILayout.Button("Reset", GUILayout.Width(50))) {
                bindInputString = "";
                GetBindSelectList();
            }

            EditorGUILayout.EndHorizontal();
            string tempString = GUILayout.TextField(bindInputString, "SearchTextField");
            if (tempString.Equals(bindInputString) == false) {
                bindInputString = tempString;
                GetBindSelectList();
            }

            GUILayout.EndHorizontal();

            float maxAmount = 5;
            float itemHeight = 54.5f;
            float height = Mathf.Clamp((selectComponentAmount + selectDataAmount) * itemHeight, 0, itemHeight * maxAmount);
            if (height > 0) {
                GUILayout.BeginVertical("box");
                bindScrollPosition = EditorGUILayout.BeginScrollView(bindScrollPosition, false, false, GUILayout.ExpandWidth(true), GUILayout.Height(height));

                for (int i = selectComponentAmount - 1; i >= 0; i--) {
                    GUILayout.BeginVertical("frameBox");

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    ComponentBindInfo componentBindInfo = selectComponentList[i];
                    int itemIndex = objectInfo.gameObjectBindInfoList.IndexOf(componentBindInfo);

                    if (itemIndex == -1) GetBindSelectList();

                    int tempComponentBindInfoIndex = EditorGUILayout.Popup(componentBindInfo.index, componentBindInfo.GetTypeStrings());
                    if (tempComponentBindInfoIndex != componentBindInfo.index) {
                        objectInfo.gameObjectBindInfoList[itemIndex].index = tempComponentBindInfoIndex;
                        isSavaSetting = true;
                    }
                    if (componentBindInfo.GetTypeName() == nameof(GameObject)) { EditorGUILayout.ObjectField(componentBindInfo.instanceObject, componentBindInfo.GetValue().GetType(), true); }
                    else { EditorGUILayout.ObjectField(componentBindInfo.GetValue(), componentBindInfo.GetValue().GetType(), true); }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();

                    GUILayout.Label("变量名");
                    string tempComponentBindInfoName = CommonTools.GetNumberAlpha(GUILayout.TextField(componentBindInfo.name));
                    if (tempComponentBindInfoName != componentBindInfo.name) {
                        objectInfo.gameObjectBindInfoList[itemIndex].name = tempComponentBindInfoName;
                        isSavaSetting = true;
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();

                    if (GUILayout.Button("操作", GUILayout.Width(100))) {
                        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
                        menu.AddItem(new GUIContent("设置默认名"), false, () => {
                            objectInfo.gameObjectBindInfoList[itemIndex].name = CommonTools.SetName(componentBindInfo.instanceObject.name, commonSettingData.selectCreateNameSetting);
                            isSavaSetting = true;
                        });

                        menu.ShowAsContext(); //显示菜单
                    }

                    GUI.color = Color.red;
                    if (GUILayout.Button("删除", GUILayout.Width(100))) {
                        objectInfo.gameObjectBindInfoList.RemoveAt(itemIndex);
                        isSavaSetting = true;
                        break;
                    }
                    GUI.color = Color.white;

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();

                    GUILayout.EndHorizontal();
                }

                for (int i = 0; i < selectDataAmount; i++) {
                    GUILayout.BeginVertical("frameBox");

                    DataBindInfo dataBindInfo = selectDataList[i];
                    int itemIndex = objectInfo.dataBindInfoList.IndexOf(dataBindInfo);

                    if (itemIndex == -1) GetBindSelectList();

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();

                    GUILayout.Label(dataBindInfo.typeString.typeName);

                    EditorGUILayout.ObjectField(dataBindInfo.bindObject, dataBindInfo.typeString.ToType(), true);

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();

                    GUILayout.Label("变量名");
                    string tempComponentBindInfoName = CommonTools.GetNumberAlpha(GUILayout.TextField(dataBindInfo.name));
                    if (tempComponentBindInfoName != dataBindInfo.name) {
                        objectInfo.dataBindInfoList[itemIndex].name = tempComponentBindInfoName;
                        isSavaSetting = true;
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();

                    if (GUILayout.Button("操作", GUILayout.Width(100))) {
                        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 
                        menu.AddItem(new GUIContent("设置默认名"), false, () => {
                            objectInfo.dataBindInfoList[itemIndex].name = CommonTools.SetName(dataBindInfo.bindObject.name, commonSettingData.selectCreateNameSetting);
                            isSavaSetting = true;
                        });

                        menu.ShowAsContext(); //显示菜单
                    }

                    GUI.color = Color.red;
                    if (GUILayout.Button("删除", GUILayout.Width(100))) {
                        objectInfo.dataBindInfoList.RemoveAt(itemIndex);
                        isSavaSetting = true;
                        break;
                    }
                    GUI.color = Color.white;

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();

                GUILayout.EndHorizontal();
            }
        }

        void GetBindSelectList()
        {
            selectComponentList = new List<ComponentBindInfo>();
            int bindComponentAmount = objectInfo.gameObjectBindInfoList.Count;
            selectDataList = new List<DataBindInfo>();
            int bindDataAmount = objectInfo.dataBindInfoList.Count;
            switch (searchType) {
                case SearchType.All:
                    for (int i = 0; i < bindComponentAmount; i++) {
                        ComponentBindInfo info = objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.GetTypeName(), bindInputString)
                            || CommonTools.Search(info.GetObject().name, bindInputString)
                            || CommonTools.Search(info.name, bindInputString)) { selectComponentList.Add(info); }
                    }
                    for (int i = 0; i < bindDataAmount; i++) {
                        DataBindInfo info = objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.typeString.typeName, bindInputString)
                            || CommonTools.Search(info.bindObject.name, bindInputString)
                            || CommonTools.Search(info.name, bindInputString)) { selectDataList.Add(info); }
                    }
                    break;
                case SearchType.TypeName:
                    for (int i = 0; i < bindComponentAmount; i++) {
                        ComponentBindInfo info = objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.GetTypeName(), bindInputString)) selectComponentList.Add(info);
                    }
                    for (int i = 0; i < bindDataAmount; i++) {
                        DataBindInfo info = objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.typeString.typeName, bindInputString)) selectDataList.Add(info);
                    }
                    break;
                case SearchType.GameObjectName:
                    for (int i = 0; i < bindComponentAmount; i++) {
                        ComponentBindInfo info = objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.GetObject().name, bindInputString)) selectComponentList.Add(info);
                    }
                    for (int i = 0; i < bindDataAmount; i++) {
                        DataBindInfo info = objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.bindObject.name, bindInputString)) selectDataList.Add(info);
                    }
                    break;
                case SearchType.VariableName:
                    for (int i = 0; i < bindComponentAmount; i++) {
                        ComponentBindInfo info = objectInfo.gameObjectBindInfoList[i];
                        if (CommonTools.Search(info.name, bindInputString)) selectComponentList.Add(info);
                    }
                    for (int i = 0; i < bindDataAmount; i++) {
                        DataBindInfo info = objectInfo.dataBindInfoList[i];
                        if (CommonTools.Search(info.name, bindInputString)) selectDataList.Add(info);
                    }
                    break;
            }

            selectComponentAmount = selectComponentList.Count;
            selectDataAmount = objectInfo.dataBindInfoList.Count;
        }

        void BindComponent(GameObject bindObject, int index)
        {
            ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindObject);
            objectInfo.Bind(componentBindInfo, index);
            if (_bindWindow.commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) componentBindInfo.name = CommonTools.GetNumberAlpha(componentBindInfo.instanceObject.name);
            _bindWindow.isSavaSetting = true;
        }

        void SelectBindInfo(int index)
        {
            selectComponentList = new List<ComponentBindInfo>();
            selectComponentList.Add(objectInfo.gameObjectBindInfoList[index]);
            selectComponentAmount = 1;
        }

        void RemoveBindInfo(GameObject removeObject, RemoveType removeType)
        {
            switch (removeType) {
                case RemoveType.This:
                    int thisBindInfoAmount = objectInfo.gameObjectBindInfoList.Count;
                    for (int i = thisBindInfoAmount - 1; i >= 0; i--) {
                        ComponentBindInfo componentBindInfo = objectInfo.gameObjectBindInfoList[i];
                        if (componentBindInfo.instanceObject == removeObject) objectInfo.gameObjectBindInfoList.RemoveAt(i);
                    }
                    break;
                case RemoveType.Child:
                {
                    Transform[] transforms = removeObject.GetComponentsInChildren<Transform>(true);
                    int amount = transforms.Length;
                    for (int i = 0; i < amount; i++) {
                        Transform transform = transforms[i];
                        if (transform.gameObject != removeObject) {
                            int childBindInfoAmount = objectInfo.gameObjectBindInfoList.Count;
                            for (int j = childBindInfoAmount - 1; j >= 0; j--) {
                                ComponentBindInfo componentBindInfo = objectInfo.gameObjectBindInfoList[j];
                                if (componentBindInfo.instanceObject == transform.gameObject) objectInfo.gameObjectBindInfoList.RemoveAt(j);
                            }
                        }
                    }
                    break;
                }
                case RemoveType.ThisAndChild:
                {
                    Transform[] transforms = removeObject.GetComponentsInChildren<Transform>(true);
                    int amount = transforms.Length;
                    for (int i = 0; i < amount; i++) {
                        Transform transform = transforms[i];
                        int childBindInfoAmount = objectInfo.gameObjectBindInfoList.Count;
                        for (int j = childBindInfoAmount - 1; j >= 0; j--) {
                            ComponentBindInfo componentBindInfo = objectInfo.gameObjectBindInfoList[j];
                            if (componentBindInfo.instanceObject == transform.gameObject) objectInfo.gameObjectBindInfoList.RemoveAt(j);
                        }
                    }
                    break;
                }
            }

            isSavaSetting = true;
        }
    }
}