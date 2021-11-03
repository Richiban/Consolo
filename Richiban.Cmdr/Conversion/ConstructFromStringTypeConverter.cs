using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using static Richiban.Cmdr.Prelude;

namespace Richiban.Cmdr
{
    internal class ConstructFromStringTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            [AllowNull] out object convertedValue)
        {
            if (TryGetCompatibleConstructor(convertToType, rawValues.Count)
                .IsSome(out var constructor))
            {
                try
                {
                    convertedValue = constructor.Invoke(rawValues.ToArray());

                    return true;
                }
                catch (TargetInvocationException e)
                {
                    throw new TypeConversionException(
                        TypeConversionException.TheConstructorForTypeXThrewAnException(
                            convertToType,
                            rawValues),
                        e.InnerException);
                }
            }

            convertedValue = null;

            return false;
        }

        private static Option<ConstructorInfo> TryGetCompatibleConstructor(
            Type convertToType,
            int stringParameterCount)
        {
            return convertToType.GetConstructors()
                .Where(
                    constructor =>
                    {
                        var constructorParameters = constructor.GetParameters();

                        return constructorParameters.Length == stringParameterCount &&
                               constructorParameters.All(
                                   x => x.ParameterType == typeof(string));
                    })
                .Select(Some)
                .SingleOrDefault();
        }
    }
}