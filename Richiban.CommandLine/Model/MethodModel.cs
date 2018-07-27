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
                    return default;
                }
            }

            args = Parameters.ExpandShortForms(args);

            foreach (var prop in Parameters)
            {
                var x = prop.Matches(args, out var argumentsMatched);

                x.IfSome(s =>
                {
                    parameterMappings.Add(s);
                    args = args.Without(argumentsMatched);
                });

                if (x.HasValue == false)
                {
                    return default;
                }
            }

            if (args.Any())
                return default;

            return new MethodMapping(this, parameterMappings);
        }
    }
}