using System;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class PropertyMapping
    {
        public PropertyMapping(ParameterModel parameterModel, object suppliedValue)
        {
            ParameterModel = parameterModel ?? throw new ArgumentNullException(nameof(parameterModel));
            Value = ConvertValue(parameterModel.PropertyType, suppliedValue);
        }

        public ParameterModel ParameterModel { get; }
        public object Value { get; }

        private static object ConvertValue(Type convertToType, object suppliedValue)
        {
            if (convertToType == typeof(string) || suppliedValue == Type.Missing)
            {
                return suppliedValue;
            }

            try
            {
                return Convert.ChangeType(suppliedValue, convertToType);
            }
            catch (Exception)
            { }

            var constructor = convertToType
                .GetConstructors()
                .Where(CanBeConstructedFromString)
                .SingleOrDefault();

            if(constructor != null)
            {
                try
                {
                    return constructor.Invoke(new[] { suppliedValue });
                }
                catch(TargetInvocationException e)
                {
                    throw new Exception(
                        $"An exception was thrown when constructing type {convertToType} with value '{suppliedValue}'",
                        e.InnerException);
                }
            }

            throw new InvalidOperationException(
                $"Could not find any way of converting value '{suppliedValue}' to type {convertToType}");
        }

        private static bool CanBeConstructedFromString(ConstructorInfo constructor)
        {
            var constructorParameters = constructor.GetParameters();

            return constructorParameters.Length == 1 &&
                constructorParameters[0].ParameterType == typeof(string);
        }
    }
}