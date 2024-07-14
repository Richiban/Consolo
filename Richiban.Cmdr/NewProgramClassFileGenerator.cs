using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Richiban.Cmdr;

internal class NewProgramClassFileGenerator(IReadOnlyCollection<MethodModel> results) : CodeFileGenerator
{
    public override string FileName => "Program.g.cs";

    public override string GetCode()
    {
        var codeBuilder = new CodeBuilder();

        codeBuilder.AppendLines(
            "using System;",
            "using System.CommandLine;",
            "using System.CommandLine.Invocation;",
            "using System.Linq;");

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

                commands.Add(new Com(path, method, result.Parameters));
            }

            foreach (var com in commands.SortOrder(Com.CompareTo))
            {
                var pathStrings = com.Path.Select(x => $"\"{x}\"");

                var parameterStrings = com.Parameters
                    .Where(x => x.IsRequired)
                    .Select(x => $"var {x.Name}");

                var allStrings = pathStrings.Concat(parameterStrings);
                
                var optionalParameters = com.Parameters
                    .Where(x => !x.IsRequired)
                    .Select(x => $"var {x.Name}");

                if (optionalParameters.Any())
                {
                    allStrings = allStrings.Append("..");
                }

                var ps = com.Parameters
                    .Where(x => x.IsRequired)
                    .Select(x => x.Name);

                codeBuilder.AppendLine(
                    $"case [{String.Join(", ", allStrings)}]:"
                );

                using (codeBuilder.Indent())
                {

                    codeBuilder.AppendLine(
                        $"{com.Method}({String.Join(", ", ps)});"
                    );

                    codeBuilder.AppendLine("break;");
                }
            }

            codeBuilder.AppendLine("default: ");
            using (codeBuilder.Indent())
            {
                codeBuilder.AppendLine("Console.WriteLine(\"Command not found\");");
                codeBuilder.AppendLine("break;");
            }
        }


        codeBuilder.AppendLine("}");

        codeBuilder.AppendLine();

        codeBuilder.AppendLine(
            """
            static string[] NormaliseArgs(string[] args)
            {
                var copy = args
                    .SelectMany(s => s switch {
                        ['-', not '-', ..] => s.Skip(1).Select(c => $"-{c}"),
                        _ => [s],
                    })
                    .Where(s => !String.IsNullOrEmpty(s))
                    .ToArray();

                Array.Sort(copy, (x, y) =>
                    (x, y) switch
                    {
                        (['-', ..], [not '-', ..]) => 1,
                        ([not '-', ..], ['-', ..]) => -1,
                        _ => 0,
                    }
                );

                return copy;
            }
            """);

        return codeBuilder.ToString();
    }
}

class Com(
    IReadOnlyCollection<string> path,
    string method,
    IReadOnlyCollection<ParameterModel> parameters)
{
    public IReadOnlyCollection<string> Path { get; } = path;
    public string Method { get; } = method;
    public IReadOnlyCollection<ParameterModel> Parameters { get; } = parameters;

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

static class EnumerableExtensions1
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
}