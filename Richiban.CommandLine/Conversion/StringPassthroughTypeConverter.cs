using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class StringPassthroughTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            out object convertedValue)
        {
            if (convertToType == typeof(string) && rawValues.Count == 1)
            {
                convertedValue = rawValues[0];
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
