using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    /// <summary>
    /// This exception means that a string value provided at the command line could not be converted
    /// to the target type for the method parameter.
    /// </summary>
    [Serializable]
    public class TypeConversionException : Exception
    {
        internal TypeConversionException(string message) : base(message)
        {
        }

        internal TypeConversionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal static string NoTypeConvertersCouldConvertValue(
            IReadOnlyCollection<string> rawValues, Type convertToType) =>
            $"No {nameof(ITypeConverter)} could be found that could convert '{String.Join(", ", rawValues)}' to type '{convertToType.Name}'";

        internal static string ATypeConverterReturnedAnIncorrectValue(
            IReadOnlyList<string> rawValues, Type convertToType, object result, ITypeConverter converter) =>
            $"{nameof(ITypeConverter)} '{converter}' returned an incorrect result. Expected type '{convertToType}' but got a {result.GetType()}";

        internal static string TheConstructorForTypeXThrewAnException(Type convertToType, IReadOnlyCollection<string> rawValues) =>
            $"The constructor for type {convertToType} threw an exception when given '{String.Join(", ", rawValues)}'";
    }
}