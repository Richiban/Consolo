using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Richiban.CommandLine.Tests.Routes
{
    class RouteTests
    {
        [Test]
        public void SingleLevelRouteTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "test-route");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.RoutedAction)));
        }

        [Test]
        public void MultiLevelRouteTest()
        {
            var config = CommandLineConfiguration.Default;
            config.AssemblyToScan = GetType().Assembly;
            CommandLine.Execute(config, "test-route-1", "test-route-2");

            Assert.That(TestProgram.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiLevelRoutedAction)));
        }
    }
}
