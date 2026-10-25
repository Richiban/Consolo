using BenchmarkDotNet.Attributes;
using Consolo.TestBench;

namespace Consolo.TestBench.Runner;

[MemoryDiagnoser]
public class CommandBenchmarks
{
    private static readonly string[] Arguments = ["echo", "benchmark"];

    [Benchmark(Baseline = true)]
    public void HandWrittenParser()
    {
        if (Arguments.Length >= 1 && Arguments[0] == "echo" &&
            Arguments.Length == 2)
        {
            Commands.Echo(Arguments[1]);
        }
    }

    [Benchmark]
    public void GeneratedCommand() => Consolo.TestBench.g.Program.Main(Arguments);
}
