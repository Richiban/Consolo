using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class MethodMapping : IReadOnlyList<ParameterMapping>
    {
        private readonly IReadOnlyList<ParameterMapping> _propertyMappings;


        public MethodMapping(
            MethodModel methodModel,
            IReadOnlyList<ParameterMapping> propertyMappings)
        {
            _propertyMappings = propertyMappings;
            MethodModel = methodModel;

            MatchDisambiguation = propertyMappings
                .Any(prop => prop.MatchDisambiguation == MatchDisambiguation.ImplicitMatch)
                ? MatchDisambiguation.ImplicitMatch
                : MatchDisambiguation.ExplicitMatch;
        }
        
        public MethodModel MethodModel { get; }
        public MatchDisambiguation MatchDisambiguation { get; }

        public int Count => _propertyMappings.Count;
        public ParameterMapping this[int index] => _propertyMappings[index];
        public IEnumerator<ParameterMapping> GetEnumerator() => _propertyMappings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
