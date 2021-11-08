using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Richiban.Cmdr.Generator.Tests
{
    [TestFixture]
    class ModelBuilderTests
    {
        [Test]
        public void Test1()
        {
            var sut = new MethodModelBuilder();

            var actual = sut.GetMethods();

            Console.WriteLine(actual);
        }
    }
}