using BindTool;

public static class NameHelper
{
    public static string SetVariableName(string content, CreateNameSetting createNameSetting)
    {
        string newName = CommonTools.GetNumberAlpha(content);
        int amount = createNameSetting.variableNameReplaceDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            NameReplaceData nameReplaceData = createNameSetting.variableNameReplaceDataList[i];
            if (nameReplaceData.nameCheck.Check(newName, out string matchingContent)) newName = content.Replace(matchingContent, nameReplaceData.targetName);
        }

        return newName;
    }

    public static string SetPropertyName(string content, CreateNameSetting createNameSetting)
    {
        string newName = CommonTools.GetNumberAlpha(content);
        int amount = createNameSetting.propertyNameReplaceDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            NameReplaceData nameReplaceData = createNameSetting.propertyNameReplaceDataList[i];
            if (nameReplaceData.nameCheck.Check(newName, out string matchingContent)) newName = content.Replace(matchingContent, nameReplaceData.targetName);
        }

        return newName;
    }

    public static string NameSettingByName(ComponentBindInfo componentBindInfo, NameSetting nameSetting)
    {
        return NameSettingByName(componentBindInfo.name, componentBindInfo, nameSetting);
    }

    public static string NameSettingByName(string targetName, ComponentBindInfo componentBindInfo, NameSetting nameSetting)
    {
        switch (nameSetting.namingDispose)
        {
            case ScriptNamingDispose.InitialLower:
                targetName = CommonTools.InitialLower(targetName);
                break;
            case ScriptNamingDispose.InitialUpper:
                targetName = CommonTools.InitialUpper(targetName);
                break;
            case ScriptNamingDispose.AllLower:
                targetName = componentBindInfo.name.ToLower();
                break;
            case ScriptNamingDispose.AllUppe:
                targetName = componentBindInfo.name.ToUpper();
                break;
        }

        if (nameSetting.isAddClassName)
        {
            if (nameSetting.isFrontOrBehind) { targetName = componentBindInfo.GetTypeName() + componentBindInfo.name; }
            else { targetName = componentBindInfo.name + componentBindInfo.GetTypeName(); }
        }

        if (nameSetting.isAddFront) targetName = nameSetting.frontName + componentBindInfo.name;
        if (nameSetting.isAddBehind) targetName = componentBindInfo.name + nameSetting.behindName;
        return targetName;
    }

    public static string NameSettingByName(DataBindInfo dataBindInfo, NameSetting nameSetting)
    {
        return NameSettingByName(dataBindInfo.name, dataBindInfo, nameSetting);
    }

    public static string NameSettingByName(string targetName, DataBindInfo dataBindInfo, NameSetting nameSetting)
    {
        switch (nameSetting.namingDispose)
        {
            case ScriptNamingDispose.InitialLower:
                targetName = CommonTools.InitialLower(targetName);
                break;
            case ScriptNamingDispose.InitialUpper:
                targetName = CommonTools.InitialUpper(targetName);
                break;
            case ScriptNamingDispose.AllLower:
                targetName = dataBindInfo.name.ToLower();
                break;
            case ScriptNamingDispose.AllUppe:
                targetName = dataBindInfo.name.ToUpper();
                break;
        }

        if (nameSetting.isAddClassName)
        {
            if (nameSetting.isFrontOrBehind) { targetName = dataBindInfo.typeString.typeName + dataBindInfo.name; }
            else { targetName = dataBindInfo.name + dataBindInfo.typeString.typeName; }
        }

        if (nameSetting.isAddFront) targetName = nameSetting.frontName + dataBindInfo.name;
        if (nameSetting.isAddBehind) targetName = dataBindInfo.name + nameSetting.behindName;
        return targetName;
    }
}