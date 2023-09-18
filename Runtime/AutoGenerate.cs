using System;

namespace UnityBindTool
{
    public enum AutoGenerateType
    {
        Content,
        OriginalField,
        FixedContent,
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