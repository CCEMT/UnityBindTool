using System;
using System.Collections.Generic;
using BindTool;
using Sirenix.OdinInspector;
using UnityEditor;

[Serializable]
public class CSharpScriptSetting
{
    [LabelText("是否构建新的代码"), BoxGroup("基础设置")]
    public bool isGenerateNew;

    [LabelText("继承的类"), BoxGroup("基础设置")]
    public TypeString inheritClass;

    [LabelText("目标程序集"), BoxGroup("基础设置")]
    public string createScriptAssembly;

    [LabelText("是否分文件生成"), BoxGroup("基础设置")]
    public bool isGeneratePartial;

    [LabelText("子文件拓展名"), BoxGroup("基础设置")]
    public string partialName;

    [LabelText("是否使用命名空间"), BoxGroup("基础设置")]
    public bool isSpecifyNamespace;

    [LabelText("使用的命名空间"), BoxGroup("基础设置")]
    public string useNamespace;

    [LabelText("字段访问类型"), BoxGroup("基础设置")]
    public VisitType variableVisitType;

    [LabelText("是否创建属性"), BoxGroup("属性设置")]
    public bool isAddProperty;

    [LabelText("属性访问类型"), BoxGroup("属性设置")]
    public VisitType propertyVisitType;

    [LabelText("属性设置"), BoxGroup("属性设置")]
    public PropertyType propertyType;

    [BoxGroup("属性设置"), HideLabel]
    public NameSetting propertyNameSetting = new NameSetting();

    [LabelText("模板脚本保存路径"), BoxGroup("模板脚本设置")]
    public DefaultAsset templateScriptSavaFolderPath;

    [BoxGroup("模板脚本设置"), HideLabel]
    public NameSetting methodNameSetting = new NameSetting();

    [LabelText("模板列表")]
    public List<TemplateData> templateDataList = new List<TemplateData>();
}

[Serializable]
public class TemplateData
{
    public TypeString targetType;
    public TypeString temlateType;
    public TypeString temlateBaseType;
}

[Serializable]
public struct NameSetting
{
    [LabelText("命名规范")]
    public ScriptNamingDispose namingDispose;

    [LabelText("重复名称处理")]
    public RepetitionNameDispose repetitionNameDispose;

    [LabelText("是否添加类型名称")]
    public bool isAddClassName;

    [LabelText("类型名称是前缀还是后缀")]
    public bool isFrontOrBehind;

    [LabelText("是否添加前缀")]
    public bool isAddFront;

    [LabelText("前缀名称")]
    public string frontName;

    [LabelText("是否添加后缀")]
    public bool isAddBehind;

    [LabelText("后缀名称")]
    public string behindName;
}

public enum ScriptNamingDispose
{
    [LabelText("不处理")]
    None,

    [LabelText("首字母小写")]
    InitialLower,

    [LabelText("首字母大写")]
    InitialUpper,

    [LabelText("全小写")]
    AllLower,

    [LabelText("全大写")]
    AllUppe
}

public enum RepetitionNameDispose
{
    [LabelText("添加数字")]
    AddNumber, //添加数字

    [LabelText("不处理")]
    None, //不处理（当出现重复名时将会出现编译错误）
}

public enum PropertyType
{
    [LabelText("生成Get")]
    Get,

    [LabelText("生成Set")]
    Set,

    [LabelText("生成Set和Get")]
    SetAndGet
}