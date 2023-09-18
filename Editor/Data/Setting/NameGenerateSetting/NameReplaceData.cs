using System;
using Sirenix.OdinInspector;

namespace UnityBindTool
{
    [Serializable]
    public class NameReplaceData
    {
        [LabelText("替换名称")]
        public string targetName;

        [LabelText("名称检查规则")]
        public NameCheck nameCheck = new NameCheck();
    }
}