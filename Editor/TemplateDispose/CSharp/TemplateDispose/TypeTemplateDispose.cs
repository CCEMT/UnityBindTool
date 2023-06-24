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
        public List<DisposeCotentData> disposes;

        public override void Dispose()
        {
            disposes = new List<DisposeCotentData>();
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
            ClassDeclarationSyntax generateMain = mainTargetClass;
            ClassDeclarationSyntax generatePartial = partialTargetClass;

            List<string> nameList = new List<string>();

            int bindDataAmount = this.generateData.objectInfo.bindDataList.Count;
            for (int i = 0; i < bindDataAmount; i++)
            {
                BindData bindData = this.generateData.objectInfo.bindDataList[i];
                nameList.Add(bindData.name);
            }

            int bindCollectionAmount = this.generateData.objectInfo.bindCollectionList.Count;
            for (int i = 0; i < bindCollectionAmount; i++)
            {
                BindCollection bindCollection = this.generateData.objectInfo.bindCollectionList[i];
                nameList.Add(bindCollection.name);
            }

            int nameAmount = nameList.Count;
            for (int i = 0; i < nameAmount; i++)
            {
                string name = nameList[i];
                int disposeAmount = this.disposes.Count;
                for (int j = 0; j < disposeAmount; j++)
                {
                    DisposeCotentData disposeCotentData = this.disposes[j];
                    DisposeCotentData targetDispose = ReplaceField(disposeCotentData, name);
                    if (targetDispose.generateTarget == this.mainTargetClass) { generateMain = generateMain.AddMembers(targetDispose.generateContent); }
                    else { generatePartial = generatePartial.AddMembers(targetDispose.generateContent); }
                }
            }

            mainTargetClass = generateMain;
            partialTargetClass = generatePartial;
        }

        DisposeCotentData ReplaceField(DisposeCotentData disposeCotentData, string replaceName)
        {
            DisposeCotentData newDispose = new DisposeCotentData();
            newDispose.generateContent = disposeCotentData.generateContent;
            newDispose.generateTarget = disposeCotentData.generateTarget;
            string templateFieldName = templateField.Declaration.Variables.First().Identifier.ValueText;
            IdentifierRewriter rewriter = new IdentifierRewriter(templateFieldName, replaceName);
            newDispose.generateContent = (MemberDeclarationSyntax) rewriter.Visit(newDispose.generateContent);
            return newDispose;
        }

        private void DisposeField(FieldInfo fieldInfo)
        {
            fieldInfo.GetCustomAttribute<TemplateFieldAttribute>();

            Attribute[] attributes = fieldInfo.GetCustomAttributes().ToArray();

            string fieldName = fieldInfo.Name;
            FieldDeclarationSyntax fieldDeclarationSyntax = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldName);
            });

            DisposeCotentData disposeCotentData = new DisposeCotentData();
            disposeCotentData.generateContent = fieldDeclarationSyntax;
            disposeCotentData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, disposeCotentData, attributes);
            TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, disposeCotentData, attributes);

            disposes.Add(disposeCotentData);
        }

        private void DisposeProperty(PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;
            PropertyDeclarationSyntax propertyDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault((property) => property.Identifier.ValueText == propertyName);

            DisposeCotentData disposeCotentData = new DisposeCotentData();
            disposeCotentData.generateContent = propertyDeclarationSyntax;
            disposeCotentData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            Attribute[] attributes = propertyInfo.GetCustomAttributes().ToArray();

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, disposeCotentData, attributes);
            TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, disposeCotentData, attributes);

            disposes.Add(disposeCotentData);
        }

        private void DisposeMethod(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            MethodDeclarationSyntax methodDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault((method) => method.Identifier.ValueText == methodName);

            DisposeCotentData disposeCotentData = new DisposeCotentData();
            disposeCotentData.generateContent = methodDeclarationSyntax;
            disposeCotentData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            Attribute[] attributes = methodInfo.GetCustomAttributes().ToArray();

            TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, disposeCotentData, attributes);
            TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, disposeCotentData, attributes);

            disposes.Add(disposeCotentData);
        }
    }
}