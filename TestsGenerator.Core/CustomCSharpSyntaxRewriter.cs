using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGenerator.Core.Services;

namespace TestsGenerator.Core
{
    internal class CustomCSharpSyntaxRewriter : CSharpSyntaxRewriter
    {
        private string _currentNamespace;
        private readonly NamespaceGeneratorService _namespaceGeneratorService;

        public CustomCSharpSyntaxRewriter(NamespaceGeneratorService namespaceGeneratorService)
        {
            _namespaceGeneratorService = namespaceGeneratorService;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Parent is NamespaceDeclarationSyntax ns)
            {
                _currentNamespace = ns.Name.ToString();
            }

            _namespaceGeneratorService.AddNamespace(_currentNamespace, node);
            base.VisitClassDeclaration(node);
            return node;
        }
    }
}
