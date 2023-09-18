using UnityEngine;

namespace UnityBindTool
{
    public interface IBindInfo
    {
        TypeString[] GetTypeStrings();
        Object GetValue(int index);
    }
}