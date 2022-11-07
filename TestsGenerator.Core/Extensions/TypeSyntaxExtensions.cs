using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
