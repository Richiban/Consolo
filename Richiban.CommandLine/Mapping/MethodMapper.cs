using System.Collections.Generic;
using System.Linq;
using static Richiban.CommandLine.Prelude;

namespace Richiban.CommandLine
{
    class MethodMapper
    {
        private readonly ParameterMapper _parameterMapper;

        public MethodMapper(ParameterMapper parameterMapper) => _parameterMapper = parameterMapper;

        [TracerAttributes.TraceOn]
        public Option<MethodMapping> GetMethodMapping(
            MethodModel methodModel,
            CommandLineArgumentList args)
        {
            var remainingArgs = args;
            var parameterMappings = new List<ParameterMapping>();

            {
                if (methodModel.Verbs.Matches(remainingArgs, out var argumentsMatched))
                {
                    remainingArgs = remainingArgs.Without(argumentsMatched);
                }
                else
                {
                    return None;
                }
            }

            remainingArgs = methodModel.Parameters.ExpandShortForms(remainingArgs);

            foreach (var prop in methodModel.Parameters)
            {
                var maybePropertyMapping = _parameterMapper.Map(prop, remainingArgs, out var argumentsMatched);

                maybePropertyMapping.IfSome(s =>
                {
                    parameterMappings.Add(s);
                    remainingArgs = remainingArgs.Without(argumentsMatched);
                });

                if (maybePropertyMapping.HasValue == false)
                {
                    return None;
                }
            }

            if (remainingArgs.Any())
                return None;

            return new MethodMapping(methodModel, parameterMappings);
        }
    }
}
