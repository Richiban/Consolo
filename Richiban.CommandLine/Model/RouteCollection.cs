using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Richiban.CommandLine
{
    internal class RouteCollection : IReadOnlyCollection<Route>
    {
        private readonly IReadOnlyList<Route> _routes;

        public RouteCollection(MethodInfo methodInfo)
        {
            var methodRoutes =
                methodInfo
                .GetCustomAttributes(inherit: true)
                .OfType<RouteAttribute>()
                .Select(m => new Route(methodInfo.Name, m.RouteParts))
                .ToArray();

            var classRoutes =
                methodInfo
                .DeclaringType
                .GetCustomAttributes(inherit: true)
                .OfType<RouteAttribute>()
                .Select(m => new Route(methodInfo.Name, m.RouteParts))
                .ToArray();

            if (classRoutes.Length == 0)
            {
                _routes = methodRoutes;
            }
            else
            if (methodRoutes.Length == 0)
            {
                _routes = classRoutes;
            }
            else
            _routes =
                (from c in classRoutes
                 from m in methodRoutes
                 select c.Concat(m))
                    .ToList();
        }

        public int Count => _routes.Count;
        public IEnumerator<Route> GetEnumerator() => _routes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs)
        {
            var bestAccuracy = 0;

            foreach (var route in _routes)
            {
                using (var verbEnumerator = route.GetEnumerator())
                using (var inputArgumentEnumerator = commandLineArgs.GetEnumerator())
                {
                    var currentAccuracy = 0;

                    while (verbEnumerator.MoveNext())
                    {
                        if (inputArgumentEnumerator.MoveNext() &&
                            inputArgumentEnumerator.Current is CommandLineArgument.Free free &&
                            verbEnumerator.Current.Matches(free.Value))
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
            foreach (var route in _routes)
            {
                if (route.MatchesVerbSequence(commandLineArguments, out argumentsMatched))
                {
                    return true;
                }
            }

            argumentsMatched = new CommandLineArgument[0];
            return false;
        }
    }
}