using BindTool;

public static class CSharpGeneratorHelper
{
    public const string PropertyValue = "value";
    public const string MethodVoid = "void";

    public static string GetPropertyName(string fieldName, CreateNameSetting createNameSetting)
    {
        string propertyName = CommonTools.SetPropertyName(fieldName, createNameSetting);
        return propertyName;
    }
}