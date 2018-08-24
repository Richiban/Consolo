using NUnit.Framework;

namespace Richiban.CommandLine.Tests
{
    class MultiParameterTests : CommandLineTest
    {
        [Test]
        public void MultipleStringParameterPowerShellStyleTest()
        {
            var result = RunTest("-paramA", "valueOfA1", "-paramB", "valueOfB1").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA1, paramB = valueOfB1 }"));
        }

        [Test]
        public void MultipleStringParameterUnixStyleTest()
        {
            var result = RunTest("--paramA=valueOfA2", "--paramB=valueOfB2").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA2, paramB = valueOfB2 }"));
        }

        [Test]
        public void MultipleStringParameterWindowsStyleTest()
        {
            var result = RunTest("/paramA:valueOfA3", "/paramB:valueOfB3").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA3, paramB = valueOfB3 }"));
        }

        [Test]
        public void MultipleStringParameterMixedWindowsStyleTestAndPositional()
        {
            var result = RunTest("valueOfB4", "/paramA:valueOfA4").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA4, paramB = valueOfB4 }"));
        }
    }
}
