using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;

namespace UnityBindTool
{
    public class AssemblySelectHelper
    {
        public static void DrawAssemblySelect(Action<string> endCallback)
        {
            Dictionary<string, string> assemblyForDraw = new Dictionary<string, string>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            int assemblyAmount = assemblies.Length;
            for (int i = 0; i < assemblyAmount; i++)
            {
                string assemblyName = assemblies[i].GetName().Name;
                assemblyForDraw.Add(assemblyName, assemblyName);
            }

            IEnumerable<GenericSelectorItem<string>> customCollection = assemblyForDraw.Keys.Select(itemName =>
                new GenericSelectorItem<string>($"{itemName}", assemblyForDraw[itemName]));

            GenericSelector<string> CustomGenericSelector = new("选择程序集", false, customCollection);
            CustomGenericSelector.EnableSingleClickToSelect();
            CustomGenericSelector.SelectionChanged += ints => {
                string result = ints.FirstOrDefault();
                if (result != null) { endCallback?.Invoke(result); }
            };

            CustomGenericSelector.ShowInPopup(300);
        }
    }
}