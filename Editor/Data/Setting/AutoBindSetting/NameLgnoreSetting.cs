using System;
using System.Collections.Generic;

[Serializable]
public class NameLgnoreSetting
{
    public bool isEnable;
    public List<NameCheck> nameLgnoreDataList = new List<NameCheck>();
}