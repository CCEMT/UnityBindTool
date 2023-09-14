using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityBindTool
{
    public class CommonTemplateDispose : BaseTemplateDispose
    {
        private List<CommonDisposeCotentData> disposes;

        public override void Dispose()
        {
            this.disposes = new List<CommonDisposeCotentData>();
            string typeName = this.templateClass.Identifier.ValueText;
            Type templateType = Type.GetType(typeName);

            FieldInfo[] fieldInfos = templateType.GetFields();
            foreach (FieldInfo fieldInfo in fieldInfos) DisposeField(fieldInfo);

            PropertyInfo[] propertyInfos = templateType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos) DisposeProperty(propertyInfo);

            MethodInfo[] methodInfos = templateType.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos) DisposeMethod(methodInfo);
        }

        public override void Generate()
        {
            ClassDeclarationSyntax generateMain = mainTargetClass;
            ClassDeclarationSyntax generatePartial = partialTargetClass;

            int amount = this.disposes.Count;
            for (int i = 0; i < amount; i++)
            {
                CommonDisposeCotentData disposeCotentData = this.disposes[i];
                if (disposeCotentData.generateTarget == this.mainTargetClass) { generateMain = generateMain.AddMembers(disposeCotentData.generateContent); }
                else { generatePartial = generatePartial.AddMembers(disposeCotentData.generateContent); }
            }
            mainTargetClass = generateMain;
            partialTargetClass = generatePartial;
        }

        private void DisposeField(FieldInfo fieldInfo)
        {
            string fieldName = fieldInfo.Name;
            FieldDeclarationSyntax fieldDeclarationSyntax = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldName);
            });

            if (fieldDeclarationSyntax == null) return;

            Attribute[] attributes = fieldInfo.GetCustomAttributes().ToArray();

            CommonDisposeCotentData disposeCotentData = new CommonDisposeCotentData();
            disposeCotentData.generateContent = fieldDeclarationSyntax;
            disposeCotentData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, disposeCotentData, attributes);
            this.disposes.Add(disposeCotentData);
        }

        private void DisposeProperty(PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;
            PropertyDeclarationSyntax propertyDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault((property) => property.Identifier.ValueText == propertyName);

            if (propertyDeclarationSyntax == null) return;

            Attribute[] attributes = propertyInfo.GetCustomAttributes().ToArray();

            CommonDisposeCotentData disposeCotentData = new CommonDisposeCotentData();
            disposeCotentData.generateContent = propertyDeclarationSyntax;
            disposeCotentData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, disposeCotentData, attributes);
            this.disposes.Add(disposeCotentData);
        }

        private void DisposeMethod(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            MethodDeclarationSyntax methodDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault((method) => method.Identifier.ValueText == methodName);

            if (methodDeclarationSyntax == null) return;

            Attribute[] attributes = methodInfo.GetCustomAttributes().ToArray();

            CommonDisposeCotentData disposeCotentData = new CommonDisposeCotentData();
            disposeCotentData.generateContent = methodDeclarationSyntax;
            disposeCotentData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, disposeCotentData, attributes);

            this.disposes.Add(disposeCotentData);
        }
    }
}