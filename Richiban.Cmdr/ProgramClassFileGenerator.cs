using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Richiban.Cmdr.CommandModel;

namespace Richiban.Cmdr;

internal class ProgramClassFileGenerator(
    string assemblyName,
    RootCommandModel rootCommand) : CodeFileGenerator
{
    private readonly CodeBuilder _codeBuilder = new CodeBuilder();
    public override string FileName => "Program.g.cs";

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
        _codeBuilder.AppendLine("handleArguments:");

        _codeBuilder.AppendLine("var (positionalArgs, options) = NormaliseArgs(args);");
        _codeBuilder.AppendLine($"// Found {rootCommand.SubCommands.Count} commands");

        WriteCommand(rootCommand, []);

        _codeBuilder.AppendLine();
        WriteHelperMethods();

        return _codeBuilder.ToString();

        void WriteCommand(CommandModel command, ImmutableArray<string> depth)
        {
            switch (command)
            {
                case RootCommandModel { Method: var m } rootCommand when m.IsSome(out var m2):
                    _codeBuilder.AppendLine($"if (positionalArgs.Length == 0 && options.Length == 0)");
                    using (_codeBuilder.IndentBraces())
                    {
                        _codeBuilder.AppendLine($"{m2.FullyQualifiedName}();");
                        _codeBuilder.AppendLine("return;");
                    }
                    break;
                case SubCommandModel subCommand:
                    var a = depth.Length + subCommand.MandatoryParameterCount;
                    var b = depth.Length + subCommand.MandatoryParameterCount + subCommand.OptionalParameterCount;
                    
                    _codeBuilder.AppendLines(
                        $"if (positionalArgs.Length >= {depth.Length} && positionalArgs[{depth.Length - 1}] == \"{subCommand.CommandName}\")");

                    using (_codeBuilder.IndentBraces())
                    {
                        _codeBuilder.AppendLines(
                            $"if (",
                            $"    positionalArgs.Length >= {a} ",
                            $"    && positionalArgs.Length <= {b}",
                            ")");

                        using (_codeBuilder.IndentBraces())
                        {
                            if (subCommand.Method.IsSome(out var method))
                            {
                                foreach (var (p, i) in subCommand.Parameters.OfType<CommandParameterModel.CommandPositionalParameterModel>().Select((p, i) => (p, i)))
                                {
                                    _codeBuilder.AppendLines(
                                        $"var {p.Name} = positionalArgs[{depth.Length + i}];");
                                }

                                foreach (var (p, i) in subCommand.Parameters.OfType<CommandParameterModel.CommandOptionalPositionalParameterModel>().Select((p, i) => (p, i)))
                                {
                                    _codeBuilder.AppendLines(
                                        $"var {p.Name} = positionalArgs.Length >= {a + i + 1} ? positionalArgs[{a + i}] : {p.DefaultValue};");
                                }

                                foreach (var (flag, i) in subCommand.Parameters.OfType<CommandParameterModel.CommandFlagModel>().Select((p, i) => (p, i)))
                                {
                                    _codeBuilder.AppendLines(
                                        $"var {flag.Name} = options.Contains(\"--{flag.Name}\") || options.Contains(\"-{flag.ShortForm}\");");
                                }

                                var argString = String.Join(", ", subCommand.Parameters.Select(x => x.Name));

                                _codeBuilder.AppendLine($"{method.FullyQualifiedName}({argString});");
                                _codeBuilder.AppendLine("return;");
                            }
                            else 
                            {
                                if (subCommand.Description.IsSome(out var description))
                                {
                                    _codeBuilder.AppendLine($"Console.WriteLine(\"{description}\");");
                                    _codeBuilder.AppendLine("return;");
                                }
                                else 
                                {
                                    _codeBuilder.AppendLine($"Console.WriteLine(\"{subCommand.CommandName}\");");
                                    _codeBuilder.AppendLine("return;");
                                }
                            }
                        }

                        // _codeBuilder.AppendLine("else");
                        // using (_codeBuilder.IndentBraces())
                        // {
                        //     _codeBuilder.AppendLine($"Console.WriteLine(\"Invalid number of arguments for command '{subCommand.CommandName}'\");");
                        //     _codeBuilder.AppendLine("return;");
                        // }

                        break;
                    }
            }

            foreach (var c in command.SubCommands)
            {
                using var _ = _codeBuilder.Indent();
                WriteCommand(c, depth.Add(c.CommandName));
            }
        }
    }

    private void WriteHelperMethods()
    {
        _codeBuilder.AppendLine(
                    """
            static (ImmutableArray<string> positionalArgs, ImmutableArray<string> options) NormaliseArgs(string[] args)
            {
                var positionalArgs = new List<string>();
                var options = new List<string>();

                foreach (var arg in args)
                {
                    switch (arg)
                    {
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

                options.Sort((x, y) =>
                    (x, y) switch
                    {
                        ("--help" or "-h", _) => 1,
                        (_, "--help" or "-h") => -1,
                        _ => x.CompareTo(y),
                    });

                return (positionalArgs.ToImmutableArray(), options.ToImmutableArray());
            }
            """);
    }

    private List<Com> PrepareCommands(IReadOnlyCollection<MethodModel> results)
    {
        var commands = new List<Com>();

        foreach (var result in results)
        {
            var path = result.GroupCommandPath
                    .Select(x => x.Name)
                    .Append(result.ProvidedName | result.MethodName)
                    .Where(x => x is not (null or ""))
                    .ToImmutableArray();

            var method = $"{result.FullyQualifiedClassName}.{result.MethodName}";

            commands.Add(new Com(path, method, result.Parameters, result.Description | ""));
        }

        return commands;
    }

    private void WriteDefaultCase(string assemblyName, List<Com> commands)
    {
        _codeBuilder.AppendLine("default: ");

        using (_codeBuilder.Indent())
        {
            _codeBuilder.AppendLines($"Console.WriteLine(\"{assemblyName}\");", "Console.WriteLine();");
            _codeBuilder.AppendLine("Console.WriteLine(\"Commands:\");");

            var availableCommands = commands
                .Select(c => c.GetHelpText())
                .ToImmutableArray();

            var longestCommand = availableCommands.Max(x => x.Item1.Length);

            foreach (var (command, description) in availableCommands)
            {
                var cmd = command.PadRight(longestCommand);
                _codeBuilder.AppendLine($"Console.Write(\"    {cmd}\");");
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
                _codeBuilder.AppendLine($"Console.WriteLine(\"  {description}\");");
                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }

            _codeBuilder.AppendLine("break;");
        }
    }

    private void WriteCommandCase(string assemblyName, Com com)
    {
        var pathStrings = com.Path.Select(x => $"\"{x}\"");

        var requiredParameterDeclarations = com.Parameters
            .Where(x => x.IsRequired)
            .Select(x => $"var {x.Name}");

        var optionalParameters = com.Parameters
            .Where(x => !x.IsRequired && !x.IsFlag)
            .ToImmutableArray();

        var caseDeclarations = pathStrings
            .Concat(requiredParameterDeclarations)
            .Concat(optionalParameters.Any() ? ["..var optionalArguments"] : Array.Empty<string>());

        var flags = com.Parameters
            .Where(x => x.IsFlag)
            .ToImmutableArray();

        var optionalParameterDeclarations = optionalParameters
            .Select(x => $"var {x.Name}");

        var requiredParameters = com.Parameters
            .Where(x => x.IsRequired);

        WriteHelpCase(assemblyName, com);

        _codeBuilder.AppendLines(
            $"case ([{String.Join(", ", caseDeclarations)}], var options):",
            "{"
        );

        using (_codeBuilder.Indent())
        {
            foreach (var (optionalParam, i) in optionalParameters.Select((a, b) => (a, b)))
            {
                var leadingOptionalParams = optionalParameters.Take(i);
                var a = Enumerable.Range(0, i).Select(i => "_").Append($"var v{i}");
                var listMatch = String.Join(", ", a);

                _codeBuilder.AppendLine(
                    $"var {optionalParam.Name} = optionalArguments is [{listMatch}] ? v{i} : {SourceValueUtils.SourceValue(optionalParam.DefaultValue)};"
                );
            }

            foreach (var flag in flags)
            {
                if (flag.ShortForm.HasValue)
                {
                    _codeBuilder.Append(
                        $"var {flag.Name} = options.Contains(\"-{flag.ShortForm}\") || options.Contains(\"--{flag.Name}\")",
                        withIndentation: true
                    );
                }
                else
                {
                    _codeBuilder.Append(
                        $"var {flag.Name} = options.Contains(\"--{flag.Name}\")",
                        withIndentation: true
                    );
                }

                if (flag.DefaultValue.IsSome(out var defaultValue))
                {
                    _codeBuilder.Append(
                        $" || {defaultValue.ToLower()}"
                    );
                }

                _codeBuilder.Append(";");
                _codeBuilder.AppendLine();
            }

            if (flags.Any())
            {
                _codeBuilder.AppendLines(
                    $"if (options.Except([{String.Join(", ", flags.Select(x => $"\"--{x.Name}\""))}]).ToList() is {{Count: > 0}} unrecognisedOptions)",
                    "{",
                    "    Console.Error.WriteLine($\"Unrecogised option(s): {String.Join(\", \", unrecognisedOptions)}\");",
                    "    args = args.Append(\"--help\").ToArray();",
                    "    goto handleArguments;",
                    "}",
                    "else",
                    "{");

                using (_codeBuilder.Indent())
                {
                    WriteCommandMethodCall(com, optionalParameters, flags, requiredParameters);
                }

                _codeBuilder.AppendLine("}");
            }
            else 
            {
                WriteCommandMethodCall(com, optionalParameters, flags, requiredParameters);
            }
            _codeBuilder.AppendLine("break;");
        }
        _codeBuilder.AppendLine("}");
    }

    private void WriteCommandMethodCall(Com com, ImmutableArray<ParameterModel> optionalParameters, ImmutableArray<ParameterModel> flags, IEnumerable<ParameterModel> requiredParameters)
    {
        var arguments = requiredParameters
                        .Select(p => p.Name)
                        .Concat(optionalParameters.Select(x => $"{x.Name}: {x.Name}"))
                        .Concat(flags.Select(x => $"{x.Name}: {x.Name}"));

        _codeBuilder.AppendLine(
            $"{com.Method}({String.Join(", ", arguments)});"
        );
    }

    private void WriteHelpCase(
        string assemblyName, 
        Com com)
    {
        var pathStrings = com.Path.Select(x => $"\"{x}\"");

        _codeBuilder.AppendLines(
            $"case ([{String.Join(", ", pathStrings)}, ..], [.., \"--help\" or \"-h\"]):",
            "{"
        );

        var allHelpText = com.Path.Concat(
            com.Parameters.Select(x => x.GetHelpTextInPlace())
        );

        using (_codeBuilder.Indent())
        {
            _codeBuilder.AppendLine(
                $"Console.WriteLine("
            );

            using (_codeBuilder.Indent())
            {
                _codeBuilder.AppendLines(
                    "\"\"\"",
                    $"{assemblyName}",
                    "",
                    $"Command:",
                    $"    {String.Join(" ", allHelpText)}",
                    "\"\"\""
                );
            }

            _codeBuilder.AppendLines(
                $");"
            );

            if (!String.IsNullOrEmpty(com.Description))
            {
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");

                _codeBuilder.AppendLines(
                    $"Console.WriteLine(",
                    $"    \"\"\"",
                    "",
                    $"        {com.Description}",
                    $"    \"\"\"",
                    ");"
                );

                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }

            if (com.Parameters.Any())
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
                    com.Parameters.Select(x => (x.GetHelpTextOutOfPlace(), x.Description));

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

            _codeBuilder.AppendLines("break;");
        }

        _codeBuilder.AppendLines("}");
    }
}

