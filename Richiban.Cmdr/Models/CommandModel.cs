using System;
using System.Collections.Generic;
using Richiban.Cmdr.Utils;

namespace Richiban.Cmdr.Models
{
    internal abstract class CommandModel
    {
        private CommandModel()
        {
        }

        public sealed class LeafCommandModel : CommandModel
        {
            public LeafCommandModel(
                string commandName,
                string fullyQualifiedClassName,
                string methodName,
                IReadOnlyCollection<CommandParameterModel> parameters)
            {
                CommandName = commandName;
                Parameters = parameters;
                FullyQualifiedName = $"{fullyQualifiedClassName}.{methodName}";
                VariableName = $"{StringUtils.ToCamelCase(methodName)}Command";
            }

            public string CommandName { get; }
            public IReadOnlyCollection<CommandParameterModel> Parameters { get; }
            public string FullyQualifiedName { get; }
            public string VariableName { get; }
        }

        public sealed class CommandGroupModel : CommandModel
        {
            public CommandGroupModel(
                string commandName,
                IReadOnlyCollection<CommandModel> subCommands)
            {
                CommandName = commandName;
                SubCommands = subCommands;
            }

            public string CommandName { get; }
            public IReadOnlyCollection<CommandModel> SubCommands { get; }
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