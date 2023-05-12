using System;

namespace BindTool
{
    public enum AutoGenerateType
    {
        Content,
    }

    public class AutoGenerate : Attribute
    {
        public AutoGenerateType autoGenerateType;

        public AutoGenerate(AutoGenerateType type = AutoGenerateType.Content)
        {
            this.autoGenerateType = type;
        }
    }
}