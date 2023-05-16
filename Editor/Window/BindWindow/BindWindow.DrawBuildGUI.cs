using System;
using BindTool;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class BindWindow
{
    private GUIStyle preivewStyle;
    private GUIStyle contentStyle;

    private bool isError;
    private TypeString chackTypeString;
    private bool isNull;
    private bool isTypeError;

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

        if (GUILayout.Button("开始生成"))
        {
            if (this.isError == false) { BindBuild.Build(this.bindSetting.selectCompositionSetting, this.generateData); }
            else { Debug.LogError("生成失败，请先解决错误"); }
        }

        EditorGUILayout.BeginHorizontal("box");
        {
            string content = this.bindSetting.selectCompositionSetting != null ? this.bindSetting.selectCompositionSetting.compositionName : "选择设置为空，请先选择设置";
            GUILayout.Label($"生成配置：{content}");

            if (GUILayout.Button("编辑", GUILayout.Width(50f))) { BindSettingWindow.OpenWindow(); }
        }
        EditorGUILayout.EndHorizontal();

        if (this.bindSetting.selectCompositionSetting.commonSetting.isCreateScript)
        {
            if (this.bindSetting.selectCompositionSetting.scriptSetting.csharpScriptSetting.isGenerateNew) { DrawNewScript(); }
            else { DrawSelectScript(); }
        }

        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("生成配置预览", this.preivewStyle);

            SirenixEditorGUI.HorizontalLineSeparator(Color.white, 1);

            CommonSetting commonSetting = this.bindSetting.selectCompositionSetting.commonSetting;


            GUILayout.Label($"{GetBoolInfo(commonSetting.isCreatePrefab)}生成预制体", contentStyle);
            if (commonSetting.isCreatePrefab)
            {
                GUILayout.Label($"{GetBoolInfo(commonSetting.isDetachPrefab)}分离预制体", contentStyle);
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

    void DrawSelectScript()
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            GUILayout.Label("选择要附加信息的脚本：");
            if (GUILayout.Button(this.generateData.mergeTypeString.typeName))
            {
                BindDataHelper.DropDownBinDataTypeSwitch(this.editorObjectInfo.rootData, (select) => this.generateData.mergeTypeString = select);
            }
        }
        EditorGUILayout.EndHorizontal();

        ChackBuildError();
    }

    void ChackBuildError()
    {
        if (chackTypeString.Equals(this.generateData.mergeTypeString) == false)
        {
            chackTypeString = this.generateData.mergeTypeString;

            if (generateData.mergeTypeString.IsEmpty())
            {
                isNull = true;
                isError = true;
                return;
            }
            else { isNull = false; }

            Type monoType = typeof(MonoBehaviour);
            Type objectType = typeof(Object);
            if (monoType.IsSubclassOf(this.generateData.mergeTypeString.ToType()) == false || objectType == chackTypeString.ToType())
            {
                SirenixEditorGUI.ErrorMessageBox("选择的类型必须继承MonoBehaviour!!!");
                isTypeError = true;
                isError = true;
                return;
            }
            else { isTypeError = false; }

            this.isError = false;
        }
        if (this.isNull) { SirenixEditorGUI.ErrorMessageBox("选择的类型为空!!!"); }
        if (this.isTypeError) { SirenixEditorGUI.ErrorMessageBox("选择的类型必须继承MonoBehaviour!!!"); }
    }

    void DrawNewScript()
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            GUILayout.Label("新建脚本名：");
            string tempString = GUILayout.TextField(this.generateData.newScriptName);
            if (tempString.Equals(this.generateData.newScriptName) == false) this.generateData.newScriptName = CommonTools.GetNumberAlpha(this.generateData.newScriptName);
        }
        EditorGUILayout.EndHorizontal();
    }

    string GetBoolInfo(bool isEnable)
    {
        return isEnable ? "<color=green>启用</color>" : "<color=red>禁用</color>";
    }
}