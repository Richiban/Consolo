using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Richiban.Cmdr;

internal abstract class CommandTree
{
    private CommandTree()
    {
    }

    public List<SubCommand> SubCommands { get; } = new();
    public Option<CommandMethod> Method { get; set; }
    public IReadOnlyCollection<CommandParameter> Parameters { get; set; } = [];
    public Option<string> Description { get; set; }

    public ImmutableArray<CommandParameter.Positional> MandatoryParameters => 
        Parameters
            .OfType<CommandParameter.Positional>()
            .ToImmutableArray();

    public ImmutableArray<CommandParameter.OptionalPositional> OptionalParameters => 
        Parameters
            .OfType<CommandParameter.OptionalPositional>()
            .ToImmutableArray();

    public ImmutableArray<CommandParameter.Flag> Flags =>
        Parameters
            .OfType<CommandParameter.Flag>()
            .ToImmutableArray();

    public int MandatoryParameterCount => 
        Parameters
            .OfType<CommandParameter.Positional>()
            .Count();

    public int OptionalParameterCount => 
        Parameters
            .OfType<CommandParameter.OptionalPositional>()
            .Count();

    public int FlagCount =>
        Parameters
            .OfType<CommandParameter.Flag>()
            .Count();

    public sealed class SubCommand(string name) : CommandTree
    {
        public string CommandName { get; } = name;
    }

    public sealed class Root : CommandTree
    {
    }
}