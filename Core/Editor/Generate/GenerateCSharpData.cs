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

        public static List<string> Generate(CommonSettingData commonSettingData, bool isSpecifyNamespace)
        {
            List<string> generateList = new List<string>();

            var selectSettion = commonSettingData.selectScriptSetting;
            var tempData = commonSettingData.tempGenerateData;

            Writer($"#region {ConstData.DefaultName}", 1);

            List<ComponentNameData> coomponentNameList = new List<ComponentNameData>();
            List<DataName> dataNameList = new List<DataName>();
            Dictionary<string, int> variableNameAmountDict = new Dictionary<string, int>();
            Dictionary<string, int> propertyNameAmountDict = new Dictionary<string, int>();
            Dictionary<string, int> methodNameAmountDict = new Dictionary<string, int>();
            List<string> methodNameList = new List<string>();

            int componentBindAmount = tempData.objectInfo.gameObjectBindInfoList.Count;
            for (int i = 0; i < componentBindAmount; i++)
            {
                ComponentBindInfo data = tempData.objectInfo.gameObjectBindInfoList[i];
                string propertyName = CommonTools.NameSettingByName(data, selectSettion.propertyNameSetting);
                string variableName = CommonTools.NameSettingByName(data, selectSettion.nameSetting);
                ComponentNameData componentNameData = new ComponentNameData();
                componentNameData.componentBindInfo = data;
                AddVariable(componentNameData, variableName, propertyName, data.GetTypeString());
                coomponentNameList.Add(componentNameData);
            }

            int dataBindAmount = tempData.objectInfo.dataBindInfoList.Count;
            for (int i = 0; i < dataBindAmount; i++)
            {
                DataBindInfo dataBindInfo = tempData.objectInfo.dataBindInfoList[i];
                string variableName = CommonTools.NameSettingByName(dataBindInfo, selectSettion.nameSetting);
                string propertyName = CommonTools.NameSettingByName(dataBindInfo, selectSettion.propertyNameSetting);
                DataName dataName = new DataName();
                dataName.dataBindInfo = dataBindInfo;
                AddVariable(dataName, variableName, propertyName, dataBindInfo.typeString);
                dataNameList.Add(dataName);
            }

            generateList.Add("");

            string bindMethodName = ConstData.DefaultBindMethodName;
            tempData.getBindDataMethodName = bindMethodName;
            Writer($"public  void {bindMethodName}()", 1);
            Writer("{", 1);

            int componentAmount = coomponentNameList.Count;
            for (int i = 0; i < componentAmount; i++)
            {
                var data = coomponentNameList[i];
                TypeString type = data.componentBindInfo.GetTypeString();
                if (data.componentBindInfo.instanceObject == tempData.bindObject)
                {
                    if (type.ToType() == typeof(GameObject)) { Writer($"{data.variableName} = gameObject;", 2); }
                    else { Writer($"{data.variableName} = GetComponent<{type.GetVisitString()}>();", 2); }
                }
                else
                {
                    if (type.ToType() == typeof(GameObject))
                    {
                        string contnet =
                            $"{data.variableName} = transform.Find(\"{CommonTools.GetWholePath(data.componentBindInfo.instanceObject.transform, tempData.bindObject)}\").gameObject;";
                        Writer(contnet, 2);
                    }
                    else
                    {
                        string contnet =
                            $"{data.variableName} = transform.Find(\"{CommonTools.GetWholePath(data.componentBindInfo.instanceObject.transform, tempData.bindObject)}\").GetComponent<{type.GetVisitString()}>();";
                        Writer(contnet, 2);
                    }
                }
            }

            Writer("#if UNITY_EDITOR");
            int dataAmount = dataNameList.Count;
            for (int i = 0; i < dataAmount; i++)
            {
                var data = dataNameList[i];
                TypeString type = data.dataBindInfo.typeString;
                string contnet = $"{data.variableName} = UnityEditor.AssetDatabase.LoadAssetAtPath<{type.GetVisitString()}>(\"{AssetDatabase.GetAssetPath(data.dataBindInfo.bindObject)}\");";
                Writer(contnet, 2);
            }
            Writer("#endif");

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
                    var data = coomponentNameList[j];
                    var type = data.componentBindInfo.GetTypeString();
                    if (type.Equals(variableData.targetType)) addComponentMethodList.Add(data);
                }
                for (int j = 0; j < dataNameAmount; j++)
                {
                    var data = dataNameList[j];
                    if (data.dataBindInfo.typeString.Equals(variableData.targetType)) addDataMethodList.Add(data);
                }

                int addComponentAmount = addComponentMethodList.Count;
                for (int j = 0; j < addComponentAmount; j++)
                {
                    ComponentNameData componentNameData = addComponentMethodList[j];
                    string methodName = CommonTools.NameSettingByName(componentNameData.componentBindInfo, selectSettion.methodNameSetting);
                    if (methodNameAmountDict.ContainsKey(methodName))
                    {
                        bool isCanName = false;
                        while (isCanName == false)
                        {
                            methodNameAmountDict[methodName]++;
                            switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                            {
                                case RepetitionNameDispose.AddNumber:
                                    var targetName = methodName + methodNameAmountDict[methodName].ToString();

                                    if (methodNameList.Contains(targetName)) continue;
                                    var findNameData = coomponentNameList.Find((findData) => {
                                        if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                        return false;
                                    });
                                    if (findNameData == null)
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
                    else { methodNameAmountDict.Add(methodName, 1); }

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
                    string methodName = CommonTools.NameSettingByName(dataName.dataBindInfo, selectSettion.methodNameSetting);
                    if (methodNameAmountDict.ContainsKey(methodName))
                    {
                        bool isCanName = false;
                        while (isCanName == false)
                        {
                            methodNameAmountDict[methodName]++;
                            switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                            {
                                case RepetitionNameDispose.AddNumber:
                                    var targetName = methodName + methodNameAmountDict[methodName].ToString();

                                    if (methodNameList.Contains(targetName)) continue;
                                    var findNameData = coomponentNameList.Find((findData) => {
                                        if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                        return false;
                                    });
                                    if (findNameData == null)
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
                    else { methodNameAmountDict.Add(methodName, 1); }

                    methodNameList.Add(methodName);

                    Writer($"public  {variableData.variableType.GetVisitString()} Get{methodName}()", 1);
                    Writer("{", 1);
                    Writer($"return {dataName.variableName}.{variableData.varialbleName};", 2);
                    Writer("}", 1);

                    Writer($"public void Set{methodName}({variableData.variableType.GetVisitString()} value)", 1);
                    Writer("{", 1);
                    Writer($"{dataName.variableName}.{variableData.varialbleName}=value;", 2);
                    Writer("}", 1);
                }
            }

            int templateAmount = selectSettion.templateDataList.Count;
            for (int i = 0; i < templateAmount; i++)
            {
                TemplateData templateData = selectSettion.templateDataList[i];
                string fullPath = $"{Application.dataPath}/{selectSettion.templateScriptSavaPath}/{templateData.temlateType.typeName}.cs";
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
                        if (line.Contains($"#region {ConstData.TemplateRegionName}")) startLine = j;
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
                    var data = coomponentNameList[j];
                    var type = data.componentBindInfo.GetTypeString();
                    if (type.Equals(templateData.targetType)) addComponentMethodList.Add(data);
                }
                for (int j = 0; j < dataNameAmount; j++)
                {
                    var data = dataNameList[j];
                    if (data.dataBindInfo.typeString.Equals(templateData.targetType)) addDataMethodList.Add(data);
                }

                int addComponentMethodAmount = addComponentMethodList.Count;
                for (int j = 0; j < addComponentMethodAmount; j++)
                {
                    ComponentNameData componentNameData = addComponentMethodList[j];
                    string methodName = CommonTools.NameSettingByName(componentNameData.componentBindInfo, selectSettion.methodNameSetting);
                    TempMethod(templateData, addLine, methodName, componentNameData.variableName);
                }
                int addDataMethodAmount = addDataMethodList.Count;
                for (int j = 0; j < addDataMethodAmount; j++)
                {
                    DataName dataName = addDataMethodList[i];
                    string methodName = CommonTools.NameSettingByName(dataName.dataBindInfo, selectSettion.methodNameSetting);
                    TempMethod(templateData, addLine, methodName, dataName.variableName);
                }
            }

            Writer("#endregion", 1);
            generateList.Add("");
            Writer("#endregion", 1);

            return generateList;

            string CreateTab(int level)
            {
                if (isSpecifyNamespace) level++;
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
                            if (methodNameAmountDict.ContainsKey(fullName))
                            {
                                bool isCanName = false;
                                while (isCanName == false)
                                    switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                                    {
                                        case RepetitionNameDispose.AddNumber:
                                            var targetName = methodName + methodNameAmountDict[fullName].ToString() + memberInfo.Name;

                                            if (methodNameList.Contains(targetName))
                                            {
                                                methodNameAmountDict[fullName] += 1;
                                                continue;
                                            }
                                            var findNameData = coomponentNameList.Find((findData) => {
                                                if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                                return false;
                                            });
                                            if (findNameData == null)
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
                            else { methodNameAmountDict.Add(fullName, 1); }

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
                if (variableNameAmountDict.ContainsKey(variableName))
                {
                    bool isCanName = false;
                    while (isCanName == false)
                    {
                        variableNameAmountDict[variableName]++;
                        switch (selectSettion.nameSetting.repetitionNameDispose)
                        {
                            case RepetitionNameDispose.AddNumber:
                                var targetName = variableName + variableNameAmountDict[variableName].ToString();

                                var findNameData = dataNameList.Find((findData) => {
                                    if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                    return false;
                                });
                                if (findNameData == null)
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
                else { variableNameAmountDict.Add(variableName, 1); }
                baseData.variableName = variableName;

                string content = $"[UnityEngine.SerializeField]{CommonTools.GetVisitString(selectSettion.variableVisitType)} ";

                TypeString type = typeString;
                content += type.GetVisitString();
                content += $" {baseData.variableName};";
                Writer(content, 1);

                if (selectSettion.isAddProperty)
                {
                    if (propertyNameAmountDict.ContainsKey(propertyName))
                    {
                        bool isCanName = false;
                        while (isCanName == false)
                        {
                            propertyNameAmountDict[propertyName]++;
                            switch (selectSettion.propertyNameSetting.repetitionNameDispose)
                            {
                                case RepetitionNameDispose.AddNumber:
                                    var targetName = propertyName + propertyNameAmountDict[propertyName].ToString();

                                    var findNameData = dataNameList.Find((findData) => {
                                        if (findData.variableName.Equals(targetName) || findData.propertyName.Equals(targetName)) return true;
                                        return false;
                                    });
                                    if (findNameData == null)
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
                    else { propertyNameAmountDict.Add(propertyName, 1); }

                    baseData.propertyName = propertyName;

                    string propertyContent = $"{CommonTools.GetVisitString(selectSettion.propertyVisitType)} ";
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