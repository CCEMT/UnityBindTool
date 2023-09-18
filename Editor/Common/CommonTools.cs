using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityBindTool
{
    public static class CommonTools
    {
        public static GameObject GetPrefabAsset(GameObject gameObject)
        {
            if (gameObject == null) return null;
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject)) return gameObject;
            PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage != null) return prefabStage.prefabContentsRoot;
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject)) return PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
            return null;
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
            for (int i = 0; i < amount; i++)
            {
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
            for (int i = amount - 1; i >= 0; i--)
            {
                char c = content[i];
                suffix.Add(c);
                if (c == '_' || char.IsUpper(c)) break;
            }
            suffix.Reverse();
            return new string(suffix.ToArray());
        }

        public static string InitialUpper(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";
            char[] contents = content.ToCharArray();
            contents[0] = char.ToUpper(contents[0]);
            return new string(contents);
        }

        public static string InitialLower(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";
            char[] contents = content.ToCharArray();
            contents[0] = char.ToLower(contents[0]);
            return new string(contents);
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

        public static bool Search(string check, string input)
        {
            return string.IsNullOrEmpty(input) || Bdt(check.ToLower(), input.ToLower());
        }

        public static bool Bdt(string text, string str)
        {
            int i = 0;
            bool reu = false;
            foreach (var temp in str)
            {
                reu = false;
                for (; i < text.Length; i++)
                {
                    if (temp != text[i]) continue;
                    reu = true;
                    break;
                }
            }
            return reu;
        }

        public static bool SearchNumber(string checkNumber, string input)
        {
            if (string.IsNullOrEmpty(input)) return true;
            string inputNumber = GetNumber(input);
            if (checkNumber.Contains(inputNumber)) return true;
            return false;
        }

        public static string GetFolderPath(DefaultAsset folder)
        {
            string path = AssetDatabase.GetAssetPath(folder);
            path = path.Substring(6);
            return path;
        }

        public static string GetObjectPath(Object target)
        {
            string path = AssetDatabase.GetAssetPath(target);
            path = path.Substring(7);
            return path;
        }

        public static bool IsExist(string typeName, string typeNameSpace, string assemblyName)
        {
            TypeString typeString = new TypeString();
            typeString.typeName = typeName;
            typeString.typeNameSpace = typeNameSpace;
            typeString.assemblyName = assemblyName;
            Type type = typeString.ToType();
            return type != null;
        }
    }
}