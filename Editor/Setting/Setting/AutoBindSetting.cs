#region Using

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
    }

    [Serializable]
    public class NameBindData
    {
        public NameCheck nameCheck = new NameCheck();
        public TypeString typeString;
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
    }

    [Serializable]
    public class StreamingBindData
    {
        public int sequence;
        public bool isElse;
        public TypeString typeString;
    }

    [Serializable]
    public struct NameRule
    {
        public bool isCaseSensitive; //是否区分大小写
        public NameMatchingRule nameMatchingRule; //名字匹配规则
    }

    public enum NameMatchingRule
    {
        Contain, //包含
        Prefix, //前缀匹配
        Suffix, //后缀匹配
        All, //全字匹配
    }
}