using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class IdentifierNameSyntaxRewriter : CSharpSyntaxRewriter
{
    private readonly string _targetName;

    public IdentifierNameSyntaxRewriter(string targetName)
    {
        _targetName = targetName;
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(_targetName));
    }
}