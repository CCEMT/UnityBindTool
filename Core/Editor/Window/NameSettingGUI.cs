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
        private Vector2 nameCreateScrollPosition;

        private int nameReqlaceAmount;
        private List<NameReplaceData> selectNameReplaceDataList;
        private int selectNameReqlaceAmount;

        public void DrawNameSettingGUI()
        {
            GUILayout.Label("NameSetting", settingStyle);

            GUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(commonSettingData.selectCreateNameSetting.programName))
            {
                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                int amount = commonSettingData.createNameSettingList.Count;
                for (int i = 0; i < amount; i++)
                {
                    var scriptIndex = i;
                    menu.AddItem(new GUIContent(commonSettingData.createNameSettingList[i].programName), false,
                        () => {
                            commonSettingData.selectCreateNameSetting = commonSettingData.createNameSettingList[scriptIndex];
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

                    CreateNameSetting createNameSetting = CreateInstance<CreateNameSetting>();
                    commonSettingData.selectCreateNameSetting = createNameSetting;
                    commonSettingData.createNameSettingList.Add(createNameSetting);
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

                    isSavaSetting = true;
                }
            }
            GUI.color = Color.red;
            if (GUILayout.Button("删除", GUILayout.Width(100)))
            {
                if (commonSettingData.createNameSettingList.Count > 1)
                {
                    commonSettingData.createNameSettingList.Remove(commonSettingData.selectCreateNameSetting);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(commonSettingData.selectCreateNameSetting));
                    commonSettingData.selectCreateNameSetting = commonSettingData.createNameSettingList.First();
                    isSavaSetting = true;
                }
            }

            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            CreateNameList();
        }

        void CreateNameList()
        {
            GUILayout.BeginVertical("box");

            var selectSetting = commonSettingData.selectCreateNameSetting;
            if (nameReqlaceAmount != selectSetting.nameReplaceDataList.Count)
            {
                nameReqlaceAmount = selectSetting.nameReplaceDataList.Count;
                GetSelectCreateNameList();
            }

            bool tempIsGenerateName = GUILayout.Toggle(selectSetting.isBindAutoGenerateName, "绑定时是否自动生成名称");
            if (tempIsGenerateName != selectSetting.isBindAutoGenerateName)
            {
                selectSetting.isBindAutoGenerateName = tempIsGenerateName;
                isSavaSetting = true;
            }

            EditorGUI.BeginDisabledGroup(selectSetting.isBindAutoGenerateName == false);

            string tempString = GUILayout.TextField(nameCreateInput, "SearchTextField");
            if (tempString.Equals(nameCreateInput) == false)
            {
                nameCreateInput = tempString;
                GetSelectCreateNameList();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("名称替换列表");
            if (GUILayout.Button("添加"))
            {
                selectSetting.nameReplaceDataList.Add(new NameReplaceData());
                isSavaSetting = true;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            nameCreateScrollPosition = EditorGUILayout.BeginScrollView(nameCreateScrollPosition, false, false, GUILayout.ExpandWidth(true), GUILayout.Height(300));

            for (int i = 0; i < selectNameReqlaceAmount; i++)
            {
                GUILayout.BeginVertical("frameBox");
                EditorGUILayout.BeginHorizontal();

                NameReplaceData nameReplaceData = selectNameReplaceDataList[i];

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("检查名称");
                string tempCheckName = GUILayout.TextField(nameReplaceData.nameCheck.name, GUILayout.MinWidth(50));
                if (tempCheckName != nameReplaceData.nameCheck.name)
                {
                    nameReplaceData.nameCheck.name = tempCheckName;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("替换名称");
                string tempTargetName = GUILayout.TextField(nameReplaceData.targetName, GUILayout.MinWidth(50));
                if (tempTargetName != nameReplaceData.targetName)
                {
                    nameReplaceData.targetName = tempTargetName;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("区分大小写");
                bool tempIsCaseSensitive = GUILayout.Toggle(nameReplaceData.nameCheck.nameRule.isCaseSensitive, "");
                if (tempIsCaseSensitive != nameReplaceData.nameCheck.nameRule.isCaseSensitive)
                {
                    nameReplaceData.nameCheck.nameRule.isCaseSensitive = tempIsCaseSensitive;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("匹配规则");
                NameMatchingRule tempnNameMatchingRule = (NameMatchingRule) EditorGUILayout.EnumPopup(nameReplaceData.nameCheck.nameRule.nameMatchingRule, GUILayout.Width(100));
                if (tempnNameMatchingRule != nameReplaceData.nameCheck.nameRule.nameMatchingRule)
                {
                    nameReplaceData.nameCheck.nameRule.nameMatchingRule = tempnNameMatchingRule;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("操作", GUILayout.Width(50))) { }

                GUI.color = Color.red;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    selectSetting.nameReplaceDataList.Remove(nameReplaceData);
                    isSavaSetting = true;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
        }

        void GetSelectCreateNameList()
        {
            selectNameReplaceDataList = new List<NameReplaceData>();

            int nameReplaceAmount = commonSettingData.selectCreateNameSetting.nameReplaceDataList.Count;
            for (int i = 0; i < nameReplaceAmount; i++)
            {
                NameReplaceData nameReplaceData = commonSettingData.selectCreateNameSetting.nameReplaceDataList[i];
                if (CommonTools.Search(nameReplaceData.targetName, nameCreateInput) || CommonTools.Search(nameReplaceData.nameCheck.name, nameCreateInput))
                {
                    selectNameReplaceDataList.Add(nameReplaceData);
                }
            }

            selectNameReqlaceAmount = selectNameReplaceDataList.Count;
        }
    }
}