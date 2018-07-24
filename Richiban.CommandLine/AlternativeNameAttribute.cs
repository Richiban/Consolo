using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    public class AlternativeNameAttribute : Attribute
    {
        public AlternativeNameAttribute(params string[] alternativeNames)
        {
            AlternativeNames = alternativeNames.Distinct().ToList();
        }

        public IReadOnlyList<string> AlternativeNames { get; }
    }
}