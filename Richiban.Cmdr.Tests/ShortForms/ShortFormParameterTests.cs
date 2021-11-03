using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests
{
    internal class ShortFormParameterTests : CommandLineTest
    {
        [Test]
        public void ShortFormCanBeUsedInsteadOfFullParameterName()
        {
            var result = RunTest("short-form-tests", "-a", "-b", "-c").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.ShortFormTestAction)));

            Assert.That(
                result.Output,
                Is.EqualTo("{ paramA = True, paramB = True, paramC = True }"));
        }

        [Test]
        public void FullParameterNameCanBeUsedInsteadOfShortForm()
        {
            var result = RunTest("short-form-tests", "-paramA", "-paramB").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.ShortFormTestAction)));

            Assert.That(
                result.Output,
                Is.EqualTo("{ paramA = True, paramB = True, paramC = False }"));
        }

        [Test]
        public void
            FullParameterNameCannotBeUsedInsteadOfShortFormIfDisallowLongFormIsSet()
        {
            var result = RunTest("short-form-tests", "-paramC");

            Assert.That(result.ProgramOutput, Is.Null);
            Assert.That(result.OutputHelp, Does.Contain("Help for short-form-tests"));
        }

        [Test]
        public void ShortFormsCanBeCombined()
        {
            var result = RunTest("short-form-tests", "-abc").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.ShortFormTestAction)));

            Assert.That(
                result.Output,
                Is.EqualTo("{ paramA = True, paramB = True, paramC = True }"));
        }

        [Test]
        public void ShortFormsAreCaseInsensitive()
        {
            var result = RunTest("short-form-tests", "-A").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.ShortFormTestAction)));

            Assert.That(
                result.Output,
                Is.EqualTo("{ paramA = True, paramB = False, paramC = False }"));
        }

        [Test]
        public void AlternativeShortFormsCanReferToSameParameter()
        {
            var result = RunTest("short-form-tests", "/z").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.ShortFormTestAction)));

            Assert.That(
                result.Output,
                Is.EqualTo("{ paramA = False, paramB = True, paramC = False }"));
        }
    }
}