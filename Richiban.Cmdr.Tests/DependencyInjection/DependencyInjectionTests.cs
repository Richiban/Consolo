using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests.DependencyInjection
{
    internal class DependencyInjectionTests : CommandLineTest
    {
        [SetUp]
        public void SetUp()
        {
            var container = new TestDependencyContainer();
            _configuration.ObjectFactory = container.Resolve;
        }

        [Test]
        public void InstantiateClassWithDependency()
        {
            var result = RunTest(
                    "DependencyInjectionTestProgram",
                    "ActionRequiringDependency")
                .ProgramOutput;

            Assert.That(
                result.ExecutedAction,
                Is.EqualTo(
                    nameof(DependencyInjectionTestProgram.ActionRequiringDependency)));

            Assert.That(
                result.Output,
                Is.EqualTo("This is a message from the dependency"));
        }
    }
}