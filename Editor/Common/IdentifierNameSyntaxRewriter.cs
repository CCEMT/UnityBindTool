using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class IdentifierRewriter : CSharpSyntaxRewriter
{
    private readonly string _oldName;
    private readonly string _newName;

    public IdentifierRewriter(string oldName, string newName)
    {
        _oldName = oldName;
        _newName = newName;
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (node.Identifier.ValueText == _oldName) return node.WithIdentifier(SyntaxFactory.Identifier(_newName));
        return base.VisitIdentifierName(node);
    }
}