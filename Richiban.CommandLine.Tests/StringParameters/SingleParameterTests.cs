using NUnit.Framework;
using System;

namespace Richiban.CommandLine.Tests
{
    [TestFixture]
    public class SingleParameterTests
    {
        [Test]
        public void SingleStringParameterPowerShellStyleTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "-paramA", "valueOfA1");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(TestProgram.Output, Is.EqualTo("valueOfA1"));
        }

        [Test]
        public void SingleStringParameterUnixStyleTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "--paramA=valueOfA2");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(TestProgram.Output, Is.EqualTo("valueOfA2"));
        }

        [Test]
        public void SingleStringParameterWindowsStyleTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "/paramA:valueOfA3");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.SingleStringParameterTestAction)));
            Assert.That(TestProgram.Output, Is.EqualTo("valueOfA3"));
        }
    }
}
