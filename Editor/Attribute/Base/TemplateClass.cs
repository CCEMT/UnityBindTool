using System;

namespace UnityBindTool
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TemplateClass : Attribute
    {
        public CSharpTemplateType templateType;

        public TemplateClass(CSharpTemplateType templateType)
        {
            this.templateType = templateType;
        }
    }
}