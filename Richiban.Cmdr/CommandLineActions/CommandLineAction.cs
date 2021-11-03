using System;

namespace Richiban.Cmdr
{
    internal class CommandLineAction
    {
        private readonly Func<object> _action;

        public CommandLineAction(Func<object> action, MethodModel model)
        {
            _action = action;
            Model = model;
        }

        public MethodModel Model { get; }

        [TracerAttributes.TraceOn]
        public object Invoke() => _action();
    }
}