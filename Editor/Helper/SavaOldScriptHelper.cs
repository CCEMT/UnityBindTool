using System;
using System.IO;
using BindTool;
using UnityEditor;

public static class SavaOldScriptHelper
{
    public static string GetDirectoryNameByGenerateData(GenerateData generateData)
    {
        if (generateData.mergeTypeString.IsEmpty() == false) { return generateData.mergeTypeString.typeName; }
        return generateData.newScriptName;
    }

    public static void GenerateOldScriptFile(string path, string directoryName)
    {
        MainSetting mainSetting = MainSetting.Get();

        DefaultAsset saveFolder = mainSetting.selectScriptSetting.oldScriptFolderPath;
        string savePath = AssetDatabase.GetAssetPath(saveFolder);

        string fileName = Path.GetFileNameWithoutExtension(savePath);
        string fileContent = File.ReadAllText(path);

        string directoryPath = savePath + directoryName;

        if (Directory.Exists(directoryPath) == false) Directory.CreateDirectory(directoryPath);

        string timeNumber = DateTime.Now.ToString("yyyyMMddHHmmssms");
        int guid = Guid.NewGuid().GetHashCode();
        string targetID = timeNumber + guid;

        string fullPath = $"{savePath}/{directoryName}/{fileName}_{targetID}{CommonConst.TextFileSuffix}";

        StreamWriter streamWriter = File.CreateText(fullPath);
        streamWriter.Write(fileContent);
        streamWriter.Close();
    }
}