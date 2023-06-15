using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class GenerateSingleAttribute : Attribute
{
    public bool isSingleContent;

    public GenerateSingleAttribute(bool isOn = false)
    {
        this.isSingleContent = isOn;
    }
}