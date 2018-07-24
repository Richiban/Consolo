using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class Scanner
    {
        internal IEnumerable<TypeModel> ImplementingTypes { get; }

        public Scanner()
        {
            ImplementingTypes = BuildModel();
        }

        public void Scan(CommandLineArgumentCollection args)
        {
            var (typeToInstantiate, arguments) = GetTypeToInstantiate(args, ImplementingTypes);

            var instance = typeToInstantiate.CreateInstance(arguments);

            instance.Execute();
        }

        private static IEnumerable<TypeModel> BuildModel()
        {
            return Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => typeof(ICommandLineAction).IsAssignableFrom(t) && t.IsAbstract == false && t.IsClass)
                .Select(t => new TypeModel(t));
        }

        private (TypeModel, IReadOnlyList<(CommandLineArgument, PropertyModel)>) GetTypeToInstantiate(
            CommandLineArgumentCollection args,
            IEnumerable<TypeModel> implementingTypes)
        {
            var matchingTypes = implementingTypes
                .Select(t => (t, Match(args, t)))
                .Where(x => x.Item2.Item1)
                .Select(x => (x.Item1, x.Item2.Item2))
                .ToList();

            switch (matchingTypes.Count)
            {
                case 0:
                    throw new Exception("No type found");
                case 1:
                    return matchingTypes.Single();
                case var n:
                    var bestMatch =
                        matchingTypes.GroupBy(m => m.Item2.Count)
                        .OrderByDescending(m => m.Key)
                        .First()
                        .ToList();

                    if (bestMatch.Count() > 1)
                    {
                        throw new Exception("Multiple types found");
                    }
                    else
                    {
                        return bestMatch.Single();
                    }
            }
        }

        private (bool, IReadOnlyList<(CommandLineArgument, PropertyModel)>) Match(
            CommandLineArgumentCollection args, 
            TypeModel type)
        {
            if(type.MatchesVerb(args, ref args) == false)
            {
                return (false, null);
            }

            var namedArgPool = args.OfType<CommandLineArgument.Named>().ToList();
            var flagPool = args.OfType<CommandLineArgument.Flag>().ToList();
            var freePool = args.Where(a => a is CommandLineArgument.Free).ToList();
            
            var propPool = type.Properties.ToList();

            var namedPairings = new List<(CommandLineArgument, PropertyModel)>();

            foreach (var namedArg in namedArgPool.ToList())
            {
                var prop = propPool.SingleOrDefault(p => p.NameMatches(namedArg.Name));

                if (prop == null) return (false, null);

                namedPairings.Add((namedArg, prop));

                namedArgPool.Remove(namedArg);
                propPool.Remove(prop);
            }

            if (namedArgPool.Any())
            {
                return (false, null);
            }

            foreach (var flag in flagPool.ToList())
            {
                var prop = propPool
                    .Where(p => p.NameMatches(flag.Name))
                    .SingleOrDefault(p => p.PropertyType == typeof(bool));

                if (prop == null) return (false, null);

                namedPairings.Add((flag, prop));

                flagPool.Remove(flag);
                propPool.Remove(prop);
            }

            if (flagPool.Any())
            {
                return (false, null);
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
                return (false, null);
            }

            return (true, namedPairings);
        }
    }
}
