using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("{Help}")]
    internal class MethodModel
    {
        public MethodModel(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            var verbAttribute = methodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<VerbAttribute>()
                .SingleOrDefault();

            Verbs = new VerbCollection(methodInfo.Name, verbAttribute);

            Parameters = new ParameterModelList(methodInfo.GetParameters());

            Help = $"{Verbs.Help} {Parameters.Help}";

            IsStatic = methodInfo.IsStatic;
        }

        public ParameterModelList Parameters { get; }
        public VerbCollection Verbs { get; }
        public string Help { get; }
        public bool IsStatic { get; }
        public MethodInfo MethodInfo { get; }
    }
}