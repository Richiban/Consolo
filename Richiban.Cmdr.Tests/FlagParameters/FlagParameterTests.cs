using NUnit.Framework;

namespace Richiban.Cmdr.Tests
{
    class FlagParameterTests : CommandLineTest
    {
        [Test]
        public void FlagParametersAreMappedToTrueWhenNameIsPresent()
        {
            var result = RunTest("test-route-1", "--param1");

            Assert.That(result.ProgramOutput, Is.Not.Null);
            Assert.That(result.ProgramOutput.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleLevelRoutedActionWithParameter)));
            Assert.That(result.ProgramOutput.Output, Is.EqualTo("True"));
        }

        [Test]
        public void FlagParametersAreMappedToFalseWhenNameIsNotPresent()
        {
            var result = RunTest("test-route-1");

            Assert.That(result.ProgramOutput, Is.Not.Null);
            Assert.That(result.ProgramOutput.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleLevelRoutedActionWithParameter)));
            Assert.That(result.ProgramOutput.Output, Is.EqualTo("False"));
        }

        [Test]
        public void FlagParametersAreMappedToTrueWhenNameAndValueArePresent()
        {
            var result = RunTest("test-route-1", "--param1=true");

            Assert.That(result.ProgramOutput, Is.Not.Null);
            Assert.That(result.ProgramOutput.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleLevelRoutedActionWithParameter)));
            Assert.That(result.ProgramOutput.Output, Is.EqualTo("True"));
        }

        [Test]
        public void FlagParametersAreMappedToFalseWhenNameAndValueArePresent()
        {
            var result = RunTest("test-route-1", "--param1=false");

            Assert.That(result.ProgramOutput, Is.Not.Null);
            Assert.That(result.ProgramOutput.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleLevelRoutedActionWithParameter)));
            Assert.That(result.ProgramOutput.Output, Is.EqualTo("False"));
        }
    }
}
