using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faker.Core.Interfaces;
using Xunit;
using Moq;
using Lepesh.Lepesh;
using Faker.Core.Services;

namespace Lepesh.Lepesh.Tests
{
    class LepeshClassTests
    {
        private LepeshClass _sut;
        private Mock<int> _LepeshCal;

        LepeshClassTests()
        {
            var _sut = new LepeshClass(default);
        }

        public void LepeshMethodTest()
        {
            _sut.LepeshMethod();
            Assert.True(false);
        }

        public void LepeshMethod2Test()
        {
            _sut.LepeshMethod();
            Assert.True(false);
        }
    }
}

namespace Faker.Core.Services.Tests
{
    class CycleResolveServiceTests
    {
        private CycleResolveService _sut;

        CycleResolveServiceTests()
        {
            var _sut = new CycleResolveService();
        }

        public void AddTest()
        {
            Type t = default;
            _sut.Add(t);
            Assert.True(false);
        }

        public void RemoveTest()
        {
            Type t = default;
            _sut.Remove(t);
            Assert.True(false);
        }

        public void ContainsTest()
        {
            Type t = default;
            bool result = _sut.Contains(t);
            bool expected = default;
            Assert.That(result, Is.EqualTo(expected));
            Assert.True(false);
        }

        public void ClearTest()
        {
            _sut.Clear();
            Assert.True(false);
        }
    }
}