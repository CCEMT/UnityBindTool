#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#endregion

namespace BindTool
{
    public class ScriptGenerate
    {
        public static void CSharpWrite(CommonSettingData commonSettingData, string scriptPath)
        {
            var selectSettion = commonSettingData.selectScriptSetting;
            var tempData = commonSettingData.tempGenerateData;

            if (selectSettion.isGenerateNew)
            {
                string scriptFile = scriptPath + $"{commonSettingData.tempGenerateData.newScriptName}.cs";

                if (selectSettion.isGeneratePartial)
                {
                    //创建空的主文件
                    if (File.Exists(scriptFile) == false)
                    {
                        bool isExist = TypeString.IsExist(tempData.newScriptName, selectSettion.useNamespace, ConstData.DefaultAssembly);
                        if (isExist)
                        {
                            Debug.LogError("ScriptGenerateError:已经存在该脚本");
                            commonSettingData.tempGenerateData.isStartBuild = false;
                            return;
                        }

                        //创建
                        GenerateCSharpScript(commonSettingData, scriptFile, tempData.newScriptName, false, true);
                    }
                    else
                    {
                        //检查是否包含Partial关键字
                        string content = File.ReadAllText(scriptFile);
                        string filtration = content.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                        if (filtration.Contains($"partialclass{tempData.addTypeString.typeName}") == false)
                        {
                            int index = content.IndexOf(tempData.addTypeString.typeName, StringComparison.Ordinal);
                            string interval = string.Empty;
                            for (int i = index - 1; i >= 0; i--)
                            {
                                if (content[i].Equals('s')) { break; }
                                else { interval += content[i].ToString(); }
                            }
                            content = content.Replace($"class{interval}{tempData.addTypeString.typeName}", $"partial class {tempData.addTypeString.typeName}");
                        }
                        StreamWriter sw = new StreamWriter(scriptFile, false);
                        sw.WriteLine(content);
                        sw.Close();
                    }

                    string scriptPartialFile = scriptPath + $"{commonSettingData.tempGenerateData.newScriptName}.{selectSettion.partialName}.cs";
                    //创建Partial并写入数据
                    if (File.Exists(scriptPartialFile))
                    {
                        if (selectSettion.isSavaOldScript) SavaOldScript(scriptPartialFile, commonSettingData);
                        File.Delete(scriptPartialFile);
                    }
                    GenerateCSharpScript(commonSettingData, scriptPartialFile, tempData.newScriptName, true, true);
                }
                else
                {
                    //创建文件并写入数据
                    if (File.Exists(scriptFile) == false)
                    {
                        bool isExist = TypeString.IsExist(tempData.newScriptName, selectSettion.useNamespace, ConstData.DefaultAssembly);
                        if (isExist)
                        {
                            Debug.LogError("ScriptGenerateError:已经存在该脚本");
                            commonSettingData.tempGenerateData.isStartBuild = false;
                            return;
                        }

                        GenerateCSharpScript(commonSettingData, scriptFile, tempData.newScriptName, true, false);
                    }
                    else
                    {
                        //打开文件并更改数据
                        if (selectSettion.isSavaOldScript) SavaOldScript(scriptFile, commonSettingData);
                        OpenCSharpFileAlterData(commonSettingData, scriptFile);
                    }
                }
            }
            else
            {
                if (selectSettion.isGeneratePartial)
                {
                    string scriptPartialFile = scriptPath + $"{commonSettingData.tempGenerateData.newScriptName}.{selectSettion.partialName}.cs";
                    //检查是否包含Partial关键字
                    //创建Partial并写入数据

                    if (File.Exists(scriptPartialFile))
                    {
                        if (selectSettion.isSavaOldScript) SavaOldScript(scriptPartialFile, commonSettingData);
                        File.Delete(scriptPartialFile);
                    }
                    GenerateCSharpScript(commonSettingData, scriptPartialFile, tempData.objectInfo.typeString.typeName, true, true);
                }
                else
                {
                    string scriptPartialFile = scriptPath + $"{commonSettingData.tempGenerateData.newScriptName}.cs";
                    //打开文件并更改数据
                    if (selectSettion.isSavaOldScript) SavaOldScript(scriptPartialFile, commonSettingData);
                    OpenCSharpFileAlterData(commonSettingData, scriptPartialFile);
                }
            }
        }

