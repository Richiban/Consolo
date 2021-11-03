using System;

namespace Richiban.Cmdr.Tests.DependencyInjection
{
    [Route]
    internal class DependencyInjectionTestProgram
    {
        private readonly ISomeDependency _someDependency;

        public DependencyInjectionTestProgram(ISomeDependency someDependency)
        {
            _someDependency = someDependency;
        }

        [CommandLine, Route]
        public object ActionRequiringDependency() =>
            new
            {
                ExecutedAction = nameof(ActionRequiringDependency),
                Output = _someDependency.SomeMessage
            };
    }
}