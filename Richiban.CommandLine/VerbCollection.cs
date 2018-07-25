using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class VerbCollection : IReadOnlyList<string>
    {
        private string _methodName;
        private IReadOnlyList<string> _verbs;

        public VerbCollection(string methodName, VerbAttribute verbAttribute)
        {
            _methodName = methodName;

            if(verbAttribute == null)
            {
                _verbs = new string[0];
            }
            else
            {
                if(verbAttribute.Verbs.Count == 0)
                {
                    _verbs = new[] { methodName };
                }
                else
                {
                    _verbs = verbAttribute.Verbs;
                }
            }

            AllowNone = verbAttribute == null || _verbs.Contains("");

            Help = string.Join("|", _verbs.Except(new[] { "" }));

            if (AllowNone)
                Help = $"[{Help}]";
        }

        public string this[int index] => _verbs[index];

        public int Count => _verbs.Count;

        public bool AllowNone { get; }

        public IEnumerator<string> GetEnumerator() => _verbs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Matches(
            CommandLineArgumentList commandLineArguments, 
            out CommandLineArgument[] argumentsMatched)
        {
            var commandLineArgument = commandLineArguments.FirstOrDefault();

            argumentsMatched = new CommandLineArgument[0];

            if (commandLineArgument is CommandLineArgument.Free free)
            {
                if (_verbs.Any(v => v.Equals(free.Value, StringComparison.CurrentCultureIgnoreCase)))
                {
                    argumentsMatched = new[] { free };

                    return true;
                }
            }

            return AllowNone;
        }

        public string Help { get; }
    }
}