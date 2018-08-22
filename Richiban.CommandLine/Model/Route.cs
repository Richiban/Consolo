using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class Route : IReadOnlyList<Verb>
    {
        private readonly IReadOnlyList<Verb> _verbs;
        private readonly string _methodName;
        
        public Route(string methodName, IReadOnlyList<string> routeParts)
        {
            if (routeParts.Count == 0)
                _verbs = new [] { new Verb(methodName) };
            else
                _verbs = routeParts.Where(v => v != "").Select(s => new Verb(s)).ToList();

            _methodName = methodName;
        }

        private Route(string methodName, IReadOnlyList<Verb> verbs)
        {
            _methodName = methodName;
            _verbs = verbs;
        }

        public Verb this[int index] => _verbs[index];

        public int Count => _verbs.Count;

        public IEnumerator<Verb> GetEnumerator() => _verbs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool MatchesVerbSequence(
            CommandLineArgumentList commandLineArguments,
            out IReadOnlyCollection<CommandLineArgument> argumentsMatched)
        {
            var argsMatched = new List<CommandLineArgument>();

            using (var verbEnumerator = GetEnumerator())
            using (var inputArgumentEnumerator = commandLineArguments.GetEnumerator())
            {
                while (verbEnumerator.MoveNext())
                {
                    if (inputArgumentEnumerator.MoveNext() &&
                        inputArgumentEnumerator.Current is CommandLineArgument.Free free &&
                        verbEnumerator.Current.Matches(free.Value))
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

        public Route Concat(Route otherRoute)
        {
            return new Route(_methodName, _verbs.Concat(otherRoute._verbs).ToList());
        }
    }
}