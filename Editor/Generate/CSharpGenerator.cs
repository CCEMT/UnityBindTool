using System.Collections.Generic;
using System.IO;
using System.Linq;
using BindTool;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

public class CSharpGenerator : IGenerator
{
    private MainSetting mainSetting;
    private ScriptSetting scriptSetting;
    private GenerateData generateData;
    private string scriptPath;

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

    }

    void GenerateNewFile()
    {
        string mainFilePath = scriptPath + $"{generateData.newScriptName}{CommonConst.CSharpFileSuffix}";
        if (this.scriptSetting.isGeneratePartial)
        {
            string partialFilePath = scriptPath + $"{generateData.newScriptName}.{this.scriptSetting.partialName}{CommonConst.CSharpFileSuffix}";
            GeneratePartialFile(mainFilePath, partialFilePath);
            GenerateFixedContent(partialFilePath);
            GenerateAddition(mainFilePath, partialFilePath, true);
        }
        else
        {
            GenerateMainFile(mainFilePath);
            GenerateFixedContent(mainFilePath);
            GenerateAddition(mainFilePath, string.Empty, false);
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

        ClassDeclarationSyntax[] classDeclarationSyntaxes =
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
        if (classDeclarationSyntaxes.Length == 0)
        {
            Debug.LogError("原主文件未包含对应的类");
            return;
        }

        ClassDeclarationSyntax targetClass = classDeclarationSyntaxes.First();

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
        classDeclaration.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList(baseType)));
        classDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        if (isPartial) { classDeclaration.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword))); }
        classDeclaration.NormalizeWhitespace();

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit();
        compilationUnit.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeNameSpace)));

        if (this.scriptSetting.isSpecifyNamespace == false) { compilationUnit.AddMembers(classDeclaration); }
        else
        {
            NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(scriptSetting.useNamespace)).NormalizeWhitespace();
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);
            compilationUnit.AddMembers(namespaceDeclaration);
        }

        string code = compilationUnit.NormalizeWhitespace().ToFullString();
        StreamWriter mainWriter = File.CreateText(filePath);
        mainWriter.Write(code);
        mainWriter.Close();
    }

    void ModifyFile()
    {
        string mainFilePath = scriptPath + $"{generateData.newScriptName}{CommonConst.CSharpFileSuffix}";
        if (this.scriptSetting.isSavaOldScript) { SavaOldScriptHelper.GenerateOldScriptFile(mainFilePath); }
        DeleteOldContent(mainFilePath);
        if (this.scriptSetting.isGeneratePartial)
        {
            string partialFilePath = scriptPath + $"{generateData.newScriptName}.{this.scriptSetting.partialName}{CommonConst.CSharpFileSuffix}";
            if (this.scriptSetting.isSavaOldScript) { SavaOldScriptHelper.GenerateOldScriptFile(partialFilePath); }
            DeleteOldContent(partialFilePath);
            GenerateFixedContent(partialFilePath);
            GenerateAddition(mainFilePath, partialFilePath, true);
        }
        else
        {
            GenerateFixedContent(mainFilePath);
            GenerateAddition(mainFilePath, string.Empty, false);
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

        root.RemoveNodes(deleteNodeList, SyntaxRemoveOptions.KeepExteriorTrivia);

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

        ClassDeclarationSyntax[] classDeclarationSyntaxes =
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
        if (classDeclarationSyntaxes.Length == 0)
        {
            Debug.LogError("原主文件未包含对应的类");
            return;
        }

        ClassDeclarationSyntax targetClass = classDeclarationSyntaxes.First();
        
    }

    void GenerateAddition(string mainFilePath, string partialFilePath, bool isPartial)
    {
        string targetFilePath = isPartial ? partialFilePath : mainFilePath;
        
        string code = File.ReadAllText(targetFilePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        ClassDeclarationSyntax[] classDeclarationSyntaxes =
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
        if (classDeclarationSyntaxes.Length == 0)
        {
            Debug.LogError("原主文件未包含对应的类");
            return;
        }

        ClassDeclarationSyntax targetClass = classDeclarationSyntaxes.First();
    }
}