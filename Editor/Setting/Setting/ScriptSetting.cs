#region Using

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#endregion

namespace BindTool
{
    //[CreateAssetMenu(fileName = "ScriptSetting", menuName = "CreatScriptSetting", order = 0)]
    public class ScriptSetting : ScriptableObject
    {
        //Common Setting
        public string programName;

        public bool isSavaOldScript; //是否保存旧脚本
        public DefaultAsset oldScriptFolderPath;
        
        //CSharp Setting
        
        //是否构建新的代码
        public bool isGenerateNew;

        //继承的类
        public TypeString inheritClass;

        public string createScriptAssembly;

        //是否分文件生成
        public bool isGeneratePartial;

        //子文件拓展名
        public string partialName;

        //是否使用命名空间
        public bool isSpecifyNamespace;

        //使用的命名空间
        public string useNamespace;

        //是否开启绑定时自动生成变量名称
        public VisitType variableVisitType; //字段的访问类型
        public NameSetting nameSetting;

        //是否创建属性
        public bool isAddProperty;
        public VisitType propertyVisitType;
        public PropertyType propertyType;
        public NameSetting propertyNameSetting;
        
        public DefaultAsset templateScriptSavaFolderPath;
        
        public NameSetting methodNameSetting;
        public List<VariableData> variableList;
        public List<TemplateData> templateDataList;
        
        //Lua Setting
    }

    [Serializable]
    public class VariableData
    {
        public VisitType visitType;
        public TypeString targetType;
        public TypeString variableType;
        public string varialbleName;
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
        //命名规范
        public ScriptNamingDispose namingDispose;

        //重复名称处理
        public RepetitionNameDispose repetitionNameDispose;

        //是否创建类型名称
        public bool isAddClassName;

        //类型名称是前缀还是后缀
        public bool isFrontOrBehind;

        //是否添加前缀
        public bool isAddFront;

        //前缀名称
        public string frontName;

        //是否添加后缀
        public bool isAddBehind;

        //后缀名称
        public string behindName;
    }

    public enum ScriptNamingDispose
    {
        None,
        InitialLower, //首字母小写
        InitialUpper, //首字母大写
        AllLower, //全小写
        AllUppe //全大写
    }

    public enum RepetitionNameDispose
    {
        AddNumber, //添加数字
        None, //不处理（当出现重复名时将会出现编译错误）
    }

    public enum PropertyType
    {
        Set,
        Get,
        SetAndGet
    }
}