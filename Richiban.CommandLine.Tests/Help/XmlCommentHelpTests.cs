using NUnit.Framework;

namespace Richiban.CommandLine.Tests.Routes
{
    class XmlCommentHelpTests : CommandLineTest
    {
        [Test]
        public void ExplicitHelpGlyphResultsInHelpWithXmlCommentsForRoute()
        {
            var outputHelp = RunTest("test-route-1", "/?").OutputHelp;

            Assert.That(outputHelp, Does.Contain("This is the comment for test-route-1"));
        }

        [Test]
        public void HelpPseudoRouteResultsInHelpWithXmlCommentsForRoute()
        {
            var outputHelp = RunTest("help", "test-route-1").OutputHelp;

            Assert.That(outputHelp, Does.Contain("This is the comment for test-route-1"));
        }

        [Test]
        public void TooFewRoutePartsResultsInHelpWithXmlCommentsForRoute()
        {
            var outputHelp = RunTest("two-part-route-1").OutputHelp;

            Assert.That(outputHelp, Does.Contain("This is the comment for two-part-route-1"));
        }
    }
}