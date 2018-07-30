using NUnit.Framework;

namespace Richiban.CommandLine.Tests.Routes
{
    class HelpTests
    {
        private string outputHelp;

        private dynamic RunTest(params string[] args)
        {
            var config = CommandLineConfiguration.GetDefault();
            config.AssemblyToScan = GetType().Assembly;
            config.HelpOutput = s => outputHelp = s;

            return CommandLine.Execute(config, args);
        }

        [Test]
        public void NoArgumentsResultsInHelp()
        {
            RunTest();

            Assert.That(outputHelp.StartsWith("Usage:"));
        }

        [Test]
        public void ExplicitCallToHelpResultsInHelp()
        {
            RunTest("/?");

            Assert.That(outputHelp, Does.StartWith("Usage:"));
        }

        [Test]
        public void ExplicitHelpGlyphResultsInHelpForRoute()
        {
            RunTest("test-route-1", "/?");

            Assert.That(outputHelp, Does.StartWith("Help for test-route-1:"));
        }

        [Test, Ignore("This feature doesn't quite work for now")]
        public void TooFewRoutePartsResultsInHelpForRoute()
        {
            RunTest("two-part-route-1");

            Assert.That(outputHelp, Does.StartWith("Help for two-part-route-1:"));
        }
    }
}