using BindTool;
using UnityEngine;

public interface BindInfo
{
    TypeString[] GetTypeStrings();
    Object GetValue(int index);
}