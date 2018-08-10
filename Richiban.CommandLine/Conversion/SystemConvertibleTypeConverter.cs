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
                try
                {
                    convertedValue = Convert.ChangeType(rawValues[0], convertToType);
                    return true;
                }
                catch (Exception) { }
            }

            convertedValue = null;
            return false;
        }
    }
}
