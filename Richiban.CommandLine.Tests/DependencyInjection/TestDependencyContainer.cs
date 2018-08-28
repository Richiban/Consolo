using System;

namespace Richiban.CommandLine.Tests.DependencyInjection
{
    internal class TestDependencyContainer
    {
        public object Resolve(Type typeToInstantiate)
        {
            if (typeToInstantiate == typeof(DependencyInjectionTestProgram))
                return new DependencyInjectionTestProgram((ISomeDependency)Resolve(typeof(ISomeDependency)));

            if (typeToInstantiate == typeof(ISomeDependency))
                return new SomeDependency();

            throw new InvalidOperationException($"The type {typeToInstantiate} has not been registered");
        }
    }
}