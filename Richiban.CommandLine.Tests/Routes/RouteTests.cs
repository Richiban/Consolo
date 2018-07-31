using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests.Routes
{
    class RouteTests
    {
        private dynamic RunTest(params string[] args)
        {
            var config = CommandLineConfiguration.GetDefault();
            config.AssembliesToScan = new List<Assembly> { GetType().Assembly };
            return CommandLine.Execute(config, args);
        }

        [Test]
        public void SingleLevelRouteTest()
        {
            var result = RunTest("test-route");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.RoutedAction)));
        }

        [Test]
        public void MultiLevelRouteTest()
        {
            var result = RunTest("test-route-1", "test-route-2");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.MultiLevelRoutedAction)));
        }

        [Test]
        public void ZeroLevelClassAndMethodRouteTest()
        {
            var result = RunTest("class-test-route-1");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.ClassRoutedActions.ZeroLevelRoutedAction)));
        }

        [Test]
        public void SingleLevelClassAndMethodRouteTest()
        {
            var result = RunTest("class-test-route-1", "class-test-route-2");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.ClassRoutedActions.SingleLevelClassRoutedActionWithParameter)));
        }

        [Test]
        public void MultiLevelClassAndMethodRouteTest()
        {
            var result = RunTest("class-test-route-1", "class-test-route-2", "class-test-route-3");

            Assert.That(result.ExecutedAction, Is.EqualTo(nameof(TestProgram.ClassRoutedActions.MultiLevelClassRoutedAction)));
        }
    }
}
