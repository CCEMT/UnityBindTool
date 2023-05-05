#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#endregion

namespace BindTool
{
    public static class BindBuild
    {
        public const string CSharpBuildGenerateDataKey = "CSharpBuildGenerateData";

        public static void Build(GameObject bindObject, ObjectInfo objectInfo, GeneratorType generatorType)
        {
            MainSetting mainSetting = MainSetting.Get();

            string path = Application.dataPath + "/" + mainSetting.createScriptPath + "/";
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            GenerateData generateData = new GenerateData();

            //保存临时数据
            generateData.newScriptName = mainSetting.newScriptName;
            generateData.mergeTypeString = mainSetting.mergeTypeString;
            generateData.bindObject = bindObject;
            generateData.objectInfo = objectInfo;
            generateData.isStartBuild = true;

            //创建文件夹
            if (mainSetting.isCreateScriptFolder)
            {
                path += $"{generateData.newScriptName}/";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            Debug.Log("脚本生成路径：" + path);

            if (mainSetting.isCustomBind == false)
            {
                generateData.objectInfo.gameObjectBindInfoList.Clear();
                Transform[] gameObjects = bindObject.GetComponentsInChildren<Transform>(true);
                List<ComponentBindInfo> componentBindInfoList = new List<ComponentBindInfo>();

                int amount = gameObjects.Length;
                for (int i = 0; i < amount; i++)
                {
                    Transform go = gameObjects[i];
                    int componentAmount = new ComponentBindInfo(go.gameObject).typeStrings.Length;
                    for (int j = 0; j < componentAmount; j++)
                    {
                        ComponentBindInfo info = new ComponentBindInfo(go.gameObject);
                        info.index = j;
                        componentBindInfoList.Add(info);
                        if (mainSetting.selectCreateNameSetting.isBindAutoGenerateName) info.name = CommonTools.GetNumberAlpha(info.instanceObject.name);
                    }
                }
                int infoAmount = componentBindInfoList.Count;
                for (int i = 0; i < infoAmount; i++)
                {
                    ComponentBindInfo info = componentBindInfoList[i];
                    if (generateData.objectInfo.gameObjectBindInfoList.Contains(info) == false) generateData.objectInfo.gameObjectBindInfoList.Add(info);
                }
            }

            BindComponentsHelper.AddBindComponent(generateData);
            IGenerator generator = GeneratorFactory.GetGenerator(generatorType, mainSetting, generateData);

            switch (generatorType)
            {
                case GeneratorType.CSharp:
                    CSharpBuild(generator, path, generateData);
                    break;
                case GeneratorType.Lua:
                    LuaBuild(generator, path, mainSetting, generateData);
                    break;
                default:
                    Debug.LogError("生成失败,没有对应的生成器");
                    break;
            }

            Debug.Log("Create ScriptSetting Finish.");
        }

        static void CSharpBuild(IGenerator generator, string path, GenerateData generateData)
        {
            generator.Write(path);
            EditorPrefs.SetString(CSharpBuildGenerateDataKey, JsonUtility.ToJson(generateData));
        }

        [DidReloadScripts]
        static void CSharpDispose()
        {
            if (EditorPrefs.HasKey(CSharpBuildGenerateDataKey) == false) { return; }
            MainSetting mainSetting = MainSetting.Get();
            GenerateData generateData = JsonUtility.FromJson<GenerateData>(EditorPrefs.GetString(CSharpBuildGenerateDataKey));

            if (generateData == null) return;
            EditorPrefs.DeleteKey(CSharpBuildGenerateDataKey);

            GameObject bindObject = generateData.bindObject;
            if (bindObject == null) return;
            string path = Application.dataPath + "/" + mainSetting.createPrefabPath;

            //检查路径是否有效
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (mainSetting.isCreatePrefabFolder)
            {
                path += $"/{bindObject.name}";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            path += $"/{bindObject.name}{CommonConst.PrefabFileSuffix}";

            BindComponents bindComponents = bindObject.GetComponent<BindComponents>();
            bindComponents.bindComponentList.Clear();
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

            Type addType = generateData.objectInfo.typeString.ToType();
            Component component = null;
            if (addType != null)
            {
                component = generateData.bindObject.GetComponent(addType);
                if (component == null) component = generateData.bindObject.AddComponent(addType);
                bindComponents.bindRoot = component;

                MethodInfo method = addType.GetMethod(generateData.getBindDataMethodName, new Type[] { });
                method.Invoke(component, new object[] { });
            }
            else { Debug.Log("添加类型为空"); }

            if (mainSetting.isCreatePrefab)
            {
                //创建预制体
                if (File.Exists(path)) { path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)); }
                PrefabUtility.SaveAsPrefabAssetAndConnect(bindObject, path, InteractionMode.AutomatedAction);
                Debug.Log("Create Prefab Finish.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void LuaBuild(IGenerator generator, string scriptPath, MainSetting setting, GenerateData generateData)
        {
            GameObject bindObject = generateData.bindObject;

            BindComponents bindComponents = bindObject.GetComponent<BindComponents>();
            bindComponents.bindComponentList.Clear();
            int componentAmount = generateData.objectInfo.gameObjectBindInfoList.Count;
            for (int i = 0; i < componentAmount; i++)
            {
                ComponentBindInfo componentBindInfo = generateData.objectInfo.gameObjectBindInfoList[i];
                bindComponents.bindComponentList.Add(componentBindInfo.GetValue());
            }

            if (setting.isCreatePrefab)
            {
                //创建预制体
                string path = Application.dataPath + "/" + setting.createPrefabPath;
                if (Directory.Exists(path) == false)
                {
                    Debug.LogError($"{path} 不是有效路径！");
                    return;
                }
                path += $"/{bindObject.name}{CommonConst.PrefabFileSuffix}";

                if (File.Exists(path)) { path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)); }
                PrefabUtility.SaveAsPrefabAssetAndConnect(bindObject, path, InteractionMode.AutomatedAction);
                Debug.Log("Create Prefab Finish.");
            }

            generator.Write(scriptPath);
        }
    }
}