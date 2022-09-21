#region Using

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#endregion

namespace BindTool
{
    public partial class BindWindown
    {
        private int autoBindSettingIndex;

        private string autoSettingInput = "";

        private Vector2 autoScrollPosition1;
        private Vector2 autoScrollPosition2;
        private Vector2 autoScrollPosition3;

        private List<NameBindData> selectNameBindDataList;
        private List<NameCheck> selectLgnoreDataList;
        private List<SequenceTypeData> selectSequenceTypeDataList;

        private int selectNameBindAmount;
        private int selectLgnoreAmount;
        private int selectSequenceAmount;

        private int nameBindCount;
        private int lgnoreCount;
        private int sequenceCount;

        public void DrawAutoSettingGUI()
        {
            GUILayout.Label("AutoBindSetting", settingStyle);

            GUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(commonSettingData.selectAutoBindSetting.programName))
            {
                GenericMenu menu = new GenericMenu(); //初始化GenericMenu 

                int amount = commonSettingData.autoBindSettingList.Count;
                for (int i = 0; i < amount; i++)
                {
                    var bindIndex = i;
                    menu.AddItem(new GUIContent(commonSettingData.autoBindSettingList[i].programName), false,
                        () => { commonSettingData.selectAutoBindSetting = commonSettingData.autoBindSettingList[bindIndex]; });
                }
                menu.ShowAsContext();
            }

            GUI.color = Color.green;
            if (GUILayout.Button("新建", GUILayout.Width(100)))
            {
                string targetPath = EditorUtility.OpenFolderPanel("选择AutoSetting保存路径", Application.dataPath, null);
                if (string.IsNullOrEmpty(targetPath) == false)
                {
                    targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);

                    string path = "Assets/" + targetPath;
                    int number = 1;
                    string assteName = "AutoBindSetting";
                    string fullPath = path + $"/{assteName}.asset";

                    while (File.Exists(fullPath))
                    {
                        assteName = "AutoBindSetting" + number;
                        fullPath = path + $"/{assteName}.asset";
                        number++;
                    }

                    AutoBindSetting autoBindSetting = CreateInstance<AutoBindSetting>();
                    commonSettingData.selectAutoBindSetting = autoBindSetting;
                    commonSettingData.autoBindSettingList.Add(autoBindSetting);
                    autoBindSetting.programName = ConstData.DefaultAutoBindSettingName;

                    autoBindSetting.programName = assteName;
                    AssetDatabase.CreateAsset(autoBindSetting, fullPath);
                    isSavaSetting = true;
                }
            }

            GUI.color = Color.red;
            if (GUILayout.Button("删除", GUILayout.Width(100)))
            {
                if (commonSettingData.autoBindSettingList.Count > 1)
                {
                    commonSettingData.autoBindSettingList.Remove(commonSettingData.selectAutoBindSetting);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(commonSettingData.selectAutoBindSetting));
                    commonSettingData.selectAutoBindSetting = commonSettingData.autoBindSettingList.First();
                    isSavaSetting = true;
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();

            AutoBindSettingGUI();

            GUILayout.EndHorizontal();
        }

        void AutoBindSettingGUI()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Search Item：");
            string tempString = GUILayout.TextField(autoSettingInput, "SearchTextField");
            if (tempString.Equals(autoSettingInput) == false)
            {
                autoSettingInput = tempString;
                GetSelectAutoBindData();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("frameBox");
            autoBindSettingIndex = GUILayout.Toolbar(autoBindSettingIndex, new string[] {"NameBindList", "NameLgnoreList", "SequenceTypeList"});
            switch (autoBindSettingIndex)
            {
                case 0:
                    DrawNameBindList();
                    break;
                case 1:
                    DrawNameLgnoreList();
                    break;
                case 2:
                    DrawSequenceTypeList();
                    break;
            }
            GUILayout.EndHorizontal();
        }

