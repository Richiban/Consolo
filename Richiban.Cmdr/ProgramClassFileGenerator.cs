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

        void WriteCommand(CommandModel command, ImmutableArray<string> path)
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

                    foreach (var c in command.SubCommands)
                    {
                        WriteCommand(c, path.Add(c.CommandName));
                    }
                    
                    break;
                case SubCommandModel subCommand:
                    var a = path.Length + subCommand.MandatoryParameterCount;
                    var b = path.Length + subCommand.MandatoryParameterCount + subCommand.OptionalParameterCount;
                    
                    _codeBuilder.AppendLines(
                        $"if (positionalArgs.Length >= {path.Length} && positionalArgs[{path.Length - 1}] == \"{subCommand.CommandName}\")");

                    using (_codeBuilder.IndentBraces())
                    {
                        foreach (var c in command.SubCommands)
                        {
                            WriteCommand(c, path.Add(c.CommandName));
                        }

                        _codeBuilder.AppendLines(
                            $"if (positionalArgs.Length >= {a} && positionalArgs.Length <= {b})");

                        using (_codeBuilder.IndentBraces())
                        {
                            if (subCommand.Method.IsSome(out var method))
                            {
                                foreach (var (p, i) in subCommand.Parameters.OfType<CommandParameterModel.CommandPositionalParameterModel>().Select((p, i) => (p, i)))
                                {
                                    _codeBuilder.AppendLines(
                                        $"var {p.Name} = positionalArgs[{path.Length + i}];");
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

                        _codeBuilder.AppendLine("else");
                        using (_codeBuilder.IndentBraces())
                        {
                            _codeBuilder.AppendLine($"Console.Error.WriteLine(\"Invalid number of arguments for command '{subCommand.CommandName}'\");");
                            WriteHelp(assemblyName, path, subCommand);
                            _codeBuilder.AppendLine("return;");
                        }

                        break;
                    }
            }
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
            var p => $"<{p.Name}>",
        };
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
        CommandModel com)
    {
        var pathStrings = path.Select(x => $"\"{x}\"");

        var allHelpText = path.Concat(
            com.Parameters.Select(GetHelpTextInPlace)
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
}
