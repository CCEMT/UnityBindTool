using BindTool;
using UnityEngine;

public enum GeneratorType
{
    CSharp,
    Lua
}

public static class GeneratorFactory
{
    public static IGenerator GetGenerator(GeneratorType generatorType, CompositionSetting setting, GenerateData generateData)
    {
        IGenerator generator = default;
        switch (generatorType)
        {
            case GeneratorType.CSharp:
            {
                generator = new CSharpGenerator();
                break;
            }
            case GeneratorType.Lua:
            {
                generator = new LuaGenerator();
                break;
            }
        }
        if (generator.Equals(default))
        {
            Debug.LogError("没有对应的生成器");
            return default;
        }
        generator.Init(setting, generateData);
        return generator;
    }
}