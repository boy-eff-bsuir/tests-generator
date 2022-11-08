using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.Extensions
{
    public static class TypeSyntaxExtensions
    {
        public static LiteralExpressionSyntax GetDefaultValue(this TypeSyntax typeSyntax)
        {
            switch (typeSyntax.ToString())
            {
                case "string":
                    {
                        return SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(""));
                    }
                case "int":
                    {
                        return SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(0));
                    }
                default:
                    {
                        return SyntaxFactory.LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    SyntaxFactory.Token(SyntaxKind.DefaultKeyword));
                    }
            }
        }
    
        public static bool IsInterface(this TypeSyntax typeSyntax, SyntaxToken identifier)
        {
            var identifierName = "i" + identifier.ToString().ToLowerInvariant();
            return identifierName.Equals(typeSyntax.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
