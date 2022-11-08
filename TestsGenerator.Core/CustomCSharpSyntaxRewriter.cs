using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.Providers;

namespace TestsGenerator.Core
{
    internal class CustomCSharpSyntaxRewriter : CSharpSyntaxRewriter
    {
        private string _currentNamespace;
        private readonly NamespaceProvider _namespaceProvider;

        public CustomCSharpSyntaxRewriter(NamespaceProvider namespaceGenerator)
        {
            _namespaceProvider = namespaceGenerator;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Parent is NamespaceDeclarationSyntax ns)
            {
                _currentNamespace = ns.Name.ToString();
            }

            _namespaceProvider.AddNamespace(_currentNamespace, node);
            base.VisitClassDeclaration(node);
            return node;
        }
    }
}
