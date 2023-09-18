using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityBindTool
{
    //[CreateAssetMenu(fileName = "BindSetting", menuName = "BindSetting", order = 0)]
    public class BindSetting : SingletonScriptableObject<BindSetting>
    {
        public BaseSetting baseSetting = new BaseSetting();

        [NonSerialized, OdinSerialize]
        public CompositionSetting selectCompositionSetting;

        [NonSerialized, OdinSerialize]
        public List<CompositionSetting> compositionSettingList = new List<CompositionSetting>();
    }

    [Serializable]
    public class BaseSetting
    {
        [LabelText("扫描的程序集列表(名称)")]
        public List<string> scanAssemblyList = new List<string>();

        [LabelText("扫描的程序集列表(Asset)")]
        public List<AssemblyDefinitionAsset> scanAssemblyAssetList = new List<AssemblyDefinitionAsset>();
    }

    [Serializable]
    public class CompositionSetting
    {
        [LabelText("名称")]
        public string compositionName;

        [HideInInspector]
        public CommonSetting commonSetting = new CommonSetting();

        [HideInInspector]
        public ScriptSetting scriptSetting = new ScriptSetting();

        [HideInInspector]
        public AutoBindSetting autoBindSetting = new AutoBindSetting();

        [HideInInspector]
        public NameGenerateSetting nameGenerateSetting = new NameGenerateSetting();
    }

    [Serializable]
    public class CommonSetting
    {
        [LabelText("是否创建脚本")]
        public bool isCreateScript;

        [LabelText("是否创建脚本文件夹")]
        public bool isCreateScriptFolder;

        [LabelText("脚本文件夹路径"), FolderPath(ParentFolder = CommonConst.AssetsFilterPath)]
        public string createScriptPath;

        [LabelText("是否创建预制体")]
        public bool isCreatePrefab;

        [LabelText("是否分离预制体")]
        public bool isDetachPrefab;

        [LabelText("BindComponents是否绑定到预制体上")]
        public bool isBindComponentsBindPrefab;

        [LabelText("是否创建预制体文件夹")]
        public bool isCreatePrefabFolder;

        [LabelText("预制体文件夹路径"), FolderPath(ParentFolder = CommonConst.AssetsFilterPath)]
        public string createPrefabPath;

        [LabelText("是否创建Lua脚本")]
        public bool isCreateLua;

        [LabelText("是否创建Lua文件夹")]
        public bool isCreateLuaFolder;

        [LabelText("Lua文件夹路径"), FolderPath(ParentFolder = CommonConst.AssetsFilterPath)]
        public string createLuaPath;
    }

    [Serializable]
    public class ScriptSetting
    {
        [LabelText("是否保存旧脚本")]
        public bool isSavaOldScript; //是否保存旧脚本

        [LabelText("旧脚本文件夹")]
        public DefaultAsset oldScriptFolderPath;

        [BoxGroup("命名设置"), HideLabel]
        public NameSetting nameSetting = new NameSetting();

        [HideInInspector]
        public CSharpScriptSetting csharpScriptSetting = new CSharpScriptSetting();

        [HideInInspector]
        public LuaScriptSetting luaScriptSetting = new LuaScriptSetting();
    }

    [Serializable]
    public class AutoBindSetting
    {
        [HideInInspector]
        public NameBindSetting nameBindSetting = new NameBindSetting();

        [HideInInspector]
        public NameLgnoreSetting nameLgnoreSetting = new NameLgnoreSetting();

        [HideInInspector]
        public StreamingBindSetting streamingBindSetting = new StreamingBindSetting();
    }

    [Serializable]
    public class NameGenerateSetting
    {
        [LabelText("是否绑定自动生成名称")]
        public bool isBindAutoGenerateName;

        [LabelText("名称替换列表")]
        public List<NameReplaceData> nameReplaceDataList = new List<NameReplaceData>();

        [HideInInspector]
        public CSharpNameGenerateSetting csharpNameGenerateSetting = new CSharpNameGenerateSetting();

        [HideInInspector]
        public LuaNameGenerateSetting luaNameGenerateSetting = new LuaNameGenerateSetting();
    }
}