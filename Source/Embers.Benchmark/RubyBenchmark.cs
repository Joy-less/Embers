using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Embers.Benchmark
{
    [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest), RankColumn]
    public class RubyBenchmark
    {
        public Scope EmbersScope;

        [GlobalSetup]
        public void Setup()
        {
            EmbersScope = new();
        }
        [BenchmarkCategory("Embers"), Benchmark(Description = "100_000 Iterations")]
        public void Embers_100_000_Iterations()
        {
            for (int i = 0; i < 100_000; i++)
            {
                EmbersScope.Evaluate("a = 0; a += 1");
            }
        }
        [BenchmarkCategory("Embers"), Benchmark(Description = "1_000_000 Iterations")]
        public void Embers_1_000_000_Iterations()
        {
            Expression[] Expressions = EmbersScope.Parse("a = 0; a += 1");
            for (int i = 0; i < 1_000_000; i++)
            {
                EmbersScope.Interpret(Expressions);
            }
        }
        [BenchmarkCategory("Embers"), Benchmark(Description = "1_000_000 times blocks")]
        public void Embers_1_000_000_times_blocks()
        {
            EmbersScope.Evaluate("1_000_000.times do; a = 0; a += 1; end");
        }
        [BenchmarkCategory("Embers"), Benchmark(Description = "empty 10_000_0000 times blocks")]
        public void Embers_empty_10_000_0000_times_blocks()
        {
            EmbersScope.Evaluate(@"10_000_000.times do; end");
        }
        [BenchmarkCategory("Embers"), Benchmark(Description = "fibonacci to 25")]
        public void Embers_fibonacci_to_25()
        {
            EmbersScope.Evaluate(@"
        def fibonacci(n)
          if n <= 1
            return n
          else
            return fibonacci(n - 1) + fibonacci(n - 2)
          end
        end
        
        25.times do |n|
            fibonacci(n)
        end
                        ");
        }

    }
}