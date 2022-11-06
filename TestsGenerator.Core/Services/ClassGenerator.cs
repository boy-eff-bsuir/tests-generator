using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.Services
{
    public class ClassGenerator
    {
        private readonly Dictionary<string, MethodInfo> _methods = new();

        public ClassGenerator(string name, SeparatedSyntaxList<ParameterSyntax> constructorParams, params MethodDeclarationSyntax[] methods)
        {
            Name = name;
            ConstructorParams = constructorParams;
            foreach (var method in methods)
            {
                _methods.TryAdd(method.Identifier.ValueText, new MethodInfo(0, method.ParameterList.Parameters, method.ReturnType));
                _methods[method.Identifier.ValueText].Count++;
            }
            
        }

        public string Name { get; set; }
        public SeparatedSyntaxList<ParameterSyntax> ConstructorParams { get; set; }
        public IEnumerable<MethodDto> Methods
        {
            get
            {
                return _methods.ToList()
                    .Select(x => x.ToMethodDto());
            }
        }

        public void AddMethod(MethodDeclarationSyntax method)
        {
            var methodName = method.Identifier.ValueText;
            _methods.TryAdd(methodName, new MethodInfo(0, method.ParameterList.Parameters, method.ReturnType));
            _methods[methodName].Count++;
        }
    }
}
