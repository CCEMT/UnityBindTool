using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

namespace UnityBindTool
{
    public class TypeTemplateDispose : BaseTemplateDispose
    {
        public FieldDeclarationSyntax templateField;
        public List<BaseAttributeDispose> disposes;

        public override void Dispose()
        {
            disposes = new List<BaseAttributeDispose>();
            string typeName = this.templateClass.Identifier.ValueText;
            Type templateType = Type.GetType(typeName);

            FieldInfo[] fieldInfos = templateType.GetFields();
            int amount = fieldInfos.Length;
            for (int i = 0; i < amount; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                TemplateFieldAttribute templateFieldAttribute = fieldInfo.GetCustomAttribute<TemplateFieldAttribute>();
                if (templateFieldAttribute == null) continue;
                templateField = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                    return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldInfo.Name);
                });
                break;
            }

            if (this.templateField == null)
            {
                Debug.LogError("模板类中没有找到模板字段");
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                DisposeField(fieldInfo);
            }

            PropertyInfo[] propertyInfos = templateType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos) DisposeProperty(propertyInfo);

            MethodInfo[] methodInfos = templateType.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos) DisposeMethod(methodInfo);
        }

        public override void Generate()
        {
            
        }

        private void DisposeField(FieldInfo fieldInfo)
        {
            string fieldName = fieldInfo.Name;
            FieldDeclarationSyntax fieldDeclarationSyntax = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldName);
            });

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, fieldInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.generateContent = fieldDeclarationSyntax;
                dispose.DisposeField();
                disposes.Add(dispose);
            });

            TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, fieldInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.templateField = templateField;
                dispose.generateContent = fieldDeclarationSyntax;
                dispose.DisposeField();
                disposes.Add(dispose);
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
                disposes.Add(dispose);
            });

            TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, propertyInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.templateField = templateField;
                dispose.generateContent = propertyDeclarationSyntax;
                dispose.DisposeProperty();
                disposes.Add(dispose);
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
                disposes.Add(dispose);
            });

            TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, methodInfo.GetCustomAttributes().ToArray(), (dispose) => {
                dispose.templateField = templateField;
                dispose.generateContent = methodDeclarationSyntax;
                dispose.DisposeMethod();
                disposes.Add(dispose);
            });
        }
    }
}