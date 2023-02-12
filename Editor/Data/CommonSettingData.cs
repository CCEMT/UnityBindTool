#region Using

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace BindTool
{
    //[CreateAssetMenu(fileName = "CommonSettingData", menuName = "CommonSettingData", order = 0)]
    public class CommonSettingData : ScriptableObject
    {

        //ScriptSetting
        public bool isCreateScriptFolder;
        public string createScriptPath;
        public bool isCustomBind;
        public string newScriptName;
        public TypeString addTypeString;

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

       


        #region Equals

        public override bool Equals(object other)
        {
            CommonSettingData commonSettingData = (CommonSettingData) other;
            if (commonSettingData != null) { return Equals(commonSettingData); }
            else { return false; }
        }

        protected bool Equals(CommonSettingData other)
        {
            return base.Equals(other) && isCreateScriptFolder == other.isCreateScriptFolder && createScriptPath == other.createScriptPath && isCustomBind == other.isCustomBind &&
                   isCreatePrefab == other.isCreatePrefab && isCreatePrefabFolder == other.isCreatePrefabFolder && createPrefabPath == other.createPrefabPath && isCreateLua == other.isCreateLua &&
                   isCreateLuaFolder == other.isCreateLuaFolder && createLuaPath == other.createLuaPath && Equals(selectScriptSetting, other.selectScriptSetting) &&
                   Equals(scriptSettingList, other.scriptSettingList) && Equals(selectAutoBindSetting, other.selectAutoBindSetting) && Equals(autoBindSettingList, other.autoBindSettingList) &&
                   Equals(selectCreateNameSetting, other.selectCreateNameSetting) && Equals(createNameSettingList, other.createNameSettingList);
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ isCreateScriptFolder.GetHashCode();
                hashCode = (hashCode * 397) ^ (createScriptPath != null ? createScriptPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isCustomBind.GetHashCode();
                hashCode = (hashCode * 397) ^ isCreatePrefab.GetHashCode();
                hashCode = (hashCode * 397) ^ isCreatePrefabFolder.GetHashCode();
                hashCode = (hashCode * 397) ^ (createPrefabPath != null ? createPrefabPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isCreateLua.GetHashCode();
                hashCode = (hashCode * 397) ^ isCreateLuaFolder.GetHashCode();
                hashCode = (hashCode * 397) ^ (createLuaPath != null ? createLuaPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectScriptSetting != null ? selectScriptSetting.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (scriptSettingList != null ? scriptSettingList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectAutoBindSetting != null ? selectAutoBindSetting.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (autoBindSettingList != null ? autoBindSettingList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectCreateNameSetting != null ? selectCreateNameSetting.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (createNameSettingList != null ? createNameSettingList.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}