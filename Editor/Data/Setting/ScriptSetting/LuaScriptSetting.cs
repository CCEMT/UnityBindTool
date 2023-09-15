using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class LuaScriptSetting
{
    [LabelText("模拟类名")]
    public string baseClassName;

    [LabelText("继承类名")]
    public string inheritClass;

    [LabelText("Lua模板")]
    public TextAsset luaTemplate;
}