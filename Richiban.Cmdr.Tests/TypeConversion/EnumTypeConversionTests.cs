using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests.TypeConversion
{
    [TestFixture]
    internal class EnumTypeConversionTests : CommandLineTest
    {
        [TestCase("memberA"), TestCase("memberB")]
        public void EnumTypeConversionTest(string testParam)
        {
            var result = RunTest("type-conversion-tests", "enum", "-param", testParam)
                .ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo("EnumTypeConversionAction"));

            StringAssert.AreEqualIgnoringCase(
                result.Output,
                $"{{ param = {testParam} }}");
        }

        [TestCase("x"), TestCase("")]
        public void EnumTypeConversionFailureTest(string testValue)
        {
            var result = RunTest("type-conversion-tests", "enum", "-param", testValue);

            var expectedMessage =
                $"No ITypeConverter could be found that could convert '{testValue}' to type 'TestEnum'";

            Assert.That(result.ProgramOutput, Is.Null);
            Assert.That(result.OutputHelp, Does.Contain(expectedMessage));
        }
    }
}