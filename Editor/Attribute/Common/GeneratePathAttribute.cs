using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class GeneratePathAttribute : Attribute
{
    public enum PathType
    {
        Prefab,
        CSharp,
        Lua,
    }

    public PathType pathType;

    public GeneratePathAttribute(PathType pathType = PathType.Prefab)
    {
        this.pathType = pathType;
    }
}