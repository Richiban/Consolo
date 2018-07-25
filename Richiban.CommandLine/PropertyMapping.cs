using System;

namespace Richiban.CommandLine
{
    internal class PropertyMapping
    {
        public PropertyMapping(ParameterModel parameterModel, object value)
        {
            ParameterModel = parameterModel ?? throw new ArgumentNullException(nameof(parameterModel));
            Value = Convert.ChangeType(value, parameterModel.PropertyType);
        }

        public ParameterModel ParameterModel { get; }
        public object Value { get; }
    }
}