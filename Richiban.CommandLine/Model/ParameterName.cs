using System;

namespace Richiban.CommandLine
{
    abstract class ParameterName
    {
        private ParameterName() {}

        public class LongForm : ParameterName
        {
            public LongForm(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public override bool Matches(char c) => false;

            public override bool Matches(string s) => Value.Equals(s, StringComparison.InvariantCultureIgnoreCase);
            public override string ToString() => Value;
        }

        public class ShortForm : ParameterName
        {
            public ShortForm(char value)
            {
                Value = value;
            }

            public char Value { get; }

            public override bool Matches(char c) => Value.Equals(c);

            public override bool Matches(string s) => false;
            public override string ToString() => Value.ToString();
        }

        public abstract bool Matches(char c);
        public abstract bool Matches(string s);
        public abstract override string ToString();
    }
}
