using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineArgumentList : IReadOnlyList<CommandLineArgument>
    {
        private readonly IReadOnlyList<CommandLineArgument> _args;

        private CommandLineArgumentList(IReadOnlyList<CommandLineArgument> args) =>
            _args = args;

        public static CommandLineArgumentList Parse(string[] args)
        {
            return new CommandLineArgumentList(args.SelectMany(CommandLineArgument.Parse).ToList());
        }

        public int Count => _args.Count;
        public CommandLineArgument this[int index] => _args[index];
        public IEnumerator<CommandLineArgument> GetEnumerator() => _args.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CommandLineArgumentList Without(CommandLineArgument commandLineArgument) =>
            new CommandLineArgumentList(_args.Where(x => x != commandLineArgument).ToList());
    }
}
