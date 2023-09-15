using System;
using System.Collections.Generic;
using BindTool;
using Sirenix.OdinInspector;

[Serializable]
public class StreamingBindSetting
{
    [LabelText("是否启用")]
    public bool isEnable;

    //是否绑定
    [LabelText("是否绑定组件")]
    public bool isBindComponent;

    //是否绑定所有
    [LabelText("是否绑定所有组件")]
    public bool isBindAllComponent;

    //根据流绑定
    public List<StreamingBindData> streamingBindDataList = new List<StreamingBindData>();
}

[Serializable]
public class StreamingBindData
{
    public int sequence;
    public bool isElse;
    public TypeString typeString;
}