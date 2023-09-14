using BindTool;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityBindTool
{
    public abstract class BaseTemplateDispose
    {
        public GenerateData generateData;
        public ScriptSetting scriptSetting;
        public NameGenerateSetting nameGenerateSetting;
        public NameDisposeCentre nameDisposeCentre;

        public ClassDeclarationSyntax mainTargetClass;
        public ClassDeclarationSyntax partialTargetClass;

        public ClassDeclarationSyntax templateClass;

        public abstract void Dispose();

        public abstract void Generate();
    }
}