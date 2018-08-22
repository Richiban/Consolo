using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal class ParameterMapping
    {
        public ParameterMapping(
            ParameterModel parameterModel,
            MatchDisambiguation matchDisambiguation,
            params string[] suppliedValues) : this(
                parameterModel, matchDisambiguation, (IReadOnlyList<string>)suppliedValues)
        {
        }

        public ParameterMapping(
            ParameterModel parameterModel,
            MatchDisambiguation matchDisambiguation,
            IReadOnlyList<string> suppliedValues)
        {
            MatchDisambiguation = matchDisambiguation;
            SuppliedValues = suppliedValues;
            ConvertToType = parameterModel.ParameterType;
        }

        public MatchDisambiguation MatchDisambiguation { get; }
        public IReadOnlyList<string> SuppliedValues { get; }
        public Type ConvertToType { get; }
    }
}