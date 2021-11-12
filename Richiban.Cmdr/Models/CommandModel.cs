using System;
using System.Collections.Generic;
using Richiban.Cmdr.Utils;

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {
    }
}

namespace Richiban.Cmdr.Models
{
    internal abstract class CommandModel
    {
        private CommandModel()
        {
        }

        public sealed class SubCommandModel : CommandModel
        {
            public string? CommandName { get; init; }

            public override string GetVariableName()
            {
                if (Method != null)
                {
                    return StringUtils.ToCamelCase(Method.Name) + "Command";
                }

                if (CommandName != null)
                {
                    return StringUtils.ToCamelCase(CommandName) + "Command";
                }

                throw new InvalidOperationException(
                    "Neither the method nor the command name have been set on this object");
            }

            public override string ToString() =>
                $"Command({CommandName})[{SubCommands.Count}] => {{{Method?.ToString() ?? "<null>"}}}";
        }

        public sealed class RootCommandModel : CommandModel
        {
            public override string GetVariableName() => "rootCommand";

            public override string ToString() =>
                $"RootCommand[{SubCommands.Count}] => {{{Method?.ToString() ?? "<null>"}}}";
        }

        public List<SubCommandModel> SubCommands { get; } = new();
        public CommandMethod? Method { get; set; }
        public abstract string GetVariableName();
    }
}