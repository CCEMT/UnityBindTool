#region Using

using System;
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

        public static void Build(CompositionSetting setting, GenerateData generateData)
        {
            CommonSetting commonSetting = setting.commonSetting;

            string path = Application.dataPath + "/" + commonSetting.createScriptPath + "/";
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSetting.isCreateScriptFolder)
            {
                path += $"{generateData.newScriptName}/";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            Debug.Log("脚本生成路径：" + path);

            BindComponentsHelper.AddBindComponent(generateData);

            if (commonSetting.isCreateScript)
            {
                IGenerator generator = GeneratorFactory.GetGenerator(GeneratorType.CSharp, setting, generateData);
                CSharpBuild(generator, path, generateData);
            }

            if (commonSetting.isCreateLua)
            {
                IGenerator generator = GeneratorFactory.GetGenerator(GeneratorType.Lua, setting, generateData);
                LuaBuild(generator, path, setting, generateData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            if (EditorPrefs.HasKey(CSharpBuildGenerateDataKey) == false) return;
            BindSetting bindSetting = BindSetting.Get();
            CommonSetting commonSetting = bindSetting.selectCompositionSetting.commonSetting;

            GenerateData generateData = JsonUtility.FromJson<GenerateData>(EditorPrefs.GetString(CSharpBuildGenerateDataKey));

            if (generateData == null) return;
            EditorPrefs.DeleteKey(CSharpBuildGenerateDataKey);

            GameObject bindObject = generateData.bindObject;
            if (bindObject == null) return;
            string path = Application.dataPath + "/" + commonSetting.createPrefabPath;

            //检查路径是否有效
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSetting.isCreatePrefabFolder)
            {
                path += $"/{bindObject.name}";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            path += $"/{bindObject.name}{CommonConst.PrefabFileSuffix}";

            BindComponents bindComponents = bindObject.GetComponent<BindComponents>();

            Type addType = generateData.objectInfo.typeString.ToType();
            Component component = null;
            if (addType != null)
            {
                component = generateData.bindObject.GetComponent(addType);
                if (component == null) component = generateData.bindObject.AddComponent(addType);
                bindComponents.targetType = component.GetType();

                MethodInfo method = addType.GetMethod(generateData.getBindDataMethodName, new Type[] { });
                method.Invoke(component, new object[] { });
            }
            else { Debug.Log("添加类型为空"); }

            if (commonSetting.isCreatePrefab)
            {
                //创建预制体
                if (File.Exists(path)) { path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)); }
                PrefabUtility.SaveAsPrefabAssetAndConnect(bindObject, path, InteractionMode.AutomatedAction);
                Debug.Log("Create Prefab Finish.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void LuaBuild(IGenerator generator, string scriptPath, CompositionSetting setting, GenerateData generateData)
        {
            CommonSetting commonSetting = setting.commonSetting;

            GameObject bindObject = generateData.bindObject;

            if (commonSetting.isCreatePrefab)
            {
                //创建预制体
                string path = Application.dataPath + "/" + commonSetting.createPrefabPath;
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