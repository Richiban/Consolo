using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoLazy;

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
                parameterMappings.OfType<ParameterMapping.NoValue>().Any(),
                parameterMappings.OfType<ParameterMapping.PositionalValue>().Any(),
                explicitRouteMatch);
        }

        public MethodModel MethodModel { get; }
        public MethodMatchPriority MatchPriority { get; }

        public int Count => _parameterMappings.Count;
        public ParameterMapping this[int index] => _parameterMappings[index];

        public IEnumerator<ParameterMapping> GetEnumerator() =>
            _parameterMappings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [Lazy]
        public override string ToString() =>
            $"{MethodModel}({string.Join(", ", _parameterMappings)})";
    }
}