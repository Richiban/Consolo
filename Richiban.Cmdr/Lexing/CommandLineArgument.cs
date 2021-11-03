using System;

namespace Richiban.Cmdr
{
    internal abstract class CommandLineArgument
    {
        private readonly string _raw;

        private CommandLineArgument(string raw)
        {
            _raw = raw;
        }

        public override string ToString() => _raw;

        public static CommandLineArgument Parse(string raw, int index)
        {
            switch (raw)
            {
                case "/?":
                case "-?":
                case "--?":
                case "help" when index == 0:
                    return new HelpSwitch(raw);
                case "/?trace":
                    return new DiagnosticSwitch(raw);
                case var _ when raw.StartsWith("/"):
                {
                    var parts = raw.TrimStart(trimChar: '/').Split(separator: ':');

                    if (parts.Length > 1)
                    {
                        return new NameValuePair(parts[0], parts[1], raw);
                    }

                    return new BareNameOrFlag(parts[0], raw);
                }
                case var _ when raw.StartsWith("--"):
                {
                    var parts = raw.TrimStart(trimChar: '-').Split(separator: '=');

                    if (parts.Length > 1)
                    {
                        return new NameValuePair(parts[0], parts[1], raw);
                    }

                    return new BareNameOrFlag(parts[0], raw);
                }
                case var _ when raw.StartsWith("-"):
                {
                    return new BareNameOrFlag(raw.TrimStart(trimChar: '-'), raw);
                }
                default:
                    return new Free(raw);
            }
        }

        public class Free : CommandLineArgument
        {
            public Free(string value) : base(value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public class NameValuePair : CommandLineArgument
        {
            public NameValuePair(string name, string value, string raw) : base(raw)
            {
                (Name, Value) = (name, value);
            }

            public string Name { get; }
            public string Value { get; }
        }

        public class BareNameOrFlag : CommandLineArgument
        {
            public BareNameOrFlag(string name, string raw) : base(raw)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public class HelpSwitch : CommandLineArgument
        {
            public HelpSwitch(string raw) : base(raw)
            {
            }
        }

        public class DiagnosticSwitch : CommandLineArgument
        {
            public DiagnosticSwitch(string raw) : base(raw)
            {
            }
        }
    }
}