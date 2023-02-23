using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BindTool
{
    public partial class BindWindow
    {
        private string nameCreateInput = "";

        private int nameReqlaceAmount;
        private List<NameReplaceData> editorNameReplaceDataList;
        private List<NameReplaceData> selectNameReplaceDataList;
        private int selectNameReqlaceAmount;

        private int nameSettingIndex;

        private const int NameReqlaceShowAmount = 5;
        private int nameReqlaceListMaxIndex;
        private int nameReqlaceListIndex = 1;

        public void DrawNameSettingGUI()
        {
            GUILayout.Label("NameSetting", this.settingStyle);

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(this.commonSettingData.selectCreateNameSetting.programName))
                    {
                        GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                        int amount = this.commonSettingData.createNameSettingList.Count;
                        for (int i = 0; i < amount; i++)
                        {
                            int scriptIndex = i;
                            menu.AddItem(new GUIContent(this.commonSettingData.createNameSettingList[i].programName), false,
                                () => {
                                    this.commonSettingData.selectCreateNameSetting = this.commonSettingData.createNameSettingList[scriptIndex];
                                    this.isSavaSetting = true;
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
                            this.commonSettingData.selectScriptSetting = scriptSetting;
                            this.commonSettingData.scriptSettingList.Add(scriptSetting);

                            CreateNameSetting createNameSetting = CreateInstance<CreateNameSetting>();
                            this.commonSettingData.selectCreateNameSetting = createNameSetting;
                            this.commonSettingData.createNameSettingList.Add(createNameSetting);
                            createNameSetting.programName = ConstData.DefaultCreateNameSettingName;

                            int number = 1;
                            string assteName = "CreateNameSetting";
                            string fullPath = path + $"/{assteName}.asset";
                            while (File.Exists(fullPath))
                            {
                                assteName = "CreateNameSetting" + number;
                                fullPath = path + $"/{assteName}.asset";
                                number++;
                            }
                            scriptSetting.programName = assteName;

                            AssetDatabase.CreateAsset(scriptSetting, fullPath);

                            this.isSavaSetting = true;
                        }
                    }
                    GUI.color = Color.red;
                    if (GUILayout.Button("删除", GUILayout.Width(100)))
                    {
                        if (this.commonSettingData.createNameSettingList.Count > 1)
                        {
                            this.commonSettingData.createNameSettingList.Remove(this.commonSettingData.selectCreateNameSetting);
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this.commonSettingData.selectCreateNameSetting));
                            this.commonSettingData.selectCreateNameSetting = this.commonSettingData.createNameSettingList.First();
                            this.isSavaSetting = true;
                        }
                    }

                    GUI.color = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            CreateNameList();
        }

        void CreateNameList()
        {
            GUILayout.BeginVertical("box");
            {
                CreateNameSetting selectSetting = this.commonSettingData.selectCreateNameSetting;

                int tempIndex = GUILayout.Toolbar(this.nameSettingIndex, new string[] {"Variable", "Property"});
                if (tempIndex != this.nameSettingIndex)
                {
                    this.nameSettingIndex = tempIndex;
                    GUI.FocusControl(null);
                }

                if (this.nameSettingIndex == 0) { this.editorNameReplaceDataList = selectSetting.variableNameReplaceDataList; }
                else { this.editorNameReplaceDataList = selectSetting.propertyNameReplaceDataList; }

                if (this.nameReqlaceAmount != this.editorNameReplaceDataList.Count)
                {
                    this.nameReqlaceAmount = this.editorNameReplaceDataList.Count;
                    GetSelectCreateNameList();
                }

                if (this.nameSettingIndex == 0)
                {
                    bool tempIsGenerateName = GUILayout.Toggle(selectSetting.isBindAutoGenerateName, "绑定时是否自动生成名称");
                    if (tempIsGenerateName != selectSetting.isBindAutoGenerateName)
                    {
                        selectSetting.isBindAutoGenerateName = tempIsGenerateName;
                        this.isSavaSetting = true;
                    }
                }

                EditorGUI.BeginDisabledGroup(selectSetting.isBindAutoGenerateName == false);
                {
                    string tempString = GUILayout.TextField(this.nameCreateInput, "SearchTextField");
                    if (tempString.Equals(this.nameCreateInput) == false)
                    {
                        this.nameCreateInput = tempString;
                        GetSelectCreateNameList();
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("名称替换列表");
                        if (GUILayout.Button("添加"))
                        {
                            this.editorNameReplaceDataList.Add(new NameReplaceData());
                            this.isSavaSetting = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (this.selectNameReqlaceAmount > 0)
                    {
                        GUILayout.BeginVertical("box", GUILayout.Height(275f));
                        {
                            for (int i = 0; i < NameReqlaceShowAmount; i++)
                            {
                                int showIndex = (this.nameReqlaceListIndex - 1) * NameReqlaceShowAmount + i;
                                if (showIndex >= this.selectNameReqlaceAmount) { break; }
                                GUILayout.BeginVertical("frameBox");
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        NameReplaceData nameReplaceData = this.selectNameReplaceDataList[showIndex];

                                        EditorGUILayout.BeginVertical();
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            {
                                                GUILayout.Label("检查名称");
                                                string tempCheckName = GUILayout.TextField(nameReplaceData.nameCheck.name, GUILayout.MinWidth(50));
                                                if (tempCheckName != nameReplaceData.nameCheck.name)
                                                {
                                                    nameReplaceData.nameCheck.name = tempCheckName;
                                                    this.isSavaSetting = true;
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.BeginHorizontal();
                                            {
                                                GUILayout.Label("替换名称");
                                                string tempTargetName = GUILayout.TextField(nameReplaceData.targetName, GUILayout.MinWidth(50));
                                                if (tempTargetName != nameReplaceData.targetName)
                                                {
                                                    nameReplaceData.targetName = tempTargetName;
                                                    this.isSavaSetting = true;
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }
                                        EditorGUILayout.EndVertical();

                                        EditorGUILayout.BeginVertical();
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            {
                                                GUILayout.Label("区分大小写");
                                                bool tempIsCaseSensitive = GUILayout.Toggle(nameReplaceData.nameCheck.nameRule.isCaseSensitive, "");
                                                if (tempIsCaseSensitive != nameReplaceData.nameCheck.nameRule.isCaseSensitive)
                                                {
                                                    nameReplaceData.nameCheck.nameRule.isCaseSensitive = tempIsCaseSensitive;
                                                    this.isSavaSetting = true;
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.BeginHorizontal();
                                            {
                                                GUILayout.Label("匹配规则");
                                                NameMatchingRule tempnNameMatchingRule =
                                                    (NameMatchingRule) EditorGUILayout.EnumPopup(nameReplaceData.nameCheck.nameRule.nameMatchingRule, GUILayout.Width(100));
                                                if (tempnNameMatchingRule != nameReplaceData.nameCheck.nameRule.nameMatchingRule)
                                                {
                                                    nameReplaceData.nameCheck.nameRule.nameMatchingRule = tempnNameMatchingRule;
                                                    this.isSavaSetting = true;
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }
                                        EditorGUILayout.EndVertical();

                                        EditorGUILayout.BeginVertical();
                                        {
                                            if (GUILayout.Button("操作", GUILayout.Width(50))) { }

                                            GUI.color = Color.red;
                                            if (GUILayout.Button("删除", GUILayout.Width(50)))
                                            {
                                                this.editorNameReplaceDataList.Remove(nameReplaceData);
                                                this.isSavaSetting = true;
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
                        GUILayout.EndVertical();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"{this.nameReqlaceListIndex}/{this.nameReqlaceListMaxIndex}", this.tabIndexStyle);
                            GUILayout.Space(10);
                            if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationLeft", GUILayout.Width(30), GUILayout.Height(20)))
                            {
                                this.nameReqlaceListIndex--;
                                this.nameReqlaceListIndex = Mathf.Clamp(this.nameReqlaceListIndex, 1, this.nameReqlaceListMaxIndex);
                            }
                            if (GUILayout.Button(string.Empty, (GUIStyle) "ArrowNavigationRight", GUILayout.Width(30), GUILayout.Height(20)))
                            {
                                this.nameReqlaceListIndex++;
                                this.nameReqlaceListIndex = Mathf.Clamp(this.nameReqlaceListIndex, 1, this.nameReqlaceListMaxIndex);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();
        }

        void GetSelectCreateNameList()
        {
            this.selectNameReplaceDataList = new List<NameReplaceData>();

            int nameReplaceAmount = this.editorNameReplaceDataList.Count;
            for (int i = 0; i < nameReplaceAmount; i++)
            {
                NameReplaceData nameReplaceData = this.editorNameReplaceDataList[i];
                if (CommonTools.Search(nameReplaceData.targetName, this.nameCreateInput) || CommonTools.Search(nameReplaceData.nameCheck.name, this.nameCreateInput))
                {
                    this.selectNameReplaceDataList.Add(nameReplaceData);
                }
            }

            this.selectNameReqlaceAmount = this.selectNameReplaceDataList.Count;

            this.nameReqlaceListMaxIndex = (int) Math.Ceiling(this.selectNameReqlaceAmount / (double) NameReqlaceShowAmount);
            this.nameReqlaceListIndex = Mathf.Clamp(this.nameReqlaceListIndex, 1, this.nameReqlaceListMaxIndex);
        }
    }
}