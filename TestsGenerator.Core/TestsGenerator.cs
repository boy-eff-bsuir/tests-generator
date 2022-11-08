using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;
using TestsGenerator.Core.Providers;

namespace TestsGenerator.Core
{
    public class Generator
    {
        private const string _xunit = "Xunit";
        private const string _moq = "Moq";
        
        public string Generate(string name, string content)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(content);
            var root = tree.GetCompilationUnitRoot();
            var usings = InitializeUsings(root);
            var namespaceProvider = InitializeNamespaceProvider(root);
            var namespaceDeclarations = GenerateNamespaceDeclarations(namespaceProvider, usings);

            var resultCompilationUnit = SyntaxGenerator.GenerateCompilationUnit(usings, namespaceDeclarations);

            var workspace = new AdhocWorkspace();
            resultCompilationUnit = (CompilationUnitSyntax)Formatter.Format(resultCompilationUnit, workspace);
            return resultCompilationUnit.ToString();
        }

        private List<UsingDirectiveSyntax> InitializeUsings(CompilationUnitSyntax root)
        {
            var usings = root.Usings.ToList();
            AddToUsings(usings, _xunit);
            AddToUsings(usings, _moq);
            
            return usings;
        }

        private void AddToUsings(List<UsingDirectiveSyntax> usings, string identifier)
        {
            var directive = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(identifier));
            usings.Add(directive);
        }

        private NamespaceProvider InitializeNamespaceProvider(CompilationUnitSyntax root)
        {
            var namespaceProvider = new NamespaceProvider();
            var visitor = new CustomCSharpSyntaxRewriter(namespaceProvider);
            visitor.Visit(root);
            return namespaceProvider;
        }

        private List<MemberDeclarationSyntax> GenerateNamespaceDeclarations(NamespaceProvider namespaceProvider, List<UsingDirectiveSyntax> usings)
        {
            var namespaces = new List<MemberDeclarationSyntax>();
            foreach(var namespaceName in namespaceProvider.Namespaces)
            {
                var classes = namespaceProvider.GetClassesByNamespace(namespaceName);

                var classDeclarations = GenerateClassDeclarations(classes);

                if (classDeclarations.Count == 0)
                {
                    continue;
                }

                AddToUsings(usings, namespaceName);

                var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.IdentifierName(namespaceName + ".Tests"),
                    new SyntaxList<ExternAliasDirectiveSyntax>(),
                    new SyntaxList<UsingDirectiveSyntax>(),
                    SyntaxFactory.List(classDeclarations)
                    );

                namespaces.Add(namespaceDeclaration);
            }
            return namespaces;
        }

        private List<MemberDeclarationSyntax> GenerateClassDeclarations(List<ClassDto> classes)
        {
            var classDeclarations = new List<MemberDeclarationSyntax>();

            foreach (var classDto in classes)
                {
                    if (!classDto.Methods.Any())
                    {
                        continue;
                    }

                    var classMembers = new List<MemberDeclarationSyntax>();
                    var classIdentifier = SyntaxFactory.Identifier(classDto.Name + "Tests");

                    AddPrivateFields(classDto, classMembers);
                    AddConstructor(classIdentifier, classDto, classMembers);
                    AddMethods(classDto, classMembers);

                    var classDeclaration = SyntaxGenerator.GeneratePublicClass(classIdentifier, classMembers);

                    classDeclarations.Add(classDeclaration);
                }
            return classDeclarations;
        }
    
        private void AddPrivateFields(ClassDto classDto, List<MemberDeclarationSyntax> classMembers)
        {
            var sutField = SyntaxGenerator.GeneratePrivateField(classDto.Name, "_sut");
            classMembers.Add(sutField);
        
            foreach (var param in classDto.ConstructorParams)
            {
                var field = SyntaxGenerator.GeneratePrivateField(param.Type.ToString().Mocked(), param.Identifier.ValueText.Underscored());
                classMembers.Add(field);
            }
        }
    
        private void AddConstructor(SyntaxToken classIdentifier, ClassDto classDto, List<MemberDeclarationSyntax> classMembers)
        {
            var block = SyntaxGenerator.GenerateTestClassConstructorBlock(classDto.Name, classDto.ConstructorParams.ToArray());
            var ctor = SyntaxGenerator.GeneratePublicConstructor(classIdentifier, block);

            classMembers.Add(ctor);
        }
    
        private void AddMethods(ClassDto classDto, List<MemberDeclarationSyntax> classMembers)
        {
            foreach (var methodDto in classDto.Methods)
            {
                var methodName = methodDto.Name + "Test";
                var testMethod = SyntaxGenerator.GenerateTestMethodDeclaration(methodName,
                        methodDto.ReturnType,
                        methodDto.Parameters.ToArray());

                classMembers.Add(testMethod);
                
                if (methodDto.Count > 1)
                {
                    for (int i = 2; i <= methodDto.Count; i++)
                    {
                        methodName = methodDto.Name + i + "Test";
                        testMethod = SyntaxGenerator.GenerateTestMethodDeclaration(methodName,
                        methodDto.ReturnType,
                        methodDto.Parameters.ToArray());
                        
                        classMembers.Add(testMethod);
                    }
                }
            }
        }
    }
}
