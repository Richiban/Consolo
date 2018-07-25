using System;

namespace Richiban.CommandLine
{
    internal class CommandLineAction
    {
        private readonly Action _action;

        public CommandLineAction(Action action) => _action = action;

        internal void Execute() => _action();
    }
}