        void DrawNameBindList()
        {
            var selectSetting = commonSettingData.selectAutoBindSetting;

            if (nameBindCount != selectSetting.nameBindDataList.Count)
            {
                GetSelectAutoBindData();
                nameBindCount = selectSetting.nameBindDataList.Count;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("名称绑定列表");
            if (GUILayout.Button("添加"))
            {
                selectSetting.nameBindDataList.Add(new NameBindData());
                isSavaSetting = true;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            autoScrollPosition1 = EditorGUILayout.BeginScrollView(autoScrollPosition1, false, false, GUILayout.ExpandWidth(true), GUILayout.Height(275));
            for (int i = selectNameBindAmount - 1; i >= 0; i--)
            {
                GUILayout.BeginVertical("frameBox");
                EditorGUILayout.BeginHorizontal();

                var data = selectNameBindDataList[i];

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("检查名称");
                string tempCheckName = GUILayout.TextField(data.nameCheck.name, GUILayout.MinWidth(50));
                if (tempCheckName != data.nameCheck.name)
                {
                    data.nameCheck.name = tempCheckName;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("对应类");
                GUI.color = Color.green;
                if (GUILayout.Button(data.typeString.typeName))
                {
                    var bindIndex = i;
                    TypeSelectWindown.ShowWindown(typeof(Component), (isSelect, type) => {
                        if (isSelect)
                        {
                            data.typeString = type;
                            selectSetting.nameBindDataList[bindIndex] = data;
                            isSavaSetting = true;
                        }
                    }, position.position);
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("区分大小写");
                bool tempIsCaseSensitive = GUILayout.Toggle(data.nameCheck.nameRule.isCaseSensitive, "");
                if (tempIsCaseSensitive != data.nameCheck.nameRule.isCaseSensitive)
                {
                    data.nameCheck.nameRule.isCaseSensitive = tempIsCaseSensitive;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("匹配规则");
                NameMatchingRule tempnNameMatchingRule = (NameMatchingRule) EditorGUILayout.EnumPopup(data.nameCheck.nameRule.nameMatchingRule, GUILayout.Width(100));
                if (tempnNameMatchingRule != data.nameCheck.nameRule.nameMatchingRule)
                {
                    data.nameCheck.nameRule.nameMatchingRule = tempnNameMatchingRule;
                    isSavaSetting = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("操作", GUILayout.Width(50))) { }

                GUI.color = Color.red;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    selectSetting.nameBindDataList.Remove(data);
                    isSavaSetting = true;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        void DrawNameLgnoreList()
        {
            var selectSetting = commonSettingData.selectAutoBindSetting;

            if (lgnoreCount != selectSetting.nameLgnoreDataList.Count)
            {
                GetSelectAutoBindData();
                lgnoreCount = selectSetting.nameLgnoreDataList.Count;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("名称忽略列表");
            if (GUILayout.Button("添加"))
            {
                selectSetting.nameLgnoreDataList.Add(new NameCheck());
                isSavaSetting = true;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            autoScrollPosition2 = EditorGUILayout.BeginScrollView(autoScrollPosition2, false, false, GUILayout.ExpandWidth(true), GUILayout.Height(300));
            for (int i = selectLgnoreAmount - 1; i >= 0; i--)
            {
                GUILayout.BeginVertical("frameBox");
                EditorGUILayout.BeginHorizontal();

                var data = selectLgnoreDataList[i];
                GUILayout.Label("检查名称");
                string tempCheckName = GUILayout.TextField(data.name, GUILayout.MinWidth(50));
                if (tempCheckName != data.name)
                {
                    data.name = tempCheckName;
                    isSavaSetting = true;
                }

                GUILayout.Label("匹配规则");
                NameMatchingRule tempNameMatchingRule = (NameMatchingRule) EditorGUILayout.EnumPopup(data.nameRule.nameMatchingRule, GUILayout.Width(75));
                if (tempNameMatchingRule != data.nameRule.nameMatchingRule)
                {
                    data.nameRule.nameMatchingRule = tempNameMatchingRule;
                    isSavaSetting = true;
                }

                bool tempIsCaseSensitive = GUILayout.Toggle(data.nameRule.isCaseSensitive, "区分大小写");
                if (tempIsCaseSensitive != data.nameRule.isCaseSensitive)
                {
                    data.nameRule.isCaseSensitive = tempIsCaseSensitive;
                    isSavaSetting = true;
                }

                GUI.color = Color.red;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    selectSetting.nameLgnoreDataList.Remove(data);
                    isSavaSetting = true;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        void DrawSequenceTypeList()
        {
            var selectSetting = commonSettingData.selectAutoBindSetting;

            if (sequenceCount != selectSetting.sequenceTypeDataList.Count)
            {
                GetSelectAutoBindData();
                sequenceCount = selectSetting.sequenceTypeDataList.Count;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("类型顺序列表");
            if (GUILayout.Button("添加"))
            {
                selectSetting.sequenceTypeDataList.Add(new SequenceTypeData());
                isSavaSetting = true;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            autoScrollPosition3 = EditorGUILayout.BeginScrollView(autoScrollPosition3, false, false, GUILayout.ExpandWidth(true), GUILayout.Height(300));
            for (int i = selectSequenceAmount - 1; i >= 0; i--)
            {
                GUILayout.BeginVertical("frameBox");
                EditorGUILayout.BeginHorizontal();

                var data = selectSequenceTypeDataList[i];

                GUILayout.Label("顺序");
                string numberString = GUILayout.TextField(data.sequence.ToString(), 3, GUILayout.Width(30));
                if (string.IsNullOrEmpty(numberString)) numberString = 0.ToString();
                int tempNumber = int.Parse(CommonTools.GetNumber(numberString));
                if (tempNumber != data.sequence)
                {
                    data.sequence = tempNumber;
                    isSavaSetting = true;
                }
                GUILayout.Label("是否其他", GUILayout.Width(60));
                bool tempIsElse = GUILayout.Toggle(data.isElse, "");
                if (tempIsElse != data.isElse)
                {
                    data.isElse = tempIsElse;
                    isSavaSetting = true;
                }

                GUILayout.Label("对应类名");
                GUI.color = Color.green;
                if (GUILayout.Button(data.typeString.typeName))
                {
                    var index = i;
                    TypeSelectWindown.ShowWindown(typeof(Component), (isSelect, type) => {
                        if (isSelect)
                        {
                            data.typeString = type;
                            selectSetting.sequenceTypeDataList[index] = data;
                            isSavaSetting = true;
                        }
                    }, position.position);
                }

                GUI.color = Color.red;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    selectSetting.sequenceTypeDataList.Remove(data);
                    isSavaSetting = true;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        void GetSelectAutoBindData()
        {
            selectNameBindDataList = new List<NameBindData>();
            int nameBindAmount = commonSettingData.selectAutoBindSetting.nameBindDataList.Count;
            for (int i = 0; i < nameBindAmount; i++)
            {
                var nameBind = commonSettingData.selectAutoBindSetting.nameBindDataList[i];
                if (CommonTools.Search(nameBind.typeString.typeName, autoSettingInput) || CommonTools.Search(nameBind.nameCheck.name, autoSettingInput)) selectNameBindDataList.Add(nameBind);
            }
            selectNameBindAmount = selectNameBindDataList.Count;

            selectLgnoreDataList = new List<NameCheck>();
            int lgnoreAmount = commonSettingData.selectAutoBindSetting.nameLgnoreDataList.Count;
            for (int i = 0; i < lgnoreAmount; i++)
            {
                var lgnore = commonSettingData.selectAutoBindSetting.nameLgnoreDataList[i];
                if (CommonTools.Search(lgnore.name, autoSettingInput)) selectLgnoreDataList.Add(lgnore);
            }
            selectLgnoreAmount = selectLgnoreDataList.Count;

            selectSequenceTypeDataList = new List<SequenceTypeData>();
            int sequenceAmount = commonSettingData.selectAutoBindSetting.sequenceTypeDataList.Count;
            for (int i = 0; i < sequenceAmount; i++)
            {
                var sequenceData = commonSettingData.selectAutoBindSetting.sequenceTypeDataList[i];
                if (CommonTools.Search(sequenceData.typeString.typeName, autoSettingInput) || CommonTools.SearchNumber(sequenceData.sequence.ToString(), autoSettingInput))
                {
                    selectSequenceTypeDataList.Add(sequenceData);
                }
            }
            selectSequenceAmount = selectSequenceTypeDataList.Count;
        }
    }
}