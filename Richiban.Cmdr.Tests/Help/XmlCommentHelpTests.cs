using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests.Routes
{
    internal class XmlCommentHelpTests : CommandLineTest
    {
        [Test]
        public void ExplicitHelpGlyphResultsInHelpWithXmlCommentsForRoute()
        {
            var outputHelp = RunTest("test-route-1", "-?").OutputHelp;

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
            var outputHelp = RunTest(
                    "five-part-route-1",
                    "five-part-route-2",
                    "five-part-route-2",
                    "five-part-route-4")
                .OutputHelp;

            Assert.That(outputHelp, Does.Contain("Comments for five-part-route"));
        }
    }
}