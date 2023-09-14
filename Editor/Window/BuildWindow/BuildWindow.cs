using System;
using BindTool;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuildWindow : OdinEditorWindow
{
    public static void OpenWindow(GenerateData data, Vector2 position)
    {
        var window = GetWindow<BuildWindow>(true, "BuildWindow");
        Vector2 size = new Vector2(400, 350);
        window.position = new Rect(position - new Vector2(size.x / 2f, size.y / 2f), size);
        window.Init(data);
        window.Show();
    }

    private GUIStyle buildButtonStyle;
    private GUIStyle preivewStyle;
    private GUIStyle contentStyle;

    private bool isError;
    private TypeString chackTypeString;
    private bool isNull;
    private bool isTypeError;

    private BindSetting bindSetting;
    private GenerateData generateData;

    void Init(GenerateData data)
    {
        generateData = data;
        bindSetting = BindSetting.Get();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bindSetting = null;
    }

    protected override void OnGUI()
    {
        if (this.bindSetting == null)
        {
            Close();
            return;
        }

        GetDrawBuildGUIGUIStyle();

        DrawBuild();
        DrawSetting();
        DrawPreivewSetting();
    }

    void GetDrawBuildGUIGUIStyle()
    {
        if (buildButtonStyle == null)
        {
            buildButtonStyle = new GUIStyle(GUI.skin.button);
            buildButtonStyle.fontSize = 18;
        }

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

    void DrawBuild()
    {
        EditorGUILayout.BeginVertical("box");

        GUI.color = Color.green;
        if (GUILayout.Button("生成", buildButtonStyle, GUILayout.Height(30)))
        {
            if (this.isError) { Debug.LogError("生成失败，请先解决错误"); }
            else
            {
                BindBuild.Build(bindSetting.selectCompositionSetting, this.generateData);
                bindSetting = null;
            }
        }
        GUI.color = Color.white;

        if (bindSetting != null && bindSetting.selectCompositionSetting.commonSetting.isCreateScript)
        {
            if (bindSetting.selectCompositionSetting.scriptSetting.csharpScriptSetting.isGenerateNew) { DrawNewScript(); }
            else { DrawSelectScript(); }
        }

        EditorGUILayout.EndVertical();
    }

    void DrawNewScript()
    {
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("新建脚本名：");
            string tempString = GUILayout.TextField(this.generateData.newScriptName);
            if (tempString.Equals(this.generateData.newScriptName) == false) this.generateData.newScriptName = CommonTools.GetNumberAlpha(tempString);
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawSelectScript()
    {
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("选择要附加信息的脚本：");
            if (GUILayout.Button(this.generateData.mergeTypeString.typeName))
            {
                BindDataHelper.DropDownBinDataTypeSwitch(this.generateData.objectInfo.rootData, (select) => this.generateData.mergeTypeString = select);
            }
        }
        EditorGUILayout.EndHorizontal();

        ChackBuildError();
    }

    void DrawSetting()
    {
        if (bindSetting == null) return;
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal("box");
        {
            string content = this.bindSetting.selectCompositionSetting != null ? bindSetting.selectCompositionSetting.compositionName : "选择设置为空，请先选择设置";
            GUILayout.Label($"生成配置：{content}");

            if (GUILayout.Button("编辑", GUILayout.Width(50f))) { BindSettingWindow.OpenWindow(); }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawPreivewSetting()
    {
        if (bindSetting == null) return;
        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("生成配置预览", this.preivewStyle);

            SirenixEditorGUI.HorizontalLineSeparator(Color.white);

            CommonSetting commonSetting = bindSetting.selectCompositionSetting.commonSetting;

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

    string GetBoolInfo(bool isEnable)
    {
        return isEnable ? "<color=green>启用</color>" : "<color=red>禁用</color>";
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
}