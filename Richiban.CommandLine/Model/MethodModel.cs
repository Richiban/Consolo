using AutoLazy;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("Help")]
    internal class MethodModel
    {
        private readonly MethodInfo _methodInfo;

        public MethodModel(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;

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

        [Lazy]
        public override string ToString() => $"{_methodInfo.DeclaringType}.{_methodInfo.Name}";
    }
}