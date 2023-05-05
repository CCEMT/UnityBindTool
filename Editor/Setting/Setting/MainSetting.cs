#region Using

using System.Collections.Generic;

#endregion

namespace BindTool
{
    //[CreateAssetMenu(fileName = "CommonSettingData", menuName = "CommonSettingData", order = 0)]
    public class MainSetting : SingletonScriptableObject<MainSetting>
    {
        //ScriptSetting
        public bool isCreateScriptFolder;
        public string createScriptPath;
        public bool isCustomBind;
        public string newScriptName;
        public TypeString mergeTypeString;

        //PrefabSetting
        public bool isCreatePrefab;
        public bool isCreatePrefabFolder;
        public string createPrefabPath;

        //LuaSetting
        public bool isCreateLua;
        public bool isCreateLuaFolder;
        public string createLuaPath;

        //Setting
        public ScriptSetting selectScriptSetting;
        public List<ScriptSetting> scriptSettingList;

        public AutoBindSetting selectAutoBindSetting;
        public List<AutoBindSetting> autoBindSettingList;

        public CreateNameSetting selectCreateNameSetting;
        public List<CreateNameSetting> createNameSettingList;
    }
}