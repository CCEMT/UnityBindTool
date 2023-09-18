using System.Collections.Generic;

namespace UnityBindTool
{
    public class NameAddNumberDisposer : IRepetitionNameDisposer
    {
        //key=名称 ,value=使用次数
        private Dictionary<string, int> rawNames = new Dictionary<string, int>();

        public string DisposeName(NameDisposeCentre nameDisposeCentre, string rawName)
        {
            string targetName = rawName;
            while (true)
            {
                if (nameDisposeCentre.useNames.Contains(targetName) == false) { break; }

                if (this.rawNames.ContainsKey(rawName)) { rawNames[rawName]++; }
                else { this.rawNames.Add(rawName, 1); }
                targetName = rawName + rawNames[rawName];
            }

            nameDisposeCentre.useNames.Add(targetName);
            return targetName;
        }
    }
}