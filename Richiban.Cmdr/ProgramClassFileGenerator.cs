using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Richiban.Cmdr.CommandTree;

namespace Richiban.Cmdr;

internal class ProgramClassFileGenerator(
    string assemblyName,
    Root rootCommand) : CodeFileGenerator
{
    private readonly CodeBuilder _codeBuilder = new CodeBuilder();
    public override string FileName => "Program.g.cs";

    private void WriteCommandDebug(CommandTree command)
    {
        var commandName = command is SubCommand sub ? sub.CommandName : "root";
        _codeBuilder.AppendLines($"// {commandName} command{(rootCommand.Method.IsSome(out _) ? "*" : "")}");

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

        _codeBuilder.AppendLine($"// Found {rootCommand.SubCommands.Count} commands");
        WriteCommandDebug(rootCommand);
        _codeBuilder.AppendLine();

        // _codeBuilder.AppendLine();
        // _codeBuilder.AppendLine("handleArguments:");

        _codeBuilder.AppendLine("var (positionalArgs, options, isHelp) = NormaliseArgs(args);");

        WriteCommand(rootCommand, []);

        _codeBuilder.AppendLine();
        WriteHelperMethods();

        return _codeBuilder.ToString();
    }

    private void WriteCommand(CommandTree command, ImmutableArray<string> path)
    {
        var minPositionalCount = path.Length + command.MandatoryParameterCount;
        var maxPositionalCount = minPositionalCount + command.OptionalParameterCount;

        if (command is SubCommand s)
        {
            _codeBuilder.AppendLines(
                $"if (positionalArgs.Length >= {path.Length} && positionalArgs[{path.Length - 1}] == \"{s.CommandName}\")");
        }
        else
        {
            _codeBuilder.AppendLines(
                $"if (positionalArgs.Length >= 0)");
        }

        using (_codeBuilder.IndentBraces())
        {
            foreach (var c in command.SubCommands)
            {
                WriteCommand(c, path.Add(c.CommandName));
            }

            _codeBuilder.AppendLines(
                $"if (positionalArgs.Length >= {minPositionalCount} && positionalArgs.Length <= {maxPositionalCount} && !isHelp)");

            using (_codeBuilder.IndentBraces())
            {
                _codeBuilder.AppendLines($"// Found {command.Parameters.Count()} parameters");
                if (command.Method.IsSome(out var method))
                {
                    foreach (var (p, i) in command.Parameters.OfType<CommandParameterModel.CommandPositionalParameterModel>().Select((p, i) => (p, i)))
                    {
                        _codeBuilder.AppendLines(
                            $"var {p.Name} = positionalArgs[{path.Length + i}];");
                    }

                    foreach (var (p, i) in command.Parameters.OfType<CommandParameterModel.CommandOptionalPositionalParameterModel>().Select((p, i) => (p, i)))
                    {
                        _codeBuilder.AppendLines(
                            $"var {p.Name} = positionalArgs.Length >= {minPositionalCount + i + 1} ? positionalArgs[{minPositionalCount + i}] : {p.DefaultValue};");
                    }

                    foreach (var (flag, i) in command.Parameters.OfType<CommandParameterModel.CommandFlagModel>().Select((p, i) => (p, i)))
                    {
                        if (flag.ShortForm.IsSome(out var shortForm))
                        {
                            _codeBuilder.AppendLines(
                                $"var {flag.Name} = options.Contains(\"--{flag.Name}\") || options.Contains(\"-{shortForm}\");");
                        }
                        else
                        {
                            _codeBuilder.AppendLines(
                                $"var {flag.Name} = options.Contains(\"--{flag.Name}\");");
                        }
                    }

                    var argString = String.Join(", ", command.Parameters.Select(x => x.Name));

                    _codeBuilder.AppendLine($"{method.FullyQualifiedName}({argString});");
                    _codeBuilder.AppendLine("return;");
                }
                else
                {
                    WriteHelp(assemblyName, path, command);
                    _codeBuilder.AppendLine("return;");
                }
            }

            _codeBuilder.AppendLines("", $"if (positionalArgs.Length < {minPositionalCount} && !isHelp)");
            
            using (_codeBuilder.IndentBraces())
            {
                if (command is SubCommand sub)
                {
                    WriteError($"Missing arguments for command '{sub.CommandName}'");
                }
                else
                {
                    WriteError($"Missing arguments");
                }
            }

            _codeBuilder.AppendLines("", $"if (positionalArgs.Length > {maxPositionalCount} && !isHelp)");
            
            using (_codeBuilder.IndentBraces())
            {
                if (command is SubCommand sub)
                {
                    WriteError($"Unrecognised arguments for command '{sub.CommandName}'");
                }
                else
                {
                    WriteError($"Unrecognised arguments");
                }
            }

            WriteHelp(assemblyName, path, command);
            _codeBuilder.AppendLine("return;");
        }
    }

    private string GetHelpTextInPlace(CommandParameterModel parameter)
    {
        return parameter switch
        {
            CommandParameterModel.CommandOptionalPositionalParameterModel p => $"[<{p.Name}>]",
            CommandParameterModel.CommandFlagModel p when p.ShortForm.IsSome(out var shortForm) => $"[-{shortForm} | --{p.Name}]",
            CommandParameterModel.CommandFlagModel p => $"[--{p.Name}]",
            var p => $"<{p.Name}>",
        };
    }

    private string GetHelpTextOutOfPlace(CommandParameterModel parameter)
    {
        return parameter switch
        {
            CommandParameterModel.CommandOptionalPositionalParameterModel p => $"{p.Name}",
            CommandParameterModel.CommandFlagModel p when p.ShortForm.IsSome(out var shortForm) => $"-{shortForm} | --{p.Name}",
            CommandParameterModel.CommandFlagModel p => $"--{p.Name}",
            var p => $"{p.Name}",
        };
    }

    private void WriteError(string errorMessage)
    {
        _codeBuilder.AppendLine($"Console.ForegroundColor = ConsoleColor.Red;");
        _codeBuilder.AppendLine($"Console.Error.WriteLine(\"{errorMessage}\");");
        _codeBuilder.AppendLine($"Console.ForegroundColor = consoleColor;");
    }

    private void WriteHelperMethods()
    {
        _codeBuilder.AppendLine(
                    """
            static (ImmutableArray<string> positionalArgs, ImmutableArray<string> options, bool isHelp) NormaliseArgs(string[] args)
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

                return (positionalArgs.ToImmutableArray(), options.ToImmutableArray(), isHelp);
            }
            """);
    }

    // private void WriteDefaultCase(string assemblyName, List<Com> commands)
    // {
    //     _codeBuilder.AppendLine("default: ");

    //     using (_codeBuilder.Indent())
    //     {
    //         _codeBuilder.AppendLines($"Console.WriteLine(\"{assemblyName}\");", "Console.WriteLine();");
    //         _codeBuilder.AppendLine("Console.WriteLine(\"Commands:\");");

    //         var availableCommands = commands
    //             .Select(c => c.GetHelpText())
    //             .ToImmutableArray();

    //         var longestCommand = availableCommands.Max(x => x.Item1.Length);

    //         foreach (var (command, description) in availableCommands)
    //         {
    //             var cmd = command.PadRight(longestCommand);
    //             _codeBuilder.AppendLine($"Console.Write(\"    {cmd}\");");
    //             _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
    //             _codeBuilder.AppendLine($"Console.WriteLine(\"  {description}\");");
    //             _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
    //         }

    //         _codeBuilder.AppendLine("break;");
    //     }
    // }

    private void WriteHelp(
        string assemblyName,
        ImmutableArray<string> path,
        CommandTree com)
    {
        var pathStrings = path.Select(x => $"\"{x}\"");

        var allHelpText = path.Concat(
            com.Parameters.Select(GetHelpTextInPlace)
        );

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

        if (com.Description.HasValue)
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
                com.Parameters.Select(p => (GetHelpTextOutOfPlace(p), p.Description));

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

    }
}
