#region Using

using System;

#endregion

namespace BindTool
{
    /// <summary>
    /// 标记为模板脚本
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptTemplateAttribute : Attribute
    { }
}