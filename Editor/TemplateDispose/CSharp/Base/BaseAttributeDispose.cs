using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityBindTool
{
    public abstract class BaseAttributeDispose
    {
        public BaseTemplateDispose templateDispose;
        public Attribute attributeValue;

        public SyntaxNode generateContent;
        public ClassDeclarationSyntax generateTarget;

        public virtual void DisposeField() { }
        public virtual void DisposeProperty() { }
        public virtual void DisposeMethod() { }
    }
}