using BindTool;
using UnityEngine;

public class BindBase : BindInfo
{
    public Object target;
    public TypeString[] typeStrings;

    public TypeString[] GetTypeStrings()
    {
        return typeStrings;
    }

    public Object GetValue(int index)
    {
        return this.target;
    }
}