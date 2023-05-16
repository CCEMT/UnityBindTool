using System;

[AttributeUsage(AttributeTargets.Field)]
public class TemplateFieldAttribute : Attribute
{
    public bool inclusionSubclass;

    public TemplateFieldAttribute(bool inclusion = false)
    {
        this.inclusionSubclass = inclusion;
    }
}