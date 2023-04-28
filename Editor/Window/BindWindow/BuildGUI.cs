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
                    string tempScriptPath = GUILayout.TextField(this.mainSetting.createScriptPath);
                    if (tempScriptPath.Equals(this.mainSetting.createScriptPath) == false)
                    {
                        this.mainSetting.createScriptPath = tempScriptPath;
                        isSavaSetting = true;
                    }
                    if (GUILayout.Button("浏览", GUILayout.Width(100f)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择代码保存路径", Application.dataPath, null);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                            this.mainSetting.createScriptPath = targetPath;
                            isSavaSetting = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(this.mainSetting.createScriptPath))
                            {
                                this.mainSetting.createScriptPath = CommonConst.DefaultScriptCreatePath;
                                isSavaSetting = true;
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
               
                string path = Application.dataPath + "/" + this.mainSetting.createScriptPath;

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

                bool tempIsCreateScriptFile = GUILayout.Toggle(this.mainSetting.isCreateScriptFolder, "是否创建文件夹");
                if (tempIsCreateScriptFile != this.mainSetting.isCreateScriptFolder)
                {
                    this.mainSetting.isCreateScriptFolder = tempIsCreateScriptFile;
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
                            this.mainSetting.createScriptPath = CommonTools.GetObjectPath(target);
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
                bool tempIsCreatePrefab = GUILayout.Toggle(this.mainSetting.isCreatePrefab, "是否创建预制体");
                if (tempIsCreatePrefab != this.mainSetting.isCreatePrefab)
                {
                    this.mainSetting.isCreatePrefab = tempIsCreatePrefab;
                    isSavaSetting = true;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("预制体保存路径：", GUILayout.Width(120f));

                    this.mainSetting.createPrefabPath = GUILayout.TextField(this.mainSetting.createPrefabPath);
                    if (GUILayout.Button("浏览", GUILayout.Width(100f)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择预制体保存路径", Application.dataPath, null);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                            this.mainSetting.createPrefabPath = targetPath;
                            isSavaSetting = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(this.mainSetting.createPrefabPath))
                            {
                                this.mainSetting.createPrefabPath = CommonConst.DefaultPrefabCreatePath;
                                isSavaSetting = true;
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
              
                string path = Application.dataPath + "/" + this.mainSetting.createPrefabPath;
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

                bool tempIsCreatePrefabFile = GUILayout.Toggle(this.mainSetting.isCreatePrefabFolder, "是否创建文件夹");
                if (tempIsCreatePrefabFile != this.mainSetting.isCreatePrefabFolder)
                {
                    this.mainSetting.isCreatePrefabFolder = tempIsCreatePrefabFile;
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
                            this.mainSetting.createPrefabPath = CommonTools.GetObjectPath(target);
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
                bool tempIsCreatePrefab = GUILayout.Toggle(this.mainSetting.isCreateLua, "是否创建Lua");
                if (tempIsCreatePrefab != this.mainSetting.isCreateLua)
                {
                    this.mainSetting.isCreateLua = tempIsCreatePrefab;
                    isSavaSetting = true;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Lua保存路径：", GUILayout.Width(120f));

                    this.mainSetting.createLuaPath = GUILayout.TextField(this.mainSetting.createLuaPath);
                    if (GUILayout.Button("浏览", GUILayout.Width(100f)))
                    {
                        string targetPath = EditorUtility.OpenFolderPanel("选择Lua保存路径", Application.dataPath, null);
                        targetPath = targetPath.Substring(Application.dataPath.Length + 1, targetPath.Length - Application.dataPath.Length - 1);
                        if (string.IsNullOrEmpty(targetPath) == false)
                        {
                            this.mainSetting.createLuaPath = targetPath;
                            isSavaSetting = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(this.mainSetting.createLuaPath))
                            {
                                {
                                    this.mainSetting.createLuaPath = CommonConst.DefaultLuaCreatePath;
                                    isSavaSetting = true;
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                string path = Application.dataPath + "/" + this.mainSetting.createLuaPath;
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

                bool tempIsCreatePrefabFile = GUILayout.Toggle(this.mainSetting.isCreateLuaFolder, "是否创建文件夹");
                if (tempIsCreatePrefabFile != this.mainSetting.isCreateLuaFolder)
                {
                    this.mainSetting.isCreateLuaFolder = tempIsCreatePrefabFile;
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
                GUIUtility.systemCopyBuffer = string.Join("\n", GenerateCSharpData.Generate(this.mainSetting, generateData));
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
                    BindBuild.Build(bindObject, objectInfo,GeneratorType.CSharp);
                    EditorUtility.SetDirty(this.mainSetting);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Close();
                }
            }
        }
    }
}