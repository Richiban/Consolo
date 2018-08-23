using System.Collections.Generic;

namespace Richiban.CommandLine
{
    class ParameterMapper
    {
        public Option<(ParameterMapping, IReadOnlyCollection<CommandLineArgument> argumentsMatched)> Map(
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
                        case CommandLineArgument.NameValuePair nvPair when parameterModel.MatchesName(nvPair.Name):
                            
                            if(parameterModel.AllowMultipleValues)
                            {
                                suppliedValues.Add(nvPair.Value);
                                argumentsMatched.Add(nvPair);
                                continue;
                            }

                            argumentsMatched.Add(nvPair);

                            return (new ParameterMapping(
                                parameterModel,
                                MatchDisambiguation.ExplicitMatch,
                                nvPair.Value), argumentsMatched);

                        case CommandLineArgument.BareNameOrFlag nameOrFlag
                            when parameterModel.MatchesName(nameOrFlag.Name) && parameterModel.IsFlag:
                            argumentsMatched.Add(nameOrFlag);

                            return (new ParameterMapping(
                                parameterModel,
                                MatchDisambiguation.ExplicitMatch,
                                $"{true}"), argumentsMatched);

                        case CommandLineArgument.BareNameOrFlag bnf when parameterModel.MatchesName(bnf.Name):
                            if (enumerator.MoveNext())
                            {
                                if (enumerator.Current is CommandLineArgument.Free free)
                                {
                                    argumentsMatched.Add(bnf);
                                    argumentsMatched.Add(free);

                                    return (new ParameterMapping(
                                        parameterModel,
                                        MatchDisambiguation.ExplicitMatch,
                                        free.Value), argumentsMatched);
                                }
                            }

                            break;

                        case CommandLineArgument.Free free when parameterModel.IsFlag:
                            continue;

                        case CommandLineArgument.Free free:

                            if (parameterModel.AllowMultipleValues && parameterModel.GreedilyGrabFreeValues)
                            {
                                suppliedValues.Add(free.Value);
                                argumentsMatched.Add(free);
                                continue;
                            }

                            argumentsMatched.Add(free);

                            return (new ParameterMapping(
                                parameterModel,
                                MatchDisambiguation.ImplicitMatch,
                                free.Value), argumentsMatched);

                        default:
                            break;
                    }
                }
            }

            if (parameterModel.AllowMultipleValues)
            {
                return (new ParameterMapping(
                    parameterModel,
                    MatchDisambiguation.ExplicitWithOptionals,
                    suppliedValues), argumentsMatched);
            }

            if (parameterModel.IsOptional)
            {
                return (new ParameterMapping(
                    parameterModel,
                    MatchDisambiguation.ExplicitWithOptionals), new List<CommandLineArgument>(0));
            }

            return default;
        }
    }
}
