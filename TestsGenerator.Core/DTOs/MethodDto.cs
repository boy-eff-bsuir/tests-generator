using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestsGenerator.Core.DTOs
{
    public record MethodDto(string Name, SeparatedSyntaxList<ParameterSyntax> Parameters, int Count, TypeSyntax ReturnType);
}