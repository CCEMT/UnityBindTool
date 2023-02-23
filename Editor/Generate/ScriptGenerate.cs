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
        static void AddBindComponents(GenerateData generateData)
        {
            GameObject root = generateData.objectInfo.rootBindInfo.GetObject();
            BindComponents bindComponents = root.GetComponent<BindComponents>();
            if (bindComponents == null)
            {
                bindComponents = root.AddComponent<BindComponents>();
                int componentAmount = generateData.objectInfo.gameObjectBindInfoList.Count;
                for (int i = 0; i < componentAmount; i++)
                {
                    ComponentBindInfo componentBindInfo = generateData.objectInfo.gameObjectBindInfoList[i];
                    bindComponents.bindComponentList.Add(componentBindInfo.GetValue());
                }

                int dataAmount = generateData.objectInfo.dataBindInfoList.Count;
                for (int i = 0; i < dataAmount; i++)
                {
                    DataBindInfo dataBindInfo = generateData.objectInfo.dataBindInfoList[i];
                    bindComponents.bindComponentList.Add(dataBindInfo.bindObject);
                }
            }
        }

        public static void CSharpWrite(CommonSettingData commonSettingData, GenerateData generateData, string scriptPath)
        {
            ScriptSetting selectSettion = commonSettingData.selectScriptSetting;
            AddBindComponents(generateData);

            //是否创建新脚本
            if (selectSettion.isGenerateNew)
            {
                string scriptFile = scriptPath + $"{generateData.newScriptName}.cs";

                //是否将生成为子文件
                if (selectSettion.isGeneratePartial)
                {
                    //创建主文件
                    CreatePartialMainFile(scriptFile, commonSettingData, generateData);

                    string scriptPartialFile = scriptPath + $"{generateData.newScriptName}.{selectSettion.partialName}.cs";
                    //创建Partial并写入数据
                    if (File.Exists(scriptPartialFile))
                    {
                        if (selectSettion.isSavaOldScript) SavaOldScript(scriptPartialFile, commonSettingData, generateData);
                        File.Delete(scriptPartialFile);
                    }
                    GenerateCSharpScript(commonSettingData, generateData, scriptPartialFile, generateData.newScriptName, true, true);
                }
                else
                {
                    //创建文件并写入数据
                    if (File.Exists(scriptFile) == false)
                    {
                        bool isExist = TypeString.IsExist(generateData.newScriptName, selectSettion.useNamespace, ConstData.DefaultAssembly);
                        if (isExist)
                        {
                            Debug.LogError("生成为新文件时错误 类型冲突:" + generateData.newScriptName);
                            generateData.isStartBuild = false;
                            return;
                        }
                        GenerateCSharpScript(commonSettingData, generateData, scriptFile, generateData.newScriptName, true, false);
                    }
                    else
                    {
                        //打开文件并更改数据
                        if (selectSettion.isSavaOldScript) SavaOldScript(scriptFile, commonSettingData, generateData);
                        OpenCSharpFileAlterData(commonSettingData, generateData, scriptFile);
                    }
                }
            }
            else
            {
                if (selectSettion.isGeneratePartial)
                {
                    string scriptPartialFile = scriptPath + $"{generateData.newScriptName}.{selectSettion.partialName}.cs";
                    //检查是否包含Partial关键字
                    //创建Partial并写入数据

                    if (File.Exists(scriptPartialFile))
                    {
                        if (selectSettion.isSavaOldScript) SavaOldScript(scriptPartialFile, commonSettingData, generateData);
                        File.Delete(scriptPartialFile);
                    }
                    GenerateCSharpScript(commonSettingData, generateData, scriptPartialFile, generateData.objectInfo.typeString.typeName, true, true);
                }
                else
                {
                    string scriptPartialFile = scriptPath + $"{generateData.newScriptName}.cs";
                    //打开文件并更改数据
                    if (selectSettion.isSavaOldScript) SavaOldScript(scriptPartialFile, commonSettingData, generateData);
                    OpenCSharpFileAlterData(commonSettingData, generateData, scriptPartialFile);
                }
            }
        }

        static void CreatePartialMainFile(string scriptFile, CommonSettingData commonSettingData, GenerateData generateData)
        {
            ScriptSetting selectSettion = commonSettingData.selectScriptSetting;
            if (File.Exists(scriptFile) == false)
            {
                bool isExist = TypeString.IsExist(generateData.newScriptName, selectSettion.useNamespace, selectSettion.createScriptAssembly);
                if (isExist)
                {
                    Debug.LogError("生成为子文件时 在生成空的主文件时错误 类型冲突:" + generateData.newScriptName);
                    generateData.isStartBuild = false;
                    return;
                }
                //创建空主文件
                GenerateCSharpScript(commonSettingData, generateData, scriptFile, generateData.newScriptName, false, true);
            }
            else
            {
                //修改主文件
                //检查是否包含Partial关键字
                string content = File.ReadAllText(scriptFile);

                string filtration = content.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                if (filtration.Contains($"partialclass{generateData.newScriptName}") == false)
                {
                    //如果包含则 将 class [ClassName]替换为partial class [ClassName]
                    //Error 该操作可能会出现问题 将其保存以便找回原数据
                    if (selectSettion.isSavaOldScript) SavaOldScript(scriptFile, commonSettingData, generateData);

                    int index = content.IndexOf(generateData.newScriptName, StringComparison.Ordinal);
                    string interval = string.Empty;
                    for (int i = index - 1; i >= 0; i--)
                    {
                        if (content[i].Equals('s')) { break; }
                        else { interval += content[i].ToString(); }
                    }
                    content = content.Replace($"class{interval}{generateData.newScriptName}", $"partial class {generateData.newScriptName}");
                }
                StreamWriter sw = new StreamWriter(scriptFile, false);
                sw.WriteLine(content);
                sw.Close();

                if (content.Contains($"#region {ConstData.DefaultName}"))
                {
                    if (selectSettion.isSavaOldScript) SavaOldScript(scriptFile, commonSettingData, generateData);
                    List<string> contents = File.ReadAllLines(scriptFile).ToList();
                    //包含自动生成数据
                    int startLine = -1;
                    int endLine = -1;
                    int amount = 0;
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
                            if (line.Contains("#region")) { amount++; }
                            if (line.Contains("#endregion"))
                            {
                                if (amount == 0)
                                {
                                    endLine = i;
                                    break;
                                }
                                amount--;
                            }
                        }
                    }

                    if (startLine != -1 && endLine != -1)
                    {
                        //删除数据
                        int deleteAmount = endLine - startLine;
                        for (int i = 0; i < deleteAmount + 1; i++) contents.RemoveAt(startLine);

                        StreamWriter delectWriter = new StreamWriter(scriptFile, false);
                        int targetAmount = contents.Count;
                        for (int i = 0; i < targetAmount; i++) delectWriter.WriteLine(contents[i]);
                        delectWriter.Close();
                    }
                }
            }
        }

        static void OpenCSharpFileAlterData(CommonSettingData commonSettingData, GenerateData generateData, string path)
        {
            ScriptSetting selectSettion = commonSettingData.selectScriptSetting;

            generateData.addTypeString = generateData.objectInfo.typeString;
            if (generateData.bindObject.GetComponent(generateData.addTypeString.ToType()) == null) { generateData.bindObject.AddComponent(generateData.addTypeString.ToType()); }

            if (selectSettion.isSavaOldScript) SavaOldScript(path, commonSettingData, generateData);

            bool isSpecifyNamespace = File.ReadAllText(path).Contains("namespace");

            //找到数据
            List<string> contents = File.ReadAllLines(path).ToList();

            //通过#region来定位
            int startLine = -1;
            int endLine = -1;
            int amount = 0;
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
                    if (line.Contains("#region")) { amount++; }
                    if (line.Contains("#endregion"))
                    {
                        if (amount == 0)
                        {
                            endLine = i;
                            break;
                        }
                        amount--;
                    }
                }
            }

            //通过#region来定位时定位失败
            if (startLine == -1 || endLine == -1)
            {
                //通过类来定位开始行
                if (ClassLocationLine(ref startLine, contents, generateData) == false) { return; }
            }
            else
            {
                //删除数据
                int deleteAmount = endLine - startLine;
                for (int i = 0; i < deleteAmount + 1; i++) contents.RemoveAt(startLine);
            }

            generateData.objectInfo.typeString = generateData.addTypeString;

            //在开始行插入数据
            List<string> addData = GenerateCSharpData.Generate(commonSettingData, generateData, isSpecifyNamespace);
            int addAmount = addData.Count;
            for (int i = 0; i < addAmount; i++) contents.Insert(startLine + i, addData[i]);

            StreamWriter sw = new StreamWriter(path, false);
            int targetAmount = contents.Count;
            for (int i = 0; i < targetAmount; i++) sw.WriteLine(contents[i]);
            sw.Close();
        }

        static bool ClassLocationLine(ref int startLine, List<string> contents, GenerateData generateData)
        {
            //未找到数据
            //通过类来定位
            int lineAmount = contents.Count;
            startLine = -1;
            int classLine = -1;
            for (int i = 0; i < lineAmount; i++)
            {
                string line = contents[i];
                if (classLine == -1)
                {
                    if (line.Contains(generateData.addTypeString.typeName))
                    {
                        classLine = i;
                        int index = line.IndexOf(generateData.addTypeString.typeName, StringComparison.Ordinal);
                        index += generateData.addTypeString.typeName.Length;
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
                    if (startLine == -1)
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
                    else { break; }
                }
            }

            if (startLine == -1)
            {
                generateData.isStartBuild = false;
                Debug.Log("文件格式错误");
                return false;
            }

            return true;
        }

        static bool FileIsType(string filePaht, string typeName)
        {
            string content = File.ReadAllText(filePaht);
            string filtration = content.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
            if (filtration.Contains($"class{typeName}")) { return true; }
            return false;
        }

        static void SavaOldScript(string path, CommonSettingData commonSettingData, GenerateData generateData)
        {
            ScriptSetting selectSettion = commonSettingData.selectScriptSetting;

            string savaPath = Application.dataPath + "/" + selectSettion.savaOldScriptPath + "/";

            string directoryName = "";
            if (generateData.addTypeString.IsEmpty() == false) { directoryName = generateData.addTypeString.typeName + "/"; }
            else { directoryName = generateData.newScriptName + "/"; }

            string directoryPath = savaPath + directoryName;

            if (Directory.Exists(directoryPath) == false) Directory.CreateDirectory(directoryPath);
            DirectoryInfo direction = new DirectoryInfo(directoryPath);

            string savaText = File.ReadAllText(path);

            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            string fileName = "";
            if (generateData.addTypeString.IsEmpty() == false) { fileName = generateData.addTypeString.typeName + ".txt"; }
            else { fileName = generateData.newScriptName + "/"; }

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
                string tempName = generateData.addTypeString.typeName + number + ".txt";
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

        static void GenerateCSharpScript(CommonSettingData commonSettingData, GenerateData generateData, string path, string className, bool isGenerateData, bool isPartial)
        {
            ScriptSetting selectSettion = commonSettingData.selectScriptSetting;

            generateData.objectInfo.typeString.typeName = generateData.newScriptName;
            generateData.objectInfo.typeString.typeNameSpace = selectSettion.useNamespace;
            generateData.objectInfo.typeString.assemblyName = selectSettion.createScriptAssembly;

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
                List<string> addList = GenerateCSharpData.Generate(commonSettingData, generateData, selectSettion.isSpecifyNamespace);
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
            if (Directory.Exists($"{Application.dataPath}/{path}") == false) { Debug.Log("模板脚本路径错误，请检查设置Setting-ScriptSetting-MethodGenerate-模板脚本保存路径"); }
            StreamWriter mainWriter = File.CreateText(fullPath);

            Writer("///该脚本为模板方法");
            Writer("///注意：");
            Writer($"///需要生成的方法写入#region-{ConstData.TemplateRegionName}");
            Writer("///生成方法使用到的类型必须为[命名空间].[类型名]，如果使用using引用命名空间生成时可能会生成失败");

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

            addLine.Add("///该脚本为模板方法");
            addLine.Add("///注意：");
            addLine.Add($"///需要生成的方法写入#region-{ConstData.TemplateRegionName}");
            addLine.Add("///生成方法使用到的类型必须为[命名空间].[类型名]，如果使用using引用命名空间生成时可能会生成失败");

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