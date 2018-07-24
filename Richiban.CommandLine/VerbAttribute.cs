using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    public class VerbAttribute : Attribute
    {
        public VerbAttribute(params string[] verbs)
        {
            Verbs = verbs.Distinct().ToList();
        }

        public IReadOnlyList<string> Verbs { get; }
    }
}