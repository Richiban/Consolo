using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    public abstract class CommandLineArgument
    {
        private CommandLineArgument() { }

        public class Free : CommandLineArgument
        {
            public string Value { get; }

            public Free(string value) => Value = value;
        }

        public class Named : CommandLineArgument
        {
            public string Name { get; }
            public string Value { get; }

            public Named(string name, string value) => (Name, Value) = (name, value);
        }

        public class Flag : CommandLineArgument
        {
            public string Name { get; }

            public Flag(string name) => Name = name;
        }

        public static IEnumerable<CommandLineArgument> Parse(string raw)
        {
            if (raw.StartsWith("/"))
            {
                var parts = raw.TrimStart('/').Split(':');

                if (parts.Length > 1)
                {
                    return new[] { new Named(parts[0], parts[1]) };
                }
                else
                {
                    return new[] { new Flag(parts[0]) };
                }
            }
            else if (raw.StartsWith("-"))
            {
                var parts = raw.TrimStart('-').ToCharArray();

                return parts.Select(c => new Flag(c.ToString()));
            }
            else
            {
                return new[] { new Free(raw) };
            }
        }
    }
}
