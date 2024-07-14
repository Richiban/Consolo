using System;
using System.Collections.Generic;

namespace Richiban.Cmdr;

internal abstract class CommandModel
{
    private CommandModel()
    {
    }

    public sealed class SubCommandModel(string name) : CommandModel
    {
        public string CommandName { get; } = name;
        public Option<string> Description { get; init; }

        public override string GetVariableName()
        {
            if (Method != null)
            {
                return StringUtils.ToCamelCase(Method.MethodName) + "Command";
            }

            if (CommandName != null)
            {
                return StringUtils.ToCamelCase(CommandName) + "Command";
            }

            throw new InvalidCommandDefinitionException(
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

    private class InvalidCommandDefinitionException : Exception
    {
        public InvalidCommandDefinitionException(string message) : base(message)
        {
        }
    }
}