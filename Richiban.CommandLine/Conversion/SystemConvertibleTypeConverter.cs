using NullGuard;
using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class SystemConvertibleTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            [AllowNull] out object convertedValue)
        {
            if (typeof(IConvertible).IsAssignableFrom(convertToType) && rawValues.Count == 1)
            {
                convertedValue = Convert.ChangeType(rawValues[0], convertToType);
                return true;
            }
            else
            {
                convertedValue = null;
                return false;
            }
        }
    }
}
