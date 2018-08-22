using NUnit.Framework;

namespace Richiban.CommandLine.Tests.TypeConversion
{
    [TestFixture]
    class UriTypeConversionTests : CommandLineTest
    {
        [TestCase("http://example.com/test/")]
        [TestCase("ftp://example.com/")]
        [TestCase("ssh://example.com/")]
        public void UriTypeConversionTest(string testParam)
        {
            var result = RunTest(
                "type-conversion-tests",
                "uri",
                "-param",
                testParam).ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo("UriTypeConversionAction"));
            StringAssert.AreEqualIgnoringCase(result.Output, $"{{ param = {testParam} }}");
        }

        [TestCase("x")]
        [TestCase("")]
        public void UriTypeConversionFailureTest(string testValue)
        {
            var result =
                RunTest(
                    "type-conversion-tests",
                    "uri",
                    "-param",
                    testValue);

            var expectedMessage =
                $"The constructor for type System.Uri threw an exception when given '{testValue}'";

            Assert.That(result.ProgramOutput, Is.Null);
            Assert.That(result.OutputHelp, Does.Contain(expectedMessage));
        }
    }
}
