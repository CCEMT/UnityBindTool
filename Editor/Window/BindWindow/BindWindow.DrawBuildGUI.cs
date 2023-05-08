using UnityEngine;

public partial class BindWindow
{
    void DrawBuildGUI()
    {
        if (GUILayout.Button("返回")) { this.bindWindowState = BindWindowState.BindInfoListGUI; }
    }
}