        static void OpenCSharpFileAlterData(CommonSettingData commonSettingData, string path)
        {
            var selectSettion = commonSettingData.selectScriptSetting;
            var tempData = commonSettingData.tempGenerateData;

            if (selectSettion.isSavaOldScript) SavaOldScript(path, commonSettingData);

            bool isSpecifyNamespace = File.ReadAllText(path).Contains("namespace");

            //找到数据
            List<string> contents = File.ReadAllLines(path).ToList();

            int startLine = -1;
            int endLine = -1;

            int lineAmount = contents.Count;
            for (int i = 0; i < lineAmount; i++)
            {
                string line = contents[i];
                if (startLine == -1)
                {
                    if (line.Contains($"#region {ConstData.DefaultName}")) startLine = i;
                }
                else
                {
                    if (line.Contains("#region Method")) endLine = -2;

                    if (endLine == -2)
                    {
                        if (line.Contains("#endregion")) endLine = -1;
                    }
                    else if (endLine == -1)
                    {
                        if (line.Contains("#endregion")) endLine = i;
                    }
                    else { break; }
                }
            }
            if (startLine == -1 || endLine == -1)
            {
                //未找到数据
                int classLine = -1;
                for (int i = 0; i < lineAmount; i++)
                {
                    string line = contents[i];
                    if (classLine == -1)
                    {
                        if (line.Contains(tempData.addTypeString.typeName))
                        {
                            classLine = i;
                            int index = line.IndexOf(tempData.addTypeString.typeName, StringComparison.Ordinal);
                            index += tempData.addTypeString.typeName.Length;
                            int lineLength = line.Length;
                            for (int j = index; j < lineLength; j++)
                            {
                                if (line[j] == '{')
                                {
                                    startLine = i + 1;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        int lineLength = line.Length;
                        for (int j = 0; j < lineLength; j++)
                        {
                            if (line[j] == '{')
                            {
                                startLine = i + 1;
                                break;
                            }
                        }
                    }
                }
                if (startLine == -1)
                {
                    Debug.Log("文件格式错误");
                    return;
                }
            }
            else
            {
                //删除数据
                int deleteAmount = endLine - startLine;
                for (int i = 0; i < deleteAmount + 1; i++) contents.RemoveAt(startLine);
            }

            //添加数据
            List<string> addData = GenerateCSharpData.Generate(commonSettingData, isSpecifyNamespace);
            int addAmount = addData.Count;
            for (int i = 0; i < addAmount; i++) contents.Insert(startLine + i, addData[i]);

            StreamWriter sw = new StreamWriter(path, false);
            int targetAmount = contents.Count;
            for (int i = 0; i < targetAmount; i++) sw.WriteLine(contents[i]);
            sw.Close();
        }

        static void SavaOldScript(string path, CommonSettingData commonSettingData)
        {
            var selectSettion = commonSettingData.selectScriptSetting;
            var tempData = commonSettingData.tempGenerateData;

            string savaPath = Application.dataPath + "/" + selectSettion.savaOldScriptPath + "/";

            string directoryName = tempData.addTypeString.typeName + "/";
            string directoryPath = savaPath + directoryName;

            string savaText = File.ReadAllText(path);

            if (Directory.Exists(directoryPath) == false) Directory.CreateDirectory(directoryPath);
            DirectoryInfo direction = new DirectoryInfo(directoryPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            string fileName = tempData.addTypeString.typeName + ".txt";

            List<string> nameList = new List<string>();
            int amount = files.Length;
            for (int i = 0; i < amount; i++)
            {
                if (files[i].Name.EndsWith(".meta")) continue;
                nameList.Add(files[i].Name);
            }

            bool isCan = false;
            int number = 1;
            while (isCan == false)
            {
                string tempName = tempData.addTypeString.typeName + number + ".txt";
                if (nameList.Contains(tempName) == false)
                {
                    isCan = true;
                    fileName = tempName;
                }
                else { number++; }
            }
            string fullPath = directoryPath + fileName;

            StreamWriter mainWriter = File.CreateText(fullPath);
            mainWriter.Write(savaText);
            mainWriter.Close();
        }

        static void GenerateCSharpScript(CommonSettingData commonSettingData, string path, string className, bool isGenerateData, bool isPartial)
        {
            var selectSettion = commonSettingData.selectScriptSetting;
            var tempData = commonSettingData.tempGenerateData;

            tempData.objectInfo.typeString.typeName = tempData.newScriptName;
            tempData.objectInfo.typeString.typeNameSpace = selectSettion.useNamespace;
            tempData.objectInfo.typeString.assemblyName = ConstData.DefaultAssembly;

            StreamWriter mainWriter = File.CreateText(path);

            if (selectSettion.isSpecifyNamespace)
            {
                Writer($"namespace {selectSettion.useNamespace}");
                Writer("{");
            }

            string inheritContent = "UnityEngine.MonoBehaviour";
            if (string.IsNullOrEmpty(selectSettion.inheritClass.typeName) == false) inheritContent = $"{selectSettion.inheritClass.GetVisitString()}";
            string partialString = "";
            if (isPartial) partialString = "partial";
            Writer($"public {partialString} class {className} : {inheritContent}", 0);
            Writer("{", 0);

            if (isGenerateData)
            {
                List<string> addList = GenerateCSharpData.Generate(commonSettingData, selectSettion.isSpecifyNamespace);
                int addAmount = addList.Count;
                for (int i = 0; i < addAmount; i++)
                {
                    string addline = addList[i];
                    mainWriter.WriteLine(addline);
                }
            }

            Writer("}", 0);

            if (selectSettion.isSpecifyNamespace) Writer("}");

            mainWriter.Close();

            string CreateTab(int level)
            {
                if (selectSettion.isSpecifyNamespace) level++;
                string content = "";
                for (int i = 0; i < level; i++) content += "\t";
                return content;
            }

            void Writer(string content, int level = -1)
            {
                mainWriter.WriteLine(CreateTab(level) + content);
            }
        }

        public static TypeString GenerateCSharpTemplateScript(TypeString targetType, string path)
        {
            string fullPath = $"{Application.dataPath}/{path}/{targetType.typeName}Template.cs";

            StreamWriter mainWriter = File.CreateText(fullPath);

            Writer($"namespace {ConstData.TemplateNamespace}");
            Writer("{");

            string inheritContent = "UnityEngine.MonoBehaviour";
            Writer($"public class {targetType.typeName}Template : {inheritContent}", 1);
            Writer("{", 1);

            Writer($"public {targetType.GetVisitString()} templateValue;", 2);

            Writer($"#region {ConstData.TemplateRegionName}", 2);
            mainWriter.WriteLine();
            Writer("#endregion", 2);

            Writer("}", 1);

            Writer("}");

            mainWriter.Close();

            string CreateTab(int level)
            {
                string content = "";
                for (int i = 0; i < level; i++) content += "\t";
                return content;
            }

            void Writer(string content, int level = 0)
            {
                mainWriter.WriteLine(CreateTab(level) + content);
            }

            TypeString typeString = new TypeString();
            typeString.typeName = $"{targetType.typeName}Template";
            typeString.typeNameSpace = ConstData.TemplateNamespace;
            typeString.assemblyName = ConstData.TemplateAssembly;

            return typeString;
        }

        public static void AlterCSharpTemplateBase(TemplateData templateData, string path)
        {
            string fullPath = $"{Application.dataPath}/{path}/{templateData.temlateType.typeName}.cs";
            List<string> contents = File.ReadAllLines(fullPath).ToList();

            List<string> addLine = new List<string>();

            int lineAmount = contents.Count;

            for (int i = 0; i < lineAmount; i++)
            {
                string line = contents[i];
                if (line.Contains("using")) { addLine.Add(line); }
                else if (line.Contains("namespace")) break;
            }

            Writer($"namespace {ConstData.TemplateNamespace}");
            Writer("{");

            string inheritContent = "UnityEngine.MonoBehaviour";
            if (templateData.temlateBaseType.IsEmpty() == false) inheritContent = $"{templateData.temlateBaseType.GetVisitString()}";
            Writer($"public class {templateData.temlateType.typeName} : {inheritContent}", 1);
            Writer("{", 1);

            Writer($"public {templateData.targetType.GetVisitString()} templateValue;", 2);

            int startLine = -1;
            int endLine = -1;

            for (int i = 0; i < lineAmount; i++)
            {
                string line = contents[i];
                if (startLine == -1)
                {
                    if (line.Contains($"#region {ConstData.TemplateRegionName}"))
                    {
                        startLine = i;
                        addLine.Add(line);
                    }
                }
                else
                {
                    if (endLine == -1)
                    {
                        addLine.Add(line);
                        if (line.Contains("#endregion")) endLine = i;
                    }
                    else { break; }
                }
            }

            Writer("}", 1);

            Writer("}");

            StreamWriter mainWriter = new StreamWriter(fullPath, false);
            int addAmount = addLine.Count;
            for (int i = 0; i < addAmount; i++) mainWriter.WriteLine(addLine[i]);
            mainWriter.Close();

            string CreateTab(int level)
            {
                string content = "";
                for (int i = 0; i < level; i++) content += "\t";
                return content;
            }

            void Writer(string content, int level = 0)
            {
                addLine.Add(CreateTab(level) + content);
            }
        }
    }
}