using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Faker.Core.Exceptions;
using Faker.Core.Interfaces;
using Faker.Core.Records;
using Xunit;
using Moq;
using Faker.Core.Services;

namespace Faker.Core.Services.Tests
{
    class GeneratorServiceTests
    {
        private GeneratorService _sut;
        private Mock<IGeneratorConfig> _generatorConfig;

        GeneratorServiceTests()
        {
            IGeneratorConfig generatorConfig = new Mock<IGeneratorConfig>();
            var _sut = new GeneratorService(generatorConfig);
        }

        public void GenerateTest()
        {
            Type type = default;
            IGeneratorContext context = default;
            string name = "";
            Type userType = default;
            Kek kek = default;
            object result = _sut.Generate(type, context, name, userType, kek);
            object expected = default;
            Assert.That(result, Is.EqualTo(expected));
            Assert.True(false);
        }
    }
}