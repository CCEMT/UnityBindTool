using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

namespace UnityBindTool
{
    public class TypeTemplateDispose : BaseTemplateDispose
    {
        public FieldDeclarationSyntax templateField;
        public FieldInfo templateFieldInfo;
        public List<DisposeCotentData> disposes;

        public IRepetitionNameDisposer fieldNameDisposer;
        public IRepetitionNameDisposer propertyNameDisposer;
        public IRepetitionNameDisposer methodNameDisposer;

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
                templateFieldInfo = fieldInfo;
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
            CSharpScriptSetting csharpScriptSetting = this.scriptSetting.csharpScriptSetting;
            fieldNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(this.scriptSetting.nameSetting.repetitionNameDispose);
            propertyNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(csharpScriptSetting.propertyNameSetting.repetitionNameDispose);
            methodNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(csharpScriptSetting.methodNameSetting.repetitionNameDispose);

            ClassDeclarationSyntax generateMain = mainTargetClass;
            ClassDeclarationSyntax generatePartial = partialTargetClass;

            List<string> nameList = new List<string>();

            int bindDataAmount = this.generateData.objectInfo.bindDataList.Count;
            for (int i = 0; i < bindDataAmount; i++)
            {
                BindData bindData = this.generateData.objectInfo.bindDataList[i];
                Type type = bindData.GetTypeString().ToType();
                if (this.templateFieldInfo.FieldType != type) continue;
                nameList.Add(bindData.name);
            }

            int bindCollectionAmount = this.generateData.objectInfo.bindCollectionList.Count;
            for (int i = 0; i < bindCollectionAmount; i++)
            {
                BindCollection bindCollection = this.generateData.objectInfo.bindCollectionList[i];
                Type type = bindCollection.GetTypeString().ToType();
                if (this.templateFieldInfo.FieldType != type) continue;
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
                    DisposeName(targetDispose, name);
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

        void DisposeName(DisposeCotentData disposeCotentData, string name)
        {
            switch (disposeCotentData.generateContent)
            {
                case FieldDeclarationSyntax field:
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        string newName = name + variable.Identifier.ValueText;
                        TypeSyntax typeSyntax = field.Declaration.Type;
                        string typeName = GetUnqualifiedName(typeSyntax);
                        newName = NameHelper.SetName(newName, this.nameGenerateSetting.nameReplaceDataList);
                        newName = NameHelper.NameSettingByName(newName, typeName, this.scriptSetting.nameSetting);
                        newName = this.fieldNameDisposer.DisposeName(this.nameDisposeCentre, newName);
                        var newVariable = variable.WithIdentifier(SyntaxFactory.Identifier(newName));
                        field.ReplaceNode(variable, newVariable);
                    }
                    disposeCotentData.generateContent = field;
                    break;
                }
                case PropertyDeclarationSyntax property:
                {
                    string newName = name + property.Identifier.ValueText;
                    TypeSyntax typeSyntax = property.Type;
                    string typeName = GetUnqualifiedName(typeSyntax);
                    newName = NameHelper.SetName(newName, this.nameGenerateSetting.csharpNameGenerateSetting.propertyNameReplaceDataList);
                    newName = NameHelper.NameSettingByName(newName, typeName, this.scriptSetting.csharpScriptSetting.propertyNameSetting);
                    newName = propertyNameDisposer.DisposeName(this.nameDisposeCentre, newName);
                    property = property.WithIdentifier(SyntaxFactory.Identifier(newName));
                    disposeCotentData.generateContent = property;
                    break;
                }
                case MethodDeclarationSyntax method:
                {
                    string newName = name + method.Identifier.ValueText;
                    newName = NameHelper.SetName(newName, this.nameGenerateSetting.csharpNameGenerateSetting.methodNameReplaceDataList);
                    newName = NameHelper.NameSettingByName(newName, string.Empty, this.scriptSetting.csharpScriptSetting.methodNameSetting);
                    newName = this.methodNameDisposer.DisposeName(this.nameDisposeCentre, newName);
                    method = method.WithIdentifier(SyntaxFactory.Identifier(newName));
                    disposeCotentData.generateContent = method;
                    break;
                }
            }
        }

        private static string GetUnqualifiedName(TypeSyntax typeSyntax)
        {
            var nameSyntax = typeSyntax as NameSyntax;
            if (nameSyntax == null) { return typeSyntax.ToString(); }
            while (nameSyntax is QualifiedNameSyntax qualifiedNameSyntax) { nameSyntax = qualifiedNameSyntax.Right; }
            return nameSyntax.ToString();
        }

        private void DisposeField(FieldInfo fieldInfo)
        {
            if (fieldInfo == this.templateFieldInfo) return;
            fieldInfo.GetCustomAttribute<TemplateFieldAttribute>();

            Attribute[] attributes = fieldInfo.GetCustomAttributes().ToArray();

            string fieldName = fieldInfo.Name;
            FieldDeclarationSyntax fieldDeclarationSyntax = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldName);
            });

            if (fieldDeclarationSyntax == null) return;

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

            if (propertyDeclarationSyntax == null) return;

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

            if (methodDeclarationSyntax == null) return;

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