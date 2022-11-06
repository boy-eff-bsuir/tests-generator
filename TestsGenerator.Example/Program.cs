using TestsGenerator.Core;

internal class Program
{
    private static void Main(string[] args)
    {
        var files = Directory.GetFiles(@"C:\Users\Asus\Desktop\5 семестр\СПП\4 лаба\TestsGenerator.Example\InputFiles");
        var generator = new Generator();
        generator.GenerateAsync(files, @"C:\Users\Asus\Desktop\5 семестр\СПП\4 лаба\TestsGenerator.Example\OutputFiles", 1, 1, 1).Wait();
    }
}