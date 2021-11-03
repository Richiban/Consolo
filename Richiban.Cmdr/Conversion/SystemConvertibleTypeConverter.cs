using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Richiban.Cmdr
{
    internal class SystemConvertibleTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            [AllowNull] out object convertedValue)
        {
            if (typeof(IConvertible).IsAssignableFrom(convertToType) &&
                rawValues.Count == 1)
            {
                try
                {
                    convertedValue = Convert.ChangeType(
                        rawValues[index: 0],
                        convertToType);

                    return true;
                }
                catch (Exception)
                {
                }
            }

            convertedValue = null;

            return false;
        }
    }
}