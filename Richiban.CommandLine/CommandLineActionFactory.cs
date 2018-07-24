using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class CommandLineActionFactory
    {
        private readonly AssemblyModel _assemblyModel;

        public CommandLineActionFactory(AssemblyModel model)
        {
            _assemblyModel = model;
        }

        public Option<ICommandLineAction> Create(CommandLineArgumentList commandLineArgs) =>
            GetTypeMapping(commandLineArgs).IfSome(x => x.CreateInstance());

        private Option<TypeMapping> GetTypeMapping(CommandLineArgumentList args)
        {
            var matchingTypes = _assemblyModel
                .Select(t => MapType(args, t))
                .Choose()
                .ToList();

            switch (matchingTypes.Count)
            {
                case 0:
                    return default;
                case 1:
                    return matchingTypes.Single();
                case var n:
                    var bestMatch =
                        matchingTypes.GroupBy(m => m.PropertyMappings.Count)
                        .OrderByDescending(m => m.Key)
                        .First()
                        .ToList();

                    if (bestMatch.Count() > 1)
                    {
                        return default;
                    }
                    else
                    {
                        return bestMatch.Single();
                    }
            }
        }

        private Option<TypeMapping> MapType(
            CommandLineArgumentList args,
            TypeModel typeModel)
        {
            if (typeModel.MatchesVerb(args, ref args) == false)
            {
                return default;
            }

            var namedArgPool = args.OfType<CommandLineArgument.Named>().ToList();
            var flagPool = args.OfType<CommandLineArgument.Flag>().ToList();
            var freePool = args.Where(a => a is CommandLineArgument.Free).ToList();

            var propPool = typeModel.Properties.ToList();

            var namedPairings = new List<(CommandLineArgument, PropertyModel)>();

            foreach (var namedArg in namedArgPool.ToList())
            {
                var prop = propPool.SingleOrDefault(p => p.NameMatches(namedArg.Name));

                if (prop == null) return default;

                namedPairings.Add((namedArg, prop));

                namedArgPool.Remove(namedArg);
                propPool.Remove(prop);
            }

            if (namedArgPool.Any())
            {
                return default;
            }

            foreach (var flag in flagPool.ToList())
            {
                var prop = propPool
                    .Where(p => p.NameMatches(flag.Name))
                    .SingleOrDefault(p => p.PropertyType == typeof(bool));

                if (prop == null) return default;

                namedPairings.Add((flag, prop));

                flagPool.Remove(flag);
                propPool.Remove(prop);
            }

            if (flagPool.Any())
            {
                return default;
            }

            foreach (var free in freePool.ToList())
            {
                var prop = propPool.FirstOrDefault();

                if (prop == null) break;

                namedPairings.Add((free, prop));

                freePool.Remove(free);
                propPool.Remove(prop);
            }

            if (propPool.All(p => p.IsOptional) == false || freePool.Any())
            {
                return default;
            }

            return new TypeMapping(typeModel, new PropertyMappingCollection(namedPairings));
        }
    }
}
