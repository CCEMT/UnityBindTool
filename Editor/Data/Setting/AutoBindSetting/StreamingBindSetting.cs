using System;
using System.Collections.Generic;
using BindTool;

[Serializable]
public class StreamingBindSetting
{
    public bool isEnable;

    //是否绑定
    public bool isBindComponent;

    //是否绑定所有
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