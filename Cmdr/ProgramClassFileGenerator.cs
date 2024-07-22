using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Cmdr.CommandTree;

namespace Cmdr;

internal class ProgramClassFileGenerator(
    string assemblyName,
    Root rootCommand) : CodeFileGenerator
{
    private readonly CodeBuilder _codeBuilder = new CodeBuilder();
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
        _codeBuilder.AppendLines(
            "using System;",
            "using System.Linq;",
            "using System.Collections.Generic;",
            "using System.Collections.Immutable;");

        _codeBuilder.AppendLine();

        _codeBuilder.AppendLine("var consoleColor = Console.ForegroundColor;");
        _codeBuilder.AppendLine("var helpTextColor = ConsoleColor.Green;");

        _codeBuilder.AppendLine();
        _codeBuilder.AppendLine("// Commands marked * have an associated method");
        WriteCommandDebug(rootCommand);
        _codeBuilder.AppendLine();

        _codeBuilder.AppendLine("var (positionalArgs, options, isHelp) = NormaliseArgs(args);");

        WriteCommandGroup(rootCommand, []);

        _codeBuilder.AppendLine();
        WriteHelperMethods();

        return _codeBuilder.ToString();
    }

    private void WriteCommandGroup(CommandTree command, ImmutableArray<string> path)
    {
        if (command is SubCommand s)
        {
            _codeBuilder.AppendLines(
                $"if (positionalArgs.Length >= {path.Length} && positionalArgs[{path.Length - 1}] == \"{s.CommandName}\")");
        }

        using (_codeBuilder.IndentBraces())
        {
            foreach (var c in command.SubCommands)
            {
                WriteCommandGroup(c, path.Add(c.CommandName));
            }

            if (command.Method.IsSome(out var method))
            {
                WriteMethodCall(command, path, method);
            }

            _codeBuilder.AppendLine("");
            WriteHelp(assemblyName, path, command);
            _codeBuilder.AppendLine("return;");
        }
    }

    private void WriteMethodCall(CommandTree command, ImmutableArray<string> path, CommandMethod method)
    {
        var minPositionalCount = path.Length + method.MandatoryParameterCount;
        var maxPositionalCount = minPositionalCount + method.OptionalParameterCount;

        _codeBuilder.AppendLines("if (!isHelp)");

        using (_codeBuilder.IndentBraces())
        {

            _codeBuilder.AppendLines(
                $"if (positionalArgs.Length >= {minPositionalCount} && positionalArgs.Length <= {maxPositionalCount})");

            using (_codeBuilder.IndentBraces())
            {
                WriteParameterAssignments(method, path, minPositionalCount);

                var argString = String.Join(", ", method.Parameters.Select(x => x.SourceName));

                _codeBuilder.AppendLine($"{method.FullyQualifiedName}({argString});");
                _codeBuilder.AppendLine("return;");
            }

            _codeBuilder.AppendLines("", $"if (positionalArgs.Length < {minPositionalCount})");

            using (_codeBuilder.IndentBraces())
            {
                if (command is SubCommand sub)
                {
                    _codeBuilder.AppendLine($"Console.ForegroundColor = ConsoleColor.Red;");
                    _codeBuilder.Append($"Console.Error.WriteLine($\"", withIndentation: true);
                    _codeBuilder.Append($"Command {sub.CommandName}: Missing argument(s) ");
                    _codeBuilder.Append($"{{String.Join(\", \", new string[] {{ {String.Join(", ", method.MandatoryParameters.Select(x => $"\"'{x.Name}'\""))} }}.Skip(positionalArgs.Length - {path.Length}))}}");
                    _codeBuilder.Append($"\");");
                    _codeBuilder.AppendLine();
                    _codeBuilder.AppendLine($"Console.ForegroundColor = consoleColor;");
                }
                else
                {
                    WriteError($"Missing arguments");
                }
            }

            _codeBuilder.AppendLines("", $"if (positionalArgs.Length > {maxPositionalCount})");

            using (_codeBuilder.IndentBraces())
            {
                if (command is SubCommand sub)
                {
                    WriteError($"Too many arguments supplied for command '{sub.CommandName}'");
                }
                else
                {
                    WriteError($"Too many arguments supplied");
                }
            }
        }
    }

    private void WriteParameterAssignments(CommandMethod method, ImmutableArray<string> path, int minPositionalCount)
    {
        foreach (var (p, i) in method.Parameters.OfType<CommandParameter.Positional>().Select((p, i) => (p, i)))
        {

            _codeBuilder.AppendLines(
                $"var {p.SourceName} = {ConvertParameter(p.Type, $"positionalArgs[{path.Length + i}]")};");

        }

        foreach (var (p, i) in method.Parameters.OfType<CommandParameter.OptionalPositional>().Select((p, i) => (p, i)))
        {
            _codeBuilder.AppendLines(
                $"var {p.SourceName} = positionalArgs.Length >= {minPositionalCount + i + 1} ? {ConvertParameter(p.Type, $"positionalArgs[{minPositionalCount + i}]")} : {p.DefaultValue};");
        }

        foreach (var (flag, i) in method.Parameters.OfType<CommandParameter.Flag>().Select((p, i) => (p, i)))
        {
            if (flag.ShortForm.IsSome(out var shortForm))
            {
                _codeBuilder.AppendLines(
                    $"var {flag.SourceName} = options.Contains(\"--{flag.Name}\") || options.Contains(\"-{shortForm}\");");
            }
            else
            {
                _codeBuilder.AppendLines(
                    $"var {flag.SourceName} = options.Contains(\"--{flag.Name}\");");
            }
        }
    }

    private string ConvertParameter(ParameterType type, string expression)
    {
        return type switch
        {
            ParameterType.AsIs => expression,
            ParameterType.Parse p => $"{p.GetFullyQualifiedName()}.{p.ParseMethodName}({expression})",
            ParameterType.Constructor c => $"new {c.GetFullyQualifiedName()}({expression})",
            ParameterType.ExplicitCast c => $"({c.GetFullyQualifiedName()})({expression})",
            ParameterType.Enum e => $"({e.GetFullyQualifiedName()})Enum.Parse(typeof({e.GetFullyQualifiedName()}), {expression}, ignoreCase: true)",
            _ => throw new NotSupportedException("Unsupported parameter type: " + type.GetType().Name)
        };
    }

    private string GetHelpTextInline(CommandParameter parameter)
    {
        return parameter switch
        {
            CommandParameter.OptionalPositional p => $"[<{p.Name}>]",
            CommandParameter.Flag p when p.ShortForm.IsSome(out var shortForm) => $"[-{shortForm} | --{p.Name}]",
            CommandParameter.Flag p => $"[--{p.Name}]",
            var p => $"<{p.Name}>",
        };
    }

    private string GetSoloHelpText(CommandParameter parameter)
    {
        return parameter switch
        {
            CommandParameter.Positional { Type: ParameterType.Enum e } => 
                $"{parameter.Name}: {String.Join("|", e.EnumValues)}",
            CommandParameter.OptionalPositional { Type: ParameterType.Enum e } => 
                $"{parameter.Name}: {String.Join("|", e.EnumValues)}",
            CommandParameter.OptionalPositional p => 
                $"{p.Name}",
            CommandParameter.Flag p when p.ShortForm.IsSome(out var shortForm) => 
                $"-{shortForm} | --{p.Name}",
            CommandParameter.Flag p => 
                $"--{p.Name}",
            var p => $"{p.Name}",
        };
    }

    private void WriteError(string errorMessage)
    {
        _codeBuilder.AppendLine($"Console.ForegroundColor = ConsoleColor.Red;");
        _codeBuilder.AppendLine($"Console.Error.WriteLine($\"{errorMessage}\");");
        _codeBuilder.AppendLine($"Console.ForegroundColor = consoleColor;");
    }

    private void WriteHelperMethods()
    {
        _codeBuilder.AppendLine(
                    """
            static (ImmutableArray<string> positionalArgs, ImmutableHashSet<string> options, bool isHelp) NormaliseArgs(string[] args)
            {
                var positionalArgs = new List<string>();
                var options = new List<string>();
                var isHelp = false;

                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "-h" or "--help":
                            isHelp = true;
                            break;
                        case ['-', '-', ..]:
                            options.Add(arg);
                            break;
                        case ['-', not '-', ..]:
                            options.AddRange(arg.Skip(1).Select(c => $"-{c}"));
                            break;
                        case not (null or ""):
                            positionalArgs.Add(arg);
                            break;
                        default:
                            break;
                    }
                }

                return (positionalArgs.ToImmutableArray(), options.ToImmutableHashSet(), isHelp);
            }
            """);
    }

    private void WriteHelp(
        string assemblyName,
        ImmutableArray<string> path,
        CommandTree command)
    {
        var pathStrings = path.Select(x => $"\"{x}\"");

        if (command.Method.IsSome(out var method))
        {
            var allHelpText = path.Concat(
                method.Parameters.Select(GetHelpTextInline)
            );

            _codeBuilder.AppendLine(
                $"Console.WriteLine("
            );

            using (_codeBuilder.Indent())
            {
                _codeBuilder.AppendLines(
                    "\"\"\"",
                    $"{assemblyName}"
                );

                if (command is SubCommand s)
                {
                    _codeBuilder.AppendLines(
                        $"",
                        $"{s.CommandName}"
                    );
                }

                _codeBuilder.AppendLines(
                    "\"\"\""
                );
            }

            _codeBuilder.AppendLines(
                $");"
            );

            if (command.Description.HasValue)
            {
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");

                _codeBuilder.AppendLines(
                    $"Console.WriteLine(",
                    $"    \"\"\"",
                    $"        {command.Description.Trim()}",
                    "",
                    $"    \"\"\"",
                    ");"
                );

                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }

            _codeBuilder.AppendLines(
                $"Console.WriteLine(",
                "    \"\"\"",
                $"    Usage:",
                $"        {assemblyName} {String.Join(" ", allHelpText)} [options]",
                "    \"\"\"",
                ");");

            if (method.Parameters.Any())
            {
                _codeBuilder.AppendLines(
                    $"Console.WriteLine(",
                    "    \"\"\"",
                    "",
                    $"    Parameters:",
                    "    \"\"\"",
                    ");"
                );

                var helpNames =
                    method.Parameters.Select(p => (GetSoloHelpText(p), p.Description));

                var longestParameter = helpNames.Max(x => x.Item1.Length);

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
                    $"Console.WriteLine(",
                    "    \"\"\"",
                    "",
                    $"    Options:",
                    "    \"\"\"",
                    ");"
                );

                var helpNames = method.Flags
                    .Select(p => (GetSoloHelpText(p), p.Description))
                    .Append(("-h | --help", "Show help and usage information"));

                var longestParameter = helpNames.DefaultIfEmpty(("", "")).Max(x => x.Item1.Length);

                foreach (var (helpName, description) in helpNames)
                {
                    _codeBuilder.AppendLines(
                        $"Console.Write(\"    {helpName.PadRight(longestParameter)}  \");",
                        "Console.ForegroundColor = helpTextColor;",
                        $"Console.WriteLine(\"{description}\");"
                    );

                    _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                }
                    
                _codeBuilder.AppendLine("Console.WriteLine();");
            }
        }
        else
        {
            WriteSubCommandHelpTextInline(command);
        }
    }

    private void WriteSubCommandHelpTextInline(CommandTree command)
    {
        _codeBuilder.AppendLines(
            $"Console.WriteLine(\"{assemblyName}\");",
            "Console.WriteLine(\"\");",
            "Console.WriteLine(\"Commands:\");"
        );

        var firstColumnLength = command.SubCommands.Select(GetFirstColumn).DefaultIfEmpty("").Max(x => x.Length);

        foreach (var subCommand in command.SubCommands)
        {
            var firstColumn = GetFirstColumn(subCommand).PadRight(firstColumnLength);
            _codeBuilder.AppendLine(
                $"Console.Write(\"    {firstColumn}\");"
            );

            if (subCommand.Description.IsSome(out var description))
            {
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
                _codeBuilder.AppendLine($"Console.WriteLine(\"  {description.Trim()}\");");
                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }
            else
            {
                _codeBuilder.AppendLine("Console.WriteLine();");
            }
        }

        string GetFirstColumn(CommandTree c)
        {
            return c switch
            {
                SubCommand s when s.Method.IsSome(out var m) => $"{s.CommandName} {m.Parameters.Select(GetHelpTextInline).StringJoin(" ")}",
                Root r when r.Method.IsSome(out var m) => $"{m.Parameters.Select(GetHelpTextInline).StringJoin(" ")}",
                SubCommand s => s.CommandName,
                _ => ""
            };
        }
    }
}
