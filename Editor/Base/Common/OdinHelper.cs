using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public static class OdinHelper
{
    public static void DrawOdinEnum(string lable, Enum enumTargetValue, Action<Enum> action, string serachName = "选择")
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label(lable);
            string enumLable = GetEnumLableText(enumTargetValue);
            if (GUILayout.Button(enumLable, "MiniPopup"))
            {
                Dictionary<string, Enum> DataForDraw = new Dictionary<string, Enum>() { };
                Enum[] enums = Enum.GetValues(typeof(NameMatchingRule)).OfType<Enum>().ToArray();
                enums.ForEach((enumValue) => { DataForDraw.Add(GetEnumLableText(enumValue), enumValue); });

                IEnumerable<GenericSelectorItem<Enum>> customCollection = DataForDraw.Keys.Select(itemName =>
                    new GenericSelectorItem<Enum>($"{itemName}", DataForDraw[itemName]));

                GenericSelector<Enum> CustomGenericSelector = new(serachName, false, customCollection);
                CustomGenericSelector.EnableSingleClickToSelect();
                CustomGenericSelector.SelectionChanged += ints => {
                    Enum result = ints.FirstOrDefault();
                    if (result != null) { action?.Invoke(result); }
                };

                CustomGenericSelector.ShowInPopup(200);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public static string GetEnumLableText(Enum value)
    {
        Type type = value.GetType();
        FieldInfo fieldInfo = type.GetField(value.ToString());
        if (fieldInfo == null) return string.Empty;
        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(LabelTextAttribute), false);
        string name = string.Empty;
        foreach (LabelTextAttribute attribute in customAttributes) { name = attribute.Text; }
        return name;
    }
}