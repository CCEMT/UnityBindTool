using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityBindTool
{
    public class TypeDisposeCotentData : DisposeCotentData
    {
        public MemberInfo memberInfo;
        public MemberDeclarationSyntax originalContent;
        public List<MemberDeclarationSyntax> memberDeclarationSyntaxs = new List<MemberDeclarationSyntax>();
    }
}