using TestsGenerator.Core;
using Xunit;

namespace TestsGenerator.Tests;

public class TestsGeneratorTests
{
    private const string filePath = ".\\Files\\";
    [Fact]
    public void ShouldGenerateTestFile()
    {
        var sut = new Generator();
    }
}