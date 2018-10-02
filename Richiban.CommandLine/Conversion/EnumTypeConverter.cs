using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class EnumTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            out object convertedValue)
        {
            if (convertToType.IsEnum && rawValues.Count == 1)
            {
                try
                {
                    convertedValue = Enum.Parse(convertToType, rawValues[0], ignoreCase: true);
                    return true;
                }
                catch (Exception)
                { }
            }

            convertedValue = null;
            return false;
        }
    }
}
