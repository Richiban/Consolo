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
        public MethodModel(MethodInfo methodInfo)
        {
            DeclaringType = methodInfo.DeclaringType;

            InvokeFunc = methodInfo.Invoke;

            Verbs = new VerbModel(methodInfo);

            Parameters = new ParameterModelList(methodInfo.GetParameters());

            Help = $"{Verbs.Help} {Parameters.Help}";

            IsStatic = methodInfo.IsStatic;
        }

        public ParameterModelList Parameters { get; }
        public VerbModel Verbs { get; }
        public string Help { get; }
        public bool IsStatic { get; }
        public Type DeclaringType { get; }
        public Func<object, object[], object> InvokeFunc { get; }

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs) =>
            Verbs.GetPartialMatchAccuracy(commandLineArgs);
    }
}