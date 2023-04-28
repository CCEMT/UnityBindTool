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
        


        #region Equals

        public override bool Equals(object obj)
        {
            ScriptSetting scriptSetting = (ScriptSetting) obj;
            if (scriptSetting != null) { return Equals(scriptSetting); }
            else { return false; }
        }

        protected bool Equals(ScriptSetting other)
        {
            return base.Equals(other) && this.programName == other.programName && this.isGenerateNew == other.isGenerateNew && this.inheritClass.Equals(other.inheritClass) &&
                   this.createScriptAssembly == other.createScriptAssembly && this.isGeneratePartial == other.isGeneratePartial && this.partialName == other.partialName &&
                   this.isSpecifyNamespace == other.isSpecifyNamespace && this.useNamespace == other.useNamespace && this.variableVisitType == other.variableVisitType &&
                   this.nameSetting.Equals(other.nameSetting) && this.isAddProperty == other.isAddProperty && this.propertyVisitType == other.propertyVisitType &&
                   this.propertyType == other.propertyType && this.propertyNameSetting.Equals(other.propertyNameSetting) && this.isSavaOldScript == other.isSavaOldScript &&
                   Equals(this.oldScriptFolderPath, other.oldScriptFolderPath) && this.methodNameSetting.Equals(other.methodNameSetting) && Equals(this.variableList, other.variableList) &&
                   Equals(this.templateDataList, other.templateDataList) && Equals(this.templateScriptSavaFolderPath, other.templateScriptSavaFolderPath);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(base.GetHashCode());
            hashCode.Add(this.programName);
            hashCode.Add(this.isGenerateNew);
            hashCode.Add(this.inheritClass);
            hashCode.Add(this.createScriptAssembly);
            hashCode.Add(this.isGeneratePartial);
            hashCode.Add(this.partialName);
            hashCode.Add(this.isSpecifyNamespace);
            hashCode.Add(this.useNamespace);
            hashCode.Add((int) this.variableVisitType);
            hashCode.Add(this.nameSetting);
            hashCode.Add(this.isAddProperty);
            hashCode.Add((int) this.propertyVisitType);
            hashCode.Add((int) this.propertyType);
            hashCode.Add(this.propertyNameSetting);
            hashCode.Add(this.isSavaOldScript);
            hashCode.Add(this.oldScriptFolderPath);
            hashCode.Add(this.methodNameSetting);
            hashCode.Add(this.variableList);
            hashCode.Add(this.templateDataList);
            hashCode.Add(this.templateScriptSavaFolderPath);
            return hashCode.ToHashCode();
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
                int hashCode = (int) visitType;
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
                int hashCode = targetType.GetHashCode();
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
            NameSetting nameSetting = (NameSetting) obj;
            return Equals(nameSetting);
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
                int hashCode = (int) namingDispose;
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