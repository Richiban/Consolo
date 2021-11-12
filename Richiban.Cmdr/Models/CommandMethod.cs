using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Models
{
    internal class CommandMethod
    {
        public CommandMethod(
            string fullyQualifiedClassName,
            string methodName,
            IReadOnlyCollection<CommandParameterModel> parameters)
        {
            Parameters = parameters;
            Name = methodName;
            FullyQualifiedName = $"{fullyQualifiedClassName}.{methodName}";
        }

        public IReadOnlyCollection<CommandParameterModel> Parameters { get; }
        public string FullyQualifiedName { get; }
        public string Name { get; }

        public override string ToString() => FullyQualifiedName;
    }
}