using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Richiban.Cmdr
{
    internal class StringPassthroughTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            [AllowNull] out object convertedValue)
        {
            if (convertToType == typeof(string) && rawValues.Count == 1)
            {
                convertedValue = rawValues[index: 0];

                return true;
            }

            convertedValue = null;

            return false;
        }
    }
}