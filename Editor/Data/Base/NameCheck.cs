using System;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;

[Serializable]
public class NameCheck
{
    [LabelText("检查名称")]
    public string name;

    [LabelText("检查规则")]
    public NameRule nameRule = new NameRule();
}

[Serializable]
public struct NameRule
{
    [LabelText("是否区分大小写")]
    public bool isCaseSensitive; //是否区分大小写

    [LabelText("匹配规则")]
    public NameMatchingRule nameMatchingRule; //名字匹配规则
}

public enum NameMatchingRule
{
    [LabelText("包含")]
    Contain,

    [LabelText("前缀匹配")]
    Prefix,

    [LabelText("后缀匹配")]
    Suffix,

    [LabelText("全字匹配")]
    All,
}