using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator.Core.Services
{
    public class MethodInfo
    {
        public MethodInfo(int count, SeparatedSyntaxList<ParameterSyntax> parameters, TypeSyntax returnType)
        {
            Count = count;
            Parameters = parameters;
            ReturnType = returnType;
        }

        public int Count { get; set; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; set; }
        public TypeSyntax ReturnType { get; set; }
    }
}
