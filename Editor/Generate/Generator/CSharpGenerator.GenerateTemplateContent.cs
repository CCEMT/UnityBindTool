using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

public partial class CSharpGenerator
{
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

        List<SyntaxTree> templateTreeList = new List<SyntaxTree>();

        int templateAmount = this.csharpScriptSetting.templateDataList.Count;
        for (int i = 0; i < templateAmount; i++)
        {
            TextAsset templateData = this.csharpScriptSetting.templateDataList[i];
            SyntaxTree templateTree = CSharpSyntaxTree.ParseText(templateData.text);
            templateTreeList.Add(templateTree);
        }

        //TODO 生成必要生成的内容

        //TODO 根据数据生成内容
        
        root = root.ReplaceNode(oldNode, targetClass);
        string newCode = root.NormalizeWhitespace().ToFullString();

        File.Delete(targetFilePath);
        StreamWriter mainWriter = File.CreateText(targetFilePath);
        mainWriter.Write(newCode);
        mainWriter.Close();
    }
}