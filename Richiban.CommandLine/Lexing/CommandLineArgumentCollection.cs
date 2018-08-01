using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using System;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("{ToString()}")]
    internal class CommandLineArgumentList : IReadOnlyList<CommandLineArgument>
    {
        private readonly IReadOnlyList<CommandLineArgument> _args;

        private CommandLineArgumentList(
            IReadOnlyList<CommandLineArgument> args,
            bool isCallForHelp,
            bool redirectDiagnosticsToStandardOutput)
        {
            _args = args;
            IsCallForHelp = isCallForHelp;
            RedirectDiagnosticsToStandardOutput = redirectDiagnosticsToStandardOutput;
        }

        public static CommandLineArgumentList Parse(string[] args)
        {
            var parsedArgs = args.Select(CommandLineArgument.Parse).ToList();

            var helpGlyphs = parsedArgs.OfType<CommandLineArgument.HelpGlyph>().ToList();

            foreach(var helpGlyph in helpGlyphs)
            {
                parsedArgs.Remove(helpGlyph);
            }

            var diagnostics = 
                parsedArgs.OfType<CommandLineArgument.DiagnosticSwitch>().ToList();

            foreach (var x in diagnostics)
            {
                parsedArgs.Remove(x);
            }

            return new CommandLineArgumentList(
                parsedArgs, 
                helpGlyphs.Any(),
                diagnostics.Any());
        }

        public int Count => _args.Count;

        public bool IsCallForHelp { get; }
        public bool RedirectDiagnosticsToStandardOutput { get; }

        public CommandLineArgument this[int index] => _args[index];
        public IEnumerator<CommandLineArgument> GetEnumerator() => _args.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CommandLineArgumentList Without(IEnumerable<CommandLineArgument> commandLineArguments) =>
            new CommandLineArgumentList(
                _args.Except(commandLineArguments).ToList(),
                IsCallForHelp,
                RedirectDiagnosticsToStandardOutput);

        public CommandLineArgumentList ExpandShortFormArgument(
            CommandLineArgument.BareNameOrFlag argumentToExpand)
        {
            var newArgumentList = this.ToList();

            newArgumentList.Remove(argumentToExpand);

            foreach (var c in argumentToExpand.Name.ToCharArray())
            {
                newArgumentList.Add(new CommandLineArgument.BareNameOrFlag(c.ToString(), $"-{c}"));
            }

            return new CommandLineArgumentList(
                newArgumentList,
                IsCallForHelp,
                RedirectDiagnosticsToStandardOutput);
        }

        public override string ToString() => string.Join(" ", _args);
    }
}
