using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class GenerateToMainAttribute : Attribute
{
    public bool isResetGenerate;

    public GenerateToMainAttribute(bool isReset = false)
    {
        this.isResetGenerate = isReset;
    }
}