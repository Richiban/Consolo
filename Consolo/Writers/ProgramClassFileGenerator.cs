using Microsoft.CodeAnalysis;
using static Consolo.CommandTree;
using static Consolo.Prelude;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Consolo;

internal class ProgramClassFileGenerator(
    string applicationName,
    string generatedNamespace,
    Root rootCommand) : CodeFileGenerator
{
    private readonly CodeBuilder _codeBuilder = new();
    public override string FileName => "Program.g.cs";

    private void WriteCommandDebug(CommandTree command)
    {
        var commandName = command is SubCommand sub ? sub.CommandName : "{root}";
        _codeBuilder.AppendLines($"// {commandName} command{(command.Method.IsSome(out _) ? "*" : "")}");

        foreach (var c in command.SubCommands)
        {
            using (_codeBuilder.Indent())
                WriteCommandDebug(c);
        }
    }

    public override string GetCode()
    {
        {
            _codeBuilder.AppendLines(
                "using System;",
                "using System.Collections.Generic;",
                "using System.Collections.Immutable;");
            
            _codeBuilder.AppendLine();

            _codeBuilder.AppendLine("namespace " + generatedNamespace);
            using var _ = _codeBuilder.IndentBraces();

            _codeBuilder.AppendLine("/// <summary>");
            _codeBuilder.AppendLine("/// Entrypoint class for " + applicationName);
            _codeBuilder.AppendLine("/// </summary>");
            _codeBuilder.AppendLine("public static class Program");
            using var _2 = _codeBuilder.IndentBraces();

            _codeBuilder.AppendLine("/// <summary>");
            _codeBuilder.AppendLine("/// Entrypoint for " + applicationName);
            _codeBuilder.AppendLine("/// </summary>");
            _codeBuilder.AppendLine("public static void Main(string[] args)");
            {
                using var _3 = _codeBuilder.IndentBraces();

                _codeBuilder.AppendLine("var consoleColor = Console.ForegroundColor;");
                _codeBuilder.AppendLine("var helpTextColor = ConsoleColor.Green;");
                _codeBuilder.AppendLine("var commandNameColor = ConsoleColor.Yellow;");

                _codeBuilder.AppendLine();
                _codeBuilder.AppendLine("// Commands marked * have an associated method");
                WriteCommandDebug(rootCommand);
                _codeBuilder.AppendLine();

                _codeBuilder.AppendLines("var normalizeArgs = false;", "for (var i = 0; i < args.Length; i++)");
                using (_codeBuilder.IndentBraces())
                {
                    _codeBuilder.AppendLine("if (args[i].StartsWith(' ') && !args[i].StartsWith(\"--\"))");
                    using (_codeBuilder.IndentBraces())
                    {
                        _codeBuilder.AppendLines("normalizeArgs = true;", "break;");
                    }
                }
                _codeBuilder.AppendLine("if (normalizeArgs)");
                using (_codeBuilder.IndentBraces())
                {
                    _codeBuilder.AppendLines("var normalizedArgs = new List<string>();", "for (var i = 0; i < args.Length; i++)");
                    using (_codeBuilder.IndentBraces())
                    {
                        _codeBuilder.AppendLine("var arg = args[i];");
                        _codeBuilder.AppendLine("if (arg.StartsWith(' ') && !arg.StartsWith(\"--\"))");
                        using (_codeBuilder.IndentBraces())
                        {
                            _codeBuilder.AppendLine("for (var j = 1; j < arg.Length; j++)");
                            using (_codeBuilder.IndentBraces())
                            {
                                _codeBuilder.AppendLine("normalizedArgs.Add($\"-{arg[j]}\");");
                            }
                        }
                        _codeBuilder.AppendLine("else");
                        using (_codeBuilder.IndentBraces())
                        {
                            _codeBuilder.AppendLine("normalizedArgs.Add(arg);");
                        }
                    }
                    _codeBuilder.AppendLine("args = normalizedArgs.ToArray();");
                }
                _codeBuilder.AppendLine();

                _codeBuilder.AppendLines("var isHelp = false;", "for (var i = args.Length - 1; i >= 0; i--)");
                using (_codeBuilder.IndentBraces())
                {
                    _codeBuilder.AppendLine("if (args[i] == \"--help\" || args[i] == \"-h\" || args[i] == \"-?\")");
                    using (_codeBuilder.IndentBraces())
                    {
                        _codeBuilder.AppendLines("isHelp = true;", "break;");
                    }
                }
                _codeBuilder.AppendLine();

                WriteCommandGroup(rootCommand, []);
            }

            _codeBuilder.AppendLine();
            WriteHelperMethods();
            WriteVersionMethod();
        }

        return _codeBuilder.ToString();
    }

    private void WriteCommandGroup(CommandTree command, ImmutableArray<string> path)
    {
        if (command is SubCommand s)
        {
            _codeBuilder.AppendLines(
                $"if (args.Length >= {path.Length} && args[{path.Length - 1}] == \"{s.CommandName}\")");
        }

        using (_codeBuilder.IndentBraces())
        {
            foreach (var c in command.SubCommands)
            {
                WriteCommandGroup(c, path.Add(c.CommandName));
            }

            if (command.Method.IsSome(out var method))
            {
                WriteCommandHandlerBody(path, method);
            }
            else
            {
                _codeBuilder.AppendLine($"if (!isHelp)");
                using (_codeBuilder.IndentBraces())
                {
                    _codeBuilder.AppendLine($"if (args.Length == {path.Length})");
                    using (_codeBuilder.IndentBraces())
                    {
                        if (path.Length > 0)
                        {
                            WriteError($"Missing command after: \" + string.Join(\" \", args, {path.Length - 1}, args.Length - {path.Length - 1}) + \"");
                        }
                        else
                        {
                            WriteError("Missing command");
                        }
                    }
                    _codeBuilder.AppendLine("else");
                    using (_codeBuilder.IndentBraces())
                    {
                        WriteError($"Unknown command: \" + string.Join(\" \", args, {path.Length}, args.Length - {path.Length}) + \"");
                    }
                }
            }

            _codeBuilder.AppendLine("");
            WriteHelp(path, command);
            _codeBuilder.AppendLine("return;");
        }
    }

    private void WriteCommandHandlerBody(ImmutableArray<string> path, CommandMethod method)
    {
        var positionalCount = path.Length + method.MandatoryParameterCount;

        _codeBuilder.AppendLines("if (!isHelp)");

        using (_codeBuilder.IndentBraces())
        {
            _codeBuilder.AppendLine(
                $"var unclaimedArgs = new UnclaimedArgs(args.Length, {path.Length});");

            _codeBuilder.AppendLine("var processingError = false;");

            _codeBuilder.AppendLine();

            WriteParameterAssignments(method);

            _codeBuilder.AppendLines($"if (unclaimedArgs.Any)");

            using (_codeBuilder.IndentBraces())
            {
                WriteError("Unrecognised args: \" + unclaimedArgs.Describe(args) + \"");
                _codeBuilder.AppendLine("processingError = true;");
            }

            _codeBuilder.AppendLines("", "if (!processingError)");

            using (_codeBuilder.IndentBraces())
            {
                var argString = String.Join(", ", method.Parameters.Select(x => x.SourceName));

                var line = $"{method.FullyQualifiedName}({argString})";

                if (method.IsTaskReturn)
                {
                    line += ".Wait()";
                }

                line += ';';
                
                _codeBuilder.AppendLine(line);
                _codeBuilder.AppendLine("return;");
            }
        }
    }

    private void WriteParameterAssignments(CommandMethod method)
    {
        foreach (var p in
            method.Parameters.OrderBy(p =>
                p switch
                {
                    CommandParameter.Option { IsFlag: true } => 0,
                    CommandParameter.Option => 1,
                    _ => 2
                }
                ))
        {
            _codeBuilder.AppendLine($"var {p.SourceName} = {p.DefaultValue};");

            switch (p)
            {
                case CommandParameter.Option { IsFlag: true } flag:
                    {
                        _codeBuilder.AppendLine(
                            $"MatchNextFlag(args, {GetMatcherNameArguments(flag)}, ref {flag.SourceName}, ref unclaimedArgs);");
                        break;
                    }
                case CommandParameter.Option option:
                    {
                        _codeBuilder.AppendLine(
                            $"if (MatchNextOption(args, {GetMatcherNameArguments(option)}, ref {option.SourceName}, s => {ConvertParameter(option.Type, "s")}, ref unclaimedArgs) == 2)");
                        using (_codeBuilder.IndentBraces())
                        {
                            WriteError($"Missing value for option '--{option.Name}'");
                            _codeBuilder.AppendLine("processingError = true;");
                        }
                        break;
                    }
                case var positional:
                    {
                        _codeBuilder.AppendLine(
                    $"if (!MatchNextPositional(args, ref {positional.SourceName}, s => {ConvertParameter(positional.Type, "s")}, ref unclaimedArgs))");
                        using (_codeBuilder.IndentBraces())
                        {
                            WriteError($"Missing value for argument '{positional.SourceName}'");
                            _codeBuilder.AppendLine("processingError = true;");
                        }
                        break;
                    }
            }

            _codeBuilder.AppendLine();
        }
    }

    private string ConvertParameter(ParameterType type, string expression)
    {
        return type switch
        {
            ParameterType.AsIs => expression,
            ParameterType.Bool => $"bool.Parse({expression})",
            ParameterType.Parse p => $"{p.FullyQualifiedTypeName}.{p.ParseMethodName}({expression})",
            ParameterType.Constructor c => $"new {c.FullyQualifiedTypeName}({expression})",
            ParameterType.ExplicitCast c => $"({c.FullyQualifiedTypeName})({expression})",
            ParameterType.Enum e => $"({e.FullyQualifiedTypeName})Enum.Parse(typeof({e.FullyQualifiedTypeName}), {expression}, ignoreCase: true)",
            _ => throw new NotSupportedException("Unsupported parameter type: " + type.GetType().Name)
        };
    }

    private string GetHelpTextInline(CommandParameter parameter)
    {
        return parameter switch
        {
            CommandParameter.Option p when IsFlag(p) && p.Alias.IsSome(out var alias) => $"[-{alias}, --{p.Name}]",
            CommandParameter.Option p when IsFlag(p) => $"[<{p.Name}>]",
            CommandParameter.Option p when p.Alias.IsSome(out var alias) => $"[-{alias}, --{p.Name} <{p.SourceName}>]",
            CommandParameter.Option p => $"[--{p.Name}]",
            var p => $"<{p.Name}>",
        };

        bool IsFlag(CommandParameter p) => p switch
        {
            CommandParameter.Option { Type: ParameterType.Bool } => true,
            _ => false
        };
    }

    private string GetSoloHelpFirstColumn(CommandParameter.Positional parameter)
    {
        return parameter switch
        {
            { Type: ParameterType.Enum e } =>
                $"<{e.EnumValues.Select(v => v.HelpName).Truncate(3, "..").StringJoin("|")}>",
            var p => $"{p.Name}",
        };
    }

    private string GetSoloHelpFirstColumn(CommandParameter.Option parameter)
    {
        var parameterName = GetNamesForOption(parameter).StringJoin(", ");

        return parameter switch
        {
            { Type: ParameterType.Enum e } =>
                $"{parameterName} {e.EnumValues.Select(v => v.HelpName).Truncate(3, "..").StringJoin("|")}",
            { Type: ParameterType.Bool } =>
                $"{parameterName}",
            _ =>
                $"{parameterName} <{parameter.SourceName}>",
        };
    }

    private string GetOptionDescription(CommandParameter.Option parameter) =>
        parameter.HelpDefaultValue.IsSome(out var defaultValue)
            ? $"{parameter.Description | ""} (default: {defaultValue})"
            : parameter.Description | "";

    private string[] GetNamesForOption(CommandParameter.Option option) =>
        (option.Name, option.Alias) switch
        {
            ((true, var n), (true, var a)) => [$"-{a}", $"--{n}"],
            ((true, var n), _) => [$"--{n}"],
            (_, (true, var a)) => [$"-{a}"],
            _ => throw new InvalidOperationException("Option must have a name")
        };

    private string GetMatcherNameArguments(CommandParameter.Option option)
    {
        var names = GetNamesForOption(option);
        var optionName = names.Length == 2 ? names[1] : names[0];
        var optionAlias = names.Length == 2 ? $"\"{names[0]}\"" : "\"\"";

        return $"\"{optionName}\", {optionAlias}";
    }

    private void WriteError(string errorMessage)
    {
        _codeBuilder.AppendLine($"Console.ForegroundColor = ConsoleColor.Red;");
        _codeBuilder.AppendLine($"Console.Error.WriteLine($\"{errorMessage}\");");
        _codeBuilder.AppendLine($"Console.ForegroundColor = consoleColor;");
    }
    
    private void WriteVersionMethod()
    {
        _codeBuilder.AppendLine(
            """
            static void PrintVersion()
            {
                Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            }
            """
            );
    }

    private void WriteHelperMethods()
    {
        _codeBuilder.AppendLine(
            """
                    #pragma warning disable CS8321
                    // Tracks unclaimed argument indexes. Up to 64 arguments are held in a ulong bit mask
                    // (a set bit means unclaimed); longer invocations fall back to a bool[].
                    private struct UnclaimedArgs
                    {
                        private ulong _mask;
                        private readonly bool[] _flags;

                        public UnclaimedArgs(int length, int start)
                        {
                            Length = length;
                            _mask = 0;
                            _flags = Array.Empty<bool>();

                            if (length <= 64)
                            {
                                if (start < length)
                                {
                                    var count = length - start;
                                    var bits = count == 64 ? ulong.MaxValue : (1UL << count) - 1;
                                    _mask = bits << start;
                                }
                            }
                            else
                            {
                                _flags = new bool[length];

                                for (var i = start; i < length; i++)
                                {
                                    _flags[i] = true;
                                }
                            }
                        }

                        public int Length { get; }

                        public bool this[int index] =>
                            index >= 0 && index < Length
                            && (Length <= 64 ? (_mask & (1UL << index)) != 0 : _flags[index]);

                        public bool Any => Length <= 64 ? _mask != 0 : Array.IndexOf(_flags, true) >= 0;

                        public void Clear(int index)
                        {
                            if (Length <= 64)
                            {
                                _mask &= ~(1UL << index);
                            }
                            else
                            {
                                _flags[index] = false;
                            }
                        }

                        public string Describe(string[] args)
                        {
                            var unclaimed = new List<string>();

                            for (var i = 0; i < Length; i++)
                            {
                                if (this[i])
                                {
                                    unclaimed.Add(args[i]);
                                }
                            }

                            return string.Join(", ", unclaimed);
                        }
                    }

                    static bool MatchNextPositional<T>(string[] args, ref T value, Func<string, T> mapper, ref UnclaimedArgs unclaimedArgs)
                    {
                        for (var index = 0; index < unclaimedArgs.Length; index++)
                        {
                            if (!unclaimedArgs[index]) continue;
                            var arg = args[index];
                            if (!arg.StartsWith("-"))
                            {
                                value = mapper(arg);
                                unclaimedArgs.Clear(index);
                                return true;
                            }
                        }

                        return false;
                    }

                    static bool MatchNextFlag(string[] args, string optionName, string optionAlias, ref bool value, ref UnclaimedArgs unclaimedArgs)
                    {
                        for (var index = 0; index < unclaimedArgs.Length; index++)
                        {
                            if (!unclaimedArgs[index]) continue;
                            var arg = args[index];
                        
                            if (arg.EndsWith(":true", StringComparison.InvariantCultureIgnoreCase) || arg.EndsWith("=true", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var argOptionName = arg.Substring(0, arg.Length - 5);

                                if (optionName == argOptionName || optionAlias.Length > 0 && optionAlias == argOptionName)
                                {
                                    value = true;
                                    unclaimedArgs.Clear(index);
                                    return true;
                                }
                            }
                        
                            if (arg.EndsWith(":false", StringComparison.InvariantCultureIgnoreCase) || arg.EndsWith("=false", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var argOptionName = arg.Substring(0, arg.Length - 6);

                                if (optionName == argOptionName || optionAlias.Length > 0 && optionAlias == argOptionName)
                                {
                                    value = false;
                                    unclaimedArgs.Clear(index);
                                    return true;
                                }
                            }

                            if (optionName == arg || optionAlias.Length > 0 && optionAlias == arg)
                            {
                                value = true;
                                unclaimedArgs.Clear(index);
                                return true;
                            }
                        }

                        return false;
                    }

                    // Returns 0 if the option is not found
                    // Returns 1 if the option is found and the value is found
                    // Returns 2 if the option is found but the value is missing
                    static int MatchNextOption<T>(string[] args, string optionName, string optionAlias, ref T value, Func<string, T> mapper, ref UnclaimedArgs unclaimedArgs)
                    {
                        for (var i = 0; i < unclaimedArgs.Length; i++)
                        {
                            if (!unclaimedArgs[i]) continue;
                            var arg = args[i];

                            if (optionName == arg || optionAlias.Length > 0 && optionAlias == arg)
                            {
                                if (i + 1 < args.Length && unclaimedArgs[i + 1] && !args[i + 1].StartsWith("-"))
                                {
                                    value = mapper(args[i + 1]);
                                    unclaimedArgs.Clear(i);
                                    unclaimedArgs.Clear(i + 1);
                                    return 1;
                                }
                                
                                return 2;
                            }
            
                            var colonIndex = arg.IndexOf(':');

                            if (colonIndex >= 0 && IsOptionName(optionName, optionAlias, arg, colonIndex))
                            {
                                value = mapper(arg.Substring(colonIndex + 1));
                                unclaimedArgs.Clear(i);
                                return 1;
                            }

                            var equalsIndex = arg.IndexOf('=');

                            if (equalsIndex >= 0 && IsOptionName(optionName, optionAlias, arg, equalsIndex))
                            {
                                value = mapper(arg.Substring(equalsIndex + 1));
                                unclaimedArgs.Clear(i);
                                return 1;
                            }
                        }

                        return 0;
                    }

                    static bool IsOptionName(string optionName, string optionAlias, string arg, int nameLength)
                    {
                        return (optionName.Length == nameLength && string.CompareOrdinal(arg, 0, optionName, 0, nameLength) == 0)
                            || (optionAlias.Length > 0 && optionAlias.Length == nameLength && string.CompareOrdinal(arg, 0, optionAlias, 0, nameLength) == 0);
                    }
                    
                    #pragma warning restore CS8321
            """);
    }

    private void WriteHelp(
        ImmutableArray<string> path,
        CommandTree command)
    {
        _codeBuilder.AppendLines(
            $"Console.WriteLine(\"{applicationName}\");"
        );

        if (command is SubCommand s)
        {
            _codeBuilder.AppendLines(
                $"Console.WriteLine();",
                "Console.ForegroundColor = commandNameColor;",
                $"Console.WriteLine(\"{s.CommandName}\");",
                "Console.ForegroundColor = consoleColor;"
            );
        }

        if (command.Description.HasValue && command.Method.IsNone)
        {
            _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");

            _codeBuilder.AppendLines(
                $"Console.WriteLine(\"    {command.Description.Trim()}\");"
            );

            _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");

            _codeBuilder.AppendLines(
                "Console.WriteLine();"
            );
        }

        if (command.Method.IsSome(out var method))
        {
            _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");

            if (method.Description.IsSome(out var methodDescription))
            {
                _codeBuilder.AppendLines(
                    $"Console.WriteLine(\"    {method.Description.Trim()}\");"
                );
            }

            _codeBuilder.AppendLine($"Console.WriteLine();");
            _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");

            var allHelpText = path.Concat(
                method.MandatoryParameters.Select(GetHelpTextInline)
            );

            _codeBuilder.AppendLines(
                $"Console.WriteLine(\"Usage:\");",
                $"Console.WriteLine($\"    {String.Join(" ", allHelpText)} [options]\");"
            );

            if (method.MandatoryParameters.Any())
            {
                _codeBuilder.AppendLines(
                    $"Console.WriteLine();",
                    $"Console.WriteLine(\"Parameters:\");"
                );

                var helpNames =
                    method.MandatoryParameters.Select(p => (GetSoloHelpFirstColumn(p), p.Description));

                var longestParameter = helpNames.MaxOrDefault(x => x.Item1.Length);

                foreach (var (helpName, description) in helpNames)
                {
                    _codeBuilder.AppendLines(
                        $"Console.Write(\"    {helpName.PadRight(longestParameter)}  \");",
                        "Console.ForegroundColor = helpTextColor;",
                        $"Console.WriteLine(\"{description}\");"
                    );

                    _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                }
            }

            {
                _codeBuilder.AppendLines(
                    $"Console.WriteLine();",
                    $"Console.WriteLine(\"Options:\");"
                );

                var helpNames = method.Options
                    .Select(p => (
                        GetSoloHelpFirstColumn(p),
                        GetOptionDescription(p),
                        p.GetAllowedValues()))
                    .Append(("-?, -h, --help", "Show help and usage information", None));

                var longestParameter = helpNames.MaxOrDefault(x => x.Item1.Length);
                var leadingWhitespace = new string(' ', longestParameter);

                foreach (var (helpName, description, allowedValues) in helpNames)
                {
                    _codeBuilder.AppendLines(
                        $"Console.Write(\"    {helpName.PadRight(longestParameter)}  \");",
                        "Console.ForegroundColor = helpTextColor;",
                        $"Console.WriteLine(\"{SourceValueUtils.EscapeCSharpString(description)}\");"
                    );

                    foreach (var allowedValue in allowedValues)
                    {
                        var valueName = allowedValue.HelpName;
                        var valueDescription = allowedValue.Description;

                        var valueHelpText =
                            valueDescription.HasValue
                                ? $"{valueName}: {valueDescription}"
                                : $"{valueName}";

                        _codeBuilder.AppendLine(
                            $"Console.WriteLine(\"{leadingWhitespace}      - {valueHelpText}\");");
                    }

                    _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                }
            }

            _codeBuilder.AppendLine("Console.WriteLine();");
        }

        WriteSubCommandHelpTextInline(command);
    }

    private void WriteSubCommandHelpTextInline(CommandTree command)
    {
        if (!command.SubCommands.Any())
        {
            return;
        }
        
        _codeBuilder.AppendLines(
            "Console.WriteLine(\"Commands:\");"
        );

        var firstColumnLength = command.SubCommands
            .SelectMany(GetColumns)
            .DefaultIfEmpty(("", ""))
            .Max(x => x.Item1.Length);

        foreach (var subCommand in command.SubCommands)
        {
            if (!subCommand.ShowInHelp)
            {
                continue;
            }

            foreach (var (column1, column2) in GetColumns(subCommand))
            {
                _codeBuilder.AppendLine(
                    $"Console.Write(\"    {column1.PadRight(firstColumnLength)}\");"
                );

                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
                _codeBuilder.AppendLine($"Console.WriteLine(\"  {column2.Trim()}\");");
                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }
        }

        IEnumerable<(string column1, string column2)> GetColumns(CommandTree c)
        {
            if (c is SubCommand s && s.Method.IsSome(out var m))
            {
                yield return (
                    String.Join(" ", [
                        s.CommandName,
                        ..m.MandatoryParameters.Select(GetHelpTextInline),
                        m.Options.Any() ? "[options]" : ""
                    ]),
                    m.Description | "");
            }

            if (c is SubCommand s1 && s1.SubCommands.Any())
            {
                yield return (
                    $"{s1.CommandName} ..",
                    c.Description | "");
            }

            if (c is Root r && r.Method.IsSome(out var m1))
            {
                yield return ($"{m1.Parameters.Select(GetHelpTextInline).StringJoin(" ")}", m1.Description | "");
            }
        }
    }
}
