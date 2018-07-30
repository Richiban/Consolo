using System;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class ParameterMapping
    {
        private readonly Option<string> _suppliedValue;
        private readonly Type convertToType;

        public ParameterMapping(
            ParameterModel parameterModel, 
            Option<string> suppliedValue,
            MatchDisambiguation matchDisambiguation)
        {
            ParameterModel = parameterModel ?? throw new ArgumentNullException(nameof(parameterModel));
            MatchDisambiguation = matchDisambiguation;
            _suppliedValue = suppliedValue;
        }

        public ParameterModel ParameterModel { get; }
        public MatchDisambiguation MatchDisambiguation { get; }

        public object ConvertValue()
        {
            if (convertToType == typeof(string) || suppliedValue == Type.Missing)
            {
                return suppliedValue;
            }

            if (convertToType.IsEnum && suppliedValue is string s)
            {
                return Enum.Parse(convertToType, s, ignoreCase: true);
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