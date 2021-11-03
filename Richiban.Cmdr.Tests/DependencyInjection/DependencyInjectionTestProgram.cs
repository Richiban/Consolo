using System;
using System.Collections.Generic;
using System.Text;

namespace Richiban.Cmdr.Tests.DependencyInjection
{
    [Route]
    class DependencyInjectionTestProgram
    {
        private readonly ISomeDependency _someDependency;

        public DependencyInjectionTestProgram(ISomeDependency someDependency)
        {
            _someDependency = someDependency;
        }

        [CommandLine, Route]
        public object ActionRequiringDependency()
        {
            return new
            {
                ExecutedAction = nameof(ActionRequiringDependency),
                Output = _someDependency.SomeMessage
            };
        }
    }
}
