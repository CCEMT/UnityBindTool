using BindTool;

public interface IGenerator
{
    void Init(MainSetting setting, GenerateData generateData);

    void Write(string scriptPath);
}