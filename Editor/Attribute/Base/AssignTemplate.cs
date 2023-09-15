using System;

namespace UnityBindTool
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AssignTemplate : Attribute
    {
        public Type type;

        public AssignTemplate(Type assign)
        {
            this.type = assign;
        }
    }
}