using System.Collections.Generic;

namespace Richiban.Cmdr.Models
{
    abstract class CommandModel
    {
        private CommandModel()
        {
        }

        public string Name { get; }

        public sealed class LeafCommandModel : CommandModel
        {
            public CommandParameterModel[] Parameters { get; set; }
        }

        public sealed class ParentCommandModel : CommandModel
        {
            public IReadOnlyCollection<CommandModel> SubCommands { get; }
        }
    }
}