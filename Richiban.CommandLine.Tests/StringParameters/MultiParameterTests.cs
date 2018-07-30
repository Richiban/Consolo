using NUnit.Framework;
using System;

namespace Richiban.CommandLine.Tests
{
    [TestFixture]
    public class MultiParameterTests
    {
        [Test]
        public void SingleStringParameterPowerShellStyleTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "-paramA", "valueOfA1", "-paramB", "valueOfB1");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(TestProgram.Output, Is.EqualTo("{ paramA = valueOfA1, paramB = valueOfB1 }"));
        }

        [Test]
        public void SingleStringParameterUnixStyleTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "--paramA=valueOfA2", "--paramB=valueOfB2");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(TestProgram.Output, Is.EqualTo("{ paramA = valueOfA2, paramB = valueOfB2 }"));
        }

        [Test]
        public void SingleStringParameterWindowsStyleTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "/paramA:valueOfA3", "/paramB:valueOfB3");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiStringParameterTestAction)));
            Assert.That(TestProgram.Output, Is.EqualTo("{ paramA = valueOfA3, paramB = valueOfB3 }"));
        }
    }
}
