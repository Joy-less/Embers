using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Embers.IronRubyBenchmark
{
    [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest), RankColumn]
    public class RubyBenchmark
    {
        public ScriptEngine IronRubyEngine;
        public ScriptScope  IronRubyScope;
        [GlobalSetup]
        public void Setup()
        {
            IronRubyEngine = Ruby.CreateEngine();
            IronRubyScope = IronRubyEngine.CreateScope();
        }
        [BenchmarkCategory("IronRuby"), Benchmark(Description = "100_000 Iterations")]
        public void IronRuby_100_000_Iterations()
        {
            for (int i = 0; i < 100_000; i++)
            {
                IronRubyEngine.Execute("a = 0; a += 1", IronRubyScope);
            }
        }
        // [BenchmarkCategory("IronRuby"), Benchmark(Description = "1_000_000 Iterations")]
        // public void IronRuby_1_000_000_Iterations()
        // {
        //     Expression[] Expressions = EmbersScope.Parse("a = 0; a += 1");
        //     for (int i = 0; i < 1_000_000; i++)
        //     {
        //         EmbersScope.Interpret(Expressions);
        //       
        //     }
        // }
        [BenchmarkCategory("IronRuby"), Benchmark(Description = "1_000_000 times blocks")]
        public void IronRuby_1_000_000_times_blocks()
        {
            IronRubyEngine.Execute("1_000_000.times do; a = 0; a += 1; end", IronRubyScope);
        }
        [BenchmarkCategory("Embers"), Benchmark(Description = "empty 10_000_0000 times blocks")]
        public void IronRuby_empty_10_000_0000_times_blocks()
        {
            IronRubyEngine.Execute(@"10_000_000.times do; end", IronRubyScope);
        }
        [BenchmarkCategory("IronRuby"), Benchmark(Description = "fibonacci to 25")]
        public void IronRuby_fibonacci_to_25()
        {
            IronRubyEngine.Execute(@"
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
                        ", IronRubyScope);
        }
    }
}