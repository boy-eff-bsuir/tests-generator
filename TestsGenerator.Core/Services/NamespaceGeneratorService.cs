using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.Services
{
    public class NamespaceGeneratorService
    {
        private Dictionary<string, List<ClassDto>> _classesByNamespace { get; set; } = new();
        
        public List<string> Namespaces
        {
            get
            {
                return _classesByNamespace.Keys.ToList();
            }
        }

        public bool TryAddNamespace(string item)
        {
            return _classesByNamespace.TryAdd(item, new List<ClassDto>());
        }

        public void AddNamespace(string namespaceItem, params ClassDeclarationSyntax[] classes)
        {
            _classesByNamespace.TryAdd(namespaceItem, new List<ClassDto>());
            foreach (var classItem in classes)
            {
                _classesByNamespace[namespaceItem].Add(classItem.ToDto());
            }
        }

        public void AddClassesToNamespace(string namespaceItem, params ClassDeclarationSyntax[] classes)
        {
            foreach (var classItem in classes)
            {
                _classesByNamespace[namespaceItem].Add(classItem.ToDto());
            }
        }

        public List<ClassDto> GetClassesByNamespace(string namespaceItem)
        {
            return _classesByNamespace[namespaceItem];
        }
    }
}