using System;
using System.Collections;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal class MethodMapping : IReadOnlyList<PropertyMapping>
    {
        private readonly IReadOnlyList<PropertyMapping> _propertyMappings;

        public MethodMapping(MethodModel methodModel, IReadOnlyList<PropertyMapping> propertyMappings)
        {
            MethodModel = methodModel ?? throw new ArgumentNullException(nameof(methodModel));
            _propertyMappings = propertyMappings ?? throw new ArgumentNullException(nameof(propertyMappings));
            IsStatic = methodModel.IsStatic;
        }

        public MethodModel MethodModel { get; }
        public int Count => _propertyMappings.Count;
        public PropertyMapping this[int index] => _propertyMappings[index];
        public IEnumerator<PropertyMapping> GetEnumerator() => _propertyMappings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal Action GetInvokeAction(object instance, object[] methodArguments)
        {
            return () => MethodModel.MethodInfo.Invoke(instance, methodArguments);
        }

        public bool IsStatic { get; }
    }
}
