using System.Collections.Generic;
using System.IO;
using System.Linq;
using BindTool;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEngine;

public class CSharpGenerator : IGenerator
{
    private CompositionSetting selectSetting;
    private ScriptSetting scriptSetting;
    private CSharpScriptSetting csharpScriptSetting;

    private GenerateData generateData;

    private string scriptPath;

    private NameDisposeCentre nameDisposeCentre;

    public void Init(CompositionSetting setting, GenerateData data)
    {
        this.selectSetting = setting;
        this.generateData = data;
        scriptSetting = this.selectSetting.scriptSetting;
        csharpScriptSetting = this.scriptSetting.csharpScriptSetting;
        nameDisposeCentre = new NameDisposeCentre();
    }

    public void Write(string scriptPath)
    {
        this.scriptPath = scriptPath;
        if (csharpScriptSetting.isGenerateNew) { GenerateNewFile(); }
        else { ModifyFile(); }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }

    void GenerateNewFile()
    {
        generateData.objectInfo.typeString.typeName = generateData.newScriptName;
        generateData.objectInfo.typeString.typeNameSpace = csharpScriptSetting.useNamespace;
        generateData.objectInfo.typeString.assemblyName = csharpScriptSetting.createScriptAssembly;

        nameDisposeCentre.useNames.Add(generateData.newScriptName);

        string mainFilePath = scriptPath + $"{generateData.newScriptName}{CommonConst.CSharpFileSuffix}";
        if (this.csharpScriptSetting.isGeneratePartial)
        {
            string partialFilePath = scriptPath + $"{generateData.newScriptName}.{this.csharpScriptSetting.partialName}{CommonConst.CSharpFileSuffix}";
            GeneratePartialFile(mainFilePath, partialFilePath);
            GenerateFixedContent(partialFilePath);
            GenerateTemplateContent(mainFilePath, partialFilePath, true);
        }
        else
        {
            GenerateMainFile(mainFilePath);
            GenerateFixedContent(mainFilePath);
            GenerateTemplateContent(mainFilePath, string.Empty, false);
        }
    }

    void GeneratePartialFile(string mainFilePath, string partialFilePath)
    {
        if (File.Exists(mainFilePath)) { AddPartial(mainFilePath); }
        else { GenerateFile(mainFilePath, true); }

        if (File.Exists(partialFilePath) == false) { GenerateFile(partialFilePath, true); }
    }

