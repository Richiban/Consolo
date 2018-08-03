using AutoLazy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class MethodMapping : IReadOnlyList<ParameterMapping>
    {
        private readonly IReadOnlyList<ParameterMapping> _parameterMappings;

        public MethodMapping(
            MethodModel methodModel,
            IReadOnlyList<ParameterMapping> propertyMappings)
        {
            _parameterMappings = propertyMappings;
            MethodModel = methodModel;

            MatchDisambiguation = propertyMappings
                .Any(prop => prop.MatchDisambiguation == MatchDisambiguation.ImplicitMatch)
                ? MatchDisambiguation.ImplicitMatch
                : MatchDisambiguation.ExplicitMatch;
        }
        
        public MethodModel MethodModel { get; }
        public MatchDisambiguation MatchDisambiguation { get; }

        public int Count => _parameterMappings.Count;
        public ParameterMapping this[int index] => _parameterMappings[index];
        public IEnumerator<ParameterMapping> GetEnumerator() => _parameterMappings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [Lazy]
        public override string ToString()
        {
            return $"{MethodModel} {String.Join(" ", _parameterMappings.Select(p => p))}";
        }
    }
}
