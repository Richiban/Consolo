using System;
using System.Diagnostics;
using System.Reflection;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("Help")]
    internal class MethodModel
    {
        private readonly string _toString;

        public MethodModel(MethodInfo methodInfo)
        {
            DeclaringType = methodInfo.DeclaringType;

            InvokeFunc = methodInfo.Invoke;

            Verbs = new VerbModel(methodInfo);

            Parameters = new ParameterModelList(methodInfo.GetParameters());

            Help = $"{Verbs.Help} {Parameters.Help}";

            IsStatic = methodInfo.IsStatic;

            _toString = $"{methodInfo.DeclaringType}.{methodInfo.Name}";
        }

        public ParameterModelList Parameters { get; }
        public VerbModel Verbs { get; }
        public string Help { get; }
        public bool IsStatic { get; }
        public Type DeclaringType { get; }
        public Func<object, object[], object> InvokeFunc { get; }

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs) =>
            Verbs.GetPartialMatchAccuracy(commandLineArgs);

        public override string ToString() => _toString;
    }
}