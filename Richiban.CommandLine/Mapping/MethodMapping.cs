using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class MethodMapping : IReadOnlyList<ParameterMapping>
    {
        private readonly IReadOnlyList<ParameterMapping> _propertyMappings;

        public MethodMapping(MethodModel methodModel, IReadOnlyList<ParameterMapping> propertyMappings)
        {
            MethodModel = methodModel ?? throw new ArgumentNullException(nameof(methodModel));
            _propertyMappings = propertyMappings ?? throw new ArgumentNullException(nameof(propertyMappings));
            IsStatic = methodModel.IsStatic;
            InvokeFunc = methodModel.InvokeFunc;

            MatchDisambiguation = propertyMappings
                .Any(prop => prop.MatchDisambiguation == MatchDisambiguation.ImplicitMatch)
                ? MatchDisambiguation.ImplicitMatch
                : MatchDisambiguation.ExplicitMatch;
        }

        public MethodModel MethodModel { get; }
        public MatchDisambiguation MatchDisambiguation { get; }
        public bool IsStatic { get; }
        public Func<object, object[], object> InvokeFunc { get; }

        public int Count => _propertyMappings.Count;
        public ParameterMapping this[int index] => _propertyMappings[index];
        public IEnumerator<ParameterMapping> GetEnumerator() => _propertyMappings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
