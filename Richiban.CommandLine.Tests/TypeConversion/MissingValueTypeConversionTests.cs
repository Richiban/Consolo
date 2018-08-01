using NUnit.Framework;

namespace Richiban.CommandLine.Tests.TypeConversion
{
    [TestFixture]
    class MissingValueTypeConversionTests : CommandLineTest
    {
        [Test]
        public void MissingValueTypeConversionTest()
        {
            var result = RunTest(
                "type-conversion-tests",
                "missing");

            Assert.That(result.ExecutedAction, Is.EqualTo("MissingTypeConversionAction"));
            StringAssert.AreEqualIgnoringCase(result.Output, $"{{ param = 1 }}");
        }
    }
}
