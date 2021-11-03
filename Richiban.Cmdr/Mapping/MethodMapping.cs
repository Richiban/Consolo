using AutoLazy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr
{
    internal class MethodMapping : IReadOnlyList<ParameterMapping>
    {
        private readonly IReadOnlyList<ParameterMapping> _parameterMappings;

        public MethodMapping(
            MethodModel methodModel,
            IReadOnlyList<ParameterMapping> parameterMappings,
            bool explicitRouteMatch)
        {
            _parameterMappings = parameterMappings;
            MethodModel = methodModel;

            MatchPriority = new MethodMatchPriority(
                usesOptionalParameters: parameterMappings.OfType<ParameterMapping.NoValue>().Any(),
                usesPositionalParameters: parameterMappings.OfType<ParameterMapping.PositionalValue>().Any(),
                explicitRouteMatch: explicitRouteMatch);
        }
        
        public MethodModel MethodModel { get; }
        public MethodMatchPriority MatchPriority { get; }

        public int Count => _parameterMappings.Count;
        public ParameterMapping this[int index] => _parameterMappings[index];
        public IEnumerator<ParameterMapping> GetEnumerator() => _parameterMappings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [Lazy]
        public override string ToString()
        {
            return $"{MethodModel}({String.Join(", ", _parameterMappings)})";
        }
    }
}
