using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class VerbModel
    {
        private readonly IReadOnlyList<IReadOnlyList<string>> _verbSequences;

        public VerbModel(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var methodRoutes =
                methodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<RouteAttribute>()
                .Select(m => {
                    if (m.Verbs.Count == 0)
                        return new[] { methodInfo.Name };
                    return m.Verbs;
                })
                .ToArray();

            var classRoutes =
                methodInfo
                .DeclaringType
                .GetCustomAttributes(inherit: true)
                .OfType<RouteAttribute>()
                .Select(c => {
                    if (c.Verbs.Count == 0)
                        return new[] { methodInfo.DeclaringType.Name };
                    return c.Verbs;
                })
                .ToArray();

            if (classRoutes.Length == 0)
            {
                classRoutes = new[] { new string[] { } };
            }

            if (methodRoutes.Length == 0)
            {
                methodRoutes = new[] { new string[] { } };
            }

            _verbSequences = (
                from c in classRoutes
                from m in methodRoutes
                select c.Concat(m).ToList())
                .ToList();

            Help = BuildHelp(_verbSequences);
        }

        private string BuildHelp(IReadOnlyList<IReadOnlyList<string>> verbSequences)
        {
            var helpItems = verbSequences.Select(buildHelpForSequence);

            return String.Join("|", helpItems);

            string buildHelpForSequence(IReadOnlyList<string> verbSequence)
            {
                return String.Join(" ", verbSequence.Select(s => s.ToLowerInvariant()));
            }
        }

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs)
        {
            var bestAccuracy = 0;

            foreach (var verbSequence in _verbSequences)
            {
                using (var verbEnumerator = verbSequence.GetEnumerator())
                using (var inputArgumentEnumerator = commandLineArgs.GetEnumerator())
                {
                    var currentAccuracy = 0;

                    while (verbEnumerator.MoveNext())
                    {
                        if (inputArgumentEnumerator.MoveNext() &&
                            inputArgumentEnumerator.Current is CommandLineArgument.Free free &&
                            verbEnumerator.Current.Equals(free.Value, StringComparison.CurrentCultureIgnoreCase))
                        {
                            currentAccuracy++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    bestAccuracy = Math.Max(bestAccuracy, currentAccuracy);
                }
            }
            
            return bestAccuracy;
        }

        public bool Matches(
            CommandLineArgumentList commandLineArguments,
            out IReadOnlyCollection<CommandLineArgument> argumentsMatched)
        {
            foreach (var verbSequence in _verbSequences)
            {
                if (MatchesVerbSequence(verbSequence, commandLineArguments, out argumentsMatched))
                {
                    return true;
                }
            }

            argumentsMatched = new CommandLineArgument[0];
            return false;
        }


        public string Help { get; }

        private static bool MatchesVerbSequence(
            IReadOnlyList<string> verbSequence,
            CommandLineArgumentList commandLineArguments,
            out IReadOnlyCollection<CommandLineArgument> argumentsMatched)
        {
            var argsMatched = new List<CommandLineArgument>();

            using (var verbEnumerator = verbSequence.GetEnumerator())
            using (var inputArgumentEnumerator = commandLineArguments.GetEnumerator())
            {
                while (verbEnumerator.MoveNext())
                {
                    if (inputArgumentEnumerator.MoveNext() &&
                        inputArgumentEnumerator.Current is CommandLineArgument.Free free &&
                        verbEnumerator.Current.Equals(free.Value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        argsMatched.Add(free);
                    }
                    else
                    {
                        argumentsMatched = new CommandLineArgument[0];
                        return false;
                    }
                }
            }

            argumentsMatched = argsMatched;
            return true;
        }
    }
}