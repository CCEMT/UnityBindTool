using System.Linq;
using BindTool;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class BindWindow : EditorWindow
{
    private static BindWindow bindWindow;

    [MenuItem("GameObject/BindWindown", false, 0)]
    static void BindTarget()
    {
        if (Check() == false)
        {
            Debug.LogError("请选择一个正确的对象");
            return;
        }

        bindWindow = GetWindow<BindWindow>("BindWindown");
        bindWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
        bindWindow.Init();
    }

    static bool Check()
    {
        Object[] selectObjects = Selection.objects;
        if (selectObjects.Length <= 0) return false;
        Object selectObject = selectObjects.First();
        if (selectObject == null) return false;
        GameObject gameObject = selectObjects.First() as GameObject;
        return gameObject != null;
    }

    private BindWindowState bindWindowState;

    private BindSetting bindSetting;
    private GameObject bindObject;
    private ObjectInfo editorObjectInfo;
    private GenerateData generateData;

    void Init()
    {
        bindWindowState = BindWindowState.BindInfoListGUI;

        bindObject = Selection.objects.First() as GameObject;
        this.bindSetting = BindSetting.Get();
        editorObjectInfo = ObjectInfoHelper.GetObjectInfo(bindObject);
        generateData = new GenerateData();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        switch (this.bindWindowState)
        {
            case BindWindowState.BindInfoListGUI:
                BindInfoListGUIDraw();
                break;
            case BindWindowState.BuildGUI:
                BuildGUIDraw();
                break;
        }
    }
}