using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.DTOs;

namespace TestsGenerator.Core.Extensions
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static ClassDto ToDto(this ClassDeclarationSyntax syntax)
        {   
            var result = new ClassDto(syntax.Identifier.ValueText);
            var methods = syntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(x => x.Modifiers.Select(x => x.Kind()).Contains(SyntaxKind.PublicKeyword))
                .Select(x => x.Identifier.ValueText);

            foreach (var method in methods)
            {
                result.AddMethod(method);
            }
            
            return result;
        }

        public static MethodDto ToMethodDto(this KeyValuePair<string, int> pair)
        {
            return new MethodDto(pair.Key, pair.Value);
        }
    }
}