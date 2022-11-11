using FluentAssertions;
using TestsGenerator.Core;
using Xunit;

namespace TestsGenerator.Tests;

public class TestsGeneratorTests
{
    [Fact]
    public void ShouldGenerateTestFile()
    {
        var content = TestDataProvider.WrongFile;
        var fileNamespace = TestDataProvider.DefaultFileNamespace;
        var fileClass = TestDataProvider.DefaultFileClass;
        var fileMethod = TestDataProvider.DefaultFileMethod;
        
        var result = Generator.Generate(content);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("using Xunit;", Exactly.Once());
        result.Should().Contain("using Moq;", Exactly.Once());
        result.Should().Contain($"using {fileNamespace}", Exactly.Once());
        result.Should().Contain($"namespace {fileNamespace + ".Tests"}", Exactly.Once());
        result.Should().Contain($"class {fileClass + "Tests"}", Exactly.Once());
        result.Should().Contain(fileMethod + "Test", Exactly.Once());
    }

    [Fact]
    public void ShouldHandleOverloadedMethods()
    {
        var content = TestDataProvider.FileWithOverloadedMethod;
        var fileMethod = TestDataProvider.DefaultFileMethod;
        
        var result = Generator.Generate(content);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(fileMethod + "Test", Exactly.Once());
        result.Should().Contain(fileMethod + "2Test", Exactly.Once());
    }

    [Theory]
    [InlineData(TestDataProvider.FileWithEmptyNamespace)]
    [InlineData(TestDataProvider.FileWithEmptyClass)]
    [InlineData(TestDataProvider.WrongFile)]
    public void ShouldGenerateEmptyStringIfInputDataIsWrong(string content)
    {
        var result = Generator.Generate(content);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldMockConstructorInterfaceDependenciesUsingMoq()
    {
        var content = TestDataProvider.FileWithConstructor;
        
        var result = Generator.Generate(content);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain($"Mock<{TestDataProvider.ConstructorParamType}>");
    }

    [Fact]
    public void ShouldUseConstructorWithTheMostAmountOfInterfaceConstructorParameters()
    {
        var content = TestDataProvider.FileWithTwoConstructors;
        
        var result = Generator.Generate(content);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain($"new Mock", Exactly.Twice());
    }

    [Fact]
    public void ShouldUseDefaultValueIfConstructorParameterIsNotAnInterface()
    {
        var content = TestDataProvider.FileWithConstructorWithNonInterfaceParam;
        
        var result = Generator.Generate(content);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain($"new {TestDataProvider.DefaultFileClass}({TestDataProvider.ConstructorParamName}, default)");
    }
}