using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Services;

namespace TestsGenerator.Core.Extensions
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static MethodDto ToMethodDto(this KeyValuePair<string, MethodInfo> pair)
        {
            return new MethodDto(pair.Key, pair.Value.Parameters, pair.Value.Count, pair.Value.ReturnType);
        }
    }
}