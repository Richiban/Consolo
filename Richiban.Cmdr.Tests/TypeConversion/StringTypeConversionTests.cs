using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.Cmdr.Tests.TypeConversion
{
    [TestFixture]
    class StringTypeConversionTests : CommandLineTest
    {
        [Test]
        public void StringTypeConversionTest()
        {
            var result = RunTest(
                "type-conversion-tests",
                "string",
                "-param", "value").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo("StringTypeConversionAction"));
            Assert.That(result.Output, Is.EqualTo("{ param = value }"));
        }
    }
}
