#region Using

using UnityEngine;

#endregion

namespace BindTool
{
    public partial class BindWindown
    {
        private int settingIndex;

        public void DrawSettingGUI() {
            settingIndex = GUILayout.Toolbar(settingIndex, new string[] {"ScriptSetting","AutoBindSetting","CreateNameSetting"});
            switch (settingIndex) {
                case 0:
                    DrawScriptSettingGUI();
                    break;
                case 1:
                    DrawAutoSettingGUI();
                    break;
                case 2:
                    DrawNameSettingGUI();
                    break;
            }
        }
    }
}