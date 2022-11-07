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

                        var block = CreateTestClassConstructorBlock(classDto.Name, classDto.ConstructorParams.ToArray());

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
            return SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(type))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(name)))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
        }

        private static string Mocked(string name)
        {
            return "Mock<" + name + ">";
        }

        private static string Underscored(string name)
        {
            return "_" + name;
        }

        private static BlockSyntax CreateTestMethodBlock(string methodName, TypeSyntax returnType, params ParameterSyntax[] methodParams)
        {
            var sb = new StringBuilder();
            var declarations = new List<StatementSyntax>();
            foreach(var param in methodParams)
            {
                var type = param.Type.ToString();
                var identifier = param.Identifier.ValueText;
                var defaultValue = param.Type.GetDefaultValue();
                declarations.Add(CreateLocalDeclarationStatement(type, identifier, defaultValue));
            }

            var resultReturnType = returnType.ToString();
            StatementSyntax actPart;
            StatementSyntax assertExpectedInitializationPart = null;
            StatementSyntax assertCompareWithExpectedPart = null;
            var arguments = methodParams.Select(x => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(x.Identifier.ValueText)));
            if (resultReturnType == "void")
            {
                actPart = SyntaxFactory.ExpressionStatement(InvocationExpression(_sut, methodName, arguments.ToArray()));
            }
            else
            {
                actPart = CreateLocalDeclarationStatement(resultReturnType, "result", InvocationExpression(_sut, methodName, arguments.ToArray()));
                assertExpectedInitializationPart = CreateLocalDeclarationStatement(resultReturnType, "expected", returnType.GetDefaultValue());
                assertCompareWithExpectedPart = CreateAssertCompareWithExpectedStatement();
            }
            declarations.Add(actPart);
            if (assertExpectedInitializationPart != null)
            {
                declarations.Add(assertExpectedInitializationPart);
                declarations.Add(assertCompareWithExpectedPart);
            }
            var failed = CreateAssertFailedStatement();
            declarations.Add(failed);

            return SyntaxFactory.Block(declarations);
        }

        private static LocalDeclarationStatementSyntax CreateLocalDeclarationStatement(string type, string identifier, ExpressionSyntax expressionSyntax)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName(type))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(
                                SyntaxFactory.TriviaList(),
                                SyntaxKind.TypeKeyword,
                                identifier,
                                identifier,
                                SyntaxFactory.TriviaList()))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                expressionSyntax)))));
        }

        private static ExpressionStatementSyntax CreateAssertCompareWithExpectedStatement()
        {
            return SyntaxFactory.ExpressionStatement(InvocationExpression("Assert", "That",
                        SyntaxFactory.Argument(
                            SyntaxFactory.IdentifierName("result")),
                        SyntaxFactory.Argument(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Is"),
                                    SyntaxFactory.IdentifierName("EqualTo")))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName("expected"))))))));
        }

        private static ExpressionStatementSyntax CreateAssertFailedStatement()
        {
            return SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("Assert"),
                                SyntaxFactory.IdentifierName("Fail")))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("autogenerated")))))));
        }

        private static ExpressionSyntax InvocationExpression(string className, string methodName, params ArgumentSyntax[] args)
        {
            return SyntaxFactory.InvocationExpression(
                       SyntaxFactory.MemberAccessExpression(
                           SyntaxKind.SimpleMemberAccessExpression,
                           SyntaxFactory.IdentifierName(className),
                           SyntaxFactory.IdentifierName(methodName)))
                   .WithArgumentList(
                       SyntaxFactory.ArgumentList(
                           SyntaxFactory.SeparatedList<ArgumentSyntax>(args)));
        }

        private static BlockSyntax CreateTestClassConstructorBlock(string sutType, params ParameterSyntax[] parameters)
        {
            var ctorParameters = new List<LocalDeclarationStatementSyntax>();
            var args = new List<ArgumentSyntax>();
            foreach (var param in parameters)
            {
                var identifier = param.Identifier.ValueText;
                var type = param.Type.ToString();
                var expression = CreateMockedObjectExpression(type);
                var declaration = CreateLocalDeclarationStatement(type, identifier, expression);
                //var declaration = CreateMockedDeclarationStatement(identifier, param.Type.ToString());
                ctorParameters.Add(declaration);
                args.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier)));
            }

            var sutDeclaration = CreateSutDeclarationStatement(SyntaxFactory.SeparatedList(args), sutType);
            ctorParameters.Add(sutDeclaration);
            return SyntaxFactory.Block(ctorParameters);
        }

        private static ObjectCreationExpressionSyntax CreateMockedObjectExpression(string type)
        {
            return SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("Mock"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(type)))))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList());
        }

        private static LocalDeclarationStatementSyntax CreateSutDeclarationStatement(SeparatedSyntaxList<ArgumentSyntax> args, string type)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName(
                                SyntaxFactory.Identifier(
                                    SyntaxFactory.TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    SyntaxFactory.TriviaList())))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(_sut))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName(type))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(args)))))));
        }
    }
}
