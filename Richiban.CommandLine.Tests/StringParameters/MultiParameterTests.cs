using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests
{
    [TestFixture]
    public class MultiParameterTests
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
            var result = RunTest("-paramA", "valueOfA1", "-paramB", "valueOfB1");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA1, paramB = valueOfB1 }"));
        }

        [Test]
        public void SingleStringParameterUnixStyleTest()
        {
            var result = RunTest("--paramA=valueOfA2", "--paramB=valueOfB2");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA2, paramB = valueOfB2 }"));
        }

        [Test]
        public void SingleStringParameterWindowsStyleTest()
        {
            var result = RunTest("/paramA:valueOfA3", "/paramB:valueOfB3");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(result.Output, Is.EqualTo("{ paramA = valueOfA3, paramB = valueOfB3 }"));
        }
    }
}
