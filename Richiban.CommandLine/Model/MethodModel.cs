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
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            Verbs = new VerbModel(methodInfo);

            Parameters = new ParameterModelList(methodInfo.GetParameters());

            Help = $"{Verbs.Help} {Parameters.Help}";

            IsStatic = methodInfo.IsStatic;
        }

        private ParameterModelList Parameters { get; }
        public VerbModel Verbs { get; }
        public string Help { get; }
        public bool IsStatic { get; }
        public MethodInfo MethodInfo { get; }

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs) =>
            Verbs.GetPartialMatchAccuracy(commandLineArgs);

        public Option<MethodMapping> GetMethodMapping(CommandLineArgumentList args)
        {
            var parameterMappings = new List<PropertyMapping>();

            {
                if (Verbs.Matches(args, out var argumentsMatched))
                {
                    args = args.Without(argumentsMatched);
                }
                else
                {
                    return None;
                }
            }

            args = Parameters.ExpandShortForms(args);

            foreach (var prop in Parameters)
            {
                var maybePropertyMapping = prop.Matches(args, out var argumentsMatched);

                maybePropertyMapping.IfSome(s =>
                {
                    parameterMappings.Add(s);
                    args = args.Without(argumentsMatched);
                });

                if (maybePropertyMapping.HasValue == false)
                {
                    return None;
                }
            }

            if (args.Any())
                return None;

            return new MethodMapping(this, parameterMappings);
        }
    }
}