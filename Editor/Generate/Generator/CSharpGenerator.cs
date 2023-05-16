using System.Collections.Generic;
using System.IO;
using System.Linq;
using BindTool;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

public partial class CSharpGenerator : IGenerator
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

            if (IsDelect(attributeListSyntax)) deleteNodeList.Add(node);
        }

        root = root.RemoveNodes(deleteNodeList, SyntaxRemoveOptions.KeepExteriorTrivia);
        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(filePath);
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    bool IsDelect(SyntaxList<AttributeListSyntax> attributeListSyntax)
    {
        foreach (AttributeListSyntax attribute in attributeListSyntax)
        {
            foreach (AttributeSyntax attributeAttribute in attribute.Attributes)
            {
                var typeName = attributeAttribute.Name.GetLastToken().ToString();
                if (! string.Equals(typeName, nameof(AutoGenerate))) continue;
                if (IsFixedContent(attributeAttribute)) continue;
                return true;
            }
        }
        return false;
    }

    bool IsFixedContent(AttributeSyntax attributeSyntax)
    {
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attributeSyntax.ArgumentList.Arguments;
        foreach (AttributeArgumentSyntax argument in arguments)
        {
            ExpressionSyntax expression = argument.Expression;
            if (expression is not IdentifierNameSyntax nameSyntax) continue;
            if (nameSyntax.Identifier.ValueText.Contains(nameof(AutoGenerateType.FixedContent))) return true;
        }
        return false;
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
        string bindMethodParameterName = CommonConst.DefaultBindMethodParameterName;
        generateData.getBindDataMethodName = bindMethodName;
        MethodDeclarationSyntax getMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(CommonConst.MethodVoid), bindMethodName);
        getMethod = getMethod.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        ParameterSyntax parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(bindMethodParameterName)).WithType(SyntaxFactory.ParseTypeName(typeof(BindComponents).FullName));
        getMethod = getMethod.AddParameterListParameters(parameter);

        AttributeSyntax autoGenerateAttributeAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(AutoGenerate).FullName));
        getMethod = getMethod.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(autoGenerateAttributeAttribute));

        SyntaxList<ExpressionStatementSyntax> getExpressionList = SyntaxFactory.List<ExpressionStatementSyntax>();

        int bindAmount = this.generateData.objectInfo.bindDataList.Count;
        for (int i = 0; i < bindAmount; i++)
        {
            BindData bindData = this.generateData.objectInfo.bindDataList[i];
            string typeString = bindData.GetTypeFullName();
            string fieldName = fieldNameDisposer.DisposeName(this.nameDisposeCentre, bindData.name);
            string propertyName = NameHelper.SetPropertyName(fieldName, this.selectSetting.nameGenerateSetting.csharpNameGenerateSetting);
            propertyName = NameHelper.NameSettingByName(propertyName, bindData, this.csharpScriptSetting.propertyNameSetting);
            propertyName = propertyNameDisposer.DisposeName(this.nameDisposeCentre, propertyName);

            TypeSyntax itemType = SyntaxFactory.ParseTypeName(typeString);
            FieldDeclarationSyntax field = GenerateField(itemType, fieldName);
            PropertyDeclarationSyntax property = GenerateProperty(itemType, propertyName, fieldName);
            ExpressionStatementSyntax expression = GenerateItemExpression(bindData, fieldName, bindMethodParameterName, i);

            bool isEditorAsset = typeString.Contains(CommonConst.UntiyEditorNameSpace);
            if (isEditorAsset)
            {
                field = field.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                field = field.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
                property = property.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                property = property.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
                expression = expression.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                expression = expression.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
            }
            targetClass = targetClass.AddMembers(field);
            targetClass = targetClass.AddMembers(property);
            getExpressionList = getExpressionList.Add(expression);
        }

        int collectionAmount = this.generateData.objectInfo.bindCollectionList.Count;
        for (int i = 0; i < collectionAmount; i++)
        {
            BindCollection collection = this.generateData.objectInfo.bindCollectionList[i];
            string typeString = collection.GetTypeString().GetVisitString();
            string fieldName = fieldNameDisposer.DisposeName(this.nameDisposeCentre, collection.name);
            string propertyName = NameHelper.SetPropertyName(fieldName, this.selectSetting.nameGenerateSetting.csharpNameGenerateSetting);
            propertyName = NameHelper.NameSettingByName(propertyName, collection, this.csharpScriptSetting.propertyNameSetting);
            propertyName = propertyNameDisposer.DisposeName(this.nameDisposeCentre, propertyName);

            TypeSyntax collectionType = GetCollectioinTypeSyntax(collection);
            FieldDeclarationSyntax field = GenerateField(collectionType, fieldName);
            PropertyDeclarationSyntax property = GenerateProperty(collectionType, propertyName, fieldName);
            ExpressionStatementSyntax expression = GenerateCollectionExpression(collectionType, fieldName, bindMethodParameterName, i);

            bool isEditorAsset = typeString.Contains(CommonConst.UntiyEditorNameSpace);
            if (isEditorAsset)
            {
                field = field.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                field = field.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
                property = property.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                property = property.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
                expression = expression.WithLeadingTrivia(SyntaxFactory.Trivia(ifNode));
                expression = expression.WithTrailingTrivia(SyntaxFactory.Trivia(endIfNode));
            }
            targetClass = targetClass.AddMembers(field);
            targetClass = targetClass.AddMembers(property);
            getExpressionList = getExpressionList.Add(expression);
        }

        BlockSyntax newBlock = SyntaxFactory.Block(getExpressionList);
        getMethod = getMethod.WithBody(newBlock);
        targetClass = targetClass.AddMembers(getMethod);

        root = root.ReplaceNode(oldNode, targetClass);
        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(filePath);
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    FieldDeclarationSyntax GenerateField(TypeSyntax typeSyntax, string fieldName)
    {
        VariableDeclarationSyntax variable = SyntaxFactory.VariableDeclaration(typeSyntax);
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

    PropertyDeclarationSyntax GenerateProperty(TypeSyntax typeSyntax, string propertyName, string fieldName)
    {
        IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
        MemberAccessExpressionSyntax fieldExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldSyntax);
        SyntaxKind propertyVisitKey = ObjectInfoHelper.VisitTypeToSyntaxKind(this.csharpScriptSetting.propertyVisitType);

        PropertyDeclarationSyntax property = SyntaxFactory.PropertyDeclaration(typeSyntax, propertyName);
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

    ExpressionStatementSyntax GenerateItemExpression(BindData bindData, string fieldName, string bindComponentsArgName, int index)
    {
        TypeSyntax typeSyntax = SyntaxFactory.ParseTypeName(bindData.GetTypeFullName());
        IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
        MemberAccessExpressionSyntax fieldExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldSyntax);

        IdentifierNameSyntax bindComponents = SyntaxFactory.IdentifierName(bindComponentsArgName);
        IdentifierNameSyntax bindDataList = SyntaxFactory.IdentifierName(nameof(BindComponents.bindDataList));
        MemberAccessExpressionSyntax acccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, bindComponents, bindDataList);
        ArgumentSyntax indexArg = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(index)));
        BracketedArgumentListSyntax indexArgList = SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(indexArg));

        ElementAccessExpressionSyntax arrayAccessExpression = SyntaxFactory.ElementAccessExpression(acccessExpression, indexArgList);

        CastExpressionSyntax rightExpression = SyntaxFactory.CastExpression(typeSyntax, arrayAccessExpression);

        AssignmentExpressionSyntax assignmentExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldExpression, rightExpression);

        return SyntaxFactory.ExpressionStatement(assignmentExpression);
    }

    ExpressionStatementSyntax GenerateCollectionExpression(TypeSyntax collectioinTypeSyntax, string fieldName, string bindComponentsArgName, int index)
    {
        IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
        MemberAccessExpressionSyntax fieldExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldSyntax);

        IdentifierNameSyntax bindComponents = SyntaxFactory.IdentifierName(bindComponentsArgName);
        IdentifierNameSyntax bindCollectionList = SyntaxFactory.IdentifierName(nameof(BindComponents.bindCollectionList));
        MemberAccessExpressionSyntax acccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, bindComponents, bindCollectionList);

        ArgumentSyntax indexArg = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(index)));
        BracketedArgumentListSyntax indexArgList = SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(indexArg));

        ElementAccessExpressionSyntax arrayAccessExpression = SyntaxFactory.ElementAccessExpression(acccessExpression, indexArgList);
        CastExpressionSyntax rightExpression = SyntaxFactory.CastExpression(collectioinTypeSyntax, arrayAccessExpression);

        AssignmentExpressionSyntax assignmentExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldExpression, rightExpression);

        return SyntaxFactory.ExpressionStatement(assignmentExpression);
    }

    TypeSyntax GetCollectioinTypeSyntax(BindCollection collection)
    {
        TypeSyntax typeSyntax = SyntaxFactory.ParseTypeName(collection.GetTypeFullNmae());
        TypeSyntax collectioinTypeSyntax = null;
        switch (collection.collectionType)
        {
            case CollectionType.Array:
            {
                ArrayRankSpecifierSyntax array = SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression()));
                collectioinTypeSyntax = SyntaxFactory.ArrayType(typeSyntax).WithRankSpecifiers(SyntaxFactory.SingletonList(array));
                break;
            }
            case CollectionType.List:
            {
                TypeArgumentListSyntax typeList = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(typeSyntax));
                collectioinTypeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier(typeof(List<>).FullName), typeList);
                break;
            }
            case CollectionType.Dictionary:
            {
                TypeSyntax stringSyntax = SyntaxFactory.ParseTypeName(typeof(string).FullName);
                SeparatedSyntaxList<TypeSyntax> typeList = SyntaxFactory.SeparatedList<TypeSyntax>();
                typeList = typeList.Add(stringSyntax);
                typeList = typeList.Add(typeSyntax);
                collectioinTypeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier(typeof(Dictionary<,>).FullName), SyntaxFactory.TypeArgumentList(typeList));
                break;
            }
        }
        return collectioinTypeSyntax;
    }
}