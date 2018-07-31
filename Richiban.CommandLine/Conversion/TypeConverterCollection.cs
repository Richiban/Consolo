using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class TypeConverterCollection
    {
        private readonly ImmutableStack<ITypeConverter> _typeConverters;

        public TypeConverterCollection(Stack<ITypeConverter> typeConverters)
        {
            _typeConverters = ImmutableStack<ITypeConverter>.CopyFrom(typeConverters);
        }

        public object ConvertValue(Type convertToType, IReadOnlyList<string> rawValues)
        {
            var converterStack = _typeConverters;

            while (converterStack.IsEmpty == false)
            {
                ITypeConverter converter;
                (converter, converterStack) = converterStack.Pop();

                if (converter.TryConvertValue(convertToType, rawValues, out var result))
                {
                    if (ResultIsOfCorrectType(convertToType, result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new TypeConversionException(
                            TypeConversionException.ATypeConverterReturnedAnIncorrectValue(
                                rawValues, convertToType, result, converter));
                    }
                }
            }

            throw new TypeConversionException(
                TypeConversionException.NoTypeConvertersCouldConvertValue(rawValues, convertToType));
        }

        private static bool ResultIsOfCorrectType(Type convertToType, object result)
        {
            if (result == null || result == Type.Missing)
                return true;

            return convertToType.IsAssignableFrom(result.GetType());
        }
    }
}
