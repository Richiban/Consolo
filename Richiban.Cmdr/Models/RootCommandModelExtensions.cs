using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Models
{
    internal static class RootCommandModelExtensions
    {
        public static IEnumerable<CommandModel.LeafCommandModel> GetAllLeafCommandModels(
            this CommandModel.RootCommandModel root)
        {
            foreach (var subCommand in root.SubCommands)
            {
                switch (subCommand)
                {
                    case CommandModel.CommandGroupModel subGroup:
                    {
                        foreach (var pair in subGroup.GetAllLeafCommands())
                        {
                            yield return pair;
                        }

                        break;
                    }
                    case CommandModel.LeafCommandModel leafCommand:
                    {
                        yield return leafCommand;

                        break;
                    }
                    default:
                        throw new InvalidOperationException("WAT");
                }
            }
        }

        private static IEnumerable<CommandModel.LeafCommandModel> GetAllLeafCommands(
            this CommandModel.CommandGroupModel group)
        {
            foreach (var subCommand in group.SubCommands)
            {
                switch (subCommand)
                {
                    case CommandModel.CommandGroupModel subGroup:
                    {
                        foreach (var pair in subGroup.GetAllLeafCommands())
                        {
                            yield return pair;
                        }

                        break;
                    }
                    case CommandModel.LeafCommandModel leafCommand:
                    {
                        yield return leafCommand;

                        break;
                    }
                    default:
                        throw new InvalidOperationException("WAT2");
                }
            }
        }
    }
}