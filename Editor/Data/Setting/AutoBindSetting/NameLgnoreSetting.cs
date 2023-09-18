using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace UnityBindTool
{
    [Serializable]
    public class NameLgnoreSetting
    {
        [LabelText("是否启用")]
        public bool isEnable;

        [LabelText("名称忽略列表")]
        public List<NameCheck> nameLgnoreDataList = new List<NameCheck>();
    }
}