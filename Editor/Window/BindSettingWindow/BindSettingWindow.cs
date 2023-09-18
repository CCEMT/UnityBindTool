using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace UnityBindTool
{
    public class BindSettingWindow : OdinMenuEditorWindow
    {
        public static void OpenWindow()
        {
            BindSettingWindow window = GetWindow<BindSettingWindow>("Bind Setting");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 600);
            window.Init();
        }

        [SerializeField]
        private BindSetting bindSetting;

        void Init()
        {
            this.bindSetting = BindSetting.Get();
            MenuWidth = 240f;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree odinMenuTree = new OdinMenuTree();

            odinMenuTree.Add(nameof(BaseSetting), this.bindSetting.baseSetting);

            int amount = this.bindSetting.compositionSettingList.Count;
            for (int i = 0; i < amount; i++)
            {
                CompositionSetting compositionSetting = this.bindSetting.compositionSettingList[i];
                string compositionName = compositionSetting.compositionName;
                odinMenuTree.Add(compositionName, compositionSetting);

                string commonSettingName = $"{compositionName}/通用设置";
                odinMenuTree.Add(commonSettingName, compositionSetting.commonSetting);

                string scriptSettingName = $"{compositionName}/脚本设置";
                odinMenuTree.Add(scriptSettingName, compositionSetting.scriptSetting);

                string csharpScriptSettingName = $"{scriptSettingName}/C#脚本设置";
                odinMenuTree.Add(csharpScriptSettingName, compositionSetting.scriptSetting.csharpScriptSetting);

                string luaScriptSettingName = $"{scriptSettingName}/Lua脚本设置";
                odinMenuTree.Add(luaScriptSettingName, compositionSetting.scriptSetting.luaScriptSetting);

                string autoBindSettingName = $"{compositionName}/自动绑定设置";
                odinMenuTree.Add(autoBindSettingName, compositionSetting.autoBindSetting);

                string nameBindSettingName = $"{autoBindSettingName}/名称绑定设置";
                odinMenuTree.Add(nameBindSettingName, compositionSetting.autoBindSetting.nameBindSetting);

                string nameLgnoreSettingName = $"{autoBindSettingName}/名字忽略设置";
                odinMenuTree.Add(nameLgnoreSettingName, compositionSetting.autoBindSetting.nameLgnoreSetting);

                string streamingBindSettingName = $"{autoBindSettingName}/流式绑定设置";
                odinMenuTree.Add(streamingBindSettingName, compositionSetting.autoBindSetting.streamingBindSetting);

                string nameGenerateSettingName = $"{compositionName}/命名生成设置";
                odinMenuTree.Add(nameGenerateSettingName, compositionSetting.nameGenerateSetting);

                string csharpNameGenerateSetting = $"{nameGenerateSettingName}/C#命名生成设置";
                odinMenuTree.Add(csharpNameGenerateSetting, compositionSetting.nameGenerateSetting.csharpNameGenerateSetting);

                string luaNameGenerateSetting = $"{nameGenerateSettingName}/Lua命名生成设置";
                odinMenuTree.Add(luaNameGenerateSetting, compositionSetting.nameGenerateSetting.luaNameGenerateSetting);
            }

            return odinMenuTree;
        }

        protected override void OnBeginDrawEditors()
        {
            if (MenuTree == null) { return; }
            int toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;
            OdinMenuItem selected = this.MenuTree.Selection.FirstOrDefault();

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("刷新"))) { ForceMenuTreeRebuild(); }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("保存"))) { SaveSetting(); }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("创建")))
                {
                    CompositionSetting newCompositionSetting = new CompositionSetting();
                    newCompositionSetting.compositionName = "组合设置";
                    this.bindSetting.compositionSettingList.Add(newCompositionSetting);
                    SaveSetting();
                    ForceMenuTreeRebuild();
                }

                if (selected != null && selected.Value is CompositionSetting)
                {
                    CompositionSetting value = selected.Value as CompositionSetting;
                    if (SirenixEditorGUI.ToolbarButton(new GUIContent("选择")))
                    {
                        this.bindSetting.selectCompositionSetting = value;
                        SaveSetting();
                    }

                    if (SirenixEditorGUI.ToolbarButton(new GUIContent("删除")))
                    {
                        this.bindSetting.compositionSettingList.Remove(value);
                        SaveSetting();
                        ForceMenuTreeRebuild();
                    }
                }

                string content = bindSetting.selectCompositionSetting != null ? this.bindSetting.selectCompositionSetting.compositionName : "";
                GUILayout.Label(content);
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void OnDestroy()
        {
            SaveSetting();
            base.OnDestroy();
        }

        void SaveSetting()
        {
            EditorUtility.SetDirty(this.bindSetting);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}