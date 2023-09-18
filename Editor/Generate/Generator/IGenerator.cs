namespace UnityBindTool
{
    public interface IGenerator
    {
        void Init(CompositionSetting setting, GenerateData generateData);

        void Write(string scriptPath);
    }
}