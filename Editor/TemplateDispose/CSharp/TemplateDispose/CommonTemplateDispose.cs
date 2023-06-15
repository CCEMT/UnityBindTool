using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityBindTool
{
    public class CommonTemplateDispose : BaseTemplateDispose
    {
        private List<BaseCommonAttributeDispose> commonAttributeDisposes;

        public override void Dispose()
        {
            commonAttributeDisposes = new List<BaseCommonAttributeDispose>();
            string typeName = this.templateClass.Identifier.ValueText;
            Type templateType = Type.GetType(typeName);

            FieldInfo[] fieldInfos = templateType.GetFields();
            foreach (FieldInfo fieldInfo in fieldInfos) DisposeField(fieldInfo);

            PropertyInfo[] propertyInfos = templateType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos) DisposeProperty(propertyInfo);

            MethodInfo[] methodInfos = templateType.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos) DisposeMethod(methodInfo);
        }

        public override void Generate() { }

        private void DisposeField(FieldInfo fieldInfo)
        {
            string fieldName = fieldInfo.Name;
            FieldDeclarationSyntax fieldDeclarationSyntax = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldName);
            });
            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, fieldInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.generateContent = fieldDeclarationSyntax;
                dispose.DisposeField();
                commonAttributeDisposes.Add(dispose);
            });
        }

        private void DisposeProperty(PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;
            PropertyDeclarationSyntax propertyDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault((property) => property.Identifier.ValueText == propertyName);

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, propertyInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.generateContent = propertyDeclarationSyntax;
                dispose.DisposeProperty();
                commonAttributeDisposes.Add(dispose);
            });
        }

        private void DisposeMethod(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            MethodDeclarationSyntax methodDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault((method) => method.Identifier.ValueText == methodName);

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, methodInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.generateContent = methodDeclarationSyntax;
                dispose.DisposeMethod();
                commonAttributeDisposes.Add(dispose);
            });
        }
    }
}