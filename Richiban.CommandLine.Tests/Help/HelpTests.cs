using NUnit.Framework;

namespace Richiban.CommandLine.Tests.Routes
{
    class HelpTests : CommandLineTest
    {
        [Test]
        public void NoArgumentsResultsInHelp()
        {
            var outputHelp = RunTest().OutputHelp;

            Assert.That(outputHelp, Is.Not.Empty);
        }

        [Test]
        public void ExplicitCallToHelpResultsInHelp()
        {
            var outputHelp = RunTest("/?").OutputHelp;

            Assert.That(outputHelp, Is.Not.Empty);
        }

        [Test]
        public void HelpPseudoRouteResultsInHelp()
        {
            var outputHelp = RunTest("help").OutputHelp;

            Assert.That(outputHelp, Is.Not.Empty);
        }

        [Test]
        public void ExplicitHelpGlyphResultsInHelpForRoute()
        {
            var outputHelp = RunTest("test-route-1", "/?").OutputHelp;

            Assert.That(outputHelp, Does.Contain("testhost test-route-1 test-route-2"));
            Assert.That(outputHelp, Does.Contain("testhost test-route-1 [/param1]"));
        }

        [Test]
        public void HelpPseudoRouteResultsInHelpForRoute()
        {
            var outputHelp = RunTest("help", "test-route-1").OutputHelp;

            Assert.That(outputHelp, Does.StartWith("Help for test-route-1:"));
        }

        [Test]
        public void TooFewRoutePartsResultsInHelpForRoute()
        {
            var outputHelp = RunTest("two-part-route-1").OutputHelp;

            Assert.That(outputHelp, Does.Contain("Help for two-part-route-1:"));
        }
    }
}