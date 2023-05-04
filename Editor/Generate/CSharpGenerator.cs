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
    private MainSetting mainSetting;
    private ScriptSetting scriptSetting;

    private GenerateData generateData;

    private string scriptPath;

    private Dictionary<string, int> rawNames; //key=名称 ,value=使用次数
    private List<string> useNames;

    public void Init(MainSetting setting, GenerateData data)
    {
        this.mainSetting = setting;
        this.generateData = data;
        scriptSetting = mainSetting.selectScriptSetting;
    }

    public void Write(string scriptPath)
    {
        this.scriptPath = scriptPath;
        if (scriptSetting.isGenerateNew) { GenerateNewFile(); }
        else { ModifyFile(); }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }

    void GenerateNewFile()
    {
        string mainFilePath = scriptPath + $"{generateData.newScriptName}{CommonConst.CSharpFileSuffix}";
        if (this.scriptSetting.isGeneratePartial)
        {
            string partialFilePath = scriptPath + $"{generateData.newScriptName}.{this.scriptSetting.partialName}{CommonConst.CSharpFileSuffix}";
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

        bool isPartial = targetClass.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (isPartial) { return; }
        targetClass.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

        ClassDeclarationSyntax oldNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName));
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
        string typeNameSpace = nameof(UnityEngine);
        string inheritContent = nameof(MonoBehaviour);
        if (string.IsNullOrEmpty(scriptSetting.inheritClass.typeName) == false)
        {
            inheritContent = scriptSetting.inheritClass.typeName;
            typeNameSpace = this.scriptSetting.inheritClass.typeNameSpace;
        }

        BaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(inheritContent));

        ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration(generateData.newScriptName);
        classDeclaration = classDeclaration.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList(baseType)));
        classDeclaration = classDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        if (isPartial) { classDeclaration = classDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword))); }

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit();
        compilationUnit = compilationUnit.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeNameSpace)));

        if (this.scriptSetting.isSpecifyNamespace == false) { compilationUnit = compilationUnit.AddMembers(classDeclaration); }
        else
        {
            NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(scriptSetting.useNamespace)).NormalizeWhitespace();
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
        string mainFilePath = scriptPath + $"{generateData.newScriptName}{CommonConst.CSharpFileSuffix}";
        if (this.scriptSetting.isSavaOldScript)
        {
            string directoryName = SavaOldScriptHelper.GetDirectoryNameByGenerateData(this.generateData);
            SavaOldScriptHelper.GenerateOldScriptFile(mainFilePath, directoryName);
        }
        DeleteOldContent(mainFilePath);
        if (this.scriptSetting.isGeneratePartial)
        {
            string partialFilePath = scriptPath + $"{generateData.newScriptName}.{this.scriptSetting.partialName}{CommonConst.CSharpFileSuffix}";
            if (this.scriptSetting.isSavaOldScript)
            {
                string directoryName = SavaOldScriptHelper.GetDirectoryNameByGenerateData(this.generateData);
                SavaOldScriptHelper.GenerateOldScriptFile(partialFilePath, directoryName);
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
                    if (! string.Equals(typeName, nameof(AutoGenerateAttribute))) continue;
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

        string bindMethodName = CommonConst.DefaultBindMethodName;
        generateData.getBindDataMethodName = bindMethodName;
        var getMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(CSharpGeneratorHelper.MethodVoid), bindMethodName);
        getMethod = getMethod.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var list = SyntaxFactory.List<ExpressionStatementSyntax>();

        int componentBindAmount = generateData.objectInfo.gameObjectBindInfoList.Count;
        for (int i = 0; i < componentBindAmount; i++)
        {
            ComponentBindInfo componentInfo = generateData.objectInfo.gameObjectBindInfoList[i];

            string typeNameSpace = componentInfo.GetTypeString().typeNameSpace;
            string typeName = componentInfo.GetTypeString().typeName;
            string fieldName = componentInfo.name;

            //字段
            IdentifierNameSyntax fieldSyntax = SyntaxFactory.IdentifierName(fieldName);
            SyntaxKind fieldVisitKey = ObjectInfoHelper.VisitTypeToSyntaxKind(this.scriptSetting.variableVisitType);

            VariableDeclarationSyntax variable = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeName));
            variable = variable.AddVariables(SyntaxFactory.VariableDeclarator(fieldName));

            FieldDeclarationSyntax field = SyntaxFactory.FieldDeclaration(variable).AddModifiers(SyntaxFactory.Token(fieldVisitKey));
            field.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(nameof(SerializeField)))));

            targetClass = targetClass.AddMembers(field);

            //属性
            string propertyName = typeName;
            SyntaxKind propertyVisitKey = ObjectInfoHelper.VisitTypeToSyntaxKind(this.scriptSetting.variableVisitType);

            PropertyDeclarationSyntax property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(typeName), propertyName);
            property = property.AddModifiers(SyntaxFactory.Token(propertyVisitKey));

            BlockSyntax getBlock = SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName))));
            AccessorDeclarationSyntax get = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, getBlock);

            IdentifierNameSyntax propertyValueSyntax = SyntaxFactory.IdentifierName(CSharpGeneratorHelper.PropertyValue);
            AssignmentExpressionSyntax setExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldSyntax, propertyValueSyntax);
            BlockSyntax setBlock = SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ExpressionStatement(setExpression)));
            AccessorDeclarationSyntax set = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, setBlock);

            property = property.AddAccessorListAccessors(get, set);
            targetClass = targetClass.AddMembers(property);

            //赋值方法
            string findPath = CommonTools.GetWholePath(componentInfo.instanceObject.transform, generateData.bindObject);

            var stringArg = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(findPath));
            var memberArg = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(stringArg)));

            var baseMmember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.BaseExpression(), SyntaxFactory.IdentifierName(nameof(Component.transform)));
            var findMember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, baseMmember, SyntaxFactory.IdentifierName(nameof(Transform.Find)));

            var findExpression = SyntaxFactory.InvocationExpression(findMember, memberArg);

            ExpressionSyntax expression = null;

            if (componentInfo.GetTypeString().ToType() == typeof(GameObject))
            {
                expression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, findExpression, SyntaxFactory.IdentifierName(nameof(Component.gameObject)));
            }
            else
            {
                var arg = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(typeNameSpace), SyntaxFactory.IdentifierName(typeName));
                var args = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(arg));
                var getComponent = SyntaxFactory.GenericName(SyntaxFactory.Identifier(nameof(Component.GetComponent))).WithTypeArgumentList(args);
                var target = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, findExpression, getComponent);
                expression = SyntaxFactory.InvocationExpression(target);
            }

            var assignmentexpression = SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                fieldSyntax,
                expression
            );

            list = list.Add(SyntaxFactory.ExpressionStatement(assignmentexpression));
        }

        getMethod = getMethod.WithBody(SyntaxFactory.Block(list));

        targetClass = targetClass.AddMembers(getMethod);

        ClassDeclarationSyntax oldNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName));
        root = root.ReplaceNode(oldNode, targetClass);

        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(filePath);
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    void GenerateField()
    {
        
    }

    void GenerateProperty()
    {
        
    }

    void GenerateComponentMethod()
    {
        
    }

    void GenerateDataMethod()
    {
        
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
    }
}