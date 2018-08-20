using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using System;

namespace Richiban.CommandLine
{
    internal class CommandLineArgumentList : IReadOnlyList<CommandLineArgument>
    {
        private readonly IReadOnlyList<CommandLineArgument> _args;

        private CommandLineArgumentList(
            IReadOnlyList<CommandLineArgument> args,
            bool isCallForHelp,
            bool traceToStandardOutput)
        {
            _args = args;
            IsCallForHelp = isCallForHelp;
            TraceToStandardOutput = traceToStandardOutput;
        }

        public static CommandLineArgumentList Parse(string[] args)
        {
            var parsedArgs = args.Select(CommandLineArgument.Parse).ToList();

            var helpSwitches = parsedArgs.OfType<CommandLineArgument.HelpSwitch>().ToList();

            foreach (var helpSwitch in helpSwitches)
            {
                parsedArgs.Remove(helpSwitch);
            }

            var diagnosticSwitches =
                parsedArgs.OfType<CommandLineArgument.DiagnosticSwitch>().ToList();

            foreach (var diagnosticSwitch in diagnosticSwitches)
            {
                parsedArgs.Remove(diagnosticSwitch);
            }

            return new CommandLineArgumentList(
                parsedArgs,
                helpSwitches.Any(),
                diagnosticSwitches.Any());
        }

        public int Count => _args.Count;

        public bool IsCallForHelp { get; }
        public bool TraceToStandardOutput { get; }

        public CommandLineArgument this[int index] => _args[index];
        public IEnumerator<CommandLineArgument> GetEnumerator() => _args.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CommandLineArgumentList Without(IEnumerable<CommandLineArgument> commandLineArguments) =>
            new CommandLineArgumentList(
                _args.Except(commandLineArguments).ToList(),
                IsCallForHelp,
                TraceToStandardOutput);

        public CommandLineArgumentList ExpandShortFormArgument(
            CommandLineArgument.BareNameOrFlag argumentToExpand)
        {
            var newArgumentList = this.ToList();

            newArgumentList.Remove(argumentToExpand);

            foreach (var c in argumentToExpand.Name.ToCharArray())
            {
                newArgumentList.Add(new CommandLineArgument.BareNameOrFlag(
                    c.ToString(),
                    $"{CommandLineEnvironment.FlagGlyph}{c}"));
            }

            return new CommandLineArgumentList(
                newArgumentList,
                IsCallForHelp,
                TraceToStandardOutput);
        }

        public override string ToString() => string.Join(" ", _args);
    }
}
