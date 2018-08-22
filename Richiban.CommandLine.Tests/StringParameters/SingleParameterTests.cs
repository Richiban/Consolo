using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests
{
    class SingleParameterTests : CommandLineTest
    {
        [Test]
        public void SingleStringParameterPowerShellStyleTest()
        {
            var result = RunTest("-paramA", "valueOfA1").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("valueOfA1"));
        }

        [Test]
        public void SingleStringParameterUnixStyleTest()
        {
            var result = RunTest("--paramA=valueOfA2").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("valueOfA2"));
        }

        [Test]
        public void SingleStringParameterWindowsStyleTest()
        {
            var result = RunTest("/paramA:valueOfA3").ProgramOutput;

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("valueOfA3"));
        }
    }
}
