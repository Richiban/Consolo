using Microsoft.CodeAnalysis;
using static Consolo.CommandTree;
using static Consolo.Prelude;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Consolo;

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
        {
            _codeBuilder.AppendLines(
            "using System;",
            "using System.Linq;",
            "using System.Collections.Generic;",
            "using System.Collections.Immutable;");

            _codeBuilder.AppendLine("namespace " + assemblyName);
            using var _ = _codeBuilder.IndentBraces();

            _codeBuilder.AppendLine("/// <summary>");
            _codeBuilder.AppendLine("/// Entrypoint class for " + assemblyName);
            _codeBuilder.AppendLine("/// </summary>");
            _codeBuilder.AppendLine("public static class Program");
            using var _2 = _codeBuilder.IndentBraces();

            _codeBuilder.AppendLine("/// <summary>");
            _codeBuilder.AppendLine("/// Entrypoint for " + assemblyName);
            _codeBuilder.AppendLine("/// </summary>");
            _codeBuilder.AppendLine("public static void Main(string[] args)");
            using var _3 = _codeBuilder.IndentBraces();

            _codeBuilder.AppendLine();

            _codeBuilder.AppendLine("var consoleColor = Console.ForegroundColor;");
            _codeBuilder.AppendLine("var helpTextColor = ConsoleColor.Green;");

            _codeBuilder.AppendLine();
            _codeBuilder.AppendLine("// Commands marked * have an associated method");
            WriteCommandDebug(rootCommand);
            _codeBuilder.AppendLine();

            _codeBuilder.AppendLine("args = args.SelectMany(arg => arg.StartsWith(' ') && ! arg.StartsWith(\"--\") ? arg.Skip(1).Select(c => $\"-{c}\") : new [] { arg }).ToArray();");
            _codeBuilder.AppendLine();

            _codeBuilder.AppendLine("var isHelp = args.Any(arg => arg == \"--help\" || arg == \"-h\" || arg == \"-?\");");
            _codeBuilder.AppendLine();

            WriteCommandGroup(rootCommand, []);

            _codeBuilder.AppendLine();
            WriteHelperMethods();
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
                            WriteError($"Missing command after: \" + string.Join(\" \", args.Skip({path.Length - 1})) + \"");
                        }
                        else
                        {
                            WriteError("Missing command");
                        }
                    }
                    _codeBuilder.AppendLine("else");
                    using (_codeBuilder.IndentBraces())
                    {
                        WriteError($"Unknown command: \" + string.Join(\" \", args.Skip({path.Length})) + \"");
                    }
                }
            }

            _codeBuilder.AppendLine("");
            WriteHelp(assemblyName, path, command);
            _codeBuilder.AppendLine("return;");
        }
    }

    private void WriteCommandHandlerBody(ImmutableArray<string> path, CommandMethod method)
    {
        var positionalCount = path.Length + method.MandatoryParameterCount;

        _codeBuilder.AppendLines("if (!isHelp)");

        using (_codeBuilder.IndentBraces())
        {
            _codeBuilder.AppendLine("var remainingArgs = new SortedSet<int>(Enumerable.Range(0, args.Length));");

            for (var pathIndex = 0; pathIndex < path.Length; pathIndex++)
            {
                _codeBuilder.AppendLine($"remainingArgs.Remove({pathIndex});");
            }

            _codeBuilder.AppendLine("var processingError = false;");

            _codeBuilder.AppendLine();

            WriteParameterAssignments(method);

            _codeBuilder.AppendLines($"if (remainingArgs.Any())");

            using (_codeBuilder.IndentBraces())
            {
                WriteError("Unrecognised args: \" + string.Join(\", \", remainingArgs.Select(i => args[i])) + \"");
                _codeBuilder.AppendLine("processingError = true;");
            }

            _codeBuilder.AppendLines("", "if (!processingError)");

            using (_codeBuilder.IndentBraces())
            {
                var argString = String.Join(", ", method.Parameters.Select(x => x.SourceName));

                _codeBuilder.AppendLine($"{method.FullyQualifiedName}({argString});");
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
            _codeBuilder.AppendLine($"var {p.SourceName} = default({p.FullyQualifiedTypeName});");


            switch (p)
            {
                case CommandParameter.Option { IsFlag: true } flag:
                    {
                        var toSeek = GetNamesForOption(flag);

                        _codeBuilder.AppendLine(
                            $"MatchNextFlag(new [] {{ {GetNamesForOption(flag).Select(v => $"\"{v}\"").StringJoin(", ")} }}, ref {flag.SourceName}, remainingArgs);");
                        break;
                    }
                case CommandParameter.Option option:
                    {
                        _codeBuilder.AppendLine(
                            $"if (MatchNextOption(new [] {{ {GetNamesForOption(option).Select(v => $"\"{v}\"").StringJoin(", ")} }}, ref {option.SourceName}, s => {ConvertParameter(option.Type, "s")}, remainingArgs) == 2)");
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
                    $"if (!MatchNextPositional(ref {positional.SourceName}, s => {ConvertParameter(positional.Type, "s")}, remainingArgs))");
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
            CommandParameter.Option p when IsFlag(p) && p.Alias.IsSome(out var alias) => $"[-{alias} | --{p.Name}]",
            CommandParameter.Option p when IsFlag(p) => $"[<{p.Name}>]",
            CommandParameter.Option p when p.Alias.IsSome(out var alias) => $"[-{alias} | --{p.Name} <{p.SourceName}>]",
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
                $"<{e.EnumValues.Select(v => v.SourceName).Truncate(3, "..").StringJoin("|")}>",
            var p => $"{p.Name}",
        };
    }

    private string GetSoloHelpFirstColumn(CommandParameter.Option parameter)
    {
        var parameterName = GetNamesForOption(parameter).StringJoin(" | ");

        return parameter switch
        {
            { Type: ParameterType.Enum e } =>
                $"{parameterName} {e.EnumValues.Select(v => v.SourceName).StringJoin("|")}",
            { Type: ParameterType.Bool } =>
                $"{parameterName}",
            _ =>
                $"{parameterName} <{parameter.SourceName}>",
        };
    }

    private string[] GetNamesForOption(CommandParameter.Option option) =>
        (option.Name, option.Alias) switch
        {
            ((true, var n), (true, var a)) => [$"-{a}", $"--{n}"],
            ((true, var n), _) => [$"--{n}"],
            (_, (true, var a)) => [$"-{a}"],
            _ => throw new InvalidOperationException("Option must have a name")
        };

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
                    #pragma warning disable CS8321
                    bool MatchNextPositional<T>(ref T value, Func<string, T> mapper, ISet<int> remainingArgs)
                    {
                        foreach (var index in remainingArgs)
                        {
                            var arg = args[index];
                            if (!arg.StartsWith("-"))
                            {
                                value = mapper(arg);
                                remainingArgs.Remove(index);
                                return true;
                            }
                        }

                        return false;
                    }

                    bool MatchNextFlag(string[] optionNames, ref bool value, ISet<int> remainingArgs)
                    {
                        foreach (var index in remainingArgs)
                        {
                            var arg = args[index];
                        
                            if (arg.EndsWith(":true", StringComparison.InvariantCultureIgnoreCase) || arg.EndsWith("=true", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var optionName = arg.Substring(0, arg.Length - 5);

                                if (optionNames.Contains(optionName))
                                {
                                    value = true;
                                    remainingArgs.Remove(index);
                                    return true;
                                }
                            }
                        
                            if (arg.EndsWith(":false", StringComparison.InvariantCultureIgnoreCase) || arg.EndsWith("=false", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var optionName = arg.Substring(0, arg.Length - 6);

                                if (optionNames.Contains(optionName))
                                {
                                    value = false;
                                    remainingArgs.Remove(index);
                                    return true;
                                }
                            }

                            if (optionNames.Contains(arg))
                            {
                                value = true;
                                remainingArgs.Remove(index);
                                return true;
                            }
                        }

                        return false;
                    }

                    // Returns 0 if the option is not found
                    // Returns 1 if the option is found and the value is found
                    // Returns 2 if the option is found but the value is missing
                    int MatchNextOption<T>(string[] optionNames, ref T value, Func<string, T> mapper, ISet<int> remainingArgs)
                    {
                        foreach (var i in remainingArgs)
                        {
                            var arg = args[i];

                            if (optionNames.Contains(arg))
                            {
                                if (arg.Contains(':'))
                                {
                                    var parts = arg.Split(':', 2);
                                    value = mapper(parts[1]);
                                    remainingArgs.Remove(i);
                                    return 1;
                                }

                                if (arg.Contains('='))
                                {
                                    var parts = arg.Split('=', 2);
                                    value = mapper(parts[1]);
                                    remainingArgs.Remove(i);
                                    return 1;
                                }

                                if (i + 1 < args.Length && remainingArgs.Contains(i + 1) && !args[i + 1].StartsWith("-"))
                                {
                                    value = mapper(args[i + 1]);
                                    remainingArgs.Remove(i);
                                    remainingArgs.Remove(i + 1);
                                    return 1;
                                }
                                
                                return 2;
                            }
                        }

                        return 0;
                    }
                    
                    #pragma warning restore CS8321
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
                method.MandatoryParameters.Select(GetHelpTextInline)
            );

            _codeBuilder.AppendLines(
                $"Console.WriteLine(\"{assemblyName}\");",
                $"Console.WriteLine();"
            );

            if (command is SubCommand s)
            {
                _codeBuilder.AppendLines(
                    $"Console.WriteLine(\"{s.CommandName}\");"
                );
            }

            if (command.Description.HasValue)
            {
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");

                _codeBuilder.AppendLines(
                    $"Console.WriteLine(\"    {command.Description.Trim()}\");"
                );

                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }

            _codeBuilder.AppendLines(
                "Console.WriteLine();"
            );

            _codeBuilder.AppendLines(
                $"Console.WriteLine(\"Usage:\");",
                $"Console.WriteLine($\"    {assemblyName} {String.Join(" ", allHelpText)} [options]\");"
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
                    .Select(p => (GetSoloHelpFirstColumn(p), p.Description, p.GetAllowedValues()))
                    .Append(("-? | -h | --help", "Show help and usage information", None));

                var longestParameter = helpNames.MaxOrDefault(x => x.Item1.Length);

                foreach (var (helpName, description, allowedValues) in helpNames)
                {
                    _codeBuilder.AppendLines(
                        $"Console.Write(\"    {helpName.PadRight(longestParameter)}  \");",
                        "Console.ForegroundColor = helpTextColor;",
                        $"Console.WriteLine(\"{description}\");"
                    );

                    foreach (var (valueName, valueDescription) in allowedValues)
                    {
                        _codeBuilder.AppendLine(
                            $"Console.WriteLine(\"      {new string(' ', longestParameter)}- {valueName}: {valueDescription}\");");
                    }

                    _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                }
            }

            _codeBuilder.AppendLine("Console.WriteLine();");
        }
        else
        {
            WriteSubCommandHelpTextInline(command);
        }
    }

    private void WriteSubCommandHelpTextInline(CommandTree command)
    {
        _codeBuilder.AppendLines(
            $"Console.WriteLine(\"{assemblyName}\");"
        );

        if (command is SubCommand s)
        {
            _codeBuilder.AppendLines(
                "Console.WriteLine(\"\");",
                $"Console.WriteLine(\"{s.CommandName}\");"
            );
        }

        if (command.Description.IsSome(out _))
        {
            _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
            _codeBuilder.AppendLine($"Console.WriteLine(\"    {command.Description.Trim()}\");");
            _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
        }

        _codeBuilder.AppendLines(
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
                SubCommand s when s.Method.IsSome(out var m) =>
                    String.Join(" ", [
                        s.CommandName,
                        ..m.MandatoryParameters.Select(GetHelpTextInline),
                        m.Options.Any() ? "[options]" : ""
                    ]),
                Root r when r.Method.IsSome(out var m) =>
                    $"{m.Parameters.Select(GetHelpTextInline).StringJoin(" ")}",
                SubCommand s =>
                    $"{s.CommandName} ..",
                _ =>
                    ""
            };
        }
    }
}
