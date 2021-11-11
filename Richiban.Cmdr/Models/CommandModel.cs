using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Models
{
    internal abstract class CommandModel
    {
        private CommandModel()
        {
        }

        public sealed class NormalCommandModel : CommandModel
        {
            public NormalCommandModel(
                string commandName,
                string fullyQualifiedClassName,
                string methodName,
                IReadOnlyCollection<CommandParameterModel> parameters,
                IReadOnlyCollection<NormalCommandModel> subCommands)
            {
                CommandName = commandName;
                Parameters = parameters;
                SubCommands = subCommands;
                FullyQualifiedName = $"{fullyQualifiedClassName}.{methodName}";
                VariableName = $"{Utils.ToCamelCase(methodName)}Command";
            }

            public string CommandName { get; }
            public IReadOnlyCollection<CommandParameterModel> Parameters { get; }
            public IReadOnlyCollection<NormalCommandModel> SubCommands { get; }
            public string FullyQualifiedName { get; }
            public string VariableName { get; }
        }

        public sealed class RootCommandModel : CommandModel
        {
            public RootCommandModel(IReadOnlyCollection<CommandModel> subCommands)
            {
                SubCommands = subCommands;
            }

            public IReadOnlyCollection<CommandModel> SubCommands { get; }
        }
    }
}