using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.DTOs
{
    public record MethodDto(string Name, SeparatedSyntaxList<ParameterSyntax> Parameters, int Count, TypeSyntax ReturnType);
}