    void AddPartial(string filePath)
    {
        string code = File.ReadAllText(filePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        ClassDeclarationSyntax[] classDeclarationSyntaxs =
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
        if (classDeclarationSyntaxs.Length == 0)
        {
            Debug.LogError("原主文件未包含对应的类");
            return;
        }

        ClassDeclarationSyntax targetClass = classDeclarationSyntaxs.First();
        ClassDeclarationSyntax oldNode = targetClass;

        bool isPartial = targetClass.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (isPartial) { return; }
        targetClass = targetClass.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

        root = root.ReplaceNode(oldNode, targetClass);
        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(filePath);
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    void GenerateMainFile(string mainFilePath)
    {
        if (File.Exists(mainFilePath) == false) { GenerateFile(mainFilePath, false); }
    }

    void GenerateFile(string filePath, bool isPartial)
    {
        string inheritContent = typeof(MonoBehaviour).FullName;
        if (string.IsNullOrEmpty(csharpScriptSetting.inheritClass.typeName) == false) { inheritContent = csharpScriptSetting.inheritClass.GetVisitString(); }

        BaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(inheritContent));

        ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration(generateData.newScriptName);
        classDeclaration = classDeclaration.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList(baseType)));
        classDeclaration = classDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        if (isPartial) { classDeclaration = classDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword))); }

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit();

        if (this.csharpScriptSetting.isSpecifyNamespace == false) { compilationUnit = compilationUnit.AddMembers(classDeclaration); }
        else
        {
            NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(csharpScriptSetting.useNamespace)).NormalizeWhitespace();
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);
            compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);
        }

        compilationUnit = compilationUnit.NormalizeWhitespace();

        SyntaxTree tree = SyntaxFactory.SyntaxTree(compilationUnit);
        string code = tree.ToString();

        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(code);
        mainWriter.Close();
    }

    void ModifyFile()
    {
        generateData.mergeTypeString = generateData.objectInfo.typeString;
        string mainFilePath = scriptPath + $"{generateData.newScriptName}{CommonConst.CSharpFileSuffix}";
        if (this.scriptSetting.isSavaOldScript)
        {
            string directoryName = SavaOldScriptHelper.GetDirectoryNameByGenerateData(this.generateData);
            SavaOldScriptHelper.GenerateOldScriptFile(this.scriptSetting, mainFilePath, directoryName);
        }
        DeleteOldContent(mainFilePath);
        if (this.csharpScriptSetting.isGeneratePartial)
        {
            string partialFilePath = scriptPath + $"{generateData.newScriptName}.{this.csharpScriptSetting.partialName}{CommonConst.CSharpFileSuffix}";
            if (this.scriptSetting.isSavaOldScript)
            {
                string directoryName = SavaOldScriptHelper.GetDirectoryNameByGenerateData(this.generateData);
                SavaOldScriptHelper.GenerateOldScriptFile(this.scriptSetting, partialFilePath, directoryName);
            }
            DeleteOldContent(partialFilePath);
            GenerateFixedContent(partialFilePath);
            GenerateTemplateContent(mainFilePath, partialFilePath, true);
        }
        else
        {
            GenerateFixedContent(mainFilePath);
            GenerateTemplateContent(mainFilePath, string.Empty, false);
        }
    }

    void DeleteOldContent(string filePath)
    {
        string code = File.ReadAllText(filePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        List<SyntaxNode> deleteNodeList = new List<SyntaxNode>();

        SyntaxNode[] syntaxNodes = root.DescendantNodes().Where((t) => t is FieldDeclarationSyntax or MethodDeclarationSyntax or PropertyDeclarationSyntax).ToArray();
        int amount = syntaxNodes.Length;
        for (int i = 0; i < amount; i++)
        {
            SyntaxNode node = syntaxNodes[i];
            SyntaxList<AttributeListSyntax> attributeListSyntax = default;
            switch (node)
            {
                case FieldDeclarationSyntax field:
                    attributeListSyntax = field.AttributeLists;
                    break;
                case MethodDeclarationSyntax method:
                    attributeListSyntax = method.AttributeLists;
                    break;
                case PropertyDeclarationSyntax property:
                    attributeListSyntax = property.AttributeLists;
                    break;
            }

            bool isFind = false;
            foreach (AttributeListSyntax attribute in attributeListSyntax)
            {
                if (isFind) { break; }
                foreach (AttributeSyntax attributeAttribute in attribute.Attributes)
                {
                    var typeName = attributeAttribute.Name.GetLastToken().ToString();
                    if (! string.Equals(typeName, nameof(AutoGenerate))) continue;
                    isFind = true;
                    deleteNodeList.Add(node);
                    break;
                }
            }
        }

        root = root.RemoveNodes(deleteNodeList, SyntaxRemoveOptions.KeepExteriorTrivia);
        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(filePath);
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    void GenerateFixedContent(string filePath)
    {
        string code = File.ReadAllText(filePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        ClassDeclarationSyntax[] classDeclarationSyntaxs =
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
        if (classDeclarationSyntaxs.Length == 0)
        {
            Debug.LogError("原主文件未包含对应的类");
            return;
        }

        ClassDeclarationSyntax targetClass = classDeclarationSyntaxs.First();
        ClassDeclarationSyntax oldNode = targetClass;

        IRepetitionNameDisposer fieldNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(this.scriptSetting.nameSetting.repetitionNameDispose);
        IRepetitionNameDisposer propertyNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(this.csharpScriptSetting.propertyNameSetting.repetitionNameDispose);

        IfDirectiveTriviaSyntax ifNode = SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName(CommonConst.UnityEditorAcer), true, false, false);
        EndIfDirectiveTriviaSyntax endIfNode = SyntaxFactory.EndIfDirectiveTrivia(true);

        string bindMethodName = CommonConst.DefaultBindMethodName;
        generateData.getBindDataMethodName = bindMethodName;
        MethodDeclarationSyntax getMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(CommonConst.MethodVoid), bindMethodName);
        getMethod = getMethod.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        AttributeSyntax autoGenerateAttributeAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(AutoGenerate).FullName));
        getMethod = getMethod.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(autoGenerateAttributeAttribute));

        SyntaxList<ExpressionStatementSyntax> componentList = SyntaxFactory.List<ExpressionStatementSyntax>();
        int componentBindAmount = generateData.objectInfo.gameObjectBindInfoList.Count;
        for (int i = 0; i < componentBindAmount; i++)
        {
            ComponentBindInfo componentInfo = generateData.objectInfo.gameObjectBindInfoList[i];
            string typeString = componentInfo.GetTypeString().GetVisitString();
            string fieldName = fieldNameDisposer.DisposeName(this.nameDisposeCentre, componentInfo.name);
            string propertyName = NameHelper.SetPropertyName(fieldName, this.selectSetting.nameGenerateSetting.csharpNameGenerateSetting);
            propertyName = NameHelper.NameSettingByName(propertyName, componentInfo, this.csharpScriptSetting.propertyNameSetting);
            propertyName = propertyNameDisposer.DisposeName(this.nameDisposeCentre, propertyName);

            FieldDeclarationSyntax field = GenerateField(typeString, fieldName);
            targetClass = targetClass.AddMembers(field);

            PropertyDeclarationSyntax property = GenerateProperty(typeString, propertyName, fieldName);
            targetClass = targetClass.AddMembers(property);

            ExpressionStatementSyntax expression = GenerateComponentExpression(componentInfo, typeString, fieldName);
            componentList = componentList.Add(expression);
        }

        SyntaxList<ExpressionStatementSyntax> dataList = SyntaxFactory.List<ExpressionStatementSyntax>();
        int dataInfoAmount = this.generateData.objectInfo.dataBindInfoList.Count;
        for (int i = 0; i < dataInfoAmount; i++)
        {
            DataBindInfo dataInfo = this.generateData.objectInfo.dataBindInfoList[i];

            string typeString = dataInfo.typeString.GetVisitString();
            string fieldName = fieldNameDisposer.DisposeName(this.nameDisposeCentre, dataInfo.name);
            string propertyName = NameHelper.SetPropertyName(fieldName, this.selectSetting.nameGenerateSetting.csharpNameGenerateSetting);
            propertyName = NameHelper.NameSettingByName(propertyName, dataInfo, this.csharpScriptSetting.propertyNameSetting);
            propertyName = propertyNameDisposer.DisposeName(this.nameDisposeCentre, propertyName);

            bool isEditorAsset = typeString.Contains(CommonConst.UntiyEditorNameSpace);

            FieldDeclarationSyntax field = GenerateField(typeString, fieldName);
            PropertyDeclarationSyntax property = GenerateProperty(typeString, propertyName, fieldName);

            if (isEditorAsset)
            {
                field = field.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                field = field.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
                property = property.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                property = property.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
            }

            targetClass = targetClass.AddMembers(property);
            targetClass = targetClass.AddMembers(field);

            ExpressionStatementSyntax expression = GenerateDataExpression(dataInfo, typeString, fieldName);
            dataList = dataList.Add(expression);

            if (isEditorAsset) { Debug.LogWarning($"警告：绑定数据为UntiyEditor中的类型,这将发布后失效：{dataInfo.name}"); }
        }

        if (dataList.Count > 0)
        {
            ExpressionStatementSyntax rawFirst = dataList.First();
            ExpressionStatementSyntax first = rawFirst.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
            dataList = dataList.Replace(rawFirst, first);

            ExpressionStatementSyntax rawLast = dataList.Last();
            ExpressionStatementSyntax last = rawLast.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
            dataList = dataList.Replace(rawLast, last);
        }

        SyntaxList<ExpressionStatementSyntax> newList = componentList.AddRange(dataList);
        BlockSyntax newBlock = SyntaxFactory.Block(newList);
        getMethod = getMethod.WithBody(newBlock);
        targetClass = targetClass.AddMembers(getMethod);

        root = root.ReplaceNode(oldNode, targetClass);
        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(filePath);
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    FieldDeclarationSyntax GenerateField(string typeString, string fieldName)
    {
        VariableDeclarationSyntax variable = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeString));
        variable = variable.AddVariables(SyntaxFactory.VariableDeclarator(fieldName));

        SyntaxKind fieldVisitKey = ObjectInfoHelper.VisitTypeToSyntaxKind(this.csharpScriptSetting.variableVisitType);
        FieldDeclarationSyntax field = SyntaxFactory.FieldDeclaration(variable).AddModifiers(SyntaxFactory.Token(fieldVisitKey));
        AttributeListSyntax attributeList = SyntaxFactory.AttributeList();
        AttributeSyntax serializeFieldAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(SerializeField).FullName));
        AttributeSyntax autoGenerateAttributeAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(AutoGenerate).FullName));

        IdentifierNameSyntax enumType = SyntaxFactory.IdentifierName(typeof(AutoGenerateType).FullName);
        IdentifierNameSyntax enumOption = SyntaxFactory.IdentifierName(nameof(AutoGenerateType.OriginalField));
        MemberAccessExpressionSyntax enumAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, enumType, enumOption);
        AttributeArgumentSyntax autoGenerateArg = SyntaxFactory.AttributeArgument(enumAccess);
        autoGenerateAttributeAttribute = autoGenerateAttributeAttribute.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(autoGenerateArg)));

        attributeList = attributeList.AddAttributes(serializeFieldAttribute, autoGenerateAttributeAttribute);
        field = field.AddAttributeLists(attributeList);
        return field;
    }

    PropertyDeclarationSyntax GenerateProperty(string typeString, string propertyName, string fieldName)
    {
        IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
        MemberAccessExpressionSyntax fieldExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldSyntax);
        SyntaxKind propertyVisitKey = ObjectInfoHelper.VisitTypeToSyntaxKind(this.csharpScriptSetting.propertyVisitType);

        PropertyDeclarationSyntax property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(typeString), propertyName);
        property = property.AddModifiers(SyntaxFactory.Token(propertyVisitKey));

        AttributeSyntax autoGenerateAttributeAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(AutoGenerate).FullName));
        property = property.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(autoGenerateAttributeAttribute));

        BlockSyntax getBlock = SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ReturnStatement(fieldExpression)));
        AccessorDeclarationSyntax get = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, getBlock);

        IdentifierNameSyntax propertyValueSyntax = SyntaxFactory.IdentifierName(CommonConst.PropertyValue);
        AssignmentExpressionSyntax setExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldExpression, propertyValueSyntax);
        BlockSyntax setBlock = SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ExpressionStatement(setExpression)));
        AccessorDeclarationSyntax set = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, setBlock);

        switch (this.csharpScriptSetting.propertyType)
        {
            case PropertyType.Set:
                property = property.AddAccessorListAccessors(set);
                break;
            case PropertyType.Get:
                property = property.AddAccessorListAccessors(get);
                break;
            case PropertyType.SetAndGet:
                property = property.AddAccessorListAccessors(get, set);
                break;
        }

        return property;
    }

    ExpressionStatementSyntax GenerateComponentExpression(ComponentBindInfo componentInfo, string typeString, string fieldName)
    {
        IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
        MemberAccessExpressionSyntax fieldExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldSyntax);

        string findPath = CommonTools.GetWholePath(componentInfo.instanceObject.transform, generateData.bindObject);

        LiteralExpressionSyntax stringArg = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(findPath));
        ArgumentListSyntax memberArg = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(stringArg)));

        IdentifierNameSyntax transformName = SyntaxFactory.IdentifierName(nameof(Component.transform));
        MemberAccessExpressionSyntax baseMmember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.BaseExpression(), transformName);
        MemberAccessExpressionSyntax findMember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, baseMmember, SyntaxFactory.IdentifierName(nameof(Transform.Find)));
        InvocationExpressionSyntax findExpression = SyntaxFactory.InvocationExpression(findMember, memberArg);

        ExpressionSyntax expression = null;
        if (componentInfo.instanceObject == generateData.bindObject)
        {
            if (componentInfo.GetTypeString().ToType() == typeof(GameObject))
            {
                IdentifierNameSyntax gameObjectName = SyntaxFactory.IdentifierName(nameof(Component.gameObject));
                MemberAccessExpressionSyntax baseGameObjectMmember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.BaseExpression(), gameObjectName);
                expression = baseGameObjectMmember;
            }
            else
            {
                TypeArgumentListSyntax typeArgs = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ParseTypeName(typeString)));
                GenericNameSyntax getComponent = SyntaxFactory.GenericName(SyntaxFactory.Identifier(nameof(Component.GetComponent))).WithTypeArgumentList(typeArgs);
                expression = SyntaxFactory.InvocationExpression(getComponent);
            }
        }
        else
        {
            if (componentInfo.GetTypeString().ToType() == typeof(GameObject))
            {
                IdentifierNameSyntax gameObjectName = SyntaxFactory.IdentifierName(nameof(Component.gameObject));
                expression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, findExpression, gameObjectName);
            }
            else
            {
                TypeArgumentListSyntax typeArgs = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ParseTypeName(typeString)));
                GenericNameSyntax getComponent = SyntaxFactory.GenericName(SyntaxFactory.Identifier(nameof(Component.GetComponent))).WithTypeArgumentList(typeArgs);
                MemberAccessExpressionSyntax target = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, findExpression, getComponent);
                expression = SyntaxFactory.InvocationExpression(target);
            }
        }

        AssignmentExpressionSyntax assignmentexpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldExpression, expression);
        return SyntaxFactory.ExpressionStatement(assignmentexpression);
    }

    ExpressionStatementSyntax GenerateDataExpression(DataBindInfo dataInfo, string typeString, string fieldName)
    {
        IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
        MemberAccessExpressionSyntax fieldExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldSyntax);
        string findPath = AssetDatabase.GetAssetPath(dataInfo.bindObject);

        LiteralExpressionSyntax stringArg = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(findPath));
        ArgumentListSyntax memberArg = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(stringArg)));

        IdentifierNameSyntax ediotrName = SyntaxFactory.IdentifierName(nameof(UnityEditor));
        IdentifierNameSyntax assetDatabaseName = SyntaxFactory.IdentifierName(nameof(AssetDatabase));
        MemberAccessExpressionSyntax findMember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ediotrName, assetDatabaseName);

        TypeArgumentListSyntax typeArgs = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ParseTypeName(typeString)));
        GenericNameSyntax getComponent = SyntaxFactory.GenericName(SyntaxFactory.Identifier(nameof(AssetDatabase.LoadAssetAtPath))).WithTypeArgumentList(typeArgs);
        MemberAccessExpressionSyntax target = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, findMember, getComponent);
        ExpressionSyntax expression = SyntaxFactory.InvocationExpression(target, memberArg);

        AssignmentExpressionSyntax assignmentexpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldExpression, expression);
        return SyntaxFactory.ExpressionStatement(assignmentexpression);
    }

    void GenerateTemplateContent(string mainFilePath, string partialFilePath, bool isPartial)
    {
        string targetFilePath = isPartial ? partialFilePath : mainFilePath;

        string code = File.ReadAllText(targetFilePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        ClassDeclarationSyntax[] classDeclarationSyntaxs =
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
        if (classDeclarationSyntaxs.Length == 0)
        {
            Debug.LogError("原主文件未包含对应的类");
            return;
        }

        ClassDeclarationSyntax targetClass = classDeclarationSyntaxs.First();
        ClassDeclarationSyntax oldNode = targetClass;

    }
}