using System;

namespace Richiban.Cmdr.Tests.DependencyInjection
{
    internal class SomeDependency : ISomeDependency
    {
        public string SomeMessage => "This is a message from the dependency";
    }
}