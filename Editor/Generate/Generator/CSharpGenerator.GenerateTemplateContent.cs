using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityBindTool;
using UnityEngine;

public partial class CSharpGenerator
{
    void GenerateTemplateContent(string mainFilePath, string partialFilePath, bool isPartial)
    {
        ClassDeclarationSyntax mainClass = default;
        CompilationUnitSyntax mainRoot = default;

        ClassDeclarationSyntax partialClass = default;
        CompilationUnitSyntax partialRoot = default;

        if (isPartial)
        {
            string partialPath = partialFilePath;
            string partialCode = File.ReadAllText(partialPath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(partialCode);
            partialRoot = tree.GetCompilationUnitRoot();
            ClassDeclarationSyntax[] classDeclarationSyntaxs = partialRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
            classDeclarationSyntaxs = classDeclarationSyntaxs.Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();
            if (classDeclarationSyntaxs.Length == 0)
            {
                Debug.LogError("partial文件未包含对应的类");
                return;
            }

            partialClass = classDeclarationSyntaxs.First();
        }

        string mainPath = mainFilePath;
        string mainCode = File.ReadAllText(mainPath);
        SyntaxTree mainTree = CSharpSyntaxTree.ParseText(mainCode);
        mainRoot = mainTree.GetCompilationUnitRoot();
        ClassDeclarationSyntax[] mainClassDeclarationSyntaxs = mainRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
        mainClassDeclarationSyntaxs = mainClassDeclarationSyntaxs.Where((c) => string.Equals(c.Identifier.ValueText, this.generateData.newScriptName)).ToArray();

        if (mainClassDeclarationSyntaxs.Length == 0)
        {
            Debug.LogError("主文件未包含对应的类");
            return;
        }

        mainClass = mainClassDeclarationSyntaxs.First();

        ClassDeclarationSyntax oldMainClass = mainClass;
        ClassDeclarationSyntax oldPartialClass = partialClass;

        SyntaxList<ClassDeclarationSyntax> templateClassList = new SyntaxList<ClassDeclarationSyntax>();

        int templateAmount = this.csharpScriptSetting.templateDataList.Count;
        for (int i = 0; i < templateAmount; i++)
        {
            TextAsset templateData = this.csharpScriptSetting.templateDataList[i];
            SyntaxTree templateTree = CSharpSyntaxTree.ParseText(templateData.text);
            CompilationUnitSyntax templateRoot = templateTree.GetCompilationUnitRoot();
            ClassDeclarationSyntax[] templateClassDeclarationSyntaxs = templateRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
            templateClassList = templateClassList.AddRange(templateClassDeclarationSyntaxs);
        }

        int amount = templateClassList.Count;
        for (int i = 0; i < amount; i++)
        {
            ClassDeclarationSyntax templateClass = templateClassList[i];
            CSharpTemplateType templateType = GetCSharpTemplateType(templateClass);
            BaseTemplateDispose templateDispose = CSharpTemplateDisposeFactory.GetTemplateDispose(templateType);
            templateDispose.generateData = this.generateData;
            templateDispose.templateClass = templateClass;
            templateDispose.mainTargetClass = mainClass;
            templateDispose.partialTargetClass = partialClass;
            templateDispose.Dispose();
            templateDispose.Generate();
        }

        if (isPartial)
        {
            partialRoot = partialRoot.ReplaceNode(oldPartialClass, partialClass);
            string newPartialCode = partialRoot.NormalizeWhitespace().ToFullString();

            File.Delete(partialFilePath);
            StreamWriter partialWriter = File.CreateText(partialFilePath);
            partialWriter.Write(newPartialCode);
            partialWriter.Close();
        }

        mainRoot = mainRoot.ReplaceNode(oldMainClass, mainClass);
        string newCode = mainRoot.NormalizeWhitespace().ToFullString();

        File.Delete(mainFilePath);
        StreamWriter mainWriter = File.CreateText(mainFilePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }

    CSharpTemplateType GetCSharpTemplateType(ClassDeclarationSyntax classDeclarationSyntax)
    {
        SyntaxList<AttributeListSyntax> attributeListSyntaxes = classDeclarationSyntax.AttributeLists;
        foreach (AttributeListSyntax attributeListSyntax in attributeListSyntaxes)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                var typeName = attributeSyntax.Name.GetLastToken().ToString();
                if (! string.Equals(typeName, nameof(TemplateClass))) continue;
                if (IsTarget(attributeSyntax, nameof(CSharpTemplateType.Common))) return CSharpTemplateType.Common;
                if (IsTarget(attributeSyntax, nameof(CSharpTemplateType.Type))) return CSharpTemplateType.Type;
            }
        }
        return default;
    }

    bool IsTarget(AttributeSyntax attributeSyntax, string identifierName)
    {
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attributeSyntax.ArgumentList.Arguments;
        foreach (AttributeArgumentSyntax argument in arguments)
        {
            ExpressionSyntax expression = argument.Expression;
            if (expression is not IdentifierNameSyntax nameSyntax) continue;
            if (nameSyntax.Identifier.ValueText.Contains(identifierName)) return true;
        }
        return false;
    }
}