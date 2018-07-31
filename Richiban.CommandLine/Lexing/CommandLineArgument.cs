using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Richiban.CommandLine
{
    [DebuggerDisplay("{ToString()}")]
    public abstract class CommandLineArgument
    {
        private readonly string _raw;

        private CommandLineArgument(string raw)
        {
            _raw = raw;
        }

        [DebuggerDisplay("{Value}")]
        public class Free : CommandLineArgument
        {
            public string Value { get; }

            public Free(string value, string raw) : base(raw) => 
                Value = value;
        }

        [DebuggerDisplay("/{Name}:{Value}")]
        public class NameValuePair : CommandLineArgument
        {
            public string Name { get; }
            public string Value { get; }

            public NameValuePair(string name, string value, string raw) : base(raw) =>
                (Name, Value) = (name, value);
        }

        [DebuggerDisplay("/{Name}")]
        public class BareNameOrFlag : CommandLineArgument
        {
            public string Name { get; }

            public BareNameOrFlag(string name, string raw) : base(raw) =>
                Name = name;
        }

        [DebuggerDisplay("/?")]
        public class HelpGlyph : CommandLineArgument
        {
            public HelpGlyph(string raw) : base(raw) {}
        }

        public static CommandLineArgument Parse(string raw)
        {
            if (raw == "/?" || raw == "-?" || raw == "--?")
                return new HelpGlyph(raw);

            if (raw.StartsWith("/"))
            {
                var parts = raw.TrimStart('/').Split(':');

                if (parts.Length > 1)
                {
                    return new NameValuePair(parts[0], parts[1], raw);
                }
                else
                {
                    return new BareNameOrFlag(parts[0], raw);
                }
            }
            else if (raw.StartsWith("--"))
            {
                var parts = raw.TrimStart('-').Split('=');

                if (parts.Length > 1)
                {
                    return new NameValuePair(parts[0], parts[1], raw);
                }
                else
                {
                    return new BareNameOrFlag(parts[0], raw);
                }
            }
            else if (raw.StartsWith("-"))
            {
                return new BareNameOrFlag(raw.TrimStart('-'), raw);
            }
            else
            {
                return new Free(raw, raw);
            }
        }

        public override string ToString() => _raw;
    }
}
