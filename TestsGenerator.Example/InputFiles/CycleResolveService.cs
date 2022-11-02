using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faker.Core.Interfaces;

namespace Faker.Core.Services
{
    namespace Lepesh.Lepesh
    {
        class LepeshClass
        {
            public void LepeshMethod
            {

            }
        }
    }
    public class CycleResolveService : ICycleResolveService
    {
        private List<Type> _types = new List<Type>();
        public void Add(Type t)
        {
            _types.Add(t);
        }

        public void Remove(Type t)
        {
            _types.Remove(t);
        }

        public bool Contains(Type t)
        {
            return _types.Contains(t);
        }

        public void Clear()
        {
            _types.Clear();
        }
    }
}