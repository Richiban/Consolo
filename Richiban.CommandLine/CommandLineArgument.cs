using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Richiban.CommandLine
{
    public abstract class CommandLineArgument
    {
        private CommandLineArgument() { }

        [DebuggerDisplay("{Value}")]
        public class Free : CommandLineArgument
        {
            public string Value { get; }

            public Free(string value) => Value = value;
        }

        [DebuggerDisplay("/{Name}:{Value}")]
        public class NameValuePair : CommandLineArgument
        {
            public string Name { get; }
            public string Value { get; }

            public NameValuePair(string name, string value) => (Name, Value) = (name, value);
        }

        [DebuggerDisplay("/{Name}")]
        public class BareNameOrFlag : CommandLineArgument
        {
            public string Name { get; }

            public BareNameOrFlag(string name) => Name = name;
        }

        public static CommandLineArgument Parse(string raw)
        {
            if (raw.StartsWith("/"))
            {
                var parts = raw.TrimStart('/').Split(':');

                if (parts.Length > 1)
                {
                    return new NameValuePair(parts[0], parts[1]);
                }
                else
                {
                    return new BareNameOrFlag(parts[0]);
                }
            }
            else if (raw.StartsWith("--"))
            {
                var parts = raw.TrimStart('-').Split('=');

                if (parts.Length > 1)
                {
                    return new NameValuePair(parts[0], parts[1]);
                }
                else
                {
                    return new BareNameOrFlag(parts[0]);
                }
            }
            else if (raw.StartsWith("-"))
            {
                return new BareNameOrFlag(raw.TrimStart('-'));
            }
            else
            {
                return new Free(raw);
            }
        }
    }
}
