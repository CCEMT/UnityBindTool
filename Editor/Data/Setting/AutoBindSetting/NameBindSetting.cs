using System;
using System.Collections.Generic;
using BindTool;

[Serializable]
public class NameBindSetting
{
    public bool isEnable;
    public List<NameBindData> nameBindDataList = new List<NameBindData>();
}

[Serializable]
public class NameBindData
{
    public NameCheck nameCheck = new NameCheck();
    public TypeString typeString;
}