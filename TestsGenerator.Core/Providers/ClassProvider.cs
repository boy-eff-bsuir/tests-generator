using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.Providers
{
    public class ClassProvider
    {
        private readonly Dictionary<string, MethodInfo> _methods = new();

        public ClassProvider(string name, SeparatedSyntaxList<ParameterSyntax> constructorParams, params MethodDeclarationSyntax[] methods)
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
