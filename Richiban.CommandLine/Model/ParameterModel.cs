using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class ParameterModel
    {
        private readonly IReadOnlyList<ParameterName> _names;
        private readonly ParameterInfo _parameterInfo;

        public ParameterModel(ParameterInfo parameterInfo)
        {
            _parameterInfo = parameterInfo;

            OriginalName = parameterInfo.Name;

            IsOptional = parameterInfo.IsOptional;

            _names = BuildNamesList(parameterInfo);

            ParameterType = parameterInfo.ParameterType;

            IsFlag = ParameterType == typeof(bool);

            Names = _names;
        }

        private static IReadOnlyList<ParameterName> BuildNamesList(ParameterInfo parameterInfo)
        {
            var shortFormAttributes =
                parameterInfo
                    .GetCustomAttributes(inherit: true)
                    .OfType<ShortFormAttribute>();

            var allNames = new List<ParameterName>();

            allNames.AddRange(
                shortFormAttributes
                    .SelectMany(a => a.ShortForms)
                    .Select(c => new ParameterName.ShortForm(c)));

            if (shortFormAttributes.Any(a => a.DisallowLongForm))
                return allNames;

            var originalName = new ParameterName.LongForm(parameterInfo.Name);
            allNames.Add(originalName);

            var parameterNameAttribute = parameterInfo
                .GetCustomAttributes(inherit: true)
                .OfType<ParameterNameAttribute>()
                .SingleOrDefault();

            if(parameterNameAttribute != null)
            {
                allNames.AddRange(parameterNameAttribute.Names.Select(n => new ParameterName.LongForm(n)));

                if(parameterNameAttribute.IncludeOriginal == false)
                {
                    allNames.Remove(originalName);
                }
            }

            return allNames;
        }
        
        public Type ParameterType { get; }
        public bool IsFlag { get; }
        public bool IsOptional { get; }
        public bool HasShortForm { get; }
        public IReadOnlyList<ParameterName> Names { get; }
        public string OriginalName { get; }

        public bool MatchesShortForm(char c) => _names.Any(n => n.Matches(c));

        public bool NameMatches(string argumentName) =>
            _names.Any(n => n.Matches(argumentName));
    }
}