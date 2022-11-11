using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using TestsGenerator.Core.InputFiles;

namespace TestsGenerator.Core.InputFiles.Tests
{
    class TestClassTests
    {
        private TestClass _sut;

        TestClassTests()
        {
            var _sut = new TestClass();
        }

        public void TestMethodTest()
        {
            _sut.TestMethod();
            Assert.True(false);
        }
    }
}