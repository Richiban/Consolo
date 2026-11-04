using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Consolo.Tests.Generator;

internal class ParamsParameterTests : GeneratorTests
{
    private const string Source =
        """
        using System;
        using System.Collections.Generic;
        using Consolo;

        namespace ParamsTests;

        public enum Colour { Red, Green, Blue }

        public static class Commands
        {
            public static string Result = "";

            [Consolo]
            public static void Echo(string prefix, params string[] words)
            {
                Result = prefix + ":" + string.Join(",", words);
            }

            [Consolo]
            public static void Sum(params int[] numbers)
            {
                Result = "sum=" + System.Linq.Enumerable.Sum(numbers);
            }

            [Consolo]
            public static void Repeat(int times = 1, params string[] words)
            {
                Result = times + ":" + string.Join(",", words);
            }

            [Consolo]
            public static void Paint(params Colour[] colours)
            {
                Result = string.Join(",", colours);
            }

            [Consolo]
            public static void Enumerable(params IEnumerable<string> items)
            {
                Result = items.GetType().Name + ":" + string.Join(",", items);
            }

            [Consolo]
            public static void List(params List<int> values)
            {
                Result = values.GetType().Name + ":" + string.Join(",", values);
            }

            [Consolo]
            public static void Span(params ReadOnlySpan<string> parts)
            {
                Result = string.Join("+", parts.ToArray());
            }
        }
        """;

    private static (int ExitCode, string Result, string Error, string Output) Run(
        string source,
        LanguageVersion languageVersion,
        params string[] args)
    {
        var (compilation, diagnostics) = RunGenerator(source, languageVersion: languageVersion);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);

        emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var assembly = Assembly.Load(stream.ToArray());
        var main = assembly.GetType("Consolo.TestSource.g.Program")!.GetMethod("Main")!;
        var commands = assembly.GetType("ParamsTests.Commands")!;

        var originalOut = Console.Out;
        var originalError = Console.Error;
        var output = new StringWriter();
        var error = new StringWriter();

        try
        {
            Console.SetOut(output);
            Console.SetError(error);

            var exitCode = (int)main.Invoke(null, [args])!;
            var result = (string)commands.GetField("Result")!.GetValue(null)!;

            return (exitCode, result, error.ToString(), output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private static (int ExitCode, string Result, string Error, string Output) Run(params string[] args) =>
        Run(Source, LanguageVersion.CSharp13, args);

    [Test]
    public void ParamsStringArrayConsumesRemainingPositionals()
    {
        var (exitCode, result, _, _) = Run("echo", "p", "a", "b", "c");

        exitCode.ShouldBe(0);
        result.ShouldBe("p:a,b,c");
    }

    [Test]
    public void ParamsArrayMayBeEmpty()
    {
        var (exitCode, result, _, _) = Run("echo", "p");

        exitCode.ShouldBe(0);
        result.ShouldBe("p:");
    }

    [Test]
    public void ParamsArrayElementsAreConverted()
    {
        var (exitCode, result, _, _) = Run("sum", "1", "2", "3");

        exitCode.ShouldBe(0);
        result.ShouldBe("sum=6");
    }

    [Test]
    public void ParamsArrayWithNoValuesIsEmpty()
    {
        var (exitCode, result, _, _) = Run("sum");

        exitCode.ShouldBe(0);
        result.ShouldBe("sum=0");
    }

    [Test]
    public void ParamsEnumElementsAreConverted()
    {
        var (exitCode, result, _, _) = Run("paint", "red", "blue");

        exitCode.ShouldBe(0);
        result.ShouldBe("Red,Blue");
    }

    [Test]
    public void OptionsAndParamsValuesMayBeMixed()
    {
        var (exitCode, result, _, _) = Run("repeat", "a", "--times", "2", "b");

        exitCode.ShouldBe(0);
        result.ShouldBe("2:a,b");
    }

    [Test]
    public void ParamsEnumerableIsSupported()
    {
        var (exitCode, result, _, _) = Run("enumerable", "a", "b");

        exitCode.ShouldBe(0);
        result.ShouldEndWith(":a,b");
    }

    [Test]
    public void ParamsListIsSupported()
    {
        var (exitCode, result, _, _) = Run("list", "1", "2");

        exitCode.ShouldBe(0);
        result.ShouldBe("List`1:1,2");
    }

    [Test]
    public void ParamsReadOnlySpanIsSupported()
    {
        var (exitCode, result, _, _) = Run("span", "a", "b");

        exitCode.ShouldBe(0);
        result.ShouldBe("a+b");
    }

    [Test]
    public void ParamsArrayCompilesWithoutCollectionExpressions()
    {
        const string source =
            """
            using Consolo;

            namespace ParamsTests
            {
                public static class Commands
                {
                    public static string Result = "";

                    [Consolo]
                    public static void Echo(params string[] words)
                    {
                        Result = string.Join(",", words);
                    }
                }
            }
            """;

        var (exitCode, result, _, _) = Run(source, LanguageVersion.CSharp7_3, "echo", "a", "b");

        exitCode.ShouldBe(0);
        result.ShouldBe("a,b");
    }

    [Test]
    public void HelpDescribesParamsCardinality()
    {
        var (_, _, _, output) = Run("echo", "--help");

        output.ShouldContain("[<words>...]");
        output.ShouldContain("(zero or more values)");

        var (_, _, _, paintOutput) = Run("paint", "--help");

        paintOutput.ShouldContain("<red|green|blue>...");
    }

    [Test]
    public void NonParamsArrayIsUnsupported()
    {
        const string source =
            """
            using Consolo;

            public static class Commands
            {
                [Consolo]
                public static void Echo(string[] words) { }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);

        diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldHaveSingleItem()
            .Id.ShouldBe("Consolo0008");
    }
}
