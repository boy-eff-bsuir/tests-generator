using System.Text;
using System.Threading.Tasks.Dataflow;
using TestsGenerator.Core;
using TestsGenerator.Example.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        System.Console.WriteLine("Read from file max amount:");
        int readFromFileRestriction = Int32.Parse(Console.ReadLine());
        System.Console.WriteLine("Test generating max amount:");
        int generateTestFileRestriction = Int32.Parse(Console.ReadLine());
        System.Console.WriteLine("Write to file max amount:");
        int writeToFileRestriction = Int32.Parse(Console.ReadLine());

        var files = Directory.GetFiles(@"C:\Users\Asus\Desktop\5 семестр\СПП\4 лаба\TestsGenerator.Example\InputFiles");
        var generator = new Generator();
        var storePath = @"C:\Users\Asus\Desktop\5 семестр\СПП\4 лаба\TestsGenerator.Example\OutputFiles";

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
                var result = generator.Generate(input.Name, input.Content);
                return new GenerateTestFileOutput(input.Name, result);
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
}