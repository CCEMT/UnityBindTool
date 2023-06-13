namespace BindTool
{
    public interface ITemplate { }

    public interface ITemplate<T>
    {
        T TemplateValue { get; }
    }
}