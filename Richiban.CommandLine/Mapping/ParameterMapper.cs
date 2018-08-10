namespace Richiban.CommandLine
{
    class ParameterMapper
    {
        public Option<ParameterMapping> Map(
            ParameterModel parameterModel,
            CommandLineArgumentList args,
            out CommandLineArgument[] argumentsMatched)
        {
            var enumerator = args.GetEnumerator();

            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case CommandLineArgument.NameValuePair nvPair when parameterModel.NameMatches(nvPair.Name):
                        argumentsMatched = new[] { nvPair };
                        return new ParameterMapping(
                            parameterModel,
                            MatchDisambiguation.ExplicitMatch,
                            nvPair.Value);

                    case CommandLineArgument.BareNameOrFlag nameOrFlag
                        when parameterModel.NameMatches(nameOrFlag.Name) && parameterModel.IsFlag:
                        argumentsMatched = new[] { nameOrFlag };
                        return new ParameterMapping(
                            parameterModel,
                            MatchDisambiguation.ExplicitMatch,
                            $"{true}");

                    case CommandLineArgument.BareNameOrFlag bnf when parameterModel.NameMatches(bnf.Name):
                        if (enumerator.MoveNext())
                        {
                            if (enumerator.Current is CommandLineArgument.Free free)
                            {
                                argumentsMatched = new CommandLineArgument[] { bnf, free };

                                return new ParameterMapping(
                                    parameterModel,
                                    MatchDisambiguation.ExplicitMatch,
                                    free.Value);
                            }
                        }

                        break;

                    case CommandLineArgument.Free free when parameterModel.IsFlag:
                        continue;

                    case CommandLineArgument.Free free:
                        argumentsMatched = new[] { free };
                        return new ParameterMapping(
                            parameterModel,
                            MatchDisambiguation.ImplicitMatch,
                            free.Value);

                    default:
                        break;
                }
            }

            argumentsMatched = new CommandLineArgument[0];

            if (parameterModel.IsOptional)
            {
                return new ParameterMapping(
                    parameterModel,
                    MatchDisambiguation.ExplicitWithOptionals);
            }

            return default;
        }
    }
}
