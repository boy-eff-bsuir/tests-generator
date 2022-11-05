using System.Text;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Models;
using TestsGenerator.Core.Services;

namespace TestsGenerator.Core
{
    public class Generator
    {
        public async Task GenerateAsync(string[] files,
            string storePath,
            int readFromFileRestriction, 
            int generateTestFileRestriction, 
            int writeToFileRestriction)
        {

            var readFromFileBlock = new TransformBlock<string, ReadFromFileOutput>(async path =>
            {
                System.Console.WriteLine($"Opening file {path}");
                ReadFromFileOutput result;
                using (StreamReader fileStream = File.OpenText(path))
                {
                    var name = Path.GetFileName(path);
                    var content = await fileStream.ReadToEndAsync();
                    result = new ReadFromFileOutput(name, content);
                }
                return result;
            });

            var generateTestFileBlock = new TransformBlock<ReadFromFileOutput, GenerateTestFileOutput>(input =>
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(input.Content);
                System.Console.WriteLine($"Generating tests class for file {input.Name}");
                var root = tree.GetCompilationUnitRoot();

                var usings = root.Usings.ToList();

                var xunit = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Xunit"));
                usings.Add(xunit);
                var namespaceGeneratorService = new NamespaceGeneratorService();
                var visitor = new CustomCSharpSyntaxRewriter(namespaceGeneratorService);
                visitor.Visit(root);
                var namespaces = new List<MemberDeclarationSyntax>();

                foreach(var namespaceName in namespaceGeneratorService.Namespaces)
                {
                    var classes = namespaceGeneratorService.GetClassesByNamespace(namespaceName);
                    
                    var classDeclarations = new List<MemberDeclarationSyntax>();

                    foreach (var classDto in classes)
                    {
                        if (classDto.Methods.Count() == 0)
                        {
                            continue;
                        }

                        var methods = new List<MethodDeclarationSyntax>();
                        foreach (var methodDto in classDto.Methods)
                        {
                            methods.Add(
                               CreateTestMethodDeclaration(methodDto.Name + "Test")
                            );
                            if (methodDto.Count > 1)
                            {
                                for (int i = 2; i <= methodDto.Count; i++)
                                {
                                    methods.Add(CreateTestMethodDeclaration(methodDto.Name + i + "Test"));
                                }
                            }
                        }

                        var modifier = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
                        var identifier = SyntaxFactory.Identifier(classDto.Name + "Tests");

                        var declarationToAdd = SyntaxFactory.ClassDeclaration(
                            new SyntaxList<AttributeListSyntax>(),                  //Attribute list
                            new SyntaxTokenList() { modifier },                     //Modifiers
                            identifier,                                             //Identifier
                            null,                                                   //Type parameter list
                            null,                                                   //Base list
                            new SyntaxList<TypeParameterConstraintClauseSyntax>(),  //Constraint clauses list
                            SyntaxFactory.List<MemberDeclarationSyntax>(methods));

                        classDeclarations.Add(declarationToAdd);
                    }

                    if (classDeclarations.Count == 0)
                    {
                        continue;
                    }

                    var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(namespaceName));
                    usings.Add(usingDirective);

                    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.IdentifierName(namespaceName + ".Tests"),
                        new SyntaxList<ExternAliasDirectiveSyntax>(),
                        new SyntaxList<UsingDirectiveSyntax>(),
                        SyntaxFactory.List(classDeclarations)
                        );

                    namespaces.Add(namespaceDeclaration);
                 }

                var resultCompilationUnit = SyntaxFactory.CompilationUnit(
                    new SyntaxList<ExternAliasDirectiveSyntax>(),
                    SyntaxFactory.List(usings),
                    new SyntaxList<AttributeListSyntax>(),
                    SyntaxFactory.List(namespaces));

                resultCompilationUnit = (CompilationUnitSyntax)Formatter.Format(resultCompilationUnit, new AdhocWorkspace());
                GenerateTestFileOutput result = new(input.Name, resultCompilationUnit.ToString());

                return result;
            });

            var writeToFileBlock = new ActionBlock<GenerateTestFileOutput>(async input =>
            {
                System.Console.WriteLine($"Writing file {input.Name}");
                using (FileStream fileStream = File.Create(storePath + $"\\{input.Name}"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(input.Content);
                    await fileStream.WriteAsync(info);
                }
            });

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            readFromFileBlock.LinkTo(generateTestFileBlock, linkOptions);
            generateTestFileBlock.LinkTo(writeToFileBlock, linkOptions);

            foreach (var file in files)
            {
                await readFromFileBlock.SendAsync(file);
            }

            readFromFileBlock.Complete();

            writeToFileBlock.Completion.Wait();
        }

        private MethodDeclarationSyntax CreateTestMethodDeclaration(string methodName)
        {
            return SyntaxFactory.MethodDeclaration(
                new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                null,
                SyntaxFactory.Identifier(methodName),
                null,
                SyntaxFactory.ParameterList(),
                new SyntaxList<TypeParameterConstraintClauseSyntax>(),
                SyntaxFactory.Block(SyntaxFactory.ParseStatement(@"Assert.Fail(""autogenerated"");")),
                null);
        }
    }
}
