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

            ParameterType = parameterInfo.ParameterType;

            IsFlag = ParameterType == typeof(bool);
            var primaryName = _names.First();

            Name = primaryName;
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
        
        public Type ParameterType { get; }
        public bool IsFlag { get; }
        public bool IsOptional { get; }
        public bool HasShortForm { get; }
        public string Name { get; }

        public bool MatchesShortForm(char c) => _shortForms.Contains(c);

        public bool NameMatches(string argumentName) =>
            _names.Any(n => n.StartsWith(argumentName, StringComparison.CurrentCultureIgnoreCase));
    }
}