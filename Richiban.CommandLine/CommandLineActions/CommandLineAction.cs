using System;

namespace Richiban.CommandLine
{
    internal class CommandLineAction
    {
        private readonly Action _action;

        public CommandLineAction(Action action, string help)
        {
            _action = action;
            Help = help;
        }

        public string Help { get; }

        public void Invoke() => _action();
    }
}