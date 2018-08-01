using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests.TypeConversion
{
    [TestFixture]
    class IntTypeConversionTests : CommandLineTest
    {
        [TestCase("1")]
        [TestCase("0")]
        [TestCase("100000")]
        public void IntTypeConversionTest(string testValue)
        {
            var result = RunTest(
                "type-conversion-tests",
                "int",
                "-param", 
                testValue);

            Assert.That(result.ExecutedAction, Is.EqualTo("IntTypeConversionAction"));
            Assert.That(result.Output, Is.EqualTo($"{{ param = {testValue} }}"));
        }

        [TestCase("x")]
        [TestCase("")]
        [TestCase("100.0")]
        public void IntTypeConversionFailureTest(string testValue)
        {
            var ex = Assert.Throws<TypeConversionException>(() =>
                RunTest(
                    "type-conversion-tests",
                    "int",
                    "-param", 
                    testValue));

            var expectedMessage = 
                $"No ITypeConverter could be found that could convert '{testValue}' to type 'Int32'";

            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }
    }
}
