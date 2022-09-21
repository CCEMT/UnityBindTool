#region Using

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace BindTool
{
    //[CreateAssetMenu(fileName = "ScriptSetting", menuName = "CreatScriptSetting", order = 0)]
    public class ScriptSetting : ScriptableObject
    {
        public string programName;

        //是否构建新的代码
        public bool isGenerateNew;

        //继承的类
        public TypeString inheritClass;

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

        public bool isSavaOldScript; //是否保存旧脚本
        public string savaOldScriptPath; //保存路径

        public VisitType methodVisitType;
        public NameSetting methodNameSetting;
        public List<VariableData> variableList;
        public List<TemplateData> templateDataList;

        public string templateScriptSavaPath;//模板脚本保存路径

        #region Equals

        public override bool Equals(object obj)
        {
            ScriptSetting scriptSetting = (ScriptSetting) obj;
            if (scriptSetting != null) { return Equals(scriptSetting); }
            else { return false; }
        }

        protected bool Equals(ScriptSetting other)
        {
            return programName == other.programName && isGenerateNew == other.isGenerateNew && inheritClass.Equals(other.inheritClass) && isGeneratePartial == other.isGeneratePartial &&
                   partialName == other.partialName && isSpecifyNamespace == other.isSpecifyNamespace && useNamespace == other.useNamespace && variableVisitType == other.variableVisitType &&
                   nameSetting.Equals(other.nameSetting) && isAddProperty == other.isAddProperty && propertyVisitType == other.propertyVisitType && propertyType == other.propertyType &&
                   propertyNameSetting.Equals(other.propertyNameSetting) && isSavaOldScript == other.isSavaOldScript && savaOldScriptPath == other.savaOldScriptPath && methodVisitType == other.methodVisitType &&
                   methodNameSetting.Equals(other.methodNameSetting) && Equals(variableList, other.variableList) && Equals(templateDataList, other.templateDataList);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = programName != null ? programName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ isGenerateNew.GetHashCode();
                hashCode = (hashCode * 397) ^ inheritClass.GetHashCode();
                hashCode = (hashCode * 397) ^ isGeneratePartial.GetHashCode();
                hashCode = (hashCode * 397) ^ (partialName != null ? partialName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isSpecifyNamespace.GetHashCode();
                hashCode = (hashCode * 397) ^ (useNamespace != null ? useNamespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) variableVisitType;
                hashCode = (hashCode * 397) ^ nameSetting.GetHashCode();
                hashCode = (hashCode * 397) ^ isAddProperty.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) propertyVisitType;
                hashCode = (hashCode * 397) ^ (int) propertyType;
                hashCode = (hashCode * 397) ^ propertyNameSetting.GetHashCode();
                hashCode = (hashCode * 397) ^ isSavaOldScript.GetHashCode();
                hashCode = (hashCode * 397) ^ (savaOldScriptPath != null ? savaOldScriptPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) methodVisitType;
                hashCode = (hashCode * 397) ^ methodNameSetting.GetHashCode();
                hashCode = (hashCode * 397) ^ (variableList != null ? variableList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (templateDataList != null ? templateDataList.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }

    [Serializable]
    public class VariableData
    {
        public VisitType visitType;
        public TypeString targetType;
        public TypeString variableType;
        public string varialbleName;

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                VariableData variableData = obj as VariableData;
                if (variableData != null) return Equals(variableData);
            }
            return false;
        }

        public bool Equals(VariableData other)
        {
            return visitType == other.visitType && targetType.Equals(other.targetType) && variableType.Equals(other.variableType) && varialbleName == other.varialbleName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) visitType;
                hashCode = (hashCode * 397) ^ targetType.GetHashCode();
                hashCode = (hashCode * 397) ^ variableType.GetHashCode();
                hashCode = (hashCode * 397) ^ (varialbleName != null ? varialbleName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }

    [Serializable]
    public class TemplateData
    {
        public TypeString targetType;
        public TypeString temlateType;
        public TypeString temlateBaseType;

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                TemplateData templateData = obj as TemplateData;
                if (templateData != null) return Equals(templateData);
            }
            return false;
        }

        protected bool Equals(TemplateData other)
        {
            return targetType.Equals(other.targetType) && temlateType.Equals(other.temlateType) && temlateBaseType.Equals(other.temlateBaseType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = targetType.GetHashCode();
                hashCode = (hashCode * 397) ^ temlateType.GetHashCode();
                hashCode = (hashCode * 397) ^ temlateBaseType.GetHashCode();
                return hashCode;
            }
        }

        #endregion
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

        #region Equals

        public override bool Equals(object obj)
        {
            NameSetting? nameSetting = (NameSetting?) obj;
            if (nameSetting != null) { return Equals(nameSetting); }
            else { return false; }
        }

        public bool Equals(NameSetting other)
        {
            return namingDispose == other.namingDispose && repetitionNameDispose == other.repetitionNameDispose && isAddClassName == other.isAddClassName && isFrontOrBehind == other.isFrontOrBehind &&
                   isAddFront == other.isAddFront && frontName == other.frontName && isAddBehind == other.isAddBehind && behindName == other.behindName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) namingDispose;
                hashCode = (hashCode * 397) ^ (int) repetitionNameDispose;
                hashCode = (hashCode * 397) ^ isAddClassName.GetHashCode();
                hashCode = (hashCode * 397) ^ isFrontOrBehind.GetHashCode();
                hashCode = (hashCode * 397) ^ isAddFront.GetHashCode();
                hashCode = (hashCode * 397) ^ (frontName != null ? frontName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isAddBehind.GetHashCode();
                hashCode = (hashCode * 397) ^ (behindName != null ? behindName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
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