using System;
using System.Collections.Generic;
using Sirenix.Serialization;

[Serializable]
public class BindData
{
    public string name;
    public int index;

    [NonSerialized, OdinSerialize]
    public BindInfo bindTarget;

    [NonSerialized, OdinSerialize]
    public List<BindInfo> bindInfos = new List<BindInfo>();
}