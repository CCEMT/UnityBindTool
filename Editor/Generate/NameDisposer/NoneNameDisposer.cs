using System;

namespace UnityBindTool
{
    public class NoneNameDisposer : IRepetitionNameDisposer
    {
        public string DisposeName(NameDisposeCentre nameDisposeCentre, string rawName)
        {
            if (nameDisposeCentre.useNames.Contains(rawName)) throw new Exception($"名称重复：{rawName}");
            nameDisposeCentre.useNames.Add(rawName);
            return rawName;
        }
    }
}