using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr
{
    internal class Route : IReadOnlyList<Verb>
    {
        private readonly string _methodOrClassName;
        private readonly IReadOnlyList<Verb> _verbs;

        public Route(string methodOrClassName, IReadOnlyList<string> routeParts)
        {
            if (routeParts.Count == 0)
            {
                _verbs = new[] { new Verb(methodOrClassName) };
            }
            else
            {
                _verbs = routeParts.Where(v => v != "").Select(s => new Verb(s)).ToList();
            }

            _methodOrClassName = methodOrClassName;
        }

        private Route(string methodName, IReadOnlyList<Verb> verbs)
        {
            _methodOrClassName = methodName;
            _verbs = verbs;
        }

        public Verb this[int index] => _verbs[index];

        public int Count => _verbs.Count;

        public IEnumerator<Verb> GetEnumerator() => _verbs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public (bool isMatch, CommandLineArgumentList remainingArguments)
            MatchesVerbSequence(CommandLineArgumentList commandLineArguments)
        {
            var argsMatched = new List<CommandLineArgument>();

            using (var verbEnumerator = GetEnumerator())
            using (var inputArgumentEnumerator = commandLineArguments.GetEnumerator())
            {
                while (verbEnumerator.MoveNext())
                {
                    if (inputArgumentEnumerator.MoveNext() &&
                        inputArgumentEnumerator.Current is CommandLineArgument.Positional
                            free && verbEnumerator.Current.Matches(free.Value))
                    {
                        argsMatched.Add(free);
                    }
                    else
                    {
                        return (false, commandLineArguments);
                    }
                }
            }

            return (true, commandLineArguments.Without(argsMatched));
        }

        public Route Concat(Route otherRoute) =>
            new(_methodOrClassName, _verbs.Concat(otherRoute._verbs).ToList());
    }
}