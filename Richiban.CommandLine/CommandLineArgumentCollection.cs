using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using System;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("{string.Join(\" \", _args)}")]
    internal class CommandLineArgumentList : IReadOnlyList<CommandLineArgument>
    {
        private readonly IReadOnlyList<CommandLineArgument> _args;

        private CommandLineArgumentList(IReadOnlyList<CommandLineArgument> args) =>
            _args = args;

        public static CommandLineArgumentList Parse(string[] args) =>
            new CommandLineArgumentList(args.Select(CommandLineArgument.Parse).ToList());

        public int Count => _args.Count;
        public CommandLineArgument this[int index] => _args[index];
        public IEnumerator<CommandLineArgument> GetEnumerator() => _args.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CommandLineArgumentList Without(CommandLineArgument[] commandLineArguments) =>
            new CommandLineArgumentList(_args.Except(commandLineArguments).ToList());

        public CommandLineArgumentList ExpandShortFormArgument(
            CommandLineArgument.BareNameOrFlag argumentToExpand)
        {
            var newArgumentList = this.ToList();

            newArgumentList.Remove(argumentToExpand);

            foreach (var c in argumentToExpand.Name.ToCharArray())
            {
                newArgumentList.Add(new CommandLineArgument.BareNameOrFlag(c.ToString()));
            }

            return new CommandLineArgumentList(newArgumentList);
        }
    }
}
