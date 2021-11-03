using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests.Routes
{
    internal class RouteTests : CommandLineTest
    {
        [Test]
        public void SingleLevelRouteTest()
        {
            var result = RunTest("test-route").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.RoutedAction)));
        }

        [Test]
        public void MultiLevelRouteTest()
        {
            var result = RunTest("test-route-1", "test-route-2").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.MultiLevelRoutedAction)));
        }

        [Test]
        public void ZeroLevelClassAndMethodRouteTest()
        {
            var result = RunTest("class-test-route-1").ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(nameof(TestProgram.ClassRoutedActions.ZeroLevelRoutedAction)));
        }

        [Test]
        public void SingleLevelClassAndMethodRouteTest()
        {
            var result = RunTest("class-test-route-1", "class-test-route-2")
                .ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(
                    nameof(TestProgram.ClassRoutedActions
                        .SingleLevelClassRoutedActionWithParameter)));
        }

        [Test]
        public void MultiLevelClassAndMethodRouteTest()
        {
            var result = RunTest(
                    "class-test-route-1",
                    "class-test-route-2",
                    "class-test-route-3")
                .ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(
                    nameof(TestProgram.ClassRoutedActions.MultiLevelClassRoutedAction)));
        }
    }
}