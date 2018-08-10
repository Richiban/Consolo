namespace Richiban.CommandLine
{
    abstract class CommandLineArgument
    {
        public abstract override string ToString();

        public class Free : CommandLineArgument
        {
            public Free(string value) =>
                Value = value;

            public string Value { get; }

            public override string ToString() => Value;
        }

        public class NameValuePair : CommandLineArgument
        {
            public NameValuePair(string name, string value, string markerSequence) =>
                (Name, Value, MarkerSequence) = (name, value, markerSequence);

            public string Name { get; }
            public string Value { get; }
            public string MarkerSequence { get; }

            public override string ToString() => $"{MarkerSequence}{Name}={Value}";
        }

        public class BareNameOrFlag : CommandLineArgument
        {
            public BareNameOrFlag(string name, string markerSequence)
            {
                Name = name;
                MarkerSequence = markerSequence;
            }

            public string Name { get; }
            public string MarkerSequence { get; }

            public override string ToString() => $"{MarkerSequence}{Name}";
        }

        public class HelpGlyph : CommandLineArgument
        {
            public HelpGlyph(string markerSequence) { }

            public string MarkerSequence { get; }

            public override string ToString() => $"{MarkerSequence}?";
        }

        public class DiagnosticSwitch : CommandLineArgument
        {
            public DiagnosticSwitch(string markerSequence) { }

            public string MarkerSequence { get; }

            public override string ToString() => $"{MarkerSequence}?";
        }

        public static CommandLineArgument Parse(string raw)
        {
            switch (raw)
            {
                case "/?":
                case "-?":
                case "--?":
                    return new HelpGlyph(raw);
                case "/?trace":
                    return new DiagnosticSwitch(raw);
                case var _ when raw.StartsWith("/"):
                    {
                        var parts = raw.TrimStart('/').Split(':');

                        if (parts.Length > 1)
                        {
                            return new NameValuePair(parts[0], parts[1], "/");
                        }
                        else
                        {
                            return new BareNameOrFlag(parts[0], "/");
                        }
                    }
                case var _ when raw.StartsWith("--"):
                    {
                        var parts = raw.TrimStart('-').Split('=');

                        if (parts.Length > 1)
                        {
                            return new NameValuePair(parts[0], parts[1], "--");
                        }
                        else
                        {
                            return new BareNameOrFlag(parts[0], "--");
                        }
                    }
                case var _ when raw.StartsWith("-"):
                    {
                        return new BareNameOrFlag(raw.TrimStart('-'), "-");
                    }
                default:
                    return new Free(raw);
            }
        }
    }
}