using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

#nullable disable

namespace Embers.Benchmark {
    [MemoryDiagnoser]
    public class RubyBenchmark {
        public Scope EmbersScope;

        [GlobalSetup]
        public void Setup() {
            EmbersScope = new Scope();
        }

        [BenchmarkCategory("Embers"), Benchmark(Description = "100,000 iterations")]
        public void Embers_100_000_iterations() {
            for (int i = 0; i < 100_000; i++) {
                EmbersScope.Evaluate("a = 0; a += 1");
            }
        }

        [BenchmarkCategory("Embers"), Benchmark(Description = "1,000,000 iterations pre-parsed")]
        public void Embers_1_000_000_iterations() {
            Expression[] Expressions = EmbersScope.Parse("a = 0; a += 1");
            for (int i = 0; i < 1_000_000; i++) {
                EmbersScope.Interpret(Expressions);
            }
        }

        [BenchmarkCategory("Embers"), Benchmark(Description = "1,000,000 times block")]
        public void Embers_1_000_000_times_block() {
            EmbersScope.Evaluate("1_000_000.times do; a = 0; a += 1; end");
        }

        [BenchmarkCategory("Embers"), Benchmark(Description = "empty 10,000,000 times block")]
        public void Embers_empty_10_000_0000_times_block() {
            EmbersScope.Evaluate(@"10_000_000.times do; end");
        }

        [BenchmarkCategory("Embers"), Benchmark(Description = "fibonacci to 25")]
        public void Embers_fibonacci_to_25() {
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

        [BenchmarkCategory("Embers"), Benchmark(Description = "large prime")]
        public void Embers_large_prime() {
            EmbersScope.Evaluate(@"
                def is_prime?(num)
                  if num <= 1
                    return false
                  else
                    2.upto(Math.sqrt(num)) do |i|
                        return false if num % i == 0
                    end
                    return true
                  end
                end
                def get_large_prime
                  num = 10**10 + rand(10**9)
                  num += 1 until is_prime?(num)
                  num
                end
                get_large_prime
            ");
        }
    }
}