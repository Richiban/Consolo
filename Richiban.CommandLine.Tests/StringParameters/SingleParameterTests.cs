using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests
{
    [TestFixture]
    public class SingleParameterTests
    {
        private dynamic RunTest(params string[] args)
        {
            var config = CommandLineConfiguration.GetDefault();
            config.AssembliesToScan = new List<Assembly> { GetType().Assembly };
            return CommandLine.Execute(config, args);
        }

        [Test]
        public void SingleStringParameterPowerShellStyleTest()
        {
            var result = RunTest("-paramA", "valueOfA1");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("valueOfA1"));
        }

        [Test]
        public void SingleStringParameterUnixStyleTest()
        {
            var result = RunTest("--paramA=valueOfA2");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("valueOfA2"));
        }

        [Test]
        public void SingleStringParameterWindowsStyleTest()
        {
            var result = RunTest("/paramA:valueOfA3");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("valueOfA3"));
        }
    }
}
