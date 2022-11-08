using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.DTOs
{
    public record ClassDto(string Name, SeparatedSyntaxList<ParameterSyntax> ConstructorParams, IEnumerable<MethodDto> Methods);
}