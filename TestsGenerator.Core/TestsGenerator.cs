using System.Text;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TestsGenerator.Core.DTOs;
using TestsGenerator.Core.Extensions;
using TestsGenerator.Core.Models;
using TestsGenerator.Core.Services;

namespace TestsGenerator.Core
{
    public class Generator
    {
        private const string _sut = "_sut";
        public async Task GenerateAsync(string[] files,
            string storePath,
            int readFromFileRestriction, 
            int generateTestFileRestriction, 
            int writeToFileRestriction)
        {
            var readFromFileBlockOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = readFromFileRestriction };
            var generateTestFileOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = generateTestFileRestriction };
            var writeToFileBlockOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = writeToFileRestriction };


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
            },
            readFromFileBlockOptions);

            var generateTestFileBlock = new TransformBlock<ReadFromFileOutput, GenerateTestFileOutput>(input =>
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(input.Content);
                System.Console.WriteLine($"Generating tests class for file {input.Name}");
                var root = tree.GetCompilationUnitRoot();

                var usings = root.Usings.ToList();

                var xunit = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Xunit"));
                usings.Add(xunit);

                var moq = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Moq"));
                usings.Add(moq);
                
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
                        if (!classDto.Methods.Any())
                        {
                            continue;
                        }

                        var members = new List<MemberDeclarationSyntax>();

                        var publicModifier = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
                        var identifier = SyntaxFactory.Identifier(classDto.Name + "Tests");

                        var sutField = CreatePrivateField(classDto.Name, _sut);
                        members.Add(sutField);
                    
                        foreach (var param in classDto.ConstructorParams)
                        {
                            var field = CreatePrivateField(Mocked(param.Type.ToString()), Underscored(param.Identifier.ValueText));
                            members.Add(field);
                        }

                        var block = CreateConstructorBlock(classDto.Name, classDto.ConstructorParams.ToArray());

                        var ctor = SyntaxFactory.ConstructorDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            new SyntaxTokenList() { publicModifier },
                            identifier,
                            SyntaxFactory.ParameterList(),
                            null,
                            block
                            );

                        members.Add(ctor);

                        foreach (var methodDto in classDto.Methods)
                        {
                            members.Add(
                                CreateTestMethodDeclaration(methodDto.Name + "Test", methodDto.ReturnType, methodDto.Parameters.ToArray())
                            );
                            if (methodDto.Count > 1)
                            {
                                for (int i = 2; i <= methodDto.Count; i++)
                                {
                                    members.Add(CreateTestMethodDeclaration(methodDto.  Name + i + "Test", methodDto.ReturnType, methodDto.Parameters.ToArray()));
                                }
                            }
                        }

                        var declarationToAdd = SyntaxFactory.ClassDeclaration(
                            new SyntaxList<AttributeListSyntax>(),                  //Attribute list
                            new SyntaxTokenList() { publicModifier },                     //Modifiers
                            identifier,                                             //Identifier
                            null,                                                   //Type parameter list
                            null,                                                   //Base list
                            new SyntaxList<TypeParameterConstraintClauseSyntax>(),  //Constraint clauses list
                            SyntaxFactory.List<MemberDeclarationSyntax>(members));

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
            },
            generateTestFileOptions);

            var writeToFileBlock = new ActionBlock<GenerateTestFileOutput>(async input =>
            {
                System.Console.WriteLine($"Writing file {input.Name}");
                using FileStream fileStream = File.Create(storePath + $"\\{input.Name}");
                byte[] info = new UTF8Encoding(true).GetBytes(input.Content);
                await fileStream.WriteAsync(info);
            },
            writeToFileBlockOptions);

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

        private static MethodDeclarationSyntax CreateTestMethodDeclaration(string methodName, TypeSyntax returnType, params ParameterSyntax[] methodParams)
        {
            var block = CreateTestMethodBlock(methodName, returnType, methodParams);
            return SyntaxFactory.MethodDeclaration(
                new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                null,
                SyntaxFactory.Identifier(methodName),
                null,
                SyntaxFactory.ParameterList(),
                new SyntaxList<TypeParameterConstraintClauseSyntax>(),
                block,
                null);
        }
        
        private static FieldDeclarationSyntax CreatePrivateField(string type, string name)
        {
            var privateModifier = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
            return SyntaxFactory.FieldDeclaration(
                new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList() { privateModifier },
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(type), SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(name) }))
                );
        }

        private static string Mocked(string name)
        {
            return "Mock<" + name + ">";
        }

        private static string Underscored(string name)
        {
            return "_" + name;
        }

        private static BlockSyntax CreateConstructorBlock(string sutClassName, params ParameterSyntax[] ctorParams)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            if (!ctorParams.Any())
            {
                sb.AppendLine($"{_sut} = new {sutClassName}();");
                sb.AppendLine("}");
                return (BlockSyntax)SyntaxFactory.ParseStatement(sb.ToString());
            }

            foreach (var param in ctorParams)
            {
                sb.AppendLine($"{Underscored(param.Identifier.ValueText)} = new {Mocked(param.Type.ToString())}();");
            }
            sb.Append($"{_sut} = new {sutClassName}(");
            
            var last = ctorParams.Last();
            foreach (var param in ctorParams)
            {
                sb.Append($"{Underscored(param.Identifier.ValueText)}.Object");
                if (param != last)
                {
                    sb.Append(',');
                }
            }
            sb.AppendLine(");");
            sb.AppendLine("}");
            return (BlockSyntax)SyntaxFactory.ParseStatement(sb.ToString());
        }

        private static BlockSyntax CreateTestMethodBlock(string methodName, TypeSyntax returnType, params ParameterSyntax[] methodParams)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach(var param in methodParams)
            {
                sb.AppendLine($"{param.Type.ToString()} {param.Identifier.ValueText} = {param.Type.GetDefaultValue()};");
            }
            sb.AppendLine(String.Empty);
            sb.Append($"{returnType.ToString()} result = {_sut}.{methodName}(");
            if (methodParams.Any())
            {
                var last = methodParams.Last();
                foreach (var param in methodParams)
                {
                    sb.Append($"{param.Identifier.ValueText}");
                    if (param != last)
                    {
                        sb.Append(',');
                    }
                }
            }
            sb.AppendLine(");");
            sb.AppendLine(String.Empty);

            sb.AppendLine($"{returnType.ToString()} expected = {returnType.GetDefaultValue()};");
            sb.AppendLine("Assert.That(result, Is.EqualTo(expected));");
            sb.AppendLine(@"Assert.Fail(""autogenerated"");");
            sb.AppendLine("}");
            return (BlockSyntax)SyntaxFactory.ParseStatement(sb.ToString());
        }
    }
}
