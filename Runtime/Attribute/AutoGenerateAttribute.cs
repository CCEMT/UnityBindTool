using System;

namespace BindTool
{
    public enum AutoGenerateType
    {
        OriginalField
    }
    public class AutoGenerateAttribute : Attribute
    {
        public AutoGenerateType autoGenerateType;
        public AutoGenerateAttribute(AutoGenerateType type)
        {
            this.autoGenerateType = type;
        }
    }
}