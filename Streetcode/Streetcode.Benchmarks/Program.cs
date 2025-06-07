using BenchmarkDotNet.Running;

namespace Streetcode.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<FactBenchmarks>();
    }
}