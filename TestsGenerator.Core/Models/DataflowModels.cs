using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator.Core.Models
{
    public record ReadFromFileOutput(string Name, string Content);
    public record GenerateTestFileOutput(string Name, string Content);
}
