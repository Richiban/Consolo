using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Richiban.Cmdr;

internal class NewProgramClassFileGenerator(
    string assemblyName,
    IReadOnlyCollection<MethodModel> results) : CodeFileGenerator
{
    public override string FileName => "Program.g.cs";

    public override string GetCode()
    {
        var codeBuilder = new CodeBuilder();

        codeBuilder.AppendLines(
            "using System;",
            "using System.Linq;",
            "using System.Collections.Generic;");

        codeBuilder.AppendLine();

        codeBuilder.AppendLine("var consoleColor = Console.ForegroundColor;");
        codeBuilder.AppendLine("var helpTextColor = ConsoleColor.Green;");

        codeBuilder.AppendLine();

        codeBuilder.AppendLine("switch (NormaliseArgs(args))");
        codeBuilder.AppendLine("{");

        using (codeBuilder.Indent())
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

            foreach (var com in commands.SortOrder(Com.CompareTo))
            {
                var pathStrings = com.Path.Select(x => $"\"{x}\"");

                var parameterStrings = com.Parameters
                    .Where(x => x.IsRequired)
                    .Select(x => $"var {x.Name}");

                var allStrings = pathStrings.Concat(parameterStrings);

                var allHelpText = com.Path.Concat(
                    com.Parameters.Select(x => x.GetHelpTextInPlace())
                );
                
                var optionalParameters = com.Parameters
                    .Where(x => !x.IsRequired);
                
                var optionalParameterDeclarations = optionalParameters
                    .Select(x => $"var {x.Name}");

                var ps = com.Parameters
                    .Where(x => x.IsRequired)
                    .Select(x => x.Name);

                codeBuilder.AppendLines(
                    $"case ([{String.Join(", ", allStrings)}, ..], [.., \"--help\" or \"-h\"]):",
                    "{"
                );

                using (codeBuilder.Indent())
                {
                    codeBuilder.AppendLine(
                        $"Console.WriteLine("
                    );

                    using (codeBuilder.Indent())
                    {
                        codeBuilder.AppendLines(
                            "\"\"\"",
                            $"{assemblyName}",
                            "",
                            $"Command:",
                            $"    {String.Join(" ", allHelpText)}",
                            "\"\"\""
                        );
                    }

                    codeBuilder.AppendLines(
                        $");"
                    );

                    if (!String.IsNullOrEmpty(com.Description))
                    {
                        codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
                        
                        codeBuilder.AppendLines(
                            $"Console.WriteLine(",
                            $"    \"\"\"",
                            "",
                            $"        {com.Description}",
                            $"    \"\"\"",
                            ");"
                        );

                        codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                    }

                    if (com.Parameters.Any())
                    {
                        codeBuilder.AppendLines(
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
                            codeBuilder.AppendLines(
                                $"Console.Write(\"    {helpName.PadRight(longestParameter)}  \");",
                                "Console.ForegroundColor = helpTextColor;",
                                $"Console.WriteLine(\"{description}\");"
                            );

                            codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                        }
                    }

                    codeBuilder.AppendLines("break;");
                }

                codeBuilder.AppendLines("}");

                codeBuilder.AppendLines(
                    $"case ([{String.Join(", ", allStrings)}], var options):",
                    "{"
                );

                using (codeBuilder.Indent())
                {
                    foreach (var flag in optionalParameters)
                    {
                        if (flag.ShortForm.HasValue)
                        {
                            codeBuilder.AppendLine(
                                $"var {flag.Name} = options.Contains(\"-{flag.ShortForm}\") || options.Contains(\"--{flag.Name}\");"
                            );
                        }
                        else 
                        {
                            codeBuilder.AppendLine(
                                $"var {flag.Name} = options.Contains(\"--{flag.Name}\");"
                            );
                        }
                    }

                    var arguments = ps.Concat(optionalParameters.Select(x => $"{x.Name}: {x.Name}"));

                    codeBuilder.AppendLine(
                        $"{com.Method}({String.Join(", ", arguments)});"
                    );

                    codeBuilder.AppendLine("break;");
                }
                codeBuilder.AppendLine("}");
            }

            codeBuilder.AppendLine("default: ");

            using (codeBuilder.Indent())
            {
                codeBuilder.AppendLines($"Console.WriteLine(\"{assemblyName}\");", "Console.WriteLine();");
                codeBuilder.AppendLine("Console.WriteLine(\"Commands:\");");

                var availableCommands = commands
                    .Select(c => c.GetHelpText())
                    .ToImmutableArray();

                var longestCommand = availableCommands.Max(x => x.Item1.Length);
                
                foreach (var (command, description) in availableCommands)
                {
                    var cmd = command.PadRight(longestCommand);
                    codeBuilder.AppendLine($"Console.Write(\"    {cmd}\");");
                    codeBuilder.AppendLine("Console.ForegroundColor = helpTextColor;");
                    codeBuilder.AppendLine($"Console.WriteLine(\"  {description}\");");
                    codeBuilder.AppendLine("Console.ForegroundColor = consoleColor;");
                }

                codeBuilder.AppendLine("break;");
            }
        }


        codeBuilder.AppendLine("}");

        codeBuilder.AppendLine();

        codeBuilder.AppendLine(
            """
            static (IReadOnlyList<string> positionalArgs, IReadOnlyList<string> options) NormaliseArgs(string[] args)
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

                return (positionalArgs, options);
            }
            """);

        return codeBuilder.ToString();
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