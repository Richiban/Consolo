using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Richiban.Cmdr
{
    class MissingValueTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            [AllowNull] out object convertedValue)
        {
            if (rawValues.Count == 0)
            {
                convertedValue = Type.Missing;
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
