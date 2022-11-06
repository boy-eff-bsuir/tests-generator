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
        public static string GetDefaultValue(this TypeSyntax typeSyntax)
        {
            switch (typeSyntax.ToString())
            {
                case "string":
                    {
                        return string.Empty;
                    }
                case "int":
                    {
                        return "0";
                    }
                default:
                    {
                        return "null";
                    }
            }
        }
    }
}
