using System;

namespace Richiban.CommandLine
{
    internal class CommandLineAction
    {
        private readonly Func<object> _action;

        public CommandLineAction(Func<object> action, string help)
        {
            _action = action;
            Help = help;
        }

        public string Help { get; }

        [TracerAttributes.TraceOn]
        public object Invoke() => _action();
    }
}