using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static Richiban.CommandLine.Prelude;

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

        private ParameterModelList Parameters { get; }
        public VerbModel Verbs { get; }
        public string Help { get; }
        public bool IsStatic { get; }
        public Type DeclaringType { get; }
        public Func<object, object[], object> InvokeFunc { get; }

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs) =>
            Verbs.GetPartialMatchAccuracy(commandLineArgs);

        public Option<MethodMapping> GetMethodMapping(CommandLineArgumentList args)
        {
            var remainingArgs = args;
            var parameterMappings = new List<ParameterMapping>();

            {
                if (Verbs.Matches(remainingArgs, out var argumentsMatched))
                {
                    remainingArgs = remainingArgs.Without(argumentsMatched);
                }
                else
                {
                    return None;
                }
            }

            remainingArgs = Parameters.ExpandShortForms(remainingArgs);

            foreach (var prop in Parameters)
            {
                var maybePropertyMapping = prop.Matches(remainingArgs, out var argumentsMatched);

                maybePropertyMapping.IfSome(s =>
                {
                    parameterMappings.Add(s);
                    remainingArgs = remainingArgs.Without(argumentsMatched);
                });

                if (maybePropertyMapping.HasValue == false)
                {
                    return None;
                }
            }

            if (remainingArgs.Any())
                return None;

            return new MethodMapping(this, parameterMappings);
        }
    }
}