using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr;

internal abstract class CommandTree
{
    private CommandTree()
    {
    }

    public List<SubCommand> SubCommands { get; } = new();
    public Option<CommandMethod> Method { get; set; }
    public IReadOnlyCollection<CommandParameterModel> Parameters { get; set; } = [];
    public Option<string> Description { get; set; }

    public int MandatoryParameterCount => 
        Parameters
            .OfType<CommandParameterModel.CommandPositionalParameterModel>()
            .Count();

    public int OptionalParameterCount => 
        Parameters
            .OfType<CommandParameterModel.CommandOptionalPositionalParameterModel>()
            .Count();

    public int FlagCount =>
        Parameters
            .OfType<CommandParameterModel.CommandFlagModel>()
            .Count();

    public sealed class SubCommand(string name) : CommandTree
    {
        public string CommandName { get; } = name;
    }

    public sealed class Root : CommandTree
    {
    }
}