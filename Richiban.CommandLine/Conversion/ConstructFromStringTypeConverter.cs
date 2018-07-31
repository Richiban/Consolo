using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Richiban.CommandLine.Prelude;

namespace Richiban.CommandLine
{
    class ConstructFromStringTypeConverter : ITypeConverter
    {
        public bool TryConvertValue(
            Type convertToType,
            IReadOnlyList<string> rawValues,
            [AllowNull] out object convertedValue)
        {
            if (TryGetCompatibleConstructor(convertToType, rawValues.Count).IsSome(out var constructor))
            {
                try
                {
                    convertedValue = constructor.Invoke(rawValues.ToArray());
                    return true;
                }
                catch (TargetInvocationException e)
                {
                    throw new Exception(
                        $"An exception was thrown when constructing type {convertToType} with value '{String.Join(", ", rawValues)}'",
                        e.InnerException);
                }
            }
            else
            {
                convertedValue = null;
                return false;
            }
        }

        private static Option<ConstructorInfo> TryGetCompatibleConstructor(
            Type convertToType, int stringParameterCount)
        {
            return convertToType
                .GetConstructors()
                .Where(constructor =>
                {
                    var constructorParameters = constructor.GetParameters();

                    return constructorParameters.Length == stringParameterCount &&
                        constructorParameters.All(x => x.ParameterType == typeof(string));
                })
                .Select(Some)
                .SingleOrDefault();
        }
    }
}
