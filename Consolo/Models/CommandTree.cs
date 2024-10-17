using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Consolo;

internal abstract class CommandTree
{
    private CommandTree()
    {
    }

    public List<SubCommand> SubCommands { get; } = new();
    public Option<string> Description { get; set; }
    public Option<CommandMethod> Method { get; set; }

    public sealed class SubCommand(string name, bool showInHelp = true) : CommandTree
    {
        public string CommandName { get; } = name;
        public bool ShowInHelp { get; } = showInHelp;
    }

    public sealed class Root : CommandTree
    {
    }
}