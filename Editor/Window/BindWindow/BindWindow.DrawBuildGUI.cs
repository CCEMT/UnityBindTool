using BindTool;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class BindWindow
{
    private GUIStyle preivewStyle;
    private GUIStyle contentStyle;

    void GetDrawBuildGUIGUIStyle()
    {
        if (this.preivewStyle == null)
        {
            this.preivewStyle = new GUIStyle(GUI.skin.label);
            this.preivewStyle.fontSize = 18;
            this.preivewStyle.alignment = TextAnchor.MiddleCenter;
        }

        if (this.contentStyle == null)
        {
            contentStyle = new GUIStyle(GUI.skin.label);
            contentStyle.richText = true;
        }
    }

    void DrawBuildGUI()
    {
        GetDrawBuildGUIGUIStyle();

        if (GUILayout.Button("返回")) { this.bindWindowState = BindWindowState.BindInfoListGUI; }

        if (GUILayout.Button("开始生成")) BindBuild.Build(this.bindSetting.selectCompositionSetting, this.generateData);

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

            SirenixEditorGUI.HorizontalLineSeparator(Color.white, 1);

            CommonSetting commonSetting = this.bindSetting.selectCompositionSetting.commonSetting;

            GUILayout.Label($"{GetBoolInfo(commonSetting.isDetachMode)}分离模式", contentStyle);

            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreatePrefab)}生成预制体", contentStyle);
            if (commonSetting.isCreatePrefab)
            {
                GUILayout.Label($"{GetBoolInfo(commonSetting.isCreatePrefabFolder)}创建预制体文件夹", contentStyle);
                GUILayout.Label($"预制体生成路径：{commonSetting.createPrefabPath}");
            }
            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreateScript)}生成C#脚本", contentStyle);
            if (commonSetting.isCreateScript)
            {
                GUILayout.Label($"{GetBoolInfo(commonSetting.isCreateScriptFolder)}创建C#脚本文件夹", contentStyle);
                GUILayout.Label($"C#脚本生成路径：{commonSetting.createScriptPath}");
            }
            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreateLua)}生成Lua脚本", contentStyle);
            if (commonSetting.isCreateLua)
            {
                GUILayout.Label($"{GetBoolInfo(commonSetting.isCreateLuaFolder)}创建Lua文件夹", contentStyle);
                GUILayout.Label($"Lua脚本生成路径：{commonSetting.createLuaPath}", contentStyle);
            }
        }
        EditorGUILayout.EndVertical();
    }

    string GetBoolInfo(bool isEnable)
    {
        return isEnable ? "<color=green>启用</color>" : "<color=red>禁用</color>";
    }
}