using NUnit.Framework;

namespace Richiban.CommandLine.Tests
{
    class MultiValueParameterTests : CommandLineTest
    {
        [Test]
        public void MultipleStringParametersAreMappedToArray()
        {
            var result = RunTest("multi-value-param", "--param1=1", "--param1=2", "--param1=3");

            Assert.That(result.ProgramOutput, Is.Not.Null);
            Assert.That(result.ProgramOutput.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiValueParameterAction)));
            Assert.That(result.ProgramOutput.Output, Is.EqualTo("[1, 2, 3]"));
        }

        [Test]
        public void ZeroStringParametersAreMappedToArray()
        {
            var result = RunTest("multi-value-param");

            Assert.That(result.ProgramOutput, Is.Not.Null);
            Assert.That(result.ProgramOutput.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiValueParameterAction)));
            Assert.That(result.ProgramOutput.Output, Is.EqualTo("[]"));
        }
    }
}
