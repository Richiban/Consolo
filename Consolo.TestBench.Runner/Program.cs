using BenchmarkDotNet.Running;

namespace Consolo.TestBench.Runner;

public static class Program
{
    public static void Main(string[] args) =>
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
