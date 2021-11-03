using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests.TypeConversion
{
    [TestFixture]
    internal class MissingValueTypeConversionTests : CommandLineTest
    {
        [Test]
        public void MissingValueTypeConversionTest()
        {
            var result = RunTest("type-conversion-tests", "missing").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo("MissingTypeConversionAction"));
            StringAssert.AreEqualIgnoringCase(result.Output, "{ param = 1 }");
        }
    }
}