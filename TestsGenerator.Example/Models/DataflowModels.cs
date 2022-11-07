using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator.Example.Models
{
    public record ReadFromFileOutput(string Name, string Content);
    public record GenerateTestFileOutput(string Name, string Content);
}
