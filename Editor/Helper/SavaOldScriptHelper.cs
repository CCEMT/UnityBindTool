using System;
using System.IO;
using UnityEditor;

namespace UnityBindTool
{
    public static class SavaOldScriptHelper
    {
        public static string GetDirectoryNameByGenerateData(GenerateData generateData)
        {
            if (generateData.mergeTypeString.IsEmpty() == false) { return generateData.mergeTypeString.typeName; }
            return generateData.newScriptName;
        }

        public static void GenerateOldScriptFile(ScriptSetting setting, string path, string directoryName)
        {
            DefaultAsset saveFolder = setting.oldScriptFolderPath;
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
}