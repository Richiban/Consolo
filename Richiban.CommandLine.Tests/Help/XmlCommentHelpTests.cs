using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests.Routes
{
    class XmlCommentHelpTests
    {
        private string outputHelp;

        private dynamic RunTest(params string[] args)
        {
            var config = CommandLineConfiguration.GetDefault();
            config.AssembliesToScan = new List<Assembly> { GetType().Assembly };
            config.HelpOutput = s => outputHelp = s;

            return CommandLine.Execute(config, args);
        }

        [Test]
        public void ExplicitHelpGlyphResultsInHelpWithXmlCommentsForRoute()
        {
            RunTest("test-route-1", "/?");

            Assert.That(outputHelp, Does.Contain("This is the comment for test-route-1"));
        }

        [Test]
        public void HelpPseudoRouteResultsInHelpWithXmlCommentsForRoute()
        {
            RunTest("help", "test-route-1");

            Assert.That(outputHelp, Does.Contain("This is the comment for test-route-1"));
        }

        [Test]
        public void TooFewRoutePartsResultsInHelpWithXmlCommentsForRoute()
        {
            RunTest("two-part-route-1");

            Assert.That(outputHelp, Does.Contain("This is the comment for two-part-route-1"));
        }
    }
}