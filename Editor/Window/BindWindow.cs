#region Using

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#endregion

namespace BindTool
{
    public partial class BindWindow : EditorWindow
    {
        private static BindWindow _bindWindow;

        private CommonSettingData commonSettingData;

        private GameObject bindObject;
        private ObjectInfo objectInfo;

        private GUIStyle scriptGuiStyle;
        private GUIStyle prefabGuiStyle;
        private GUIStyle luaGuiStyle;
        private GUIStyle buttonGUIStyle;
        private GUIStyle settingStyle;
        private GUIStyle commonTitleStyle;
        private GUIStyle tabIndexStyle;

        private int index;
        private bool isSavaSetting;
        private List<string> errorList;

        [MenuItem("GameObject/BindWindown", false, 0)]
        static void CreateUIPanel()
        {
            if (Check())
            {
                _bindWindow = GetWindowWithRect<BindWindow>(new Rect(Screen.width / 2f, Screen.height / 2f, 500, 600), true, "BindWindown");
                _bindWindow.CenterOnMainWin();
                _bindWindow.Init();
                _bindWindow.Show();
            }
        }

        void Init()
        {
            errorList = new List<string>();
            _bindWindow.bindObject = Selection.objects.First() as GameObject;

            commonSettingData = CommonTools.GetCommonSettingData();

            if (commonSettingData.scriptSettingList.Contains(commonSettingData.selectScriptSetting) == false) commonSettingData.scriptSettingList.Add(commonSettingData.selectScriptSetting);
            if (commonSettingData.autoBindSettingList.Contains(commonSettingData.selectAutoBindSetting) == false) commonSettingData.autoBindSettingList.Add(commonSettingData.selectAutoBindSetting);
            if (commonSettingData.createNameSettingList.Contains(commonSettingData.selectCreateNameSetting) == false)
            {
                commonSettingData.createNameSettingList.Add(commonSettingData.selectCreateNameSetting);
            }

            objectInfo = CommonTools.GetObjectInfo(bindObject);
            objectInfo.rootBindInfo.instanceObject = bindObject;
            objectInfo.AgainGet();

            if (string.IsNullOrEmpty(objectInfo.typeString.typeName)) { commonSettingData.newScriptName = bindObject.name; }
            else { commonSettingData.newScriptName = objectInfo.typeString.typeName; }

            scriptGuiStyle = new GUIStyle();
            scriptGuiStyle.normal.textColor = Color.green;
            scriptGuiStyle.fontStyle = FontStyle.Bold;
            scriptGuiStyle.fontSize = 18;

            prefabGuiStyle = new GUIStyle();
            prefabGuiStyle.normal.textColor = new Color(0.4980392f, 0.8392158f, 0.9921569f, 1);
            prefabGuiStyle.fontStyle = FontStyle.Bold;
            prefabGuiStyle.fontSize = 18;

            luaGuiStyle = new GUIStyle();
            luaGuiStyle.normal.textColor = Color.blue;
            luaGuiStyle.fontStyle = FontStyle.Bold;
            luaGuiStyle.fontSize = 18;

            buttonGUIStyle = new GUIStyle();
            buttonGUIStyle.fontStyle = FontStyle.Bold;
            buttonGUIStyle.fontSize = 18;

            settingStyle = new GUIStyle();
            settingStyle.normal.textColor = Color.green;
            settingStyle.fontStyle = FontStyle.Bold;
            settingStyle.fontSize = 18;

            commonTitleStyle = new GUIStyle();
            commonTitleStyle.normal.textColor = Color.white;
            commonTitleStyle.fontStyle = FontStyle.Bold;
            commonTitleStyle.fontSize = 14;

            tabIndexStyle = new GUIStyle();
            tabIndexStyle.normal.textColor = Color.white;
            tabIndexStyle.fontSize = 14;
            tabIndexStyle.alignment = TextAnchor.MiddleRight;

            isSavaSetting = true;
        }

        static bool Check()
        {
            Object[] selectObjects = Selection.objects;
            if (selectObjects.Length > 0)
            {
                Object selectObject = selectObjects.First();
                if (selectObject != null)
                {
                    GameObject gameObject = selectObjects.First() as GameObject;
                    if (gameObject != null) return true;
                }
            }
            Debug.LogError("请选择一个正确的对象");

            return false;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            _bindWindow = this;
            ShowControl();
            if (isSavaSetting) SavaSetting();
        }

        void ShowControl()
        {
            index = GUILayout.Toolbar(index, new string[] {"Build", "Bind", "Setting"});
            switch (index)
            {
                case 0:
                    DrawBuildGUI();
                    break;
                case 1:
                    DrawBindGUI();
                    break;
                case 2:
                    DrawSettingGUI();
                    break;
            }
        }

        void SavaSetting()
        {
            isSavaSetting = false;
            EditorUtility.SetDirty(commonSettingData);
            int scriptSettingAmount = commonSettingData.scriptSettingList.Count;
            for (int i = 0; i < scriptSettingAmount; i++)
            {
                ScriptSetting scriptSerting = commonSettingData.scriptSettingList[i];
                EditorUtility.SetDirty(scriptSerting);
            }
            int autoBindSettingAmount = commonSettingData.autoBindSettingList.Count;
            for (int i = 0; i < autoBindSettingAmount; i++)
            {
                AutoBindSetting autoBindSetting = commonSettingData.autoBindSettingList[i];
                EditorUtility.SetDirty(autoBindSetting);
            }
            int createNameSetting = commonSettingData.createNameSettingList.Count;
            for (int i = 0; i < createNameSetting; i++)
            {
                CreateNameSetting nameSetting = commonSettingData.createNameSettingList[i];
                EditorUtility.SetDirty(nameSetting);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnDestroy()
        {
            _bindWindow = null;
            commonSettingData = null;
        }
    }
}