using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("{Help}")]
    internal class MethodModel
    {
        private readonly MethodInfo _methodInfo;

        public MethodModel(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            var verbAttribute = methodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<VerbAttribute>()
                .SingleOrDefault();

            Verbs = new VerbCollection(methodInfo.Name, verbAttribute);

            Parameters = new ParameterModelList(methodInfo.GetParameters());

            Help = $"{Verbs.Help} {Parameters.Help}";
        }

        public ParameterModelList Parameters { get; }
        public VerbCollection Verbs { get; }
        public string Help { get; }

        public CommandLineAction CreateAction(IReadOnlyList<PropertyMapping> mappings)
        {
            var instance = Activator.CreateInstance(_methodInfo.DeclaringType);

            var methodArguments =
                mappings.Select(m => m.Value).ToArray();

            return new CommandLineAction(() => _methodInfo.Invoke(instance, methodArguments), Help);
        }
    }
}