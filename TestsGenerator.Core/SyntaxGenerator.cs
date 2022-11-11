using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core
{
    public static class SyntaxGenerator
    {
        private const string _sut = "_sut";
        
        public static MethodDeclarationSyntax GenerateTestMethodDeclaration(
            string testMethodName,
            string methodName, 
            TypeSyntax returnType, 
            params ParameterSyntax[] methodParams)
        {
            var block = GenerateTestMethodBlock(methodName, returnType, methodParams);
            return SyntaxFactory.MethodDeclaration(
                new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                null,
                SyntaxFactory.Identifier(testMethodName),
                null,
                SyntaxFactory.ParameterList(),
                new SyntaxList<TypeParameterConstraintClauseSyntax>(),
                block,
                null);
        }
        
        public static FieldDeclarationSyntax GeneratePrivateField(
            string type, 
            string name)
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

        public static BlockSyntax GenerateTestMethodBlock(
            string methodName, 
            TypeSyntax returnType, 
            params ParameterSyntax[] methodParams)
        {
            var sb = new StringBuilder();
            var declarations = new List<StatementSyntax>();
            foreach(var param in methodParams)
            {
                var type = param.Type.ToString();
                var identifier = param.Identifier.ValueText;
                var defaultValue = param.Type.GetDefaultValue();
                declarations.Add(GenerateLocalDeclarationStatement(type, identifier, defaultValue));
            }
            var resultReturnType = returnType.ToString();
            StatementSyntax actPart;
            StatementSyntax assertExpectedInitializationPart = null;
            StatementSyntax assertCompareWithExpectedPart = null;
            var arguments = methodParams.Select(x => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(x.Identifier.ValueText)));
            if (resultReturnType == "void")
            {
                actPart = SyntaxFactory.ExpressionStatement(InvocationExpression(_sut, methodName, arguments.ToArray())).WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
            }
            else
            {
                actPart = GenerateLocalDeclarationStatement(resultReturnType, "result", InvocationExpression(_sut, methodName, arguments.ToArray()));
                assertExpectedInitializationPart = GenerateLocalDeclarationStatement(resultReturnType, "expected", returnType.GetDefaultValue());
                assertCompareWithExpectedPart = GenerateAssertCompareWithExpectedStatement();
            }
            declarations.Add(actPart);
            if (assertExpectedInitializationPart != null)
            {
                declarations.Add(assertExpectedInitializationPart);
                declarations.Add(assertCompareWithExpectedPart);
            }
            var failed = GenerateAssertFailedStatement();
            declarations.Add(failed);

            return SyntaxFactory.Block(declarations);
        }

        public static LocalDeclarationStatementSyntax GenerateLocalDeclarationStatement(
            string type, 
            string identifier, 
            ExpressionSyntax expressionSyntax)
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

        public static ExpressionStatementSyntax GenerateAssertCompareWithExpectedStatement()
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

        public static ExpressionStatementSyntax GenerateAssertFailedStatement()
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

        public static ExpressionSyntax InvocationExpression(
            string className, 
            string methodName, 
            params ArgumentSyntax[] args)
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

        public static BlockSyntax GenerateTestClassConstructorBlock(
            string sutType, 
            params ParameterSyntax[] parameters)
        {
            var ctorParameters = new List<LocalDeclarationStatementSyntax>();
            var args = new List<ArgumentSyntax>();
            foreach (var param in parameters)
            {
                if (param.Type.IsInterface(param.Identifier))
                {
                    var identifier = param.Identifier.ValueText;
                    var type = param.Type.ToString();
                    var expression = GenerateMockedObjectExpression(type);
                    var declaration = GenerateLocalDeclarationStatement(type, identifier, expression);
                    ctorParameters.Add(declaration);
                    args.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier)));
                }
                else
                {
                    args.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))));
                }
                
            }

            var sutDeclaration = GenerateSutDeclarationStatement(SyntaxFactory.SeparatedList(args), sutType);
            ctorParameters.Add(sutDeclaration);
            return SyntaxFactory.Block(ctorParameters);
        }

        public static ObjectCreationExpressionSyntax GenerateMockedObjectExpression(string type)
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

        public static LocalDeclarationStatementSyntax GenerateSutDeclarationStatement(
            SeparatedSyntaxList<ArgumentSyntax> args, 
            string type)
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
    
        public static CompilationUnitSyntax GenerateCompilationUnit(
            List<UsingDirectiveSyntax> usings, 
            List<MemberDeclarationSyntax> namespaceDeclarations)
        {
            return SyntaxFactory.CompilationUnit(
                        new SyntaxList<ExternAliasDirectiveSyntax>(),
                        SyntaxFactory.List(usings),
                        new SyntaxList<AttributeListSyntax>(),
                        SyntaxFactory.List(namespaceDeclarations));
        }

        public static ConstructorDeclarationSyntax GeneratePublicConstructor(SyntaxToken identifier, BlockSyntax block)
        {
            var publicModifier = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
            return SyntaxFactory.ConstructorDeclaration(
                        new SyntaxList<AttributeListSyntax>(),
                        new SyntaxTokenList() { publicModifier },
                        identifier,
                        SyntaxFactory.ParameterList(),
                        null,
                        block
                        );
        }

        public static ClassDeclarationSyntax GeneratePublicClass(SyntaxToken identifier, List<MemberDeclarationSyntax> members)
        {
            var publicModifier = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
            return SyntaxFactory.ClassDeclaration(
                        new SyntaxList<AttributeListSyntax>(),                  //Attribute list
                        new SyntaxTokenList() { publicModifier },                     //Modifiers
                        identifier,                                             //Identifier
                        null,                                                   //Type parameter list
                        null,                                                   //Base list
                        new SyntaxList<TypeParameterConstraintClauseSyntax>(),  //Constraint clauses list
                        SyntaxFactory.List<MemberDeclarationSyntax>(members));
        }
    }
}