using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Consolo.Tests.Generator;

internal class CollectionParameterTests : GeneratorTests
{
    private const string Source =
        """
        using System;
        using System.Collections.Generic;
        using Consolo;

        namespace CollectionTests;

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
            public static void Tagged(string name, string[] tag = null, IReadOnlyList<int> level = null)
            {
                Result = name + "|" + string.Join(",", tag ?? new string[0]) + "|" + string.Join(",", level ?? new int[0]);
            }

            [Consolo]
            public static void AtLeastOne(IEnumerable<string> items)
            {
                Result = string.Join(",", items);
            }
        }
        """;

    private static (int ExitCode, string Result, string Error, string Output) Run(params string[] args)
    {
        var (compilation, diagnostics) = RunGenerator(Source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);

        emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        var assembly = Assembly.Load(stream.ToArray());
        var main = assembly.GetType("Consolo.TestSource.g.Program")!.GetMethod("Main")!;
        var commands = assembly.GetType("CollectionTests.Commands")!;

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
    public void RepeatedOptionAccumulatesValues()
    {
        var (exitCode, result, _, _) =
            Run("tagged", "n", "--tag", "one", "--tag", "two", "--level", "3", "--level=4");

        exitCode.ShouldBe(0);
        result.ShouldBe("n|one,two|3,4");
    }

    [Test]
    public void CollectionOptionMayBeOmitted()
    {
        var (exitCode, result, _, _) = Run("tagged", "n");

        exitCode.ShouldBe(0);
        result.ShouldBe("n||");
    }

    [Test]
    public void OptionsAndPositionalsMayBeMixed()
    {
        var (exitCode, result, _, _) = Run("tagged", "--tag", "one", "n", "--tag", "two");

        exitCode.ShouldBe(0);
        result.ShouldBe("n|one,two|");
    }

    [Test]
    public void CollectionOptionWithMissingValueIsAnError()
    {
        var (exitCode, _, error, _) = Run("tagged", "n", "--tag");

        exitCode.ShouldBe(1);
        error.ShouldContain("Missing value for option '--tag'");
    }

    [Test]
    public void RequiredCollectionNeedsAtLeastOneValue()
    {
        var (exitCode, _, error, _) = Run("at-least-one");

        exitCode.ShouldBe(1);
        error.ShouldContain("Missing value for argument 'items' (expected one or more values)");
    }

    [Test]
    public void HelpDescribesCollectionCardinality()
    {
        var (_, _, _, output) = Run("tagged", "--help");

        output.ShouldContain("(can be repeated)");

        var (_, _, _, echoOutput) = Run("echo", "--help");

        echoOutput.ShouldContain("[<words>...]");
        echoOutput.ShouldContain("(zero or more values)");
    }
}
