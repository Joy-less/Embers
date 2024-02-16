using BenchmarkDotNet.Attributes;
using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Embers.IronRubyBenchmark {
    [MemoryDiagnoser]
    public class RubyBenchmark {
        public ScriptEngine IronRubyEngine;
        public ScriptScope IronRubyScope;

        [GlobalSetup]
        public void Setup() {
            IronRubyEngine = Ruby.CreateEngine();
            IronRubyScope = IronRubyEngine.CreateScope();
        }

        [BenchmarkCategory("IronRuby"), Benchmark(Description = "100,000 iterations")]
        public void IronRuby_100_000_iterations() {
            for (int i = 0; i < 100_000; i++) {
                IronRubyEngine.Execute("a = 0; a += 1", IronRubyScope);
            }
        }

        [BenchmarkCategory("IronRuby"), Benchmark(Description = "1,000,000 times block")]
        public void IronRuby_1_000_000_times_block() {
            IronRubyEngine.Execute("1_000_000.times do; a = 0; a += 1; end", IronRubyScope);
        }

        [BenchmarkCategory("IronRuby"), Benchmark(Description = "empty 10,000,000 times block")]
        public void IronRuby_empty_10_000_0000_times_block() {
            IronRubyEngine.Execute(@"10_000_000.times do; end", IronRubyScope);
        }

        [BenchmarkCategory("IronRuby"), Benchmark(Description = "fibonacci to 25")]
        public void IronRuby_fibonacci_to_25() {
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

        [BenchmarkCategory("IronRuby"), Benchmark(Description = "large prime")]
        public void IronRuby_large_prime() {
            IronRubyEngine.Execute(@"
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
            ", IronRubyScope);
        }
    }
}