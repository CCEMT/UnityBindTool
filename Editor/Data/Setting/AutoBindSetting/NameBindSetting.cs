using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace UnityBindTool
{
    [Serializable]
    public class NameBindSetting
    {
        [LabelText("是否启用")]
        public bool isEnable;

        [LabelText("名称绑定列表")]
        public List<NameBindData> nameBindDataList = new List<NameBindData>();
    }

    [Serializable]
    public class NameBindData
    {
        public NameCheck nameCheck = new NameCheck();
        public TypeString typeString;
    }
}