using System;

namespace Richiban.CommandLine
{
    internal class CommandLineAction
    {
        private readonly Action _action;

        public CommandLineAction(Action action, string help) => 
            (_action, Help) = (action, help);

        public string Help { get; }

        internal void Execute() => _action();
    }
}