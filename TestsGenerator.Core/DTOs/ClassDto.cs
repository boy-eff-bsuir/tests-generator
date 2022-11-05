using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestsGenerator.Core.Extensions;

namespace TestsGenerator.Core.DTOs
{
    public class ClassDto
    {
        private Dictionary<string, int> _methods = new();

        public ClassDto(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public IEnumerable<MethodDto> Methods
        {
            get
            {
                return _methods.ToList()
                    .Select(x => x.ToMethodDto());
            }
        }

        public void AddMethod(string methodName)
        {
            _methods.TryAdd(methodName, 0);
            _methods[methodName]++;
        }
    }
}