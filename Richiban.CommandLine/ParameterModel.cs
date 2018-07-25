using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("{Help}")]
    internal class ParameterModel
    {
        private readonly IReadOnlyCollection<string> _names;
        private ParameterInfo _parameterInfo;

        public ParameterModel(ParameterInfo parameterInfo)
        {
            _parameterInfo = parameterInfo;

            IsOptional = parameterInfo.IsOptional;

            Name = parameterInfo.Name;

            ShortForms =
                parameterInfo
                    .GetCustomAttributes(inherit: true)
                    .OfType<ShortFormAttribute>()
                    .SelectMany(a => a.ShortForms)
                    .ToArray();

            _names =
                (new[] { parameterInfo.Name })
                .Concat(parameterInfo
                    .GetCustomAttributes(inherit: true)
                    .OfType<AlternativeNameAttribute>()
                    .SelectMany(attr => attr.AlternativeNames))
                .Concat(ShortForms.Select(c => c.ToString()))
                .ToList();

            HasShortForm = ShortForms.Any();

            PropertyType = parameterInfo.ParameterType;

            var helpForm = $"<{Name.ToLowerInvariant()}>";

            Help = IsOptional ? $"[{helpForm}]" : helpForm;
        }

        public string Name { get; }
        public Type PropertyType { get; }
        public string Help { get; }
        public bool IsOptional { get; }
        public bool HasShortForm { get; }
        public IReadOnlyCollection<char> ShortForms { get; }

        internal Option<PropertyMapping> Matches(
            CommandLineArgumentList args, 
            out CommandLineArgument[] argumentsMatched)
        {
            var enumerator = args.GetEnumerator();

            while(enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case CommandLineArgument.NameValuePair nv when NameMatches(nv.Name):
                        argumentsMatched = new[] { nv };
                        return new PropertyMapping(this, nv.Value);

                    case CommandLineArgument.BareNameOrFlag bnf 
                        when NameMatches(bnf.Name) && PropertyType == typeof(bool):
                        argumentsMatched = new[] { bnf };
                        return new PropertyMapping(this, true);

                    case CommandLineArgument.BareNameOrFlag bnf when NameMatches(bnf.Name):
                        if(enumerator.MoveNext())
                        {
                            if (enumerator.Current is CommandLineArgument.Free free)
                            {
                                argumentsMatched = new CommandLineArgument[] { bnf, free };
                                return new PropertyMapping(this, free.Value);
                            }
                        }

                        break;

                    case CommandLineArgument.Free free:
                        argumentsMatched = new[] { free };
                        return new PropertyMapping(this, free.Value);

                    default:
                        break;
                }
            }

            argumentsMatched = new CommandLineArgument[0];

            if(IsOptional)
            {
                return new PropertyMapping(this, Type.Missing);
            }

            return default;
        }

        private bool NameMatches(string argumentName) =>
            _names.Any(n => n.StartsWith(argumentName, StringComparison.CurrentCultureIgnoreCase));
    }
}