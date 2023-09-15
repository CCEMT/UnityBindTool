using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BindTool;
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

        public IRepetitionNameDisposer fieldNameDisposer;
        public IRepetitionNameDisposer propertyNameDisposer;
        public IRepetitionNameDisposer methodNameDisposer;

        public List<string> nameList;
        public List<TypeDisposeCotentData> typeDisposeDatas;

        public override void Dispose()
        {
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

            nameList = new List<string>();

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

            this.typeDisposeDatas = new List<TypeDisposeCotentData>();
            CSharpScriptSetting csharpScriptSetting = this.scriptSetting.csharpScriptSetting;
            fieldNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(this.scriptSetting.nameSetting.repetitionNameDispose);
            propertyNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(csharpScriptSetting.propertyNameSetting.repetitionNameDispose);
            methodNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(csharpScriptSetting.methodNameSetting.repetitionNameDispose);

            foreach (var fieldInfo in fieldInfos) DisposeTypeField(fieldInfo);

            PropertyInfo[] propertyInfos = templateType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos) DisposeTypeProperty(propertyInfo);

            MethodInfo[] methodInfos = templateType.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos) DisposeTypeMethod(methodInfo);

            int typeDataAmount = this.typeDisposeDatas.Count;
            for (int i = 0; i < typeDataAmount; i++)
            {
                TypeDisposeCotentData typeDisposeCotentData = this.typeDisposeDatas[i];
                Attribute[] attributes = typeDisposeCotentData.memberInfo.GetCustomAttributes().ToArray();

                TemplateDisposeHelper.Dispose<BaseCommonAttributeDispose>(this, typeDisposeCotentData, attributes);
                TemplateDisposeHelper.Dispose<BaseTypeAttributeDispose>(this, typeDisposeCotentData, attributes);
            }
        }

        public override void Generate()
        {
            ClassDeclarationSyntax generateMain = mainTargetClass;
            ClassDeclarationSyntax generatePartial = partialTargetClass;

            int amount = this.typeDisposeDatas.Count;
            for (int i = 0; i < amount; i++)
            {
                TypeDisposeCotentData typeDisposeData = this.typeDisposeDatas[i];

                int memberAmount = typeDisposeData.memberDeclarationSyntaxs.Count;
                for (int j = 0; j < memberAmount; j++)
                {
                    MemberDeclarationSyntax memberDeclarationSyntax = typeDisposeData.memberDeclarationSyntaxs[j];

                    AttributeSyntax autoGenerateAttributeAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(AutoGenerate).FullName));
                    memberDeclarationSyntax = memberDeclarationSyntax.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(autoGenerateAttributeAttribute));

                    if (typeDisposeData.generateTarget == this.mainTargetClass) { generateMain = generateMain.AddMembers(memberDeclarationSyntax); }
                    else { generatePartial = generatePartial.AddMembers(memberDeclarationSyntax); }
                }
            }

            mainTargetClass = generateMain;
            partialTargetClass = generatePartial;
        }

        void DisposeTypeField(FieldInfo fieldInfo)
        {
            if (fieldInfo == this.templateFieldInfo) return;

            string fieldName = fieldInfo.Name;
            FieldDeclarationSyntax fieldDeclarationSyntax = this.templateClass.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault((field) => {
                return field.Declaration.Variables.Any((variable) => variable.Identifier.ValueText == fieldName);
            });

            if (fieldDeclarationSyntax == null) return;

            AddTypeDisposeData(fieldInfo, fieldDeclarationSyntax);
        }

        void DisposeTypeProperty(PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;
            PropertyDeclarationSyntax propertyDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault((property) => property.Identifier.ValueText == propertyName);

            if (propertyDeclarationSyntax == null) return;

            AddTypeDisposeData(propertyInfo, propertyDeclarationSyntax);
        }

        void DisposeTypeMethod(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            MethodDeclarationSyntax methodDeclarationSyntax =
                this.templateClass.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault((method) => method.Identifier.ValueText == methodName);

            if (methodDeclarationSyntax == null) return;

            AddTypeDisposeData(methodInfo, methodDeclarationSyntax);
        }

        void AddTypeDisposeData(MemberInfo memberInfo, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            TypeDisposeCotentData typeDisposeData = new TypeDisposeCotentData();
            typeDisposeData.memberInfo = memberInfo;

            typeDisposeData.originalContent = memberDeclarationSyntax;

            int amount = nameList.Count;
            for (int i = 0; i < amount; i++)
            {
                string name = nameList[i];
                string templateFieldName = templateField.Declaration.Variables.First().Identifier.ValueText;
                IdentifierRewriter rewriter = new IdentifierRewriter(templateFieldName, name);
                MemberDeclarationSyntax addMember = (MemberDeclarationSyntax) rewriter.Visit(memberDeclarationSyntax);
                addMember = DisposeName(addMember, name);
                typeDisposeData.memberDeclarationSyntaxs.Add(addMember);
            }

            typeDisposeData.generateTarget = this.partialTargetClass ?? this.mainTargetClass;

            this.typeDisposeDatas.Add(typeDisposeData);
        }

        MemberDeclarationSyntax DisposeName(MemberDeclarationSyntax memberDeclarationSyntax, string name)
        {
            switch (memberDeclarationSyntax)
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
                    return field;
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

                    return property;
                }
                case MethodDeclarationSyntax method:
                {
                    string newName = name + method.Identifier.ValueText;
                    newName = NameHelper.SetName(newName, this.nameGenerateSetting.csharpNameGenerateSetting.methodNameReplaceDataList);
                    newName = NameHelper.NameSettingByName(newName, string.Empty, this.scriptSetting.csharpScriptSetting.methodNameSetting);
                    newName = this.methodNameDisposer.DisposeName(this.nameDisposeCentre, newName);
                    method = method.WithIdentifier(SyntaxFactory.Identifier(newName));
                    return method;
                }
            }
            return null;
        }

        private static string GetUnqualifiedName(TypeSyntax typeSyntax)
        {
            var nameSyntax = typeSyntax as NameSyntax;
            if (nameSyntax == null) { return typeSyntax.ToString(); }
            while (nameSyntax is QualifiedNameSyntax qualifiedNameSyntax) { nameSyntax = qualifiedNameSyntax.Right; }
            return nameSyntax.ToString();
        }
    }
}