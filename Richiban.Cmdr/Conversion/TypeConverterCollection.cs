using System;
using System.Collections.Generic;

namespace Richiban.Cmdr
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

        
        private bool TryConvertValue(
            ITypeConverter converter,
            Type convertToType,
            IReadOnlyList<string> rawValues,
            out object result)
        {
            if (convertToType.IsArray)
            {
                var elementType = convertToType.GetElementType();
                var resultantArray = Array.CreateInstance(elementType, rawValues.Count);

                for(var i = 0; i < rawValues.Count; i++)
                {
                    object convertedValue;

                    if (!converter.TryConvertValue(elementType, new[] { rawValues[i] }, out convertedValue))
                    {
                        result = null;
                        return false;
                    }

                    resultantArray.SetValue(convertedValue, i);
                }

                result = resultantArray;
                return true;
            }
            else
            {
                return converter.TryConvertValue(convertToType, rawValues, out result);
            }
        }

        
        private static bool ResultIsOfCorrectType(Type convertToType, object result)
        {
            if (result == null || result == Type.Missing)
                return true;

            return convertToType.IsAssignableFrom(result.GetType());
        }
    }
}
