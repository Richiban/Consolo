using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class TypeConverterCollection
    {
        private readonly ImmutableStack<ITypeConverter> _typeConverters;

        [TracerAttributes.TraceOn]
        public TypeConverterCollection(Stack<ITypeConverter> typeConverters)
        {
            _typeConverters = ImmutableStack<ITypeConverter>.CopyFrom(typeConverters);
        }

        [TracerAttributes.TraceOn]
        public object ConvertValue(Type convertToType, IReadOnlyList<string> rawValues)
        {
            var converterStack = _typeConverters;

            while (converterStack.IsEmpty == false)
            {
                ITypeConverter converter;
                (converter, converterStack) = converterStack.Pop();

                if (TryConvertValue(converter, convertToType, rawValues, out var result))
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

        [TracerAttributes.TraceOn]
        private bool TryConvertValue(
            ITypeConverter converter,
            Type convertToType,
            IReadOnlyList<string> rawValues,
            out object result)
        {
            return converter.TryConvertValue(convertToType, rawValues, out result);
        }

        [TracerAttributes.TraceOn]
        private static bool ResultIsOfCorrectType(Type convertToType, object result)
        {
            if (result == null || result == Type.Missing)
                return true;

            return convertToType.IsAssignableFrom(result.GetType());
        }
    }
}
