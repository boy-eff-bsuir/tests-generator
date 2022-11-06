using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.DTOs
{
    public record ClassDto(string Name, SeparatedSyntaxList<ParameterSyntax> ConstructorParams, IEnumerable<MethodDto> Methods);
}