using System;
using BindTool;

public static class NameHelper
{
    public static string SetVariableName(string content, NameGenerateSetting nameGenerateSetting)
    {
        string newName = CommonTools.GetNumberAlpha(content);
        int amount = nameGenerateSetting.nameReplaceDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            NameReplaceData nameReplaceData = nameGenerateSetting.nameReplaceDataList[i];
            if (NameCheckContent(nameReplaceData.nameCheck, newName, out string matchingContent)) { newName = content.Replace(matchingContent, nameReplaceData.targetName); }
        }

        return newName;
    }

    public static string SetPropertyName(string content, CSharpNameGenerateSetting createNameSetting)
    {
        string newName = CommonTools.GetNumberAlpha(content);
        int amount = createNameSetting.propertyNameReplaceDataList.Count;
        for (int i = 0; i < amount; i++)
        {
            NameReplaceData nameReplaceData = createNameSetting.propertyNameReplaceDataList[i];
            if (NameCheckContent(nameReplaceData.nameCheck, newName, out string matchingContent)) { newName = content.Replace(matchingContent, nameReplaceData.targetName); }
        }

        return newName;
    }

    public static string NameSettingByName(BindData bindData, NameSetting nameSetting)
    {
        return NameSettingByName(bindData.name, bindData, nameSetting);
    }

    public static string NameSettingByName(string targetName, BindData bindData, NameSetting nameSetting)
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
                targetName = bindData.name.ToLower();
                break;
            case ScriptNamingDispose.AllUppe:
                targetName = bindData.name.ToUpper();
                break;
        }

        if (nameSetting.isAddClassName)
        {
            if (nameSetting.isFrontOrBehind) { targetName = bindData.GetTypeName() + bindData.name; }
            else { targetName = bindData.name + bindData.GetTypeName(); }
        }

        if (nameSetting.isAddFront) targetName = nameSetting.frontName + bindData.name;
        if (nameSetting.isAddBehind) targetName = bindData.name + nameSetting.behindName;
        return targetName;
    }

    public static bool NameCheckContent(NameCheck nameCheck, string content, out string matchingContent)
    {
        matchingContent = "";
        if (string.IsNullOrEmpty(nameCheck.name) || string.IsNullOrEmpty(content)) return false;
        string tempName = nameCheck.name;
        string tempContent = content;

        if (nameCheck.nameRule.isCaseSensitive == false)
        {
            tempName = tempName.ToLower();
            tempContent = tempContent.ToLower();
        }

        switch (nameCheck.nameRule.nameMatchingRule)
        {
            case NameMatchingRule.Contain:
                int index = tempContent.IndexOf(tempName, StringComparison.Ordinal);
                if (index >= 0) matchingContent = content.Substring(index, nameCheck.name.Length);
                return tempContent.Contains(tempName);
            case NameMatchingRule.Prefix:
                string prefix = CommonTools.GetPrefix(content);
                matchingContent = content.Substring(0, prefix.Length);
                if (nameCheck.nameRule.isCaseSensitive == false) prefix = prefix.ToLower();
                return tempName.Equals(prefix);
            case NameMatchingRule.Suffix:
                string suffix = CommonTools.GetSuffix(content);
                matchingContent = content.Substring(content.Length - suffix.Length, suffix.Length);
                if (nameCheck.nameRule.isCaseSensitive == false) suffix = suffix.ToLower();
                return tempName.Equals(suffix);
            case NameMatchingRule.All:
                matchingContent = content;
                return tempName.Equals(tempContent);
        }
        return false;
    }
}