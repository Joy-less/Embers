using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Embers.Benchmark
{

    public class Program
    {
        public static void Main()
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<RubyBenchmark>();
        }
    }
}