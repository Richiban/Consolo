using System;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineAction
    {
        private readonly Action _action;
        private readonly MethodMapping _methodMapping;
        private readonly IObjectFactory _objectFactory;

        public CommandLineAction(MethodMapping methodMapping, IObjectFactory objectFactory)
        {
            _methodMapping = methodMapping;
            _objectFactory = objectFactory;
            _action = CreateAction(methodMapping, objectFactory);
        }

        public string Help { get; }

        internal void Execute() => _action();

        private static Action CreateAction(MethodMapping methodMapping, IObjectFactory objectFactory)
        {
            var instance = CreateInstanceOfDeclaringType(methodMapping, objectFactory);

            var methodArguments = methodMapping.Select(m => m.Value).ToArray();

            return methodMapping.GetInvokeAction(instance, methodArguments);
        }

        private static object CreateInstanceOfDeclaringType(MethodMapping methodMapping, IObjectFactory objectFactory)
        {
            if (methodMapping.IsStatic)
                return null;

            var declaringType = methodMapping.MethodModel.MethodInfo.DeclaringType;

            return objectFactory.CreateInstance(declaringType);
        }
    }
}