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
    public class BindBuild
    {
        public static void Build(GameObject bindObject, ObjectInfo objectInfo)
        {
            CommonSettingData commonSettingData = CommonTools.GetCommonSettingData();

            string path = Application.dataPath + "/" + commonSettingData.createScriptPath + "/";
            if (Directory.Exists(path) == false) {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            GenerateData generateData = CommonTools.CreateGenerateData();

            //保存临时数据
            generateData.newScriptName = commonSettingData.newScriptName;
            generateData.addTypeString = commonSettingData.addTypeString;
            generateData.bindObject = bindObject;
            generateData.objectInfo = objectInfo;
            generateData.isStartBuild = true;

            //创建文件夹
            if (commonSettingData.isCreateScriptFolder) {
                path += $"{generateData.newScriptName}/";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            Debug.Log("脚本生成路径：" + path);
            if (commonSettingData.isCustomBind) { ScriptGenerate.CSharpWrite(commonSettingData, generateData, path); }
            else {
                List<ComponentBindInfo> oldBindDataList = objectInfo.gameObjectBindInfoList;

                objectInfo.gameObjectBindInfoList.Clear();
                Transform[] gameObjects = bindObject.GetComponentsInChildren<Transform>(true);

                List<ComponentBindInfo> componentBindInfoList = new List<ComponentBindInfo>();

                int amount = gameObjects.Length;
                for (int i = 0; i < amount; i++) {
                    Transform go = gameObjects[i];
                    int componentAmount = new ComponentBindInfo(go.gameObject).typeStrings.Length;
                    for (int j = 0; j < componentAmount; j++) {
                        ComponentBindInfo info = new ComponentBindInfo(go.gameObject);
                        info.index = j;
                        componentBindInfoList.Add(info);
                        if (commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) info.name = CommonTools.GetNumberAlpha(info.instanceObject.name);
                    }
                }
                int infoAmount = componentBindInfoList.Count;
                for (int i = 0; i < infoAmount; i++) {
                    ComponentBindInfo info = componentBindInfoList[i];
                    if (objectInfo.gameObjectBindInfoList.Contains(info) == false) objectInfo.gameObjectBindInfoList.Add(info);
                }
                Debug.Log("脚本生成路径：" + path);
                ScriptGenerate.CSharpWrite(commonSettingData, generateData, path);
                objectInfo.gameObjectBindInfoList = oldBindDataList;
            }
            Debug.Log("Create ScriptSetting Finish.");
        }

        [DidReloadScripts]
        static void AddComponent()
        {
            CreatePrefab();
        }

        static void CreatePrefab()
        {
            CommonSettingData commonSettingData = CommonTools.GetCommonSettingData();
            GenerateData generateData = CommonTools.GetGenerateData();
            string generateDataPath = AssetDatabase.GetAssetPath(generateData);

            if (generateData == null) return;
            if (generateData.isStartBuild) { generateData.isStartBuild = false; }
            else {
                AssetDatabase.DeleteAsset(generateDataPath);
                AssetDatabase.Refresh();
                return;
            }

            GameObject bindObject = generateData.bindObject;
            if (bindObject == null) return;
            string path = Application.dataPath + "/" + commonSettingData.createPrefabPath;

            //检查路径是否有效
            if (Directory.Exists(path) == false) {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSettingData.isCreatePrefabFolder) {
                path += $"/{bindObject.name}";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            path += $"/{bindObject.name}.prefab";

            BindComponents bindComponents = bindObject.GetComponent<BindComponents>();
            Type addType = generateData.objectInfo.typeString.ToType();
            Component component = null;
            if (addType != null) {
                component = generateData.bindObject.GetComponent(addType);
                if (component == null) component = generateData.bindObject.AddComponent(addType);
                bindComponents.bindRoot = component;

                MethodInfo method = addType.GetMethod(generateData.getBindDataMethodName, new Type[] { });
                method.Invoke(component, new object[] { });
            }
            else { Debug.Log("添加类型为空"); }


            if (commonSettingData.isCreatePrefab) {
                //创建预制体
                PrefabUtility.SaveAsPrefabAssetAndConnect(bindObject, path, InteractionMode.AutomatedAction);
                Debug.Log("Create Prefab Finish.");
            }

            AssetDatabase.DeleteAsset(generateDataPath);
            AssetDatabase.Refresh();
        }
    }
}