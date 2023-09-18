using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace UnityBindTool
{
    [Serializable]
    public class StreamingBindSetting
    {
        [LabelText("是否启用")]
        public bool isEnable;

        [LabelText("是否绑定组件")]
        public bool isBindComponent;

        [LabelText("是否绑定所有组件")]
        public bool isBindAllComponent;

        [LabelText("流绑定列表")]
        public List<StreamingBindData> streamingBindDataList = new List<StreamingBindData>();
    }

    [Serializable]
    public class StreamingBindData
    {
        public int sequence;
        public bool isElse;
        public TypeString typeString;
    }
}