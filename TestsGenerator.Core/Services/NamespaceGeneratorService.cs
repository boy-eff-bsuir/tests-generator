using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.Services
{
    public class NamespaceGeneratorService
    {
        private readonly Dictionary<string, List<ClassGenerator>> _classesByNamespace = new();
        
        public List<string> Namespaces
        {
            get
            {
                return _classesByNamespace.Keys.ToList();
            }
        }

        public bool TryAddNamespace(string item)
        {
            return _classesByNamespace.TryAdd(item, new List<ClassGenerator>());
        }

        public void AddNamespace(string namespaceItem, params ClassDeclarationSyntax[] classes)
        {
            _classesByNamespace.TryAdd(namespaceItem, new List<ClassGenerator>());
            
            foreach (var classItem in classes)
            {
                var ctorParams = GetPublicCtorWithInterfaceTypeParameters(classItem)?.ParameterList?.Parameters;
                var methods = classItem.Members.OfType<MethodDeclarationSyntax>();
                var classGenerator = new ClassGenerator(classItem.Identifier.ValueText, ctorParams.GetValueOrDefault(), methods.ToArray());
                _classesByNamespace[namespaceItem].Add(classGenerator);
            }
        }

        public List<ClassDto> GetClassesByNamespace(string namespaceItem)
        {
            var classGenerator = _classesByNamespace[namespaceItem];
            return classGenerator.Select(x => new ClassDto(x.Name, x.ConstructorParams, x.Methods)).ToList();
        }

        private static bool IsInterface(TypeSyntax type, SyntaxToken identifier)
        {
            var identifierName = "i" + identifier.ToString().ToLowerInvariant();
            return identifierName.Equals(type.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
        }

        private static ConstructorDeclarationSyntax GetPublicCtorWithInterfaceTypeParameters(ClassDeclarationSyntax classItem)
        {
            return classItem.Members
                    .OfType<ConstructorDeclarationSyntax>()
                    .Where(x => x.Modifiers.Select(x => x.Kind()).Contains(SyntaxKind.PublicKeyword))
                    .MaxBy(x => x.ParameterList.Parameters.Where(x => IsInterface(x.Type, x.Identifier)).Count());
        }
    }
}