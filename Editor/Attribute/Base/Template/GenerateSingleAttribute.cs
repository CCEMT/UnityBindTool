using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class GenerateSingleAttribute : Attribute
{
    public bool isMustGenerate;

    public GenerateSingleAttribute(bool isOn = false)
    {
        this.isMustGenerate = isOn;
    }
}