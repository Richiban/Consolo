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

            var verbAttributes = methodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<VerbAttribute>()
                .ToArray();

            var verbSequenceAttributes =
                methodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<VerbSequenceAttribute>()
                .ToArray();

            Verbs = new VerbModel(methodInfo.Name, verbAttributes, verbSequenceAttributes);

            Parameters = new ParameterModelList(methodInfo.GetParameters());

            Help = $"{Verbs.Help} {Parameters.Help}";

            IsStatic = methodInfo.IsStatic;
        }

        public ParameterModelList Parameters { get; }
        public VerbModel Verbs { get; }
        public string Help { get; }
        public bool IsStatic { get; }
        public MethodInfo MethodInfo { get; }
    }
}