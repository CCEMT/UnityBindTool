namespace UnityBindTool
{
    public class CSharpTemplateDisposeFactory
    {
        public static BaseTemplateDispose GetTemplateDispose(CSharpTemplateType templateType)
        {
            switch (templateType)
            {
                case CSharpTemplateType.Common:
                    return new CommonTemplateDispose();
                case CSharpTemplateType.Type:
                    return new TypeTemplateDispose();
            }
            return default;
        }
    }
}