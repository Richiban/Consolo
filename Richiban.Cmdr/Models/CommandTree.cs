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
    public Option<string> Description { get; set; }
    public Option<CommandMethod> Method { get; set; }

    public sealed class SubCommand(string name) : CommandTree
    {
        public string CommandName { get; } = name;
    }

    public sealed class Root : CommandTree
    {
    }
}