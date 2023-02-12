﻿#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BindTool
{
    public static class CommonTools
    {
        public static CommonSettingData GetCommonSettingData()
        {
            string typeSearchString = $" t:{nameof(CommonSettingData)}";
            string[] guids = AssetDatabase.FindAssets(typeSearchString);
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var preferences = AssetDatabase.LoadAssetAtPath<CommonSettingData>(path);
                if (preferences != null) return preferences;
            }
            return null;
        }

        public static GenerateData GetGenerateData()
        {
            string typeSearchString = $" t:{nameof(GenerateData)}";
            string[] guids = AssetDatabase.FindAssets(typeSearchString);
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var preferences = AssetDatabase.LoadAssetAtPath<GenerateData>(path);
                if (preferences != null) return preferences;
            }
            return null;
        }

        public static GenerateData CreateGenerateData()
        {
            string typeSearchString = $" t:{nameof(CommonSettingData)}";
            string[] guids = AssetDatabase.FindAssets(typeSearchString);
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string createPath = new DirectoryInfo(path).Parent.ToString();
                createPath += $"/{nameof(GenerateData)}.asset";
                createPath = createPath.Substring(createPath.IndexOf("Assets", StringComparison.Ordinal));
                GenerateData generateData = ScriptableObject.CreateInstance<GenerateData>();
                AssetDatabase.CreateAsset(generateData, createPath);
                AssetDatabase.Refresh();
                return generateData;
            }
            return null;
        }

        /// <summary>
        /// 获取预制体资源。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static GameObject GetPrefabAsset(GameObject gameObject)
        {
            if (gameObject == null) return null;
            // Project中的Prefab是Asset不是Instance
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject)) {
                // 预制体资源就是自身
                return gameObject;
            }


            // PrefabMode中的GameObject既不是Instance也不是Asset
            PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage != null) {
                // 预制体资源：prefabAsset = prefabStage.prefabContentsRoot
                return prefabStage.prefabContentsRoot;
            }

            // Scene中的Prefab Instance是Instance不是Asset
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject)) {
                // 获取预制体资源
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                return prefabAsset;
            }


            // 不是预制体
            return null;
        }

        public static ObjectInfo GetObjectInfo(GameObject bindObject)
        {
            ObjectInfo objectInfo = null;

            BindComponents bindComponents = bindObject.GetComponent<BindComponents>();
            if (bindComponents != null) {
                objectInfo = new ObjectInfo();
                objectInfo.typeString = new TypeString(bindComponents.bindRoot.GetType());
                objectInfo.rootBindInfo = new ComponentBindInfo(bindObject);
                objectInfo.gameObjectBindInfoList=new List<ComponentBindInfo>();
                objectInfo.dataBindInfoList = new List<DataBindInfo>();

                int amount = bindComponents.bindComponentList.Count;
                for (int i = 0; i < amount; i++) {
                    Object bindComponent = bindComponents.bindComponentList[i];
                    if (bindComponent is GameObject == false && bindComponent.GetType().IsSubclassOf(typeof(Component)) == false) {
                        ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindComponent);
                        objectInfo.gameObjectBindInfoList.Add(componentBindInfo);
                    }
                    else {
                        DataBindInfo dataBindInfo = new DataBindInfo(bindComponent);
                        objectInfo.dataBindInfoList.Add(dataBindInfo);
                    }
                }
            }

            if (objectInfo == null) {
                objectInfo = new ObjectInfo();
                objectInfo.gameObjectBindInfoList = new List<ComponentBindInfo>();
                objectInfo.dataBindInfoList = new List<DataBindInfo>();
                objectInfo.SetObject(bindObject);
            }

            return objectInfo;
        }

        public static string SetName(string content, CreateNameSetting createNameSetting)
        {
            string newName = GetNumberAlpha(content);
            int amount = createNameSetting.nameReplaceDataList.Count;
            for (int i = 0; i < amount; i++) {
                NameReplaceData nameReplaceData = createNameSetting.nameReplaceDataList[i];
                if (nameReplaceData.nameCheck.Check(newName, out string matchingContent)) newName = content.Replace(matchingContent, nameReplaceData.targetName);
            }

            return newName;
        }

        public static string GetNumberAlpha(string source)
        {
            if (string.IsNullOrEmpty(source)) return null;
            string pattern = "[A-Za-z0-9_]";
            string strRet = "";
            MatchCollection results = Regex.Matches(source, pattern);
            foreach (Match v in results) strRet += v.ToString();
            return strRet;
        }

        public static string GetNumber(string source)
        {
            if (string.IsNullOrEmpty(source)) return null;
            string pattern = "[0-9]";
            string strRet = "";
            MatchCollection results = Regex.Matches(source, pattern);
            foreach (Match v in results) strRet += v.ToString();
            return strRet;
        }

        public static string GetPrefix(string content)
        {
            int amount = content.Length;
            string prefix = "";
            for (int i = 0; i < amount; i++) {
                char c = content[i];
                if ((c == '_' && i != 0) || (char.IsUpper(c) && i != 0)) { break; }
                else { prefix += c.ToString(); }
            }
            return prefix;
        }

        public static string GetSuffix(string content)
        {
            int amount = content.Length;
            List<char> suffix = new List<char>();
            for (int i = amount - 1; i >= 0; i--) {
                char c = content[i];
                suffix.Add(c);
                if (c == '_' || char.IsUpper(c)) break;
            }
            suffix.Reverse();
            return new string(suffix.ToArray());
        }

        public static string InitialUpper(string content)
        {
            if (string.IsNullOrEmpty(content) == false) {
                char[] contents = content.ToCharArray();
                contents[0] = char.ToUpper(contents[0]);
                return new string(contents);
            }
            return "";
        }

        public static string InitialLower(string content)
        {
            if (string.IsNullOrEmpty(content) == false) {
                char[] contents = content.ToCharArray();
                contents[0] = char.ToLower(contents[0]);
                return new string(contents);
            }
            return "";
        }

        public static string NameSettingByName(ComponentBindInfo componentBindInfo, NameSetting nameSetting)
        {
            string targetName = componentBindInfo.name;
            switch (nameSetting.namingDispose) {
                case ScriptNamingDispose.InitialLower:
                    targetName = InitialLower(targetName);
                    break;
                case ScriptNamingDispose.InitialUpper:
                    targetName = InitialUpper(targetName);
                    break;
                case ScriptNamingDispose.AllLower:
                    targetName = componentBindInfo.name.ToLower();
                    break;
                case ScriptNamingDispose.AllUppe:
                    targetName = componentBindInfo.name.ToUpper();
                    break;
            }

            if (nameSetting.isAddClassName) {
                if (nameSetting.isFrontOrBehind) { targetName = componentBindInfo.GetTypeName() + componentBindInfo.name; }
                else { targetName = componentBindInfo.name + componentBindInfo.GetTypeName(); }
            }

            if (nameSetting.isAddFront) targetName = nameSetting.frontName + componentBindInfo.name;
            if (nameSetting.isAddBehind) targetName = componentBindInfo.name + nameSetting.behindName;
            return targetName;
        }

        public static string NameSettingByName(DataBindInfo dataBindInfo, NameSetting nameSetting)
        {
            string targetName = dataBindInfo.name;
            switch (nameSetting.namingDispose) {
                case ScriptNamingDispose.InitialLower:
                    targetName = InitialLower(targetName);
                    break;
                case ScriptNamingDispose.InitialUpper:
                    targetName = InitialUpper(targetName);
                    break;
                case ScriptNamingDispose.AllLower:
                    targetName = dataBindInfo.name.ToLower();
                    break;
                case ScriptNamingDispose.AllUppe:
                    targetName = dataBindInfo.name.ToUpper();
                    break;
            }

            if (nameSetting.isAddClassName) {
                if (nameSetting.isFrontOrBehind) { targetName = dataBindInfo.typeString.typeName + dataBindInfo.name; }
                else { targetName = dataBindInfo.name + dataBindInfo.typeString.typeName; }
            }

            if (nameSetting.isAddFront) targetName = nameSetting.frontName + dataBindInfo.name;
            if (nameSetting.isAddBehind) targetName = dataBindInfo.name + nameSetting.behindName;
            return targetName;
        }

        public static string GetWholePath(Transform currentGameObject, GameObject target)
        {
            if (currentGameObject.parent == null || currentGameObject.parent.gameObject == target) return currentGameObject.name;
            return GetWholePath(currentGameObject.parent, target) + "/" + currentGameObject.name;
        }

        public static bool GetIsParent(Transform currentGameObject, GameObject target)
        {
            if (currentGameObject.parent == null) return false;
            if (currentGameObject.parent.gameObject == target) return true;
            return GetIsParent(currentGameObject.parent, target);
        }

        public static Type[] GetAllDerivedTypes(this AppDomain targetAppDomain, Type targetType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = targetAppDomain.GetAssemblies();
            int assebliesAmount = assemblies.Length;
            for (int i = 0; i < assebliesAmount; i++) {
                Assembly assembly = assemblies[i];
                Type[] types = assembly.GetTypes();
                int typesAmount = types.Length;
                for (int j = 0; j < typesAmount; j++) {
                    Type type = types[j];
                    if (type.IsSubclassOf(targetType)) result.Add(type);
                }
            }

            return result.ToArray();
        }

        public static Rect GetEditorMainWindowPos()
        {
            Type containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).FirstOrDefault(t => t.Name == "ContainerWindow");
            if (containerWinType == null) throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            FieldInfo showModeField = containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo positionProperty = containerWinType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
            if (showModeField == null || positionProperty == null) {
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            }
            Object[] windows = Resources.FindObjectsOfTypeAll(containerWinType);
            int amount = windows.Length;
            for (int i = 0; i < amount; i++) {
                Object window = windows[i];
                int showmode = (int) showModeField.GetValue(window);
                if (showmode == 4) // main window
                {
                    Rect pos = (Rect) positionProperty.GetValue(window, null);
                    return pos;
                }
            }
            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }

        public static void CenterOnMainWin(this EditorWindow aWin)
        {
            Rect main = GetEditorMainWindowPos();
            Rect pos = aWin.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            try { aWin.position = pos; }
            catch (Exception e) {
                Debug.Log("编译错误：请删除Library/ScriptAssemblies/Assembly-CSharp-Editor和BindTool,重新打开项目后导入BindTool");
                Debug.Log(e);
            }
        }

        public static string GetVisitString(VisitType valueTuple)
        {
            switch (valueTuple) {
                case VisitType.Public:
                    return "public";
                case VisitType.Private:
                    return "private";
                case VisitType.Protected:
                    return "protected";
                case VisitType.Internal:
                    return "internal";
            }
            return null;
        }

        public static string GetRemoveString(RemoveType removeType)
        {
            switch (removeType) {
                case RemoveType.This:
                    return "解除自身绑定";
                case RemoveType.Child:
                    return "解除子对象绑定";
                case RemoveType.ThisAndChild:
                    return "解除自身以及子对象绑定";
            }
            return null;
        }

        public static bool Search(string check, string input)
        {
            if (string.IsNullOrEmpty(input)) return true;
            check = check.ToLower();
            input = input.ToLower();
            if (check.Contains(input)) return true;
            return false;
        }

        public static bool SearchNumber(string checkNumber, string input)
        {
            if (string.IsNullOrEmpty(input)) return true;
            string inputNumber = GetNumber(input);
            if (checkNumber.Contains(inputNumber)) return true;
            return false;
        }
    }
}