using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityBindTool
{
    public partial class BindWindow : OdinEditorWindow
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

        [SerializeField]
        private BindSetting bindSetting;

        [SerializeField]
        private GameObject bindObject;

        [NonSerialized, OdinSerialize]
        private GenerateData generateData;

        [NonSerialized, OdinSerialize]
        private ObjectInfo editorObjectInfo;

        void Init()
        {
            bindObject = Selection.objects.First() as GameObject;
            this.bindSetting = BindSetting.Get();
            editorObjectInfo = ObjectInfoHelper.GetObjectInfo(bindObject);
            generateData = new GenerateData();
            generateData.objectInfo = this.editorObjectInfo;
            generateData.bindObject = this.bindObject;
            generateData.newScriptName = this.bindObject.name;

            BindInfoListInit();
        }

        protected override void OnEnable()
        {
            bindWindow = this;
            base.OnEnable();
        }

        protected override void OnDestroy()
        {
            bindWindow = null;
            base.OnDestroy();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
            
        }

        private void OnValidate()
        {
            if (bindObject == null) Close();
            else
            {
                this.bindSetting = BindSetting.Get();
                editorObjectInfo = ObjectInfoHelper.GetObjectInfo(bindObject);
                generateData = new GenerateData();
                generateData.objectInfo = this.editorObjectInfo;
                generateData.bindObject = this.bindObject;
                generateData.newScriptName = this.bindObject.name;

                BindInfoListInit();
            }
        }
    }
}