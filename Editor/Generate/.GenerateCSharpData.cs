using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BindTool
{
    public class GenerateCSharpData
    {
        class BaseData
        {
            public string variableName = "";
            public string propertyName = "";
        }

        class ComponentNameData : BaseData
        {
            public ComponentBindInfo componentBindInfo;
        }

        class DataName : BaseData
        {
            public DataBindInfo dataBindInfo;
        }

        public static List<string> Generate(MainSetting mainSetting, GenerateData generateData)
        {
            List<string> generateList = new List<string>();

            ScriptSetting selectSettion = mainSetting.selectScriptSetting;

            Writer($"#region {CommonConst.DefaultName}", 1);

            Writer($"///#region {CommonConst.DefaultName} #endregion里的内容为自动生成，请勿去修改它", 1);

            List<string> nameList = new List<string>();
            List<ComponentNameData> coomponentNameList = new List<ComponentNameData>();
            List<DataName> dataNameList = new List<DataName>();
            Dictionary<string, int> variableNameAmountDict = new Dictionary<string, int>();
            Dictionary<string, int> propertyNameAmountDict = new Dictionary<string, int>();
            Dictionary<string, int> methodNameAmountDict = new Dictionary<string, int>();
            List<string> methodNameList = new List<string>();

            int componentBindAmount = generateData.objectInfo.gameObjectBindInfoList.Count;
            for (int i = 0; i < componentBindAmount; i++)
            {
                ComponentBindInfo componentInfo = generateData.objectInfo.gameObjectBindInfoList[i];
                string variableName = componentInfo.name;
                string propertyName = NameHelper.SetPropertyName(variableName, mainSetting.selectCreateNameSetting);
                propertyName = NameHelper.NameSettingByName(propertyName, componentInfo, selectSettion.propertyNameSetting);
                ComponentNameData componentNameData = new ComponentNameData();
                componentNameData.componentBindInfo = componentInfo;
                AddVariable(componentNameData, variableName, propertyName, componentInfo.GetTypeString());
                coomponentNameList.Add(componentNameData);
            }

            int dataBindAmount = generateData.objectInfo.dataBindInfoList.Count;
            for (int i = 0; i < dataBindAmount; i++)
            {
                DataBindInfo dataInfo = generateData.objectInfo.dataBindInfoList[i];
                string variableName = dataInfo.name;
                string propertyName = NameHelper.SetPropertyName(variableName, mainSetting.selectCreateNameSetting);
                propertyName = NameHelper.NameSettingByName(propertyName, dataInfo, selectSettion.propertyNameSetting);
                DataName dataName = new DataName();
                dataName.dataBindInfo = dataInfo;
                AddVariable(dataName, variableName, propertyName, dataInfo.typeString);
                dataNameList.Add(dataName);
            }

            generateList.Add("");

            string bindMethodName = CommonConst.DefaultBindMethodName;
            generateData.getBindDataMethodName = bindMethodName;
            Writer($"public void {bindMethodName}()", 1);
            Writer("{", 1);

            int componentAmount = coomponentNameList.Count;
            for (int i = 0; i < componentAmount; i++)
            {
                ComponentNameData data = coomponentNameList[i];
                TypeString type = data.componentBindInfo.GetTypeString();
                if (data.componentBindInfo.instanceObject == generateData.bindObject)
                {
                    if (type.ToType() == typeof(GameObject)) { Writer($"{data.variableName} = gameObject;", 2); }
                    else { Writer($"{data.variableName} = GetComponent<{type.GetVisitString()}>();", 2); }
                }
                else
                {
                    if (type.ToType() == typeof(GameObject))
                    {
                        string contnet =
                            $"{data.variableName} = transform.Find(\"{CommonTools.GetWholePath(data.componentBindInfo.instanceObject.transform, generateData.bindObject)}\").gameObject;";
                        Writer(contnet, 2);
                    }
                    else
                    {
                        string contnet =
                            $"{data.variableName} = transform.Find(\"{CommonTools.GetWholePath(data.componentBindInfo.instanceObject.transform, generateData.bindObject)}\").GetComponent<{type.GetVisitString()}>();";
                        Writer(contnet, 2);
                    }
                }
            }
            int dataAmount = dataNameList.Count;
            if (dataAmount > 0)
            {
                Writer("#if UNITY_EDITOR");
                for (int i = 0; i < dataAmount; i++)
                {
                    DataName data = dataNameList[i];
                    TypeString type = data.dataBindInfo.typeString;
                    string contnet = $"{data.variableName} = UnityEditor.AssetDatabase.LoadAssetAtPath<{type.GetVisitString()}>(\"{AssetDatabase.GetAssetPath(data.dataBindInfo.bindObject)}\");";
                    Writer(contnet, 2);
                }
                Writer("#endif");
            }

            Writer("}", 1);
            generateList.Add("");
            Writer($"#region Method", 1);

            int variableAmount = selectSettion.variableList.Count;
            int componentNameAmount = coomponentNameList.Count;
            int dataNameAmount = dataNameList.Count;
            for (int i = 0; i < variableAmount; i++)
            {
                VariableData variableData = selectSettion.variableList[i];
                List<ComponentNameData> addComponentMethodList = new List<ComponentNameData>();
                List<DataName> addDataMethodList = new List<DataName>();

                for (int j = 0; j < componentNameAmount; j++)
                {
                    ComponentNameData data = coomponentNameList[j];
                    TypeString type = data.componentBindInfo.GetTypeString();
                    if (type.Equals(variableData.targetType)) addComponentMethodList.Add(data);
                }
                for (int j = 0; j < dataNameAmount; j++)
                {
                    DataName data = dataNameList[j];
                    if (data.dataBindInfo.typeString.Equals(variableData.targetType)) addDataMethodList.Add(data);
                }

                int addComponentAmount = addComponentMethodList.Count;
                for (int j = 0; j < addComponentAmount; j++)
                {
                    ComponentNameData componentNameData = addComponentMethodList[j];
                    string methodName = NameHelper.NameSettingByName(componentNameData.componentBindInfo, selectSettion.methodNameSetting);
                    bool isCantainsComponentMethod = methodNameAmountDict.ContainsKey(methodName);
                    if (isCantainsComponentMethod == false) { methodNameAmountDict.Add(methodName, 1); }

                    if (isCantainsComponentMethod || nameList.Contains(methodName))
                    {
                        bool isCanName = false;
                        while (isCanName == false)
                        {
                            methodNameAmountDict[methodName]++;
                            switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                            {
                                case RepetitionNameDispose.AddNumber:
                                    string targetName = methodName + methodNameAmountDict[methodName].ToString();

                                    if (methodNameList.Contains(targetName)) continue;
                                    ComponentNameData findNameData = coomponentNameList.Find((findData) => {
                                        if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                        return false;
                                    });
                                    if (findNameData == null && nameList.Contains(targetName) == false)
                                    {
                                        methodName = targetName;
                                        isCanName = true;
                                    }
                                    break;
                                case RepetitionNameDispose.None:
                                default:
                                    isCanName = true;
                                    break;
                            }
                        }
                    }

                    nameList.Add(methodName);
                    methodNameList.Add(methodName);

                    Writer($"public  {variableData.variableType.GetVisitString()} Get{methodName}()", 1);
                    Writer("{", 1);
                    Writer($"return {componentNameData.variableName}.{variableData.varialbleName};", 2);
                    Writer("}", 1);

                    Writer($"public void Set{methodName}({variableData.variableType.GetVisitString()} value)", 1);
                    Writer("{", 1);
                    Writer($"{componentNameData.variableName}.{variableData.varialbleName}=value;", 2);
                    Writer("}", 1);
                }

                int addDataAmount = addDataMethodList.Count;
                for (int j = 0; j < addDataAmount; j++)
                {
                    DataName dataName = addDataMethodList[j];
                    string methodName = NameHelper.NameSettingByName(dataName.dataBindInfo, selectSettion.methodNameSetting);
                    bool isCantainsDataMethod = methodNameAmountDict.ContainsKey(methodName);
                    if (isCantainsDataMethod == false) { methodNameAmountDict.Add(methodName, 1); }

                    if (isCantainsDataMethod || nameList.Contains(methodName))
                    {
                        bool isCanName = false;
                        while (isCanName == false)
                            switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                            {
                                case RepetitionNameDispose.AddNumber:
                                    string targetName = methodName + methodNameAmountDict[methodName].ToString();

                                    if (methodNameList.Contains(targetName))
                                    {
                                        methodNameAmountDict[methodName]++;
                                        continue;
                                    }
                                    ComponentNameData findNameData = coomponentNameList.Find((findData) => {
                                        if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                        return false;
                                    });
                                    if (findNameData == null && nameList.Contains(targetName) == false)
                                    {
                                        methodName = targetName;
                                        isCanName = true;
                                    }
                                    else { methodNameAmountDict[methodName]++; }
                                    break;
                                case RepetitionNameDispose.None:
                                default:
                                    isCanName = true;
                                    break;
                            }
                    }

                    nameList.Add(methodName);
                    methodNameList.Add(methodName);

                    Writer($"public {variableData.variableType.GetVisitString()} Get{methodName}()", 1);
                    Writer("{", 1);
                    Writer($"return {dataName.variableName}.{variableData.varialbleName};", 2);
                    Writer("}", 1);

                    Writer($"public void Set{methodName}({variableData.variableType.GetVisitString()} value)", 1);
                    Writer("{", 1);
                    Writer($"{dataName.variableName}.{variableData.varialbleName}=value;", 2);
                    Writer("}", 1);
                }
            }

            string folderPath = AssetDatabase.GetAssetPath(selectSettion.templateScriptSavaFolderPath);
            folderPath = folderPath.Substring(6);

            int templateAmount = selectSettion.templateDataList.Count;
            for (int i = 0; i < templateAmount; i++)
            {
                TemplateData templateData = selectSettion.templateDataList[i];
                string fullPath = $"{Application.dataPath}/{folderPath}/{templateData.temlateType.typeName}.cs";
                List<string> contents = File.ReadAllLines(fullPath).ToList();
                List<string> addLine = new List<string>();

                int startLine = -1;
                int endLine = -1;

                int lineAmount = contents.Count;
                for (int j = 0; j < lineAmount; j++)
                {
                    string line = contents[j];
                    if (startLine == -1)
                    {
                        if (line.Contains($"#region {CommonConst.TemplateRegionName}")) startLine = j;
                    }
                    else
                    {
                        if (endLine == -1)
                        {
                            if (line.Contains("#endregion")) { endLine = j; }
                            else { addLine.Add(line); }
                        }
                        else { break; }
                    }
                }

                List<ComponentNameData> addComponentMethodList = new List<ComponentNameData>();
                List<DataName> addDataMethodList = new List<DataName>();

                for (int j = 0; j < componentNameAmount; j++)
                {
                    ComponentNameData data = coomponentNameList[j];
                    TypeString type = data.componentBindInfo.GetTypeString();
                    if (type.Equals(templateData.targetType)) addComponentMethodList.Add(data);
                }
                for (int j = 0; j < dataNameAmount; j++)
                {
                    DataName data = dataNameList[j];
                    if (data.dataBindInfo.typeString.Equals(templateData.targetType)) addDataMethodList.Add(data);
                }

                int addComponentMethodAmount = addComponentMethodList.Count;
                for (int j = 0; j < addComponentMethodAmount; j++)
                {
                    ComponentNameData componentNameData = addComponentMethodList[j];
                    string methodName = NameHelper.NameSettingByName(componentNameData.componentBindInfo, selectSettion.methodNameSetting);
                    TempMethod(templateData, addLine, methodName, componentNameData.variableName);
                }
                int addDataMethodAmount = addDataMethodList.Count;
                for (int j = 0; j < addDataMethodAmount; j++)
                {
                    DataName dataName = addDataMethodList[i];
                    string methodName = NameHelper.NameSettingByName(dataName.dataBindInfo, selectSettion.methodNameSetting);
                    TempMethod(templateData, addLine, methodName, dataName.variableName);
                }
            }

            Writer("#endregion", 1);
            generateList.Add("");
            Writer("#endregion", 1);

            return generateList;

            string CreateTab(int level)
            {
                if (mainSetting.selectScriptSetting.isSpecifyNamespace) level++;
                string content = "";
                for (int i = 0; i < level; i++) content += "\t";
                return content;
            }

            void Writer(string content, int level = -1)
            {
                generateList.Add(CreateTab(level) + content);
            }

            void TempMethod(TemplateData templateData, List<string> addLine, string methodName, string variableName)
            {
                MemberInfo[] members = templateData.temlateType.ToType().GetMembers();
                int memberAmount = members.Length;

                int addLineAmount = addLine.Count;
                for (int i = 0; i < addLineAmount; i++)
                {
                    string line = addLine[i];
                    line = line.Replace($"templateValue", variableName);

                    for (int j = 0; j < memberAmount; j++)
                    {
                        MemberInfo memberInfo = members[j];
                        string fullName = methodName + memberInfo.Name;

                        if (line.Contains(memberInfo.Name))
                        {
                            bool isCantainsTemplateMethod = methodNameAmountDict.ContainsKey(fullName);
                            if (isCantainsTemplateMethod == false) { methodNameAmountDict.Add(fullName, 1); }

                            if (isCantainsTemplateMethod || nameList.Contains(fullName))
                            {
                                bool isCanName = false;
                                while (isCanName == false)
                                    switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                                    {
                                        case RepetitionNameDispose.AddNumber:
                                            string targetName = methodName + methodNameAmountDict[fullName].ToString() + memberInfo.Name;

                                            if (methodNameList.Contains(targetName))
                                            {
                                                methodNameAmountDict[fullName] += 1;
                                                continue;
                                            }
                                            ComponentNameData findNameData = coomponentNameList.Find((findData) => {
                                                if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                                return false;
                                            });
                                            if (findNameData == null && nameList.Contains(targetName) == false)
                                            {
                                                fullName = targetName;
                                                isCanName = true;
                                            }
                                            else { methodNameAmountDict[fullName] += 1; }
                                            break;
                                        case RepetitionNameDispose.None:
                                        default:
                                            isCanName = true;
                                            break;
                                    }
                            }

                            nameList.Add(fullName);

                            line = line.Replace(memberInfo.Name, fullName);
                            methodNameList.Add(fullName);
                            break;
                        }
                    }
                    Writer(line);
                }
            }

            void AddVariable(BaseData baseData, string variableName, string propertyName, TypeString typeString)
            {
                bool isCantainsVariable = variableNameAmountDict.ContainsKey(variableName);

                if (isCantainsVariable == false) { variableNameAmountDict.Add(variableName, 1); }

                if (isCantainsVariable || nameList.Contains(variableName))
                {
                    bool isCanName = false;
                    while (isCanName == false)
                    {
                        variableNameAmountDict[variableName]++;
                        switch (selectSettion.nameSetting.repetitionNameDispose)
                        {
                            case RepetitionNameDispose.AddNumber:
                                string targetName = variableName + variableNameAmountDict[variableName].ToString();

                                DataName findNameData = dataNameList.Find((findData) => {
                                    if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                    return false;
                                });
                                if (findNameData == null && nameList.Contains(targetName) == false)
                                {
                                    variableName = targetName;
                                    isCanName = true;
                                }
                                break;
                            case RepetitionNameDispose.None:
                            default:
                                isCanName = true;
                                break;
                        }
                    }
                }

                nameList.Add(variableName);
                baseData.variableName = variableName;

                string content = $"[BindTool.{nameof(AutoGenerate)}({nameof(AutoGenerateType.OriginalField)})][UnityEngine.SerializeField]{LabelHelper.GetVisitString(selectSettion.variableVisitType)} ";

                TypeString type = typeString;
                content += type.GetVisitString();
                content += $" {baseData.variableName};";
                Writer(content, 1);

                if (selectSettion.isAddProperty)
                {
                    bool isCantainsProperty = propertyNameAmountDict.ContainsKey(propertyName);
                    if (isCantainsProperty == false) { propertyNameAmountDict.Add(propertyName, 1); }

                    if (isCantainsProperty || nameList.Contains(propertyName))
                    {
                        bool isCanName = false;
                        while (isCanName == false)
                        {
                            propertyNameAmountDict[propertyName]++;
                            switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                            {
                                case RepetitionNameDispose.AddNumber:
                                    string targetName = propertyName + propertyNameAmountDict[propertyName].ToString();

                                    DataName findNameData = dataNameList.Find((findData) => {
                                        if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                        return false;
                                    });
                                    if (findNameData == null && nameList.Contains(targetName) == false)
                                    {
                                        propertyName = targetName;
                                        isCanName = true;
                                    }
                                    break;
                                case RepetitionNameDispose.None:
                                default:
                                    isCanName = true;
                                    break;
                            }
                        }
                    }

                    nameList.Add(propertyName);
                    baseData.propertyName = propertyName;

                    string propertyContent = $"{LabelHelper.GetVisitString(selectSettion.propertyVisitType)} ";
                    propertyContent += typeString.GetVisitString();
                    propertyContent += $" {propertyName}";

                    switch (selectSettion.propertyType)
                    {
                        case PropertyType.Set:
                            propertyContent += "{" + $"set=>{baseData.variableName}=value;" + "}";
                            break;
                        case PropertyType.Get:
                            propertyContent += "{" + $"get=>{baseData.variableName};" + "}";
                            break;
                        case PropertyType.SetAndGet:
                            propertyContent += "{" + $"get=>{baseData.variableName}; set=>{baseData.variableName}=value;" + "}";
                            break;
                    }
                    Writer(propertyContent, 1);
                }
                generateList.Add("");
            }
        }
    }
}