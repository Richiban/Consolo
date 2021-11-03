using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Richiban.Cmdr
{
    internal class MissingValueTypeConverter : ITypeConverter
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

            convertedValue = null;

            return false;
        }
    }
}