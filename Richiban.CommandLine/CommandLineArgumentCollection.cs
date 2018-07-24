using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineArgumentCollection : IReadOnlyList<CommandLineArgument>
    {
        private readonly IReadOnlyList<CommandLineArgument> _args;

        private CommandLineArgumentCollection(IReadOnlyList<CommandLineArgument> args) =>
            _args = args;

        public static CommandLineArgumentCollection Parse(string[] args)
        {
            return new CommandLineArgumentCollection(args.SelectMany(CommandLineArgument.Parse).ToList());
        }

        public int Count => _args.Count;
        public CommandLineArgument this[int index] => _args[index];
        public IEnumerator<CommandLineArgument> GetEnumerator() => _args.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CommandLineArgumentCollection Without(CommandLineArgument commandLineArgument) =>
            new CommandLineArgumentCollection(_args.Where(x => x != commandLineArgument).ToList());
    }
}
