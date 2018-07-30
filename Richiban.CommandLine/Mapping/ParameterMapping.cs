using System;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class ParameterMapping
    {
        private readonly Option<string> _suppliedValue;
        private readonly Type _convertToType;

        public ParameterMapping(
            Type parameterType,
            Option<string> suppliedValue,
            MatchDisambiguation matchDisambiguation)
        {
            MatchDisambiguation = matchDisambiguation;
            _suppliedValue = suppliedValue;
            _convertToType = parameterType;
        }
        
        public MatchDisambiguation MatchDisambiguation { get; }

        public object ConvertValue() =>
            _suppliedValue.Match(
                None: () => Type.Missing,
                Some: suppliedValue =>
                {
                    if (_convertToType == typeof(string))
                    {
                        return suppliedValue;
                    }

                    if (_convertToType.IsEnum && suppliedValue is string s)
                    {
                        return Enum.Parse(_convertToType, s, ignoreCase: true);
                    }

                    try
                    {
                        return Convert.ChangeType(suppliedValue, _convertToType);
                    }
                    catch (Exception)
                    { }

                    var constructor = _convertToType
                        .GetConstructors()
                        .Where(CanBeConstructedFromString)
                        .SingleOrDefault();

                    if (constructor != null)
                    {
                        try
                        {
                            return constructor.Invoke(new[] { suppliedValue });
                        }
                        catch (TargetInvocationException e)
                        {
                            throw new Exception(
                                $"An exception was thrown when constructing type {_convertToType} with value '{suppliedValue}'",
                                e.InnerException);
                        }
                    }

                    throw new InvalidOperationException(
                        $"Could not find any way of converting value '{suppliedValue}' to type {_convertToType}");

                });

        private static bool CanBeConstructedFromString(ConstructorInfo constructor)
        {
            var constructorParameters = constructor.GetParameters();

            return constructorParameters.Length == 1 &&
                constructorParameters[0].ParameterType == typeof(string);
        }
    }
}