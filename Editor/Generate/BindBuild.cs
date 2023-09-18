using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityBindTool
{
    public static class BindBuild
    {
        public const string CSharpBuildGenerateDataKey = "CSharpBuildGenerateData";

        public static void Build(CompositionSetting setting, GenerateData generateData)
        {
            CommonSetting commonSetting = setting.commonSetting;

            BindComponentsHelper.AddBindComponent(generateData);

            if (commonSetting.isCreateScript) CreateCSharp(generateData, setting, commonSetting);
            if (commonSetting.isCreateLua) CreateLua(generateData, setting, commonSetting);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Build Finish.");
        }

        static void CreateCSharp(GenerateData generateData, CompositionSetting setting, CommonSetting commonSetting)
        {
            string csharpPath = Application.dataPath + "/" + commonSetting.createScriptPath + "/";

            if (Directory.Exists(csharpPath) == false)
            {
                Debug.LogError($"创建C#脚本错误 路径：{csharpPath} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSetting.isCreateScriptFolder)
            {
                csharpPath += $"{generateData.newScriptName}/";
                if (Directory.Exists(csharpPath) == false) Directory.CreateDirectory(csharpPath);
            }

            Debug.Log("CSharp脚本生成路径：" + csharpPath);

            IGenerator generator = GeneratorFactory.GetGenerator(GeneratorType.CSharp, setting, generateData);
            CSharpBuild(generator, csharpPath, generateData);
        }

        static void CSharpBuild(IGenerator generator, string path, GenerateData generateData)
        {
            generator.Write(path);
            EditorPrefs.SetString(CSharpBuildGenerateDataKey, JsonUtility.ToJson(generateData));
        }

        [DidReloadScripts]
        static void CSharpDispose()
        {
            if (EditorPrefs.HasKey(CSharpBuildGenerateDataKey) == false) return;
            BindSetting bindSetting = BindSetting.Get();
            CommonSetting commonSetting = bindSetting.selectCompositionSetting.commonSetting;

            GenerateData generateData = JsonUtility.FromJson<GenerateData>(EditorPrefs.GetString(CSharpBuildGenerateDataKey));

            if (generateData == null) return;
            EditorPrefs.DeleteKey(CSharpBuildGenerateDataKey);

            GameObject bindObject = generateData.bindObject;
            if (bindObject == null) return;

            if (commonSetting.isCreatePrefab) CreatePrefab(bindObject, commonSetting);

            BindComponents bindComponents = bindObject.GetComponent<BindComponents>();

            Type addType = generateData.objectInfo.typeString.ToType();
            Component component = null;
            if (addType != null)
            {
                component = generateData.bindObject.GetComponent(addType);
                if (component == null) component = generateData.bindObject.AddComponent(addType);
                bindComponents.targetType = component.GetType();

                MethodInfo method = addType.GetMethod(generateData.getBindDataMethodName, new[] {typeof(BindComponents)});
                method.Invoke(component, new object[] {bindComponents});
            }
            else { Debug.Log("添加类型为空"); }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void CreateLua(GenerateData generateData, CompositionSetting setting, CommonSetting commonSetting)
        {
            string luaPath = Application.dataPath + "/" + commonSetting.createLuaPath + "/";
            if (Directory.Exists(luaPath) == false)
            {
                Debug.LogError($"创建Lua脚本错误 路径{luaPath} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSetting.isCreateLuaFolder)
            {
                luaPath += $"{generateData.newScriptName}/";
                if (Directory.Exists(luaPath) == false) Directory.CreateDirectory(luaPath);
            }

            Debug.Log("Lua脚本生成路径：" + luaPath);

            IGenerator generator = GeneratorFactory.GetGenerator(GeneratorType.Lua, setting, generateData);
            LuaBuild(generator, luaPath, setting, generateData);
        }

        static void LuaBuild(IGenerator generator, string scriptPath, CompositionSetting setting, GenerateData generateData)
        {
            CommonSetting commonSetting = setting.commonSetting;

            GameObject bindObject = generateData.bindObject;

            if (commonSetting.isCreatePrefab) CreatePrefab(bindObject, commonSetting);

            generator.Write(scriptPath);
        }

        static void CreatePrefab(GameObject bindObject, CommonSetting commonSetting)
        {
            //创建预制体
            string path = Application.dataPath + "/" + commonSetting.createPrefabPath;
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"创建预制体错误！路径：{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSetting.isCreatePrefabFolder)
            {
                path += $"/{bindObject.name}";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            path += $"/{bindObject.name}{CommonConst.PrefabFileSuffix}";

            path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));

            if (commonSetting.isDetachPrefab) PrefabUtility.SaveAsPrefabAsset(bindObject, path);
            else PrefabUtility.SaveAsPrefabAssetAndConnect(bindObject, path, InteractionMode.AutomatedAction);
            Debug.Log("Create Prefab Finish.");
        }
    }
}