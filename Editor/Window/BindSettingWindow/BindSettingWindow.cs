using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class BindSettingWindow : OdinMenuEditorWindow
{
    [MenuItem("Tools/BindSettingWindow")]
    public static void OpenWindow()
    {
        BindSettingWindow window = GetWindow<BindSettingWindow>("Bind Setting");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 600);
        window.Init();
    }

    private BindSetting bindSetting;

    void Init()
    {
        this.bindSetting = BindSetting.Get();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree odinMenuTree = new OdinMenuTree();

        int amount = this.bindSetting.compositionSettingList.Count;
        for (int i = 0; i < amount; i++)
        {
            CompositionSetting compositionSetting = this.bindSetting.compositionSettingList[i];
            string compositionName = compositionSetting.compositionName;
            odinMenuTree.Add(compositionName, compositionSetting);

            string commonSettingName = $"{compositionName}/{nameof(CommonSetting)}";
            odinMenuTree.Add(commonSettingName, compositionSetting.commonSetting);

            string scriptSettingName = $"{compositionName}/{nameof(ScriptSetting)}";
            odinMenuTree.Add(scriptSettingName, compositionSetting.scriptSetting);

            string csharpScriptSettingName = $"{scriptSettingName}/{nameof(CSharpScriptSetting)}";
            odinMenuTree.Add(csharpScriptSettingName, compositionSetting.scriptSetting.csharpScriptSetting);

            string luaScriptSettingName = $"{scriptSettingName}/{nameof(LuaScriptSetting)}";
            odinMenuTree.Add(luaScriptSettingName, compositionSetting.scriptSetting.luaScriptSetting);

            string autoBindSettingName = $"{compositionName}/{nameof(AutoBindSetting)}";
            odinMenuTree.Add(autoBindSettingName, compositionSetting.autoBindSetting);

            string nameBindSettingName = $"{autoBindSettingName}/{nameof(NameBindSetting)}";
            odinMenuTree.Add(nameBindSettingName, compositionSetting.autoBindSetting.nameBindSetting);

            string nameLgnoreSettingName = $"{autoBindSettingName}/{nameof(NameLgnoreSetting)}";
            odinMenuTree.Add(nameLgnoreSettingName, compositionSetting.autoBindSetting.nameLgnoreSetting);

            string streamingBindSettingName = $"{autoBindSettingName}/{nameof(StreamingBindSetting)}";
            odinMenuTree.Add(streamingBindSettingName, compositionSetting.autoBindSetting.streamingBindSetting);

            string nameGenerateSettingName = $"{compositionName}/{nameof(NameGenerateSetting)}";
            odinMenuTree.Add(nameGenerateSettingName, compositionSetting.nameGenerateSetting);

            string csharpNameGenerateSetting = $"{nameGenerateSettingName}/{nameof(CSharpNameGenerateSetting)}";
            odinMenuTree.Add(csharpNameGenerateSetting, compositionSetting.nameGenerateSetting.csharpNameGenerateSetting);

            string luaNameGenerateSetting = $"{nameGenerateSettingName}/{nameof(LuaNameGenerateSetting)}";
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
                newCompositionSetting.compositionName = "New Composition";
                this.bindSetting.compositionSettingList.Add(newCompositionSetting);
                SaveSetting();
                ForceMenuTreeRebuild();
            }

            if (selected != null)
            {
                if (selected.Value is CompositionSetting && SirenixEditorGUI.ToolbarButton(new GUIContent("删除")))
                {
                    this.bindSetting.compositionSettingList.Remove((CompositionSetting) selected.Value);
                    SaveSetting();
                    ForceMenuTreeRebuild();
                }
            }

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