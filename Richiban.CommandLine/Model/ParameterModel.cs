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
        public readonly IReadOnlyCollection<char> _shortForms;

        public ParameterModel(ParameterInfo parameterInfo)
        {
            _parameterInfo = parameterInfo;

            IsOptional = parameterInfo.IsOptional;

            var shortFormAttributes =
                parameterInfo
                    .GetCustomAttributes(inherit: true)
                    .OfType<ShortFormAttribute>();

            _shortForms =
                shortFormAttributes.SelectMany(a => a.ShortForms).ToArray();

            _names = BuildNamesList(parameterInfo, _shortForms);

            if (shortFormAttributes.SingleOrDefault()?.DisallowLongForm == true)
                _names = _shortForms.Select(c => c.ToString()).ToList();
            
            HasShortForm = _shortForms.Any();

            PropertyType = parameterInfo.ParameterType;

            IsFlag = PropertyType == typeof(bool);
            var primaryName = _names.First();

            var helpForm =
                IsFlag
                ? $"-{primaryName.ToLowerInvariant()}"
                : $"<{primaryName.ToLowerInvariant()}>";

            Help = IsOptional ? $"[{helpForm}]" : helpForm;
        }

        private static IReadOnlyList<string> BuildNamesList(
            ParameterInfo parameterInfo, IReadOnlyCollection<char> shortForms)
        {
            var allNames = new List<string>
            {
                parameterInfo.Name
            };

            allNames.AddRange(shortForms.Select(c => c.ToString()));

            var parameterNameAttribute = parameterInfo
                .GetCustomAttributes(inherit: true)
                .OfType<ParameterNameAttribute>()
                .SingleOrDefault();

            if(parameterNameAttribute != null)
            {
                allNames.AddRange(parameterNameAttribute.Names);

                if(parameterNameAttribute.IncludeOriginal == false)
                {
                    allNames.Remove(parameterInfo.Name);
                }
            }

            return allNames;
        }
        
        public Type PropertyType { get; }
        public bool IsFlag { get; }
        public string Help { get; }
        public bool IsOptional { get; }
        public bool HasShortForm { get; }

        public bool MatchesShortForm(char c) => _shortForms.Contains(c);

        public Option<PropertyMapping> Matches(
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
                        when NameMatches(bnf.Name) && IsFlag:
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