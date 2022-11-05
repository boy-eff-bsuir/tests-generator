using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestsGenerator.Core.DTOs
{
    public class MethodDto
    {
        public MethodDto(string name, int count)
        {
            Name = name;
            Count = count;
        }

        public string Name { get; set; }
        public int Count { get; set; }
    }
}