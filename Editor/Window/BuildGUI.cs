#region Using

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#endregion

namespace BindTool
{
    public partial class BindWindow
    {
        public void DrawBuildGUI()
        {
            CreateScriptInfoSet();
            CreatePrefabInfoSet();
            //CreateLuaInfoSet();
            Create();
        }

        void CreateScriptInfoSet()
        {
            GUILayout.Label("Script", scriptGuiStyle);

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("代码保存路径：", GUILayout.Width(120f));
                    string tempScriptPath = GUILayout.TextField(commonSettingData.createScriptPath);
                    if (tempScriptPath.Equals(commonSettingData.createScriptPath) == false)
                    {
                        commonSettingData.createScriptPath = tempScriptPath;
                        isSavaSetting = true;
                    }
                    if (GUILayout.Button("浏览", GUILayout.Width(100f)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择代码保存路径", Application.dataPath, null);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                            commonSettingData.createScriptPath = targetPath;
                            isSavaSetting = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(commonSettingData.createScriptPath))
                            {
                                commonSettingData.createScriptPath = ConstData.DefaultScriptCreatePath;
                                isSavaSetting = true;
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
               
                string path = Application.dataPath + "/" + commonSettingData.createScriptPath;

                string errorInfo = "错误：脚本保存路径错误，请重新选择路径！";
                if (Directory.Exists(path) == false)
                {
                    if (errorList.Contains(errorInfo) == false) errorList.Add(errorInfo);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.color = Color.red;
                        GUILayout.BeginVertical("box");
                        GUILayout.Label(errorInfo);
                        GUILayout.EndHorizontal();
                        GUI.color = Color.white;

                        if (GUILayout.Button("创建", GUILayout.Width(120f), GUILayout.Height(25)))
                        {
                            Directory.CreateDirectory(path);
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (errorList.Contains(errorInfo)) errorList.Remove(errorInfo);
                }

                bool tempIsCreateScriptFile = GUILayout.Toggle(commonSettingData.isCreateScriptFolder, "是否创建文件夹");
                if (tempIsCreateScriptFile != commonSettingData.isCreateScriptFolder)
                {
                    commonSettingData.isCreateScriptFolder = tempIsCreateScriptFile;
                    isSavaSetting = true;
                }
            }
            GUILayout.EndHorizontal();
            Rect scriptRect = GUILayoutUtility.GetLastRect();
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (scriptRect.Contains(e.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            Object target = DragAndDrop.objectReferences.First();
                            commonSettingData.createScriptPath = CommonTools.GetObjectPath(target);
                        }
                        e.Use();
                    }
                    break;
            }
        }
        
        void CreatePrefabInfoSet()
        {
            GUILayout.Label("Prefab", prefabGuiStyle);

            GUILayout.BeginVertical("box");
            {
                bool tempIsCreatePrefab = GUILayout.Toggle(commonSettingData.isCreatePrefab, "是否创建预制体");
                if (tempIsCreatePrefab != commonSettingData.isCreatePrefab)
                {
                    commonSettingData.isCreatePrefab = tempIsCreatePrefab;
                    isSavaSetting = true;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("预制体保存路径：", GUILayout.Width(120f));

                    commonSettingData.createPrefabPath = GUILayout.TextField(commonSettingData.createPrefabPath);
                    if (GUILayout.Button("浏览", GUILayout.Width(100f)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择预制体保存路径", Application.dataPath, null);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                            commonSettingData.createPrefabPath = targetPath;
                            isSavaSetting = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(commonSettingData.createPrefabPath))
                            {
                                commonSettingData.createPrefabPath = ConstData.DefaultPrefabCreatePath;
                                isSavaSetting = true;
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
              
                string path = Application.dataPath + "/" + commonSettingData.createPrefabPath;
                string errorInfo = "错误：预制体保存路径错误，请重新选择路径！";
                if (Directory.Exists(path) == false)
                {
                    if (errorList.Contains(errorInfo) == false) errorList.Add(errorInfo);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.color = Color.red;
                        GUILayout.BeginVertical("box");
                        GUILayout.Label(errorInfo);
                        GUILayout.EndHorizontal();
                        GUI.color = Color.white;

                        if (GUILayout.Button("创建", GUILayout.Width(120f), GUILayout.Height(25)))
                        {
                            Directory.CreateDirectory(path);

                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (errorList.Contains(errorInfo)) errorList.Remove(errorInfo);
                }

                bool tempIsCreatePrefabFile = GUILayout.Toggle(commonSettingData.isCreatePrefabFolder, "是否创建文件夹");
                if (tempIsCreatePrefabFile != commonSettingData.isCreatePrefabFolder)
                {
                    commonSettingData.isCreatePrefabFolder = tempIsCreatePrefabFile;
                    isSavaSetting = true;
                }
            }
            GUILayout.EndHorizontal();
            Rect prefabRect = GUILayoutUtility.GetLastRect();
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (prefabRect.Contains(e.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            Object target = DragAndDrop.objectReferences.First();
                            commonSettingData.createPrefabPath = CommonTools.GetObjectPath(target);
                        }
                        e.Use();
                    }
                    break;
            }
        }

        void CreateLuaInfoSet()
        {
            GUILayout.Label("Lua", luaGuiStyle);

            GUILayout.BeginVertical("box");
            {
                bool tempIsCreatePrefab = GUILayout.Toggle(commonSettingData.isCreateLua, "是否创建Lua");
                if (tempIsCreatePrefab != commonSettingData.isCreateLua)
                {
                    commonSettingData.isCreateLua = tempIsCreatePrefab;
                    isSavaSetting = true;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Lua保存路径：", GUILayout.Width(120f));

                    commonSettingData.createLuaPath = GUILayout.TextField(commonSettingData.createLuaPath);
                    if (GUILayout.Button("浏览", GUILayout.Width(100f)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择Lua保存路径", Application.dataPath, null);
                        targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            commonSettingData.createLuaPath = targetPath;
                            isSavaSetting = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(commonSettingData.createLuaPath))
                            {
                                {
                                    commonSettingData.createLuaPath = ConstData.DefaultLuaCreatePath;
                                    isSavaSetting = true;
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                string path = Application.dataPath + "/" + commonSettingData.createLuaPath;
                string errorInfo = "错误：Lua保存路径错误，请重新选择路径！";
                if (Directory.Exists(path) == false)
                {
                    if (errorList.Contains(errorInfo) == false) errorList.Add(errorInfo);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.color = Color.red;
                        GUILayout.BeginVertical("box");
                        GUILayout.Label(errorInfo);
                        GUILayout.EndHorizontal();
                        GUI.color = Color.white;

                        if (GUILayout.Button("创建", GUILayout.Width(120f), GUILayout.Height(25)))
                        {
                            Directory.CreateDirectory(path);

                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (errorList.Contains(errorInfo)) errorList.Remove(errorInfo);
                }

                bool tempIsCreatePrefabFile = GUILayout.Toggle(commonSettingData.isCreateLuaFolder, "是否创建文件夹");
                if (tempIsCreatePrefabFile != commonSettingData.isCreateLuaFolder)
                {
                    commonSettingData.isCreateLuaFolder = tempIsCreatePrefabFile;
                    isSavaSetting = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        void Create()
        {
            if (GUILayout.Button("Copy CSharp Code"))
            {
                GenerateData generateData = new GenerateData();
                generateData.bindObject = bindObject;
                generateData.objectInfo = objectInfo;
                GUIUtility.systemCopyBuffer = string.Join("\n", GenerateCSharpData.Generate(commonSettingData, generateData, false));
            }
            //if (GUILayout.Button("Copy Lua Code")) Debug.Log("开发中...");

            if (GUILayout.Button("Build"))
            {
                int errorAmount = errorList.Count;
                if (errorAmount > 0)
                {
                    Debug.LogError($"构建失败,有{errorAmount}个错误");
                    for (int i = 0; i < errorAmount; i++)
                    {
                        string errorInfo = errorList[i];
                        Debug.LogError(errorInfo);
                    }
                }
                else
                {
                    BindBuild.Build(bindObject, objectInfo);
                    EditorUtility.SetDirty(commonSettingData);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Close();
                }
            }
        }
    }
}