class Com(
    IReadOnlyCollection<string> path,
    string method,
    IReadOnlyCollection<ParameterModel> parameters,
    string description)
{
    public IReadOnlyCollection<string> Path { get; } = path;
    public string Method { get; } = method;
    public IReadOnlyCollection<ParameterModel> Parameters { get; } = parameters;
    public string Description { get; } = description;

    public (string A, string Description) GetHelpText() => 
        (String.Join(" ", Path.Concat(Parameters.Select(GetParameterHelp))), Description);

    private static string GetParameterHelp(ParameterModel parameter) =>
        parameter switch
        {
            {IsFlag: true, ShortForm: {HasValue: false}} flag =>
                $"[--{flag.Name}]",
            {IsFlag: true, ShortForm: {HasValue: true}} flag =>
                $"[-{flag.ShortForm} | --{flag.Name}]",
            {IsRequired: true} => $"<{parameter.Name}>",
            _ => $"[{parameter.Name}]"
        };

    public static int CompareTo(Com left, Com right)
    {
        return left.Path.Count.CompareTo(right.Path.Count) switch
        {
            > 0 => -1,
            < 0 => 1,
            _ => left.Parameters.Count.CompareTo(right.Parameters.Count) switch
            {
                > 0 => -1,
                < 0 => 1,
                _ => 0
            },
        };
    }
}

static class Ext
{
    public static ImmutableArray<T> SortOrder<T>(
        this IEnumerable<T> source,
        Func<T, T, int> compareToFunc)
    {
        var items = source.ToArray();

        int CompareTo(T left, T right) => compareToFunc(left, right);

        Array.Sort(items, CompareTo);

        return items.ToImmutableArray();
    }

    public static string GetHelpTextInPlace(this ParameterModel parameter) =>
        parameter switch
        {
            { IsFlag: true } => parameter.ShortForm.HasValue
                                ? $"[-{parameter.ShortForm} | --{parameter.Name}]"
                                : $"[--{parameter.Name}]",
            { IsRequired: false } => $"[<{parameter.Name}>]",
            _ => $"<{parameter.Name}>",
        };

    public static string GetHelpTextOutOfPlace(this ParameterModel parameter) =>
        parameter switch
        {
            { IsFlag: true } => parameter.ShortForm.HasValue
                                ? $"-{parameter.ShortForm} | --{parameter.Name}"
                                : $"--{parameter.Name}",
            _ => $"{parameter.Name}",
        };
}