using BenchmarkDotNet.Attributes;
using Consolo.TestBench;

namespace Consolo.TestBench.Runner;

[MemoryDiagnoser]
public class CommandBenchmarks
{
    private static readonly string[] PositionalArguments = ["echo", "benchmark"];
    private static readonly string[] OptionsAbsentArguments = ["options", "benchmark"];
    private static readonly string[] OptionsPresentArguments = ["options", "benchmark", "--number", "42"];
    private static readonly string[] OptionAliasArguments = ["options", "benchmark", "-n", "42"];
    private static readonly string[] OptionColonArguments = ["options", "benchmark", "--number:42"];
    private static readonly string[] OptionEqualsArguments = ["options", "benchmark", "--number=42"];
    private static readonly string[] FlagsArguments = ["flags", "benchmark", "-f", "--dry-run"];
    private static readonly string[] MultipleParametersArguments = ["multiple", "first", "second", "3"];
    private static readonly string[] InvalidArguments = ["echo", "benchmark", "--unknown"];
    private static readonly string[] ArgumentMaskBoundaryArguments = BuildManyArguments(optionCount: 63);
    private static readonly string[] ArgumentMaskFallbackArguments = BuildManyArguments(optionCount: 64);
    private TextWriter _originalOut = TextWriter.Null;
    private TextWriter _originalError = TextWriter.Null;

    [GlobalSetup]
    public void SuppressCommandOutput()
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
    }

    [GlobalCleanup]
    public void RestoreCommandOutput()
    {
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);
    }

    [Benchmark]
    public void GeneratedSimplePositional() =>
        Consolo.TestBench.g.Program.Main(PositionalArguments);

    [Benchmark]
    public void GeneratedOptionsAbsent() =>
        Consolo.TestBench.g.Program.Main(OptionsAbsentArguments);

    [Benchmark]
    public void GeneratedOptionsPresent() =>
        Consolo.TestBench.g.Program.Main(OptionsPresentArguments);

    [Benchmark]
    public void GeneratedOptionAlias() =>
        Consolo.TestBench.g.Program.Main(OptionAliasArguments);

    [Benchmark]
    public void GeneratedOptionColonValue() =>
        Consolo.TestBench.g.Program.Main(OptionColonArguments);

    [Benchmark]
    public void GeneratedOptionEqualsValue() =>
        Consolo.TestBench.g.Program.Main(OptionEqualsArguments);

    [Benchmark]
    public void GeneratedFlags() =>
        Consolo.TestBench.g.Program.Main(FlagsArguments);

    [Benchmark]
    public void GeneratedMultipleParameters() =>
        Consolo.TestBench.g.Program.Main(MultipleParametersArguments);

    [Benchmark]
    public void GeneratedInvalidInput() =>
        Consolo.TestBench.g.Program.Main(InvalidArguments);

    [Benchmark]
    public void Generated64ArgumentMaskBoundary() =>
        Consolo.TestBench.g.Program.Main(ArgumentMaskBoundaryArguments);

    [Benchmark]
    public void Generated65ArgumentMaskFallback() =>
        Consolo.TestBench.g.Program.Main(ArgumentMaskFallbackArguments);

    private static string[] BuildManyArguments(int optionCount)
    {
        var arguments = new string[optionCount + 1];
        arguments[0] = "many";

        for (var i = 0; i < optionCount; i++)
        {
            arguments[i + 1] = $"--option{i:D2}";
        }

        return arguments;
    }
}
