using System.Collections.Generic;
using System.Linq;
using static Richiban.Cmdr.Prelude;

namespace Richiban.Cmdr
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
            var parameterMappings = new List<ParameterMapping>();

            var (routeMatches, remainingArgs) = methodModel.Routes.Matches(args);

            if (routeMatches == false)
            {
                return None;
            }

            var explicitRouteMatch = args.Count != remainingArgs.Count;

            remainingArgs = methodModel.Parameters.ExpandShortForms(remainingArgs);

            foreach (var prop in methodModel.Parameters)
            {
                var maybePropertyMapping = _parameterMapper.Map(prop, remainingArgs);

                maybePropertyMapping.IfSome(s =>
                {
                    var (mapping, _) = s;
                    remainingArgs = s.remainingArguments;
                    parameterMappings.Add(mapping);
                });

                if (maybePropertyMapping.HasValue == false)
                {
                    return None;
                }
            }

            if (remainingArgs.Any())
                return None;

            return new MethodMapping(methodModel, parameterMappings, explicitRouteMatch);
        }
    }
}
