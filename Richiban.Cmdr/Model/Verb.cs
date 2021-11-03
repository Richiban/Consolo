using System;

namespace Richiban.Cmdr
{
    internal class Verb
    {
        private readonly string _value;

        public Verb(string value)
        {
            _value = value;

            if (Matches("help"))
                throw new InvalidOperationException("\"help\" is a reserved word and cannot be used in a route");
        }

        public bool Matches(string rawValue) =>
            _value.Equals(rawValue, StringComparison.CurrentCultureIgnoreCase);

        public override string ToString() => _value.ToLowerInvariant();
    }
}