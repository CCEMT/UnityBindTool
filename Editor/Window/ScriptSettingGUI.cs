#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BindTool
{
    public partial class BindWindow
    {
        private int scriptSettingIndex;
        private int nameBindIndex;

        public string scriptInput = "";

        private int variableListAmount;
        private int templateListAmount;

        private TypeString variableSelectType;
        private TypeString templateSelectType;

        private List<VariableData> selectVariableDataList;
        private List<TemplateData> selectTemplateDataList;

        private int selectVariableAmount;
        private int selectTemplateAmount;

        private const int VariableShowAmount = 3;
        private const int TemplateShowAmount = 3;

        private int variableListMaxIndex;
        private int variableListIndex=1;

        private int templateListMaxIndex;
        private int templateListIndex=1;

        public void DrawScriptSettingGUI()
        {
            GUILayout.Label("ScriptSetting", settingStyle);

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(commonSettingData.selectScriptSetting.programName))
                    {
                        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                        int amount = commonSettingData.scriptSettingList.Count;
                        for (int i = 0; i < amount; i++)
                        {
                            int scriptIndex = i;
                            menu.AddItem(new GUIContent(commonSettingData.scriptSettingList[i].programName), false,
                                () => {
                                    commonSettingData.selectScriptSetting = commonSettingData.scriptSettingList[scriptIndex];
                                    isSavaSetting = true;
                                });
                        }
                        menu.ShowAsContext();
                    }

                    GUI.color = Color.green;
                    if (GUILayout.Button("新建", GUILayout.Width(100)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择NameSetting保存路径", Application.dataPath, null);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                            string path = "Assets/" + targetPath;
                            ScriptSetting scriptSetting = CreateInstance<ScriptSetting>();
                            commonSettingData.selectScriptSetting = scriptSetting;
                            commonSettingData.scriptSettingList.Add(scriptSetting);
                            scriptSetting.programName = ConstData.DefaultScriptSettingName;

                            int number = 1;
                            string assteName = "ScriptSetting";
                            string fullPath = path + $"/{assteName}.asset";
                            while (File.Exists(fullPath))
                            {
                                assteName = "ScriptSetting" + number;
                                fullPath = path + $"/{assteName}.asset";
                                number++;
                            }
                            scriptSetting.programName = assteName;

                            AssetDatabase.CreateAsset(scriptSetting, fullPath);

                            isSavaSetting = true;
                        }
                    }
                    GUI.color = Color.red;
                    if (GUILayout.Button("删除", GUILayout.Width(100)))
                    {
                        if (commonSettingData.scriptSettingList.Count > 1)
                        {
                            commonSettingData.scriptSettingList.Remove(commonSettingData.selectScriptSetting);
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(commonSettingData.selectScriptSetting));
                            commonSettingData.selectScriptSetting = commonSettingData.scriptSettingList.First();
                            isSavaSetting = true;
                        }
                    }

                    GUI.color = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();

            scriptSettingIndex = GUILayout.Toolbar(scriptSettingIndex, new string[] {"CommonGenerate", "MethodGenerate"});
            switch (scriptSettingIndex)
            {
                case 0:
                    ScriptSettingGUI();
                    break;
                case 1:
                    TemplateGenerate();
                    break;
            }
        }

        void ScriptSettingGUI()
        {
            ScriptSetting selectSetting = commonSettingData.selectScriptSetting;

            GUILayout.BeginVertical("box");
            {
                GUILayout.Label("Create", commonTitleStyle);
                GUILayout.BeginVertical("frameBox");
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        bool tempIsGenerateNew = GUILayout.Toggle(selectSetting.isGenerateNew, "是否生成新脚本");
                        if (tempIsGenerateNew != selectSetting.isGenerateNew)
                        {
                            selectSetting.isGenerateNew = tempIsGenerateNew;
                            isSavaSetting = true;
                        }

                        bool tempIsGeneratePartial = GUILayout.Toggle(selectSetting.isGeneratePartial, "是否分文件生成");
                        if (tempIsGeneratePartial != selectSetting.isGeneratePartial)
                        {
                            selectSetting.isGeneratePartial = tempIsGeneratePartial;
                            isSavaSetting = true;
                        }
                        if (selectSetting.isGeneratePartial)
                        {
                            GUILayout.Label("子文件拓展名：");
                            string content = GUILayout.TextField(selectSetting.partialName);
                            content = CommonTools.GetNumberAlpha(content);
                            if (content != selectSetting.partialName)
                            {
                                selectSetting.partialName = content;
                                isSavaSetting = true;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("继承的类");
                        GUI.color = Color.green;
                        if (GUILayout.Button(selectSetting.inheritClass.typeName))
                        {
                            TypeSelectWindow.ShowWindown(typeof(MonoBehaviour), (isSelect, type) => {
                                if (isSelect)
                                {
                                    selectSetting.inheritClass = type;
                                    isSavaSetting = true;
                                }
                            }, position.position);
                        }
                        GUI.color = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("程序集名称");

                        selectSetting.createScriptAssembly = GUILayout.TextField(selectSetting.createScriptAssembly);
                        if (GUILayout.Button("搜索", GUILayout.Width(50)))
                        {
                            SearchAssemblyWindow.ShowWindown((isSelect, assembly) => {
                                if (isSelect)
                                {
                                    selectSetting.createScriptAssembly = assembly;
                                    isSavaSetting = true;
                                }
                            }, position.position);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        bool tempIsSpecifyNamespace = GUILayout.Toggle(selectSetting.isSpecifyNamespace, "是否使用命名空间");
                        if (tempIsSpecifyNamespace != selectSetting.isSpecifyNamespace)
                        {
                            selectSetting.isSpecifyNamespace = tempIsSpecifyNamespace;
                            isSavaSetting = true;
                        }
                        if (selectSetting.isSpecifyNamespace)
                        {
                            string tempUseNamespace = GUILayout.TextField(selectSetting.useNamespace);
                            if (tempUseNamespace != selectSetting.useNamespace)
                            {
                                selectSetting.useNamespace = tempUseNamespace;
                                isSavaSetting = true;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    bool tempIsSavaOldScript = GUILayout.Toggle(selectSetting.isSavaOldScript, "是否保存旧脚本");
                    if (tempIsSavaOldScript != selectSetting.isSavaOldScript)
                    {
                        selectSetting.isSavaOldScript = tempIsSavaOldScript;
                        isSavaSetting = true;
                    }
                    if (selectSetting.isSavaOldScript)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("路径：", GUILayout.MinWidth(50));
                            string tempOldSavaPath = GUILayout.TextArea(selectSetting.savaOldScriptPath, GUILayout.MaxWidth(500));
                            if (tempOldSavaPath != selectSetting.savaOldScriptPath)
                            {
                                selectSetting.savaOldScriptPath = tempOldSavaPath;
                                isSavaSetting = true;
                            }

                            if (GUILayout.Button("浏览", GUILayout.MinWidth(50)))
                            {
                                string targetPath = EditorUtility.OpenFolderPanel("选择代码保存路径", Application.dataPath, null);
                                if (string.IsNullOrEmpty(targetPath) == false)
                                {
                                    targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                                    selectSetting.savaOldScriptPath = targetPath;
                                    isSavaSetting = true;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        string errorInfo = "错误：旧脚本保存路径为空或不存在";
                        string path = Application.dataPath + "/" + selectSetting.savaOldScriptPath;
                        if (string.IsNullOrEmpty(selectSetting.savaOldScriptPath) || Directory.Exists(path) == false)
                        {
                            if (errorList.Contains(errorInfo) == false) errorList.Add(errorInfo);
                            GUI.color = Color.red;
                            GUILayout.BeginVertical("box");
                            GUILayout.Label(errorInfo);
                            GUILayout.EndHorizontal();
                            GUI.color = Color.white;
                        }
                        else
                        {
                            if (errorList.Contains(errorInfo)) errorList.Remove(errorInfo);
                        }
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("NameBind", commonTitleStyle);
                GUILayout.BeginVertical("frameBox");
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        bool tempIsPubildGet = GUILayout.Toggle(selectSetting.isAddProperty, "是否创建属性");
                        if (tempIsPubildGet != selectSetting.isAddProperty)
                        {
                            selectSetting.isAddProperty = tempIsPubildGet;
                            isSavaSetting = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (selectSetting.isAddProperty)
                    {
                        nameBindIndex = GUILayout.Toolbar(nameBindIndex, new string[] {"Variable", "Property"});

                        switch (nameBindIndex)
                        {
                            case 0:
                                VariableName();
                                break;
                            case 1:
                                PropertyName();
                                break;
                        }
                    }
                    else { VariableName(); }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        void VariableName()
        {
            ScriptSetting selectSetting = commonSettingData.selectScriptSetting;

            EditorGUILayout.BeginHorizontal();
            {
                bool tempIsAddClassName = GUILayout.Toggle(selectSetting.nameSetting.isAddClassName, "变量名中是否添加类名");
                if (tempIsAddClassName != selectSetting.nameSetting.isAddClassName)
                {
                    selectSetting.nameSetting.isAddClassName = tempIsAddClassName;
                    isSavaSetting = true;
                }
                if (selectSetting.nameSetting.isAddClassName)
                {
                    bool tempIsFrontOrBehind = GUILayout.Toggle(selectSetting.nameSetting.isFrontOrBehind, "类名为前缀还是后缀");
                    if (tempIsFrontOrBehind != selectSetting.nameSetting.isFrontOrBehind)
                    {
                        selectSetting.nameSetting.isFrontOrBehind = tempIsFrontOrBehind;
                        isSavaSetting = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                bool tempIsAddFront = GUILayout.Toggle(selectSetting.nameSetting.isAddFront, "是否添加前缀");
                if (tempIsAddFront != selectSetting.nameSetting.isAddFront)
                {
                    selectSetting.nameSetting.isAddFront = tempIsAddFront;
                    isSavaSetting = true;
                }
                if (selectSetting.nameSetting.isAddFront)
                {
                    string tempFrontName = GUILayout.TextField(selectSetting.nameSetting.frontName);
                    if (tempFrontName != selectSetting.nameSetting.frontName)
                    {
                        selectSetting.nameSetting.frontName = tempFrontName;
                        isSavaSetting = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                bool tempIsAddBehind = GUILayout.Toggle(selectSetting.nameSetting.isAddBehind, "是否添加后缀");
                if (tempIsAddBehind != selectSetting.nameSetting.isAddBehind)
                {
                    selectSetting.nameSetting.isAddBehind = tempIsAddBehind;
                    isSavaSetting = true;
                }
                if (selectSetting.nameSetting.isAddBehind)
                {
                    string tempBehindName = GUILayout.TextField(selectSetting.nameSetting.behindName);
                    if (tempBehindName != selectSetting.nameSetting.behindName)
                    {
                        selectSetting.nameSetting.behindName = tempBehindName;
                        isSavaSetting = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("访问类型：", GUILayout.Width(120));
                VisitType tempVisitType = (VisitType) EditorGUILayout.EnumPopup(selectSetting.variableVisitType, GUILayout.Width(100));
                if (tempVisitType != selectSetting.variableVisitType)
                {
                    selectSetting.variableVisitType = tempVisitType;
                    isSavaSetting = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("脚本命名处理");
                ScriptNamingDispose tempNamingDispose = (ScriptNamingDispose) EditorGUILayout.EnumPopup(selectSetting.nameSetting.namingDispose);
                if (tempNamingDispose != selectSetting.nameSetting.namingDispose)
                {
                    selectSetting.nameSetting.namingDispose = tempNamingDispose;
                    isSavaSetting = true;
                }
                GUILayout.Label("名称重复处理规则");
                RepetitionNameDispose tempRepetitionNameDispose = (RepetitionNameDispose) EditorGUILayout.EnumPopup(selectSetting.nameSetting.repetitionNameDispose);
                if (tempRepetitionNameDispose != selectSetting.nameSetting.repetitionNameDispose)
                {
                    selectSetting.nameSetting.repetitionNameDispose = tempRepetitionNameDispose;
                    isSavaSetting = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void PropertyName()
        {
            ScriptSetting selectSetting = commonSettingData.selectScriptSetting;

            EditorGUILayout.BeginHorizontal();
            {
                bool tempIsAddClassName = GUILayout.Toggle(selectSetting.propertyNameSetting.isAddClassName, "变量名中是否添加类名");
                if (tempIsAddClassName != selectSetting.propertyNameSetting.isAddClassName)
                {
                    selectSetting.propertyNameSetting.isAddClassName = tempIsAddClassName;
                    isSavaSetting = true;
                }
                if (selectSetting.propertyNameSetting.isAddClassName)
                {
                    bool tempIsFrontOrBehind = GUILayout.Toggle(selectSetting.propertyNameSetting.isFrontOrBehind, "类名为前缀还是后缀");
                    if (tempIsFrontOrBehind != selectSetting.propertyNameSetting.isFrontOrBehind)
                    {
                        selectSetting.propertyNameSetting.isFrontOrBehind = tempIsFrontOrBehind;
                        isSavaSetting = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                bool tempIsAddFront = GUILayout.Toggle(selectSetting.propertyNameSetting.isAddFront, "是否添加前缀");
                if (tempIsAddFront != selectSetting.propertyNameSetting.isAddFront)
                {
                    selectSetting.propertyNameSetting.isAddFront = tempIsAddFront;
                    isSavaSetting = true;
                }
                if (selectSetting.propertyNameSetting.isAddFront)
                {
                    string tempFrontName = GUILayout.TextField(selectSetting.propertyNameSetting.frontName);
                    if (tempFrontName != selectSetting.propertyNameSetting.frontName)
                    {
                        selectSetting.propertyNameSetting.frontName = tempFrontName;
                        isSavaSetting = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {

                bool tempIsAddBehind = GUILayout.Toggle(selectSetting.propertyNameSetting.isAddBehind, "是否添加后缀");
                if (tempIsAddBehind != selectSetting.propertyNameSetting.isAddBehind)
                {
                    selectSetting.propertyNameSetting.isAddBehind = tempIsAddBehind;
                    isSavaSetting = true;
                }
                if (selectSetting.propertyNameSetting.isAddBehind)
                {
                    string tempBehindName = GUILayout.TextField(selectSetting.propertyNameSetting.behindName);
                    if (tempBehindName != selectSetting.propertyNameSetting.behindName)
                    {
                        selectSetting.propertyNameSetting.behindName = tempBehindName;
                        isSavaSetting = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("访问类型：", GUILayout.Width(120));
                VisitType tempVisitType = (VisitType) EditorGUILayout.EnumPopup(selectSetting.propertyVisitType, GUILayout.Width(100));
                if (tempVisitType != selectSetting.propertyVisitType)
                {
                    selectSetting.propertyVisitType = tempVisitType;
                    isSavaSetting = true;
                }
                GUILayout.Label("属性类型：", GUILayout.Width(140));
                PropertyType tempPointerType = (PropertyType) EditorGUILayout.EnumPopup(selectSetting.propertyType, GUILayout.Width(100));
                if (tempPointerType != selectSetting.propertyType)
                {
                    selectSetting.propertyType = tempPointerType;
                    isSavaSetting = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("脚本命名处理");
                ScriptNamingDispose tempNamingDispose = (ScriptNamingDispose) EditorGUILayout.EnumPopup(selectSetting.propertyNameSetting.namingDispose);
                if (tempNamingDispose != selectSetting.propertyNameSetting.namingDispose)
                {
                    selectSetting.propertyNameSetting.namingDispose = tempNamingDispose;
                    isSavaSetting = true;
                }
                GUILayout.Label("名称重复处理规则");
                RepetitionNameDispose tempRepetitionNameDispose = (RepetitionNameDispose) EditorGUILayout.EnumPopup(selectSetting.propertyNameSetting.repetitionNameDispose);
                if (tempRepetitionNameDispose != selectSetting.propertyNameSetting.repetitionNameDispose)
                {
                    selectSetting.propertyNameSetting.repetitionNameDispose = tempRepetitionNameDispose;
                    isSavaSetting = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void TemplateGenerate()
        {
            MethodName();
            ScriptSetting selectSetting = commonSettingData.selectScriptSetting;

            if (this.variableListAmount != selectSetting.variableList.Count || this.templateListAmount != selectSetting.templateDataList.Count)
            {
                variableListAmount = selectSetting.variableList.Count;
                templateListAmount = selectSetting.templateDataList.Count;
                GetSelectMethodData();
            }
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.Label("MethodSetting", commonTitleStyle);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("frameBox");
            {
                string tempString = GUILayout.TextField(scriptInput, "SearchTextField");
                if (tempString.Equals(scriptInput) == false)
                {
                    scriptInput = tempString;
                    GetSelectMethodData();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(225));
                    {
                        GUILayout.Label("字段访问方法列表：", commonTitleStyle);

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        {
                            GUI.color = Color.green;
                            if (GUILayout.Button(variableSelectType.typeName))
                            {
                                TypeSelectWindow.ShowWindown(typeof(Component), (isSelect, type) => {
                                    if (isSelect) variableSelectType = type;
                                }, position.position);
                            }
                            GUI.color = Color.white;
                            if (GUILayout.Button("添加", GUILayout.Width(50)))
                            {
                                if (variableSelectType.IsEmpty() == false)
                                {
                                    VariableData variableData = new VariableData();
                                    variableData.targetType = variableSelectType;
                                    selectSetting.variableList.Add(variableData);
                                    isSavaSetting = true;
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (this.selectVariableAmount > 0)
                        {
                            GUILayout.BeginVertical("box", GUILayout.Width(225), GUILayout.Height(160));
                            {
                                for (int i = 0; i < VariableShowAmount; i++)
                                {
                                    int showIndex = (this.variableListIndex - 1) * VariableShowAmount + i;
                                    if (showIndex >= selectVariableAmount) { break; }

                                    GUILayout.BeginVertical("frameBox");
                                    {
                                        VariableData variable = selectVariableDataList[showIndex];

                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            EditorGUILayout.BeginVertical();
                                            {
                                                GUI.color = Color.green;
                                                GUILayout.Label(variable.targetType.typeName);
                                                GUI.color = Color.white;
                                                if (GUILayout.Button(variable.varialbleName, GUILayout.MinWidth(120)))
                                                {
                                                    GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                                                    PropertyInfo[] propertyInfos = variable.targetType.ToType().GetProperties();
                                                    int amount = propertyInfos.Length;
                                                    for (int j = 0; j < amount; j++)
                                                    {
                                                        PropertyInfo propertyInfo = propertyInfos[j];
                                                        menu.AddItem(new GUIContent(propertyInfo.Name), false, () => {
                                                            variable.variableType = new TypeString(propertyInfo.PropertyType);
                                                            variable.varialbleName = propertyInfo.Name;
                                                            isSavaSetting = true;
                                                        }); //向菜单中添加菜单项
                                                    }

                                                    menu.ShowAsContext(); //显示菜单
                                                }
                                            }
                                            EditorGUILayout.EndVertical();

                                            EditorGUILayout.BeginVertical();
                                            {
                                                VisitType tempVisitType = (VisitType) EditorGUILayout.EnumPopup(variable.visitType, GUILayout.Width(75));
                                                if (tempVisitType != variable.visitType)
                                                {
                                                    variable.visitType = tempVisitType;
                                                    isSavaSetting = true;
                                                }

                                                GUI.color = Color.red;
                                                if (GUILayout.Button("删除", GUILayout.Width(75)))
                                                {
                                                    selectSetting.variableList.Remove(variable);
                                                    isSavaSetting = true;
                                                }
                                                GUI.color = Color.white;
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    GUILayout.EndVertical();
                                }
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label($"{this.variableListIndex}/{this.variableListMaxIndex}", tabIndexStyle);
                                GUILayout.Space(10);
                                if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationLeft", GUILayout.Width(30), GUILayout.Height(20)))
                                {
                                    variableListIndex--;
                                    this.variableListIndex = Mathf.Clamp(variableListIndex, 1, this.variableListMaxIndex);
                                }
                                if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationRight", GUILayout.Width(30), GUILayout.Height(20)))
                                {
                                    variableListIndex++;
                                    this.variableListIndex = Mathf.Clamp(variableListIndex, 1, this.variableListMaxIndex);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f));
                    {
                        GUILayout.Label("模板方法列表：", commonTitleStyle);

                        GUILayout.Space(5);

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUI.color = Color.green;
                            if (GUILayout.Button(templateSelectType.typeName))
                            {
                                TypeSelectWindow.ShowWindown(typeof(Component), (isSelect, type) => {
                                    if (isSelect) templateSelectType = type;
                                }, position.position);
                            }
                            GUI.color = Color.white;
                            if (GUILayout.Button("添加", GUILayout.Width(50)))
                            {
                                if (templateSelectType.IsEmpty() == false)
                                {
                                    TemplateData findData = selectSetting.templateDataList.Find((type) => {
                                        if (type.targetType.Equals(templateSelectType)) return true;
                                        return false;
                                    });

                                    if (findData == null)
                                    {
                                        TemplateData templateData = new TemplateData();
                                        templateData.targetType = templateSelectType;

                                        TypeString templateType = ScriptGenerate.GenerateCSharpTemplateScript(templateData.targetType, selectSetting.templateScriptSavaPath);
                                        templateData.temlateType = templateType;
                                        templateData.temlateBaseType = new TypeString(typeof(MonoBehaviour));
                                        selectSetting.templateDataList.Add(templateData);
                                        isSavaSetting = true;
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    }
                                    else { Debug.Log("不能重复添加"); }
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (this.selectTemplateAmount > 0)
                        {
                            GUILayout.BeginVertical("box", GUILayout.Height(160));
                            {
                                for (int i = 0; i < TemplateShowAmount; i++)
                                {
                                    int showIndex = (this.templateListIndex - 1) * TemplateShowAmount + i;
                                    if (showIndex >= selectTemplateAmount) { break; }

                                    GUILayout.BeginVertical("frameBox");
                                    {
                                        TemplateData templateData = selectTemplateDataList[showIndex];

                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            EditorGUILayout.BeginVertical();
                                            {
                                                GUI.color = Color.green;
                                                GUILayout.Label(templateData.targetType.typeName);
                                                GUI.color = Color.white;
                                                if (GUILayout.Button("打开脚本"))
                                                {
                                                    string fullPath = $"Assets/{selectSetting.templateScriptSavaPath}/{templateData.temlateType.typeName}.cs";
                                                    Object script = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
                                                    if (script != null) AssetDatabase.OpenAsset(script);
                                                }
                                            }
                                            EditorGUILayout.EndVertical();

                                            EditorGUILayout.BeginVertical();
                                            {
                                                GUI.color = Color.green;
                                                if (GUILayout.Button(templateData.temlateBaseType.typeName))
                                                {
                                                    TypeSelectWindow.ShowWindown(typeof(MonoBehaviour), (isSelect, type) => {
                                                        if (isSelect)
                                                        {
                                                            templateData.temlateBaseType = type;
                                                            ScriptGenerate.AlterCSharpTemplateBase(templateData, selectSetting.templateScriptSavaPath);
                                                            isSavaSetting = true;
                                                            AssetDatabase.SaveAssets();
                                                            AssetDatabase.Refresh();
                                                        }
                                                    }, position.position);
                                                }
                                                GUI.color = Color.white;

                                                GUI.color = Color.red;
                                                if (GUILayout.Button("删除"))
                                                {
                                                    string fullPath = $"{Application.dataPath}/{selectSetting.templateScriptSavaPath}/{templateData.temlateType.typeName}.cs";
                                                    if (File.Exists(fullPath)) File.Delete(fullPath);
                                                    selectSetting.templateDataList.Remove(templateData);
                                                    isSavaSetting = true;
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.Refresh();
                                                }

                                                GUI.color = Color.white;
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label($"{this.templateListIndex}/{this.templateListMaxIndex}", tabIndexStyle);
                                GUILayout.Space(10);
                                if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationLeft", GUILayout.Width(30), GUILayout.Height(20)))
                                {
                                    templateListIndex--;
                                    this.templateListIndex = Mathf.Clamp(templateListIndex, 1, this.templateListMaxIndex);
                                }
                                if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationRight", GUILayout.Width(30), GUILayout.Height(20)))
                                {
                                    templateListIndex++;
                                    this.templateListIndex = Mathf.Clamp(templateListIndex, 1, this.templateListMaxIndex);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }

        void MethodName()
        {
            ScriptSetting selectSetting = commonSettingData.selectScriptSetting;

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.Label("MethodName", commonTitleStyle);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("frameBox");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool tempIsAddClassName = GUILayout.Toggle(selectSetting.methodNameSetting.isAddClassName, "变量名中是否添加类名");
                    if (tempIsAddClassName != selectSetting.methodNameSetting.isAddClassName)
                    {
                        selectSetting.methodNameSetting.isAddClassName = tempIsAddClassName;
                        isSavaSetting = true;
                    }
                    if (selectSetting.methodNameSetting.isAddClassName)
                    {
                        bool tempIsFrontOrBehind = GUILayout.Toggle(selectSetting.methodNameSetting.isFrontOrBehind, "类名为前缀还是后缀");
                        if (tempIsFrontOrBehind != selectSetting.methodNameSetting.isFrontOrBehind)
                        {
                            selectSetting.methodNameSetting.isFrontOrBehind = tempIsFrontOrBehind;
                            isSavaSetting = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    bool tempIsAddFront = GUILayout.Toggle(selectSetting.methodNameSetting.isAddFront, "是否添加前缀");
                    if (tempIsAddFront != selectSetting.methodNameSetting.isAddFront)
                    {
                        selectSetting.methodNameSetting.isAddFront = tempIsAddFront;
                        isSavaSetting = true;
                    }
                    if (selectSetting.methodNameSetting.isAddFront)
                    {
                        string tempFrontName = GUILayout.TextField(selectSetting.methodNameSetting.frontName);
                        if (tempFrontName != selectSetting.methodNameSetting.frontName)
                        {
                            selectSetting.methodNameSetting.frontName = tempFrontName;
                            isSavaSetting = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    bool tempIsAddBehind = GUILayout.Toggle(selectSetting.methodNameSetting.isAddBehind, "是否添加后缀");
                    if (tempIsAddBehind != selectSetting.methodNameSetting.isAddBehind)
                    {
                        selectSetting.methodNameSetting.isAddBehind = tempIsAddBehind;
                        isSavaSetting = true;
                    }
                    if (selectSetting.methodNameSetting.isAddBehind)
                    {
                        string tempBehindName = GUILayout.TextField(selectSetting.methodNameSetting.behindName);
                        if (tempBehindName != selectSetting.methodNameSetting.behindName)
                        {
                            selectSetting.methodNameSetting.behindName = tempBehindName;
                            isSavaSetting = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("脚本命名处理");
                    ScriptNamingDispose tempNamingDispose = (ScriptNamingDispose) EditorGUILayout.EnumPopup(selectSetting.methodNameSetting.namingDispose);
                    if (tempNamingDispose != selectSetting.methodNameSetting.namingDispose)
                    {
                        selectSetting.methodNameSetting.namingDispose = tempNamingDispose;
                        isSavaSetting = true;
                    }
                    GUILayout.Label("名称重复处理规则");
                    RepetitionNameDispose tempRepetitionNameDispose = (RepetitionNameDispose) EditorGUILayout.EnumPopup(selectSetting.methodNameSetting.repetitionNameDispose);
                    if (tempRepetitionNameDispose != selectSetting.methodNameSetting.repetitionNameDispose)
                    {
                        selectSetting.methodNameSetting.repetitionNameDispose = tempRepetitionNameDispose;
                        isSavaSetting = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("模板脚本保存路径：", GUILayout.MinWidth(100));
                    string tempTemplateScriptSavaPath = GUILayout.TextArea(selectSetting.templateScriptSavaPath, GUILayout.MaxWidth(300));
                    if (tempTemplateScriptSavaPath != selectSetting.templateScriptSavaPath)
                    {
                        selectSetting.templateScriptSavaPath = tempTemplateScriptSavaPath;
                        isSavaSetting = true;
                    }

                    if (GUILayout.Button("浏览", GUILayout.MinWidth(50)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择模板脚本保存路径", Application.dataPath, null);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                            selectSetting.templateScriptSavaPath = targetPath;
                            isSavaSetting = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                string path = Application.dataPath + "/" + selectSetting.templateScriptSavaPath;
                if (string.IsNullOrEmpty(selectSetting.templateScriptSavaPath) || Directory.Exists(path) == false)
                {
                    GUI.color = Color.yellow;
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("警告：保存路径为空或不存在");
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }
            }
            GUILayout.EndVertical();
        }

        void GetSelectMethodData()
        {
            selectVariableDataList = new List<VariableData>();
            int variableAmount = commonSettingData.selectScriptSetting.variableList.Count;
            for (int i = 0; i < variableAmount; i++)
            {
                VariableData variableData = commonSettingData.selectScriptSetting.variableList[i];
                if (CommonTools.Search(variableData.targetType.ToType().Name, scriptInput)) selectVariableDataList.Add(variableData);
            }

            selectTemplateDataList = new List<TemplateData>();
            int templateAmount = commonSettingData.selectScriptSetting.templateDataList.Count;
            for (int i = 0; i < templateAmount; i++)
            {
                TemplateData templateData = commonSettingData.selectScriptSetting.templateDataList[i];
                if (CommonTools.Search(templateData.targetType.ToType().Name, scriptInput)) selectTemplateDataList.Add(templateData);
            }

            selectVariableAmount = selectVariableDataList.Count;
            selectTemplateAmount = selectTemplateDataList.Count;

            this.variableListMaxIndex = (int) Math.Ceiling(selectVariableAmount / (double) VariableShowAmount);
            this.variableListIndex = Mathf.Clamp(variableListIndex, 1, this.variableListMaxIndex);

            this.templateListMaxIndex = (int) Math.Ceiling(this.selectTemplateAmount / (double) TemplateShowAmount);
            this.templateListIndex = Mathf.Clamp(templateListIndex, 1, this.variableListMaxIndex);
        }
    }
}