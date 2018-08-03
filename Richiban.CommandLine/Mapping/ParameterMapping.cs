using System;
using System.Collections.Generic;
using System.Linq;
using AutoLazy;

namespace Richiban.CommandLine
{
    internal class ParameterMapping
    {
        public ParameterMapping(
            ParameterModel parameterModel,
            MatchDisambiguation matchDisambiguation,
            params string[] suppliedValues)
        {
            MatchDisambiguation = matchDisambiguation;
            SuppliedValues = suppliedValues;
            ConvertToType = parameterModel.ParameterType;
            Name = parameterModel.Name;
        }
        
        public MatchDisambiguation MatchDisambiguation { get; }
        public IReadOnlyList<string> SuppliedValues { get; }
        public Type ConvertToType { get; }
        public string Name { get; }

        [Lazy]
        public override string ToString()
        {
            if (SuppliedValues.Count == 0)
                return $"{Name} = <default>";
            if (SuppliedValues.Count == 1)
                return $"{Name} = {SuppliedValues.First()}";
            return $"{Name}: ({String.Join(", ", SuppliedValues)})";
        }
    }
}