using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal class ParameterMapping
    {
        public ParameterMapping(
            Type parameterType,
            IReadOnlyList<string> suppliedValues,
            MatchDisambiguation matchDisambiguation)
        {
            MatchDisambiguation = matchDisambiguation;
            SuppliedValues = suppliedValues;
            ConvertToType = parameterType;
        }
        
        public MatchDisambiguation MatchDisambiguation { get; }
        public IReadOnlyList<string> SuppliedValues { get; }
        public Type ConvertToType { get; }
    }
}