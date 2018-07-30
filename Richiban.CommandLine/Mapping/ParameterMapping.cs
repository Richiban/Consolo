using System;

namespace Richiban.CommandLine
{
    internal class ParameterMapping
    {
        public ParameterMapping(
            Type parameterType,
            Option<string> suppliedValue,
            MatchDisambiguation matchDisambiguation)
        {
            MatchDisambiguation = matchDisambiguation;
            SuppliedValue = suppliedValue;
            ConvertToType = parameterType;
        }
        
        public MatchDisambiguation MatchDisambiguation { get; }
        public Option<string> SuppliedValue { get; }
        public Type ConvertToType { get; }
    }
}