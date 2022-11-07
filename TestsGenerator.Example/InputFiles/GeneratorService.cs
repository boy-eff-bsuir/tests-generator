using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Faker.Core.Exceptions;
using Faker.Core.Interfaces;
using Faker.Core.Records;

namespace Faker.Core.Services
{
    public struct Kek
    {

    }
    public class GeneratorService : IGeneratorService
    {
        private List<IValueGenerator> _generators;
        private IGeneratorConfig _config;

        public GeneratorService(IGeneratorConfig generatorConfig)
        {
            InitializeGenerators();
            _config = config;
        }

        public GeneratorService()
        {
            InitializeGenerators();
        }

        public object Generate(Type type, IGeneratorContext context, string name = null, Type userType = null, Kek kek)
        {
            if (name != null && _config != null && userType != null)
            {
                var configKeyRecord = new ConfigKeyRecord(userType, name);
                var configGenerator = _config.GetGeneratorByName(configKeyRecord);
                if (configGenerator != null)
                {
                    return configGenerator.Generate(type, context);
                }
            }
            var generator = _generators.SingleOrDefault(x => x.CanGenerate(type));
            if (generator == null)
            {
                throw new UnsupportedTypeException();
            }
            var result = generator.Generate(type, context);
            return result;
        }

        private void InitializeGenerators()
        {
            _generators = new List<IValueGenerator>();
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsAssignableTo(typeof(IValueGenerator)) && type.IsClass)
                {
                    var generator = Activator.CreateInstance(type);
                    _generators.Add((IValueGenerator)generator);
                }
            }
        }
    }
}