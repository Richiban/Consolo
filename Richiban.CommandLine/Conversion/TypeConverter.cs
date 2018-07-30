using System;
using System.Linq;
using System.Reflection;
using static Richiban.CommandLine.Prelude;

namespace Richiban.CommandLine
{
    static class TypeConverter
    {
        public static object ConvertValue(Type convertToType, Option<string> rawValue) =>
            rawValue.Match(
                None: () => Type.Missing,
                Some: suppliedValue =>
                {
                    if (convertToType == typeof(string))
                    {
                        return suppliedValue;
                    }

                    if (convertToType.IsEnum)
                    {
                        return Enum.Parse(convertToType, suppliedValue, ignoreCase: true);
                    }

                    try
                    {
                        return Convert.ChangeType(suppliedValue, convertToType);
                    }
                    catch (Exception)
                    { }

                    if(TryGetCompatibleConstructor(convertToType).IsSome(out var constructor))
                    {
                        try
                        {
                            return constructor.Invoke(new[] { suppliedValue });
                        }
                        catch (TargetInvocationException e)
                        {
                            throw new Exception(
                                $"An exception was thrown when constructing type {convertToType} with value '{suppliedValue}'",
                                e.InnerException);
                        }
                    }

                    throw new InvalidOperationException(
                        $"Could not find any way of converting value '{suppliedValue}' to type {convertToType}");
                });

        private static Option<ConstructorInfo> TryGetCompatibleConstructor(Type convertToType)
        {
            return convertToType
                .GetConstructors()
                .Where(constructor => {
                    var constructorParameters = constructor.GetParameters();

                    return constructorParameters.Length == 1 &&
                        constructorParameters[0].ParameterType == typeof(string);
                })  
                .Select(Some)
                .SingleOrDefault();
        }
    }
}
