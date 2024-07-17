using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr;

internal abstract class CommandModel
{
    private CommandModel()
    {
    }

    public List<SubCommandModel> SubCommands { get; } = new();
    public Option<CommandMethod> Method { get; set; }
    public IReadOnlyCollection<CommandParameterModel> Parameters { get; init; } = [];
    public Option<string> Description { get; init; }

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

    public sealed class SubCommandModel(string name) : CommandModel
    {
        public string CommandName { get; } = name;
    }

    public sealed class RootCommandModel : CommandModel
    {
    }
}