using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class VerbModel
    {
        private string _methodName;
        private readonly IReadOnlyList<IReadOnlyList<string>> _verbSequences;

        public VerbModel(
            string methodName,
            IReadOnlyCollection<VerbAttribute> verbAttributes,
            IReadOnlyCollection<VerbSequenceAttribute> verbSequenceAttributes)
        {
            if (verbAttributes == null)
            {
                throw new ArgumentNullException(nameof(verbAttributes));
            }

            if (verbSequenceAttributes == null)
            {
                throw new ArgumentNullException(nameof(verbSequenceAttributes));
            }

            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));

            _verbSequences = verbAttributes
                    .Select(a => (IReadOnlyList<string>)new[] 
                    {
                        a.Verb ?? methodName.ToLower(CultureInfo.CurrentCulture)
                    })
                    .Concat(verbSequenceAttributes.Select(s => s.Verbs).ToList()).ToList();

            Help = BuildHelp(_verbSequences);
        }

        private string BuildHelp(IReadOnlyList<IReadOnlyList<string>> verbSequences)
        {
            var helpItems = verbSequences.Select(buildHelpForSequence);

            return String.Join("|", helpItems);

            string buildHelpForSequence(IReadOnlyList<string> verbSequence)
            {
                return String.Join(" ", verbSequence);
            }
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