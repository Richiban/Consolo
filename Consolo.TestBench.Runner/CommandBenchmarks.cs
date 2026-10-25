using BenchmarkDotNet.Attributes;
using Consolo.TestBench;

namespace Consolo.TestBench.Runner;

[MemoryDiagnoser]
public class CommandBenchmarks
{
    private static readonly string[] Arguments = ["echo", "benchmark"];

    [Benchmark(Baseline = true)]
    public void DirectCall() => Commands.Echo("benchmark");

    [Benchmark]
    public void GeneratedCommand() => Consolo.TestBench.g.Program.Main(Arguments);
}
