using System;
using System.Collections.Generic;

namespace Richiban.Cmdr;

internal static class RootCommandModelExtensions
{
    public static IEnumerable<CommandTree.SubCommand>
        GetDescendentCommands(this CommandTree root)
    {
        foreach (var subCommand in root.SubCommands)
        {
            yield return subCommand;

            foreach (var subCommandSubCommand in GetDescendentCommands(subCommand))
            {
                yield return (subCommandSubCommand);
            }
        }
    }
}