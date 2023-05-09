using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class BindWindow
{
    private GUIStyle preivewStyle;

    void GetDrawBuildGUIGUIStyle()
    {
        if (this.preivewStyle != null) { return; }
        this.preivewStyle = new GUIStyle(GUI.skin.label);
        this.preivewStyle.fontSize = 18;
        this.preivewStyle.alignment = TextAnchor.MiddleCenter;
    }

    void DrawBuildGUI()
    {
        GetDrawBuildGUIGUIStyle();
        
        if (GUILayout.Button("返回")) { this.bindWindowState = BindWindowState.BindInfoListGUI; }

        if (GUILayout.Button("开始生成")) { }

        EditorGUILayout.BeginHorizontal("box");
        {
            string content = this.bindSetting.selectCompositionSetting != null ? this.bindSetting.selectCompositionSetting.compositionName : "选择设置为空，请先选择设置";
            GUILayout.Label($"生成配置：{content}");

            if (GUILayout.Button("编辑", GUILayout.Width(50f))) { BindSettingWindow.OpenWindow(); }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("生成配置预览", this.preivewStyle);
            
            SirenixEditorGUI.HorizontalLineSeparator(Color.white,1);

            CommonSetting commonSetting = this.bindSetting.selectCompositionSetting.commonSetting;

            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreatePrefab)}生成预制体");
            if (commonSetting.isCreatePrefab) { GUILayout.Label($"预制体生成路径：{commonSetting.createPrefabPath}"); }
            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreateScriptFolder)}生成C#脚本");
            if (commonSetting.isCreateScriptFolder) { GUILayout.Label($"C#脚本生成路径：{commonSetting.createScriptPath}"); }
            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreateLua)}生成Lua脚本");
            if (commonSetting.isCreateLua) { GUILayout.Label($"Lua脚本生成路径：{commonSetting.createLuaPath}"); }
        }
        EditorGUILayout.EndVertical();
    }

    string GetBoolInfo(bool isEnable)
    {
        return isEnable ? "启用" : "禁用";
    }
}