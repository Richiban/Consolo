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

        WriteCommandDebug(rootCommand);
        _codeBuilder.AppendLine();

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
                if (command.Method.IsSome(out var method))
                {
                    foreach (var (p, i) in command.Parameters.OfType<CommandParameter.Positional>().Select((p, i) => (p, i)))
                    {
                        _codeBuilder.AppendLines(
                            $"var {p.Name} = positionalArgs[{path.Length + i}];");
                    }

                    foreach (var (p, i) in command.Parameters.OfType<CommandParameter.OptionalPositional>().Select((p, i) => (p, i)))
                    {
                        _codeBuilder.AppendLines(
                            $"var {p.Name} = positionalArgs.Length >= {minPositionalCount + i + 1} ? positionalArgs[{minPositionalCount + i}] : {p.DefaultValue};");
                    }

                    foreach (var (flag, i) in command.Parameters.OfType<CommandParameter.Flag>().Select((p, i) => (p, i)))
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
                // else
                // {
                //     WriteHelp(assemblyName, path, command);
                //     _codeBuilder.AppendLine("return;");
                // }
            }

            _codeBuilder.AppendLines("", $"if (positionalArgs.Length < {minPositionalCount} && !isHelp)");

            using (_codeBuilder.IndentBraces())
            {
                if (command is SubCommand sub)
                {
                    _codeBuilder.AppendLine($"Console.ForegroundColor = ConsoleColor.Red;");
                    _codeBuilder.Append($"Console.Error.WriteLine($\"", withIndentation: true);
                    _codeBuilder.Append($"Command {sub.CommandName}: Missing arguments ");
                    _codeBuilder.Append($"{{String.Join(\", \", new string[] {{ {String.Join(", ", command.MandatoryParameters.Select(x => $"\"{x.Name}\""))} }}.Skip(positionalArgs.Length - {path.Length}))}}");
                    _codeBuilder.Append($"\");");
                    _codeBuilder.AppendLine();
                    _codeBuilder.AppendLine($"Console.ForegroundColor = consoleColor;");
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
                    WriteError($"Too many arguments supplied for command '{sub.CommandName}'");
                }
                else
                {
                    WriteError($"Too many arguments supplied");
                }
            }
            
            _codeBuilder.AppendLine("");
            WriteHelp(assemblyName, path, command);
            _codeBuilder.AppendLine("return;");
        }
    }

    private string GetHelpTextInPlace(CommandParameter parameter)
    {
        return parameter switch
        {
            CommandParameter.OptionalPositional p => $"[<{p.Name}>]",
            CommandParameter.Flag p when p.ShortForm.IsSome(out var shortForm) => $"[-{shortForm} | --{p.Name}]",
            CommandParameter.Flag p => $"[--{p.Name}]",
            var p => $"<{p.Name}>",
        };
    }

    private string GetHelpTextOutOfPlace(CommandParameter parameter)
    {
        return parameter switch
        {
            CommandParameter.OptionalPositional p => $"{p.Name}",
            CommandParameter.Flag p when p.ShortForm.IsSome(out var shortForm) => $"-{shortForm} | --{p.Name}",
            CommandParameter.Flag p => $"--{p.Name}",
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

    private void WriteHelp(
        string assemblyName,
        ImmutableArray<string> path,
        CommandTree command)
    {
        var pathStrings = path.Select(x => $"\"{x}\"");

        if (command.Method.IsSome(out var method))
        {
            var allHelpText = path.Concat(
                command.Parameters.Select(GetHelpTextInPlace)
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

            if (command.Description.HasValue)
            {
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");

                _codeBuilder.AppendLines(
                    $"Console.WriteLine(",
                    $"    \"\"\"",
                    "",
                    $"        {command.Description}",
                    $"    \"\"\"",
                    ");"
                );

                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }

            if (command.Parameters.Any())
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
                    command.Parameters.Select(p => (GetHelpTextOutOfPlace(p), p.Description));

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

        var firstColumnLength = command.SubCommands.Select(GetFirstColumn).Max(x => x.Length);

        foreach (var subCommand in command.SubCommands)
        {
            var firstColumn = GetFirstColumn(subCommand).PadRight(firstColumnLength);
            _codeBuilder.AppendLine(
                $"Console.Write(\"    {firstColumn}\");"
            );

            if (subCommand.Description.IsSome(out var description))
            {
                _codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
                _codeBuilder.AppendLine($"Console.WriteLine(\"  {description}\");");
                _codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
            }

            _codeBuilder.AppendLine("Console.WriteLine();");
        }
        
        string GetFirstColumn(CommandTree c) 
        {
            return c switch
            {
                SubCommand s => s.CommandName,
                _ => ""
            } + " " + c.Parameters.Select(GetHelpTextInPlace).StringJoin(" ");
        }
    }
}
