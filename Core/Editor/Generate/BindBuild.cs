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
        static CommonSettingData GetCommonSettingData()
        {
            DataContainer dataContainer = Resources.Load<DataContainer>(ConstData.DataContainerName);
            if (dataContainer != null) { return dataContainer.commonSettingData; }
            else { return null; }
        }

        public static void Build(GameObject bindObject, ObjectInfo objectInfo)
        {
            CommonSettingData commonSettingData = GetCommonSettingData();

            string path = Application.dataPath + "/" + commonSettingData.createScriptPath + "/";
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSettingData.isCreateScriptFolder)
            {
                path += $"{commonSettingData.tempGenerateData.newScriptName}/";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            //保存临时数据
            commonSettingData.tempGenerateData.bindObject = bindObject;
            commonSettingData.tempGenerateData.objectInfo = objectInfo;
            commonSettingData.tempGenerateData.isStartBuild = true;
            Debug.Log("脚本生成路径：" + path);
            if (commonSettingData.isCustomBind) { ScriptGenerate.CSharpWrite(commonSettingData, path); }
            else
            {
                List<ComponentBindInfo> oldBindDataList = objectInfo.gameObjectBindInfoList;

                objectInfo.gameObjectBindInfoList.Clear();
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
                        if (commonSettingData.selectCreateNameSetting.isBindAutoGenerateName) info.name = CommonTools.GetNumberAlpha(info.instanceObject.name);
                    }
                }
                int infoAmount = componentBindInfoList.Count;
                for (int i = 0; i < infoAmount; i++)
                {
                    ComponentBindInfo info = componentBindInfoList[i];
                    if (objectInfo.gameObjectBindInfoList.Contains(info) == false) objectInfo.gameObjectBindInfoList.Add(info);
                }
                Debug.Log("脚本生成路径：" + path);
                ScriptGenerate.CSharpWrite(commonSettingData, path);
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
            CommonSettingData commonSettingData = GetCommonSettingData();
            if (commonSettingData == null) return;
            if (commonSettingData.tempGenerateData.isStartBuild) { commonSettingData.tempGenerateData.isStartBuild = false; }
            else { return; }

            GameObject bindObject = commonSettingData.tempGenerateData.bindObject;
            if (bindObject == null) return;
            string path = Application.dataPath + "/" + commonSettingData.createPrefabPath;

            //检查路径是否有效
            if (Directory.Exists(path) == false)
            {
                Debug.LogError($"{path} 不是有效路径！");
                return;
            }

            //创建文件夹
            if (commonSettingData.isCreatePrefabFolder)
            {
                path += $"/{bindObject.name}";
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            }

            path += $"/{bindObject.name}.prefab";

            Type addType = commonSettingData.tempGenerateData.objectInfo.typeString.ToType();
            Component component = null;
            if (addType != null)
            {
                component = commonSettingData.tempGenerateData.bindObject.GetComponent(addType);
                if (component == null) component = commonSettingData.tempGenerateData.bindObject.AddComponent(addType);

                MethodInfo method = addType.GetMethod(commonSettingData.tempGenerateData.getBindDataMethodName, new Type[] { });
                method.Invoke(component, new object[] { });
            }
            else { Debug.Log("添加类型为空"); }

            if (commonSettingData.isCreatePrefab)
            {
                //创建预制体
                PrefabUtility.SaveAsPrefabAssetAndConnect(bindObject, path, InteractionMode.AutomatedAction);

                Debug.Log("Create Prefab Finish.");
            }
        }
    }
}