using System.Collections.Generic;
using System.Linq;
using static Richiban.CommandLine.Prelude;

namespace Richiban.CommandLine
{
    class ParameterMapper
    {
        public Option<(ParameterMapping, CommandLineArgumentList remainingArguments)> Map(
            ParameterModel parameterModel,
            CommandLineArgumentList args)
        {
            var maybeMappingByName = NamedPass(parameterModel, args);

            if (maybeMappingByName.HasValue)
                return maybeMappingByName;

            var maybeMappingByPosition = PositionalPass(parameterModel, args);

            if (maybeMappingByPosition.HasValue)
                return maybeMappingByPosition;

            if (parameterModel.AllowNoValues)
            {
                return (new ParameterMapping.NoValue(parameterModel), args);
            }

            return None;
        }

        public Option<(ParameterMapping, CommandLineArgumentList remainingArguments)> NamedPass(
            ParameterModel parameterModel,
            CommandLineArgumentList args)
        {
            var argumentsMatched = new List<CommandLineArgument>();
            var suppliedValues = new List<string>();

            using (var enumerator = args.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    switch (enumerator.Current)
                    {
                        case CommandLineArgument.NameValuePair nvPair
                        when parameterModel.MatchesName(nvPair.Name):

                            argumentsMatched.Add(nvPair);

                            if (parameterModel.AllowMultipleValues)
                            {
                                suppliedValues.Add(nvPair.Value);
                                continue;
                            }

                            return (
                                new ParameterMapping.NamedValue(parameterModel, ListOf(nvPair.Value)),
                                args.Without(argumentsMatched));

                        case CommandLineArgument.BareNameOrFlag nameOrFlag
                        when parameterModel.MatchesName(nameOrFlag.Name) && parameterModel.IsFlag:
                            argumentsMatched.Add(nameOrFlag);

                            return (
                                new ParameterMapping.Flag(parameterModel),
                                args.Without(argumentsMatched));

                        case CommandLineArgument.BareNameOrFlag bnf
                        when parameterModel.MatchesName(bnf.Name):
                            if (enumerator.MoveNext())
                            {
                                if (enumerator.Current is CommandLineArgument.Free free)
                                {
                                    argumentsMatched.Add(bnf);
                                    argumentsMatched.Add(free);

                                    if (parameterModel.AllowMultipleValues)
                                    {
                                        suppliedValues.Add(free.Value);
                                        continue;
                                    }

                                    return (
                                        new ParameterMapping.NamedValue(
                                            parameterModel,
                                            ListOf(free.Value)),
                                        args.Without(argumentsMatched));
                                }
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            if (parameterModel.AllowMultipleValues && suppliedValues.Any())
            {
                return (
                    new ParameterMapping.NamedValue(parameterModel, suppliedValues),
                    args.Without(argumentsMatched));
            }

            return default;
        }

        public Option<(ParameterMapping, CommandLineArgumentList remainingArguments)> PositionalPass(
            ParameterModel parameterModel,
            CommandLineArgumentList args)
        {
            if (parameterModel.IsFlag)
                return None;

            var argumentsMatched = new List<CommandLineArgument>();
            var suppliedValues = new List<string>();

            foreach (var free in args.OfType<CommandLineArgument.Free>())
            {
                suppliedValues.Add(free.Value);
                argumentsMatched.Add(free);

                if (!parameterModel.GreedilyGrabFreeValues)
                {
                    break;
                }
            }

            if (suppliedValues.Any() == false && parameterModel.AllowNoValues == false)
            {
                return None;
            }

            return (
                new ParameterMapping.PositionalValue(parameterModel, suppliedValues), 
                args.Without(argumentsMatched));
        }
    }
}
