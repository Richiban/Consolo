using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    [Serializable]
    internal class TypeConversionException : Exception
    {
        public TypeConversionException(string message) : base(message)
        {
        }

        public static string NoTypeConvertersCouldConvertValue(
            IReadOnlyCollection<string> rawValues, Type convertToType) =>
            $"No {nameof(ITypeConverter)} could be found that could convert '{String.Join(", ", rawValues)}' to type '{convertToType.Name}'";

        internal static string ATypeConverterReturnedAnIncorrectValue(
            IReadOnlyList<string> rawValues, Type convertToType, object result, ITypeConverter converter) =>
            $"{nameof(ITypeConverter)} '{converter}' returned an incorrect result. Expected type '{convertToType}' but got a {result.GetType()}";
    }
}