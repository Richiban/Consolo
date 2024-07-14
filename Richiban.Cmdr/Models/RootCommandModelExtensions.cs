using System;
using System.Collections.Generic;

namespace Richiban.Cmdr;

internal static class RootCommandModelExtensions
{
    public static IEnumerable<CommandModel.SubCommandModel>
        GetDescendentCommands(this CommandModel root)
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