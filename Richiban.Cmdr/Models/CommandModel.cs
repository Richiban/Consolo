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

        public sealed class NormalCommandModel : CommandModel
        {
            public string? CommandName { get; init; }
            public override CommandMethod? Method { get; set; }
            public override List<NormalCommandModel> SubCommands { get; } = new();

            public string VariableName
            {
                get
                {
                    if (Method != null)
                    {
                        return StringUtils.ToCamelCase(Method.Name) + "Command";
                    }

                    if (CommandName != null)
                    {
                        return StringUtils.ToCamelCase(CommandName) + "Command";
                    }

                    return "unknownCommand";
                }
            }

            public override string ToString() =>
                $"Command({CommandName})[{SubCommands.Count}] => {{{Method?.ToString() ?? "<null>"}}}";
        }

        public sealed class RootCommandModel : CommandModel
        {
            public override List<NormalCommandModel> SubCommands { get; } = new();
            public override CommandMethod? Method { get; set; }

            public override string ToString() =>
                $"RootCommand[{SubCommands.Count}] => {{{Method?.ToString() ?? "<null>"}}}";
        }

        public abstract List<NormalCommandModel> SubCommands { get; }
        public abstract CommandMethod? Method { get; set; }
    }
}