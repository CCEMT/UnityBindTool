﻿#region Using

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace BindTool
{
    /// <summary>
    /// 自动绑定设置
    /// </summary>
    //[CreateAssetMenu(fileName = "AutoBindSetting", menuName = "CreatAutoBindSetting", order = 0)]
    public class AutoBindSetting : ScriptableObject
    {
        public string programName;

        //根据名称绑定
        public List<NameBindData> nameBindDataList = new List<NameBindData>();

        //忽略数据
        public List<NameCheck> nameLgnoreDataList = new List<NameCheck>();

        //是否启用流绑定
        public bool isEnableStreamingBind;

        //是否绑定
        public bool isBindComponent;

        //是否绑定所有
        public bool isBindAllComponent;

        //根据流绑定
        public List<StreamingBindData> streamingBindDataList = new List<StreamingBindData>();

        #region Equals

        public override bool Equals(object other)
        {
            AutoBindSetting equalsValue = (AutoBindSetting) other;
            if (equalsValue != null) return Equals(equalsValue);
            return false;
        }

        protected bool Equals(AutoBindSetting other)
        {
            return base.Equals(other) && this.programName == other.programName && Equals(this.nameBindDataList, other.nameBindDataList) && Equals(this.nameLgnoreDataList, other.nameLgnoreDataList) &&
                   this.isEnableStreamingBind == other.isEnableStreamingBind && this.isBindComponent == other.isBindComponent && this.isBindAllComponent == other.isBindAllComponent &&
                   Equals(this.streamingBindDataList, other.streamingBindDataList);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.programName != null ? this.programName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.nameBindDataList != null ? this.nameBindDataList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.nameLgnoreDataList != null ? this.nameLgnoreDataList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.isEnableStreamingBind.GetHashCode();
                hashCode = (hashCode * 397) ^ this.isBindComponent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.isBindAllComponent.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.streamingBindDataList != null ? this.streamingBindDataList.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }

    [Serializable]
    public class NameBindData
    {
        public NameCheck nameCheck = new NameCheck();
        public TypeString typeString;

        #region Equals

        public override bool Equals(object obj)
        {
            NameBindData equalsValue = (NameBindData) obj;
            if (equalsValue != null) return Equals(equalsValue);
            return false;
        }

        protected bool Equals(NameBindData other)
        {
            return Equals(nameCheck, other.nameCheck) && typeString.Equals(other.typeString);
        }

        public override int GetHashCode()
        {
            unchecked { return ((nameCheck != null ? nameCheck.GetHashCode() : 0) * 397) ^ typeString.GetHashCode(); }
        }

        #endregion
    }

    [Serializable]
    public class NameCheck
    {
        public string name;
        public NameRule nameRule;

        public bool Check(string content, out string matchingContent)
        {
            matchingContent = "";
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content)) return false;
            string tempName = name;
            string tempContent = content;

            if (nameRule.isCaseSensitive == false)
            {
                tempName = tempName.ToLower();
                tempContent = tempContent.ToLower();
            }

            switch (nameRule.nameMatchingRule)
            {
                case NameMatchingRule.Contain:
                    int index = tempContent.IndexOf(tempName, StringComparison.Ordinal);
                    if (index >= 0) matchingContent = content.Substring(index, name.Length);
                    return tempContent.Contains(tempName);
                case NameMatchingRule.Prefix:
                    string prefix = CommonTools.GetPrefix(content);
                    matchingContent = content.Substring(0, prefix.Length);
                    if (nameRule.isCaseSensitive == false) prefix = prefix.ToLower();
                    return tempName.Equals(prefix);
                case NameMatchingRule.Suffix:
                    string suffix = CommonTools.GetSuffix(content);
                    matchingContent = content.Substring(content.Length - suffix.Length, suffix.Length);
                    if (nameRule.isCaseSensitive == false) suffix = suffix.ToLower();
                    return tempName.Equals(suffix);
                case NameMatchingRule.All:
                    matchingContent = content;
                    return tempName.Equals(tempContent);
            }
            return false;
        }

        #region Equals

        public override bool Equals(object obj)
        {
            NameCheck nameCheck = (NameCheck) obj;
            if (nameCheck != null) return Equals(nameCheck);
            return false;
        }

        protected bool Equals(NameCheck other)
        {
            return name == other.name && nameRule.Equals(other.nameRule);
        }

        public override int GetHashCode()
        {
            unchecked { return ((name != null ? name.GetHashCode() : 0) * 397) ^ nameRule.GetHashCode(); }
        }

        #endregion
    }

    [Serializable]
    public class StreamingBindData
    {
        public int sequence;
        public bool isElse;
        public TypeString typeString;

        #region Equals

        public override bool Equals(object obj)
        {
            StreamingBindData equalsValue = (StreamingBindData) obj;
            if (equalsValue != null) return Equals(equalsValue);
            return false;
        }

        protected bool Equals(StreamingBindData other)
        {
            return sequence == other.sequence && isElse == other.isElse && typeString.Equals(other.typeString);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = sequence;
                hashCode = (hashCode * 397) ^ isElse.GetHashCode();
                hashCode = (hashCode * 397) ^ typeString.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }

    [Serializable]
    public struct NameRule
    {
        public bool isCaseSensitive; //是否区分大小写
        public NameMatchingRule nameMatchingRule; //名字匹配规则

        #region Equals

        public override bool Equals(object obj)
        {
            NameRule equalsValue = (NameRule) obj;
            return Equals(equalsValue);
        }

        public bool Equals(NameRule other)
        {
            return isCaseSensitive == other.isCaseSensitive && nameMatchingRule == other.nameMatchingRule;
        }

        public override int GetHashCode()
        {
            unchecked { return (isCaseSensitive.GetHashCode() * 397) ^ (int) nameMatchingRule; }
        }

        #endregion
    }

    public enum NameMatchingRule
    {
        Contain, //包含
        Prefix, //前缀匹配
        Suffix, //后缀匹配
        All, //全字匹配
    }
}