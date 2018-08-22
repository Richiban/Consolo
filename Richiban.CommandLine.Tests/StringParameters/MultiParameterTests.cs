using NUnit.Framework;

namespace Richiban.CommandLine.Tests
{
    class MultiParameterTests : CommandLineTest
    {
        [Test]
        public void SingleStringParameterPowerShellStyleTest()
        {
            var result = RunTest("-paramA", "valueOfA1", "-paramB", "valueOfB1").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA1, paramB = valueOfB1 }"));
        }

        [Test]
        public void SingleStringParameterUnixStyleTest()
        {
            var result = RunTest("--paramA=valueOfA2", "--paramB=valueOfB2").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA2, paramB = valueOfB2 }"));
        }

        [Test]
        public void SingleStringParameterWindowsStyleTest()
        {
            var result = RunTest("/paramA:valueOfA3", "/paramB:valueOfB3").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA3, paramB = valueOfB3 }"));
        }
    }
}
