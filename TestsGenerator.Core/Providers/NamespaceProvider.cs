using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.Providers
{
    public class NamespaceProvider
    {
        private readonly Dictionary<string, List<ClassProvider>> _classesByNamespace = new();
        
        public List<string> Namespaces
        {
            get
            {
                return _classesByNamespace.Keys.ToList();
            }
        }

        public void AddNamespace(string namespaceItem, params ClassDeclarationSyntax[] classes)
        {
            _classesByNamespace.TryAdd(namespaceItem, new List<ClassProvider>());
            
            foreach (var classItem in classes)
            {
                var ctorParams = GetPublicCtorWithInterfaceTypeParameters(classItem)?.ParameterList?.Parameters;
                var methods = classItem.Members.OfType<MethodDeclarationSyntax>();
                var classGenerator = new ClassProvider(classItem.Identifier.ValueText, ctorParams.GetValueOrDefault(), methods.ToArray());
                _classesByNamespace[namespaceItem].Add(classGenerator);
            }
        }

        public List<ClassDto> GetClassesByNamespace(string namespaceItem)
        {
            var classGenerator = _classesByNamespace[namespaceItem];
            return classGenerator.Select(x => new ClassDto(x.Name, x.ConstructorParams, x.Methods)).ToList();
        }

        private static ConstructorDeclarationSyntax GetPublicCtorWithInterfaceTypeParameters(ClassDeclarationSyntax classItem)
        {
            return classItem.Members
                    .OfType<ConstructorDeclarationSyntax>()
                    .Where(x => x.Modifiers.Select(x => x.Kind()).Contains(SyntaxKind.PublicKeyword))
                    .MaxBy(x => x.ParameterList.Parameters.Where(x => x.Type.IsInterface(x.Identifier)).Count());
        }
    }
}